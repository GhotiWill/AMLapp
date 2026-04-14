using AMLapp.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AMLapp;

public partial class AuthPageView : UserControl
{
    public AuthPageView()
    {
        InitializeComponent();
        DataContext = new AuthPageViewModel();
    }
}