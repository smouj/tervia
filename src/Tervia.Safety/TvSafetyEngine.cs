using Tervia.Core;

namespace Tervia.Safety;

/// <summary>
/// Central gate for potentially destructive operations. Unknown risk is never
/// treated as safe: it requires explicit confirmation.
/// </summary>
public sealed class TvSafetyEngine
{
    private readonly Dictionary<TvPlatform, ITvSafetyPolicy> _policies;

    public TvSafetyEngine(IEnumerable<ITvSafetyPolicy> policies)
    {
        ArgumentNullException.ThrowIfNull(policies);
        _policies = policies.ToDictionary(p => p.Platform);
    }

    public SafetyDecision Evaluate(SafetyRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (_policies.TryGetValue(request.Platform, out var policy))
        {
            return policy.Evaluate(request);
        }

        return DefaultPolicy.Instance.Evaluate(request);
    }
}
