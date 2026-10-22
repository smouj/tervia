using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Tervia.App.ViewModels;

public sealed class HomeApplicationOption(string id, string name, bool isVisible, bool isPinned) : INotifyPropertyChanged
{
    private bool _isVisible = isVisible;
    private bool _isPinned = isPinned;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Id { get; } = id;
    public string Name { get; } = name;
    public string VisibilityLabel => IsVisible ? "Visible" : "Oculta";
    public string PinLabel => IsPinned ? "Fijada" : "Fijar primero";

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
}
