using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml;

namespace XSDService
{
    public static class Iso20022
    {

        #region public
        /// <summary>
        /// Convert given json object to ISO 20022 XML Message
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <param name="schemaNamespace"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static XElement Convert(string jsonModel, string schemaNamespace)
        {
            ArgumentException.ThrowIfNullOrEmpty(jsonModel, nameof(jsonModel));
            ArgumentException.ThrowIfNullOrEmpty(schemaNamespace, nameof(schemaNamespace));

            XElement xml = null;
            try
            {
                JsonNode jsonNode = JsonNode.Parse(jsonModel);
                if (jsonNode != null)
                {
                    JsonObject jsonObj = jsonNode.AsObject();
                    if (jsonObj == null || jsonObj.Count == 0)
                        throw new ArgumentException("JSON object is empty or null");

                    foreach (var property in jsonObj)
                    {
                        xml = ToXml(property.Value, property.Key, xml, schemaNamespace);
                    }

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return xml;
        }

        /// <summary>
        /// Validate MX or ISO 20022 Message
        /// </summary>
        /// <param name="xsdContent"></param>
        /// <param name="xmlMessage"></param>
        /// <param name="validationMessage"></param>
        /// <returns></returns>
        public static bool ValidateMXMessage(string xsdContent, string xmlMessage, out string validationMessage)
        {
            ArgumentException.ThrowIfNullOrEmpty(xsdContent, nameof(xsdContent));
            ArgumentException.ThrowIfNullOrEmpty(xmlMessage, nameof(xmlMessage));
            validationMessage = string.Empty;
            string fValidationMessage = string.Empty;
            bool isValid = true;
            string targetNamespace = GetTargetNamespaceFromXsd(xsdContent);
            XmlSchemaSet schemaSet = new();
            schemaSet.Add(targetNamespace, XmlReader.Create(new StringReader(xsdContent)));

            XmlReaderSettings settings = new();
            settings.Schemas.Add(schemaSet);
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += (sender, e) =>
            {
                isValid = false;
                fValidationMessage = e.Message;
            };

            using (XmlReader reader = XmlReader.Create(new StringReader(xmlMessage), settings))
            {
                try
                {
                    while (reader.Read()) { }
                }
                catch (XmlException ex)
                {
                    isValid = false;
                    validationMessage = ex.Message;
                }
            }
            validationMessage = fValidationMessage;
            return isValid;
        }
        #endregion

        #region private methods
        private static XElement ToXml(JsonNode jsonNode, string nodeName, XElement parentElement, string schemaNamespace)
        {
            if (jsonNode is JsonObject jsonObject)
            {
                XElement element;
                if (!string.IsNullOrEmpty(schemaNamespace))
                {
                    element = new(XNamespace.Get(schemaNamespace) + nodeName);
                }
                else
                {
                    element = new(nodeName);
                }
                bool isCurrency = false;
                foreach (var property in jsonObject)
                {
                    string key = GetNodeName(property.Key);
                    if (key == "Ccy" && jsonObject.Count() == 2 && jsonObject.Any(c => GetNodeName(c.Key) == "Amt"))
                    {
                        isCurrency = true;
                        element.SetAttributeValue(key, property.Value);
                    }
                    else if (isCurrency)
                    {
                        element.Value = property.Value?.ToString() ?? string.Empty;
                        isCurrency = false;
                    }
                    else
                    {
                        XElement childElement = ToXml(property.Value, GetNodeName(property.Key), element, schemaNamespace);
                        if (childElement != null)
                        {
                            element.Add(childElement);
                        }
                    }
                }

                return element;
            }
            else if (jsonNode is JsonArray jsonArray)
            {
                foreach (var item in jsonArray)
                {
                    parentElement?.Add(ToXml(item, GetNodeName(nodeName), parentElement, schemaNamespace));
                }
                return null;
            }
            else
            {
                string fNodeName = GetNodeName(nodeName);
                return new XElement(XNamespace.Get(schemaNamespace) + fNodeName, jsonNode?.ToString());
            }
        }
        private static string GetTargetNamespaceFromXsd(string xsdContent)
        {
            using (XmlReader reader = XmlReader.Create(new System.IO.StringReader(xsdContent)))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "xs:schema")
                    {
                        return reader.GetAttribute("targetNamespace");
                    }
                }
            }

            throw new Exception("targetNamespace not found in XSD content.");
        }
        private static string GetNodeName(string nodeName)
        {
            string[] nodeNames = nodeName.Split("_");
            string fnodeName = nodeNames[^1];
            return fnodeName;
        }
        #endregion
    }
}
