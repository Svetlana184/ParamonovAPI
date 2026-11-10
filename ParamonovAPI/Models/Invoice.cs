using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ParamonovAPI.Models;

public partial class Invoice
{
    public int IdInvoice { get; set; }

    public string Name { get; set; } = null!;

    public decimal Cost { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
    [JsonIgnore]
    public virtual ICollection<ChangeRequest> ChangeRequests { get; set; } = new List<ChangeRequest>();
    [JsonIgnore]
    public virtual ICollection<TaskList> TaskLists { get; set; } = new List<TaskList>();
}
