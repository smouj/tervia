using Tervia.Application;
using Tervia.Capabilities;
using Tervia.Core;
using Tervia.Safety;
using Tervia.Testing.Mocks;
using Xunit;

namespace Tervia.IntegrationTests;

public sealed class SessionTests
{
    private static readonly int[] ExpectedOperationOrder = [1, 2];

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
    public async Task Successful_screenshot_marks_only_that_device_capability_verified()
    {
        var adapter = new ScreenshotTestAdapter();
        var registry = new PlatformAdapterRegistry();
        registry.Register(adapter);
        var sessionService = new TvSessionService(registry, new TvSafetyEngine([]));
        var screenshotService = new TvScreenshotService(sessionService);
        var device = new TvDevice
        {
            Id = "192.168.1.42",
            FriendlyName = "Living room TV",
            NetworkAddress = "192.168.1.42",
            Platform = TvPlatform.AndroidTV,
        };

        var capture = await screenshotService.CaptureAsync(device, CancellationToken.None);

        Assert.Equal(1920, capture.Screenshot.Width);
        Assert.Equal(CapabilityStatus.Supported, capture.Capabilities.Find(CapabilityId.DeveloperScreenshot)?.Status);
        Assert.Equal(CapabilityConfidence.Verified, capture.Capabilities.Find(CapabilityId.DeveloperScreenshot)?.Confidence);
        Assert.Equal(CapabilityStatus.Unknown, device.Capabilities.Find(CapabilityId.DeveloperScreenshot)?.Status ?? CapabilityStatus.Unknown);
        Assert.Equal(1, adapter.DisconnectCalls);
    }

    [Fact]
    public async Task Operation_queue_serialises_the_same_television()
    {
        using var queue = new TvOperationScheduler();
        var order = new List<int>();

        var first = queue.ExecuteAsync("tv", async _ =>
        {
            await Task.Delay(30, CancellationToken.None);
            order.Add(1);
            return 1;
        }, CancellationToken.None);

        var second = queue.ExecuteAsync("tv", _ =>
        {
            order.Add(2);
            return Task.FromResult(2);
        }, CancellationToken.None);

        await Task.WhenAll(first, second);

        Assert.Equal(ExpectedOperationOrder, order);
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

    private sealed class ScreenshotTestAdapter : ITvPlatformAdapter, ITvScreenshotProvider
    {
        public int DisconnectCalls { get; private set; }

        public TvPlatform Platform => TvPlatform.AndroidTV;

        public Task<TvDevice> IdentifyAsync(TvCandidate candidate, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<TvSession> ConnectAsync(TvDevice device, CancellationToken cancellationToken)
            => Task.FromResult(new TvSession(device, this, device.Capabilities) { Connected = true });

        public Task DisconnectAsync(TvSession session, CancellationToken cancellationToken)
        {
            DisconnectCalls++;
            session.Connected = false;
            return Task.CompletedTask;
        }

        public Task<CapabilitySet> ProbeCapabilitiesAsync(TvDevice device, CancellationToken cancellationToken)
            => Task.FromResult(CapabilitySet.From(
            [CapabilityInfo.Unknown(CapabilityId.DeveloperScreenshot, "not attempted yet")]));

        public Task<TvScreenshot> CaptureScreenshotAsync(TvSession session, CancellationToken cancellationToken)
            => Task.FromResult(new TvScreenshot
            {
                PngData = new byte[] { 137, 80, 78, 71 },
                Width = 1920,
                Height = 1080,
                CapturedAt = DateTimeOffset.UtcNow,
            });
    }
}
