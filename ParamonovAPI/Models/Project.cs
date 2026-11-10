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
    [JsonIgnore]
    public virtual User IdCustomerNavigation { get; set; } = null!;
    [JsonIgnore]
    public virtual Team? IdTeamNavigation { get; set; }
    [JsonIgnore]
    public virtual ICollection<TechnicalSpecification> TechnicalSpecifications { get; set; } = new List<TechnicalSpecification>();
}
