using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public string Name { get; set; }
        public string DataType { get; set; }
        public string MinOccurs { get; set; }
        public string MaxOccurs { get; set; }
        public string MinLength { get; set; }
        public string MaxLength { get; set; }
        public string Pattern { get; set; }
        public string FractionDigits { get; set; }
        public string TotalDigits { get; set; }
        public string MinInclusive { get; set; }
        public string MaxInclusive { get; set; }
        public string[] Values { get; set; }
        public bool IsCurrency { get; set; }
        public string XPath { get; set; }
        public List<SchemaElement> Elements { get; set; }
    }

}
