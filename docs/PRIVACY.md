# Privacidad

Tervia es local-first. La alpha no tiene cuenta, publicidad, telemetría ni servicio
remoto de Tervia.

## Lo que hace la alpha

- Guarda la biblioteca de televisores en `%LOCALAPPDATA%\Tervia\tvs\library.json`.
- Guarda un perfil de personalización de Tervia Home por TV en
  `%LOCALAPPDATA%\Tervia\profiles\home`. El nombre de archivo deriva de un hash del
  identificador local de la TV; el JSON contiene título, subtítulo, título de sección,
  paleta, visibilidad de reloj/marca, tamaño de tarjetas y los identificadores de paquetes
  que se muestran, se ocultan o se fijan en un orden elegido.
- La búsqueda envía consultas SSDP/mDNS en la red local y recibe anuncios de los
  dispositivos que respondan. No usa un servidor de Tervia.
- El alta manual valida la sintaxis de una dirección IP, pero no intenta conectarse a
  esa dirección.
- El control remoto solo conecta al pulsar una tecla en el perfil seleccionado. Cada
  acción envía una tecla ADB y después Tervia intenta cerrar la conexión al televisor.
- **Guardar diseño en este equipo** escribe el perfil local sin conectarse a la TV.
  Instalar/actualizar Tervia Home y enviarle el diseño solo conectan tras pulsar sus
  botones. Al aplicarlo correctamente, el perfil queda guardado localmente y en las
  preferencias privadas de Tervia Home; no se sincroniza con una cuenta, servidor ni otro
  equipo.
- Al abrir la sección **Inicio de TV**, Tervia consulta por ADB el inventario de apps de
  la TV seleccionada para poder mostrarlas en el editor. No consulta otras TVs de la
  biblioteca y cierra la sesión al terminar.
- La captura de pantalla solo conecta tras la acción explícita **Conectar y capturar**
  y la confirmación ADB que el usuario acepte en la TV. El PNG se previsualiza en
  memoria, no se guarda en la biblioteca ni se envía a un servidor de Tervia. El método
  alternativo de captura crea un archivo temporal en el host y otro en la TV; ambos se
  eliminan al terminar siempre que el sistema de archivos y la conexión lo permitan.
- La aplicación no guarda credenciales ni tokens de pairing; el pairing aún no está
  implementado. La herramienta `adb` externa puede conservar una clave de host propia
  en el perfil del usuario; Tervia no la lee ni la copia.

El archivo local contiene nombres, direcciones y los datos de perfil que el usuario
añade. El usuario controla el acceso al perfil de Windows y puede cerrar Tervia y
eliminar esa carpeta para borrar su biblioteca.

La pantalla de inicio de Android solo muestra u ordena accesos de aplicaciones dentro de
Tervia Home. Cambiar el inicio predeterminado pasa por la selección de Android en la TV;
el launcher original del fabricante permanece instalado y puede elegirse de nuevo.

## Límites

El transporte ADB depende de Platform-Tools instalado por el usuario y de la
configuración de depuración del fabricante. Al habilitar depuración de red se amplía el
acceso de herramientas en esa red; apágala en los ajustes de la TV cuando acabes.
Tervia no exporta diagnósticos ni realiza capturas automáticas.
