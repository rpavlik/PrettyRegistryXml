# XmlRoundtripper.ParseAndLoad method (1 of 2)

Parse an XML file, and also load it into an XmlRoundtripper. This overload assumes UTF-8.

```csharp
public static XmlRoundtripper ParseAndLoad(string filename, out XDocument document)
```

| parameter | description |
| --- | --- |
| filename | Path of an XML file to parse |
| document | The document object to populate |

## Return Value

An object you can use to restore the header lines when writing out your document again.

## See Also

* class [XmlRoundtripper](../XmlRoundtripper.md)
* namespace [PrettyRegistryXml.Core](../../PrettyRegistryXml.Core.md)

---

# XmlRoundtripper.ParseAndLoad method (2 of 2)

Parse an XML file, and also load it into an XmlRoundtripper. This overload assumes UTF-8

```csharp
public static XmlRoundtripper ParseAndLoad(string filename, Encoding encoding, 
    out XDocument document)
```

| parameter | description |
| --- | --- |
| filename | Path of an XML file to parse |
| encoding | File encoding for reading and writing |
| document | The document object to populate |

## Return Value

An object you can use to restore the header lines when writing out your document again.

## See Also

* class [XmlRoundtripper](../XmlRoundtripper.md)
* namespace [PrettyRegistryXml.Core](../../PrettyRegistryXml.Core.md)

<!-- DO NOT EDIT: generated by xmldocmd for PrettyRegistryXml.Core.dll -->