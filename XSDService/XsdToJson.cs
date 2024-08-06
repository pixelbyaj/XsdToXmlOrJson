
namespace XSDService
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization.Metadata;
    using System.Xml;
    using System.Xml.Schema;

    /// <summary>
    /// XSD Converter Library
    /// </summary>
    public class XsdToJson
    {
        #region constructor
        public XsdToJson(string filePath)
        {
            ArgumentNullException.ThrowIfNull(filePath, nameof(filePath));
            xmlTextReader = new XmlTextReader(filePath);
            writerSettings = new XmlWriterSettings
            {
                ConformanceLevel = ConformanceLevel.Auto,
                CloseOutput = false
            };
            XPath = new List<string>();
        }
        public XsdToJson(Stream input)
        {
            ArgumentNullException.ThrowIfNull(input, nameof(input));
            xmlTextReader = new XmlTextReader(input);
            writerSettings = new XmlWriterSettings
            {
                ConformanceLevel = ConformanceLevel.Auto,
                CloseOutput = false
            };
            XPath = new List<string>();
        }
        public XsdToJson(TextReader input)
        {
            ArgumentNullException.ThrowIfNull(input, nameof(input));
            xmlTextReader = new XmlTextReader(input);
            writerSettings = new XmlWriterSettings
            {
                ConformanceLevel = ConformanceLevel.Auto,
                CloseOutput = false
            };
            XPath = new List<string>();
        }
        public XsdToJson(string xsdSchema, JsonSerializerOptions options = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(xsdSchema, nameof(xsdSchema));
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
                    WriteIndented = true,
                    TypeInfoResolver = new DefaultJsonTypeInfoResolver()
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
        const int MAX_DEPTH = 64;
        #endregion

        #region public member

        public string SchemaJson { get; private set; }
        public XsdSchema XsdSchema { get; private set; }
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
                Id = "document",
                MinOccurs = "1"
            };

            foreach (XmlSchemaElement element in myschema.Elements.Values)
            {
                schemaElement.Name = element.Name;
                schemaElement.Id = element.Name;
                schemaElement.XPath = element.Name;
                XPath.Add(element.Name);
                XmlSchemaComplexType complexType = element.ElementSchemaType as XmlSchemaComplexType;
                Iterate(complexType, schemaElement);
            }
            SchemaElement = schemaElement;
            XsdSchema xsd = new()
            {
                Namespace = myschema.TargetNamespace,
                SchemaElement = schemaElement
            };

            SchemaJson = JsonSerializer.Serialize<XsdSchema>(xsd, XsdSchemaContext.Default.XsdSchema);

        }
        #endregion

        #region private method
        private void Iterate(XmlSchemaComplexType complexType, SchemaElement schemaElement)
        {
            if (complexType.BaseXmlSchemaType.BaseXmlSchemaType != null && complexType.BaseXmlSchemaType is XmlSchemaSimpleType)
            {
                GetSimpleType(complexType.BaseXmlSchemaType as XmlSchemaSimpleType, schemaElement);
            }

            if (complexType.AttributeUses.Count > 0)
            {
                IDictionaryEnumerator enumerator =
                    complexType.AttributeUses.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    XmlSchemaAttribute attribute =
                        (XmlSchemaAttribute)enumerator.Value;
                    if (!string.IsNullOrEmpty(attribute.Name) && attribute.Name.Equals("Ccy"))
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
                    if (XPath.Count >= MAX_DEPTH)
                    {
                        throw new JsonException($"CurrentDepth {MAX_DEPTH} is equal to or larger than the maximum allowed depth of {MAX_DEPTH}");
                    }
                    var element = new SchemaElement
                    {
                        Name = childElement.Name,
                        XPath = string.Join("/", XPath.ToArray()),
                        Id = string.Join("_", XPath.ToArray()),
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
                else if (xmlSchemaObjectCollection[i] is XmlSchemaChoice)
                {
                    var childElement = xmlSchemaObjectCollection[i] as XmlSchemaChoice;
                    var element = new SchemaElement
                    {
                        DataType = "choice"
                    };
                    AddSchemaElement(childElement.Items, element);
                    schemaElement.Elements.Add(element);
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
                        Id = string.Join("_", XPath.ToArray())
                    };
                    schemaElement.Elements.Add(element);
                    XPath.RemoveAt(XPath.Count - 1);
                }

            }
        }
        private static void GetSimpleType(XmlSchemaSimpleType xmlSchemaSimpleType, SchemaElement schemaElement)
        {
            schemaElement.DataType = xmlSchemaSimpleType.Datatype.TypeCode.ToString().ToLower();
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
