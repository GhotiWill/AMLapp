using System;
using System.Collections.Generic;

namespace AMLapp.Models;

public partial class Question
{
    public int Id { get; set; }

    public string Text { get; set; } = null!;

    public int Test { get; set; }

    public int NumberInTest { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual Test TestNavigation { get; set; } = null!;
}
