using Newtonsoft.Json;

namespace DisantAPI.Models
{
    public class Workshop
    {
        [JsonProperty("number")]
        public long DisanId { get; set; } = 0;
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        public List<Custom> Workers { get; set; } = new List<Custom>();
    }
}
