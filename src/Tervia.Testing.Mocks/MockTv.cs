using Tervia.Application;
using Tervia.Capabilities;
using Tervia.Core;

namespace Tervia.Testing.Mocks;

/// <summary>
/// Deterministic television used by tests and by the app when no hardware is
/// present. Each mock exposes a different capability set on purpose.
/// </summary>
public sealed class MockTv : ITvPlatformAdapter, ITvRemoteControl, ITvApplicationManager, ITvDiagnosticsProvider
{
    private readonly CapabilitySet _capabilities;
    private readonly List<TvApplication> _applications;

    public MockTv(
        string name,
        TvPlatform platform,
        CapabilitySet capabilities,
        IEnumerable<TvApplication>? applications = null)
    {
        Name = name;
        Platform = platform;
        _capabilities = capabilities;
        _applications = applications?.ToList() ?? [];
    }

    public string Name { get; }
    public TvPlatform Platform { get; }
    public List<TvRemoteKey> PressedKeys { get; } = [];
    public List<string> LaunchedApps { get; } = [];

    public Task<TvDevice> IdentifyAsync(TvCandidate candidate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        return Task.FromResult(new TvDevice
        {
            Id = candidate.Address,
            FriendlyName = Name,
            NetworkAddress = candidate.Address,
            Platform = Platform,
            Status = TvDeviceStatus.Online,
            Capabilities = _capabilities,
        });
    }

    public Task<TvSession> ConnectAsync(TvDevice device, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(device);
        return Task.FromResult(new TvSession(device, this, _capabilities) { Connected = true });
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
        => Task.FromResult(_capabilities);

    public Task PressAsync(TvSession session, TvRemoteKey key, CancellationToken cancellationToken)
    {
        if (!_capabilities.Supports(CapabilityId.RemoteNavigation))
        {
            throw new UnsupportedCapabilityException(CapabilityId.RemoteNavigation.Value);
        }
        PressedKeys.Add(key);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<TvApplication>> ListApplicationsAsync(TvSession session, CancellationToken cancellationToken)
    {
        if (!_capabilities.Supports(CapabilityId.AppsList))
        {
            throw new UnsupportedCapabilityException(CapabilityId.AppsList.Value);
        }
        return Task.FromResult<IReadOnlyList<TvApplication>>(_applications);
    }

    public Task LaunchApplicationAsync(TvSession session, string applicationId, CancellationToken cancellationToken)
    {
        if (!_capabilities.Supports(CapabilityId.AppsLaunch))
        {
            throw new UnsupportedCapabilityException(CapabilityId.AppsLaunch.Value);
        }
        LaunchedApps.Add(applicationId);
        return Task.CompletedTask;
    }

    public Task StopApplicationAsync(TvSession session, string applicationId, CancellationToken cancellationToken)
        => throw new UnsupportedCapabilityException(CapabilityId.AppsStop.Value);

    public Task<IReadOnlyDictionary<string, string>> ReadDiagnosticsAsync(TvSession session, CancellationToken cancellationToken)
    {
        if (!_capabilities.Supports(CapabilityId.DiagnosticsSystemInfo))
        {
            throw new UnsupportedCapabilityException(CapabilityId.DiagnosticsSystemInfo.Value);
        }
        return Task.FromResult<IReadOnlyDictionary<string, string>>(new Dictionary<string, string>
        {
            ["model"] = Name,
            ["platform"] = Platform.ToString(),
        });
    }
}
