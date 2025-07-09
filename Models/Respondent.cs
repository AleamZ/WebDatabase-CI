using System;
using System.Collections.Generic;

namespace CIResearch.Models;

public partial class Respondent
{
    public int SbjNum { get; set; }

    public string? NameRes { get; set; }

    public string? EmailRes { get; set; }

    public string? PhoneRes { get; set; }

    public short? YearOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Job { get; set; }

    public string? Marriage { get; set; }

    public string? AdressDetail { get; set; }

    public string? WardCode { get; set; }

    public string? PathAdd { get; set; }

    public string? HouseholdIncome { get; set; }

    public string? IndivialIncome { get; set; }

    public virtual Marriagestatus? MarriageNavigation { get; set; }

    public virtual Ward? WardCodeNavigation { get; set; }
}
