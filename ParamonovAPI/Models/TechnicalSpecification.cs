using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
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
    public string Status { get; set; } = "draft";
    [JsonIgnore]
    [ValidateNever]
    public virtual Project IdProjectNavigation { get; set; } = null!;
    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<SpecificationItem> SpecificationItems { get; set; } = new List<SpecificationItem>();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SpecStatus { draft, pending_approval, approved }
