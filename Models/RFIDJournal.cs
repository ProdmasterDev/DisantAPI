using Newtonsoft.Json;
using System.Configuration;
using System.Text.Json.Serialization;

namespace DisantAPI.Models
{
    public class RFIDJournal
    {
        [JsonProperty("number")]
        public long Number { get; set; } = 0;
        [JsonProperty("action")]
        public int Action { get; set; } = 0;
        [JsonProperty("modify")]
        public DateTime Modify { get; set; } = DateTime.Now;
        [JsonProperty("workshop")]
        public long Workshop { get; set; } = 0;
        [JsonProperty("cuser")]
        public long CUser { get; set; } = 0;
    }
}
