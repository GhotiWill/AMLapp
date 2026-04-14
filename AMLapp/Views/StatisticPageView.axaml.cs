using AMLapp.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AMLapp;

public partial class StatisticPageView : UserControl
{
    public StatisticPageView()
    {
        InitializeComponent();
        DataContext = new StatisticPageViewModel();
    }
}