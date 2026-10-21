using Tervia.Application;
using Tervia.Core;

namespace Tervia.App.ViewModels;

public sealed class DevicesViewModel(
    ShellViewModel shell,
    TvScreenshotService screenshots,
    TvRemoteControlService remoteControl)
{
    public ShellViewModel Shell { get; } = shell;

    public Task<TvScreenshotCapture> CaptureScreenshotAsync(TvDevice device, CancellationToken cancellationToken)
        => screenshots.CaptureAsync(device, cancellationToken);

    public Task PressRemoteKeyAsync(TvDevice device, TvRemoteKey key, CancellationToken cancellationToken)
        => remoteControl.PressAsync(device, key, cancellationToken);
}
