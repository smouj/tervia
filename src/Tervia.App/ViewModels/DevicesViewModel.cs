using Tervia.Application;
using Tervia.Core;

namespace Tervia.App.ViewModels;

public sealed class DevicesViewModel(ShellViewModel shell, TvScreenshotService screenshots)
{
    public ShellViewModel Shell { get; } = shell;

    public Task<TvScreenshotCapture> CaptureScreenshotAsync(TvDevice device, CancellationToken cancellationToken)
        => screenshots.CaptureAsync(device, cancellationToken);
}
