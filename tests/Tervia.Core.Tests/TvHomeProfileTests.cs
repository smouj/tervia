using Tervia.Core;
using Xunit;

namespace Tervia.Core.Tests;

public sealed class TvHomeProfileTests
{
    private static readonly string[] ExpectedPinned = ["com.example.video", "com.example.other"];
    private static readonly string[] ExpectedHidden = ["org.example.music"];

    [Fact]
    public void Normalize_limits_text_and_accepts_only_known_themes_and_package_ids()
    {
        var profile = new TvHomeProfile
        {
            Title = " Sala\nprincipal ",
            Subtitle = new string('x', 120),
            Theme = "not-a-theme",
            PinnedApplicationIds = ["com.example.video", "bad package", "com.example.video", "com.example.other"],
            HiddenApplicationIds = ["../system", "org.example.music"],
        }.Normalize();

        Assert.Equal("Sala principal", profile.Title);
        Assert.Equal(88, profile.Subtitle.Length);
        Assert.Equal("ocean", profile.Theme);
        Assert.Equal(ExpectedPinned, profile.PinnedApplicationIds);
        Assert.Equal(ExpectedHidden, profile.HiddenApplicationIds);
    }

    [Fact]
    public void ForDevice_uses_the_tv_name_only_for_default_title()
    {
        var tv = new TvDevice { Id = "192.168.1.20", FriendlyName = "Sala", NetworkAddress = "192.168.1.20" };

        Assert.Equal("Sala", new TvHomeProfile().ForDevice(tv).Title);
        Assert.Equal("Cine", new TvHomeProfile { Title = "Cine" }.ForDevice(tv).Title);
    }
}
