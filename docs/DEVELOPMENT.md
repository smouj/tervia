# Development

## Requirements

- .NET 10 SDK
- Windows 10 1809+ or Windows 11 for the desktop shell
- optional: platform tools (`adb`, Tizen CLI, webOS CLI) for real hardware

## Build and test

On Windows, the full solution builds the WinUI 3 shell:

```bash
dotnet restore Tervia.sln
dotnet build Tervia.sln -c Release
dotnet test Tervia.sln -c Release
```

On Linux and macOS, use the portable solution filter. It includes all libraries,
adapters and tests while excluding the Windows-only application:

```bash
dotnet restore Tervia.Portable.slnf
dotnet build Tervia.Portable.slnf -c Release
dotnet test Tervia.Portable.slnf -c Release
```

## Layout

```text
src/            WinUI app, core, application, infrastructure, adapters, mocks
tests/          unit, storage, discovery and integration tests
docs/           architecture, platform notes, policies
data/           capability and profile fixtures
assets/branding logos
```

## Milestones

M0 repository · M1 domain · M2 capabilities · M3 contracts · M4 mocks ·
M5 discovery · M6 initial WinUI shell · M7 Android TV validation · M8 Tizen ·
M9 webOS · M10 Roku validation · M11 VIDAA — each milestone ends green.
