using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AMLapp.ViewModels;

namespace AMLapp;

public partial class AssignedTestsPageView : UserControl
{
    public AssignedTestsPageView()
    {
        InitializeComponent();
        DataContext = new AssignedTestsPageViewModel();
    }
}
