# Tervia

<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="assets/branding/logo-tervia-white.png">
    <img src="assets/branding/logo-tervia-color.png" alt="Tervia" width="132">
  </picture>
</p>

<p align="center"><strong>Local Smart TV management from your desktop.</strong><br>
Discover, control, inspect and maintain physical Smart TVs from one Windows application.</p>

<p align="center">
  <a href="https://github.com/smouj/tervia/actions/workflows/ci.yml"><img alt="CI" src="https://github.com/smouj/tervia/actions/workflows/ci.yml/badge.svg?branch=main"></a>
  <a href="https://github.com/smouj/tervia/actions/workflows/codeql.yml"><img alt="CodeQL" src="https://github.com/smouj/tervia/actions/workflows/codeql.yml/badge.svg?branch=main"></a>
  <a href="LICENSE"><img alt="License: MIT" src="https://img.shields.io/badge/license-MIT-2f6feb.svg"></a>
  <img alt=".NET 10" src="https://img.shields.io/badge/.NET-10-512bd4.svg">
  <img alt="Windows 10 / 11" src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078d4.svg">
  <img alt="Local first" src="https://img.shields.io/badge/local--first-no%20telemetry-1f9d55.svg">
</p>

---

Tervia treats each television as a device with **capabilities**, not as a protocol.
A Samsung Tizen TV, an LG webOS TV, a Roku TV and an Android TV expose very
different amounts of control; Tervia discovers what each one actually supports and
shows only what is real.

Supported ecosystems (protocol level):

| Platform | Discovery | Remote | Apps | Diagnostics | Developer |
| --- | --- | --- | --- | --- | --- |
| Android TV / Google TV | ADB, mDNS | protocol | protocol | protocol | ADB |
| Fire TV televisions | ADB, mDNS | protocol | protocol | protocol | ADB |
| Samsung Tizen | SSDP | protocol | restricted | vendor | Developer Mode |
| LG webOS | SSDP | protocol | vendor CLI | vendor | Developer Mode |
| Roku TV | SSDP | ECP | ECP | ECP | Developer Mode |
| VIDAA | SSDP | unverified | unverified | unverified | unknown |

> Status legend: **protocol** = implemented against documented local protocols;
> **restricted/unknown** = probed at runtime and reported honestly. Nothing is
> claimed as verified until it has been tested on real hardware and recorded in
> [docs/COMPATIBILITY.md](docs/COMPATIBILITY.md).

## Principles

- **Local first.** Configuration, TV profiles, snapshots and logs stay on the PC.
- **No Tervia account, no subscription, no ads, no telemetry.**
- **Capability driven.** Unsupported features are hidden or explained, never faked.
- **Platform isolated.** Core never references ADB, Tizen tooling, webOS CLI or ECP.
- **Conservative by default.** Destructive operations pass through the safety engine.
- **Works offline.** Core operation does not require Internet access.

## What Tervia is not

Not an Android-only app, not an ADB frontend, not a streaming-box manager, not a
cloud service. Physical televisions only — sticks, boxes, phones and consoles are
out of scope by design.

## Architecture

```text
Tervia.App (WinUI 3)
        │
Tervia.Application ── Capability engine ── Safety engine
        │
Platform adapter registry
        │
Android TV · Tizen · webOS · Roku TV · VIDAA · Unknown
        │
Physical Smart TVs on the local network
```

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the full layering and the
dependency rules, and [docs/CAPABILITIES.md](docs/CAPABILITIES.md) for the
canonical capability ids.

## Build

Requires the .NET 10 SDK.

```bash
dotnet restore Tervia.sln
dotnet build Tervia.sln -c Release
dotnet test Tervia.sln -c Release
```

The desktop application targets Windows 10 1809+ / Windows 11 and is built from
`src/Tervia.App`. Core libraries, adapters and tests are platform neutral and run
on any OS with the .NET 10 SDK.

## Documentation

| Document | Contents |
| --- | --- |
| [ARCHITECTURE](docs/ARCHITECTURE.md) | Layering, dependency rules, adapter contracts |
| [CAPABILITIES](docs/CAPABILITIES.md) | Canonical capability ids and statuses |
| [COMPATIBILITY](docs/COMPATIBILITY.md) | Tested models and verified behaviour |
| [DISCOVERY](docs/DISCOVERY.md) | SSDP, mDNS and manual addition |
| [SECURITY_MODEL](docs/SECURITY_MODEL.md) | Pairing, secrets, process execution |
| [RECOVERY](docs/RECOVERY.md) | Snapshots and what they can restore |
| [OFFLINE_MODEL](docs/OFFLINE_MODEL.md) | What works without Internet |
| [TESTING](docs/TESTING.md) | Mocks, unit tests, hardware validation |
| [PRIVACY](docs/PRIVACY.md) | What stays local |

## Status

Early development. The architecture, domain model, capability engine, safety
engine, discovery framework and mock environment are implemented and tested.
Platform adapters are being built in the order defined by the specification:
Android TV first as the reference adapter, then Tizen, webOS, Roku TV and VIDAA.
The WinUI 3 shell lands after the platform contracts are proven by tests.

See [CHANGELOG.md](CHANGELOG.md) for the milestone log.

## License

MIT — see [LICENSE](LICENSE).
