# Android TV / Google TV / Fire TV

Transport: ADB over the network (or USB, selected by the user).

Implemented:

- device identification from the discovered endpoint
- key events for navigation, home, back, volume
- application inventory (`pm list packages -3`)
- application launch (`monkey -p <id> ...`)
- force stop (`am force-stop`)
- diagnostics from system properties
- structured argument construction, no shell concatenation

Restricted:

- APK installation requires developer options and an authorised ADB key
- package disable/uninstall goes through the safety engine and is never offered
  as a bulk "debloat" button

Fire TV televisions share the transport but have their own package
classification (`FireTvPlatformProfile`).
