using Newtonsoft.Json;
using System.Configuration;

namespace DisantAPI.Models
{
    public class RFIDInvoice
    {
        [JsonProperty("journal")]
        public long Journal { get; set; } = 0;
        [JsonProperty("tag")]
        public string Tag {  get; set; } = string.Empty;
        [JsonProperty("employee")]
        public long Employee { get; set; } = 0;
    }
}
