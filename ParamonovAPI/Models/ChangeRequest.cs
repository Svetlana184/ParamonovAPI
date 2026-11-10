using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ParamonovAPI.Models;

public partial class ChangeRequest
{
    public int IdChange { get; set; }

    public int IdSpecItem { get; set; }

    public int? IdTask { get; set; }

    public int? IdInvoice { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public int IdCreator { get; set; }

    public int? IdApprover { get; set; }
    [JsonIgnore]
    public virtual User? IdApproverNavigation { get; set; }
    [JsonIgnore]
    public virtual User IdCreatorNavigation { get; set; } = null!;
    [JsonIgnore]
    public virtual Invoice? IdInvoiceNavigation { get; set; }
    [JsonIgnore]
    public virtual SpecificationItem IdSpecItemNavigation { get; set; } = null!;
    [JsonIgnore]
    public virtual Task? IdTaskNavigation { get; set; }
}
