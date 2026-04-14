using AMLapp.ViewModels;
using Avalonia.Controls;

namespace AMLapp;

public partial class CreateUserPageView : UserControl
{
    public CreateUserPageView()
    {
        InitializeComponent();
        DataContext = new CreateUserPageViewModel();
    }
}
