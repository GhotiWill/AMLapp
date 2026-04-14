using System;
using System.Collections.Generic;

namespace AMLapp.Models;

public partial class UserAnswer
{
    public int Id { get; set; }

    public int User { get; set; }

    public int Answer { get; set; }

    public virtual Answer AnswerNavigation { get; set; } = null!;

    public virtual User UserNavigation { get; set; } = null!;
}
