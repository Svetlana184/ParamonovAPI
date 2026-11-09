using System;
using System.Collections.Generic;

namespace ParamonovAPI.Models;

public partial class Invoice
{
    public int IdInvoice { get; set; }

    public string Name { get; set; } = null!;

    public decimal Cost { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public virtual ICollection<ChangeRequest> ChangeRequests { get; set; } = new List<ChangeRequest>();

    public virtual ICollection<TaskList> TaskLists { get; set; } = new List<TaskList>();
}
