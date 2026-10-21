using Microsoft.UI.Xaml.Controls;
using Tervia.Infrastructure;

namespace Tervia.App.Pages;

public sealed partial class SettingsPage : Page
{
    public SettingsPage(TerviaPaths paths)
    {
        InitializeComponent();
        DataRoot = paths.Root;
        LibraryFilePath = Path.Combine(paths.Tvs, "library.json");
        AppVersion = typeof(App).Assembly.GetName().Version?.ToString() ?? "Desarrollo";
    }

    public string DataRoot { get; }

    public string LibraryFilePath { get; }

    public string AppVersion { get; }
}
