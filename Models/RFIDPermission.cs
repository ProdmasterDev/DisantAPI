using Newtonsoft.Json;

namespace DisantAPI.Models
{
    public class RFIDPermission
    {
        [JsonProperty("permission")]
        public int? Permission { get; set; }
    }
}
