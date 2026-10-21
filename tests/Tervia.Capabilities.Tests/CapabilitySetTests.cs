using Tervia.Capabilities;
using Xunit;

namespace Tervia.Capabilities.Tests;

public sealed class CapabilitySetTests
{
    [Fact]
    public void Empty_set_supports_nothing()
    {
        Assert.False(CapabilitySet.Empty.Supports(CapabilityId.RemoteNavigation));
        Assert.Null(CapabilitySet.Empty.Find(CapabilityId.RemoteNavigation));
    }

    [Fact]
    public void Supported_capability_is_usable()
    {
        var set = CapabilitySet.From([CapabilityInfo.Supported(CapabilityId.RemoteNavigation)]);

        Assert.True(set.Supports(CapabilityId.RemoteNavigation));
        Assert.Single(set.All);
    }

    [Theory]
    [InlineData(CapabilityStatus.Restricted)]
    [InlineData(CapabilityStatus.Unavailable)]
    [InlineData(CapabilityStatus.Unknown)]
    public void Non_supported_statuses_are_not_usable(CapabilityStatus status)
    {
        var info = new CapabilityInfo { Id = CapabilityId.AppsLaunch, Status = status, Reason = "not available" };
        var set = CapabilitySet.From([info]);

        Assert.False(set.Supports(CapabilityId.AppsLaunch));
        Assert.Equal("not available", set.Find(CapabilityId.AppsLaunch)!.Reason);
    }

    [Fact]
    public void With_replaces_existing_capability()
    {
        var set = CapabilitySet.From([CapabilityInfo.Unavailable(CapabilityId.AppsList, "no")]);
        var updated = set.With(CapabilityInfo.Supported(CapabilityId.AppsList));

        Assert.True(updated.Supports(CapabilityId.AppsList));
        Assert.False(set.Supports(CapabilityId.AppsList));
    }
}
