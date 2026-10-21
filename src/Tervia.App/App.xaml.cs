using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Tervia.Application;
using Tervia.App.Pages;
using Tervia.App.ViewModels;
using Tervia.Core;
using Tervia.Discovery;
using Tervia.Infrastructure;
using Tervia.Platforms.AndroidTV;
using Tervia.Safety;

namespace Tervia.App;

public partial class App : Microsoft.UI.Xaml.Application
{
    private readonly ServiceProvider _services;
    private Window? _window;

    public App()
    {
        InitializeComponent();

        var registrations = new ServiceCollection();
        registrations.AddSingleton(_ => new TerviaPaths());
        registrations.AddSingleton<ProcessRunner>();
        registrations.AddSingleton<ITvDeviceLibrary, JsonTvDeviceLibrary>();
        registrations.AddSingleton<ITvDiscoveryStrategy>(_ => new SsdpDiscoveryStrategy());
        registrations.AddSingleton<ITvDiscoveryStrategy>(_ => new MdnsDiscoveryStrategy());
        registrations.AddSingleton<TvDiscoveryService>();
        registrations.AddSingleton<AndroidTvAdapter>(services => new AndroidTvAdapter(
            new AdbClient(services.GetRequiredService<ProcessRunner>())));
        registrations.AddSingleton(services =>
        {
            var registry = new PlatformAdapterRegistry();
            var android = services.GetRequiredService<AndroidTvAdapter>();
            registry.Register(android);
            registry.Register(TvPlatform.GoogleTV, android);
            registry.Register(TvPlatform.FireTV, android);
            return registry;
        });
        registrations.AddSingleton<TvSafetyEngine>(_ => new TvSafetyEngine([]));
        registrations.AddSingleton<TvSessionService>();
        registrations.AddSingleton<TvScreenshotService>();
        registrations.AddSingleton<ShellViewModel>();
        registrations.AddSingleton<DashboardViewModel>();
        registrations.AddSingleton<DevicesViewModel>();
        registrations.AddSingleton<DiscoveryViewModel>();
        registrations.AddTransient<DashboardPage>();
        registrations.AddTransient<DevicesPage>();
        registrations.AddTransient<DiscoveryPage>();
        registrations.AddTransient<SettingsPage>();
        _services = registrations.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _services.GetRequiredService<TerviaPaths>().EnsureCreated();
        _window = ActivatorUtilities.CreateInstance<MainWindow>(_services);
        _window.Activate();
    }
}
