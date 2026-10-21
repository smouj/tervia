# Samsung Tizen

Local control requires pairing a token with the television: the TV shows a
prompt the user must accept. Developer features additionally require Developer
Mode, the development PC IP registered on the TV and a signed certificate.

Tervia therefore reports:

- `remote.navigation` — restricted until the pairing token exists
- `apps.list` — restricted to Developer Mode
- `developer.shell` — unavailable

Tervia never disables signature checks and never claims that production apps can
be removed arbitrarily.
