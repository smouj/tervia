using System.Collections.ObjectModel;
using Tervia.Application;
using Tervia.Core;

namespace Tervia.App.ViewModels;

public sealed class ShellViewModel : ViewModelBase
{
    private readonly ITvDeviceLibrary _library;
    private TvDevice? _selectedDevice;
    private string? _errorMessage;
    private bool _isLoading = true;

    public ShellViewModel(ITvDeviceLibrary library) => _library = library;

    public ObservableCollection<TvDevice> Devices { get; } = [];

    public bool HasDevices => Devices.Count > 0;

    public TvDevice? SelectedDevice
    {
        get => _selectedDevice;
        set
        {
            if (ReferenceEquals(_selectedDevice, value))
            {
                return;
            }

            _selectedDevice = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedPlatformLabel));
            OnPropertyChanged(nameof(SelectedStatusLabel));
            OnPropertyChanged(nameof(HasSelectedDevice));
        }
    }

    public string SelectedPlatformLabel => SelectedDevice?.Platform.ToString() ?? "Sin TV seleccionada";

    public string SelectedStatusLabel => SelectedDevice?.Status.ToString() ?? string.Empty;

    public bool HasSelectedDevice => SelectedDevice is not null;

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (_errorMessage == value)
            {
                return;
            }
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (_isLoading == value)
            {
                return;
            }
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var devices = await _library.LoadAsync(cancellationToken);
            Devices.Clear();
            foreach (var device in devices.OrderBy(item => item.FriendlyName, StringComparer.CurrentCultureIgnoreCase))
            {
                Devices.Add(device);
            }
            OnPropertyChanged(nameof(HasDevices));
            SelectedDevice = Devices.FirstOrDefault();
            ErrorMessage = null;
        }
        catch (Exception exception)
        {
            ErrorMessage = $"No se pudo leer la biblioteca local: {exception.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task SaveDeviceAsync(TvDevice device, CancellationToken cancellationToken = default)
    {
        await _library.SaveAsync(device, cancellationToken);
        var index = -1;
        for (var itemIndex = 0; itemIndex < Devices.Count; itemIndex++)
        {
            if (string.Equals(Devices[itemIndex].Id, device.Id, StringComparison.OrdinalIgnoreCase))
            {
                index = itemIndex;
                break;
            }
        }

        if (index < 0)
        {
            Devices.Add(device);
            OnPropertyChanged(nameof(HasDevices));
        }
        else
        {
            Devices[index] = device;
        }

        SelectedDevice = device;
        ErrorMessage = null;
    }

    public async Task RemoveDeviceAsync(TvDevice device, CancellationToken cancellationToken = default)
    {
        await _library.RemoveAsync(device.Id, cancellationToken);
        var existing = Devices.FirstOrDefault(item => string.Equals(item.Id, device.Id, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            Devices.Remove(existing);
        }
        OnPropertyChanged(nameof(HasDevices));
        SelectedDevice = Devices.FirstOrDefault();
    }
}
