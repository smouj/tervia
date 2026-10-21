# Testing

## Unit tests

- `Tervia.Core.Tests` — domain model and typed errors
- `Tervia.Capabilities.Tests` — capability statuses and sets
- `Tervia.Safety.Tests` — risk policy and denials
- `Tervia.Discovery.Tests` — SSDP parsing, deduplication, failing strategies
- `Tervia.Infrastructure.Tests` — local profile round-trip, updates and removal
- `Tervia.Platforms.Tests` — structured argument construction
- `Tervia.IntegrationTests` — registry, session service, operation queue

## Mocks

`Tervia.Testing.Mocks` provides deterministic televisions with **different**
capability sets. A mock never supports everything:

| Mock | Capabilities |
| --- | --- |
| Sony Google TV | remote, apps, diagnostics, screenshot, packages |
| Samsung Tizen | restricted remote, restricted apps |
| LG webOS | restricted remote |
| Roku TV | remote, apps, diagnostics |
| Hisense VIDAA | unknown remote |
| Unknown TV | none |

## Hardware validation

Record every tested model in `docs/COMPATIBILITY.md` with OS version, firmware and
the exact features exercised. CI never requires a physical television.

The WinUI shell is built by the Windows CI job. Portable libraries and tests run
on Linux so parsers and local persistence remain exercisable without Windows app
tooling.
