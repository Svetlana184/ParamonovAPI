using System;
using System.Collections.Generic;

namespace ParamonovAPI.Models;

public partial class Team
{
    public int IdTeam { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int IdCompany { get; set; }

    public virtual Company IdCompanyNavigation { get; set; } = null!;

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
