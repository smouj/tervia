using System;
using System.Reflection;
using Microsoft.UI.Xaml.Controls;
using Tervia.Infrastructure;

namespace Tervia.App.Pages;

public sealed partial class SettingsPage : Page
{
    public SettingsPage(TerviaPaths paths)
    {
        DataRoot = ToDisplayPath(paths.Root);
        LibraryFilePath = ToDisplayPath(Path.Combine(paths.Tvs, "library.json"));
        var informationalVersion = typeof(App).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        AppVersion = informationalVersion?.Split('+')[0] ?? "Desarrollo";
        InitializeComponent();
    }

    public string DataRoot { get; }

    public string LibraryFilePath { get; }

    public string AppVersion { get; }

    private static string ToDisplayPath(string path)
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return !string.IsNullOrWhiteSpace(localAppData) &&
            path.StartsWith(localAppData, StringComparison.OrdinalIgnoreCase)
            ? $"%LOCALAPPDATA%{path[localAppData.Length..]}"
            : path;
    }
}
