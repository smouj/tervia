using Tervia.Core;

namespace Tervia.Discovery;

/// <summary>Merges discovery strategies and removes duplicates by address.</summary>
public sealed class TvDiscoveryService
{
    private readonly IReadOnlyList<ITvDiscoveryStrategy> _strategies;

    public TvDiscoveryService(IEnumerable<ITvDiscoveryStrategy> strategies)
    {
        ArgumentNullException.ThrowIfNull(strategies);
        _strategies = strategies.ToList();
    }

    public async Task<IReadOnlyList<TvCandidate>> DiscoverAsync(CancellationToken cancellationToken)
    {
        var results = new List<TvCandidate>();

        foreach (var strategy in _strategies)
        {
            try
            {
                var candidates = await strategy.DiscoverAsync(cancellationToken).ConfigureAwait(false);
                results.AddRange(candidates);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                // A failing strategy must not take the whole scan down.
            }
        }

        return results
            .GroupBy(c => $"{Normalize(c.Address)}|{c.PlatformHint}", StringComparer.OrdinalIgnoreCase)
            .Select(g => g.OrderByDescending(c => c.Confidence).First())
            .OrderByDescending(c => c.Confidence)
            .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string Normalize(string address) => address.Trim().ToLowerInvariant();
}
