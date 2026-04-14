using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using AMLapp;
using ReactiveUI;

namespace AMLapp.ViewModels
{
    public class TakeTestPageViewModel : ViewModelBase
    {
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;

        public int TestId { get; }
        public string TestName { get; }
        public ObservableCollection<TestQuestionItem> Questions { get; } = new();

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

        public ReactiveCommand<Unit, Unit> SubmitTestCommand { get; }
        public ReactiveCommand<Unit, Unit> BackCommand { get; }

        public TakeTestPageViewModel(int testId, string testName)
        {
            TestId = testId;
            TestName = testName;

            LoadQuestions();

            SubmitTestCommand = ReactiveCommand.Create(SubmitTest);
            BackCommand = ReactiveCommand.Create(() => { MainWindowViewModel.Instance.Page = new AssignedTestsPageView(); });
        }

        private void LoadQuestions()
        {
            var db = MainWindowViewModel.Instance.db;

            var questions = db.Questions
                .Where(q => q.Test == TestId)
                .OrderBy(q => q.NumberInTest)
                .Select(q => new TestQuestionItem
                {
                    QuestionText = q.Text,
                    Answers = new ObservableCollection<TestAnswerItem>(
                        q.Answers.Select(a => new TestAnswerItem
                        {
                            AnswerId = a.Id,
                            Text = a.Text,
                            Score = a.Score
                        }).ToList())
                })
                .ToList();

            foreach (var question in questions)
            {
                Questions.Add(question);
            }
        }

        private void SubmitTest()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            var userId = MainWindowViewModel.Instance.CurrentUserId;
            if (!userId.HasValue)
            {
                ErrorMessage = "Пользователь не определён.";
                return;
            }

            if (Questions.Any(q => q.SelectedAnswer is null))
            {
                ErrorMessage = "Ответьте на все вопросы перед завершением теста.";
                return;
            }

            var db = MainWindowViewModel.Instance.db;

            var answerIdsInTest = db.Answers
                .Where(a => a.QuestionNavigation.Test == TestId)
                .Select(a => a.Id)
                .ToList();

            var previousAnswers = db.UserAnswers
                .Where(ua => ua.User == userId.Value && answerIdsInTest.Contains(ua.Answer))
                .ToList();

            db.UserAnswers.RemoveRange(previousAnswers);

            foreach (var question in Questions)
            {
                db.UserAnswers.Add(new Models.UserAnswer
                {
                    User = userId.Value,
                    Answer = question.SelectedAnswer!.AnswerId
                });
            }

            db.SaveChanges();

            SuccessMessage = "Тест успешно отправлен.";
            MainWindowViewModel.Instance.Page = new AssignedTestsPageView();
        }
    }

    public class TestQuestionItem : ViewModelBase
    {
        private TestAnswerItem? _selectedAnswer;

        public string QuestionText { get; set; } = string.Empty;
        public ObservableCollection<TestAnswerItem> Answers { get; set; } = new();

        public TestAnswerItem? SelectedAnswer
        {
            get => _selectedAnswer;
            set => this.RaiseAndSetIfChanged(ref _selectedAnswer, value);
        }
    }

    public class TestAnswerItem
    {
        public int AnswerId { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Score { get; set; }
    }
}
