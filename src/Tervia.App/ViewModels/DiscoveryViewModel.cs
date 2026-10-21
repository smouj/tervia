using System.Collections.ObjectModel;
using System.Net;
using Tervia.Core;
using Tervia.Discovery;

namespace Tervia.App.ViewModels;

public sealed class DiscoveryViewModel : ViewModelBase
{
    private readonly TvDiscoveryService _discovery;
    private readonly ShellViewModel _shell;
    private CancellationTokenSource? _scanCancellation;
    private TvCandidate? _selectedCandidate;
    private bool _isScanning;
    private string _statusMessage = "La búsqueda consulta SSDP y mDNS en la red local. No se conecta a los televisores.";
    private string? _errorMessage;

    public DiscoveryViewModel(TvDiscoveryService discovery, ShellViewModel shell)
    {
        _discovery = discovery;
        _shell = shell;
    }

    public ObservableCollection<TvCandidate> Candidates { get; } = [];

    public ObservableCollection<string> Failures { get; } = [];

    public bool HasFailures => Failures.Count > 0;

    public TvCandidate? SelectedCandidate
    {
        get => _selectedCandidate;
        set
        {
            if (ReferenceEquals(_selectedCandidate, value))
            {
                return;
            }
            _selectedCandidate = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanAddSelectedCandidate));
        }
    }

    public bool CanAddSelectedCandidate => !IsScanning && SelectedCandidate is not null;

    public bool CanScan => !IsScanning;

    public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool IsScanning
    {
        get => _isScanning;
        private set
        {
            if (_isScanning == value)
            {
                return;
            }
            _isScanning = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanAddSelectedCandidate));
            OnPropertyChanged(nameof(CanScan));
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage == value)
            {
                return;
            }
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

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
            OnPropertyChanged(nameof(HasErrorMessage));
        }
    }

    public async Task ScanAsync()
    {
        if (IsScanning)
        {
            return;
        }

        var scanCancellation = new CancellationTokenSource();
        _scanCancellation = scanCancellation;
        IsScanning = true;
        ErrorMessage = null;
        Failures.Clear();
        OnPropertyChanged(nameof(HasFailures));
        Candidates.Clear();
        SelectedCandidate = null;
        StatusMessage = "Buscando dispositivos en la red local…";

        try
        {
            var report = await _discovery.DiscoverDetailedAsync(scanCancellation.Token);
            foreach (var candidate in report.Candidates)
            {
                Candidates.Add(candidate);
            }
            foreach (var failure in report.Failures)
            {
                Failures.Add($"{failure.Strategy}: {failure.Message}");
            }
            OnPropertyChanged(nameof(HasFailures));

            StatusMessage = Candidates.Count == 0
                ? "No se encontraron candidatos. Comprueba que el PC y el televisor estén en la misma red local."
                : $"{Candidates.Count} candidato(s). Selecciona uno y confirma que es un televisor físico antes de guardarlo.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Búsqueda cancelada.";
        }
        catch (Exception exception)
        {
            ErrorMessage = $"La búsqueda no pudo completarse: {exception.Message}";
            StatusMessage = "Revisa el acceso a la red local y vuelve a intentarlo.";
        }
        finally
        {
            if (ReferenceEquals(_scanCancellation, scanCancellation))
            {
                _scanCancellation = null;
            }
            scanCancellation.Dispose();
            IsScanning = false;
        }
    }

    public void CancelScan() => _scanCancellation?.Cancel();

    public async Task<bool> AddCandidateAsync(
        TvCandidate candidate,
        TvPlatform selectedPlatform,
        bool physicalTelevisionConfirmed)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        if (!physicalTelevisionConfirmed)
        {
            ErrorMessage = "Confirma que el dispositivo es un televisor físico antes de guardarlo.";
            return false;
        }

        if (!IPAddress.TryParse(candidate.Address, out var parsedAddress))
        {
            ErrorMessage = "La dirección detectada no es una dirección IP válida. Añade el televisor manualmente con su IP.";
            return false;
        }

        var normalizedAddress = parsedAddress.ToString();
        var profile = new TvDevice
        {
            Id = normalizedAddress,
            FriendlyName = string.IsNullOrWhiteSpace(candidate.Name) ? normalizedAddress : candidate.Name.Trim(),
            NetworkAddress = normalizedAddress,
            Platform = selectedPlatform,
            ConnectionMethod = TvConnectionMethod.Network,
            LastSeen = DateTimeOffset.UtcNow,
            Status = TvDeviceStatus.Unknown,
            Capabilities = Tervia.Capabilities.CapabilitySet.Empty,
        };

        try
        {
            await _shell.SaveDeviceAsync(profile);
            ErrorMessage = null;
            StatusMessage = $"{profile.FriendlyName} se guardó en esta biblioteca local. Aún no se ha conectado ni se han comprobado capacidades.";
            return true;
        }
        catch (Exception exception)
        {
            ErrorMessage = $"No se pudo guardar el perfil local: {exception.Message}";
            return false;
        }
    }

    public async Task<bool> AddManualAddressAsync(
        string address,
        TvPlatform selectedPlatform,
        bool physicalTelevisionConfirmed)
    {
        if (!IPAddress.TryParse(address.Trim(), out var parsedAddress))
        {
            ErrorMessage = "Escribe una dirección IPv4 o IPv6 válida; no se admiten nombres DNS.";
            return false;
        }

        var normalized = parsedAddress.ToString();
        var candidate = new TvCandidate
        {
            Name = normalized,
            Address = normalized,
            DiscoveryProtocol = "manual",
            Confidence = 0.5,
        };
        return await AddCandidateAsync(candidate, selectedPlatform, physicalTelevisionConfirmed);
    }
}
