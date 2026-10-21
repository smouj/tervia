# Contributing

## Ground rules

1. **Capabilities over platforms.** New behaviour is expressed as a capability
   with an honest status; adapters implement what the vendor actually documents.
2. **Never fake support.** If a platform cannot do something, the status is
   `Unavailable` or `Unknown` and the UI explains why.
3. **Keep core clean.** `Tervia.Core` and `Tervia.Application` must not reference
   ADB, Tizen tooling, webOS CLI or the Roku ECP client.
4. **Safety review.** Any operation classified `Medium` or above needs a safety
   policy entry and a test.
5. **Tests or it did not happen.** Bug fixes include a regression test.

## Workflow

```bash
dotnet build Tervia.sln -c Release
dotnet test Tervia.sln -c Release
```

Use conventional commit subjects (`feat:`, `fix:`, `docs:`, `test:`, `chore:`).
Keep pull requests focused on one milestone or one capability.

## Adding a platform adapter

1. Add a project under `src/Platforms`.
2. Implement `ITvPlatformAdapter` and only the capability interfaces that the
   platform genuinely supports.
3. Publish a `CapabilitySet` from a probe, never from a hardcoded "everything".
4. Document the vendor requirements in `docs/PLATFORM_<NAME>.md`.
5. Add fixtures that expose *different* capability sets, not a copy of the same one.
