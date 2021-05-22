using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Getagency
{
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
