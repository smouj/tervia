using System.Net;
using System.Net.Sockets;
using System.Text;
using Tervia.Core;

namespace Tervia.Discovery;

/// <summary>Discovers Google Cast DNS-SD services on the local mDNS link.</summary>
public sealed class MdnsDiscoveryStrategy : ITvDiscoveryStrategy
{
    private const int MaximumResponseBytes = 9000;
    private static readonly IPAddress MulticastAddress = IPAddress.Parse("224.0.0.251");
    private static readonly IPEndPoint DefaultEndpoint = new(MulticastAddress, 5353);
    private const string CastServiceType = "_googlecast._tcp.local";
    private static readonly string[] KnownStreamingModels =
    [
        "chromecast",
        "nvidia shield",
        "google tv streamer",
        "fire tv stick",
        "fire tv cube",
        "roku streaming stick",
    ];

    private readonly TimeSpan _responseWindow;
    private readonly IPEndPoint _endpoint;

    public MdnsDiscoveryStrategy(TimeSpan? responseWindow = null, IPEndPoint? endpoint = null)
    {
        _responseWindow = responseWindow ?? TimeSpan.FromSeconds(2);
        if (_responseWindow <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(responseWindow), "The response window must be positive.");
        }
        _endpoint = endpoint ?? DefaultEndpoint;
    }

    public string Name => "mDNS";

    public async Task<IReadOnlyList<TvCandidate>> DiscoverAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var client = new UdpClient(AddressFamily.InterNetwork);
        client.Ttl = 255;
        client.MulticastLoopback = false;
        var query = MdnsQueryBuilder.BuildPtrQuery(CastServiceType);
        await client.SendAsync(query, _endpoint, cancellationToken).ConfigureAwait(false);

        using var timeout = new CancellationTokenSource(_responseWindow);
        using var receiveCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        var candidates = new List<TvCandidate>();

        while (true)
        {
            UdpReceiveResult response;
            try
            {
                response = await client.ReceiveAsync(receiveCancellation.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (response.Buffer.Length is < 12 or > MaximumResponseBytes)
            {
                continue;
            }

            foreach (var service in MdnsDnsSdParser.Parse(response.Buffer))
            {
                if (!service.ServiceType.Equals(CastServiceType, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                service.Text.TryGetValue("md", out var model);
                if (IsKnownStreamingAccessory(model))
                {
                    continue;
                }

                service.Text.TryGetValue("fn", out var friendlyName);
                candidates.Add(new TvCandidate
                {
                    Name = string.IsNullOrWhiteSpace(friendlyName) ? InstanceDisplayName(service) : friendlyName,
                    Address = response.RemoteEndPoint.Address.ToString(),
                    DiscoveryProtocol = "mdns",
                    ModelHint = string.IsNullOrWhiteSpace(model) ? null : model,
                    // Cast advertises a service, not the physical form factor or OS.
                    // Leave the platform unknown until the user verifies the TV.
                    PlatformHint = TvPlatform.Unknown,
                    Confidence = 0.35,
                });
            }
        }

        return candidates
            .GroupBy(candidate => $"{candidate.Address}|{candidate.Name}", StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    private static bool IsKnownStreamingAccessory(string? model)
        => !string.IsNullOrWhiteSpace(model) && KnownStreamingModels.Any(marker => model.Contains(marker, StringComparison.OrdinalIgnoreCase));

    private static string InstanceDisplayName(MdnsServiceObservation service)
    {
        var suffix = $".{service.ServiceType}";
        return service.InstanceName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
            ? service.InstanceName[..^suffix.Length]
            : service.InstanceName;
    }
}
