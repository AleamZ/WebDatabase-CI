using System;
using System.Collections.Generic;

namespace CIResearch.Models;

public partial class Project
{
    public string? ProjectId { get; set; }

    public string ProjectName { get; set; } = null!;

    public short? ProjectYear { get; set; }

    public string? ObjectOfUse { get; set; }

    public string? NameType { get; set; }

    public virtual Typeproject? NameTypeNavigation { get; set; }
}
