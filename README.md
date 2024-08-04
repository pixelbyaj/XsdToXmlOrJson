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
    public string Namespace { get; set; }
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
## Version 1.3.1 release
  * Convert JSON to ISO20022 XML Message 
## Usage

```c#

using XSDService;
string targetNamespace = "urn:iso:std:iso:20022:tech:xsd:camt.053.001.10";
string jsonData = File.ReadAllText(@jsonPath);
string xsdContent = File.ReadAllText(@xsdFilePath);
XElement xml = Iso20022.Convert(jsonData, targetNamespace) ?? throw new Exception("Conversion failed");
if (Iso20022.ValidateMXMessage(xsdContent, xml.ToString(), out string validationMessage))
{
    if (string.IsNullOrEmpty(validationMessage))
    {
        Console.WriteLine(xml?.ToString());
    }
    else
    {
       Console.Error.WriteLine(validationMessage);
    }
}
```
### Example 

  ```json
  {
    "Document": {
        "Document_BkToCstmrStmt": {
            "Document_BkToCstmrStmt_GrpHdr": {
                "Document_BkToCstmrStmt_GrpHdr_MsgId": "235549650",
                "Document_BkToCstmrStmt_GrpHdr_CreDtTm": "2023-10-05T14:43:51.979",
                "Document_BkToCstmrStmt_GrpHdr_MsgRcpt": {
                    "Document_BkToCstmrStmt_GrpHdr_MsgRcpt_Nm": "Test Client Ltd.",
                    "Document_BkToCstmrStmt_GrpHdr_MsgRcpt_Id": {
                        "Document_BkToCstmrStmt_GrpHdr_MsgRcpt_Id_OrgId": {
                            "Document_BkToCstmrStmt_GrpHdr_MsgRcpt_Id_OrgId_Othr": [
                                {
                                    "Document_BkToCstmrStmt_GrpHdr_MsgRcpt_Id_OrgId_Othr_Id": "test001"
                                }
                            ]
                        }
                    }
                },
                "Document_BkToCstmrStmt_GrpHdr_AddtlInf": "AddTInf"
            },
            "Document_BkToCstmrStmt_Stmt": [
                {
                    "Document_BkToCstmrStmt_Stmt_Id": "258158850",
                    "Document_BkToCstmrStmt_Stmt_ElctrncSeqNb": "1",
                    "Document_BkToCstmrStmt_Stmt_LglSeqNb": "1",
                    "Document_BkToCstmrStmt_Stmt_CreDtTm": "2023-10-05T14:43:52.098",
                    "Document_BkToCstmrStmt_Stmt_FrToDt": {
                        "Document_BkToCstmrStmt_Stmt_FrToDt_FrDtTm": "2023-09-30T20:00:00.000",
                        "Document_BkToCstmrStmt_Stmt_FrToDt_ToDtTm": "2023-10-01T19:59:59.000"
                    },
                    "Document_BkToCstmrStmt_Stmt_Acct": {
                        "Document_BkToCstmrStmt_Stmt_Acct_Tp": {
                            "Document_BkToCstmrStmt_Stmt_Acct_Tp_Prtry": "IBDA_DDA"
                        },
                        "Document_BkToCstmrStmt_Stmt_Acct_Ccy": "USD",
                        "Document_BkToCstmrStmt_Stmt_Acct_Nm": "Sample Name 123",
                        "Document_BkToCstmrStmt_Stmt_Acct_Svcr": {
                            "Document_BkToCstmrStmt_Stmt_Acct_Svcr_FinInstnId": {
                                "Document_BkToCstmrStmt_Stmt_Acct_Svcr_FinInstnId_BICFI": "GSCRUS30",
                                "Document_BkToCstmrStmt_Stmt_Acct_Svcr_FinInstnId_Nm": "Goldman Sachs Bank"
                            }
                        }
                    },
                    "Document_BkToCstmrStmt_Stmt_Bal": [
                        {
                            "Document_BkToCstmrStmt_Stmt_Bal_Tp": {
                                "Document_BkToCstmrStmt_Stmt_Bal_Tp_CdOrPrtry": {
                                    "Document_BkToCstmrStmt_Stmt_Bal_Tp_CdOrPrtry_Cd": "OPBD"
                                }
                            },
                            "Document_BkToCstmrStmt_Stmt_Bal_Amt": {
                                "Document_BkToCstmrStmt_Stmt_Bal_Amt_Ccy": "USD",
                                "Document_BkToCstmrStmt_Stmt_Bal_Amt_Amt": "843686.20"
                            },
                            "Document_BkToCstmrStmt_Stmt_Bal_CdtDbtInd": "DBIT",
                            "Document_BkToCstmrStmt_Stmt_Bal_Dt": {
                                "Document_BkToCstmrStmt_Stmt_Bal_Dt_DtTm": "2023-09-30T20:00:00.000"
                            }
                        },
                        {
                            "Document_BkToCstmrStmt_Stmt_Bal_Tp": {
                                "Document_BkToCstmrStmt_Stmt_Bal_Tp_CdOrPrtry": {
                                    "Document_BkToCstmrStmt_Stmt_Bal_Tp_CdOrPrtry_Cd": "CLAV"
                                }
                            },
                            "Document_BkToCstmrStmt_Stmt_Bal_Amt": {
                                "Document_BkToCstmrStmt_Stmt_Bal_Amt_Ccy": "USD",
                                "Document_BkToCstmrStmt_Stmt_Bal_Amt_Amt": "334432401.27"
                            },
                            "Document_BkToCstmrStmt_Stmt_Bal_CdtDbtInd": "CRDT",
                            "Document_BkToCstmrStmt_Stmt_Bal_Dt": {
                                "Document_BkToCstmrStmt_Stmt_Bal_Dt_DtTm": "2023-10-01T23:59:00.000Z"
                            }
                        }
                    ]
                }
            ]
        }
    }
}
  ```
