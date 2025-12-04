using UnityEngine;
using UnityEngine.InputSystem;

public class Accelerating : MonoBehaviour {
  public float acceleration_multiplier = 5f;
  public float max_speed = 10f;
  public float rotation_speed = 5f;
  public float movement_threshold = 0.2f;

  public float min_latitude = 0f;
  public float max_latitude = 0f;
  public float min_longitude = 0f;
  public float max_longitude = 0f;
  
  public float magnetometer_offset = 0f;
  
  public float world_north_angle = 0f;

  // Sensores
  private Accelerometer accelerometer;
  private UnityEngine.InputSystem.Gyroscope gyroscope;
  private MagneticFieldSensor magnetometer;
  private GravitySensor gravity_sensor;

  // Estado de sensores
  private bool is_magnetometer_working = false;
  private bool is_gps_working = false;
  private bool is_gyroscope_working = false;
  private bool is_accelerometer_working = false;

  // Control de movimiento
  private Vector3 current_velocity = Vector3.zero;
  private Quaternion target_rotation;
  private bool is_in_range = true;
  
  // Referencias de norte
  private Vector3 north_reference = Vector3.forward;
  private float last_sensor_check_time = 0f;
  private float sensor_check_interval = 1f;
  
  // Control de inicialización
  private bool sensors_initialized = false;
  private float initialization_time = 0f;
  private float initialization_delay = 2f;
  
  // Threshold para detectar valores en cero
  private const float ZERO_THRESHOLD = 0.001f;

  void Start() {
    Debug.Log("=== Inicializando Sensores ===");
    
    target_rotation = transform.rotation;
    north_reference = Get_World_North();
    initialization_time = Time.time;
    
    Initialize_All_Sensors();
  }

  void Initialize_All_Sensors() {
    // Habilitar Acelerómetro
    accelerometer = Accelerometer.current;
    if (accelerometer != null) {
      InputSystem.EnableDevice(accelerometer);
      Debug.Log("Acelerómetro habilitado");
    } else {
      Debug.LogWarning("Acelerómetro no disponible");
    }

    var rotationSensor = AttitudeSensor.current;
    if (rotationSensor != null) {
      InputSystem.EnableDevice(rotationSensor);
      Debug.Log("Sensor combinado (Attitude) habilitado");
    }

    // Habilitar Giroscopio
    gyroscope = UnityEngine.InputSystem.Gyroscope.current;
    if (gyroscope != null) {
      InputSystem.EnableDevice(gyroscope);
      Debug.Log("Giroscopio habilitado");
    } else {
      Debug.LogWarning("Giroscopio no disponible");
    }

    // Habilitar Magnetómetro
    magnetometer = MagneticFieldSensor.current;
    if (magnetometer != null)
    {
      InputSystem.EnableDevice(magnetometer);
      Debug.Log("Magnetómetro habilitado");
    } else {
      Debug.LogWarning("Magnetómetro no disponible");
    }

    // Habilitar Sensor de Gravedad
    gravity_sensor = GravitySensor.current;
    if (gravity_sensor != null)
    {
      InputSystem.EnableDevice(gravity_sensor);
      Debug.Log("Sensor de gravedad habilitado");
    }

    // Iniciar GPS
    if (!Input.location.isEnabledByUser)
    {
      Debug.LogWarning("GPS no habilitado por el usuario");
    } else {
      Input.location.Start(1f, 0.1f);
      Debug.Log("GPS iniciado");
    }
  }

  void Update()
  {
    if (!sensors_initialized)
    {
      if (Time.time - initialization_time >= initialization_delay)
      {
        sensors_initialized = true;
        Check_Sensor_Functionality();
        Debug.Log("Sensores estabilizados y verificados");
      }
      return;
    }
    
    if (Time.time - last_sensor_check_time > sensor_check_interval) {
      Check_Sensor_Functionality();
      last_sensor_check_time = Time.time;
    }

    // Verificar rango GPS
    Check_GPS_Range();

    if (!is_in_range) {
      // Detener movimiento fuera de rango
      current_velocity = Vector3.zero;
      return;
    }

    // Orientar hacia el norte
    Orient_To_North();

    // Mover con aceleración
    Move_With_Acceleration();
  }

