using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Tervia.App.Pages;
using Tervia.App.ViewModels;

namespace Tervia.App;

public sealed partial class MainWindow : Window
{
    private readonly IServiceProvider _services;

    public MainWindow(IServiceProvider services, ShellViewModel shell)
    {
        InitializeComponent();
        _services = services;
        Shell = shell;
        Navigation.SelectedItem = Navigation.MenuItems[0];
        _ = Shell.InitializeAsync();
    }

    public ShellViewModel Shell { get; }

    private void OnNavigationSelectionChanged(
        NavigationView sender,
        NavigationViewSelectionChangedEventArgs args)
    {
        var route = args.SelectedItemContainer?.Tag as string;
        ContentFrame.Content = route switch
        {
            "dashboard" => _services.GetRequiredService<DashboardPage>(),
            "devices" => _services.GetRequiredService<DevicesPage>(),
            "discovery" => _services.GetRequiredService<DiscoveryPage>(),
            "settings" => _services.GetRequiredService<SettingsPage>(),
            _ => _services.GetRequiredService<DashboardPage>(),
        };
    }
}
