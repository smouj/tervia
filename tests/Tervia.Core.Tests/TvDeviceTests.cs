using Tervia.Core;
using Xunit;

namespace Tervia.Core.Tests;

public sealed class TvDeviceTests
{
    [Fact]
    public void Device_keeps_identity_and_platform()
    {
        var device = new TvDevice
        {
            Id = "192.168.1.10",
            FriendlyName = "Living Room",
            NetworkAddress = "192.168.1.10",
            Platform = TvPlatform.Tizen,
            Manufacturer = "Samsung",
        };

        Assert.Equal("192.168.1.10", device.Id);
        Assert.Equal(TvPlatform.Tizen, device.Platform);
        Assert.Equal(TvDeviceStatus.Unknown, device.Status);
    }

    [Fact]
    public void Unsupported_capability_error_names_the_capability()
    {
        var error = new UnsupportedCapabilityException("apps.install");
        Assert.Contains("apps.install", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Result_carries_value_or_error()
    {
        var ok = TvResult<int>.Ok(3);
        var fail = TvResult<int>.Fail("nope");

        Assert.True(ok.IsSuccess);
        Assert.Equal(3, ok.Value);
        Assert.False(fail.IsSuccess);
        Assert.Equal("nope", fail.Error);
    }
}
