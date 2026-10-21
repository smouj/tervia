namespace Tervia.Core;

public enum TvInputType
{
    Unknown = 0,
    Hdmi,
    Antenna,
    Composite,
    Component,
    Usb,
    Application,
}

public sealed record TvInput
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public TvInputType Type { get; init; } = TvInputType.Unknown;
    public bool Connected { get; init; }
    public bool Active { get; init; }
    public string? DeviceName { get; init; }
}
