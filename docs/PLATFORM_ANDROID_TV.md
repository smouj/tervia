# Android TV / Google TV / Fire TV

Transport: classic ADB over the network on port 5555. The current desktop action
requires Android SDK Platform-Tools (`adb.exe`), an address saved by the user, network
debugging enabled on the TV, and explicit authorization on that TV. Tervia checks
`ANDROID_SDK_ROOT`, `ANDROID_HOME`, the standard local SDK folder and then `PATH` for
ADB. The Windows ZIP includes the Tervia Home APK but does not redistribute Android
Platform-Tools. Tervia does not pair modern Wireless debugging by code/QR or turn
debugging on for the user.

Implemented:

- host connection and authorization-state check when a session is opened
- explicit remote navigation keys (up, down, left, right, OK, back, home and menu)
- on-demand PNG capture, first using `adb exec-out screencap -p`
- fallback to a unique temporary TV file plus `adb pull` when binary stdout is not a valid PNG
- best-effort removal of the temporary file on the TV, the host transfer file, and the host-side ADB disconnect
- structured argument construction, no local shell concatenation
- companion Tervia Home APK with Android `HOME` and TV launcher declarations
- Windows editor for per-TV title, subtitle, accent theme, clock, visible apps and pinned-first order
- install/update through a user-initiated ADB session; Android's own home-app selection asks before changing the default
- direct route to Android home-app settings so the user can restore the OEM launcher; no stock launcher package is disabled or removed

The UI exposes the remote keys and screenshot preview. Each button sends a single
explicit request and disconnects afterward. A screenshot is shown in memory and is not
stored in the TV profile. `screencap` may return an empty or unusable image for HDMI, Live
TV, DRM-protected video or firmware that blocks screen capture. An ADB protocol path is
not hardware verification; the compatibility matrix has no physical Android TV records
yet. Users should turn network debugging off in TV settings when finished.

Adapter implementations also exist for app inventory/launch/stop and system-property
diagnostics, but these actions are not currently exposed by the desktop UI and are not
claimed as hardware-verified.

## Tervia Home

The home APK targets Android API 23+ and uses the platform home role where available.
On the TV it lists launchable apps with their TV-provided icon and label, supports D-pad
focus, and saves bounded profile data in private app preferences. The Windows editor
currently enumerates third-party package identifiers over ADB. The profile is sent as
validated Base64 JSON to an explicit component; the Android app applies a size limit,
text cleanup, package-id allowlist and theme allowlist before storing it. No privileged
Android permission is requested.

The desktop preview uses placeholder app cards; the launcher on the television reads
the actual apps installed there. Visibility changes affect only the Tervia Home grid and
do not disable, hide or uninstall any Android package. Pinned apps sort before the rest.

Making Tervia Home the default requires the user's confirmation on the TV. On Android
versions or vendor builds that do not offer the role request UI, Tervia opens home-app
settings so the user can choose manually. The OEM launcher remains installed and can be
selected again in those same settings. Fire TV firmware can restrict or omit Android's
standard role surface; that path needs per-model physical verification.

### Build

The Android companion is a Java/Gradle module under `src/Tervia.Home.Android`. The Windows
publish script builds the debug-signed APK by default and copies it into the portable
release folder. A public release build uses a separately managed private keystore via
environment variables/GitHub Actions secrets. Never commit a keystore. Hardware support
and release key configuration are separate concerns: a successful APK build is not a TV
compatibility claim.

Restricted:

- APK installation requires developer options and an authorised ADB key
- package disable/uninstall goes through the safety engine and is never offered
  as a bulk "debloat" button

Fire TV televisions share the transport but have their own package
classification (`FireTvPlatformProfile`).
