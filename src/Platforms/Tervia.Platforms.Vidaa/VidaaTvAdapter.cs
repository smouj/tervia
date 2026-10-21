using Tervia.Application;
using Tervia.Capabilities;
using Tervia.Core;

namespace Tervia.Platforms.Vidaa;

/// <summary>
/// VIDAA adapter. Local control is undocumented; every capability stays
/// Unknown until it is verified on real hardware.
/// </summary>
public sealed class VidaaTvAdapter : ITvPlatformAdapter
{
    public TvPlatform Platform => TvPlatform.Vidaa;

    public Task<TvDevice> IdentifyAsync(TvCandidate candidate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        return Task.FromResult(new TvDevice
        {
            Id = candidate.Address,
            FriendlyName = candidate.Name,
            NetworkAddress = candidate.Address,
            Platform = TvPlatform.Vidaa,
            Manufacturer = candidate.ManufacturerHint ?? "Hisense",
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
            CapabilityInfo.Unknown(CapabilityId.RemoteNavigation, "VIDAA local control is not documented."),
            CapabilityInfo.Unknown(CapabilityId.AppsList, "VIDAA local application control is not documented."),
            CapabilityInfo.Unknown(CapabilityId.DiagnosticsSystemInfo, "No verified diagnostic endpoint."),
        ]));
    }
}
