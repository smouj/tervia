using System.Diagnostics;

namespace Tervia.Infrastructure;

public sealed record ProcessResult(
    string FileName,
    int ExitCode,
    string StandardOutput,
    string StandardError,
    TimeSpan Duration,
    bool TimedOut)
{
    public bool Succeeded => ExitCode == 0 && !TimedOut;
}

public sealed record BinaryProcessResult(
    string FileName,
    int ExitCode,
    byte[] StandardOutput,
    string StandardError,
    TimeSpan Duration,
    bool TimedOut)
{
    public bool Succeeded => ExitCode == 0 && !TimedOut;
}

public interface IProcessRunner
{
    Task<ProcessResult> RunAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken);

    Task<BinaryProcessResult> RunBinaryAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        TimeSpan timeout,
        int maximumStandardOutputBytes,
        CancellationToken cancellationToken);
}

/// <summary>
/// Runs external platform tools with structured arguments, no shell
/// concatenation, a timeout and cancellation support.
/// </summary>
public sealed class ProcessRunner : IProcessRunner
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Instance shape allows substitution and decorators in tests.")]
    public async Task<ProcessResult> RunAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(arguments);

        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = new Process { StartInfo = startInfo };
        var started = Stopwatch.StartNew();

        if (!process.Start())
        {
            throw new InvalidOperationException($"Could not start '{fileName}'.");
        }

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        using var timeoutSource = new CancellationTokenSource(timeout);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);
        var timedOut = false;

        try
        {
            await process.WaitForExitAsync(linked.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            timedOut = timeoutSource.IsCancellationRequested && !cancellationToken.IsCancellationRequested;
            TryKill(process);
            if (!timedOut)
            {
                throw;
            }
        }

        started.Stop();
        var stdout = await SafeReadAsync(stdoutTask).ConfigureAwait(false);
        var stderr = await SafeReadAsync(stderrTask).ConfigureAwait(false);

        return new ProcessResult(
            fileName,
            timedOut ? -1 : process.ExitCode,
            stdout.Trim(),
            stderr.Trim(),
            started.Elapsed,
            timedOut);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Instance shape keeps binary and text process execution substitutable.")]
    public async Task<BinaryProcessResult> RunBinaryAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        TimeSpan timeout,
        int maximumStandardOutputBytes,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumStandardOutputBytes);

        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = new Process { StartInfo = startInfo };
        var started = Stopwatch.StartNew();

        if (!process.Start())
        {
            throw new InvalidOperationException($"Could not start '{fileName}'.");
        }

        using var timeoutSource = new CancellationTokenSource(timeout);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);
        var stdoutTask = ReadBinaryAsync(process.StandardOutput.BaseStream, maximumStandardOutputBytes, linked.Token);
        var stderrTask = process.StandardError.ReadToEndAsync(linked.Token);
        var timedOut = false;

        try
        {
            var waitTask = process.WaitForExitAsync(linked.Token);
            var firstCompleted = await Task.WhenAny(waitTask, stdoutTask).ConfigureAwait(false);
            if (firstCompleted == stdoutTask)
            {
                await stdoutTask.ConfigureAwait(false);
            }

            await waitTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            timedOut = timeoutSource.IsCancellationRequested && !cancellationToken.IsCancellationRequested;
            TryKill(process);
            if (!timedOut)
            {
                throw;
            }
        }
        catch
        {
            TryKill(process);
            throw;
        }

        started.Stop();
        var stdout = await SafeReadBinaryAsync(stdoutTask).ConfigureAwait(false);
        var stderr = await SafeReadAsync(stderrTask).ConfigureAwait(false);

        return new BinaryProcessResult(
            fileName,
            timedOut ? -1 : process.ExitCode,
            stdout,
            stderr.Trim(),
            started.Elapsed,
            timedOut);
    }

    private static async Task<string> SafeReadAsync(Task<string> task)
    {
        try
        {
            return await task.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return string.Empty;
        }
    }

    private static async Task<byte[]> ReadBinaryAsync(Stream source, int maximumBytes, CancellationToken cancellationToken)
    {
        await using var output = new MemoryStream();
        var buffer = new byte[81920];
        while (true)
        {
            var count = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (count == 0)
            {
                break;
            }

            if (output.Length + count > maximumBytes)
            {
                throw new InvalidDataException($"Process output exceeded the {maximumBytes} byte limit.");
            }

            await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
        }

        return output.ToArray();
    }

    private static async Task<byte[]> SafeReadBinaryAsync(Task<byte[]> task)
    {
        try
        {
            return await task.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return [];
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }
    }
}
