using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Tervia.App.ViewModels;
using Tervia.Core;
using Windows.Storage.Streams;

namespace Tervia.App.Pages;

public sealed partial class DevicesPage : Page
{
    private string? _screenshotDeviceId;

    public DevicesPage(DevicesViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        DeviceList.SelectedItem = ViewModel.Shell.SelectedDevice;
        ViewModel.Shell.Devices.CollectionChanged += (_, _) => RefreshDetails();
        ViewModel.Shell.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ShellViewModel.SelectedDevice))
            {
                DeviceList.SelectedItem = ViewModel.Shell.SelectedDevice;
                RefreshDetails();
            }
        };
        RefreshDetails();
    }

    public DevicesViewModel ViewModel { get; }

    private void OnDeviceSelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (DeviceList.SelectedItem is TvDevice selected)
        {
            ViewModel.Shell.SelectedDevice = selected;
        }
        RefreshDetails();
    }

    private async void OnRemoveClick(object sender, RoutedEventArgs args)
    {
        if (DeviceList.SelectedItem is not TvDevice selected)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "Quitar perfil local",
            Content = $"Se quitará {selected.FriendlyName} de la biblioteca de este equipo. No se enviará ninguna operación al televisor.",
            PrimaryButtonText = "Quitar perfil",
            CloseButtonText = "Cancelar",
            DefaultButton = ContentDialogButton.None,
            XamlRoot = XamlRoot,
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary)
        {
            return;
        }

        try
        {
            await ViewModel.Shell.RemoveDeviceAsync(selected);
            ErrorText.Text = string.Empty;
            ErrorText.Visibility = Visibility.Collapsed;
        }
        catch (Exception exception)
        {
            ErrorText.Text = $"No se pudo actualizar la biblioteca local: {exception.Message}";
            ErrorText.Visibility = Visibility.Visible;
        }
    }

    private async void OnCaptureClick(object sender, RoutedEventArgs args)
    {
        if (ViewModel.Shell.SelectedDevice is not { } selected || !CanCapture(selected.Platform))
        {
            return;
        }

        CaptureButton.IsEnabled = false;
        DeviceList.IsEnabled = false;
        CaptureButton.Content = "Conectando…";
        CaptureProgress.IsActive = true;
        CaptureProgress.Visibility = Visibility.Visible;
        ScreenshotInfo.Visibility = Visibility.Collapsed;
        ScreenshotImage.Source = null;
        ScreenshotFrame.Visibility = Visibility.Collapsed;
        _screenshotDeviceId = null;
        ErrorText.Text = string.Empty;
        ErrorText.Visibility = Visibility.Collapsed;

        try
        {
            var capture = await ViewModel.CaptureScreenshotAsync(selected, CancellationToken.None);
            ScreenshotImage.Source = await DecodeScreenshotAsync(capture.Screenshot.PngData);
            ScreenshotFrame.Visibility = Visibility.Visible;
            ScreenshotInfo.Text = $"Capturada ahora: {capture.Screenshot.Width} × {capture.Screenshot.Height}. Se muestra en esta ventana y no se guarda en el perfil.";
            ScreenshotInfo.Visibility = Visibility.Visible;
            _screenshotDeviceId = selected.Id;

            var updated = selected with { Capabilities = capture.Capabilities };
            try
            {
                await ViewModel.Shell.SaveDeviceAsync(updated);
            }
            catch
            {
                ScreenshotInfo.Text += " No se pudo guardar el estado de capacidad en la biblioteca local.";
            }
        }
        catch (AuthenticationRequiredException exception)
        {
            ShowCaptureError(exception.Message);
        }
        catch (PlatformToolMissingException)
        {
            ShowCaptureError("No se encontró adb.exe. Instala Android SDK Platform-Tools y añade la carpeta platform-tools al PATH.");
        }
        catch (TvNotFoundException)
        {
            ShowCaptureError("No se alcanzó el televisor. Comprueba que siga encendido, en la misma red y con depuración ADB por red activada en el puerto 5555.");
        }
        catch (OperationRejectedException exception)
        {
            ShowCaptureError(exception.Message);
        }
        catch (PlatformToolFailureException)
        {
            ShowCaptureError("ADB no pudo completar la captura. La TV puede rechazar capturas en contenido protegido, HDMI o Live TV.");
        }
        catch (Exception)
        {
            ShowCaptureError("No se pudo capturar la pantalla. Comprueba la conexión ADB del televisor e inténtalo de nuevo.");
        }
        finally
        {
            CaptureProgress.IsActive = false;
            CaptureProgress.Visibility = Visibility.Collapsed;
            CaptureButton.Content = "Conectar y capturar";
            DeviceList.IsEnabled = true;
            CaptureButton.IsEnabled = CanCapture(ViewModel.Shell.SelectedDevice?.Platform);
        }
    }

    private void ShowCaptureError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }

    private static bool CanCapture(TvPlatform? platform)
        => platform is TvPlatform.AndroidTV or TvPlatform.GoogleTV or TvPlatform.FireTV;

    private static async Task<BitmapImage> DecodeScreenshotAsync(ReadOnlyMemory<byte> pngData)
    {
        using var stream = new InMemoryRandomAccessStream();
        using (var output = stream.GetOutputStreamAt(0))
        using (var writer = new DataWriter(output))
        {
            writer.WriteBytes(pngData.ToArray());
            await writer.StoreAsync();
            writer.DetachStream();
        }

        stream.Seek(0);
        var image = new BitmapImage();
        await image.SetSourceAsync(stream);
        return image;
    }

    private void RefreshDetails()
    {
        var selected = ViewModel.Shell.SelectedDevice;
        if (!string.Equals(_screenshotDeviceId, selected?.Id, StringComparison.OrdinalIgnoreCase))
        {
            ScreenshotImage.Source = null;
            ScreenshotFrame.Visibility = Visibility.Collapsed;
            ScreenshotInfo.Visibility = Visibility.Collapsed;
            _screenshotDeviceId = null;
        }

        SelectedName.Text = selected?.FriendlyName ?? "Selecciona un televisor";
        SelectedAddress.Text = selected?.NetworkAddress ?? string.Empty;
        SelectedDetails.Text = selected is null
            ? "Añade una TV física desde la sección Descubrir."
            : $"Plataforma: {selected.Platform}\nEstado: {selected.Status}\nFabricante/modelo: {selected.Manufacturer ?? "Sin confirmar"} / {selected.Model ?? "Sin confirmar"}\nSala: {selected.Room ?? "Sin asignar"}\nFunciones verificadas en este dispositivo: {selected.Capabilities.All.Count(capability => capability.Status == Tervia.Capabilities.CapabilityStatus.Supported && capability.Confidence == Tervia.Capabilities.CapabilityConfidence.Verified)}";
        RemoveButton.IsEnabled = selected is not null;
        CaptureButton.IsEnabled = CanCapture(selected?.Platform);
    }
}
