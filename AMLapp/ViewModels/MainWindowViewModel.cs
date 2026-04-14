using Avalonia.Controls;
using AMLapp.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMLapp.ViewModels
{
    internal class MainWindowViewModel: ViewModelBase
    {
        public static MainWindowViewModel Instance { get; set; }
        public _43pBardakovExamPrepContext db { get; } = new _43pBardakovExamPrepContext();
        public int? CurrentUserId { get; set; }
        public bool IsCurrentUserAdmin { get; set; }

        public MainWindowViewModel()
        {
            Instance = this;
        }

        UserControl _page = new AuthPageView();

        public UserControl Page
        {
            get => _page;
            set => this.RaiseAndSetIfChanged(ref _page, value);
        }
    }
}
