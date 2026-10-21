using Tervia.Application;
using Tervia.Capabilities;
using Tervia.Core;

namespace Tervia.Platforms.Tizen;

/// <summary>
/// Samsung Tizen adapter. Local control requires a paired token and developer
/// features require Developer Mode with a signed certificate, so nothing is
/// advertised as supported until those flows exist.
/// </summary>
public sealed class TizenTvAdapter : ITvPlatformAdapter
{
    public TvPlatform Platform => TvPlatform.Tizen;

    public Task<TvDevice> IdentifyAsync(TvCandidate candidate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        return Task.FromResult(new TvDevice
        {
            Id = candidate.Address,
            FriendlyName = candidate.Name,
            NetworkAddress = candidate.Address,
            Platform = TvPlatform.Tizen,
            Manufacturer = candidate.ManufacturerHint ?? "Samsung",
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
                "Samsung local control requires pairing a token with the TV.",
                RiskLevel.Low),
            CapabilityInfo.Restricted(
                CapabilityId.AppsList,
                "Requires Samsung Developer Mode, the PC IP registered on the TV and a signed certificate.",
                RiskLevel.Medium),
            CapabilityInfo.Unknown(
                CapabilityId.DiagnosticsSystemInfo,
                "No documented local diagnostics endpoint verified for this firmware."),
            CapabilityInfo.Unavailable(
                CapabilityId.DeveloperShell,
                "Tizen televisions do not expose a general purpose shell."),
        ]));
    }
}
