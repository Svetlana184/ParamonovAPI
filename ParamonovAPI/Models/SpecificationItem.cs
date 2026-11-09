using System;
using System.Collections.Generic;

namespace ParamonovAPI.Models;

public partial class SpecificationItem
{
    public int IdItem { get; set; }

    public int IdSpec { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<ChangeRequest> ChangeRequests { get; set; } = new List<ChangeRequest>();

    public virtual TechnicalSpecification IdSpecNavigation { get; set; } = null!;
}
