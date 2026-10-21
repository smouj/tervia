namespace Tervia.Core;

public abstract class TerviaException : Exception
{
    protected TerviaException(string message) : base(message) { }
    protected TerviaException(string message, Exception inner) : base(message, inner) { }
}

public sealed class TvNotFoundException : TerviaException
{
    public TvNotFoundException(string address) : base($"No television was found at '{address}'.") { }
}

public sealed class TvDisconnectedException : TerviaException
{
    public TvDisconnectedException(string tvId) : base($"Television '{tvId}' is not connected.") { }
}

public sealed class PairingRequiredException : TerviaException
{
    public PairingRequiredException(string platform) : base($"{platform} requires pairing before this operation.") { }
}

public sealed class AuthenticationRequiredException : TerviaException
{
    public AuthenticationRequiredException(string platform) : base($"{platform} requires authentication before this operation.") { }
}

public sealed class DeveloperModeRequiredException : TerviaException
{
    public DeveloperModeRequiredException(string platform) : base($"{platform} requires developer mode for this operation.") { }
}

public sealed class UnsupportedCapabilityException : TerviaException
{
    public UnsupportedCapabilityException(string capabilityId)
        : base($"The selected television does not support capability '{capabilityId}'.") { }
}

public sealed class OperationRejectedException : TerviaException
{
    public OperationRejectedException(string reason) : base(reason) { }
}

public sealed class OperationTimeoutException : TerviaException
{
    public OperationTimeoutException(string operation, TimeSpan timeout)
        : base($"Operation '{operation}' did not finish within {timeout.TotalSeconds:0} seconds.") { }
}

public sealed class PlatformToolMissingException : TerviaException
{
    public PlatformToolMissingException(string tool) : base($"Required platform tool '{tool}' was not found.") { }
}

public sealed class PlatformToolFailureException : TerviaException
{
    public PlatformToolFailureException(string tool, int exitCode, string message)
        : base($"Platform tool '{tool}' failed with exit code {exitCode}: {message}") { }
}

public sealed class VendorRestrictionException : TerviaException
{
    public VendorRestrictionException(string message) : base(message) { }
}
