using Tervia.Core;

namespace Tervia.Application;

/// <summary>Stores user-approved physical TV profiles on the local machine.</summary>
public interface ITvDeviceLibrary
{
    Task<IReadOnlyList<TvDevice>> LoadAsync(CancellationToken cancellationToken);

    Task SaveAsync(TvDevice device, CancellationToken cancellationToken);

    Task RemoveAsync(string deviceId, CancellationToken cancellationToken);
}
