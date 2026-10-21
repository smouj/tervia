using System.Buffers.Binary;
using System.IO.Compression;
using Tervia.Core;
using Tervia.Infrastructure;
using Tervia.Platforms.AndroidTV;
using Xunit;

namespace Tervia.Platforms.Tests;

public sealed class AdbClientTests
{
    private static readonly string[] VersionCommand = ["version"];
    private static readonly string[] ConnectCommand = ["connect", "192.168.1.70:5555"];
    private static readonly string[] GetStateCommand = ["-s", "192.168.1.70:5555", "get-state"];

    [Fact]
    public async Task Connect_checks_tool_and_authorized_device_using_structured_arguments()
    {
        var runner = new FakeProcessRunner
        {
            TextResult = (_, arguments) => arguments[^1] == "get-state"
                ? Success("device")
                : Success("connected to 192.168.1.70:5555"),
        };
        var adb = new AdbClient(runner, "fake-adb");

        var target = await adb.ConnectAsync("192.168.1.70", CancellationToken.None);

        Assert.Equal("192.168.1.70:5555", target);
        Assert.Collection(runner.TextCommands,
            command => Assert.Equal(VersionCommand, command),
            command => Assert.Equal(ConnectCommand, command),
            command => Assert.Equal(GetStateCommand, command));
    }

    [Fact]
    public async Task Connect_reports_authorization_required_when_device_is_unauthorized()
    {
        var runner = new FakeProcessRunner
        {
            TextResult = (_, arguments) => arguments[^1] == "get-state"
                ? Success("unauthorized")
                : Success("connected to 192.168.1.70:5555"),
        };
        var adb = new AdbClient(runner, "fake-adb");

        var error = await Assert.ThrowsAsync<AuthenticationRequiredException>(
            () => adb.ConnectAsync("192.168.1.70", CancellationToken.None));

        Assert.Contains("depuración USB", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(3, runner.TextCommands.Count);
    }

    [Fact]
    public async Task Screenshot_falls_back_to_pull_and_cleans_both_temporary_files()
    {
        var png = CreatePng();
        string? temporaryHostPath = null;
        string? temporaryTvPath = null;
        var runner = new FakeProcessRunner
        {
            BinaryOutput = "device wrapper log"u8.ToArray(),
            TextResult = (fileName, arguments) =>
            {
                if (arguments.Contains("pull", StringComparer.Ordinal))
                {
                    temporaryHostPath = arguments[^1];
                    File.WriteAllBytes(temporaryHostPath, png);
                    return Success($"{arguments[^2]}: 1 file pulled");
                }

                if (arguments.Contains("screencap", StringComparer.Ordinal))
                {
                    temporaryTvPath = arguments[^1];
                }
                else if (arguments.Contains("rm", StringComparer.Ordinal))
                {
                    Assert.Equal(temporaryTvPath, arguments[^1]);
                }

                return Success(string.Empty);
            },
        };
        var adb = new AdbClient(runner, "fake-adb");

        var screenshot = await adb.CaptureScreenshotAsync("192.168.1.70", CancellationToken.None);

        Assert.Equal(png, screenshot.PngData.ToArray());
        Assert.Equal(1, screenshot.Width);
        Assert.Equal(1, screenshot.Height);
        Assert.NotNull(temporaryTvPath);
        Assert.StartsWith("/data/local/tmp/tervia-screen-", temporaryTvPath!, StringComparison.Ordinal);
        Assert.EndsWith(".png", temporaryTvPath!, StringComparison.Ordinal);
        Assert.NotNull(temporaryHostPath);
        Assert.False(File.Exists(temporaryHostPath));
        Assert.Contains(runner.TextCommands, command => command.Contains("pull", StringComparer.Ordinal));
        Assert.Contains(runner.TextCommands, command => command.Contains("rm", StringComparer.Ordinal));
    }

    private static ProcessResult Success(string output)
        => new("fake-adb", 0, output, string.Empty, TimeSpan.Zero, TimedOut: false);

    private static byte[] CreatePng()
    {
        ReadOnlySpan<byte> signature = [137, 80, 78, 71, 13, 10, 26, 10];
        using var output = new MemoryStream();
        output.Write(signature);

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

        var crc = uint.MaxValue;
        foreach (var value in type)
        {
            crc = UpdateCrc(crc, value);
        }
        foreach (var value in data)
        {
            crc = UpdateCrc(crc, value);
        }

        Span<byte> crcBytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crcBytes, ~crc);
        output.Write(crcBytes);
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

    private sealed class FakeProcessRunner : IProcessRunner
    {
        public List<string[]> TextCommands { get; } = [];
        public List<string[]> BinaryCommands { get; } = [];
        public byte[] BinaryOutput { get; init; } = [];
        public Func<string, IReadOnlyList<string>, ProcessResult> TextResult { get; init; }
            = (_, _) => Success(string.Empty);

        public Task<ProcessResult> RunAsync(
            string fileName,
            IReadOnlyList<string> arguments,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            var copy = arguments.ToArray();
            TextCommands.Add(copy);
            return Task.FromResult(TextResult(fileName, copy));
        }

        public Task<BinaryProcessResult> RunBinaryAsync(
            string fileName,
            IReadOnlyList<string> arguments,
            TimeSpan timeout,
            int maximumStandardOutputBytes,
            CancellationToken cancellationToken)
        {
            BinaryCommands.Add(arguments.ToArray());
            return Task.FromResult(new BinaryProcessResult(
                fileName,
                0,
                BinaryOutput,
                string.Empty,
                TimeSpan.Zero,
                TimedOut: false));
        }
    }
}
