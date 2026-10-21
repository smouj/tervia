using Tervia.Application;
using Tervia.Capabilities;
using Tervia.Core;
using Tervia.Safety;
using Tervia.Testing.Mocks;
using Xunit;

namespace Tervia.IntegrationTests;

public sealed class SessionTests
{
    [Fact]
    public void Registry_resolves_registered_platform()
    {
        var registry = new PlatformAdapterRegistry();
        registry.Register(MockTvFactory.CreateRokuTv());

        Assert.NotNull(registry.Resolve(TvPlatform.RokuTV));
        Assert.Null(registry.Resolve(TvPlatform.Tizen));
    }

    [Fact]
    public async Task Session_exposes_probed_capabilities()
    {
        var mock = MockTvFactory.CreateRokuTv();
        var registry = new PlatformAdapterRegistry();
        registry.Register(mock);
        var service = new TvSessionService(registry, new TvSafetyEngine([]));

        var device = new TvDevice
        {
            Id = "roku",
            FriendlyName = "Roku TV",
            NetworkAddress = "192.168.1.60",
            Platform = TvPlatform.RokuTV,
        };

        var session = await service.OpenAsync(device, CancellationToken.None);

        Assert.True(session.Capabilities.Supports(CapabilityId.RemoteNavigation));
        Assert.True(session.Capabilities.Supports(CapabilityId.AppsLaunch));
        Assert.False(session.Capabilities.Supports(CapabilityId.DeveloperScreenshot));
    }

    [Fact]
    public async Task Unsupported_capability_throws_typed_error()
    {
        var mock = MockTvFactory.CreateSamsungTizenTv();
        var session = await mock.ConnectAsync(new TvDevice
        {
            Id = "samsung",
            FriendlyName = "Samsung",
            NetworkAddress = "192.168.1.61",
            Platform = TvPlatform.Tizen,
        }, CancellationToken.None);

        await Assert.ThrowsAsync<UnsupportedCapabilityException>(
            () => mock.PressAsync(session, TvRemoteKey.Home, CancellationToken.None));
    }

    [Fact]
    public async Task Operation_queue_serialises_the_same_television()
    {
        using var queue = new TvOperationQueue();
        var order = new List<int>();

        var first = queue.ExecuteAsync("tv", async _ =>
        {
            await Task.Delay(30);
            order.Add(1);
            return 1;
        }, CancellationToken.None);

        var second = queue.ExecuteAsync("tv", _ =>
        {
            order.Add(2);
            return Task.FromResult(2);
        }, CancellationToken.None);

        await Task.WhenAll(first, second);

        Assert.Equal(new[] { 1, 2 }, order);
    }

    [Fact]
    public void Safety_engine_defaults_to_confirmation_for_unknown_platform()
    {
        var engine = new TvSafetyEngine([]);
        var decision = engine.Evaluate(new SafetyRequest
        {
            OperationId = "op",
            Risk = RiskLevel.Medium,
            TvId = "tv",
            Platform = TvPlatform.Unknown,
        });

        Assert.Equal(SafetyOutcome.RequiresConfirmation, decision.Outcome);
    }
}
