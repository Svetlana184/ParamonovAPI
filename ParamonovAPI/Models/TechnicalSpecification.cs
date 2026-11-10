using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ParamonovAPI.Models;

public partial class TechnicalSpecification
{
    public int IdSpec { get; set; }

    public int IdProject { get; set; }

    public string Name { get; set; } = null!;

    public DateOnly? Deadline { get; set; }

    public string? Description { get; set; }
    [JsonIgnore]
    public virtual Project IdProjectNavigation { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<SpecificationItem> SpecificationItems { get; set; } = new List<SpecificationItem>();
}
