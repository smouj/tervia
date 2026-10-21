using System.Collections.ObjectModel;
using Tervia.Capabilities;

namespace Tervia.App.ViewModels;

public sealed class DashboardViewModel : ViewModelBase
{
    private readonly ShellViewModel _shell;

    public DashboardViewModel(ShellViewModel shell)
    {
        _shell = shell;
        _shell.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ShellViewModel.SelectedDevice))
            {
                Refresh();
            }
            else if (args.PropertyName == nameof(ShellViewModel.ErrorMessage))
            {
                OnPropertyChanged(nameof(LibraryError));
                OnPropertyChanged(nameof(HasLibraryError));
            }
        };
        Refresh();
    }

    public ObservableCollection<CapabilityInfo> Capabilities { get; } = [];

    public bool HasDevice => _shell.SelectedDevice is not null;

    public string DeviceName => _shell.SelectedDevice?.FriendlyName ?? "Aún no hay televisores en la biblioteca";

    public string DeviceAddress => _shell.SelectedDevice?.NetworkAddress ?? "Añade un televisor desde Descubrir";

    public string DevicePlatform => _shell.SelectedDevice?.Platform.ToString() ?? "Sin verificar";

    public string DeviceStatus => _shell.SelectedDevice?.Status.ToString() ?? "Sin conexión";

    public string DeviceRoom => _shell.SelectedDevice?.Room ?? "Sala no indicada";

    public string LibraryError => _shell.ErrorMessage ?? string.Empty;

    public bool HasLibraryError => !string.IsNullOrWhiteSpace(_shell.ErrorMessage);

    public string SupportedSummary => $"{SupportedCount} comprobadas";

    public string NeedsSetupSummary => $"{NeedsSetupCount} requieren configuración";

    public string UnknownSummary => $"{UnknownCount} sin verificar";

    public int SupportedCount => Capabilities.Count(item => item.Status == CapabilityStatus.Supported);

    public int NeedsSetupCount => Capabilities.Count(item => item.Status == CapabilityStatus.Restricted);

    public int UnknownCount => Capabilities.Count(item => item.Status == CapabilityStatus.Unknown);

    public bool HasCapabilities => Capabilities.Count > 0;

    private void Refresh()
    {
        Capabilities.Clear();
        if (_shell.SelectedDevice is { } selected)
        {
            foreach (var capability in selected.Capabilities.All.OrderBy(item => item.Id.Value, StringComparer.Ordinal))
            {
                Capabilities.Add(capability);
            }
        }

        OnPropertyChanged(nameof(SupportedCount));
        OnPropertyChanged(nameof(NeedsSetupCount));
        OnPropertyChanged(nameof(UnknownCount));
        OnPropertyChanged(nameof(HasCapabilities));
        OnPropertyChanged(nameof(SupportedSummary));
        OnPropertyChanged(nameof(NeedsSetupSummary));
        OnPropertyChanged(nameof(UnknownSummary));
        OnPropertyChanged(nameof(HasDevice));
        OnPropertyChanged(nameof(DeviceName));
        OnPropertyChanged(nameof(DeviceAddress));
        OnPropertyChanged(nameof(DevicePlatform));
        OnPropertyChanged(nameof(DeviceStatus));
        OnPropertyChanged(nameof(DeviceRoom));
        OnPropertyChanged(nameof(LibraryError));
        OnPropertyChanged(nameof(HasLibraryError));
    }
}
