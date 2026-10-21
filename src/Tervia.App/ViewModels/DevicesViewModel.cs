namespace Tervia.App.ViewModels;

public sealed class DevicesViewModel(ShellViewModel shell)
{
    public ShellViewModel Shell { get; } = shell;
}
