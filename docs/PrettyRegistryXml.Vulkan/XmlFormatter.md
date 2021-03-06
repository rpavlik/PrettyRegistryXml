# XmlFormatter class

Vulkan-specific policy for formatting XML.

```csharp
public class XmlFormatter : XmlFormatterBase
```

## Public Members

| name | description |
| --- | --- |
| [XmlFormatter](XmlFormatter/XmlFormatter.md)(…) | Constructor |
| override [IndentChars](XmlFormatter/IndentChars.md) { get; } |  |
| override [IndentLevelWidth](XmlFormatter/IndentLevelWidth.md) { get; } |  |
| override [ComputeLevelAdjust](XmlFormatter/ComputeLevelAdjust.md)(…) |  |

## Protected Members

| name | description |
| --- | --- |
| override [CleanWhitespaceNode](XmlFormatter/CleanWhitespaceNode.md)(…) |  |
| override [WriteElement](XmlFormatter/WriteElement.md)(…) | This is the recursive part that contains most of the "policy" |

## See Also

* namespace [PrettyRegistryXml.Vulkan](../PrettyRegistryXml.Vulkan.md)
* [XmlFormatter.cs](../../src/PrettyRegistryXml.Vulkan/XmlFormatter.cs)

<!-- DO NOT EDIT: generated by xmldocmd for PrettyRegistryXml.Vulkan.dll -->
