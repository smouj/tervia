# Security model

## Secrets

- No vendor account passwords are stored, ever.
- Pairing is not implemented in the current alpha, and no pairing tokens are stored.
- Any future pairing support must protect required tokens with Windows-protected
  storage before those flows are exposed to users.
- Remote keyboard text is not logged.

## Process execution

External tools run through `ProcessRunner`:

- arguments are passed as a list, never concatenated into a shell string
- output is captured separately for stdout and stderr
- every call has a timeout and honours cancellation
- the exit code is checked; failures surface as `PlatformToolFailureException`

## Network

- Discovery uses documented multicast protocols only.
- No service accepts inbound connections; discovery only receives replies to its local
  multicast queries.
- No telemetry, no cloud endpoint, no Tervia domain.
- An explicit Android screenshot request opens a host-side ADB connection to the saved
  address on port 5555. Tervia does not enable ADB or
  pair a device. The TV may display an authorization prompt; only authorize a TV you own
  or administer. Network ADB should be disabled again in the TV settings after use.
- Tervia does not read or copy ADB credentials. The separately installed `adb` tool can
  retain its own authorized host key under the Windows user profile; revoke that
  workstation from the TV's developer settings if it should no longer be trusted.
- A screenshot stays in process memory for preview. If `adb exec-out` does not return a
  valid PNG, Tervia uses a GUID-named temporary file in `/data/local/tmp`, pulls it to a
  GUID-named host temporary file, and attempts to delete both. The host temporary file is
  deleted in a `finally` block; remote cleanup is best effort if the connection drops.

## Destructive operations

`TvSafetyEngine` gates every state change. Critical operations (firmware, root,
bootloader, signing bypass) are denied by policy, not hidden in the UI.
