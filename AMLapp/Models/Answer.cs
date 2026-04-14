using System;
using System.Collections.Generic;

namespace AMLapp.Models;

public partial class Answer
{
    public int Id { get; set; }

    public string Text { get; set; } = null!;

    public int Score { get; set; }

    public int Question { get; set; }

    public virtual Question QuestionNavigation { get; set; } = null!;

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
