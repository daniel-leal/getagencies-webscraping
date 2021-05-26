using System.Collections.Generic;
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

    // API Class
    public class AgencyCode
    {
        [JsonPropertyName("nomes")]
        public List<NamesVO> names { get; set; }
    }

    public class NamesVO
    {
        [JsonPropertyName("val")]
        public string val { get; set; }

        [JsonPropertyName("text")]
        public string text { get; set; }
    }
}