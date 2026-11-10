using System.Text.Json.Serialization;

namespace ParamonovAPI.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProjectStatus
    {
        draft,
        in_progress,
        pending_approval,
        completed,
        closed,
        frozen
    }
}
