# AGENTS.md

This repository is built by agents under a closed specification.

- Follow the milestone order in the project specification: repository, domain,
  capabilities, contracts, mocks, discovery, UI, then real adapters.
- Never claim verified support without a hardware record in
  docs/COMPATIBILITY.md.
- Never let a platform concept leak into Tervia.Core or Tervia.Application.
- Keep every commit green: build, tests and warnings clean.
- Do not add cloud services, telemetry, accounts or paid dependencies.
