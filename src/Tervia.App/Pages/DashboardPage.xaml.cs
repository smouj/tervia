using Microsoft.UI.Xaml.Controls;
using Tervia.App.ViewModels;

namespace Tervia.App.Pages;

public sealed partial class DashboardPage : Page
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
    }

    public DashboardViewModel ViewModel { get; }
}
