using Tervia.Core;

namespace Tervia.Application;

public sealed class PlatformAdapterRegistry
{
    private readonly Dictionary<TvPlatform, ITvPlatformAdapter> _adapters = [];

    public void Register(ITvPlatformAdapter adapter)
    {
        ArgumentNullException.ThrowIfNull(adapter);
        _adapters[adapter.Platform] = adapter;
    }

    public void Register(TvPlatform platform, ITvPlatformAdapter adapter)
    {
        ArgumentNullException.ThrowIfNull(adapter);
        if (platform == TvPlatform.Unknown)
        {
            throw new ArgumentOutOfRangeException(nameof(platform), "Unknown is not a registerable platform.");
        }

        _adapters[platform] = adapter;
    }

    public ITvPlatformAdapter? Resolve(TvPlatform platform)
        => _adapters.TryGetValue(platform, out var adapter) ? adapter : null;

    public IReadOnlyCollection<ITvPlatformAdapter> All => _adapters.Values.Distinct().ToList();

    public ITvPlatformAdapter Require(TvPlatform platform)
        => Resolve(platform) ?? throw new UnsupportedCapabilityException($"platform.{platform}");
}
