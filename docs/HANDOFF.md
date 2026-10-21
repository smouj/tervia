# Handoff

Context for a coding agent continuing this repository.

## Product priority

- Tervia Home is a primary product workflow: Windows configures a launcher running on a
  compatible Android TV, Google TV or integrated Fire TV. The desktop app also retains
  its local TV library and cross-platform discovery.
- Launcher personalization includes title, subtitle, theme, clock, visible apps and
  pinned-first ordering. Keep app settings local per TV and send them only on an explicit
  user action.
- Replacing the default launcher must use Android's system role/home selection and
  consent. Never remove or disable the manufacturer's launcher; always keep a clear path
  back to Android Home Settings.
- Do not claim physical TV compatibility until the model and firmware have a record in
  `docs/COMPATIBILITY.md`.

## Current state

- Architecture, domain, capabilities, safety, SSDP/mDNS discovery, local JSON
  TV profiles, Android TV and Roku adapters, mocks and tests are in place.
- The first WinUI 3 shell provides a TV library, local discovery, explicit
  physical-device confirmation, capability summary and privacy/data view.
- The shell has an explicit Android/Google/Fire TV screenshot action in the local
  device library. It requires classic ADB over TCP port 5555, a TV that exposes
  `screencap`, and user authorization; the image is previewed and not saved.
- Basic Android/Google/Fire TV remote keys (directional, OK, back, home, menu) are
  exposed in the UI. Each press opens and closes an explicit ADB session.
- A separate `Tervia.Home.Android` APK provides a D-pad-friendly Home screen. The WinUI
  page configures title, subtitle, theme, clock, app visibility and pinned order; it
  installs/updates the APK and sends the profile over the user's authorized ADB session.
- Tervia Home requests the Android `HOME` role from the TV, which displays its own
  confirmation. The original launcher remains installed. Returning to the original home
  app opens Android's home-app settings.
- The Windows publish script builds the Android APK before packaging it beside
  `Tervia.exe`. Release signing needs a private Android signing key in CI secrets; do not
  create, commit or print signing credentials.
- App operations and live diagnostics are still not exposed in the UI.
- No physical television has been validated; a successful screenshot marks only that
  saved device capability `Verified` and does not add a model-wide compatibility claim.
- Tizen, webOS and VIDAA adapters currently publish capability sets only; their
  operations are intentionally unimplemented.
- Windows CI builds the WinUI app; portable CI builds and tests the libraries.

## Rules that must not be broken

1. `Tervia.Core` and `Tervia.Application` never reference a platform protocol or
   a UI framework.
2. Capabilities come from probing; nothing is `Supported` without a test or a
   hardware record.
3. Destructive operations go through `TvSafetyEngine` and the per-TV queue.
4. No cloud service, account, telemetry or paid dependency.
5. Structured process arguments only; never build a shell string.

## Next milestones

- M7: finish the Tervia Home installation path, improve the first-run ADB guidance,
  exercise profile/role consent in an Android TV emulator, and validate on recorded TV
  models. Add modern Wireless debugging pairing only after its permission and UX flow is
  specified.
- M8: Tizen pairing token flow and Developer Mode detection.
- M9: webOS pairing and Developer Mode flow.
- M10: Roku TV hardware validation and compatibility records.
- M11: VIDAA discovery and operation validation.
