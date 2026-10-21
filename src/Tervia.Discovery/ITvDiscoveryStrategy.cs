using Tervia.Core;

namespace Tervia.Discovery;

public interface ITvDiscoveryStrategy
{
    string Name { get; }
    Task<IReadOnlyList<TvCandidate>> DiscoverAsync(CancellationToken cancellationToken);
}
