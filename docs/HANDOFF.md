# Handoff

Context for a coding agent continuing this repository.

## Current state

- Architecture, domain, capabilities, safety, discovery, infrastructure,
  Android TV and Roku adapters, mocks and tests are in place.
- Tizen, webOS and VIDAA adapters currently publish capability sets only; their
  operations are intentionally unimplemented.
- The WinUI 3 shell is the next milestone.

## Rules that must not be broken

1. `Tervia.Core` and `Tervia.Application` never reference a platform protocol or
   a UI framework.
2. Capabilities come from probing; nothing is `Supported` without a test or a
   hardware record.
3. Destructive operations go through `TvSafetyEngine` and the per-TV queue.
4. No cloud service, account, telemetry or paid dependency.
5. Structured process arguments only; never build a shell string.

## Next milestones

- M5: mDNS strategy and a real SSDP listener with cancellation.
- M6: WinUI 3 shell (TV library, dashboard, capability-driven sections).
- M7: Android TV integration tests against a fake ADB executable.
- M8: Tizen pairing token flow and Developer Mode detection.
