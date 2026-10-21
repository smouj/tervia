using Tervia.Capabilities;
using Tervia.Core;
using Tervia.Safety;

namespace Tervia.Application;

/// <summary>Coordinates adapter resolution, safety checks and session creation.</summary>
public sealed class TvSessionService
{
    private readonly PlatformAdapterRegistry _registry;
    private readonly TvSafetyEngine _safety;

    public TvSessionService(PlatformAdapterRegistry registry, TvSafetyEngine safety)
    {
        _registry = registry;
        _safety = safety;
    }

    public async Task<TvSession> OpenAsync(TvDevice device, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(device);
        var adapter = _registry.Require(device.Platform);
        var session = await adapter.ConnectAsync(device, cancellationToken).ConfigureAwait(false);
        try
        {
            var capabilities = await adapter.ProbeCapabilitiesAsync(device, cancellationToken).ConfigureAwait(false);
            return new TvSession(device, adapter, capabilities) { Connected = true };
        }
        catch
        {
            try
            {
                await adapter.DisconnectAsync(session, CancellationToken.None).ConfigureAwait(false);
            }
            catch
            {
                // Keep the probe error as the actionable failure.
            }

            throw;
        }
    }

    public SafetyDecision Authorize(SafetyRequest request) => _safety.Evaluate(request);
}
