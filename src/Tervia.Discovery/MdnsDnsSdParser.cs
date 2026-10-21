using System.Buffers.Binary;
using System.Net;
using System.Text;

namespace Tervia.Discovery;

/// <summary>One DNS-SD instance assembled from records in a single mDNS response.</summary>
public sealed record MdnsServiceObservation(
    string ServiceType,
    string InstanceName,
    string HostName,
    ushort Port,
    IReadOnlyDictionary<string, string> Text,
    IReadOnlyList<IPAddress> Addresses);

/// <summary>Parses the PTR/SRV/TXT/A records needed for one-shot DNS-SD discovery.</summary>
public static class MdnsDnsSdParser
{
    private const int MaximumPacketBytes = 9000;
    private const int MaximumRecords = 512;

    public static IReadOnlyList<MdnsServiceObservation> Parse(ReadOnlySpan<byte> message)
    {
        if (message.Length is < 12 or > MaximumPacketBytes)
        {
            return [];
        }

        try
        {
            return ParseCore(message.ToArray());
        }
        catch (InvalidDataException)
        {
            return [];
        }
        catch (ArgumentException)
        {
            return [];
        }
    }

    private static List<MdnsServiceObservation> ParseCore(byte[] packet)
    {
        var reader = new PacketReader(packet);
        var flags = reader.ReadUInt16(2);
        if ((flags & 0x8000) == 0) // Ignore queries and announcements without a response bit.
        {
            return [];
        }

        var questionCount = reader.ReadUInt16(4);
        var recordCount = reader.ReadUInt16(6) + reader.ReadUInt16(8) + reader.ReadUInt16(10);
        if (recordCount > MaximumRecords)
        {
            return [];
        }

        var offset = 12;
        for (var index = 0; index < questionCount; index++)
        {
            _ = reader.ReadName(ref offset);
            reader.Skip(ref offset, 4);
        }

        var pointers = new List<(string Service, string Instance)>();
        var services = new Dictionary<string, (string Host, ushort Port)>(StringComparer.OrdinalIgnoreCase);
        var texts = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        var addresses = new Dictionary<string, List<IPAddress>>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < recordCount; index++)
        {
            var owner = reader.ReadName(ref offset);
            var type = reader.ReadUInt16(ref offset);
            _ = reader.ReadUInt16(ref offset); // class, including the cache-flush bit
            _ = reader.ReadUInt32(ref offset); // TTL
            var dataLength = reader.ReadUInt16(ref offset);
            var dataOffset = offset;
            var dataEnd = checked(dataOffset + dataLength);
            if (dataEnd > packet.Length)
            {
                throw new InvalidDataException("A DNS record extends past the packet boundary.");
            }

            switch (type)
            {
                case 12: // PTR
                {
                    var targetOffset = dataOffset;
                    var target = reader.ReadName(ref targetOffset);
                    pointers.Add((owner, target));
                    break;
                }
                case 33 when dataLength >= 7: // SRV
                {
                    var port = reader.ReadUInt16(dataOffset + 4);
                    var targetOffset = dataOffset + 6;
                    var host = reader.ReadName(ref targetOffset);
                    services[Normalize(owner)] = (host, port);
                    break;
                }
                case 16: // TXT
                    texts[Normalize(owner)] = reader.ReadTextStrings(dataOffset, dataEnd);
                    break;
                case 1 when dataLength == 4: // A
                    AddAddress(addresses, owner, new IPAddress(packet.AsSpan(dataOffset, 4)));
                    break;
                case 28 when dataLength == 16: // AAAA
                    AddAddress(addresses, owner, new IPAddress(packet.AsSpan(dataOffset, 16)));
                    break;
            }

            offset = dataEnd;
        }

        var observations = new List<MdnsServiceObservation>();
        foreach (var (service, instance) in pointers)
        {
            if (!services.TryGetValue(Normalize(instance), out var srv))
            {
                continue;
            }

            texts.TryGetValue(Normalize(instance), out var txt);
            addresses.TryGetValue(Normalize(srv.Host), out var hostAddresses);
            observations.Add(new MdnsServiceObservation(
                Normalize(service),
                Normalize(instance),
                Normalize(srv.Host),
                srv.Port,
                txt ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                hostAddresses?.ToList() ?? []));
        }

        return observations;
    }

    private static void AddAddress(Dictionary<string, List<IPAddress>> addresses, string host, IPAddress address)
    {
        var key = Normalize(host);
        if (!addresses.TryGetValue(key, out var values))
        {
            values = [];
            addresses.Add(key, values);
        }
        values.Add(address);
    }

    private static string Normalize(string name) => name.TrimEnd('.');

    private sealed class PacketReader(byte[] packet)
    {
        public ushort ReadUInt16(int offset)
        {
            EnsureRange(offset, 2);
            return BinaryPrimitives.ReadUInt16BigEndian(packet.AsSpan(offset, 2));
        }

        public uint ReadUInt32(ref int offset)
        {
            var value = ReadUInt32(offset);
            offset += 4;
            return value;
        }

        public uint ReadUInt32(int offset)
        {
            EnsureRange(offset, 4);
            return BinaryPrimitives.ReadUInt32BigEndian(packet.AsSpan(offset, 4));
        }

        public ushort ReadUInt16(ref int offset)
        {
            var value = ReadUInt16(offset);
            offset += 2;
            return value;
        }

        public void Skip(ref int offset, int count)
        {
            EnsureRange(offset, count);
            offset += count;
        }

        public string ReadName(ref int offset)
        {
            var labels = new List<string>();
            var position = offset;
            var resumePosition = -1;
            var visited = new HashSet<int>();
            var wireLength = 0;

            while (true)
            {
                EnsureRange(position, 1);
                var length = packet[position];
                if (length == 0)
                {
                    position++;
                    offset = resumePosition < 0 ? position : resumePosition;
                    return string.Join('.', labels);
                }

                if ((length & 0xC0) == 0xC0)
                {
                    EnsureRange(position, 2);
                    var pointer = ((length & 0x3F) << 8) | packet[position + 1];
                    if (pointer >= packet.Length || !visited.Add(pointer) || visited.Count > 32)
                    {
                        throw new InvalidDataException("A DNS name contains an invalid compression pointer.");
                    }

                    resumePosition = resumePosition < 0 ? position + 2 : resumePosition;
                    position = pointer;
                    continue;
                }

                if ((length & 0xC0) != 0 || length > 63)
                {
                    throw new InvalidDataException("A DNS label has an invalid length.");
                }

                EnsureRange(position + 1, length);
                wireLength += length + 1;
                if (wireLength > 255)
                {
                    throw new InvalidDataException("A DNS name is too long.");
                }

                labels.Add(Encoding.UTF8.GetString(packet, position + 1, length));
                position += length + 1;
            }
        }

        public Dictionary<string, string> ReadTextStrings(int offset, int end)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            while (offset < end)
            {
                var length = packet[offset++];
                if (offset + length > end)
                {
                    throw new InvalidDataException("A DNS TXT string extends past its record.");
                }

                if (length == 0)
                {
                    continue;
                }

                var item = Encoding.UTF8.GetString(packet, offset, length);
                var separator = item.IndexOf('=');
                var key = (separator < 0 ? item : item[..separator]).Trim();
                if (key.Length > 0)
                {
                    values[key] = separator < 0 ? string.Empty : item[(separator + 1)..];
                }
                offset += length;
            }
            return values;
        }

        private void EnsureRange(int offset, int count)
        {
            if (offset < 0 || count < 0 || offset > packet.Length - count)
            {
                throw new InvalidDataException("The DNS packet ended unexpectedly.");
            }
        }
    }
}
