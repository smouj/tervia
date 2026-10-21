# Development

## Requirements

- .NET 10 SDK
- Windows 10 1809+ or Windows 11 for the desktop shell
- Visual C++ Redistributable x64 on machines that do not already have it
- optional: platform tools (`adb`, Tizen CLI, webOS CLI) for real hardware

## Build and test

On Windows, the full solution builds the WinUI 3 shell:

```bash
dotnet restore Tervia.sln -p:Platform=x64
dotnet build Tervia.sln -c Release -p:Platform=x64
dotnet test Tervia.sln -c Release -p:Platform=x64
```

To validate the same self-contained Windows app that is included in the downloadable ZIP:

```powershell
.\scripts\publish-windows.ps1
```

The output goes to `artifacts/publish/win-x64/Tervia.exe`. Publishing a `v*` tag
builds the x64 app, places it and its runtime files in `Tervia-win-x64.zip`, calculates
the archive's SHA-256, and attaches both files to a GitHub release. A tag containing a
hyphen (for example `v0.1.0-alpha`) creates a pre-release. The app is not commercially
code-signed, so SmartScreen can show a warning.

On Linux and macOS, use the portable solution filter. It includes all libraries,
adapters and tests while excluding the Windows-only application:

```bash
dotnet restore Tervia.Portable.slnf
dotnet build Tervia.Portable.slnf -c Release
dotnet test Tervia.Portable.slnf -c Release
```

## CodeQL

CI runs CodeQL queries for both the portable projects and the Windows app. When the
workflow's `security-events: write` permission is available, CodeQL uploads SARIF for
review in GitHub Security. A successful workflow run means analysis completed; check
the Code scanning alerts page to review any findings.

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
