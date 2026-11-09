using System;
using System.Collections.Generic;

namespace ParamonovAPI.Models;

public partial class Company
{
    public int IdCompany { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
