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
        => (await DiscoverDetailedAsync(cancellationToken).ConfigureAwait(false)).Candidates;

    public async Task<TvDiscoveryReport> DiscoverDetailedAsync(CancellationToken cancellationToken)
    {
        var allCandidates = new List<TvCandidate>();
        var failures = new List<TvDiscoveryFailure>();

        foreach (var strategy in _strategies)
        {
            try
            {
                var strategyCandidates = await strategy.DiscoverAsync(cancellationToken).ConfigureAwait(false);
                allCandidates.AddRange(strategyCandidates);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                failures.Add(new TvDiscoveryFailure(strategy.Name, exception.Message));
            }
        }

        var candidates = allCandidates
            .GroupBy(c => Normalize(c.Address), StringComparer.OrdinalIgnoreCase)
            .Select(g => g.OrderByDescending(c => c.Confidence).First())
            .OrderByDescending(c => c.Confidence)
            .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new TvDiscoveryReport(candidates, failures);
    }

    private static string Normalize(string address) => address.Trim().ToLowerInvariant();
}
