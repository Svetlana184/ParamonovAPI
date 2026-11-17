using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ParamonovAPI.Models;

public partial class Project
{
    public int IdProject { get; set; }

    public int IdCustomer { get; set; }

    public int? IdTeam { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? IdInvoice { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.draft;

    [NotMapped]
    public decimal TotalCost => IdInvoiceNavigation?.TotalCost ?? 0m;

    [JsonIgnore]
    [ValidateNever]
    public virtual User IdCustomerNavigation { get; set; } = null!;

    [JsonIgnore]
    [ValidateNever]
    public virtual Team? IdTeamNavigation { get; set; }

    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<TechnicalSpecification> TechnicalSpecifications { get; set; } = new List<TechnicalSpecification>();

    [JsonIgnore]
    [ValidateNever]
    public virtual Invoice? IdInvoiceNavigation { get; set; }
}