# Recovery

Snapshots are capability driven. They are not full system images.

```text
TvSnapshot
  TvId, Platform, CapturedAt
  Properties[]: Name, OriginalValue, CurrentValue, CanRestore
  OperationHistory[]
```

Restore is diff based: only properties captured by Tervia and marked
`CanRestore` are applied. A property that changed outside Tervia is reported, not
silently overwritten.

Rules:

- capture the previous value before a destructive change
- if the TV disconnects mid-operation, mark the transaction interrupted and
  re-inspect the real state before retrying
- never claim a restore that the platform cannot perform
