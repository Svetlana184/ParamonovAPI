using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ParamonovAPI.Models;

public partial class SpecificationItem
{
    public int IdItem { get; set; }

    public int IdSpec { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }
    [JsonIgnore]
    public virtual ICollection<ChangeRequest> ChangeRequests { get; set; } = new List<ChangeRequest>();
    [JsonIgnore]
    public virtual TechnicalSpecification IdSpecNavigation { get; set; } = null!;
}
