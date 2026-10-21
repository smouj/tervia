# Tervia

<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="assets/branding/logo-tervia-white.png">
    <img src="assets/branding/logo-tervia-color.png" alt="Tervia" width="132">
  </picture>
</p>

<p align="center"><strong>Organiza televisores Smart TV desde tu PC Windows.</strong><br>
Aplicación local para descubrir televisores en la red y guardar sus perfiles en este equipo.</p>

<p align="center">
  <a href="https://github.com/smouj/tervia/actions/workflows/ci.yml"><img alt="CI" src="https://github.com/smouj/tervia/actions/workflows/ci.yml/badge.svg?branch=main"></a>
  <a href="https://github.com/smouj/tervia/actions/workflows/codeql.yml"><img alt="CodeQL" src="https://github.com/smouj/tervia/actions/workflows/codeql.yml/badge.svg?branch=main"></a>
  <a href="LICENSE"><img alt="Licencia MIT" src="https://img.shields.io/badge/licencia-MIT-2f6feb.svg"></a>
  <img alt=".NET 10" src="https://img.shields.io/badge/.NET-10-512bd4.svg">
  <img alt="Windows 10 y 11, x64" src="https://img.shields.io/badge/Windows-10%20%7C%2011%20x64-0078d4.svg">
  <img alt="Local, sin telemetría" src="https://img.shields.io/badge/local-sin%20telemetr%C3%ADa-1f9d55.svg">
</p>

---

Tervia es una aplicación de escritorio para Windows. **No se instala en el televisor.**
Su objetivo es ayudarte a encontrar y organizar televisores físicos de tu red local,
mostrando con claridad qué se ha comprobado y qué sigue pendiente.

> **Estado: alpha.** El inicio, la biblioteca local, el descubrimiento y el alta manual
> forman parte del shell actual. La biblioteca permite solicitar una captura real de
> pantalla en Android TV, Google TV y Fire TV con ADB por red. El control remoto, la
> instalación o el lanzamiento de aplicaciones y los diagnósticos todavía no están
> disponibles desde la interfaz. Ningún modelo se ha validado en hardware real.

## Así se ve la aplicación

Estas imágenes se capturan de la aplicación Tervia ejecutándose en Windows. Son
pantallas de la aplicación de escritorio; **no son capturas de una televisión**.
La captura real por ADB está disponible bajo demanda para TVs Android compatibles,
pero todavía no hay modelos validados en hardware.

| Inicio | Descubrimiento |
| --- | --- |
| ![Pantalla Inicio de Tervia para Windows](assets/screenshots/home.png) | ![Pantalla de descubrimiento local de Tervia](assets/screenshots/discovery.png) |

| Privacidad y datos |
| --- |
| ![Pantalla de privacidad de Tervia](assets/screenshots/privacy.png) |

## Descargar y empezar

**Windows 10 (versión 1809 o posterior) o Windows 11, x64.** La publicación de
Windows se distribuye como un ejecutable portátil autocontenido: no requiere instalar
por separado .NET ni Windows App SDK. La descarga es un ZIP portátil; no modifica el
registro ni instala servicios.

