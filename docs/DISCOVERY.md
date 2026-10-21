# Discovery

Strategies implement `ITvDiscoveryStrategy` and are merged by
`TvDiscoveryService`, which deduplicates by address and platform hint.

## SSDP

`SsdpResponseParser` reads the response headers and maps evidence to a platform:

- `SERVER`/`USN` containing `roku` → Roku TV
- `tizen` or `samsung` → Tizen
- `webos` or `lge` → webOS
- `vidaa` or `hisense` → VIDAA
- `firetv` → Fire TV
- `google` → Google TV
- `android` → Android TV

Confidence is 0.8 when the platform is identified, 0.4 when only a device is seen.

## Manual

`ManualAddressStrategy` adds a television by address with confidence 0.5. This is
the fallback when a TV does not answer multicast discovery.

## Rules

- Never auto-connect to everything discovered.
- Never scan arbitrary ports.
- Respect cancellation and per-strategy timeouts; a failing strategy must not
  abort the whole scan.
