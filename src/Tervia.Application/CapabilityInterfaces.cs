using Tervia.Core;

namespace Tervia.Application;

public interface ITvRemoteControl
{
    Task PressAsync(TvSession session, TvRemoteKey key, CancellationToken cancellationToken);
}

public interface ITvTextInput
{
    Task SendTextAsync(TvSession session, string text, CancellationToken cancellationToken);
}

public interface ITvPowerControl
{
    Task PowerOffAsync(TvSession session, CancellationToken cancellationToken);
    Task PowerOnAsync(TvDevice device, CancellationToken cancellationToken);
}

public interface ITvVolumeControl
{
    Task ChangeVolumeAsync(TvSession session, int delta, CancellationToken cancellationToken);
    Task SetMutedAsync(TvSession session, bool muted, CancellationToken cancellationToken);
}

public interface ITvInputManager
{
    Task<IReadOnlyList<TvInput>> ListInputsAsync(TvSession session, CancellationToken cancellationToken);
    Task SwitchInputAsync(TvSession session, TvInput input, CancellationToken cancellationToken);
}

public interface ITvApplicationManager
{
    Task<IReadOnlyList<TvApplication>> ListApplicationsAsync(TvSession session, CancellationToken cancellationToken);
    Task LaunchApplicationAsync(TvSession session, string applicationId, CancellationToken cancellationToken);
    Task StopApplicationAsync(TvSession session, string applicationId, CancellationToken cancellationToken);
}

public interface ITvDiagnosticsProvider
{
    Task<IReadOnlyDictionary<string, string>> ReadDiagnosticsAsync(TvSession session, CancellationToken cancellationToken);
}

public interface ITvScreenshotProvider
{
    Task<TvScreenshot> CaptureScreenshotAsync(TvSession session, CancellationToken cancellationToken);
}

public interface ITvDisplayProvider
{
    Task<IReadOnlyDictionary<string, string>> ReadDisplayAsync(TvSession session, CancellationToken cancellationToken);
}
