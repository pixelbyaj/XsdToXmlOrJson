
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
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
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
        public string SchemaXML { get; private set; }
        public string SchemaJson { get; private set; }
        public SchemaElement SchemaElement { get; private set; }
        #endregion

        #region public method
        /// <summary>
        /// Convert the XSD to XML, JSON and SchemaElement Object
        /// </summary>
        public void Convert()
        {
            XmlSchema myschema = XmlSchema.Read(xmlTextReader, ValidationCallback);
            XmlSchemaSet schemaSet = new();
            schemaSet.Add(myschema);
            schemaSet.Compile();
            StringBuilder stringBuilder = new();
            using XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, writerSettings);
            xmlWriter.WriteStartDocument();
            var schemaElement = new SchemaElement
            {
                Name = "Document",
                XPath = "Document"
            };
            foreach (XmlSchemaElement element in myschema.Elements.Values)
            {
                xmlWriter.WriteStartElement(element.Name);
                XPath.Add(element.Name);
                XmlSchemaComplexType complexType = element.ElementSchemaType as XmlSchemaComplexType;
                Iterate(complexType, xmlWriter, schemaElement);
                xmlWriter.WriteEndElement();
                XPath = new List<string>();

            }
            xmlWriter.WriteEndDocument();
            xmlWriter.Flush();
            SchemaXML = stringBuilder.ToString();
            SchemaElement = schemaElement;

            SchemaJson = JsonSerializer.Serialize(SchemaElement, typeof(SchemaElement), jsonSerializerOptions);

        }
        #endregion

        #region private method
        private void Iterate(XmlSchemaComplexType complexType, XmlWriter xmlWriter, SchemaElement schemaElement)
        {
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
                        var content = (attribute.AttributeSchemaType.Content as XmlSchemaSimpleTypeRestriction).Facets[0];
                        xmlWriter.WriteAttributeString(attribute.Name, (content as XmlSchemaPatternFacet).Value);
                        schemaElement.IsCurrency = true;
                    }
                }
            }

            // Get the sequence particle of the complex type.
            if (complexType.ContentTypeParticle is XmlSchemaSequence sequence)
            {
                AddSchemaElement(sequence.Items, xmlWriter, schemaElement);

            }
            else if (complexType.ContentTypeParticle is XmlSchemaChoice schemaChoice)
            {
                xmlWriter.WriteAttributeString("dataType", "choice");
                schemaElement.DataType = "choice";
                AddSchemaElement(schemaChoice.Items, xmlWriter, schemaElement);
            }
        }
        private void GetComplexType(XmlSchemaComplexType xmlSchemaComplexType, XmlWriter xmlWriter, SchemaElement schemaElement)
        {
            var contentTypeParticle = xmlSchemaComplexType.ContentTypeParticle;
            if (!string.IsNullOrEmpty(contentTypeParticle.MinOccursString))
            {
                xmlWriter.WriteAttributeString("minOccurs", contentTypeParticle.MinOccursString);
                schemaElement.MinOccurs = contentTypeParticle.MinOccursString;
            }
            if (!string.IsNullOrEmpty(contentTypeParticle.MaxOccursString))
            {
                xmlWriter.WriteAttributeString("maxOccurs", contentTypeParticle.MaxOccursString);
                schemaElement.MaxOccurs = contentTypeParticle.MinOccursString;
            }
            Iterate(xmlSchemaComplexType, xmlWriter, schemaElement);
        }
        private void AddSchemaElement(XmlSchemaObjectCollection xmlSchemaObjectCollection, XmlWriter xmlWriter, SchemaElement schemaElement)
        {
            for (int i = 0; i < xmlSchemaObjectCollection.Count; i++)
            {
                if (xmlSchemaObjectCollection[i] is XmlSchemaElement)
                {
                    var childElement = xmlSchemaObjectCollection[i] as XmlSchemaElement;

                    XPath.Add(childElement.Name);
                    xmlWriter.WriteStartElement(childElement.Name);
                    var element = new SchemaElement
                    {
                        Name = childElement.Name,
                        XPath = string.Join("/", XPath.ToArray())
                    };

                    if (childElement.ElementSchemaType is XmlSchemaComplexType xmlSchemaComplexType)
                    {
                        GetComplexType(xmlSchemaComplexType, xmlWriter, element);

                    }
                    else if (childElement.ElementSchemaType is XmlSchemaSimpleType xmlSchemaSimpleType)
                    {
                        xmlWriter.WriteAttributeString("xPath", string.Join("/", XPath.ToArray()));
                        // schemaElement.XPath = string.Join("/", XPath.ToArray());
                        GetSimpleType(xmlSchemaSimpleType, xmlWriter, element);
                    }
                    xmlWriter.WriteEndElement();
                    schemaElement.Elements.Add(element);
                    XPath.RemoveAt(XPath.Count - 1);
                }
                else if (xmlSchemaObjectCollection[i] is XmlSchemaAny)
                {
                    xmlWriter.WriteStartElement("Any");
                    XPath.Add("Any");
                    xmlWriter.WriteEndElement();
                }

            }
        }
        private static void GetSimpleType(XmlSchemaSimpleType xmlSchemaSimpleType, XmlWriter xmlWriter, SchemaElement schemaElement)
        {
            xmlWriter.WriteAttributeString("dataType", xmlSchemaSimpleType.Datatype.ValueType.Name);
            schemaElement.DataType = xmlSchemaSimpleType.Datatype.ValueType.Name;
            if (xmlSchemaSimpleType.Content is XmlSchemaSimpleTypeRestriction content)
            {
                GetAttributes(content, xmlWriter, schemaElement);
            }
        }
        private static void GetAttributes(XmlSchemaSimpleTypeRestriction xmlSchemaSimpleTypeRestriction, XmlWriter xmlWriter, SchemaElement schemaElement)
        {
            var valueList = new List<string>();
            foreach (var item in xmlSchemaSimpleTypeRestriction.Facets)
            {
                if (item is XmlSchemaMinLengthFacet)
                {
                    xmlWriter.WriteAttributeString("minLength", (item as XmlSchemaMinLengthFacet).Value);
                    schemaElement.MinLength = (item as XmlSchemaMinLengthFacet).Value;
                }
                else if (item is XmlSchemaMaxLengthFacet)
                {
                    xmlWriter.WriteAttributeString("maxLength", (item as XmlSchemaMaxLengthFacet).Value);
                    schemaElement.MaxLength = (item as XmlSchemaMaxLengthFacet).Value;

                }
                else if (item is XmlSchemaPatternFacet)
                {
                    xmlWriter.WriteAttributeString("pattern", (item as XmlSchemaPatternFacet).Value);
                    schemaElement.Pattern = (item as XmlSchemaPatternFacet).Value;
                }
                else if (item is XmlSchemaFractionDigitsFacet)
                {
                    xmlWriter.WriteAttributeString("fractionDigits", (item as XmlSchemaFractionDigitsFacet).Value);
                    schemaElement.FractionDigits = (item as XmlSchemaFractionDigitsFacet).Value;

                }
                else if (item is XmlSchemaTotalDigitsFacet)
                {
                    xmlWriter.WriteAttributeString("totalDigits", (item as XmlSchemaTotalDigitsFacet).Value);
                    schemaElement.TotalDigits = (item as XmlSchemaTotalDigitsFacet).Value;

                }
                else if (item is XmlSchemaMinExclusiveFacet)
                {
                    xmlWriter.WriteAttributeString("minInclusive", (item as XmlSchemaMinExclusiveFacet).Value);
                    schemaElement.MinInclusive = (item as XmlSchemaMinExclusiveFacet).Value;

                }
                else if (item is XmlSchemaMaxExclusiveFacet)
                {
                    xmlWriter.WriteAttributeString("maxExclusive", (item as XmlSchemaMaxExclusiveFacet).Value);
                    schemaElement.MaxInclusive = (item as XmlSchemaMaxExclusiveFacet).Value;

                }
                else if (item is XmlSchemaEnumerationFacet)
                {
                    valueList.Add((item as XmlSchemaEnumerationFacet).Value);
                }
            }
            if (valueList.Any())
            {
                xmlWriter.WriteAttributeString("value", string.Join(",", valueList.ToArray()));
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
