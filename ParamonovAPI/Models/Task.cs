using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ParamonovAPI.Models;

public partial class Task
{
    public int IdTask { get; set; }

    public int IdTaskList { get; set; }

    public string? Description { get; set; }

    public decimal? PlannedHours { get; set; }

    public decimal? ActualHours { get; set; }

    public decimal? Cost { get; set; }

    public int? IdDeveloper { get; set; }
    [JsonIgnore]
    public virtual ICollection<ChangeRequest> ChangeRequests { get; set; } = new List<ChangeRequest>();
    [JsonIgnore]
    public virtual User? IdDeveloperNavigation { get; set; }
    [JsonIgnore]
    public virtual TaskList IdTaskListNavigation { get; set; } = null!;
}
