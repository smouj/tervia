using Microsoft.UI.Xaml.Controls;
using Tervia.App.ViewModels;

namespace Tervia.App.Pages;

public sealed partial class DashboardPage : Page
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        HeroLogo.Source = AppAssets.LoadBitmap("logo-tervia-white.png");
    }

    public DashboardViewModel ViewModel { get; }

    public event Action<string>? NavigationRequested;

    private void OnDiscoveryClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs args)
        => NavigationRequested?.Invoke("discovery");

    private void OnManualAddClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs args)
        => NavigationRequested?.Invoke("manual-add");

    private void OnDevicesClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs args)
        => NavigationRequested?.Invoke("devices");

    private void OnPrivacyClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs args)
        => NavigationRequested?.Invoke("settings");
}
