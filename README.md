# Xsd To Json Converter
[![Nuget](https://img.shields.io/nuget/v/XSDService)](https://www.nuget.org/packages/XSDService/)

## This will help to convert any XSD to JSON

## Install package

1. Install the `XSDService` NuGet package.
  * .NET CLI
  ```cs
    dotnet add package XSDService --version 1.1.4
  ```
  * PackageManager
  ```cs
  Install-Package XSDService -Version 1.1.4
  ```

## Usage

```C#
using XSDService;

var fileInfo = new FileInfo(fileName);
if (File.Exists(fileName) && fileInfo.Extension.Equals(".xsd"))
{
    XsdToJson xsdLib = new(fileName);
    xsdLib.Convert();
    File.AppendAllText(fileInfo.FullName.Replace(".xsd", ".json"), xsdLib.SchemaJson);
}
```
## Model of JSON
```cs
public class XsdSchema
{
    [JsonPropertyName("namespace")]
    public string Namespace { get; set; }
    [JsonPropertyName("schemaElement")]
    public SchemaElement SchemaElement { get; set; }
}

public class SchemaElement
{
    
    public string Id { get; set; }
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
```
### Example
```json
{
    "namespace": "urn:iso:std:iso:20022:tech:xsd:camt.053.001.10",
    "schemaElement": {
        "id": "Document",
        "name": "Document",
        "dataType": null,
        "minOccurs": "1",
        "maxOccurs": null,
        "minLength": null,
        "maxLength": null,
        "pattern": null,
        "fractionDigits": null,
        "totalDigits": null,
        "minInclusive": null,
        "maxInclusive": null,
        "values": null,
        "isCurrency": false,
        "xpath": "Document",
        "elements":[
          ...
        ]
    }
}
```

## Changes
### Version 1.3.0 release
  * New output model.