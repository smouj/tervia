using Tervia.Core;

namespace Tervia.Safety;

/// <summary>Risk based decision used when a platform has no specific policy.</summary>
public class DefaultPolicy : ITvSafetyPolicy
{
    public static DefaultPolicy Instance { get; } = new();

    public virtual TvPlatform Platform => TvPlatform.Unknown;

    public virtual SafetyDecision Evaluate(SafetyRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.Risk switch
        {
            Capabilities.RiskLevel.Safe or Capabilities.RiskLevel.Low => SafetyDecision.Allow("Low risk operation."),
            Capabilities.RiskLevel.Medium => SafetyDecision.Confirm("Medium risk operation changes television state."),
            Capabilities.RiskLevel.High => SafetyDecision.Confirm("High risk operation: review the exact change."),
            Capabilities.RiskLevel.Critical => SafetyDecision.Deny("Critical operations are not supported by Tervia."),
            _ => SafetyDecision.Confirm("Risk is unknown, so explicit confirmation is required."),
        };
    }
}
