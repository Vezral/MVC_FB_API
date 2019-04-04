using Newtonsoft.Json;

namespace TEST_FB_API.Models.HeatMap
{
    public class DataPoint
    {
        [JsonProperty("x")]
        public int x { get; set; }

        [JsonProperty("y")]
        public int y { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }
    }
}