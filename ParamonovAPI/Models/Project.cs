using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ParamonovAPI.Models;

public partial class Project
{
    public int IdProject { get; set; }

    public int IdCustomer { get; set; }

    public int? IdTeam { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.draft;
    [JsonIgnore]
    [ValidateNever]
    public virtual User IdCustomerNavigation { get; set; } = null!;
    [JsonIgnore]
    [ValidateNever]
    public virtual Team? IdTeamNavigation { get; set; }
    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<TechnicalSpecification> TechnicalSpecifications { get; set; } = new List<TechnicalSpecification>();
}
