using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.UI;

public class Information : MonoBehaviour {
  public Transform ui_parent;  
  public Text ui_prefab;       

  string[] known_sensors = {
    "Accelerometer",
    "Gyroscope",
    "GravitySensor",
    "AttitudeSensor",
    "LinearAccelerationSensor",
    "MagneticFieldSensor",
    "LightSensor",
    "PressureSensor",
    "ProximitySensor",
    "HumiditySensor",
    "AmbientTemperatureSensor",
    "StepCounter",
    "GPS"  
  };

  System.Collections.Generic.Dictionary<string, Text> ui_entries =
  new System.Collections.Generic.Dictionary<string, Text>();

  bool gpsStarted = false;
  float gpsTimeout = 20f;
  float gpsTimer = 0f;

  void Start() {
    var found_sensors = new System.Collections.Generic.HashSet<string>();

    foreach (var device in InputSystem.devices) {
      if (device is Sensor) {
        InputSystem.EnableDevice(device);
        found_sensors.Add(device.layout);
      }
    }

    if (Input.location.isEnabledByUser) {
      Input.location.Start();
      gpsStarted = true;
      gpsTimer = 0f;
      found_sensors.Add("GPS"); 
    }

    foreach (var sensor in known_sensors) {
      bool available = found_sensors.Contains(sensor);

      Text entry = null;
      if (ui_parent != null && ui_prefab != null) {
        entry = Instantiate(ui_prefab, ui_parent);
        entry.color = available ? Color.green : Color.red;
        ui_entries[sensor] = entry;
      }

      if (!available) {
        Debug.Log("Sensor NO detectado: " + sensor);
        if (entry != null)
          entry.text = sensor + ": NO detectado";
      }
    }
  }

  void Update() {
    foreach (var device in InputSystem.devices) {
      if (!(device is Sensor))
        continue;

      string sensor_name = device.layout;
      if (!ui_entries.ContainsKey(sensor_name))
        continue;

      string value_str = "";

      switch (device) {
        case Accelerometer acc:
          value_str = acc.acceleration.ReadValue().ToString("F3");
          break;
        case UnityEngine.InputSystem.Gyroscope gyro:
          value_str = gyro.angularVelocity.ReadValue().ToString("F3");
          break;
        case GravitySensor grav:
          value_str = grav.gravity.ReadValue().ToString("F3");
          break;
        case AttitudeSensor att:
          value_str = att.attitude.ReadValue().eulerAngles.ToString("F3");
          break;
        case LinearAccelerationSensor lacc:
          value_str = lacc.acceleration.ReadValue().ToString("F3");
          break;
        case MagneticFieldSensor mag:
          value_str = mag.magneticField.ReadValue().ToString("F3");
          break;
        case LightSensor light:
          value_str = light.lightLevel.ReadValue().ToString("F3");
          break;
        case PressureSensor press:
          value_str = press.atmosphericPressure.ReadValue().ToString("F3");
          break;
        case ProximitySensor prox:
          value_str = prox.distance.ReadValue().ToString("F3");
          break;
        case HumiditySensor hum:
          value_str = hum.relativeHumidity.ReadValue().ToString("F3");
          break;
        case AmbientTemperatureSensor temp:
          value_str = temp.ambientTemperature.ReadValue().ToString("F3");
          break;
        case StepCounter steps:
          value_str = steps.stepCounter.ReadValue().ToString();
          break;
      }

      ui_entries[sensor_name].text = sensor_name + ": " + value_str;
    }

    if (gpsStarted && ui_entries.ContainsKey("GPS")) {
      gpsTimer += Time.deltaTime;

      if (Input.location.status == LocationServiceStatus.Running) {
        var data = Input.location.lastData;
        ui_entries["GPS"].text =
          $"Lat: {data.latitude:F6} " +
          $"Lon: {data.longitude:F6} " +
          $"Alt: {data.altitude:F1} m " +
          $"Precisión: {data.horizontalAccuracy:F1} m";
      } else if (Input.location.status == LocationServiceStatus.Initializing) {
        if (gpsTimer > gpsTimeout) {
          ui_entries["GPS"].text = "GPS: Timeout";
          gpsStarted = false;
        } else {
          ui_entries["GPS"].text = "GPS: Inicializando...";
        }
      } else {
        ui_entries["GPS"].text = "GPS: No disponible";
      }
    }
  }

  void OnDisable() {
    if (gpsStarted) {
      Input.location.Stop();
    }
  }
}
