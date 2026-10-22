using System.Collections.ObjectModel;
using Tervia.Application;
using Tervia.Core;

namespace Tervia.App.ViewModels;

public sealed class HomeViewModel : ViewModelBase
{
    private readonly TvHomeService _home;
    private int _loadGeneration;
    private TvHomeProfile _lastLoadedProfile = new();
    private bool _applicationInventoryLoaded;
    private bool _isBusy;
    private string _status = "Selecciona una TV Android para configurar Tervia Home.";
    private string? _error;
    private string _title = "Mi televisión";
    private string _subtitle = TvHomeProfile.DefaultSubtitle;
    private string _applicationsTitle = "Tus aplicaciones";
    private string _theme = "ocean";
    private string _cardLayout = "comfortable";
    private bool _showClock = true;
    private bool _showBranding = true;

    public HomeViewModel(ShellViewModel shell, TvHomeService home)
    {
        Shell = shell;
        _home = home;
    }

    public ShellViewModel Shell { get; }

    public ObservableCollection<HomeApplicationOption> Applications { get; } = [];
    public IReadOnlyList<HomeApplicationOption> VisibleApplications
        => Applications
            .Select((application, index) => (application, index))
            .Where(item => item.application.IsVisible)
            .OrderBy(item => item.application.IsPinned ? 0 : 1)
            .ThenBy(item => item.application.IsPinned ? item.index : 0)
            .ThenBy(item => item.application.IsPinned ? string.Empty : item.application.DisplayName,
                StringComparer.CurrentCultureIgnoreCase)
            .Select(item => item.application)
            .ToArray();
    public bool HasApplicationInventory => _applicationInventoryLoaded;

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (_isBusy == value) return;
            _isBusy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanInstall));
            OnPropertyChanged(nameof(CanSync));
            OnPropertyChanged(nameof(CanRestore));
            OnPropertyChanged(nameof(CanSaveLocally));
        }
    }

    public bool IsHomeInstalled { get; private set; }
    public bool CanInstall => !IsBusy && IsSupportedTv;
    public bool CanSync => !IsBusy && IsSupportedTv && IsHomeInstalled;
    public bool CanRestore => !IsBusy && IsSupportedTv && IsHomeInstalled;
    public bool CanSaveLocally => !IsBusy && Shell.SelectedDevice is not null;

    public bool IsSupportedTv => Shell.SelectedDevice?.Platform is TvPlatform.AndroidTV or TvPlatform.GoogleTV or TvPlatform.FireTV;

    public string Status
    {
        get => _status;
        private set { _status = value; OnPropertyChanged(); }
    }

    public string? Error
    {
        get => _error;
        private set { _error = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(Error);

    public string Title
    {
        get => _title;
        set { if (_title == value) return; _title = value; OnPropertyChanged(); }
    }

    public string Subtitle
    {
        get => _subtitle;
        set { if (_subtitle == value) return; _subtitle = value; OnPropertyChanged(); }
    }

    public string ApplicationsTitle
    {
        get => _applicationsTitle;
        set { if (_applicationsTitle == value) return; _applicationsTitle = value; OnPropertyChanged(); }
    }

    public string Theme
    {
        get => _theme;
        set
        {
            var normalized = TvHomeProfile.SupportedThemes.Contains(value) ? value : "ocean";
            if (_theme == normalized) return;
            _theme = normalized;
            OnPropertyChanged();
        }
    }

    public string CardLayout
    {
        get => _cardLayout;
        set
        {
            if (_cardLayout == value) return;
            _cardLayout = TvHomeProfile.SupportedCardLayouts.Contains(value) ? value : "comfortable";
            OnPropertyChanged();
        }
    }

    public bool ShowClock
    {
        get => _showClock;
        set
        {
            if (_showClock == value) return;
            _showClock = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowClockLabel));
        }
    }

    public string ShowClockLabel => ShowClock ? "Reloj: visible" : "Reloj: oculto";

    public bool ShowBranding
    {
        get => _showBranding;
        set
        {
            if (_showBranding == value) return;
            _showBranding = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowBrandingLabel));
        }
    }

    public string ShowBrandingLabel => ShowBranding ? "Marca Tervia: visible" : "Marca Tervia: oculta";

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var generation = Interlocked.Increment(ref _loadGeneration);
        if (Shell.SelectedDevice is not { } device)
        {
            IsBusy = false;
            ClearApplications();
            ApplyProfileToEditor(new TvHomeProfile());
            IsHomeInstalled = false;
            Error = null;
            Status = "Añade y selecciona una TV Android para preparar la pantalla de inicio.";
            OnPropertyChanged(nameof(IsSupportedTv));
            NotifyHomeActionsChanged();
            return;
        }

        if (!IsSupportedTv)
        {
            ClearApplications();
            IsHomeInstalled = false;
            Error = null;
            IsBusy = true;
            Status = "Cargando el perfil local de esta TV…";
            OnPropertyChanged(nameof(IsSupportedTv));
            try
            {
                var profile = await _home.LoadProfileAsync(device, cancellationToken);
                if (generation != Volatile.Read(ref _loadGeneration)) return;
                ApplyProfileToEditor(profile);
                Status = "El perfil queda guardado en este equipo. El launcher de esta plataforma no se modifica.";
            }
            catch (Exception exception)
            {
                Error = GetUserError(exception);
                Status = "No se pudo leer el perfil local de esta TV.";
            }
            finally
            {
                if (generation == Volatile.Read(ref _loadGeneration))
                {
                    IsBusy = false;
                    NotifyHomeActionsChanged();
                }
            }
            return;
        }

        IsHomeInstalled = false;
        NotifyHomeActionsChanged();
        IsBusy = true;
        Error = null;
        Status = $"Leyendo las aplicaciones instaladas en {device.FriendlyName}…";
        OnPropertyChanged(nameof(IsSupportedTv));
        ClearApplications();
        try
        {
            var profile = await _home.LoadProfileAsync(device, cancellationToken);
            if (generation != Volatile.Read(ref _loadGeneration)) return;
            ApplyProfileToEditor(profile);
            var installedApps = await _home.ListApplicationsAsync(device, cancellationToken);
            if (generation != Volatile.Read(ref _loadGeneration)) return;
            IsHomeInstalled = installedApps.Any(item => string.Equals(item.Id, "com.tervia.home", StringComparison.Ordinal));
            NotifyHomeActionsChanged();
            var pinned = new HashSet<string>(profile.PinnedApplicationIds, StringComparer.Ordinal);
            var hidden = new HashSet<string>(profile.HiddenApplicationIds, StringComparer.Ordinal);
            var pinnedOrder = profile.PinnedApplicationIds
                .Select((id, index) => (id, index))
                .ToDictionary(item => item.id, item => item.index, StringComparer.Ordinal);
            ClearApplications();
            foreach (var app in installedApps
                         .OrderBy(item => pinnedOrder.GetValueOrDefault(item.Id, int.MaxValue))
                         .ThenBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase))
            {
                if (app.Id != "com.tervia.home")
                {
                    var option = new HomeApplicationOption(app.Id, app.Name, !hidden.Contains(app.Id), pinned.Contains(app.Id));
                    option.PropertyChanged += OnApplicationPropertyChanged;
                    Applications.Add(option);
                }
            }
            _applicationInventoryLoaded = true;
            OnPropertyChanged(nameof(HasApplicationInventory));
            RefreshPinnedMoveAvailability();
            OnPropertyChanged(nameof(VisibleApplications));
            Status = Applications.Count == 0
                ? IsHomeInstalled
                    ? "Tervia Home está instalada. No se encontraron aplicaciones de terceros para mostrar."
                    : "No hay aplicaciones de terceros detectadas. Puedes instalar Tervia Home y elegir la pantalla en la TV."
                : $"{Applications.Count} aplicaciones detectadas. Ajusta el diseño y sincronízalo con la TV.";
        }
        catch (Exception exception)
        {
            Error = GetUserError(exception);
            Status = "No se pudo leer la configuración de esta TV.";
        }
        finally
        {
            if (generation == Volatile.Read(ref _loadGeneration))
            {
                IsBusy = false;
                NotifyHomeActionsChanged();
            }
        }
    }

    public void ToggleVisibility(HomeApplicationOption application)
    {
        if (!Applications.Contains(application)) return;
        application.IsVisible = !application.IsVisible;
        if (!application.IsVisible)
        {
            application.IsPinned = false;
        }
    }

    public void TogglePinned(HomeApplicationOption application)
    {
        if (!Applications.Contains(application)) return;
        var shouldPin = !application.IsPinned;
        application.IsPinned = shouldPin;
        if (!shouldPin) return;

        var lastPinned = Applications.LastOrDefault(item => item != application && item.IsVisible && item.IsPinned);
        var targetIndex = lastPinned is null ? 0 : Applications.IndexOf(lastPinned) + 1;
        var currentIndex = Applications.IndexOf(application);
        if (currentIndex != targetIndex)
        {
            Applications.Move(currentIndex, targetIndex);
        }
        OnPropertyChanged(nameof(VisibleApplications));
        RefreshPinnedMoveAvailability();
    }

    public void MovePinnedApplication(HomeApplicationOption application, int direction)
    {
        if (!Applications.Contains(application) || !application.IsPinned || !application.IsVisible || Math.Abs(direction) != 1)
        {
            return;
        }

        var pinned = Applications.Where(item => item.IsVisible && item.IsPinned).ToList();
        var pinnedIndex = pinned.IndexOf(application);
        var targetPinnedIndex = pinnedIndex + direction;
        if (targetPinnedIndex < 0 || targetPinnedIndex >= pinned.Count) return;

        var currentIndex = Applications.IndexOf(application);
        var targetIndex = Applications.IndexOf(pinned[targetPinnedIndex]);
        Applications.Move(currentIndex, targetIndex);
        OnPropertyChanged(nameof(VisibleApplications));
        RefreshPinnedMoveAvailability();
    }

    public async Task SaveLocallyAsync(CancellationToken cancellationToken = default)
    {
        if (Shell.SelectedDevice is not { } device)
        {
            Error = "Selecciona primero una TV para guardar su diseño local.";
            return;
        }

        IsBusy = true;
        Error = null;
        Status = "Guardando el diseño solo en este equipo…";
        try
        {
            var profile = BuildProfile().ForDevice(device);
            await _home.SaveProfileLocallyAsync(device, profile, cancellationToken);
            _lastLoadedProfile = profile;
            ApplyProfileToEditor(profile);
            Status = "Diseño guardado en este equipo. Aún no se han enviado cambios a la TV.";
        }
        catch (Exception)
        {
            Error = "No se pudo guardar el diseño local en este equipo.";
            Status = "No se completó el guardado local.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearApplications()
    {
        _applicationInventoryLoaded = false;
        OnPropertyChanged(nameof(HasApplicationInventory));
        foreach (var application in Applications)
        {
            application.PropertyChanged -= OnApplicationPropertyChanged;
        }
        Applications.Clear();
        OnPropertyChanged(nameof(VisibleApplications));
    }

    private void ApplyProfileToEditor(TvHomeProfile profile)
    {
        Title = profile.Title;
        Subtitle = profile.Subtitle;
        ApplicationsTitle = profile.ApplicationsTitle;
        Theme = profile.Theme;
        CardLayout = profile.CardLayout;
        ShowClock = profile.ShowClock;
        ShowBranding = profile.ShowBranding;
        _lastLoadedProfile = profile;
    }

    private void OnApplicationPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(HomeApplicationOption.IsPinned) or nameof(HomeApplicationOption.IsVisible))
        {
            OnPropertyChanged(nameof(VisibleApplications));
            RefreshPinnedMoveAvailability();
        }
    }

    private void RefreshPinnedMoveAvailability()
    {
        var pinned = Applications.Where(item => item.IsVisible && item.IsPinned).ToList();
        for (var index = 0; index < pinned.Count; index++)
        {
            pinned[index].SetMoveAvailability(index > 0, index < pinned.Count - 1);
        }
        foreach (var unpinned in Applications.Where(item => !item.IsPinned || !item.IsVisible))
        {
            unpinned.SetMoveAvailability(false, false);
        }
    }

    public async Task InstallAndApplyAsync(string apkPath, bool confirmed, CancellationToken cancellationToken = default)
    {
        if (Shell.SelectedDevice is not { } device)
        {
            Error = "Selecciona primero una TV.";
            return;
        }
        IsBusy = true;
        Error = null;
        Status = "Instalando Tervia Home y enviando tu diseño…";
        try
        {
            var profile = BuildProfile().ForDevice(device);
            ApplyProfileToEditor(profile);
            await _home.InstallAndApplyAsync(device, profile, apkPath, confirmed, cancellationToken);
            _lastLoadedProfile = profile;
            Status = "Tervia Home se abrió en la TV. Confirma allí si quieres establecerla como inicio.";
            IsHomeInstalled = true;
            NotifyHomeActionsChanged();
        }
        catch (Exception exception)
        {
            Error = GetUserError(exception);
            Status = "No se completó la instalación.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        if (Shell.SelectedDevice is not { } device)
        {
            Error = "Selecciona primero una TV.";
            return;
        }
        IsBusy = true;
        Error = null;
        Status = "Sincronizando el diseño…";
        try
        {
            var profile = BuildProfile().ForDevice(device);
            await _home.ApplyAsync(device, profile, cancellationToken);
            _lastLoadedProfile = profile;
            Status = "Diseño guardado en esta TV y en este equipo.";
        }
        catch (Exception exception)
        {
            Error = GetUserError(exception);
            Status = "No se pudo sincronizar el diseño.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task OpenHomeSettingsAsync(CancellationToken cancellationToken = default)
    {
        if (Shell.SelectedDevice is not { } device)
        {
            Error = "Selecciona primero una TV.";
            return;
        }
        IsBusy = true;
        Error = null;
        Status = "Abriendo la selección de pantalla de inicio en la TV…";
        try
        {
            await _home.OpenHomeSettingsAsync(device, cancellationToken);
            Status = "En la TV, selecciona el launcher original para restaurarlo o Tervia Home para activarlo.";
        }
        catch (Exception exception)
        {
            Error = GetUserError(exception);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private TvHomeProfile BuildProfile()
        => new TvHomeProfile
        {
            Title = Title,
            Subtitle = Subtitle,
            ApplicationsTitle = ApplicationsTitle,
            Theme = Theme,
            CardLayout = CardLayout,
            ShowClock = ShowClock,
            ShowBranding = ShowBranding,
            PinnedApplicationIds = _applicationInventoryLoaded
                ? Applications.Where(item => item.IsVisible && item.IsPinned).Select(item => item.Id).ToArray()
                : _lastLoadedProfile.PinnedApplicationIds,
            HiddenApplicationIds = _applicationInventoryLoaded
                ? Applications.Where(item => !item.IsVisible).Select(item => item.Id).ToArray()
                : _lastLoadedProfile.HiddenApplicationIds,
        }.Normalize();

    private void NotifyHomeActionsChanged()
    {
        OnPropertyChanged(nameof(IsHomeInstalled));
        OnPropertyChanged(nameof(CanInstall));
        OnPropertyChanged(nameof(CanSync));
        OnPropertyChanged(nameof(CanRestore));
        OnPropertyChanged(nameof(CanSaveLocally));
    }

    private static string GetUserError(Exception exception)
        => exception switch
        {
            FileNotFoundException => "No se encontró el APK de Tervia Home. Usa la carpeta completa de la versión o compila el componente Android.",
            PlatformToolMissingException => "No se encontró adb.exe. Instala Android SDK Platform-Tools, comprueba su ubicación o añade platform-tools al PATH.",
            AuthenticationRequiredException => "Acepta el aviso de depuración en la TV y vuelve a intentarlo.",
            TvNotFoundException => "No se alcanzó la TV. Comprueba que esté encendida, en la misma red y con ADB autorizado.",
            UnsupportedCapabilityException => "Esta TV no expone la integración de inicio personalizada.",
            PlatformToolFailureException platform => $"La TV rechazó la operación ADB: {platform.Message}",
            OperationRejectedException rejected => rejected.Message,
            _ => "No se pudo completar la operación. Comprueba la conexión ADB y vuelve a intentarlo.",
        };
}
