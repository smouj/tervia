using System.ComponentModel;
using Tervia.Core;
using Tervia.Infrastructure;

namespace Tervia.Platforms.AndroidTV;

public sealed class AdbClient
{
    private const int MaximumScreenshotBytes = 64 * 1024 * 1024;
    private readonly ProcessRunner _runner;
    private readonly string _executable;

    public AdbClient(ProcessRunner runner, string executable = "adb")
    {
        _runner = runner;
        _executable = executable;
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _runner.RunAsync(
                _executable,
                ["version"],
                TimeSpan.FromSeconds(10),
                cancellationToken).ConfigureAwait(false);
            return result.Succeeded;
        }
        catch (Win32Exception)
        {
            return false;
        }
    }

    public async Task<string> ConnectAsync(string networkAddress, CancellationToken cancellationToken)
    {
        if (!await IsAvailableAsync(cancellationToken).ConfigureAwait(false))
        {
            throw new PlatformToolMissingException(_executable);
        }

        var target = AdbCommandLine.TargetForAddress(networkAddress);
        var connect = await _runner.RunAsync(
            _executable,
            ["connect", target],
            TimeSpan.FromSeconds(15),
            cancellationToken).ConfigureAwait(false);

        if (!connect.Succeeded)
        {
            var detail = string.Concat(connect.StandardOutput, " ", connect.StandardError);
            if (detail.Contains("unauthorized", StringComparison.OrdinalIgnoreCase))
            {
                throw AuthorizationRequired();
            }
            if (detail.Contains("refused", StringComparison.OrdinalIgnoreCase) ||
                detail.Contains("failed to connect", StringComparison.OrdinalIgnoreCase) ||
                detail.Contains("no route", StringComparison.OrdinalIgnoreCase))
            {
                throw new TvNotFoundException(networkAddress);
            }
            throw new PlatformToolFailureException(_executable, connect.ExitCode, detail.Trim());
        }

        var state = await _runner.RunAsync(
            _executable,
            AdbCommandLine.ForDevice(target, "get-state"),
            TimeSpan.FromSeconds(10),
            cancellationToken).ConfigureAwait(false);
        var stateText = string.Concat(state.StandardOutput, " ", state.StandardError).Trim();
        if (stateText.Contains("unauthorized", StringComparison.OrdinalIgnoreCase))
        {
            throw AuthorizationRequired();
        }
        if (stateText.Contains("offline", StringComparison.OrdinalIgnoreCase))
        {
            throw new TvDisconnectedException(networkAddress);
        }
        if (!state.Succeeded || !string.Equals(state.StandardOutput.Trim(), "device", StringComparison.OrdinalIgnoreCase))
        {
            throw new TvNotFoundException(networkAddress);
        }

        return target;
    }

    public async Task DisconnectAsync(string networkAddress, CancellationToken cancellationToken)
    {
        var target = AdbCommandLine.TargetForAddress(networkAddress);
        try
        {
            await _runner.RunAsync(
                _executable,
                ["disconnect", target],
                TimeSpan.FromSeconds(5),
                cancellationToken).ConfigureAwait(false);
        }
        catch (Win32Exception)
        {
            // A best-effort host-side disconnect must not hide the operation result.
        }
    }

    public async Task<string> GetPropertyAsync(string? serial, string property, CancellationToken cancellationToken)
    {
        var result = await _runner.RunAsync(
            _executable,
            AdbCommandLine.GetProp(Target(serial), property),
            TimeSpan.FromSeconds(15),
            cancellationToken).ConfigureAwait(false);

        if (!result.Succeeded)
        {
            throw new PlatformToolFailureException(_executable, result.ExitCode, result.StandardError);
        }

        return result.StandardOutput.Trim();
    }

    public async Task<string> ShellAsync(string? serial, string[] command, CancellationToken cancellationToken)
    {
        var result = await _runner.RunAsync(
            _executable,
            AdbCommandLine.Shell(Target(serial), command),
            TimeSpan.FromSeconds(30),
            cancellationToken).ConfigureAwait(false);

        if (!result.Succeeded)
        {
            throw new PlatformToolFailureException(_executable, result.ExitCode, result.StandardError);
        }

        return result.StandardOutput.Trim();
    }

    public async Task KeyEventAsync(string? serial, int keyCode, CancellationToken cancellationToken)
    {
        await ShellAsync(serial, ["input", "keyevent", keyCode.ToString(System.Globalization.CultureInfo.InvariantCulture)], cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<TvScreenshot> CaptureScreenshotAsync(string? serial, CancellationToken cancellationToken)
    {
        var target = Target(serial);
        var direct = await _runner.RunBinaryAsync(
            _executable,
            AdbCommandLine.ForDevice(target, "exec-out", "screencap", "-p"),
            TimeSpan.FromSeconds(45),
            MaximumScreenshotBytes,
            cancellationToken).ConfigureAwait(false);

        if (direct.TimedOut)
        {
            throw new OperationTimeoutException("android.screenshot", TimeSpan.FromSeconds(45));
        }

        if (direct.Succeeded && PngScreenshotParser.TryExtract(direct.StandardOutput, out var directPng, out var directWidth, out var directHeight))
        {
            return CreateScreenshot(directPng, directWidth, directHeight);
        }

        var suffix = Guid.NewGuid().ToString("N", System.Globalization.CultureInfo.InvariantCulture);
        var remotePath = $"/data/local/tmp/tervia-screen-{suffix}.png";
        var localPath = Path.Combine(Path.GetTempPath(), $"tervia-screen-{suffix}.png");
        try
        {
            await ShellAsync(target, ["screencap", "-p", remotePath], cancellationToken).ConfigureAwait(false);
            var pull = await _runner.RunAsync(
                _executable,
                AdbCommandLine.ForDevice(target, "pull", remotePath, localPath),
                TimeSpan.FromSeconds(45),
                cancellationToken).ConfigureAwait(false);

            if (!pull.Succeeded || !File.Exists(localPath))
            {
                var detail = string.Concat(pull.StandardOutput, " ", pull.StandardError).Trim();
                throw new PlatformToolFailureException(_executable, pull.ExitCode, detail);
            }

            var fileInfo = new FileInfo(localPath);
            if (fileInfo.Length is <= 0 or > MaximumScreenshotBytes)
            {
                throw new OperationRejectedException("The television returned an empty or oversized screenshot.");
            }

            var bytes = await File.ReadAllBytesAsync(localPath, cancellationToken).ConfigureAwait(false);
            if (!PngScreenshotParser.TryExtract(bytes, out var png, out var width, out var height))
            {
                throw new OperationRejectedException("The television did not return a valid PNG screenshot. Protected video and HDMI inputs may not be capturable.");
            }

            return CreateScreenshot(png, width, height);
        }
        finally
        {
            try
            {
                await ShellAsync(target, ["rm", "-f", remotePath], CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is TerviaException or OperationCanceledException or Win32Exception or IOException or InvalidOperationException)
            {
                // The random remote name is confined to /data/local/tmp; a failed cleanup is non-fatal.
            }

            try
            {
                File.Delete(localPath);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    private static string Target(string? serial)
        => AdbCommandLine.TargetForAddress(serial ?? throw new ArgumentNullException(nameof(serial)));

    private static AuthenticationRequiredException AuthorizationRequired()
        => new("Android TV", "acepta la solicitud de depuración USB en la pantalla del televisor y vuelve a intentarlo.");

    private static TvScreenshot CreateScreenshot(byte[] png, int width, int height)
        => new()
        {
            PngData = png,
            Width = width,
            Height = height,
            CapturedAt = DateTimeOffset.UtcNow,
        };

}
