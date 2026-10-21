# Discovery

Strategies implement `ITvDiscoveryStrategy` and are merged by
`TvDiscoveryService`, which deduplicates by address, keeps the highest-confidence
candidate and reports per-strategy failures. A failed strategy does not hide
successful results from another one.

## SSDP

`SsdpDiscoveryStrategy` sends one `M-SEARCH` request to the standard local
multicast address and listens for unicast replies for a bounded window. It uses
the datagram sender address instead of trusting a reply's `LOCATION` host. It
does not connect to candidates or scan arbitrary ports.

`SsdpResponseParser` reads the response headers and maps evidence to a platform:

- `SERVER`/`USN` containing `roku` → Roku TV
- `tizen` or `samsung` → Tizen
- `webos` or `lge` → webOS
- `vidaa` or `hisense` → VIDAA
- `firetv` → Fire TV
- `google` → Google TV
- `android` → Android TV

Confidence is 0.8 when the platform is identified, 0.4 when only a device is seen.

## mDNS

The DNS-SD strategy queries `_googlecast._tcp.local` and parses PTR, SRV, TXT,
and address records in the response. It uses the sender address for the candidate
and records the model as a hint only. Google Cast can run on streaming accessories
as well as televisions, so the platform remains unknown and the user must confirm
that a result is a physical TV before saving it. Known Chromecast, NVIDIA Shield,
Google TV Streamer, Fire TV Stick/Cube and Roku Streaming Stick models are
excluded from the result set.

## Manual

`ManualAddressStrategy` adds a television by address with confidence 0.5. This is
the fallback when a TV does not answer multicast discovery.

## Rules

- Never auto-connect to everything discovered.
- Never scan arbitrary ports.
- Respect cancellation and per-strategy timeouts; a failing strategy must not
  abort the whole scan or disappear from the detailed scan report.
