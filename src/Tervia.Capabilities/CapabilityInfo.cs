namespace Tervia.Capabilities;

public sealed record CapabilityInfo
{
    public required CapabilityId Id { get; init; }
    public required CapabilityStatus Status { get; init; }
    public string? Reason { get; init; }
    public IReadOnlyList<string> Requirements { get; init; } = [];
    public RiskLevel Risk { get; init; } = RiskLevel.Unknown;
    public string? Source { get; init; }
    public CapabilityConfidence Confidence { get; init; } = CapabilityConfidence.Unknown;

    public bool IsUsable => Status == CapabilityStatus.Supported;

    public static CapabilityInfo Supported(CapabilityId id, RiskLevel risk = RiskLevel.Safe, string? source = null)
        => new() { Id = id, Status = CapabilityStatus.Supported, Risk = risk, Source = source };

    public static CapabilityInfo Restricted(CapabilityId id, string reason, RiskLevel risk = RiskLevel.Medium)
        => new() { Id = id, Status = CapabilityStatus.Restricted, Reason = reason, Risk = risk };

    public static CapabilityInfo Unavailable(CapabilityId id, string reason)
        => new() { Id = id, Status = CapabilityStatus.Unavailable, Reason = reason, Risk = RiskLevel.Safe };

    public static CapabilityInfo Unknown(CapabilityId id, string reason)
        => new() { Id = id, Status = CapabilityStatus.Unknown, Reason = reason, Risk = RiskLevel.Unknown };
}
