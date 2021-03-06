# XmlUtilities.NeighboringElementsMeetPredicate method

See if the closest neighboring elements meeting the predicate

```csharp
public static bool NeighboringElementsMeetPredicate(XNode node, 
    Predicate<XElement> elementPredicate)
```

| parameter | description |
| --- | --- |
| node | A non-null XNode |
| elementPredicate | A predicate on XElement |

## Return Value

true if this node's immediately preceding and following XElement both exist and succeed in *elementPredicate*

## See Also

* class [XmlUtilities](../XmlUtilities.md)
* namespace [PrettyRegistryXml.Core](../../PrettyRegistryXml.Core.md)

<!-- DO NOT EDIT: generated by xmldocmd for PrettyRegistryXml.Core.dll -->
