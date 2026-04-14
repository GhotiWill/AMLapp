using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using AMLapp;
using ReactiveUI;

namespace AMLapp.ViewModels
{
    public class AssignTestPageViewModel : ViewModelBase
    {
        private SelectionItem? _selectedUser;
        private SelectionItem? _selectedTest;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;

        public ObservableCollection<SelectionItem> Users { get; }
        public ObservableCollection<SelectionItem> Tests { get; }

        public SelectionItem? SelectedUser
        {
            get => _selectedUser;
            set => this.RaiseAndSetIfChanged(ref _selectedUser, value);
        }

        public SelectionItem? SelectedTest
        {
            get => _selectedTest;
            set => this.RaiseAndSetIfChanged(ref _selectedTest, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set => this.RaiseAndSetIfChanged(ref _successMessage, value);
        }

        public ReactiveCommand<Unit, Unit> AssignTestCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToStatisticsCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToCreateUserCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToCreateTestCommand { get; }
        public ReactiveCommand<Unit, Unit> LogoutCommand { get; }

        public AssignTestPageViewModel()
        {
            var db = MainWindowViewModel.Instance.db;

            Users = new ObservableCollection<SelectionItem>(
                db.Users
                  .Where(u => !u.IsAdmin)
                  .Select(u => new SelectionItem { Id = u.Id, Name = u.Login })
                  .ToList());

            Tests = new ObservableCollection<SelectionItem>(
                db.Tests
                  .Select(t => new SelectionItem { Id = t.Id, Name = t.Name })
                  .ToList());

            AssignTestCommand = ReactiveCommand.Create(AssignTest);
            GoToStatisticsCommand = ReactiveCommand.Create(() => { MainWindowViewModel.Instance.Page = new StatisticPageView(); });
            GoToCreateUserCommand = ReactiveCommand.Create(() => { MainWindowViewModel.Instance.Page = new CreateUserPageView(); });
            GoToCreateTestCommand = ReactiveCommand.Create(() => { MainWindowViewModel.Instance.Page = new CreateTestPageView(); });
            LogoutCommand = ReactiveCommand.Create(() =>
            {
                MainWindowViewModel.Instance.CurrentUserId = null;
                MainWindowViewModel.Instance.IsCurrentUserAdmin = false;
                MainWindowViewModel.Instance.Page = new AuthPageView();
            });
        }

        private void AssignTest()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (SelectedUser is null || SelectedTest is null)
            {
                ErrorMessage = "Выберите пользователя и тест.";
                return;
            }

            var db = MainWindowViewModel.Instance.db;

            var exists = db.UserTests.Any(ut => ut.User == SelectedUser.Id && ut.Test == SelectedTest.Id);
            if (exists)
            {
                ErrorMessage = "Этот тест уже назначен пользователю.";
                return;
            }

            var user = db.Users.FirstOrDefault(u => u.Id == SelectedUser.Id);
            if (user is null || user.IsAdmin)
            {
                ErrorMessage = "Назначать тесты администраторам нельзя.";
                return;
            }

            db.UserTests.Add(new Models.UserTest
            {
                User = SelectedUser.Id,
                Test = SelectedTest.Id
            });

            db.SaveChanges();

            SuccessMessage = "Тест успешно назначен пользователю.";
        }
    }

    public class SelectionItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
