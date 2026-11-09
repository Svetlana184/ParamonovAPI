using System;
using System.Collections.Generic;

namespace ParamonovAPI.Models;

public partial class Project
{
    public int IdProject { get; set; }

    public int IdCustomer { get; set; }

    public int? IdTeam { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual User IdCustomerNavigation { get; set; } = null!;

    public virtual Team? IdTeamNavigation { get; set; }

    public virtual ICollection<TechnicalSpecification> TechnicalSpecifications { get; set; } = new List<TechnicalSpecification>();
}
