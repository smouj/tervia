using System.Net;
using System.Net.Sockets;
using System.Text;
using Tervia.Core;

namespace Tervia.Discovery;

/// <summary>Sends a local SSDP M-SEARCH and listens for unicast responses.</summary>
public sealed class SsdpDiscoveryStrategy : ITvDiscoveryStrategy
{
    private const int MaximumResponseBytes = 32 * 1024;
    private static readonly IPEndPoint DefaultEndpoint = new(IPAddress.Parse("239.255.255.250"), 1900);
    private const string SearchRequest =
        "M-SEARCH * HTTP/1.1\r\n" +
        "HOST: 239.255.255.250:1900\r\n" +
        "MAN: \"ssdp:discover\"\r\n" +
        "MX: 1\r\n" +
        "ST: ssdp:all\r\n" +
        "USER-AGENT: Tervia/0.1 UPnP/1.1\r\n\r\n";

    private readonly TimeSpan _responseWindow;
    private readonly IPEndPoint _endpoint;

    public SsdpDiscoveryStrategy(TimeSpan? responseWindow = null, IPEndPoint? endpoint = null)
    {
        _responseWindow = responseWindow ?? TimeSpan.FromSeconds(2);
        if (_responseWindow <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(responseWindow), "The response window must be positive.");
        }

        _endpoint = endpoint ?? DefaultEndpoint;
    }

    public string Name => "SSDP";

    public async Task<IReadOnlyList<TvCandidate>> DiscoverAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var client = new UdpClient(AddressFamily.InterNetwork);
        var request = Encoding.ASCII.GetBytes(SearchRequest);
        await client.SendAsync(request, _endpoint, cancellationToken).ConfigureAwait(false);

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

            if (response.Buffer.Length is 0 or > MaximumResponseBytes)
            {
                continue;
            }

            var candidate = SsdpResponseParser.Parse(
                Encoding.ASCII.GetString(response.Buffer),
                response.RemoteEndPoint.Address);
            if (candidate is not null)
            {
                candidates.Add(candidate);
            }
        }

        return candidates;
    }
}
