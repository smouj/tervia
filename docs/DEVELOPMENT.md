# Development

## Requirements

- .NET 10 SDK
- Windows 10 1809+ or Windows 11 for the desktop shell
- optional: platform tools (`adb`, Tizen CLI, webOS CLI) for real hardware

## Build and test

```bash
dotnet restore Tervia.sln
dotnet build Tervia.sln -c Release
dotnet test Tervia.sln -c Release
```

## Layout

```text
src/            core, application, infrastructure, adapters, mocks
tests/          unit and integration tests
docs/           architecture, platform notes, policies
data/           capability and profile fixtures
assets/branding logos
```

## Milestones

M0 repository · M1 domain · M2 capabilities · M3 contracts · M4 mocks ·
M5 discovery · M6 UI · M7 Android TV · M8 Tizen · M9 webOS · M10 Roku ·
M11 VIDAA — each milestone ends green.
