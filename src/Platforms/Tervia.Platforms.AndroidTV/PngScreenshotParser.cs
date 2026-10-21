using System.Buffers.Binary;

namespace Tervia.Platforms.AndroidTV;

internal static class PngScreenshotParser
{
    private const int MaximumDimension = 32768;
    private static readonly uint[] CrcTable = CreateCrcTable();

    public static bool TryExtract(byte[] data, out byte[] png, out int width, out int height)
    {
        ArgumentNullException.ThrowIfNull(data);
        ReadOnlySpan<byte> signature = [137, 80, 78, 71, 13, 10, 26, 10];
        ReadOnlySpan<byte> ihdr = "IHDR"u8;
        ReadOnlySpan<byte> idat = "IDAT"u8;
        ReadOnlySpan<byte> iend = "IEND"u8;
        png = [];
        width = 0;
        height = 0;

        for (var start = 0; start <= data.Length - signature.Length; start++)
        {
            if (!data.AsSpan(start, signature.Length).SequenceEqual(signature))
            {
                continue;
            }

            var offset = start + signature.Length;
            var foundHeader = false;
            var foundImageData = false;
            while (offset <= data.Length - 12)
            {
                var chunkLength = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset, 4));
                if (chunkLength > (uint)(data.Length - offset - 12))
                {
                    break;
                }

                var type = data.AsSpan(offset + 4, 4);
                var crcOffset = offset + 8 + (int)chunkLength;
                var expectedCrc = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(crcOffset, 4));
                if (CalculateCrc(data.AsSpan(offset + 4, 4 + (int)chunkLength)) != expectedCrc)
                {
                    break;
                }

                if (!foundHeader)
                {
                    if (!type.SequenceEqual(ihdr) || chunkLength != 13)
                    {
                        break;
                    }

                    width = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(offset + 8, 4));
                    height = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(offset + 12, 4));
                    if (width is <= 0 or > MaximumDimension || height is <= 0 or > MaximumDimension)
                    {
                        break;
                    }
                    foundHeader = true;
                }
                else if (type.SequenceEqual(idat))
                {
                    foundImageData = true;
                }

                offset += checked((int)chunkLength + 12);
                if (type.SequenceEqual(iend))
                {
                    if (foundHeader && foundImageData && chunkLength == 0)
                    {
                        png = data.AsSpan(start, offset - start).ToArray();
                        return true;
                    }

                    break;
                }
            }
        }

        width = 0;
        height = 0;
        return false;
    }

    private static uint CalculateCrc(ReadOnlySpan<byte> bytes)
    {
        var crc = uint.MaxValue;
        foreach (var value in bytes)
        {
            crc = (crc >> 8) ^ CrcTable[(int)((crc ^ value) & 0xff)];
        }
        return ~crc;
    }

    private static uint[] CreateCrcTable()
    {
        var table = new uint[256];
        for (var index = 0; index < table.Length; index++)
        {
            var crc = (uint)index;
            for (var bit = 0; bit < 8; bit++)
            {
                crc = (crc & 1) == 0 ? crc >> 1 : (crc >> 1) ^ 0xedb88320;
            }
            table[index] = crc;
        }
        return table;
    }
}