1. Abre [Releases](https://github.com/smouj/tervia/releases) y descarga
   `Tervia-win-x64.zip` de la versión más reciente.
2. En el Explorador, haz clic derecho en el ZIP y elige **Extraer todo**.
3. Abre la carpeta extraída y ejecuta `Tervia.exe`. Puedes crear un acceso directo o
   anclarlo a Inicio desde Windows.

Si Windows indica que falta el runtime de Visual C++, instala el
[paquete x64 oficial de Microsoft](https://aka.ms/vc14/vc_redist.x64.exe) y vuelve a
abrir Tervia.

El ZIP de **Releases** ya incluye .NET y Windows App SDK. Si Windows te pide descargar
.NET, confirma que extrajiste el ZIP completo y abriste `Tervia.exe` (no `Tervia.dll` ni
una versión compilada desde el código). Para compilar Tervia desde el repositorio sí se
necesita el SDK de .NET 10.

La aplicación no está firmada con un certificado comercial; Windows SmartScreen puede
mostrar un aviso. Comprueba el SHA-256 publicado junto al ZIP y continúa solo si
confías en la descarga. En PowerShell puedes calcularlo con
`Get-FileHash .\Tervia-win-x64.zip -Algorithm SHA256` y compararlo con
`SHA256SUMS.txt`. No hace falta una cuenta de Tervia para usar las funciones locales.
Consulta [Seguridad](SECURITY.md) para informar de problemas.

## Qué puedes hacer ahora

- **Inicio:** consultar el resumen de tu biblioteca y llegar a las acciones principales.
- **Descubrir:** buscar anuncios SSDP y servicios mDNS en la red local.
- **Añadir manualmente:** introducir una dirección IPv4 o IPv6 local sin probar la
  conexión ni resolver nombres DNS.
- **Revisar y guardar:** confirmar que el resultado corresponde a un televisor físico
  antes de guardarlo en la biblioteca de este PC.
- **Capturar pantalla:** desde el perfil de un Android TV, Google TV o Fire TV, conectar
  por ADB solo al pulsar **Conectar y capturar** y mostrar el PNG en esa ventana. Puede
  aparecer una solicitud de autorización en la pantalla de la TV. La imagen no se guarda
  en el perfil; HDMI, Live TV y vídeo protegido pueden impedir la captura.
- **Privacidad:** consultar qué datos guarda Tervia y qué actividad de red realiza.

Guardar una dirección **no** empareja, sondea ni controla el dispositivo. Una pista de
fabricante o protocolo no demuestra que un dispositivo sea una TV ni que una función
esté disponible.

## Compatibilidad, sin promesas implícitas

Tervia está construida por capacidades, no por una lista fija de marcas. Los anuncios
locales pueden sugerir Roku, Tizen, webOS, VIDAA, Fire TV o Android/Google TV, pero la
detección solo produce candidatos y pistas. No hay modelos de televisores verificados
en hardware real todavía.

| Función | Estado actual |
| --- | --- |
| Interfaz de escritorio WinUI 3 | Disponible en esta alpha |
| Descubrimiento local SSDP y mDNS | Implementado; resultado no equivale a compatibilidad verificada |
| Alta manual y biblioteca local | Disponible; guardar no inicia una conexión |
| Captura de pantalla bajo demanda por ADB | Implementada para Android/Google/Fire TV; pendiente de validación física por modelo |
| Control remoto, gestión de apps y diagnósticos desde la interfaz | Pendiente |
| Emparejamiento Tizen/webOS y pruebas físicas por modelo | Pendiente |
| Captura de pantalla en Roku, Tizen, webOS y VIDAA | No disponible en esta alpha |

Consulta [COMPATIBILITY.md](docs/COMPATIBILITY.md) para los estados por plataforma y
los resultados de pruebas en hardware. Los estados actuales son `Unknown`, `Supported`,
`Restricted` y `Unavailable`; `Restricted` indica que se necesita configuración o
pairing. Un candidato detectado no se marca `Supported` por el mero hecho de reconocer
su marca o plataforma.

## Privacidad y seguridad

- Sin cuenta, suscripción, anuncios ni telemetría de Tervia.
- Perfiles y datos locales en `%LOCALAPPDATA%\Tervia`.
- La búsqueda consulta la red local mediante SSDP y mDNS. Añadir una IP manualmente no
  contacta ese dispositivo.
- La captura solo inicia una conexión cuando la solicitas. Requiere `adb.exe` de
  [Android SDK Platform-Tools](https://developer.android.com/tools/releases/platform-tools)
  en `PATH` y depuración ADB por red habilitada en la TV. Actualmente usa el puerto
  clásico 5555; no implementa el emparejamiento Wireless debugging por código o QR.
  Tervia no almacena claves de ADB; desactiva la depuración en los ajustes del televisor
  al terminar. Consulta la [guía oficial de ADB](https://developer.android.com/tools/adb).
- Las capturas se muestran en memoria y se descartan al cambiar de televisor o cerrar la
  ventana. Si el modo directo no produce un PNG válido, se usan archivos temporales
  únicos en el PC y la TV; Tervia intenta borrarlos al terminar.
- Las pistas de red no se tratan como identidad verificada.
- Las operaciones sensibles deben pasar por el motor de seguridad y requerir
  confirmación cuando corresponda.

Lee [PRIVACY.md](docs/PRIVACY.md) y [SECURITY_MODEL.md](docs/SECURITY_MODEL.md).

## Compilar desde el código fuente

Se necesita .NET 10 SDK. Para compilar la interfaz, utiliza Windows:

```powershell
dotnet restore Tervia.sln -p:Platform=x64
dotnet build Tervia.sln -c Release -p:Platform=x64 --no-restore
dotnet test Tervia.sln -c Release -p:Platform=x64 --no-build --no-restore
```

En Linux o macOS, compila las bibliotecas portables y los tests con:

```bash
dotnet restore Tervia.Portable.slnf
dotnet build Tervia.Portable.slnf -c Release --no-restore
dotnet test Tervia.Portable.slnf -c Release --no-build --no-restore
```

El flujo de CI compila también el shell WinUI en Windows. Más detalles en
[Desarrollo](docs/DEVELOPMENT.md).

## Documentación

| Documento | Contenido |
| --- | --- |
| [Arquitectura](docs/ARCHITECTURE.md) | Capas, dependencias y adaptadores |
| [Capacidades](docs/CAPABILITIES.md) | Identificadores y estados de funciones |
| [Compatibilidad](docs/COMPATIBILITY.md) | Estado por plataforma y hardware probado |
| [Descubrimiento](docs/DISCOVERY.md) | SSDP, mDNS y alta manual |
| [Modelo de seguridad](docs/SECURITY_MODEL.md) | Confirmaciones, procesos y secretos |
| [Privacidad](docs/PRIVACY.md) | Datos locales y actividad de red |
| [Recuperación](docs/RECOVERY.md) | Snapshots y límites de restauración |
| [Modo sin conexión](docs/OFFLINE_MODEL.md) | Funciones que no necesitan Internet |
| [Investigación CleanYourTV](docs/REFERENCES.md) | Alcance de la referencia y prácticas adaptadas con seguridad |
| [Desarrollo](docs/DEVELOPMENT.md) | Build, CI y milestones |
| [Cambios](CHANGELOG.md) | Historial de versiones |

## Contribuir

Lee [CONTRIBUTING.md](CONTRIBUTING.md) y el [Código de conducta](CODE_OF_CONDUCT.md).
No se declara una capacidad compatible hasta aportar pruebas y actualizar la matriz de
compatibilidad. Los informes de seguridad se reciben en privado según
[SECURITY.md](SECURITY.md).

## Licencia

MIT. Consulta [LICENSE](LICENSE).
