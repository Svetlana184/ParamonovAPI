using System.Text.Json.Serialization;

namespace ParamonovAPI.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ChangeStatus
    {
        created,
        approved,
        revoked
    }
}
