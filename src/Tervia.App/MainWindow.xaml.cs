using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Tervia.App.Pages;
using Tervia.App.ViewModels;
using Windows.Graphics;
using WinRT.Interop;

namespace Tervia.App;

public sealed partial class MainWindow : Window
{
    private readonly IServiceProvider _services;

    public MainWindow(IServiceProvider services, ShellViewModel shell)
    {
        _services = services;
        Shell = shell;
        InitializeComponent();
        BrandLogo.Source = AppAssets.LoadBitmap("logo-tervia-color.png");
        Title = "Tervia — Gestión local de Smart TVs";
        ResizeForDesktop();
        if (Navigation.MenuItems.Count > 0)
        {
            Navigation.SelectedItem = Navigation.MenuItems[0];
        }
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
            "dashboard" => CreateDashboardPage(),
            "home" => _services.GetRequiredService<HomePage>(),
            "devices" => _services.GetRequiredService<DevicesPage>(),
            "discovery" => _services.GetRequiredService<DiscoveryPage>(),
            "settings" => _services.GetRequiredService<SettingsPage>(),
            _ => _services.GetRequiredService<DashboardPage>(),
        };
    }

    private DashboardPage CreateDashboardPage()
    {
        var page = _services.GetRequiredService<DashboardPage>();
        page.NavigationRequested += NavigateTo;
        return page;
    }

    private void NavigateTo(string route)
    {
        var navigationRoute = route == "manual-add" ? "discovery" : route;
        var item = Navigation.MenuItems
            .OfType<NavigationViewItem>()
            .FirstOrDefault(candidate => string.Equals(candidate.Tag as string, navigationRoute, StringComparison.Ordinal));
        if (item is not null)
        {
            Navigation.SelectedItem = item;
        }

        if (route == "manual-add")
        {
            DispatcherQueue.TryEnqueue(() => (ContentFrame.Content as DiscoveryPage)?.FocusManualAddress());
        }
    }

    private void ResizeForDesktop()
    {
        var handle = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(handle);
        var workArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary).WorkArea;
        var width = Math.Min(workArea.Width, Math.Min(1440, Math.Max(960, (int)Math.Round(workArea.Width * 0.9))));
        var height = Math.Min(workArea.Height, Math.Min(960, Math.Max(680, (int)Math.Round(workArea.Height * 0.88))));
        var appWindow = AppWindow.GetFromWindowId(windowId);
        appWindow.Resize(new SizeInt32(width, height));
        appWindow.Move(new PointInt32(
            workArea.X + (workArea.Width - width) / 2,
            workArea.Y + (workArea.Height - height) / 2));
    }
}
