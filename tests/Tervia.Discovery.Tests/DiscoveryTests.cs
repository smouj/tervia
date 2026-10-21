using Tervia.Core;
using Tervia.Discovery;
using Xunit;

namespace Tervia.Discovery.Tests;

public sealed class DiscoveryTests
{
    private const string RokuPayload = "HTTP/1.1 200 OK\r\nCACHE-CONTROL: max-age=3600\r\nLOCATION: http://192.168.1.50:8060/dial/dd.xml\r\nSERVER: Roku/12.5 UPnP/1.0 Roku/12.5\r\nST: roku:ecp\r\nUSN: uuid:roku:ecp:1A2B3C\r\n\r\n";

    [Fact]
    public void Roku_ssdp_response_is_detected()
    {
        var candidate = SsdpResponseParser.Parse(RokuPayload);

        Assert.NotNull(candidate);
        Assert.Equal(TvPlatform.RokuTV, candidate!.PlatformHint);
        Assert.Equal("192.168.1.50", candidate.Address);
        Assert.True(candidate.Confidence > 0.5);
    }

    [Fact]
    public void Non_ssdp_payload_is_ignored()
    {
        Assert.Null(SsdpResponseParser.Parse("<html>not ssdp</html>"));
    }

    [Fact]
    public async Task Manual_strategy_returns_one_candidate_per_address()
    {
        var strategy = new ManualAddressStrategy(["192.168.1.20", " "]);
        var candidates = await strategy.DiscoverAsync(CancellationToken.None);

        Assert.Single(candidates);
        Assert.Equal("manual", candidates[0].DiscoveryProtocol);
    }

    [Fact]
    public async Task Discovery_deduplicates_by_address_and_platform()
    {
        var service = new TvDiscoveryService(
        [
            new ManualAddressStrategy(["192.168.1.20"]),
            new ManualAddressStrategy(["192.168.1.20"]),
        ]);

        var candidates = await service.DiscoverAsync(CancellationToken.None);

        Assert.Single(candidates);
    }

    [Fact]
    public async Task Failing_strategy_does_not_break_scan()
    {
        var service = new TvDiscoveryService(
        [
            new ThrowingStrategy(),
            new ManualAddressStrategy(["192.168.1.21"]),
        ]);

        var candidates = await service.DiscoverAsync(CancellationToken.None);

        Assert.Single(candidates);
    }

    private sealed class ThrowingStrategy : ITvDiscoveryStrategy
    {
        public string Name => "boom";
        public Task<IReadOnlyList<TvCandidate>> DiscoverAsync(CancellationToken cancellationToken)
            => throw new InvalidOperationException("network down");
    }
}
