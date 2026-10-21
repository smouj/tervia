# Compatibilidad

La detección de un anuncio no confirma que sea un televisor. La implementación de un
protocolo tampoco confirma que funcione con un modelo concreto. No hay televisores
validados en hardware real todavía.

## Estado de la alpha

| Plataforma | Pista o protocolo de descubrimiento | Adaptador en el código | Operaciones desde la interfaz | Hardware verificado |
| --- | --- | --- | --- | --- |
| Android TV / Google TV | SSDP y servicios Cast por mDNS | Adaptador ADB para operaciones documentadas | No disponible todavía | Ninguno |
| Fire TV | SSDP; puede sugerirse Android/Fire TV | Adaptador ADB para operaciones documentadas | No disponible todavía | Ninguno |
| Samsung Tizen | SSDP; pairing requiere configuración del televisor | Superficie restringida hasta pairing y pruebas | No disponible todavía | Ninguno |
| LG webOS | SSDP; Developer Mode/pairing requiere configuración | Superficie restringida hasta pairing y pruebas | No disponible todavía | Ninguno |
| Roku TV | SSDP; Roku ECP en el adaptador | Adaptador ECP para operaciones documentadas | No disponible todavía | Ninguno |
| VIDAA | SSDP; la identificación es solo una pista | No se afirma control operativo | No disponible todavía | Ninguno |
| Desconocida | SSDP o dirección introducida por el usuario | Sin adaptador seleccionado por defecto | Biblioteca local solamente | Ninguno |

El shell de esta alpha permite buscar candidatos, revisar y confirmar manualmente un
televisor físico, guardar una dirección local y organizar perfiles. Guardar un perfil no
abre una conexión. Control remoto, gestión de aplicaciones, diagnóstico en vivo y
captura de pantalla del televisor no están disponibles desde la interfaz.

## Significado de los estados de capacidad

- `Supported`: prueba de la capacidad y evidencia asociada. No se asigna por reconocer
  una marca o protocolo.
- `Restricted`: necesita pairing, configuración del fabricante u otra condición; la
  función aún no se anuncia como lista.
- `Unknown` / `Unavailable`: no se ha demostrado que la función esté disponible.

## Registro de hardware

_Ninguno todavía._ Añade una fila por modelo solo después de probarlo físicamente:

```text
Modelo | Año | OS | Firmware | Funciones probadas | Fecha | Resultado
```

Incluye la versión de Tervia, el método de conexión y los pasos reproducibles. No
incluyas direcciones IP públicas, números de serie, claves de pairing ni otros secretos.
