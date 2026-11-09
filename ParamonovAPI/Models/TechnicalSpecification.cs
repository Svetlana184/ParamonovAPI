using System;
using System.Collections.Generic;

namespace ParamonovAPI.Models;

public partial class TechnicalSpecification
{
    public int IdSpec { get; set; }

    public int IdProject { get; set; }

    public string Name { get; set; } = null!;

    public DateOnly? Deadline { get; set; }

    public string? Description { get; set; }

    public virtual Project IdProjectNavigation { get; set; } = null!;

    public virtual ICollection<SpecificationItem> SpecificationItems { get; set; } = new List<SpecificationItem>();
}
