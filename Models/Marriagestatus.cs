using System;
using System.Collections.Generic;

namespace CIResearch.Models;

public partial class Marriagestatus
{
    public string MarriageStatus1 { get; set; } = null!;

    public virtual ICollection<Respondent> Respondents { get; set; } = new List<Respondent>();
}
