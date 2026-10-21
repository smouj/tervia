# Changelog

All notable changes are documented here. Format follows Keep a Changelog and the
project uses semantic versioning once the first release exists.

## [Unreleased]

### Added

- Android TV, Google TV and Fire TV profiles expose basic ADB remote keys and an
  on-demand screenshot preview. ADB behavior is exercised with a fake process runner;
  no physical TV has been validated.
- The default Windows x64 WinUI build now includes .NET and Windows App SDK runtimes,
  so running the built `Tervia.exe` does not prompt to install .NET separately.

- Redesigned the Spanish-first WinUI home dashboard and added a direct path to manual
  IP entry.
- Prepared an x64 self-contained single-file Windows download with SHA-256 on tagged
  GitHub releases.
- Rewrote public compatibility, privacy, and install documentation to distinguish
  implemented desktop features from unverified TV protocols.

- M5 SSDP multicast discovery listener and mDNS/DNS-SD parsing with cancellation,
  address deduplication, bounded packets and per-strategy failure reporting.
- Initial M6 WinUI 3 shell with local TV library, manual IP entry, user-confirmed
  physical-TV enrollment, profile management, capability summary and privacy view.
- Versioned atomic JSON TV-profile storage with capability-evidence persistence.
- Windows CI build for the WinUI app and a portable solution filter for Linux CI.
- Repository foundation: MIT license, security policy, contribution guide,
  CI, CodeQL and dependency review.
- `Tervia.Core`: TV device, session, candidate, application, input and snapshot
  domain models; typed errors; remote key abstraction.
- `Tervia.Capabilities`: canonical capability ids, statuses, risk levels and
  capability sets with honest unsupported reporting.
- `Tervia.Safety`: central safety engine with per-platform policies and a
  default deny-by-unknown rule.
- `Tervia.Discovery`: strategy abstraction, merged discovery service, SSDP
  response parser and manual address strategy.
- `Tervia.Application`: adapter contracts, capability interfaces, registry and
  per-television operation queue.
- `Tervia.Infrastructure`: structured process runner and local storage layout.
- Platform adapters: Android TV (ADB), Roku TV (ECP), plus honest Tizen, webOS
  and VIDAA capability probes.
- Mock television environment with distinct capability sets per platform.
- Unit and integration tests for domain, capabilities, safety, discovery and
  adapter argument construction.
