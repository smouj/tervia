using Tervia.Platforms.AndroidTV;
using Xunit;

namespace Tervia.Platforms.Tests;

public sealed class AdbCommandLineTests
{
    private static readonly string[] PackageListArguments = ["-s", "192.168.1.30:5555", "shell", "pm", "list", "packages"];
    private static readonly string[] VersionArguments = ["version"];
    private static readonly string[] GetPropertyArguments = ["-s", "abc", "shell", "getprop", "ro.product.model"];

    [Fact]
    public void Serial_is_passed_as_structured_arguments()
    {
        var args = AdbCommandLine.Shell("192.168.1.30:5555", "pm", "list", "packages");

        Assert.Equal(PackageListArguments, args);
    }

    [Fact]
    public void Empty_serial_omits_device_flag()
    {
        var args = AdbCommandLine.ForDevice(null, "version");

        Assert.Equal(VersionArguments, args);
    }

    [Fact]
    public void GetProp_targets_the_property()
    {
        var args = AdbCommandLine.GetProp("abc", "ro.product.model");

        Assert.Equal(GetPropertyArguments, args);
    }

    [Fact]
    public void Fire_tv_packages_are_identified()
    {
        Assert.True(FireTvPlatformProfile.IsAmazonPackage("com.amazon.tv.launcher"));
        Assert.False(FireTvPlatformProfile.IsAmazonPackage("com.netflix.ninja"));
    }
}
