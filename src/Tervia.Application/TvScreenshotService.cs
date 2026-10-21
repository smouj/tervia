using Tervia.Capabilities;
using Tervia.Core;

namespace Tervia.Application;

public sealed record TvScreenshotCapture(TvScreenshot Screenshot, CapabilitySet Capabilities);

/// <summary>Captures a TV image only after an explicit caller request.</summary>
public sealed class TvScreenshotService(TvSessionService sessions)
{
    public async Task<TvScreenshotCapture> CaptureAsync(TvDevice device, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(device);
        var session = await sessions.OpenAsync(device, cancellationToken).ConfigureAwait(false);
        try
        {
            if (session.Adapter is not ITvScreenshotProvider provider)
            {
                throw new UnsupportedCapabilityException(CapabilityId.DeveloperScreenshot.Value);
            }

            var screenshot = await provider.CaptureScreenshotAsync(session, cancellationToken).ConfigureAwait(false);
            var capabilities = session.Capabilities.With(CapabilityInfo.Supported(
                CapabilityId.DeveloperScreenshot,
                RiskLevel.Safe,
                "successful on-demand capture",
                CapabilityConfidence.Verified));
            return new TvScreenshotCapture(screenshot, capabilities);
        }
        finally
        {
            await session.Adapter.DisconnectAsync(session, CancellationToken.None).ConfigureAwait(false);
        }
    }
}
