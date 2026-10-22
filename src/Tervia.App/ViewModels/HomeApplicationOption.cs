using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Tervia.App.ViewModels;

public sealed class HomeApplicationOption(string id, string name, bool isVisible, bool isPinned) : INotifyPropertyChanged
{
    private static readonly IReadOnlyDictionary<string, string> FriendlyNames = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["com.netflix.mediaclient"] = "Netflix",
        ["com.google.android.youtube.tv"] = "YouTube",
        ["com.amazon.amazonvideo.livingroom"] = "Prime Video",
        ["com.amazon.avod.thirdpartyclient"] = "Prime Video",
        ["com.disney.disneyplus"] = "Disney+",
        ["com.hbo.hbonow"] = "Max",
        ["com.plexapp.android"] = "Plex",
        ["com.spotify.tv.android"] = "Spotify",
        ["com.apple.atve.androidtv.appletv"] = "Apple TV",
        ["com.google.android.apps.youtube.music"] = "YouTube Music",
    };

    private bool _isVisible = isVisible;
    private bool _isPinned = isPinned;
    private bool _canMoveUp;
    private bool _canMoveDown;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Id { get; } = id;
    public string Name { get; } = name;
    public string DisplayName => FriendlyNames.GetValueOrDefault(Id) ?? GetFallbackName(Name);
    public string PreviewGlyph => string.Concat(DisplayName.Where(char.IsLetterOrDigit).Take(2)).ToUpperInvariant();
    public string VisibilityLabel => IsVisible ? "Visible" : "Oculta";
    public string PinLabel => IsPinned ? "Fijada" : "Fijar primero";
    public bool CanMoveUp => _canMoveUp;
    public bool CanMoveDown => _canMoveDown;

    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (SetField(ref _isVisible, value))
            {
                OnPropertyChanged(nameof(VisibilityLabel));
            }
        }
    }

    internal void SetMoveAvailability(bool canMoveUp, bool canMoveDown)
    {
        if (_canMoveUp != canMoveUp)
        {
            _canMoveUp = canMoveUp;
            OnPropertyChanged(nameof(CanMoveUp));
        }
        if (_canMoveDown != canMoveDown)
        {
            _canMoveDown = canMoveDown;
            OnPropertyChanged(nameof(CanMoveDown));
        }
    }

    public bool IsPinned
    {
        get => _isPinned;
        set
        {
            if (SetField(ref _isPinned, value))
            {
                OnPropertyChanged(nameof(PinLabel));
                if (value)
                {
                    IsVisible = true;
                }
            }
        }
    }

    private bool SetField(ref bool field, bool value, [CallerMemberName] string? propertyName = null)
    {
        if (field == value)
        {
            return false;
        }
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private static string GetFallbackName(string name)
    {
        var segment = name.Split('.', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? name;
        var words = segment.Replace('_', ' ').Replace('-', ' ');
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(words);
    }
}
