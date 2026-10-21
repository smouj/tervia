namespace Tervia.Capabilities;

public enum CapabilityStatus
{
    Unknown = 0,
    Supported,
    Restricted,
    Unavailable,
}

public enum RiskLevel
{
    Unknown = 0,
    Safe,
    Low,
    Medium,
    High,
    Critical,
}

public enum CapabilityConfidence
{
    Unknown = 0,
    Protocol,
    Verified,
}
