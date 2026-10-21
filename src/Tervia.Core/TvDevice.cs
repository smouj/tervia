using Tervia.Capabilities;

namespace Tervia.Core;

public enum TvConnectionMethod
{
    Unknown = 0,
    Network,
    Usb,
    VendorBridge,
}

public enum TvDeviceStatus
{
    Unknown = 0,
    Online,
    Offline,
    Sleeping,
    Error,
}

/// <summary>A physical television known to Tervia.</summary>
public sealed record TvDevice
{
    public required string Id { get; init; }
    public required string FriendlyName { get; init; }
    public required string NetworkAddress { get; init; }
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }
    public TvPlatform Platform { get; init; } = TvPlatform.Unknown;
    public string? PlatformVersion { get; init; }
    public string? FirmwareVersion { get; init; }
    public string? SerialNumber { get; init; }
    public int? ScreenSizeInches { get; init; }
    public string? Resolution { get; init; }
    public TvConnectionMethod ConnectionMethod { get; init; } = TvConnectionMethod.Network;
    public DateTimeOffset? LastSeen { get; init; }
    public string? Room { get; init; }
    public TvDeviceStatus Status { get; init; } = TvDeviceStatus.Unknown;
    public CapabilitySet Capabilities { get; init; } = CapabilitySet.Empty;
}
