# Android TV / Google TV / Fire TV

Transport: classic ADB over the network on port 5555. The current desktop action
requires Android SDK Platform-Tools (`adb.exe`) on `PATH`, an address saved by the user,
network debugging enabled on the TV, and explicit authorization on that TV. Tervia does
not pair modern Wireless debugging by code/QR or turn debugging on for the user.

Implemented:

- host connection and authorization-state check when a session is opened
- explicit remote navigation keys (up, down, left, right, OK, back, home and menu)
- on-demand PNG capture, first using `adb exec-out screencap -p`
- fallback to a unique temporary TV file plus `adb pull` when binary stdout is not a valid PNG
- best-effort removal of the temporary file on the TV, the host transfer file, and the host-side ADB disconnect
- structured argument construction, no local shell concatenation

The UI exposes the remote keys and screenshot preview. Each button sends a single
explicit request and disconnects afterward. A screenshot is shown in memory and is not
stored in the TV profile. `screencap` may return an empty or unusable image for HDMI, Live
TV, DRM-protected video or firmware that blocks screen capture. An ADB protocol path is
not hardware verification; the compatibility matrix has no physical Android TV records
yet. Users should turn network debugging off in TV settings when finished.

Adapter implementations also exist for app inventory/launch/stop and system-property
diagnostics, but these actions are not currently exposed by the desktop UI and are not
claimed as hardware-verified.

Restricted:

- APK installation requires developer options and an authorised ADB key
- package disable/uninstall goes through the safety engine and is never offered
  as a bulk "debloat" button

Fire TV televisions share the transport but have their own package
classification (`FireTvPlatformProfile`).
