using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using AMLapp;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using ReactiveUI;

namespace AMLapp.ViewModels
{
    public class StatisticPageViewModel : ViewModelBase
    {
        public int TotalTests { get; private set; }
        public int TotalUsers { get; private set; }

        public ObservableCollection<TestStatisticsItem> TestStatistics { get; } = new();
        public ObservableCollection<TestManagementItem> ManagedTests { get; } = new();

        public ISeries[] AverageScoreSeries { get; private set; } = Array.Empty<ISeries>();
        public Axis[] XAxes { get; private set; } = Array.Empty<Axis>();

        public ReactiveCommand<Unit, Unit> GoToCreateUserCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToCreateTestCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToAssignTestCommand { get; }
        public ReactiveCommand<Unit, Unit> LogoutCommand { get; }

        public StatisticPageViewModel()
        {
            GoToCreateUserCommand = ReactiveCommand.Create(() => { MainWindowViewModel.Instance.Page = new CreateUserPageView(); });
            GoToCreateTestCommand = ReactiveCommand.Create(() => { MainWindowViewModel.Instance.Page = new CreateTestPageView(); });
            GoToAssignTestCommand = ReactiveCommand.Create(() => { MainWindowViewModel.Instance.Page = new AssignTestPageView(); });
            LogoutCommand = ReactiveCommand.Create(() =>
            {
                MainWindowViewModel.Instance.CurrentUserId = null;
                MainWindowViewModel.Instance.IsCurrentUserAdmin = false;
                MainWindowViewModel.Instance.Page = new AuthPageView();
            });

            LoadData();
        }

        private void LoadData()
        {
            var db = MainWindowViewModel.Instance.db;

            TotalTests = db.Tests.Count();
            TotalUsers = db.Users.Count(u => !u.IsAdmin);

            TestStatistics.Clear();
            ManagedTests.Clear();

            var tests = db.Tests.OrderBy(t => t.Name).ToList();

            foreach (var test in tests)
            {
                var userScores = db.UserAnswers
                    .Where(ua => ua.AnswerNavigation.QuestionNavigation.Test == test.Id && !ua.UserNavigation.IsAdmin)
                    .GroupBy(ua => new { ua.User, ua.UserNavigation.Login })
                    .Select(g => new
                    {
                        Login = g.Key.Login,
                        Score = g.Sum(x => x.AnswerNavigation.Score)
                    })
                    .ToList();

                var hasScores = userScores.Count > 0;
                var average = hasScores ? userScores.Average(x => x.Score) : 0;
                var max = hasScores ? userScores.Max(x => x.Score) : 0;
                var min = hasScores ? userScores.Min(x => x.Score) : 0;

                var maxUsers = hasScores
                    ? string.Join(", ", userScores.Where(x => x.Score == max).Select(x => x.Login).Distinct())
                    : "—";

                var minUsers = hasScores
                    ? string.Join(", ", userScores.Where(x => x.Score == min).Select(x => x.Login).Distinct())
                    : "—";

                TestStatistics.Add(new TestStatisticsItem
                {
                    TestName = test.Name,
                    AverageScore = Math.Round(average, 2),
                    MaxScore = max,
                    MinScore = min,
                    MaxScoreUsers = maxUsers,
                    MinScoreUsers = minUsers
                });

                var testId = test.Id;
                ManagedTests.Add(new TestManagementItem
                {
                    TestName = test.Name,
                    EditCommand = ReactiveCommand.Create(() =>
                    {
                        MainWindowViewModel.Instance.Page = new EditTestPageView(testId);
                    }),
                    DeleteCommand = ReactiveCommand.Create(() =>
                    {
                        DeleteTest(testId);
                        MainWindowViewModel.Instance.Page = new StatisticPageView();
                    })
                });
            }

            AverageScoreSeries =
            [
                new ColumnSeries<double>
                {
                    Name = "Средний балл",
                    Values = TestStatistics.Select(x => x.AverageScore).ToList()
                }
            ];

            XAxes =
            [
                new Axis
                {
                    Labels = TestStatistics.Select(x => x.TestName).ToArray(),
                    LabelsRotation = 15
                }
            ];

            this.RaisePropertyChanged(nameof(TotalTests));
            this.RaisePropertyChanged(nameof(TotalUsers));
            this.RaisePropertyChanged(nameof(AverageScoreSeries));
            this.RaisePropertyChanged(nameof(XAxes));
        }

        private void DeleteTest(int testId)
        {
            var db = MainWindowViewModel.Instance.db;

            var questionIds = db.Questions.Where(q => q.Test == testId).Select(q => q.Id).ToList();
            var answerIds = db.Answers.Where(a => questionIds.Contains(a.Question)).Select(a => a.Id).ToList();

            var userAnswers = db.UserAnswers.Where(ua => answerIds.Contains(ua.Answer)).ToList();
            var answers = db.Answers.Where(a => questionIds.Contains(a.Question)).ToList();
            var questions = db.Questions.Where(q => q.Test == testId).ToList();
            var userTests = db.UserTests.Where(ut => ut.Test == testId).ToList();
            var test = db.Tests.FirstOrDefault(t => t.Id == testId);

            if (test is null)
            {
                return;
            }

            db.UserAnswers.RemoveRange(userAnswers);
            db.Answers.RemoveRange(answers);
            db.Questions.RemoveRange(questions);
            db.UserTests.RemoveRange(userTests);
            db.Tests.Remove(test);
            db.SaveChanges();
        }
    }

    public class TestManagementItem
    {
        public string TestName { get; set; } = string.Empty;
        public ReactiveCommand<Unit, Unit>? EditCommand { get; set; }
        public ReactiveCommand<Unit, Unit>? DeleteCommand { get; set; }
    }

    public class TestStatisticsItem
    {
        public string TestName { get; set; } = string.Empty;
        public double AverageScore { get; set; }
        public int MaxScore { get; set; }
        public int MinScore { get; set; }
        public string MaxScoreUsers { get; set; } = string.Empty;
        public string MinScoreUsers { get; set; } = string.Empty;
    }
}
