using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ParamonovAPI.Models;

public partial class User
{
    public int IdUser { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;
    public string Role { get; set; } = null!;

    public string Login { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int? IdTeam { get; set; }

    public int? IdCompany { get; set; }

    public string? ShortDescription { get; set; }
    [JsonIgnore]
    public virtual ICollection<ChangeRequest> ChangeRequestIdApproverNavigations { get; set; } = new List<ChangeRequest>();
    [JsonIgnore]
    public virtual ICollection<ChangeRequest> ChangeRequestIdCreatorNavigations { get; set; } = new List<ChangeRequest>();
    [JsonIgnore]
    public virtual Company? IdCompanyNavigation { get; set; }
    [JsonIgnore]
    public virtual Team? IdTeamNavigation { get; set; }
    [JsonIgnore]
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    [JsonIgnore]
    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
