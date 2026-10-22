using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Tervia.App.ViewModels;

namespace Tervia.App.Pages;

public sealed partial class HomePage : Page
{
    private bool _isLoadingTheme;
    private bool _isLoadingCardLayout;
    private readonly DispatcherTimer _previewClockTimer = new() { Interval = TimeSpan.FromMinutes(1) };

    public HomePage(HomeViewModel viewModel)
    {
        ViewModel = viewModel;
        _isLoadingTheme = true;
        InitializeComponent();
        SelectTheme(ViewModel.Theme);
        SelectCardLayout(ViewModel.CardLayout);
        _isLoadingTheme = false;
        _isLoadingCardLayout = false;
        _previewClockTimer.Tick += OnPreviewClockTick;
        UpdatePreview();
    }

    public HomeViewModel ViewModel { get; }

    private async void OnLoaded(object sender, RoutedEventArgs args)
    {
        ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        ViewModel.Shell.PropertyChanged -= OnShellPropertyChanged;
        ViewModel.Shell.PropertyChanged += OnShellPropertyChanged;
        _isLoadingTheme = true;
        _isLoadingCardLayout = true;
        SelectTheme(ViewModel.Theme);
        SelectCardLayout(ViewModel.CardLayout);
        _isLoadingTheme = _isLoadingCardLayout = false;
        await ViewModel.LoadAsync();
        _isLoadingTheme = true;
        _isLoadingCardLayout = true;
        SelectTheme(ViewModel.Theme);
        SelectCardLayout(ViewModel.CardLayout);
        _isLoadingTheme = _isLoadingCardLayout = false;
        _previewClockTimer.Start();
        UpdatePreview();
        UpdateErrorBanner();
    }

    private void OnUnloaded(object sender, RoutedEventArgs args)
    {
        ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        ViewModel.Shell.PropertyChanged -= OnShellPropertyChanged;
        _previewClockTimer.Stop();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(HomeViewModel.Error) or nameof(HomeViewModel.HasError))
        {
            UpdateErrorBanner();
        }
        if (args.PropertyName == nameof(HomeViewModel.Theme))
        {
            _isLoadingTheme = true;
            SelectTheme(ViewModel.Theme);
            _isLoadingTheme = false;
            UpdatePreview();
        }
        if (args.PropertyName == nameof(HomeViewModel.CardLayout))
        {
            _isLoadingCardLayout = true;
            SelectCardLayout(ViewModel.CardLayout);
            _isLoadingCardLayout = false;
            UpdatePreview();
        }
        if (args.PropertyName is nameof(HomeViewModel.Title)
            or nameof(HomeViewModel.Subtitle)
            or nameof(HomeViewModel.ApplicationsTitle)
            or nameof(HomeViewModel.ShowClock)
            or nameof(HomeViewModel.ShowBranding)
            or nameof(HomeViewModel.VisibleApplications)
            or nameof(HomeViewModel.HasApplicationInventory))
        {
            UpdatePreview();
        }
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

