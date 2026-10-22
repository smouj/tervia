using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Tervia.Core;
using Tervia.Discovery;
using Xunit;

namespace Tervia.Discovery.Tests;

public sealed class NetworkDiscoveryTests
{
    [Fact]
    public async Task Ssdp_strategy_sends_search_and_uses_the_reply_sender_address()
    {
        using var responder = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        var endpoint = (IPEndPoint)responder.Client.LocalEndPoint!;
        var strategy = new SsdpDiscoveryStrategy(TimeSpan.FromMilliseconds(350), endpoint);

        var scan = strategy.DiscoverAsync(CancellationToken.None);
        var request = await responder.ReceiveAsync().WaitAsync(TimeSpan.FromSeconds(2));
        var requestText = Encoding.ASCII.GetString(request.Buffer);
        Assert.Contains("M-SEARCH * HTTP/1.1", requestText, StringComparison.Ordinal);
        Assert.Contains("ST: ssdp:all", requestText, StringComparison.Ordinal);

        var response = Encoding.ASCII.GetBytes(
            "HTTP/1.1 200 OK\r\n" +
            "LOCATION: http://203.0.113.77/device.xml\r\n" +
            "SERVER: Roku/12.5 UPnP/1.0\r\n" +
            "ST: roku:ecp\r\n\r\n");
        await responder.SendAsync(response, request.RemoteEndPoint);

        var candidate = Assert.Single(await scan);
        Assert.Equal("127.0.0.1", candidate.Address);
        Assert.Equal(TvPlatform.RokuTV, candidate.PlatformHint);
    }

    [Fact]
    public async Task Mdns_strategy_queries_google_cast_and_keeps_physical_tv_unverified()
    {
        using var responder = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        var endpoint = (IPEndPoint)responder.Client.LocalEndPoint!;
        var strategy = new MdnsDiscoveryStrategy(TimeSpan.FromSeconds(2), endpoint);

        var scan = strategy.DiscoverAsync(CancellationToken.None);
        var request = await responder.ReceiveAsync().WaitAsync(TimeSpan.FromSeconds(2));
        Assert.True(request.Buffer.Length > 12);
        Assert.Equal((ushort)1, BinaryPrimitives.ReadUInt16BigEndian(request.Buffer.AsSpan(4, 2)));
        Assert.Equal((ushort)12, BinaryPrimitives.ReadUInt16BigEndian(request.Buffer.AsSpan(request.Buffer.Length - 4, 2)));
        Assert.Equal((ushort)0x8001, BinaryPrimitives.ReadUInt16BigEndian(request.Buffer.AsSpan(request.Buffer.Length - 2, 2)));

        await responder.SendAsync(CreateGoogleCastResponse("Sony Panel"), request.RemoteEndPoint);
        var candidate = Assert.Single(await scan);
        Assert.Equal("Living Room TV", candidate.Name);
        Assert.Equal("Sony Panel", candidate.ModelHint);
        Assert.Equal("127.0.0.1", candidate.Address);
        Assert.Equal(TvPlatform.Unknown, candidate.PlatformHint);
        Assert.Equal("mdns", candidate.DiscoveryProtocol);
        Assert.Equal(0.35, candidate.Confidence);
    }

    [Fact]
    public async Task Mdns_strategy_ignores_known_external_streaming_devices()
    {
        using var responder = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        var endpoint = (IPEndPoint)responder.Client.LocalEndPoint!;
        var strategy = new MdnsDiscoveryStrategy(TimeSpan.FromSeconds(2), endpoint);

        var scan = strategy.DiscoverAsync(CancellationToken.None);
        var request = await responder.ReceiveAsync().WaitAsync(TimeSpan.FromSeconds(2));
        await responder.SendAsync(CreateGoogleCastResponse("Chromecast with Google TV"), request.RemoteEndPoint);

        Assert.Empty(await scan);
    }

    [Fact]
    public void Mdns_parser_ignores_query_packets()
    {
        var query = MdnsQueryBuilder.BuildPtrQuery("_googlecast._tcp.local");

        Assert.Empty(MdnsDnsSdParser.Parse(query));
    }

    private static byte[] CreateGoogleCastResponse(string model)
    {
        using var stream = new MemoryStream();
        Span<byte> header = stackalloc byte[12];
        header.Clear();
        BinaryPrimitives.WriteUInt16BigEndian(header[2..4], 0x8400); // response + authoritative
        BinaryPrimitives.WriteUInt16BigEndian(header[6..8], 1); // answer: PTR
        BinaryPrimitives.WriteUInt16BigEndian(header[10..12], 3); // additional: SRV, TXT, A
        stream.Write(header);

        const string serviceType = "_googlecast._tcp.local";
        const string instanceName = "Living Room TV._googlecast._tcp.local";
        const string hostName = "tervia-fixture.local";

        WriteName(stream, serviceType);
        WriteUInt16(stream, 12); // PTR
        WriteUInt16(stream, 1); // IN
        WriteUInt32(stream, 120);
        using (var rdata = new MemoryStream())
        {
            WriteName(rdata, instanceName);
            WriteRecord(stream, rdata.ToArray());
        }

        WriteName(stream, instanceName);
        WriteUInt16(stream, 33); // SRV
        WriteUInt16(stream, 1);
        WriteUInt32(stream, 120);
        using (var rdata = new MemoryStream())
        {
            WriteUInt16(rdata, 0); // priority
            WriteUInt16(rdata, 0); // weight
            WriteUInt16(rdata, 8009);
            WriteName(rdata, hostName);
            WriteRecord(stream, rdata.ToArray());
        }

        WriteName(stream, instanceName);
        WriteUInt16(stream, 16); // TXT
        WriteUInt16(stream, 1);
        WriteUInt32(stream, 120);
        using (var rdata = new MemoryStream())
        {
            WriteTxtString(rdata, "fn=Living Room TV");
            WriteTxtString(rdata, $"md={model}");
            WriteRecord(stream, rdata.ToArray());
        }

        WriteName(stream, hostName);
        WriteUInt16(stream, 1); // A
        WriteUInt16(stream, 1);
        WriteUInt32(stream, 120);
        WriteRecord(stream, [192, 168, 1, 45]);
        return stream.ToArray();
    }

    private static void WriteRecord(Stream stream, byte[] data)
    {
        WriteUInt16(stream, checked((ushort)data.Length));
        stream.Write(data);
    }

    private static void WriteTxtString(Stream stream, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        stream.WriteByte(checked((byte)bytes.Length));
        stream.Write(bytes);
    }

    private static void WriteName(Stream stream, string name)
    {
        foreach (var label in name.TrimEnd('.').Split('.'))
        {
            var bytes = Encoding.UTF8.GetBytes(label);
            stream.WriteByte(checked((byte)bytes.Length));
            stream.Write(bytes);
        }
        stream.WriteByte(0);
    }

    private static void WriteUInt16(Stream stream, ushort value)
    {
        Span<byte> bytes = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(bytes, value);
        stream.Write(bytes);
    }

    private static void WriteUInt32(Stream stream, uint value)
    {
        Span<byte> bytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, value);
        stream.Write(bytes);
    }
}
