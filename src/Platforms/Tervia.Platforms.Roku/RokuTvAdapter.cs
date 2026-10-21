using System.Xml.Linq;
using Tervia.Application;
using Tervia.Capabilities;
using Tervia.Core;

namespace Tervia.Platforms.Roku;

public sealed class RokuTvAdapter : ITvPlatformAdapter, ITvRemoteControl, ITvApplicationManager, ITvDiagnosticsProvider
{
    private static readonly Dictionary<TvRemoteKey, string> Keys = new()
    {
        [TvRemoteKey.Up] = "Up",
        [TvRemoteKey.Down] = "Down",
        [TvRemoteKey.Left] = "Left",
        [TvRemoteKey.Right] = "Right",
        [TvRemoteKey.Ok] = "Select",
        [TvRemoteKey.Back] = "Back",
        [TvRemoteKey.Home] = "Home",
        [TvRemoteKey.PlayPause] = "Play",
        [TvRemoteKey.VolumeUp] = "VolumeUp",
        [TvRemoteKey.VolumeDown] = "VolumeDown",
        [TvRemoteKey.Mute] = "VolumeMute",
    };

    private readonly Func<string, RokuEcpClient> _clientFactory;

    public RokuTvAdapter(Func<string, RokuEcpClient>? clientFactory = null)
        => _clientFactory = clientFactory ?? (host => new RokuEcpClient(host));

    public TvPlatform Platform => TvPlatform.RokuTV;

    public Task<TvDevice> IdentifyAsync(TvCandidate candidate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        return Task.FromResult(new TvDevice
        {
            Id = candidate.Address,
            FriendlyName = candidate.Name,
            NetworkAddress = candidate.Address,
            Platform = TvPlatform.RokuTV,
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
            CapabilityInfo.Supported(CapabilityId.RemoteNavigation, RiskLevel.Safe, "roku.ecp"),
            CapabilityInfo.Supported(CapabilityId.RemoteHome, RiskLevel.Safe, "roku.ecp"),
            CapabilityInfo.Supported(CapabilityId.RemoteBack, RiskLevel.Safe, "roku.ecp"),
            CapabilityInfo.Supported(CapabilityId.AppsList, RiskLevel.Safe, "roku.ecp"),
            CapabilityInfo.Supported(CapabilityId.AppsLaunch, RiskLevel.Low, "roku.ecp"),
            CapabilityInfo.Supported(CapabilityId.DiagnosticsSystemInfo, RiskLevel.Safe, "roku.ecp"),
            CapabilityInfo.Restricted(CapabilityId.AppsInstall, "requires Roku Developer Mode", RiskLevel.High),
            CapabilityInfo.Unavailable(CapabilityId.DeveloperScreenshot, "Roku ECP does not expose screenshots"),
        ]));
    }

    public async Task PressAsync(TvSession session, TvRemoteKey key, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (!Keys.TryGetValue(key, out var rokuKey))
        {
            throw new UnsupportedCapabilityException($"remote.{key}");
        }

        using var client = _clientFactory(session.Device.NetworkAddress);
        await client.KeyPressAsync(rokuKey, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<TvApplication>> ListApplicationsAsync(TvSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        using var client = _clientFactory(session.Device.NetworkAddress);
        var xml = await client.GetApplicationsAsync(cancellationToken).ConfigureAwait(false);
        var document = XDocument.Parse(xml);

        return document.Root?
            .Elements("app")
            .Select(element => new TvApplication
            {
                Id = (string?)element.Attribute("id") ?? string.Empty,
                Name = element.Value.Trim(),
                Version = (string?)element.Attribute("version"),
                Kind = TvApplicationKind.User,
            })
            .Where(app => app.Id.Length > 0)
            .ToList() ?? [];
    }

    public async Task LaunchApplicationAsync(TvSession session, string applicationId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        using var client = _clientFactory(session.Device.NetworkAddress);
        await client.LaunchAsync(applicationId, cancellationToken).ConfigureAwait(false);
    }

    public Task StopApplicationAsync(TvSession session, string applicationId, CancellationToken cancellationToken)
        => throw new UnsupportedCapabilityException(CapabilityId.AppsStop.Value);

    public async Task<IReadOnlyDictionary<string, string>> ReadDiagnosticsAsync(TvSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        using var client = _clientFactory(session.Device.NetworkAddress);
        var xml = await client.GetDeviceInfoAsync(cancellationToken).ConfigureAwait(false);
        var document = XDocument.Parse(xml);
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (document.Root is not null)
        {
            foreach (var element in document.Root.Elements())
            {
                values[element.Name.LocalName] = element.Value.Trim();
            }
        }

        return values;
    }
}
