using System.Collections.Concurrent;

namespace Tervia.Application;

/// <summary>Serialises state changing operations per television.</summary>
public sealed class TvOperationQueue : IDisposable
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _gates = new(StringComparer.Ordinal);

    public async Task<T> ExecuteAsync<T>(string tvId, Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tvId);
        ArgumentNullException.ThrowIfNull(operation);

        var gate = _gates.GetOrAdd(tvId, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await operation(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            gate.Release();
        }
    }

    public void Dispose()
    {
        foreach (var gate in _gates.Values)
        {
            gate.Dispose();
        }
        _gates.Clear();
    }
}