  void Check_Sensor_Functionality()
  {
    // Prioridad 1 a magnetómetro
    if (magnetometer != null && magnetometer.enabled) {
      Vector3 mag_field = magnetometer.magneticField.ReadValue();
      is_magnetometer_working = magnetometer != null &&
                              (Mathf.Abs(mag_field.x) > 0.00001f ||
                              Mathf.Abs(mag_field.y) > 0.00001f ||
                              Mathf.Abs(mag_field.z) > 0.00001f);
      
      if (is_magnetometer_working) {
        Debug.Log($"Magnetómetro funcional: {mag_field}");
      } else {
        Debug.Log($"Magnetómetro detectado pero en cero: {mag_field}");
      }
    } else {
      is_magnetometer_working = false;
    }

    // Prioridad 1 a GPS
    if (Input.location.status == LocationServiceStatus.Running) {
      LocationInfo loc = Input.location.lastData;
      is_gps_working = (Mathf.Abs(loc.latitude) > ZERO_THRESHOLD || 
                        Mathf.Abs(loc.longitude) > ZERO_THRESHOLD);
      
      if (is_gps_working) {
        Debug.Log($"GPS funcional: Lat {loc.latitude}, Lon {loc.longitude}");
      } else {
        Debug.Log($"GPS activo pero en cero: Lat {loc.latitude}, Lon {loc.longitude}");
      }
    } else {
      is_gps_working = false;
      if (Input.location.status == LocationServiceStatus.Failed) {
        Debug.LogWarning("GPS falló al iniciar");
      }
    }

    // Prioridad 3 a giroscopio
    if (gyroscope != null && gyroscope.enabled) {
      is_gyroscope_working = true;
      Vector3 ang_vel = gyroscope.angularVelocity.ReadValue();
      Debug.Log($"Giroscopio disponible: {ang_vel}");
    } else {
      is_gyroscope_working = false;
    }

    // Prioridad 4 a acelerómetro
    if (accelerometer != null && accelerometer.enabled) {
      Vector3 accel = accelerometer.acceleration.ReadValue();
      is_accelerometer_working = !Is_Vector_Zero(accel);
      
      if (is_accelerometer_working) {
        Debug.Log($"Acelerómetro funcional: {accel}");
      } else {
        Debug.Log($"Acelerómetro en cero: {accel}");
      }
    } else {
      is_accelerometer_working = false;
    }

    // Log del sensor activo
    string active_sensor = Get_Active_Sensor_Name();
    Debug.Log($">>> SENSOR ACTIVO: {active_sensor}");
  }

  bool Is_Vector_Zero(Vector3 vec) {
    return Mathf.Abs(vec.x) < ZERO_THRESHOLD && 
           Mathf.Abs(vec.y) < ZERO_THRESHOLD && 
           Mathf.Abs(vec.z) < ZERO_THRESHOLD;
  }

  void Check_GPS_Range() {
    // Si todos los valores están en 0, no hay restricción
    if (min_latitude == 0f && max_latitude == 0f && 
        min_longitude == 0f && max_longitude == 0f) {
      is_in_range = true;
      return;
    }

    // Verificar si GPS está disponible
    if (Input.location.status == LocationServiceStatus.Running) {
      LocationInfo loc = Input.location.lastData;
      is_in_range = (loc.latitude >= min_latitude && loc.latitude <= max_latitude &&
                     loc.longitude >= min_longitude && loc.longitude <= max_longitude);
      
      if (!is_in_range) {
        Debug.LogWarning($"Fuera de rango. Lat: {loc.latitude}, Lon: {loc.longitude}");
      }
    }
    else {
      // Si no hay GPS, no restringir
      is_in_range = true;
    }
  }

