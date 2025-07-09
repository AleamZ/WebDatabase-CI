using System;
using System.Collections.Generic;

namespace CIResearch.Models;

public partial class Typeproject
{
    public string NameType { get; set; } = null!;

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
