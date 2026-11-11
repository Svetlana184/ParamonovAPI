using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ParamonovAPI.Models;

public partial class User
{
    public int IdUser { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;
    public string Role { get; set; } = "customer";

    public string Login { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int? IdTeam { get; set; }

    public int? IdCompany { get; set; }

    public string? ShortDescription { get; set; }
    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<ChangeRequest> ChangeRequestIdApproverNavigations { get; set; } = new List<ChangeRequest>();
    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<ChangeRequest> ChangeRequestIdCreatorNavigations { get; set; } = new List<ChangeRequest>();
    [JsonIgnore]
    [ValidateNever]
    public virtual Company? IdCompanyNavigation { get; set; }
    [JsonIgnore]
    [ValidateNever]
    public virtual Team? IdTeamNavigation { get; set; }
    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
