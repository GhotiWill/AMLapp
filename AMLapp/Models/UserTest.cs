using System;
using System.Collections.Generic;

namespace AMLapp.Models;

public partial class UserTest
{
    public int Id { get; set; }

    public int User { get; set; }

    public int Test { get; set; }

    public virtual Test TestNavigation { get; set; } = null!;

    public virtual User UserNavigation { get; set; } = null!;
}
