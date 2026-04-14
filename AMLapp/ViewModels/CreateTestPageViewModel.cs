using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using AMLapp;
using ReactiveUI;

namespace AMLapp.ViewModels
{
    public class CreateTestPageViewModel : ViewModelBase
    {
        private string _name = string.Empty;
        private string _description = string.Empty;
        private string _newQuestionText = string.Empty;
        private string _newAnswerText = string.Empty;
        private int _newAnswerScore;
        private QuestionDraft? _selectedQuestion;

        private TestListItem? _selectedExistingTest;
        private string _editTestName = string.Empty;
        private string _editTestDescription = string.Empty;

        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }

        public string NewQuestionText
        {
            get => _newQuestionText;
            set => this.RaiseAndSetIfChanged(ref _newQuestionText, value);
        }

        public string NewAnswerText
        {
            get => _newAnswerText;
            set => this.RaiseAndSetIfChanged(ref _newAnswerText, value);
        }

        public int NewAnswerScore
        {
            get => _newAnswerScore;
            set => this.RaiseAndSetIfChanged(ref _newAnswerScore, value);
        }

        public QuestionDraft? SelectedQuestion
        {
            get => _selectedQuestion;
            set => this.RaiseAndSetIfChanged(ref _selectedQuestion, value);
        }

        public ObservableCollection<QuestionDraft> Questions { get; } = new();

        public ObservableCollection<TestListItem> ExistingTests { get; } = new();

