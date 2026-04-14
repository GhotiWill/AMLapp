using AMLapp.ViewModels;
using Avalonia.Controls;

namespace AMLapp;

public partial class TakeTestPageView : UserControl
{
    public TakeTestPageView(int testId, string testName)
    {
        InitializeComponent();
        DataContext = new TakeTestPageViewModel(testId, testName);
    }
}
