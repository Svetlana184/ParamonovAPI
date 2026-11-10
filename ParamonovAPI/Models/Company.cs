using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ParamonovAPI.Models;

public partial class Company
{
    public int IdCompany { get; set; }

    public string Name { get; set; } = null!;
    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
