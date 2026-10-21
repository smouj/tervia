using System.Net;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Tervia.App.ViewModels;
using Tervia.Core;

namespace Tervia.App.Pages;

public sealed partial class DiscoveryPage : Page
{
    public DiscoveryPage(DiscoveryViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        Unloaded += (_, _) => ViewModel.CancelScan();
    }

    public DiscoveryViewModel ViewModel { get; }

    private async void OnScanClick(object sender, RoutedEventArgs args)
        => await ViewModel.ScanAsync();

    private void OnCancelClick(object sender, RoutedEventArgs args)
        => ViewModel.CancelScan();

    private async void OnSaveCandidateClick(object sender, RoutedEventArgs args)
    {
        var candidate = ViewModel.SelectedCandidate;
        if (candidate is null || !await ConfirmPhysicalTelevisionAsync(candidate))
        {
            return;
        }

        await ViewModel.AddCandidateAsync(candidate, GetSelectedPlatform(), physicalTelevisionConfirmed: true);
    }

    private async void OnAddManualClick(object sender, RoutedEventArgs args)
    {
        var address = ManualAddressBox.Text.Trim();
        if (!IPAddress.TryParse(address, out var parsedAddress))
        {
            await ShowValidationErrorAsync("Escribe una dirección IPv4 o IPv6 válida. No se admiten nombres DNS.");
            return;
        }

        var normalized = parsedAddress.ToString();
        var manualCandidate = new TvCandidate
        {
            Name = normalized,
            Address = normalized,
            DiscoveryProtocol = "manual",
            Confidence = 0.5,
        };
        if (!await ConfirmPhysicalTelevisionAsync(manualCandidate))
        {
            return;
        }

        if (await ViewModel.AddManualAddressAsync(address, GetSelectedPlatform(), physicalTelevisionConfirmed: true))
        {
            ManualAddressBox.Text = string.Empty;
        }
    }

    private async Task<bool> ConfirmPhysicalTelevisionAsync(TvCandidate candidate)
    {
        var hint = candidate.PlatformHint == TvPlatform.Unknown
            ? "La plataforma todavía no está verificada."
            : $"El anuncio sugiere {candidate.PlatformHint}, pero ese dato no confirma el tipo de dispositivo.";
        var dialog = new ContentDialog
        {
            Title = "Confirma el televisor físico",
            Content = $"Nombre anunciado: {candidate.Name}\nDirección local: {candidate.Address}\n\n{hint}\n\n¿Has comprobado que esta dirección corresponde a un televisor físico que tienes autorización para administrar? Guardarlo no inicia una conexión ni verifica capacidades.",
            PrimaryButtonText = "Confirmar y guardar",
            CloseButtonText = "Cancelar",
            DefaultButton = ContentDialogButton.None,
            XamlRoot = XamlRoot,
        };
        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }

    private async Task ShowValidationErrorAsync(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "Dirección no válida",
            Content = message,
            CloseButtonText = "Entendido",
            DefaultButton = ContentDialogButton.None,
            XamlRoot = XamlRoot,
        };
        await dialog.ShowAsync();
    }

    private TvPlatform GetSelectedPlatform()
    {
        if (PlatformPicker.SelectedItem is ComboBoxItem { Tag: string tag } &&
            Enum.TryParse<TvPlatform>(tag, ignoreCase: false, out var platform))
        {
            return platform;
        }
        return TvPlatform.Unknown;
    }
}
