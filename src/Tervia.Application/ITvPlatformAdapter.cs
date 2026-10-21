using Tervia.Capabilities;
using Tervia.Core;

namespace Tervia.Application;

/// <summary>
/// Platform entry point. Deliberately small: per-feature behaviour lives in the
/// capability interfaces so an adapter only implements what the TV can do.
/// </summary>
public interface ITvPlatformAdapter
{
    TvPlatform Platform { get; }

    Task<TvDevice> IdentifyAsync(TvCandidate candidate, CancellationToken cancellationToken);

    Task<TvSession> ConnectAsync(TvDevice device, CancellationToken cancellationToken);

    Task DisconnectAsync(TvSession session, CancellationToken cancellationToken);

    Task<CapabilitySet> ProbeCapabilitiesAsync(TvDevice device, CancellationToken cancellationToken);
}
