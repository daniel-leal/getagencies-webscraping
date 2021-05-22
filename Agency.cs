using System.Text.Json.Serialization;

namespace Getagency
{
    public class Agency
    {
        [JsonPropertyName("nome")]
        public string Name { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("info")]
        public string Info { get; set; }
    }
}