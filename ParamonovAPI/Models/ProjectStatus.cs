using System.Text.Json.Serialization;

namespace ParamonovAPI.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProjectStatus
    {
        draft,
        in_progress,
        pending_approval,
        pending_invoice_approval,
        pending_spec_approval,
        completed,
        closed,
        frozen
    }
}
