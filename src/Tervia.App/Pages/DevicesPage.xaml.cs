using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Tervia.App.ViewModels;
using Tervia.Core;

namespace Tervia.App.Pages;

public sealed partial class DevicesPage : Page
{
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

    private void RefreshDetails()
    {
        var selected = ViewModel.Shell.SelectedDevice;
        SelectedName.Text = selected?.FriendlyName ?? "Selecciona un televisor";
        SelectedAddress.Text = selected?.NetworkAddress ?? string.Empty;
        SelectedDetails.Text = selected is null
            ? "Añade una TV física desde la sección Descubrir."
            : $"Plataforma: {selected.Platform}\nEstado: {selected.Status}\nFabricante/modelo: {selected.Manufacturer ?? "Sin confirmar"} / {selected.Model ?? "Sin confirmar"}\nSala: {selected.Room ?? "Sin asignar"}\nCapacidades: {selected.Capabilities.All.Count} registro(s)";
        RemoveButton.IsEnabled = selected is not null;
    }
}
