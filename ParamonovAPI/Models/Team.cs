using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ParamonovAPI.Models;

public partial class Team
{
    public int IdTeam { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int IdCompany { get; set; }
    [JsonIgnore]
    [ValidateNever]
    public virtual Company IdCompanyNavigation { get; set; } = null!;
    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
