# AGENTS.md

Tervia is a local-first Windows Smart TV manager. Tervia Home, configured from the
desktop app, is a primary workflow for compatible Android-based televisions.

- Follow the milestone order in the project specification: repository, domain,
  capabilities, contracts, mocks, discovery, UI, then real adapters.
- Never claim verified support without a hardware record in
  docs/COMPATIBILITY.md.
- Android Home must be selected through the user's Android system confirmation. Never
  disable or uninstall the manufacturer's launcher; always keep a recovery route.
- Keep launcher customization bounded and local per TV. Profile transfer must not enable
  arbitrary shell commands, package removal or hidden remote actions.
- Never let a platform concept leak into Tervia.Core or Tervia.Application.
- Keep every commit green: build, tests and warnings clean.
- Do not add cloud services, telemetry, accounts or paid dependencies.
