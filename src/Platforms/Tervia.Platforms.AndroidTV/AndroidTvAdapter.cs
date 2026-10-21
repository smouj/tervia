using Tervia.Application;
using Tervia.Capabilities;
using Tervia.Core;
using Tervia.Infrastructure;

namespace Tervia.Platforms.AndroidTV;

public sealed class AndroidTvAdapter : ITvPlatformAdapter, ITvRemoteControl, ITvApplicationManager, ITvDiagnosticsProvider
{
    private static readonly IReadOnlyDictionary<TvRemoteKey, int> KeyCodes = new Dictionary<TvRemoteKey, int>
    {
        [TvRemoteKey.Up] = 19,
        [TvRemoteKey.Down] = 20,
        [TvRemoteKey.Left] = 21,
        [TvRemoteKey.Right] = 22,
        [TvRemoteKey.Ok] = 23,
        [TvRemoteKey.Back] = 4,
        [TvRemoteKey.Home] = 3,
        [TvRemoteKey.Menu] = 82,
        [TvRemoteKey.PlayPause] = 85,
        [TvRemoteKey.VolumeUp] = 24,
        [TvRemoteKey.VolumeDown] = 25,
        [TvRemoteKey.Mute] = 164,
    };

    private readonly AdbClient _adb;

    public AndroidTvAdapter(AdbClient adb) => _adb = adb;

    public TvPlatform Platform => TvPlatform.AndroidTV;

    public Task<TvDevice> IdentifyAsync(TvCandidate candidate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        return Task.FromResult(new TvDevice
        {
            Id = candidate.Address,
            FriendlyName = candidate.Name,
            NetworkAddress = candidate.Address,
            Platform = TvPlatform.AndroidTV,
            Status = TvDeviceStatus.Online,
            LastSeen = DateTimeOffset.UtcNow,
            ConnectionMethod = TvConnectionMethod.Network,
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

    public async Task<CapabilitySet> ProbeCapabilitiesAsync(TvDevice device, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(device);

        var available = await _adb.IsAvailableAsync(cancellationToken).ConfigureAwait(false);
        var source = "adb";

        if (!available)
        {
            return CapabilitySet.From(
            [
                CapabilityInfo.Unknown(CapabilityId.RemoteNavigation, "adb is not installed"),
                CapabilityInfo.Unknown(CapabilityId.AppsList, "adb is not installed"),
                CapabilityInfo.Unknown(CapabilityId.DiagnosticsSystemInfo, "adb is not installed"),
                CapabilityInfo.Unknown(CapabilityId.DeveloperShell, "adb is not installed"),
            ]);
        }

        return CapabilitySet.From(
        [
            CapabilityInfo.Supported(CapabilityId.RemoteNavigation, RiskLevel.Safe, source),
            CapabilityInfo.Supported(CapabilityId.RemoteHome, RiskLevel.Safe, source),
            CapabilityInfo.Supported(CapabilityId.RemoteBack, RiskLevel.Safe, source),
            CapabilityInfo.Supported(CapabilityId.AppsList, RiskLevel.Safe, source),
            CapabilityInfo.Supported(CapabilityId.AppsLaunch, RiskLevel.Low, source),
            CapabilityInfo.Supported(CapabilityId.AppsStop, RiskLevel.Medium, source),
            CapabilityInfo.Supported(CapabilityId.DiagnosticsSystemInfo, RiskLevel.Safe, source),
            CapabilityInfo.Supported(CapabilityId.DeveloperScreenshot, RiskLevel.Safe, source),
            CapabilityInfo.Supported(CapabilityId.DeveloperShell, RiskLevel.High, source),
            CapabilityInfo.Supported(CapabilityId.MaintenanceListPackages, RiskLevel.Low, source),
            CapabilityInfo.Restricted(CapabilityId.AppsInstall, "requires developer options and adb authorization", RiskLevel.High),
        ]);
    }

    public async Task PressAsync(TvSession session, TvRemoteKey key, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (!session.Capabilities.Supports(CapabilityId.RemoteNavigation))
        {
            throw new UnsupportedCapabilityException(CapabilityId.RemoteNavigation.Value);
        }

        if (!KeyCodes.TryGetValue(key, out var keyCode))
        {
            throw new UnsupportedCapabilityException($"remote.{key}");
        }

        await _adb.KeyEventAsync(session.Device.NetworkAddress, keyCode, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<TvApplication>> ListApplicationsAsync(TvSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        var output = await _adb.ShellAsync(
            session.Device.NetworkAddress,
            ["pm", "list", "packages", "-3"],
            cancellationToken).ConfigureAwait(false);

        return output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Replace("package:", string.Empty, StringComparison.Ordinal).Trim())
            .Where(id => id.Length > 0)
            .Select(id => new TvApplication { Id = id, Name = id, Kind = TvApplicationKind.User })
            .ToList();
    }

    public Task LaunchApplicationAsync(TvSession session, string applicationId, CancellationToken cancellationToken)
        => RunMonkeyAsync(session, applicationId, cancellationToken);

    public Task StopApplicationAsync(TvSession session, string applicationId, CancellationToken cancellationToken)
        => _adb.ShellAsync(session.Device.NetworkAddress, ["am", "force-stop", applicationId], cancellationToken);

    public async Task<IReadOnlyDictionary<string, string>> ReadDiagnosticsAsync(TvSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        var model = await _adb.GetPropertyAsync(session.Device.NetworkAddress, "ro.product.model", cancellationToken).ConfigureAwait(false);
        var version = await _adb.GetPropertyAsync(session.Device.NetworkAddress, "ro.build.version.release", cancellationToken).ConfigureAwait(false);

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["model"] = model,
            ["androidVersion"] = version,
        };
    }

    private async Task RunMonkeyAsync(TvSession session, string applicationId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);

        var output = await _adb.ShellAsync(
            session.Device.NetworkAddress,
            ["monkey", "-p", applicationId, "-c", "android.intent.category.LAUNCHER", "1"],
            cancellationToken).ConfigureAwait(false);

        if (output.Contains("No activities found", StringComparison.OrdinalIgnoreCase))
        {
            throw new OperationRejectedException($"'{applicationId}' has no launcher activity.");
        }
    }
}
