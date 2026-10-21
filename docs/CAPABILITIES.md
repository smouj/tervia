# Capabilities

Capabilities are platform agnostic. Adapters implement only what the vendor
actually allows.

| Id | Meaning | Typical risk |
| --- | --- | --- |
| `remote.navigation` | Directional keys and OK | Safe |
| `remote.home` / `remote.back` | Home and back keys | Safe |
| `remote.keyboard` | Sending text to the TV | Safe |
| `power.read` / `power.on` / `power.off` | Power state and control | Low to Medium |
| `volume.read` / `volume.change` / `volume.mute` | Audio level | Low |
| `input.list` / `input.switch` | HDMI, antenna and other inputs | Low |
| `apps.list` / `apps.launch` | Application inventory and launch | Safe to Low |
| `apps.stop` / `apps.install` / `apps.remove` | Application lifecycle | Medium to High |
| `display.read.*` / `display.change.picturemode` | Panel state | Low to Medium |
| `audio.read.output` / `audio.change.output` | Output routing | Low |
| `diagnostics.*` | System, storage, network information | Safe |
| `developer.logs` / `developer.screenshot` / `developer.shell` | Developer tools | Safe to High |
| `maintenance.*package` | Android package management | Medium to Critical |
| `recovery.snapshot` / `recovery.restore` | Snapshots and diff restore | Medium |

## Statuses

- **Supported** — implemented and exercised by tests.
- **Restricted** — possible, but requires pairing, developer mode or vendor tooling.
- **Unavailable** — the platform does not expose it.
- **Unknown** — not verified; the UI must not offer it as if it worked.

## Confidence

- `Protocol` means the adapter implements the operation and its prerequisites were
  observed, but no particular physical TV has been tested.
- `Verified` is recorded for an individual saved TV only after the operation succeeds
  there. It is not a blanket certification of that model, firmware family or brand.
