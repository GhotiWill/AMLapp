using AMLapp.ViewModels;
using Avalonia.Controls;

namespace AMLapp;

public partial class CreateTestPageView : UserControl
{
    public CreateTestPageView()
    {
        InitializeComponent();
        DataContext = new CreateTestPageViewModel();
    }
}