  void Orient_To_North() {
    Vector3 north_direction = Get_North_Direction();

    if (north_direction != Vector3.zero) {
      // Calcular rotación objetivo hacia el norte
      target_rotation = Quaternion.LookRotation(north_direction);

      // Interpolar suavemente con Slerp
      transform.rotation = Quaternion.Slerp(
        transform.rotation,
        target_rotation,
        rotation_speed * Time.deltaTime
      );
    }
  }

  Vector3 Get_North_Direction() {
    // Prioridad 1 a magnetómetro
    if (is_magnetometer_working && magnetometer != null) {
      Vector3 mag_field = magnetometer.magneticField.ReadValue();
      Vector3 north = new Vector3(-mag_field.x, 0, -mag_field.y);
      
      if (north.magnitude > ZERO_THRESHOLD) {
        north_reference = north.normalized;
        
        if (Mathf.Abs(magnetometer_offset) > 0.1f) {
          Quaternion offset_rotation = Quaternion.Euler(0, magnetometer_offset, 0);
          north_reference = offset_rotation * north_reference;
        }
        
        return north_reference;
      }
    }

    // Prioridad 2 a GPS
    if (is_gps_working && Input.location.status == LocationServiceStatus.Running) {
      north_reference = Get_World_North();
      return north_reference;
    }

    // Prioridad 3 a giroscopio
    if (is_gyroscope_working && gyroscope != null) {
      return north_reference;
    }

    // Prioridad 4 a acelerómetro
    if (is_accelerometer_working && accelerometer != null) {
      Vector3 accel = accelerometer.acceleration.ReadValue();
      Vector3 tilt_direction = new Vector3(-accel.x, 0, -accel.z);
      
      if (tilt_direction.magnitude > 0.3f) {
        north_reference = tilt_direction.normalized;
        return north_reference;
      }
    }

    // Configuración default del editor
    return Get_World_North();
  }
  
  Vector3 Get_World_North() {
    float angle_rad = world_north_angle * Mathf.Deg2Rad;
    Vector3 north = new Vector3(Mathf.Sin(angle_rad), 0, Mathf.Cos(angle_rad));
    return north.normalized;
  }

  void Move_With_Acceleration() {
    if (accelerometer == null || !accelerometer.enabled) {
      return;
    }

    Vector3 accel = accelerometer.acceleration.ReadValue();
    float forward_accel = -accel.z;

    if (Mathf.Abs(forward_accel) > movement_threshold) {
      float acceleration = forward_accel * acceleration_multiplier;
      current_velocity += transform.forward * acceleration * Time.deltaTime;

      current_velocity = Vector3.ClampMagnitude(current_velocity, max_speed);
    } else {
      current_velocity = Vector3.Lerp(current_velocity, Vector3.zero, 5f * Time.deltaTime);
    }

    transform.position += current_velocity * Time.deltaTime;
  }

  string Get_Active_Sensor_Name() {
    if (is_magnetometer_working) return "Magnetómetro (Prioridad 1)";
    if (is_gps_working) return "GPS (Prioridad 2)";
    if (is_gyroscope_working) return "Giroscopio (Prioridad 3)";
    if (is_accelerometer_working) return "Acelerómetro (Prioridad 4)";
    return "Dirección configurada";
  }

  void OnDestroy() {
    if (accelerometer != null) InputSystem.DisableDevice(accelerometer);
    if (gyroscope != null) InputSystem.DisableDevice(gyroscope);
    if (magnetometer != null) InputSystem.DisableDevice(magnetometer);
    if (gravity_sensor != null) InputSystem.DisableDevice(gravity_sensor);
    
    Input.location.Stop();
    
    Debug.Log("Sensores deshabilitados");
  }
}