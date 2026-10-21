using Tervia.Capabilities;
using Tervia.Core;

namespace Tervia.Safety;

public sealed record SafetyRequest
{
    public required string OperationId { get; init; }
    public required RiskLevel Risk { get; init; }
    public required string TvId { get; init; }
    public required TvPlatform Platform { get; init; }
    public string? Detail { get; init; }
}

public enum SafetyOutcome
{
    Allowed = 0,
    RequiresConfirmation,
    Denied,
}

public sealed record SafetyDecision(SafetyOutcome Outcome, string Reason)
{
    public static SafetyDecision Allow(string reason) => new(SafetyOutcome.Allowed, reason);
    public static SafetyDecision Confirm(string reason) => new(SafetyOutcome.RequiresConfirmation, reason);
    public static SafetyDecision Deny(string reason) => new(SafetyOutcome.Denied, reason);
}

public interface ITvSafetyPolicy
{
    TvPlatform Platform { get; }
    SafetyDecision Evaluate(SafetyRequest request);
}
