using System.Text;
using System.Buffers.Binary;
using System.IO.Compression;
using Tervia.Platforms.AndroidTV;
using Xunit;

namespace Tervia.Platforms.Tests;

public sealed class AdbCommandLineTests
{
    private static readonly string[] PackageListArguments = ["-s", "192.168.1.30:5555", "shell", "pm", "list", "packages"];
    private static readonly string[] VersionArguments = ["version"];
    private static readonly string[] GetPropertyArguments = ["-s", "abc", "shell", "getprop", "ro.product.model"];
    private static readonly byte[] PngSignature = [137, 80, 78, 71, 13, 10, 26, 10];
    private static readonly byte[] OneByOnePng = CreatePng();

    [Fact]
    public void Serial_is_passed_as_structured_arguments()
    {
        var args = AdbCommandLine.Shell("192.168.1.30:5555", "pm", "list", "packages");

        Assert.Equal(PackageListArguments, args);
    }

    [Fact]
    public void Empty_serial_omits_device_flag()
    {
        var args = AdbCommandLine.ForDevice(null, "version");

        Assert.Equal(VersionArguments, args);
    }

    [Fact]
    public void GetProp_targets_the_property()
    {
        var args = AdbCommandLine.GetProp("abc", "ro.product.model");

        Assert.Equal(GetPropertyArguments, args);
    }

    [Fact]
    public void Fire_tv_packages_are_identified()
    {
        Assert.True(FireTvPlatformProfile.IsAmazonPackage("com.amazon.tv.launcher"));
        Assert.False(FireTvPlatformProfile.IsAmazonPackage("com.netflix.ninja"));
    }

    [Theory]
    [InlineData("192.168.1.30", "192.168.1.30:5555")]
    [InlineData("192.168.1.30:6123", "192.168.1.30:6123")]
    [InlineData("2001:db8::10", "[2001:db8::10]:5555")]
    [InlineData("2001:db8::10:6123", "[2001:db8::10:6123]:5555")]
    [InlineData("[2001:db8::10]:6123", "[2001:db8::10]:6123")]
    public void Network_addresses_are_normalised_for_adb(string address, string expected)
    {
        Assert.Equal(expected, AdbCommandLine.TargetForAddress(address));
    }

    [Theory]
    [InlineData("tv.local")]
    [InlineData("192.168.1.30:0")]
    [InlineData("192.168.1.30:65536")]
    public void Invalid_or_ambiguous_adb_addresses_are_rejected(string address)
    {
        Assert.Throws<ArgumentException>(() => AdbCommandLine.TargetForAddress(address));
    }

    [Fact]
    public void Png_parser_strips_adb_wrapper_output_and_reads_dimensions()
    {
        var png = OneByOnePng;
        var wrapped = Encoding.UTF8.GetBytes("Init wrapper sys mutex successful\n")
            .Concat(png)
            .Concat(Encoding.UTF8.GetBytes("\ntrailing log"))
            .ToArray();

        Assert.True(PngScreenshotParser.TryExtract(wrapped, out var extracted, out var width, out var height));
        Assert.Equal(png, extracted);
        Assert.Equal(1, width);
        Assert.Equal(1, height);
    }

    [Fact]
    public void Png_parser_rejects_truncated_or_non_png_output()
    {
        Assert.False(PngScreenshotParser.TryExtract("no image"u8.ToArray(), out _, out _, out _));
        Assert.False(PngScreenshotParser.TryExtract(OneByOnePng[..^4], out _, out _, out _));
    }

    [Fact]
    public void Png_parser_rejects_crc_mismatch()
    {
        var corrupted = OneByOnePng.ToArray();
        corrupted[16] ^= 0x01;
        Assert.False(PngScreenshotParser.TryExtract(corrupted, out _, out _, out _));
    }

    private static byte[] CreatePng()
    {
        using var output = new MemoryStream();
        output.Write(PngSignature);

        Span<byte> header = stackalloc byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header, 1);
        BinaryPrimitives.WriteInt32BigEndian(header[4..], 1);
        header[8] = 8;
        header[9] = 6;
        WriteChunk(output, "IHDR"u8, header);

        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            zlib.Write([0, 0, 0, 0, 0]);
        }
        WriteChunk(output, "IDAT"u8, compressed.ToArray());
        WriteChunk(output, "IEND"u8, []);
        return output.ToArray();
    }

    private static void WriteChunk(Stream output, ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(length, data.Length);
        output.Write(length);
        output.Write(type);
        output.Write(data);

        var crc = CalculateCrc(type, data);
        Span<byte> crcBytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crcBytes, crc);
        output.Write(crcBytes);
    }

    private static uint CalculateCrc(ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        var crc = uint.MaxValue;
        foreach (var value in type)
        {
            crc = UpdateCrc(crc, value);
        }
        foreach (var value in data)
        {
            crc = UpdateCrc(crc, value);
        }
        return ~crc;
    }

    private static uint UpdateCrc(uint crc, byte value)
    {
        crc ^= value;
        for (var bit = 0; bit < 8; bit++)
        {
            crc = (crc & 1) == 0 ? crc >> 1 : (crc >> 1) ^ 0xedb88320;
        }
        return crc;
    }
}
