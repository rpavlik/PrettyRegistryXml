# XmlFormatterBase.WriteAlignedAttrs method (1 of 2)

Write attributes, aligned as indicated, to the writer with the associated StringBuilder.

```csharp
public static void WriteAlignedAttrs(XmlWriter writer, XElement e, IAlignmentState alignment, 
    StringBuilder sb)
```

| parameter | description |
| --- | --- |
| writer | Your writer |
| e | Element whose attributes we should write |
| alignment | Your alignment state |
| sb | The StringBuilder that *writer* writes to |

## See Also

* interface [IAlignmentState](../IAlignmentState.md)
* class [XmlFormatterBase](../XmlFormatterBase.md)
* namespace [PrettyRegistryXml.Core](../../PrettyRegistryXml.Core.md)

---

# XmlFormatterBase.WriteAlignedAttrs method (2 of 2)

Write attributes, aligned as indicated, to the writer with the associated StringBuilder.

```csharp
public static void WriteAlignedAttrs(XmlWriter writer, XElement e, 
    IEnumerable<AttributeAlignment> alignments, StringBuilder sb)
```

| parameter | description |
| --- | --- |
| writer | Your writer |
| e | Element whose attributes we should write |
| alignments | Array of alignments |
| sb | The StringBuilder that *writer* writes to |

## See Also

* struct [AttributeAlignment](../AttributeAlignment.md)
* class [XmlFormatterBase](../XmlFormatterBase.md)
* namespace [PrettyRegistryXml.Core](../../PrettyRegistryXml.Core.md)

<!-- DO NOT EDIT: generated by xmldocmd for PrettyRegistryXml.Core.dll -->