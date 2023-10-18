using Newtonsoft.Json;

namespace DisantAPI.Models
{
    public class WorkersInContextOfWorkshops
    {
        [JsonProperty("number")]
        public long DisanId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("mark")]
        public string Mark {  get; set; }
        [JsonProperty("workshopidn")]
        public long WorkshopId { get; set; }
        [JsonProperty("workshopname")]
        public string WorkshopName { get; set;}
    }
}
