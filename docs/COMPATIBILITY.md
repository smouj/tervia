# Compatibilidad

La detección de un anuncio no confirma que sea un televisor. La implementación de un
protocolo tampoco confirma que funcione con un modelo concreto. No hay televisores
validados en hardware real todavía.

## Estado de la alpha

| Plataforma | Pista o protocolo de descubrimiento | Adaptador en el código | Operaciones desde la interfaz | Hardware verificado |
| --- | --- | --- | --- | --- |
| Android TV / Google TV | SSDP y servicios Cast por mDNS | ADB con teclas, captura y Tervia Home | Tervia Home personaliza textos, tema, reloj, visibilidad y orden; la TV solicita confirmar el rol de inicio | Ninguno |
| Fire TV integrado en TV | SSDP; puede sugerirse Android/Fire TV | Adaptador ADB para operaciones documentadas | Tervia Home, control y captura solo si ese modelo ofrece ADB, categorías Android y `screencap` compatibles | Ninguno |
| Samsung Tizen | SSDP; pairing requiere configuración del televisor | Superficie restringida hasta pairing y pruebas | No disponible todavía | Ninguno |
| LG webOS | SSDP; Developer Mode/pairing requiere configuración | Superficie restringida hasta pairing y pruebas | No disponible todavía | Ninguno |
| Roku TV | SSDP; Roku ECP en el adaptador | Adaptador ECP para operaciones documentadas | No disponible todavía | Ninguno |
| VIDAA | SSDP; la identificación es solo una pista | No se afirma control operativo | No disponible todavía | Ninguno |
| Desconocida | SSDP o dirección introducida por el usuario | Sin adaptador seleccionado por defecto | Biblioteca local solamente | Ninguno |

El shell de esta alpha permite buscar candidatos, revisar y confirmar manualmente un
televisor físico, guardar una dirección local y organizar perfiles. Guardar un perfil no
abre una conexión. Una captura solo se inicia después de pulsar el botón explícito en un
perfil Android/Google/Fire TV y requiere depuración ADB por red autorizada. Control
remoto básico (direcciones, OK, Atrás, Inicio y Menú) está disponible para Android/
Google/Fire TV; gestión de aplicaciones y diagnóstico en vivo siguen pendientes desde la interfaz.

Tervia Home es un APK separado que se incluye con el paquete de Windows. Su instalación
requiere ADB por red habilitado en la TV y la autorización del usuario en pantalla. Para
convertirse en la pantalla predeterminada, Android presenta su selección/confirmación de
launcher. La app conserva la pantalla original y no ejecuta comandos para desactivarla.
El APK compila en CI, pero la interacción con el sistema y las aplicaciones sigue sin
validarse en televisores físicos; la matriz no atribuye compatibilidad a modelos todavía.

## Significado de los estados de capacidad

- `Supported`: implementación disponible para el protocolo observado. La confianza
  `Protocol` no es una prueba en un modelo físico; solo `Verified` cuenta como capacidad
  comprobada en un dispositivo.
- `Restricted`: necesita pairing, configuración del fabricante u otra condición; la
  función aún no se anuncia como lista.
- `Unknown` / `Unavailable`: no se ha demostrado que la función esté disponible.

## Registro de hardware

_Ninguno todavía._ La captura PNG está implementada por ADB, pero no se ha ejecutado en
un televisor físico. Añade una fila por modelo solo después de probarlo físicamente:

```text
Modelo | Año | OS | Firmware | Funciones probadas | Fecha | Resultado
```

Incluye la versión de Tervia, el método de conexión y los pasos reproducibles. No
incluyas direcciones IP públicas, números de serie, claves de pairing ni otros secretos.
