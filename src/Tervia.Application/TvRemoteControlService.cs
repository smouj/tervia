using Tervia.Capabilities;
using Tervia.Core;

namespace Tervia.Application;

/// <summary>Runs an explicit remote-key request in the selected TV's serialized session.</summary>
public sealed class TvRemoteControlService(TvSessionService sessions, TvOperationScheduler operations)
{
    public async Task PressAsync(TvDevice device, TvRemoteKey key, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(device);

        await operations.ExecuteAsync(device.Id, async token =>
        {
            var session = await sessions.OpenAsync(device, token).ConfigureAwait(false);
            try
            {
                if (session.Adapter is not ITvRemoteControl remote)
                {
                    throw new UnsupportedCapabilityException($"remote.{key}");
                }

                var capability = key switch
                {
                    TvRemoteKey.Home => CapabilityId.RemoteHome,
                    TvRemoteKey.Back => CapabilityId.RemoteBack,
                    _ => CapabilityId.RemoteNavigation,
                };
                if (!session.Capabilities.Supports(capability))
                {
                    throw new UnsupportedCapabilityException(capability.Value);
                }

                await remote.PressAsync(session, key, token).ConfigureAwait(false);
                return true;
            }
            finally
            {
                try
                {
                    await session.Adapter.DisconnectAsync(session, CancellationToken.None).ConfigureAwait(false);
                }
                catch
                {
                    // A failed cleanup must not replace the remote-operation result.
                }
            }
        }, cancellationToken).ConfigureAwait(false);
    }
}
