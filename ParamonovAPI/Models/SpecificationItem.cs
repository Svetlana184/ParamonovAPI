using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
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
    [ValidateNever]
    public virtual ICollection<ChangeRequest> ChangeRequests { get; set; } = new List<ChangeRequest>();
    [JsonIgnore]
    [ValidateNever]
    public virtual TechnicalSpecification IdSpecNavigation { get; set; } = null!;
}
