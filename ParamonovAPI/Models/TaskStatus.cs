using System.Text.Json.Serialization;

namespace ParamonovAPI.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TaskStatus
    {
        @new,
        in_progress,
        on_review,
        done,
        cancelled
    }
}
