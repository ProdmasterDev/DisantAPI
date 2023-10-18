using Newtonsoft.Json;

namespace DisantAPI.Models
{
    public class Custom
    {
        [JsonProperty("number")]
        public long DisanId { get; set; } = default(long);
        [JsonProperty("name")]
        public string Name {  get; set; } = string.Empty;
        [JsonProperty("mark")]
        public string Mark { get; set; } = string.Empty;
    }
}
