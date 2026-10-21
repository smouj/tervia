namespace Tervia.Core;

/// <summary>A single, in-memory screenshot captured from a television.</summary>
public sealed record TvScreenshot
{
    public required ReadOnlyMemory<byte> PngData { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required DateTimeOffset CapturedAt { get; init; }
}
