using System;
using System.Collections.Generic;

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

    public virtual ICollection<ChangeRequest> ChangeRequests { get; set; } = new List<ChangeRequest>();

    public virtual User? IdDeveloperNavigation { get; set; }

    public virtual TaskList IdTaskListNavigation { get; set; } = null!;
}
