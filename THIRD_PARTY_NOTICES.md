# Third-party notices

Tervia does not bundle vendor binaries.

External tools used at runtime, when the user has installed them:

| Tool | Vendor | License / terms | Bundled |
| --- | --- | --- | --- |
| Android platform tools (`adb`) | Google | Android SDK terms | No |
| Tizen CLI / sdb | Samsung | Samsung developer terms | No |
| webOS CLI | LG | LG developer terms | No |

Tervia detects these tools and tells the user what is missing. Redistribution is
never assumed; if a license does not clearly allow bundling, the tool stays an
external dependency.

The .NET runtime and NuGet packages used by this project are listed in
`Directory.Packages.props` and their licenses apply.
