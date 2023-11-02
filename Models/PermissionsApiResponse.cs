using Newtonsoft.Json;

namespace DisantAPI.Models
{
    public class PermissionsApiResponse
    {
        [JsonProperty("user")]
        public long UserId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("permission")]
        public List<RFIDPermission>? Permissions { get; set; }

    }
}
