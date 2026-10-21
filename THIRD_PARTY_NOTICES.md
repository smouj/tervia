# Third-party notices

The Windows release contains Tervia's own self-contained app and Tervia Home APK. It
does not bundle vendor binaries such as Android Platform-Tools.

External tools used at runtime, when the user has installed them:

| Tool | Vendor | License / terms | Bundled |
| --- | --- | --- | --- |
| Android platform tools (`adb`) | Google | Android SDK terms | No |
| Tizen CLI / sdb | Samsung | Samsung developer terms | No |
| webOS CLI | LG | LG developer terms | No |

Tervia resolves Android Platform-Tools in the standard Android SDK locations or `PATH`
and tells the user what is missing. Redistribution is never assumed; if a license does
not clearly allow bundling, a tool stays an external dependency.

The Gradle Wrapper JAR in the Android source tree is distributed under the Apache
License 2.0. Android Gradle Plugin and Android build tools are fetched from their
upstream package repositories during a build and are not included in the source archive.

The .NET runtime and NuGet packages used by this project are listed in
`Directory.Packages.props` and their licenses apply.
