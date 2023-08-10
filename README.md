# Xsd To Json Converter
[![Nuget](https://img.shields.io/nuget/v/XSDService)](https://www.nuget.org/packages/XSDService/)

## This will help to convert any XSD to JSON

## Install package

1. Install the `XSDService` NuGet package.
  * .NET CLI
  ```cs
    dotnet add package XSDService --version 1.0.0
  ```
  * PackageManager
  ```cs
  Install-Package XSDService -Version 1.0.0
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
```json
{
    id: string;
    name: string;
    dataType: string;
    minOccurs: string;
    maxOccurs: string;
    minLength: string;
    maxLength: string;
    pattern: string;
    fractionDigits: string;
    totalDigits: string;
    minInclusive: string;
    maxInclusive: string;
    values: string[];
    isCurrency: boolean;
    xpath: string;
    elements: SchemaElement[];
    children: SchemaElement[];
}

```
