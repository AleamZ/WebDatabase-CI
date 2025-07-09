using CIResearch.Models;
using System;
using System.Collections.Generic;

namespace CIResearch.Models;

public partial class AdministrativeRegion
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public string? CodeName { get; set; }

    public string? CodeNameEn { get; set; }

    public virtual ICollection<Province> Provinces { get; set; } = new List<Province>();
}
