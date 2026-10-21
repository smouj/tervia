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

## Destructive operations

`TvSafetyEngine` gates every state change. Critical operations (firmware, root,
bootloader, signing bypass) are denied by policy, not hidden in the UI.
