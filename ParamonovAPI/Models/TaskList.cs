using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ParamonovAPI.Models;

public partial class TaskList
{
    public int IdTaskList { get; set; }

    public int IdInvoice { get; set; }

    public string Name { get; set; } = null!;

    public decimal CompletionPercent { get; set; }
    [JsonIgnore]
    [ValidateNever]
    public virtual Invoice IdInvoiceNavigation { get; set; } = null!;
    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
