using Tervia.Core;
using Tervia.Infrastructure;

namespace Tervia.Platforms.AndroidTV;

public sealed class AdbClient
{
    private readonly ProcessRunner _runner;
    private readonly string _executable;

    public AdbClient(ProcessRunner runner, string executable = "adb")
    {
        _runner = runner;
        _executable = executable;
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
    {
        var result = await _runner.RunAsync(
            _executable,
            ["version"],
            TimeSpan.FromSeconds(10),
            cancellationToken).ConfigureAwait(false);
        return result.Succeeded;
    }

    public async Task<string> GetPropertyAsync(string? serial, string property, CancellationToken cancellationToken)
    {
        var result = await _runner.RunAsync(
            _executable,
            AdbCommandLine.GetProp(serial, property),
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
            AdbCommandLine.Shell(serial, command),
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
}
