namespace Tervia.Core;

public enum TvApplicationKind
{
    Unknown = 0,
    User,
    System,
    Development,
}

public enum TvApplicationState
{
    Unknown = 0,
    Stopped,
    Running,
}

public sealed record TvApplication
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Version { get; init; }
    public TvApplicationState State { get; init; } = TvApplicationState.Unknown;
    public bool Installed { get; init; } = true;
    public bool IsSystem { get; init; }
    public TvApplicationKind Kind { get; init; } = TvApplicationKind.Unknown;
}
