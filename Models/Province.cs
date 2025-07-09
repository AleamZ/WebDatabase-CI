using System;
using System.Collections.Generic;

namespace CIResearch.Models;

public partial class Province
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? NameEn { get; set; }

    public string FullName { get; set; } = null!;

    public string? FullNameEn { get; set; }

    public string? CodeName { get; set; }

    public int? AdministrativeUnitId { get; set; }

    public int? AdministrativeRegionId { get; set; }

    public virtual AdministrativeRegion? AdministrativeRegion { get; set; }

    public virtual AdministrativeUnit? AdministrativeUnit { get; set; }

    public virtual ICollection<District> Districts { get; set; } = new List<District>();
}
