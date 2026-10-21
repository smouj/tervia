using Tervia.Core;

namespace Tervia.Discovery;

public sealed record TvDiscoveryFailure(string Strategy, string Message);

public sealed record TvDiscoveryReport(
    IReadOnlyList<TvCandidate> Candidates,
    IReadOnlyList<TvDiscoveryFailure> Failures);
