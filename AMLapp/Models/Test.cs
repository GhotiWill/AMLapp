using System;
using System.Collections.Generic;

namespace AMLapp.Models;

public partial class Test
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<UserTest> UserTests { get; set; } = new List<UserTest>();
}
