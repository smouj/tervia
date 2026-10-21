using System.Collections.Immutable;

namespace Tervia.Capabilities;

/// <summary>Immutable capability snapshot for one television.</summary>
public sealed class CapabilitySet
{
    private readonly ImmutableDictionary<CapabilityId, CapabilityInfo> _items;

    private CapabilitySet(ImmutableDictionary<CapabilityId, CapabilityInfo> items) => _items = items;

    public static CapabilitySet Empty { get; } = new(ImmutableDictionary<CapabilityId, CapabilityInfo>.Empty);

    public static CapabilitySet From(IEnumerable<CapabilityInfo> capabilities)
    {
        ArgumentNullException.ThrowIfNull(capabilities);
        var builder = ImmutableDictionary.CreateBuilder<CapabilityId, CapabilityInfo>();
        foreach (var capability in capabilities)
        {
            builder[capability.Id] = capability;
        }
        return new CapabilitySet(builder.ToImmutable());
    }

    public IReadOnlyCollection<CapabilityInfo> All => _items.Values;

    public CapabilityInfo? Find(CapabilityId id) => _items.TryGetValue(id, out var info) ? info : null;

    public bool Supports(CapabilityId id) => Find(id)?.IsUsable == true;

    public CapabilitySet With(CapabilityInfo capability)
        => new(_items.SetItem(capability.Id, capability));

    public override string ToString()
        => string.Join(", ", _items.Values.Where(c => c.IsUsable).Select(c => c.Id.Value).Order());
}
