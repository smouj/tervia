namespace Tervia.Core;

public sealed record TvSnapshotProperty
{
    public required string Name { get; init; }
    public required string OriginalValue { get; init; }
    public string? CurrentValue { get; init; }
    public bool CanRestore { get; init; }
}

public sealed record TvSnapshot
{
    public required string TvId { get; init; }
    public required TvPlatform Platform { get; init; }
    public required DateTimeOffset CapturedAt { get; init; }
    public IReadOnlyList<TvSnapshotProperty> Properties { get; init; } = [];
    public IReadOnlyList<string> OperationHistory { get; init; } = [];
}
