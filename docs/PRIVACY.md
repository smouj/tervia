# Privacidad

Tervia es local-first. La alpha no tiene cuenta, publicidad, telemetría ni servicio
remoto de Tervia.

## Lo que hace la alpha

- Guarda la biblioteca de televisores en `%LOCALAPPDATA%\Tervia\tvs\library.json`.
- La búsqueda envía consultas SSDP/mDNS en la red local y recibe anuncios de los
  dispositivos que respondan. No usa un servidor de Tervia.
- El alta manual valida la sintaxis de una dirección IP, pero no intenta conectarse a
  esa dirección.
- La aplicación no guarda credenciales ni tokens de pairing; el pairing aún no está
  implementado.

El archivo local contiene nombres, direcciones y los datos de perfil que el usuario
añade. El usuario controla el acceso al perfil de Windows y puede cerrar Tervia y
eliminar esa carpeta para borrar su biblioteca.

## Límites

Las herramientas oficiales de fabricantes que se incorporen en versiones futuras
podrían tener su propio tráfico o avisos de red. Tervia no afirma que la alpha actual
exporte diagnósticos anonimizados ni que capture pantallas de televisores.
