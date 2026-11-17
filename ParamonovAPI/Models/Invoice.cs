using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ParamonovAPI.Models;

public partial class Invoice
{
    public int IdInvoice { get; set; }

    public string Name { get; set; } = null!;

    public decimal Cost { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [NotMapped]
    public decimal TotalCost => TaskLists
        .SelectMany(t => t.Tasks)
        .Sum(t => t.Cost ?? 0m);

    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<ChangeRequest> ChangeRequests { get; set; } = new List<ChangeRequest>();

    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<TaskList> TaskLists { get; set; } = new List<TaskList>();

    [JsonIgnore]
    [ValidateNever]
    public virtual Project? Project { get; set; }
}