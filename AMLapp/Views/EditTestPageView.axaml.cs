using AMLapp.ViewModels;
using Avalonia.Controls;

namespace AMLapp;

public partial class EditTestPageView : UserControl
{
    public EditTestPageView(int testId)
    {
        InitializeComponent();
        DataContext = new EditTestPageViewModel(testId);
    }
}
