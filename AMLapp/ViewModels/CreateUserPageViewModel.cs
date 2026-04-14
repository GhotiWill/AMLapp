using System;
using System.Linq;
using System.Reactive;
using System.Security.Cryptography;
using System.Text;
using AMLapp;
using ReactiveUI;

namespace AMLapp.ViewModels
{
    public class CreateUserPageViewModel : ViewModelBase
    {
        private string _login = string.Empty;
        private string _password = string.Empty;
        private bool _isAdmin;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;

        public string Login
        {
            get => _login;
            set => this.RaiseAndSetIfChanged(ref _login, value);
        }

        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        public bool IsAdmin
        {
            get => _isAdmin;
            set => this.RaiseAndSetIfChanged(ref _isAdmin, value);
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

        public ReactiveCommand<Unit, Unit> CreateUserCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToStatisticsCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToCreateTestCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToAssignTestCommand { get; }
        public ReactiveCommand<Unit, Unit> LogoutCommand { get; }

        public CreateUserPageViewModel()
        {
            CreateUserCommand = ReactiveCommand.Create(CreateUser);
            GoToStatisticsCommand = ReactiveCommand.Create(() => { MainWindowViewModel.Instance.Page = new StatisticPageView(); });
            GoToCreateTestCommand = ReactiveCommand.Create(() => { MainWindowViewModel.Instance.Page = new CreateTestPageView(); });
            GoToAssignTestCommand = ReactiveCommand.Create(() => { MainWindowViewModel.Instance.Page = new AssignTestPageView(); });
            LogoutCommand = ReactiveCommand.Create(() =>
            {
                MainWindowViewModel.Instance.CurrentUserId = null;
                MainWindowViewModel.Instance.IsCurrentUserAdmin = false;
                MainWindowViewModel.Instance.Page = new AuthPageView();
            });
        }

        private void CreateUser()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Введите логин и пароль.";
                return;
            }

            var db = MainWindowViewModel.Instance.db;
            var normalizedLogin = Login.Trim();

            if (db.Users.Any(u => u.Login == normalizedLogin))
            {
                ErrorMessage = "Пользователь с таким логином уже существует.";
                return;
            }

            db.Users.Add(new Models.User
            {
                Login = normalizedLogin,
                Password = CalculateMd5(Password),
                IsAdmin = IsAdmin
            });

            db.SaveChanges();

            SuccessMessage = "Пользователь успешно создан.";
            Login = string.Empty;
            Password = string.Empty;
            IsAdmin = false;
        }

        private static string CalculateMd5(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            var hash = MD5.HashData(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
