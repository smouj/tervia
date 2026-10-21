# Architecture

## Layers

```text
Tervia.App            WinUI 3 shell, view models, navigation
Tervia.Application    adapter contracts, capability interfaces, registry, queues
Tervia.Safety         risk decisions and per-platform policies
Tervia.Capabilities   capability ids, statuses, capability sets
Tervia.Core           domain model, errors, remote keys, snapshots
Tervia.Discovery      discovery strategies and SSDP parsing
Tervia.Infrastructure process execution, local storage
Platforms/*           one adapter per ecosystem
```

Dependency rule: arrows point downwards only. `Tervia.Core` and
`Tervia.Application` never reference ADB, Tizen tooling, the webOS CLI, the Roku
ECP client or any UI framework.

## Capability model

Every television receives a `CapabilitySet` produced by probing, not by a
hardcoded table. A capability carries a status (`Supported`, `Restricted`,
`Unavailable`, `Unknown`), a risk level and a reason. The UI renders only what
is usable and explains the rest.

## Sessions

`TvSession` binds a `TvDevice` to an adapter and a capability snapshot. There is
no global "current TV" singleton; the shell keeps a selected session per window.

## Safety

`TvSafetyEngine` evaluates every state changing operation before it runs. The
default policy allows `Safe`/`Low`, requires confirmation for `Medium`/`High`
and denies `Critical`. Unknown risk is never treated as safe.

## Operations

`TvOperationQueue` serialises state changing calls per television so an install,
a restore and a reboot never race.
