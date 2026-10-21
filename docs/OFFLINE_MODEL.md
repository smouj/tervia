# Offline model

Tervia does not require Internet access for its own operation.

Works without Internet, provided the television allows it:

- startup and local library
- discovery on the local network
- on-demand Android TV screenshots, if ADB Platform-Tools is installed and classic
  network debugging on port 5555 is enabled and authorized on the TV

Remote control, volume, inputs, diagnostics, package management, snapshots and recovery
are not available from the current desktop UI, even where an adapter implementation
exists.

Vendor dependencies (documented, not hidden):

- Samsung Developer Mode may require the TV to be online.
- LG Developer Mode keys expire and may need the vendor service.
- Roku Developer Mode may require an account.

Those are vendor requirements. Tervia never proxies them through its own servers.
