using System.Buffers.Binary;
using System.Text;

namespace Tervia.Discovery;

/// <summary>Builds a one-shot DNS-SD PTR query with the mDNS unicast-response bit.</summary>
public static class MdnsQueryBuilder
{
    public static byte[] BuildPtrQuery(string serviceType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceType);
        var labels = serviceType.TrimEnd('.').Split('.');
        if (labels.Any(label => label.Length is 0 or > 63))
        {
            throw new ArgumentException("Service type labels must contain between 1 and 63 characters.", nameof(serviceType));
        }

        using var stream = new MemoryStream();
        Span<byte> header = stackalloc byte[12];
        header.Clear();
        BinaryPrimitives.WriteUInt16BigEndian(header[4..6], 1); // QDCOUNT
        stream.Write(header);

        foreach (var label in labels)
        {
            var bytes = Encoding.ASCII.GetBytes(label);
            stream.WriteByte((byte)bytes.Length);
            stream.Write(bytes);
        }
        stream.WriteByte(0);

        Span<byte> question = stackalloc byte[4];
        BinaryPrimitives.WriteUInt16BigEndian(question[..2], 12); // PTR
        BinaryPrimitives.WriteUInt16BigEndian(question[2..], 0x8001); // IN | QU
        stream.Write(question);
        return stream.ToArray();
    }
}
