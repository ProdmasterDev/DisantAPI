using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;

namespace DisantAPI.Models
{
    public class RFIDBalance
    {
        [JsonProperty("number")]
        public long Number { get; set; } = 0;
        [JsonProperty("tag")]
        public string Tag { get; set; } = string.Empty;
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        [JsonProperty("onbalance")]
        public bool OnBalance { get; set; }
        [JsonProperty("employee")]
        public long Employee {  get; set; }
        [JsonProperty("workshop")]
        public long Workshop { get; set; }
        public string GetOnBalanceStringValue() { if (OnBalance) { return ".T."; } else { return ".F."; } }
    }
}
