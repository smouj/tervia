# LG webOS

Current LG tooling is the `webOS CLI` (the old `webOS TV CLI` is retired).
Developer features require the Developer Mode application and a key issued by LG.

Tervia reports:

- `remote.navigation` — restricted until the TV prompt is accepted
- `apps.list` — restricted to a Developer Mode session
- `apps.install` — development applications only, signed with the developer key

Tervia never generates, stores or bypasses LG developer keys. An expired key is
surfaced as an explicit state.