    private void OnCardLayoutSelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (!_isLoadingCardLayout && CardLayoutSelector.SelectedItem is ComboBoxItem { Tag: string layout })
        {
            ViewModel.CardLayout = layout;
            UpdatePreview();
        }
    }

    private void OnPreviewTextChanged(object sender, TextChangedEventArgs args) => UpdatePreview();

    private void OnPreviewClockTick(object? sender, object args) => UpdatePreviewClock();

    private void OnClockToggleClick(object sender, RoutedEventArgs args)
    {
        ViewModel.ShowClock = !ViewModel.ShowClock;
        UpdatePreview();
    }

    private void OnBrandingToggleClick(object sender, RoutedEventArgs args)
    {
        ViewModel.ShowBranding = !ViewModel.ShowBranding;
        UpdatePreview();
    }

    private void OnVisibilityClick(object sender, RoutedEventArgs args)
    {
        if (sender is Button { DataContext: HomeApplicationOption application })
        {
            ViewModel.ToggleVisibility(application);
            UpdatePreview();
        }
    }

    private void OnPinClick(object sender, RoutedEventArgs args)
    {
        if (sender is Button { DataContext: HomeApplicationOption application })
        {
            ViewModel.TogglePinned(application);
            UpdatePreview();
        }
    }

    private void OnMoveUpClick(object sender, RoutedEventArgs args)
    {
        if (sender is Button { DataContext: HomeApplicationOption application })
        {
            ViewModel.MovePinnedApplication(application, -1);
            UpdatePreview();
        }
    }

    private void OnMoveDownClick(object sender, RoutedEventArgs args)
    {
        if (sender is Button { DataContext: HomeApplicationOption application })
        {
            ViewModel.MovePinnedApplication(application, 1);
            UpdatePreview();
        }
    }

    private void UpdateErrorBanner()
    {
        ErrorText.Text = ViewModel.Error ?? string.Empty;
        ErrorBanner.Visibility = ViewModel.HasError ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdatePreview()
    {
        var palette = GetPalette(ViewModel.Theme);
        var brush = new SolidColorBrush(palette.Accent);
        PreviewBackgroundStart.Color = palette.BackgroundStart;
        PreviewBackgroundMiddle.Color = palette.BackgroundMiddle;
        PreviewBackgroundEnd.Color = palette.BackgroundEnd;
        PreviewHeroStart.Color = palette.HeroStart;
        PreviewHeroEnd.Color = palette.HeroEnd;
        PreviewHero.BorderBrush = new SolidColorBrush(WithAlpha(palette.Accent, 85));
        PreviewEyebrow.Foreground = brush;
        PreviewTitle.Text = PreviewText(ViewModel.Title, "Mi televisión", 42);
        PreviewSubtitle.Text = PreviewText(ViewModel.Subtitle, Tervia.Core.TvHomeProfile.DefaultSubtitle, 88);
        PreviewApplicationsTitle.Text = PreviewText(ViewModel.ApplicationsTitle, "Tus aplicaciones", 36);
        PreviewInventoryBadge.Text = ViewModel.HasApplicationInventory ? "APPS DE LA TV" : "MUESTRA DE DISEÑO";
        PreviewInventoryBadge.Foreground = brush;
        PreviewBrand.Visibility = ViewModel.ShowBranding ? Visibility.Visible : Visibility.Collapsed;
        PreviewClock.Visibility = ViewModel.ShowClock ? Visibility.Visible : Visibility.Collapsed;
        PreviewHeader.Visibility = ViewModel.ShowBranding || ViewModel.ShowClock ? Visibility.Visible : Visibility.Collapsed;
        UpdatePreviewClock();
        RenderPreviewApplications(palette.Accent);
    }

    private void RenderPreviewApplications(Windows.UI.Color accent)
    {
        PreviewAppsPanel.Children.Clear();
        var visible = ViewModel.VisibleApplications;
        var tiles = ViewModel.HasApplicationInventory
            ? visible.Select(application => (Name: application.DisplayName, Glyph: application.PreviewGlyph)).ToList()
            : new List<(string Name, string Glyph)>
            {
                ("Netflix", "N"),
                ("YouTube", "YT"),
                ("Prime Video", "P"),
                ("Disney+", "D+"),
                ("Plex", "P"),
                ("Spotify", "S"),
            };
        var compact = ViewModel.CardLayout == "compact";
        var width = compact ? 110d : 132d;
        var height = compact ? 87d : 110d;
        foreach (var application in tiles.Take(6))
        {
            var card = new Border
            {
                Width = width,
                Height = height,
                Padding = new Thickness(compact ? 6 : 8),
                CornerRadius = new CornerRadius(11),
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0xCC, 0x10, 0x24, 0x3A)),
                BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(0x55, 0x4B, 0x75, 0x99)),
                BorderThickness = new Thickness(1),
            };
            var body = new StackPanel { Spacing = compact ? 4 : 6, VerticalAlignment = VerticalAlignment.Center };
            var glyph = new Border
            {
                Width = compact ? 32 : 40,
                Height = compact ? 32 : 40,
                CornerRadius = new CornerRadius(compact ? 9 : 11),
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0x24, accent.R, accent.G, accent.B)),
                BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(0xA0, accent.R, accent.G, accent.B)),
                BorderThickness = new Thickness(1),
                HorizontalAlignment = HorizontalAlignment.Center,
                Child = new TextBlock
                {
                    Text = application.Glyph,
                    FontSize = compact ? 12 : 15,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(accent),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            };
            var name = new TextBlock
            {
                Text = application.Name,
                FontSize = compact ? 9 : 10,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                MaxLines = 2,
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            body.Children.Add(glyph);
            body.Children.Add(name);
            card.Child = body;
            PreviewAppsPanel.Children.Add(card);
        }

        PreviewEmptyState.Visibility = ViewModel.HasApplicationInventory && tiles.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;
        if (tiles.Count > 6)
        {
            PreviewAppsPanel.Children.Add(new TextBlock
            {
                Text = $"+{tiles.Count - 6} más",
                FontSize = 10,
                Opacity = 0.76,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(3, 0, 0, 0),
            });
        }
    }

    private void UpdatePreviewClock()
        => PreviewClock.Text = DateTime.Now.ToString("HH:mm", System.Globalization.CultureInfo.CurrentCulture);

    private static string PreviewText(string? value, string fallback, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return fallback;
        var normalized = string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return normalized.Length == 0 ? fallback : normalized[..Math.Min(maxLength, normalized.Length)];
    }

    private static Windows.UI.Color WithAlpha(Windows.UI.Color color, byte alpha)
        => Windows.UI.Color.FromArgb(alpha, color.R, color.G, color.B);

    private static ThemePalette GetPalette(string theme)
        => theme switch
        {
            "violet" => Palette("#FF9B8CFF", "#FF100C20", "#FF241A46", "#FF17152F", "#E61E1947", "#CC181C3D"),
            "sunset" => Palette("#FFFFB45A", "#FF1E100D", "#FF442515", "#FF1E1828", "#E645291B", "#CC302131"),
            "forest" => Palette("#FF55D6A3", "#FF081A19", "#FF123A31", "#FF122320", "#E61A3B32", "#CC132D2A"),
            "rose" => Palette("#FFFF78A8", "#FF1B101C", "#FF412135", "#FF20182D", "#E6422035", "#CC2E1D31"),
            "gold" => Palette("#FFF2D45C", "#FF1C190C", "#FF433719", "#FF222013", "#E63B3217", "#CC292719"),
            "ice" => Palette("#FF7CCBFF", "#FF0A1722", "#FF16334A", "#FF111E32", "#E6173850", "#CC152D45"),
            "graphite" => Palette("#FFC1CBD6", "#FF111419", "#FF2B323B", "#FF191C22", "#E62D333D", "#CC232833"),
            _ => Palette("#FF39D8F2", "#FF071220", "#FF0A233E", "#FF0B192D", "#E6112F52", "#CC102447"),
        };

    private static ThemePalette Palette(
        string accent,
        string backgroundStart,
        string backgroundMiddle,
        string backgroundEnd,
        string heroStart,
        string heroEnd)
        => new(ToColor(accent), ToColor(backgroundStart), ToColor(backgroundMiddle), ToColor(backgroundEnd),
            ToColor(heroStart), ToColor(heroEnd));

    private static Windows.UI.Color ToColor(string hex)
    {
        var value = Convert.ToUInt32(hex[1..], 16);
        return Windows.UI.Color.FromArgb(
            (byte)(value >> 24),
            (byte)(value >> 16),
            (byte)(value >> 8),
            (byte)value);
    }

    private sealed record ThemePalette(
        Windows.UI.Color Accent,
        Windows.UI.Color BackgroundStart,
        Windows.UI.Color BackgroundMiddle,
        Windows.UI.Color BackgroundEnd,
        Windows.UI.Color HeroStart,
        Windows.UI.Color HeroEnd);

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

    private async void OnSaveLocalClick(object sender, RoutedEventArgs args)
        => await ViewModel.SaveLocallyAsync();

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

    private void SelectCardLayout(string layout)
    {
        foreach (var item in CardLayoutSelector.Items.OfType<ComboBoxItem>())
        {
            if (string.Equals(item.Tag as string, layout, StringComparison.Ordinal))
            {
                CardLayoutSelector.SelectedItem = item;
                return;
            }
        }
        CardLayoutSelector.SelectedIndex = 0;
    }
}
