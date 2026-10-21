# Security

## Reporting a vulnerability

Open a private security advisory at
https://github.com/smouj/tervia/security/advisories/new or email the maintainer
listed on the GitHub profile. Do not open a public issue for a vulnerability.

Include: affected component, reproduction steps, expected vs observed behaviour,
and the impact on a user's televisions or local network. We aim to acknowledge
within 72 hours.

## Scope

Tervia runs entirely on the user's PC and talks directly to televisions on the
local network. There is no Tervia server, no account system and no telemetry.
Security-relevant areas:

- process execution for platform tooling (ADB, vendor CLIs)
- local storage of TV profiles and pairing material
- network discovery on the local LAN
- destructive operations behind the safety engine

## Rules for the codebase

- Never concatenate untrusted input into a command line; use structured arguments.
- Never store vendor account passwords or developer-account credentials.
- Never weaken platform security (signing, DRM, certificate checks).
- Never add a network service that accepts inbound connections.
- Secrets that a platform requires are kept in Windows-protected storage.
