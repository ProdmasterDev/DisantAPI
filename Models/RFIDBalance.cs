using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;

namespace DisantAPI.Models
{
    public class RFIDBalance
    {
        [JsonProperty("tag")]
        public string Tag { get; set; }
        [JsonProperty("onbalance")]
        public bool OnBalance { get; set; }
        [JsonProperty("employee")]
        public long Employee {  get; set; }
        public string GetOnBalanceStringValue() { if (OnBalance) { return ".T."; } else { return ".F."; } }
    }
}
