# Fuentes de plataforma

Tervia implementa integraciones locales solo cuando existe una interfaz documentada y
el televisor permite activarla. Esta página reúne la documentación oficial que guía las
decisiones del proyecto. Reconocer un protocolo no demuestra que una función opere en
un modelo concreto; los resultados físicos se anotan por modelo en
[`COMPATIBILITY.md`](COMPATIBILITY.md).

## Android TV, Google TV y Fire TV

- [Android: declarar una app para TV](https://developer.android.com/training/tv/get-started/create): categorías de inicio, experiencia Leanback e iconos para televisor.
- [Android: `RoleManager` y rol HOME](https://developer.android.com/reference/android/app/role/RoleManager): selección de la aplicación de inicio mediante el consentimiento del sistema.
- [Android: categoría `HOME`](https://developer.android.com/reference/android/content/Intent#CATEGORY_HOME): contrato de una actividad de pantalla de inicio.
- [Android Debug Bridge](https://developer.android.com/tools/adb): transporte de desarrollo que Tervia usa solo cuando el usuario inicia una operación.
- [SDK Platform-Tools](https://developer.android.com/tools/releases/platform-tools): distribución oficial y documentación de ADB.

Tervia Home declara los intent filters de `HOME` y `LEANBACK_LAUNCHER`. No desactiva,
desinstala ni sustituye silenciosamente el launcher del fabricante. El usuario confirma
la aplicación de inicio desde Android y puede volver a Ajustes para elegir el inicio
original. La depuración por red debe configurarse y autorizarse en el propio televisor.

## Samsung Tizen

- [Samsung TV: conectar un televisor para desarrollo](https://developer.samsung.com/smarttv/develop/getting-started/using-sdk/tv-device.html)
- [Samsung TV: guía de inicio rápido](https://developer.samsung.com/smarttv/develop/getting-started/quick-start-guide.html)

El modo de desarrollador y la firma de paquetes son requisitos del fabricante. Tervia no
los omite ni declara pairing operativo hasta completar su flujo específico y las
pruebas por modelo.

## LG webOS

- [webOS TV CLI](https://webostv.developer.lge.com/develop/tools/cli-introduction)

La herramienta oficial y el modo Developer se documentan aquí. El adaptador de Tervia
seguirá mostrando la integración como restringida mientras no exista pairing probado.

## Roku TV

- [Roku External Control API](https://developer.roku.com/dev/docs/external-control-api)

ECP es un control local en la LAN y requiere que las opciones del televisor lo permitan.
Tervia no supone que cualquier Roku detectado sea un televisor Roku ni que acepte todas
las operaciones.

## Reglas de uso de estas fuentes

- La documentación del fabricante define contratos y requisitos; no sustituye una
  prueba en la TV real.
- No ejecutar en televisores scripts de ejemplo que deshabiliten paquetes o cambien
  ajustes sensibles.
- No guardar credenciales de fabricante, eludir firmas, DRM ni modo desarrollador.
- Registrar el modelo, firmware, función y fecha de cualquier verificación física sin
  incluir datos personales ni secretos.
