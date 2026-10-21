using System.Collections.ObjectModel;
using Tervia.Application;
using Tervia.Core;

namespace Tervia.App.ViewModels;

public sealed class HomeViewModel : ViewModelBase
{
    private readonly TvHomeService _home;
    private int _loadGeneration;
    private bool _isBusy;
    private string _status = "Selecciona una TV Android para configurar Tervia Home.";
    private string? _error;
    private string _title = "Mi televisión";
    private string _subtitle = TvHomeProfile.DefaultSubtitle;
    private string _theme = "ocean";
    private bool _showClock = true;

    public HomeViewModel(ShellViewModel shell, TvHomeService home)
    {
        Shell = shell;
        _home = home;
    }

    public ShellViewModel Shell { get; }

    public ObservableCollection<HomeApplicationOption> Applications { get; } = [];

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
        }
    }

    public bool IsHomeInstalled { get; private set; }
    public bool CanInstall => !IsBusy && IsSupportedTv;
    public bool CanSync => !IsBusy && IsSupportedTv && IsHomeInstalled;
    public bool CanRestore => !IsBusy && IsSupportedTv && IsHomeInstalled;

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

    public string Theme
    {
        get => _theme;
        set { if (_theme == value) return; _theme = value; OnPropertyChanged(); }
    }

    public bool ShowClock
    {
        get => _showClock;
        set { if (_showClock == value) return; _showClock = value; OnPropertyChanged(); }
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var generation = Interlocked.Increment(ref _loadGeneration);
        if (Shell.SelectedDevice is not { } device)
        {
            IsBusy = false;
            Applications.Clear();
            IsHomeInstalled = false;
            Error = null;
            Status = "Añade y selecciona una TV Android para preparar la pantalla de inicio.";
            OnPropertyChanged(nameof(IsSupportedTv));
            NotifyHomeActionsChanged();
            return;
        }

        if (!IsSupportedTv)
        {
            IsBusy = false;
            Applications.Clear();
            IsHomeInstalled = false;
            Error = null;
            Status = "La personalización de inicio está disponible para Android TV, Google TV y televisores Fire TV compatibles. El inicio de otros sistemas permanece intacto.";
            OnPropertyChanged(nameof(IsSupportedTv));
            NotifyHomeActionsChanged();
            return;
        }

        IsHomeInstalled = false;
        NotifyHomeActionsChanged();
        IsBusy = true;
        Error = null;
        Status = $"Leyendo las aplicaciones instaladas en {device.FriendlyName}…";
        OnPropertyChanged(nameof(IsSupportedTv));
        try
        {
            var profile = await _home.LoadProfileAsync(device, cancellationToken);
            if (generation != Volatile.Read(ref _loadGeneration)) return;
            var installedApps = await _home.ListApplicationsAsync(device, cancellationToken);
            if (generation != Volatile.Read(ref _loadGeneration)) return;
            IsHomeInstalled = installedApps.Any(item => string.Equals(item.Id, "com.tervia.home", StringComparison.Ordinal));
            NotifyHomeActionsChanged();
            Title = profile.Title;
            Subtitle = profile.Subtitle;
            Theme = profile.Theme;
            ShowClock = profile.ShowClock;
            var pinned = new HashSet<string>(profile.PinnedApplicationIds, StringComparer.Ordinal);
            var hidden = new HashSet<string>(profile.HiddenApplicationIds, StringComparer.Ordinal);
            Applications.Clear();
            foreach (var app in installedApps.OrderBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase))
            {
                if (app.Id != "com.tervia.home")
                {
                    Applications.Add(new HomeApplicationOption(app.Id, app.Name, !hidden.Contains(app.Id), pinned.Contains(app.Id)));
                }
            }
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
            var profile = BuildProfile();
            await _home.InstallAndApplyAsync(device, profile, apkPath, confirmed, cancellationToken);
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
            await _home.ApplyAsync(device, BuildProfile(), cancellationToken);
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
            Theme = Theme,
            ShowClock = ShowClock,
            PinnedApplicationIds = Applications.Where(item => item.IsVisible && item.IsPinned).Select(item => item.Id).ToArray(),
            HiddenApplicationIds = Applications.Where(item => !item.IsVisible).Select(item => item.Id).ToArray(),
        }.Normalize();

    private void NotifyHomeActionsChanged()
    {
        OnPropertyChanged(nameof(IsHomeInstalled));
        OnPropertyChanged(nameof(CanInstall));
        OnPropertyChanged(nameof(CanSync));
        OnPropertyChanged(nameof(CanRestore));
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