        public TestListItem? SelectedExistingTest
        {
            get => _selectedExistingTest;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedExistingTest, value);
                if (value is not null)
                {
                    EditTestName = value.Name;
                    EditTestDescription = value.Description;
                }
            }
        }

        public string EditTestName
        {
            get => _editTestName;
            set => this.RaiseAndSetIfChanged(ref _editTestName, value);
        }

        public string EditTestDescription
        {
            get => _editTestDescription;
            set => this.RaiseAndSetIfChanged(ref _editTestDescription, value);
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

        public ReactiveCommand<Unit, Unit> AddQuestionCommand { get; }
        public ReactiveCommand<Unit, Unit> AddAnswerCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateTestCommand { get; }
        public ReactiveCommand<Unit, Unit> UpdateTestCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteTestCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToStatisticsCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToCreateUserCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToAssignTestCommand { get; }
        public ReactiveCommand<Unit, Unit> LogoutCommand { get; }

        public CreateTestPageViewModel()
        {
            LoadExistingTests();

            AddQuestionCommand = ReactiveCommand.Create(AddQuestion);
            AddAnswerCommand = ReactiveCommand.Create(AddAnswerToSelectedQuestion);
            CreateTestCommand = ReactiveCommand.Create(CreateTest);
            UpdateTestCommand = ReactiveCommand.Create(UpdateSelectedTest);
            DeleteTestCommand = ReactiveCommand.Create(DeleteSelectedTest);
            GoToStatisticsCommand = ReactiveCommand.Create(() => { MainWindowViewModel.Instance.Page = new StatisticPageView(); });
            GoToCreateUserCommand = ReactiveCommand.Create(() => { MainWindowViewModel.Instance.Page = new CreateUserPageView(); });
            GoToAssignTestCommand = ReactiveCommand.Create(() => { MainWindowViewModel.Instance.Page = new AssignTestPageView(); });
            LogoutCommand = ReactiveCommand.Create(() =>
            {
                MainWindowViewModel.Instance.CurrentUserId = null;
                MainWindowViewModel.Instance.IsCurrentUserAdmin = false;
                MainWindowViewModel.Instance.Page = new AuthPageView();
            });
        }

        private void LoadExistingTests()
        {
            ExistingTests.Clear();
            var db = MainWindowViewModel.Instance.db;
            var tests = db.Tests
                .OrderBy(t => t.Name)
                .Select(t => new TestListItem
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description
                })
                .ToList();

            foreach (var test in tests)
            {
                ExistingTests.Add(test);
            }
        }

        private void AddQuestion()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(NewQuestionText))
            {
                ErrorMessage = "Введите текст вопроса.";
                return;
            }

            var question = new QuestionDraft { Text = NewQuestionText.Trim() };
            Questions.Add(question);
            SelectedQuestion = question;
            NewQuestionText = string.Empty;
        }

        private void AddAnswerToSelectedQuestion()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (SelectedQuestion is null)
            {
                ErrorMessage = "Выберите вопрос для добавления ответа.";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewAnswerText))
            {
                ErrorMessage = "Введите текст ответа.";
                return;
            }

            SelectedQuestion.Answers.Add(new AnswerDraft
            {
                Text = NewAnswerText.Trim(),
                Score = NewAnswerScore
            });

            NewAnswerText = string.Empty;
            NewAnswerScore = 0;
        }

        private void CreateTest()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = "Введите название теста.";
                return;
            }

            if (Questions.Count == 0)
            {
                ErrorMessage = "Добавьте хотя бы один вопрос в тест.";
                return;
            }

            if (Questions.Any(q => q.Answers.Count == 0))
            {
                ErrorMessage = "Каждый вопрос должен содержать хотя бы один ответ.";
                return;
            }

            var db = MainWindowViewModel.Instance.db;
            var normalizedName = Name.Trim();

            if (db.Tests.Any(t => t.Name == normalizedName))
            {
                ErrorMessage = "Тест с таким названием уже существует.";
                return;
            }

            var test = new Models.Test
            {
                Name = normalizedName,
                Description = Description?.Trim() ?? string.Empty
            };

            db.Tests.Add(test);
            db.SaveChanges();

            for (var i = 0; i < Questions.Count; i++)
            {
                var questionDraft = Questions[i];

                var question = new Models.Question
                {
                    Test = test.Id,
                    NumberInTest = i + 1,
                    Text = questionDraft.Text
                };

                db.Questions.Add(question);
                db.SaveChanges();

                foreach (var answerDraft in questionDraft.Answers)
                {
                    db.Answers.Add(new Models.Answer
                    {
                        Question = question.Id,
                        Text = answerDraft.Text,
                        Score = answerDraft.Score
                    });
                }
            }

            db.SaveChanges();

            SuccessMessage = "Тест, вопросы и ответы успешно созданы.";
            Name = string.Empty;
            Description = string.Empty;
            NewQuestionText = string.Empty;
            NewAnswerText = string.Empty;
            NewAnswerScore = 0;
            Questions.Clear();
            SelectedQuestion = null;
            LoadExistingTests();
        }

        private void UpdateSelectedTest()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (SelectedExistingTest is null)
            {
                ErrorMessage = "Выберите тест для редактирования.";
                return;
            }

            if (string.IsNullOrWhiteSpace(EditTestName))
            {
                ErrorMessage = "Название теста не может быть пустым.";
                return;
            }

            var db = MainWindowViewModel.Instance.db;
            var normalizedName = EditTestName.Trim();

            if (db.Tests.Any(t => t.Name == normalizedName && t.Id != SelectedExistingTest.Id))
            {
                ErrorMessage = "Тест с таким названием уже существует.";
                return;
            }

            var test = db.Tests.First(t => t.Id == SelectedExistingTest.Id);
            test.Name = normalizedName;
            test.Description = EditTestDescription?.Trim() ?? string.Empty;
            db.SaveChanges();

            SuccessMessage = "Тест успешно обновлён.";
            LoadExistingTests();
            SelectedExistingTest = ExistingTests.FirstOrDefault(t => t.Id == test.Id);
        }

        private void DeleteSelectedTest()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (SelectedExistingTest is null)
            {
                ErrorMessage = "Выберите тест для удаления.";
                return;
            }

            var db = MainWindowViewModel.Instance.db;
            var testId = SelectedExistingTest.Id;

            var questionIds = db.Questions.Where(q => q.Test == testId).Select(q => q.Id).ToList();
            var answerIds = db.Answers.Where(a => questionIds.Contains(a.Question)).Select(a => a.Id).ToList();

            var userAnswers = db.UserAnswers.Where(ua => answerIds.Contains(ua.Answer)).ToList();
            var answers = db.Answers.Where(a => questionIds.Contains(a.Question)).ToList();
            var questions = db.Questions.Where(q => q.Test == testId).ToList();
            var userTests = db.UserTests.Where(ut => ut.Test == testId).ToList();
            var test = db.Tests.FirstOrDefault(t => t.Id == testId);

            if (test is null)
            {
                ErrorMessage = "Тест не найден.";
                return;
            }

            db.UserAnswers.RemoveRange(userAnswers);
            db.Answers.RemoveRange(answers);
            db.Questions.RemoveRange(questions);
            db.UserTests.RemoveRange(userTests);
            db.Tests.Remove(test);
            db.SaveChanges();

            SuccessMessage = "Тест успешно удалён.";
            SelectedExistingTest = null;
            EditTestName = string.Empty;
            EditTestDescription = string.Empty;
            LoadExistingTests();
        }
    }

    public class QuestionDraft
    {
        public string Text { get; set; } = string.Empty;
        public ObservableCollection<AnswerDraft> Answers { get; set; } = new();
    }

    public class AnswerDraft
    {
        public string Text { get; set; } = string.Empty;
        public int Score { get; set; }
        public string Display => $"{Text} (Балл: {Score})";
    }

    public class TestListItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