After Conversion
  ```xml
  <Document xmlns="urn:iso:std:iso:20022:tech:xsd:camt.053.001.10">
  <BkToCstmrStmt>
    <GrpHdr>
      <MsgId>235549650</MsgId>
      <CreDtTm>2023-10-05T14:43:51.979</CreDtTm>
      <MsgRcpt>
        <Nm>Test Client Ltd.</Nm>
        <Id>
          <OrgId>
            <Othr>
              <Id>test001</Id>
            </Othr>
          </OrgId>
        </Id>
      </MsgRcpt>
      <AddtlInf>AddTInf</AddtlInf>
    </GrpHdr>
    <Stmt>
      <Id>258158850</Id>
      <ElctrncSeqNb>1</ElctrncSeqNb>
      <LglSeqNb>1</LglSeqNb>
      <CreDtTm>2023-10-05T14:43:52.098</CreDtTm>
      <FrToDt>
        <FrDtTm>2023-09-30T20:00:00.000</FrDtTm>
        <ToDtTm>2023-10-01T19:59:59.000</ToDtTm>
      </FrToDt>
      <Acct>
        <Tp>
          <Prtry>IBDA_DDA</Prtry>
        </Tp>
        <Ccy>USD</Ccy>
        <Nm>Sample Name 123</Nm>
        <Svcr>
          <FinInstnId>
            <BICFI>GSCRUS30</BICFI>
            <Nm>Goldman Sachs Bank</Nm>
          </FinInstnId>
        </Svcr>
      </Acct>
      <Bal>
        <Tp>
          <CdOrPrtry>
            <Cd>OPBD</Cd>
          </CdOrPrtry>
        </Tp>
        <Amt Ccy="USD">843686.20</Amt>
        <CdtDbtInd>DBIT</CdtDbtInd>
        <Dt>
          <DtTm>2023-09-30T20:00:00.000</DtTm>
        </Dt>
      </Bal>
      <Bal>
        <Tp>
          <CdOrPrtry>
            <Cd>CLAV</Cd>
          </CdOrPrtry>
        </Tp>
        <Amt Ccy="USD">334432401.27</Amt>
        <CdtDbtInd>CRDT</CdtDbtInd>
        <Dt>
          <DtTm>2023-10-01T23:59:00.000Z</DtTm>
        </Dt>
      </Bal>
    </Stmt>
  </BkToCstmrStmt>
</Document>

  ```