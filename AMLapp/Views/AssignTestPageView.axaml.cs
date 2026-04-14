using AMLapp.ViewModels;
using Avalonia.Controls;

namespace AMLapp;

public partial class AssignTestPageView : UserControl
{
    public AssignTestPageView()
    {
        InitializeComponent();
        DataContext = new AssignTestPageViewModel();
    }
}
