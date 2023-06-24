using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XSDLib
{
    [Serializable]
    public class SchemaElement
    {
        public SchemaElement()
        {
            Elements = new List<SchemaElement>();
        }

        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("dataType")]
        public string DataType { get; set; }
        [JsonPropertyName("minOccurs")]
        public string MinOccurs { get; set; }
        [JsonPropertyName("maxOccurs")]
        public string MaxOccurs { get; set; }
        [JsonPropertyName("minLength")]
        public string MinLength { get; set; }
        [JsonPropertyName("maxLength")]
        public string MaxLength { get; set; }
        [JsonPropertyName("pattern")]
        public string Pattern { get; set; }
        [JsonPropertyName("fractionDigits")]
        public string FractionDigits { get; set; }
        [JsonPropertyName("totalDigits")]
        public string TotalDigits { get; set; }
        [JsonPropertyName("minInclusive")]
        public string MinInclusive { get; set; }
        [JsonPropertyName("maxInclusive")]
        public string MaxInclusive { get; set; }
        [JsonPropertyName("values")]
        public string[] Values { get; set; }
        [JsonPropertyName("isCurrency")]
        public bool IsCurrency { get; set; }
        [JsonPropertyName("xpath")]
        public string XPath { get; set; }
        [JsonPropertyName("elements")]
        public List<SchemaElement> Elements { get; set; }
    }

}
