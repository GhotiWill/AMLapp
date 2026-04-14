using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using AMLapp;
using ReactiveUI;

namespace AMLapp.ViewModels
{
    public class EditTestPageViewModel : ViewModelBase
    {
        public int TestId { get; }

        private string _name = string.Empty;
        private string _description = string.Empty;
        private string _newQuestionText = string.Empty;
        private string _newAnswerText = string.Empty;
        private int _newAnswerScore;
        private QuestionDraft? _selectedQuestion;
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
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> BackCommand { get; }

        public EditTestPageViewModel(int testId)
        {
            TestId = testId;

            AddQuestionCommand = ReactiveCommand.Create(AddQuestion);
            AddAnswerCommand = ReactiveCommand.Create(AddAnswerToSelectedQuestion);
            SaveCommand = ReactiveCommand.Create(Save);
            BackCommand = ReactiveCommand.Create(() => { MainWindowViewModel.Instance.Page = new StatisticPageView(); });

            Load();
        }

        private void Load()
        {
            var db = MainWindowViewModel.Instance.db;
            var test = db.Tests.FirstOrDefault(t => t.Id == TestId);
            if (test is null)
            {
                ErrorMessage = "Тест не найден.";
                return;
            }

            Name = test.Name;
            Description = test.Description;
            Questions.Clear();

            var questions = db.Questions
                .Where(q => q.Test == TestId)
                .OrderBy(q => q.NumberInTest)
                .Select(q => new QuestionDraft
                {
                    Text = q.Text,
                    Answers = new ObservableCollection<AnswerDraft>(
                        q.Answers
                         .Select(a => new AnswerDraft { Text = a.Text, Score = a.Score })
                         .ToList())
                })
                .ToList();

            foreach (var question in questions)
            {
                Questions.Add(question);
            }
        }

        private void AddQuestion()
        {
            ErrorMessage = string.Empty;

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

        private void Save()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = "Введите название теста.";
                return;
            }

            if (Questions.Count == 0 || Questions.Any(q => q.Answers.Count == 0))
            {
                ErrorMessage = "У теста должен быть хотя бы один вопрос и хотя бы один ответ у каждого вопроса.";
                return;
            }

            var db = MainWindowViewModel.Instance.db;

            if (db.Tests.Any(t => t.Name == Name.Trim() && t.Id != TestId))
            {
                ErrorMessage = "Тест с таким названием уже существует.";
                return;
            }

            var test = db.Tests.FirstOrDefault(t => t.Id == TestId);
            if (test is null)
            {
                ErrorMessage = "Тест не найден.";
                return;
            }

            test.Name = Name.Trim();
            test.Description = Description?.Trim() ?? string.Empty;

            var oldQuestionIds = db.Questions.Where(q => q.Test == TestId).Select(q => q.Id).ToList();
            var oldAnswerIds = db.Answers.Where(a => oldQuestionIds.Contains(a.Question)).Select(a => a.Id).ToList();

            var oldUserAnswers = db.UserAnswers.Where(ua => oldAnswerIds.Contains(ua.Answer)).ToList();
            var oldAnswers = db.Answers.Where(a => oldQuestionIds.Contains(a.Question)).ToList();
            var oldQuestions = db.Questions.Where(q => q.Test == TestId).ToList();

            db.UserAnswers.RemoveRange(oldUserAnswers);
            db.Answers.RemoveRange(oldAnswers);
            db.Questions.RemoveRange(oldQuestions);
            db.SaveChanges();

            for (var i = 0; i < Questions.Count; i++)
            {
                var questionDraft = Questions[i];
                var question = new Models.Question
                {
                    Test = TestId,
                    NumberInTest = i + 1,
                    Text = questionDraft.Text
                };
                db.Questions.Add(question);
                db.SaveChanges();

                foreach (var answer in questionDraft.Answers)
                {
                    db.Answers.Add(new Models.Answer
                    {
                        Question = question.Id,
                        Text = answer.Text,
                        Score = answer.Score
                    });
                }
            }

            db.SaveChanges();
            SuccessMessage = "Тест обновлён.";
        }
    }
}
