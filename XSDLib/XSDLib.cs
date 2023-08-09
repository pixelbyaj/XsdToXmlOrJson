
namespace XSDLib
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Xml;
    using System.Xml.Schema;

    /// <summary>
    /// XSD Converter Library
    /// </summary>
    public class XSDLib
    {
        #region constructor
        public XSDLib(string filePath)
        {
            xmlTextReader = new XmlTextReader(filePath);
            writerSettings = new XmlWriterSettings
            {
                ConformanceLevel = ConformanceLevel.Auto,
                CloseOutput = false
            };
            XPath = new List<string>();
        }
        public XSDLib(string xsdSchema, JsonSerializerOptions options = null)
        {
            xmlTextReader = new XmlTextReader(new StringReader(xsdSchema));
            writerSettings = new XmlWriterSettings
            {
                ConformanceLevel = ConformanceLevel.Auto,
                CloseOutput = false
            };
            XPath = new List<string>();
            if (options == null)
            {
                options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };
            }
            jsonSerializerOptions = options;
        }
        #endregion

        #region private member
        private readonly XmlWriterSettings writerSettings;
        private readonly XmlTextReader xmlTextReader;
        private List<string> XPath;
        private readonly JsonSerializerOptions jsonSerializerOptions;
        #endregion

        #region public member
        public string SchemaJson { get; private set; }
        public SchemaElement SchemaElement { get; private set; }
        #endregion

        #region public method
        /// <summary>
        /// Convert the XSD JSON Object
        /// </summary>
        public void Convert()
        {
            XmlSchema myschema = XmlSchema.Read(xmlTextReader, ValidationCallback);
            XmlSchemaSet schemaSet = new();
            schemaSet.Add(myschema);
            schemaSet.Compile();
            var schemaElement = new SchemaElement
            {
                Name = "Document",
                XPath = "Document",
                Id="document"
            };
            foreach (XmlSchemaElement element in myschema.Elements.Values)
            {
                XPath.Add(element.Name);
                XmlSchemaComplexType complexType = element.ElementSchemaType as XmlSchemaComplexType;
                Iterate(complexType, schemaElement);
                XPath = new List<string>();

            }
         
            SchemaElement = schemaElement;

            SchemaJson = JsonSerializer.Serialize<SchemaElement>(SchemaElement, jsonSerializerOptions);

        }
        #endregion

        #region private method
        private void Iterate(XmlSchemaComplexType complexType, SchemaElement schemaElement)
        {
            if(complexType.BaseXmlSchemaType.BaseXmlSchemaType != null && complexType.BaseXmlSchemaType is XmlSchemaSimpleType)
            {
                GetSimpleType(complexType.BaseXmlSchemaType as XmlSchemaSimpleType,schemaElement);
            }

            if (complexType.AttributeUses.Count > 0)
            {
                IDictionaryEnumerator enumerator =
                    complexType.AttributeUses.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    XmlSchemaAttribute attribute =
                        (XmlSchemaAttribute)enumerator.Value;
                    if (attribute.Name.Equals("Ccy"))
                    {
                        schemaElement.IsCurrency = true;
                        XmlSchemaPatternFacet content = (attribute.AttributeSchemaType.Content as XmlSchemaSimpleTypeRestriction).Facets[0] as XmlSchemaPatternFacet;
                        schemaElement.Pattern = content.Value;
                    }
                }
            }

            // Get the sequence particle of the complex type.
            if (complexType.ContentTypeParticle is XmlSchemaSequence sequence)
            {
                AddSchemaElement(sequence.Items, schemaElement);

            }
            else if (complexType.ContentTypeParticle is XmlSchemaChoice schemaChoice)
            {
                schemaElement.DataType = "choice";
                AddSchemaElement(schemaChoice.Items, schemaElement);
            }
        }
        private void GetComplexType(XmlSchemaComplexType xmlSchemaComplexType, SchemaElement schemaElement)
        {
            Iterate(xmlSchemaComplexType, schemaElement);
        }
        private void AddSchemaElement(XmlSchemaObjectCollection xmlSchemaObjectCollection, SchemaElement schemaElement)
        {
            for (int i = 0; i < xmlSchemaObjectCollection.Count; i++)
            {
                if (xmlSchemaObjectCollection[i] is XmlSchemaElement)
                {
                    var childElement = xmlSchemaObjectCollection[i] as XmlSchemaElement;

                    XPath.Add(childElement.Name);
                    var element = new SchemaElement
                    {
                        Name = childElement.Name,
                        XPath = string.Join("/", XPath.ToArray()),
                        Id = string.Join("_", XPath.ToArray()).ToLower(),
                        MaxOccurs = System.Convert.ToString(childElement.MaxOccurs == Decimal.MaxValue ? "unbounded" : childElement.MaxOccurs),
                        MinOccurs = System.Convert.ToString(childElement.MinOccurs),
                    };

                    if (childElement.ElementSchemaType is XmlSchemaComplexType xmlSchemaComplexType)
                    {
                        GetComplexType(xmlSchemaComplexType, element);

                    }
                    else if (childElement.ElementSchemaType is XmlSchemaSimpleType xmlSchemaSimpleType)
                    {
                        GetSimpleType(xmlSchemaSimpleType, element);
                    }
                    schemaElement.Elements.Add(element);
                    XPath.RemoveAt(XPath.Count - 1);
                }
                else if (xmlSchemaObjectCollection[i] is XmlSchemaAny)
                {
                    XPath.Add("Any");
                    var childElement = xmlSchemaObjectCollection[i] as XmlSchemaAny;

                    var element = new SchemaElement
                    {
                        Name = childElement.Namespace,
                        DataType = "any",
                        XPath = string.Join("/", XPath.ToArray()),
                        Id = string.Join("_", XPath.ToArray()).ToLower()
                    };
                    schemaElement.Elements.Add(element);
                    XPath.RemoveAt(XPath.Count - 1);
                }

            }
        }
        private static void GetSimpleType(XmlSchemaSimpleType xmlSchemaSimpleType, SchemaElement schemaElement)
        {
            schemaElement.DataType = xmlSchemaSimpleType.Datatype.ValueType.Name;
            if (xmlSchemaSimpleType.Content is XmlSchemaSimpleTypeRestriction content)
            {
                GetAttributes(content, schemaElement);
            }
        }
        private static void GetAttributes(XmlSchemaSimpleTypeRestriction xmlSchemaSimpleTypeRestriction, SchemaElement schemaElement)
        {
            var valueList = new List<string>();
            foreach (var item in xmlSchemaSimpleTypeRestriction.Facets)
            {
                if (item is XmlSchemaMinLengthFacet)
                {
                    schemaElement.MinLength = (item as XmlSchemaMinLengthFacet).Value;
                }
                else if (item is XmlSchemaMaxLengthFacet)
                {
                    schemaElement.MaxLength = (item as XmlSchemaMaxLengthFacet).Value;

                }
                else if (item is XmlSchemaPatternFacet)
                {
                    schemaElement.Pattern = (item as XmlSchemaPatternFacet).Value;
                }
                else if (item is XmlSchemaFractionDigitsFacet)
                {
                    schemaElement.FractionDigits = (item as XmlSchemaFractionDigitsFacet).Value;

                }
                else if (item is XmlSchemaTotalDigitsFacet)
                {
                    schemaElement.TotalDigits = (item as XmlSchemaTotalDigitsFacet).Value;

                }
                else if (item is XmlSchemaMinExclusiveFacet)
                {
                    schemaElement.MinInclusive = (item as XmlSchemaMinExclusiveFacet).Value;

                }
                else if (item is XmlSchemaMaxExclusiveFacet)
                {
                    schemaElement.MaxInclusive = (item as XmlSchemaMaxExclusiveFacet).Value;

                }
                else if (item is XmlSchemaEnumerationFacet)
                {
                    valueList.Add((item as XmlSchemaEnumerationFacet).Value);
                }
            }
            if (valueList.Any())
            {
                schemaElement.Values = valueList.ToArray();

            }
        }
        private static void ValidationCallback(object sender, ValidationEventArgs args)
        {

            if (args.Severity == XmlSeverityType.Error)
                throw new System.Exception($"invalid schema: {args.Message} ");
        }
        #endregion
    }
}
