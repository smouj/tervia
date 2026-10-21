using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Tervia.App.ViewModels;

namespace Tervia.App.Pages;

public sealed partial class HomePage : Page
{
    private bool _isLoadingTheme;

    public HomePage(HomeViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        _isLoadingTheme = true;
        SelectTheme(ViewModel.Theme);
        _isLoadingTheme = false;
        UpdatePreview();
    }

    public HomeViewModel ViewModel { get; }

    private async void OnLoaded(object sender, RoutedEventArgs args)
    {
        ViewModel.Shell.PropertyChanged -= OnShellPropertyChanged;
        ViewModel.Shell.PropertyChanged += OnShellPropertyChanged;
        _isLoadingTheme = true;
        SelectTheme(ViewModel.Theme);
        _isLoadingTheme = false;
        await ViewModel.LoadAsync();
        _isLoadingTheme = true;
        SelectTheme(ViewModel.Theme);
        _isLoadingTheme = false;
        UpdatePreview();
    }

    private void OnUnloaded(object sender, RoutedEventArgs args)
    {
        ViewModel.Shell.PropertyChanged -= OnShellPropertyChanged;
    }

    private async void OnShellPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(ShellViewModel.SelectedDevice))
        {
            await ViewModel.LoadAsync();
            SelectTheme(ViewModel.Theme);
            UpdatePreview();
        }
    }

    private void OnThemeSelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (!_isLoadingTheme && ThemeSelector.SelectedItem is ComboBoxItem { Tag: string theme })
        {
            ViewModel.Theme = theme;
            UpdatePreview();
        }
    }

    private void OnClockToggled(object sender, RoutedEventArgs args)
        => UpdatePreview();

    private void UpdatePreview()
    {
        var accent = ViewModel.Theme switch
        {
            "violet" => Windows.UI.Color.FromArgb(255, 155, 140, 255),
            "sunset" => Windows.UI.Color.FromArgb(255, 255, 180, 90),
            "forest" => Windows.UI.Color.FromArgb(255, 85, 214, 163),
            _ => Windows.UI.Color.FromArgb(255, 57, 216, 242),
        };
        var brush = new SolidColorBrush(accent);
        PreviewAccentBorder.BorderBrush = brush;
        PreviewEyebrow.Foreground = brush;
        PreviewClock.Visibility = ViewModel.ShowClock ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void OnInstallClick(object sender, RoutedEventArgs args)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = "Instalar Tervia Home",
            Content = "Se instalará la pantalla de inicio de Tervia en la TV y se enviará el diseño. La pantalla original se conserva. La propia TV te pedirá confirmar si quieres usar Tervia Home como inicio.",
            PrimaryButtonText = "Instalar y continuar",
            CloseButtonText = "Cancelar",
            DefaultButton = ContentDialogButton.Close,
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary)
        {
            return;
        }

        var apkPath = Path.Combine(AppContext.BaseDirectory, "Tervia.Home.Android.apk");
        await ViewModel.InstallAndApplyAsync(apkPath, confirmed: true);
    }

    private async void OnApplyClick(object sender, RoutedEventArgs args)
        => await ViewModel.ApplyAsync();

    private async void OnRestoreClick(object sender, RoutedEventArgs args)
        => await ViewModel.OpenHomeSettingsAsync();

    private void SelectTheme(string theme)
    {
        foreach (var item in ThemeSelector.Items.OfType<ComboBoxItem>())
        {
            if (string.Equals(item.Tag as string, theme, StringComparison.Ordinal))
            {
                ThemeSelector.SelectedItem = item;
                return;
            }
        }
        ThemeSelector.SelectedIndex = 0;
    }
}
