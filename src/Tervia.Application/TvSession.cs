using Tervia.Capabilities;
using Tervia.Core;

namespace Tervia.Application;

public enum TvAuthenticationState
{
    NotRequired = 0,
    Pending,
    Authenticated,
    Rejected,
}

public sealed class TvSession
{
    public TvSession(TvDevice device, ITvPlatformAdapter adapter, CapabilitySet capabilities)
    {
        Device = device;
        Adapter = adapter;
        Capabilities = capabilities;
    }

    public TvDevice Device { get; }
    public ITvPlatformAdapter Adapter { get; }
    public CapabilitySet Capabilities { get; }
    public TvAuthenticationState AuthenticationState { get; init; } = TvAuthenticationState.NotRequired;
    public bool Connected { get; set; }
}
