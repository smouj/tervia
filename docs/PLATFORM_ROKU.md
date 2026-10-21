# Roku TV

Roku exposes the External Control Protocol on port 8060 over the local network
and advertises itself through SSDP.

Implemented:

- SSDP discovery and platform detection
- keypress (`/keypress/<key>`)
- application list (`/query/apps`) and launch (`/launch/<id>`)
- device information (`/query/device-info`) as diagnostics

Requirements: **Control by mobile apps** enabled on the TV. Developer functions
additionally need Developer Mode. Roku players and sticks are out of scope.
