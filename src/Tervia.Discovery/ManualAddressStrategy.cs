using Tervia.Core;

namespace Tervia.Discovery;

/// <summary>Adds a television by IP address without scanning.</summary>
public sealed class ManualAddressStrategy : ITvDiscoveryStrategy
{
    private readonly IReadOnlyList<string> _addresses;

    public ManualAddressStrategy(IEnumerable<string> addresses)
    {
        ArgumentNullException.ThrowIfNull(addresses);
        _addresses = addresses.Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
    }

    public string Name => "manual";

    public Task<IReadOnlyList<TvCandidate>> DiscoverAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<TvCandidate> result = _addresses
            .Select(a => new TvCandidate
            {
                Name = a,
                Address = a,
                DiscoveryProtocol = "manual",
                Confidence = 0.5,
            })
            .ToList();
        return Task.FromResult(result);
    }
}
