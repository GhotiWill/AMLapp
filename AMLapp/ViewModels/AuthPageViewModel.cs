using System;
using System.Linq;
using System.Reactive;
using System.Security.Cryptography;
using System.Text;
using AMLapp;
using ReactiveUI;

namespace AMLapp.ViewModels
{
    public class AuthPageViewModel : ViewModelBase
    {
        private string _login = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;

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

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public ReactiveCommand<Unit, Unit> AuthorizeCommand { get; }

        public AuthPageViewModel()
        {
            AuthorizeCommand = ReactiveCommand.Create(Authorize);
        }

        private void Authorize()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Введите логин и пароль.";
                return;
            }

            var normalizedLogin = Login.Trim();
            var md5Password = CalculateMd5(Password);

            var user = MainWindowViewModel.Instance.db.Users
                .FirstOrDefault(u => u.Login == normalizedLogin && u.Password == md5Password);

            if (user is null)
            {
                ErrorMessage = "Неверный логин или пароль.";
                return;
            }

            MainWindowViewModel.Instance.CurrentUserId = user.Id;
            MainWindowViewModel.Instance.IsCurrentUserAdmin = user.IsAdmin;

            if (user.IsAdmin)
            {
                MainWindowViewModel.Instance.Page = new StatisticPageView();
                return;
            }

            MainWindowViewModel.Instance.Page = new AssignedTestsPageView();
        }

        private static string CalculateMd5(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            var hash = MD5.HashData(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
