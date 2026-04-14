using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using AMLapp;
using ReactiveUI;

namespace AMLapp.ViewModels
{
    internal class AssignedTestsPageViewModel : ViewModelBase
    {
        public ObservableCollection<AssignedTestItem> AssignedTests { get; }
        public ReactiveCommand<Unit, Unit> LogoutCommand { get; }

        public AssignedTestsPageViewModel()
        {
            var db = MainWindowViewModel.Instance.db;
            var currentUserId = MainWindowViewModel.Instance.CurrentUserId;

            AssignedTests = new ObservableCollection<AssignedTestItem>();

            if (currentUserId.HasValue)
            {
                var tests = db.UserTests
                    .Where(ut => ut.User == currentUserId.Value)
                    .Select(ut => new
                    {
                        ut.Test,
                        ut.IsComplete,
                        Name = ut.TestNavigation.Name,
                        Description = ut.TestNavigation.Description
                    })
                    .ToList();

                foreach (var test in tests)
                {
                    var testId = test.Test;
                    var testName = test.Name;

                    AssignedTests.Add(new AssignedTestItem
                    {
                        TestId = testId,
                        Name = testName,
                        Description = test.Description,
                        IsComplete = test.IsComplete,
                        StartCommand = ReactiveCommand.Create(() =>
                        {
                            MainWindowViewModel.Instance.Page = new TakeTestPageView(testId, testName);
                        })
                    });
                }
            }

            LogoutCommand = ReactiveCommand.Create(() =>
            {
                MainWindowViewModel.Instance.CurrentUserId = null;
                MainWindowViewModel.Instance.IsCurrentUserAdmin = false;
                MainWindowViewModel.Instance.Page = new AuthPageView();
            });
        }
    }

    internal class AssignedTestItem
    {
        public int TestId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
        public ReactiveCommand<Unit, Unit>? StartCommand { get; set; }
    }
}
