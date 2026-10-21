namespace Tervia.Core;

/// <summary>A television discovered on the network but not yet identified.</summary>
public sealed record TvCandidate
{
    public required string Name { get; init; }
    public required string Address { get; init; }
    public required string DiscoveryProtocol { get; init; }
    public string? ManufacturerHint { get; init; }
    public string? ModelHint { get; init; }
    public TvPlatform PlatformHint { get; init; } = TvPlatform.Unknown;
    public double Confidence { get; init; }
}
