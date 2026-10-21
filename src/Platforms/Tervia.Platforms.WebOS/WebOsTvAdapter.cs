using Tervia.Application;
using Tervia.Capabilities;
using Tervia.Core;

namespace Tervia.Platforms.WebOS;

/// <summary>
/// LG webOS adapter. Developer features need the Developer Mode app and a key
/// from LG; Tervia never generates or bypasses that key.
/// </summary>
public sealed class WebOsTvAdapter : ITvPlatformAdapter
{
    public TvPlatform Platform => TvPlatform.WebOS;

    public Task<TvDevice> IdentifyAsync(TvCandidate candidate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        return Task.FromResult(new TvDevice
        {
            Id = candidate.Address,
            FriendlyName = candidate.Name,
            NetworkAddress = candidate.Address,
            Platform = TvPlatform.WebOS,
            Manufacturer = candidate.ManufacturerHint ?? "LG",
            Status = TvDeviceStatus.Online,
            LastSeen = DateTimeOffset.UtcNow,
        });
    }

    public Task<TvSession> ConnectAsync(TvDevice device, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(device);
        return Task.FromResult(new TvSession(device, this, device.Capabilities) { Connected = true });
    }

    public Task DisconnectAsync(TvSession session, CancellationToken cancellationToken)
    {
        if (session is not null)
        {
            session.Connected = false;
        }
        return Task.CompletedTask;
    }

    public Task<CapabilitySet> ProbeCapabilitiesAsync(TvDevice device, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(device);

        return Task.FromResult(CapabilitySet.From(
        [
            CapabilityInfo.Restricted(
                CapabilityId.RemoteNavigation,
                "LG requires accepting the pairing prompt on the television.",
                RiskLevel.Low),
            CapabilityInfo.Restricted(
                CapabilityId.AppsList,
                "Requires the webOS CLI and a Developer Mode session key.",
                RiskLevel.Medium),
            CapabilityInfo.Restricted(
                CapabilityId.AppsInstall,
                "Only development applications can be installed, signed by the Developer Mode key.",
                RiskLevel.High),
            CapabilityInfo.Unknown(CapabilityId.DiagnosticsSystemInfo, "Not verified on this firmware."),
        ]));
    }
}
