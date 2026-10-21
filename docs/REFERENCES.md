# Investigación: CleanYourTV

Referencia analizada: [Gathin23/cleanyourtv](https://github.com/Gathin23/cleanyourtv), en especial su [README](https://github.com/Gathin23/cleanyourtv/blob/main/README.md), [`PROMPT.md`](https://github.com/Gathin23/cleanyourtv/blob/main/PROMPT.md), [troubleshooting](https://github.com/Gathin23/cleanyourtv/blob/main/docs/TROUBLESHOOTING.md), sus notas por modelo en `docs/devices/` y sus listas comunitarias en `examples/`.

## Qué es y qué no es

CleanYourTV se centra en Android TV y Google TV: diagnóstico ADB, cambios de paquetes
reversibles y un launcher alternativo. Sus notas muestran casos concretos por modelo y
firmware. No es una interfaz de control universal para Tizen, webOS, Roku o VIDAA; el
launcher y las operaciones de limpieza no corresponden al alcance actual de Tervia.

Los documentos y scripts de ese proyecto contienen pasos destinados a modificar TVs
reales. Aquí se leyeron como material de referencia; no se ejecutaron en ningún
televisor. No se copiaron código, listas de paquetes, perfiles de modelos ni recursos.
La licencia MIT del repo no convierte sus observaciones comunitarias en pruebas de
compatibilidad de Tervia.

## Prácticas que sí se incorporan

- **Evidencia específica del aparato.** Que un protocolo esté implementado no prueba
  que una función funcione en un modelo. Tervia separa ahora confianza `Protocol` de
  `Verified`; solo una captura válida solicitada en ese perfil se registra como
  `Verified`.
- **Conexión explícita.** Guardar un perfil continúa siendo local y silencioso. ADB se
  invoca solo después de pulsar **Conectar y capturar**, usa el puerto clásico 5555 y
  requiere que el usuario autorice el PC en la pantalla del televisor.
- **Captura real y limitada.** La interfaz obtiene un PNG bajo demanda, valida su
  estructura y dimensiones, y lo previsualiza sin guardarlo en la biblioteca. Si `adb
  exec-out` devuelve texto del firmware junto al PNG, intenta el método de archivo
  temporal más `adb pull`; los dos temporales se limpian al terminar siempre que la
  conexión lo permita.
- **Límites visibles.** HDMI, Live TV, vídeo protegido, firmware y configuración pueden
  impedir una captura. No se informa compatibilidad universal ni se crea un registro de
  hardware sin probar el modelo físico.

## Qué se deja fuera de Tervia

Tervia no aplica listas de `pm disable-user`, no reemplaza el launcher, no modifica
Private DNS, no desactiva paquetes ni toca servicios de fábrica. Esas acciones pueden
romper funciones de un modelo específico y son ajenas a la función de captura. Las
operaciones de ADB disponibles en adaptadores pero no expuestas por la interfaz tampoco
se presentan como funciones del producto.

## Limitación de ADB actual

Tervia busca `adb.exe` en `PATH` y conecta a `<IP>:5555`. No implementa el flujo moderno
de Android Wireless debugging que empareja por código/QR en un puerto temporal. Las TVs
que solo admiten ese emparejamiento no pueden capturarse desde esta alpha. Consulta la
[documentación Android de ADB](https://developer.android.com/tools/adb) y la guía oficial
de [SDK Platform-Tools para Windows](https://developer.android.com/tools/releases/platform-tools).
