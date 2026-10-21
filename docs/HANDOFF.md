# Handoff

Context for a coding agent continuing this repository.

## Current state

- Architecture, domain, capabilities, safety, SSDP/mDNS discovery, local JSON
  TV profiles, Android TV and Roku adapters, mocks and tests are in place.
- The first WinUI 3 shell provides a TV library, local discovery, explicit
  physical-device confirmation, capability summary and privacy/data view.
- The shell has an explicit Android/Google/Fire TV screenshot action in the local
  device library. It requires classic ADB over TCP port 5555, a TV that exposes
  `screencap`, and user authorization; the image is previewed and not saved.
- Remote keys, app operations and live diagnostics are still not exposed in the UI.
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

- M7: complete fake-ADB integration coverage, modern Wireless debugging pairing, and
  physical screenshot validation on recorded Android TV models.
- M8: Tizen pairing token flow and Developer Mode detection.
- M9: webOS pairing and Developer Mode flow.
- M10: Roku TV hardware validation and compatibility records.
- M11: VIDAA discovery and operation validation.
