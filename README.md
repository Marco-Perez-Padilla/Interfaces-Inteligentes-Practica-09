# Interfaces-Inteligentes-Practica-09

En esta práctica se trata de reoclectar información de sensores de dispositivos Android y realizar una simple orientación y aceleración hacia el norte en base a los datos recopilados por los sensores

---

### 1. Recopilación de información de sensores y uso de la misma

Utilizar la escena de los guerreros y activa la reproducción de los sonidos incluidos en la carpeta adjunta cuando un guerrero alcanza algún objetivo.

Crear una apk que oriente alguno de los guerreros de la práctica mirando siempre hacia el norte, avance con una aceleración proporcional a la del dispositivo y lo pare cuando el dispositivo esté fuera de un rango de latitud, longitud dado.
Utilizar la escena de los guerreros y activa la reproducción de los sonidos incluidos en la carpeta adjunta cuando un guerrero alcanza algún objetivo.

Para este ejercicio, en primer lugar se ha realizado un script que recopilase toda la información disponible de los sensores (si es que los hay) y los mostrase en una UI. Ya en este apartado surgieron ciertas compliaciones técnicas como:
- Funcionamiento erróneo en el editor de Unity en el ordenador: No detecta ningún sensor ni tiene capacidad de simularlos. Se recurre a Unity Remote como solución alternativa.
- En Unity Remote con un móvil Samsung A70, detecta los siguientes sensores: Acelerómetro, giroscopio, sensor de orientación, sensor de gravedad y sensor de aceleración lineal. Sin embargo, de los 5 sensores detectados, solo el acelerómetro arroja valores, mientras que los demás tienen todos sus campos en 0.
- En la versión final se crea la APK y se ejecuta en una tablet Lenovo, esta vez no detectando ningún tipo de sensor.
  
Por otra parte, la orientación y aceleración del guerrero viene condicionada por esos mismos problemas. Con Unity Editor se consiguió una orientación con respecto al acelerómetro, pero nunca una orientación automática al norte real. En la ejecución final, la orientación se basa en una variable ajustada a mano para que la posición del guerrero corresponda con el norte desde el lugar en el que se desarrolla esta práctica.

  
**Scripts:**  
- [Information.cs](Scripts/Information.cs)  
- [Accelerating.cs](Scripts/Accelerating.cs)

![GIF](Gifs/GIF-E-1.gif)


APK creada: [Guerrero.apk](apk/guerrero.apk)
