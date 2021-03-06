# XmlUtilities class

Assorted utilities acting on objects from System.Xml.Linq

```csharp
public static class XmlUtilities
```

## Public Members

| name | description |
| --- | --- |
| static [IsWhitespaceBetweenSelectedElements](XmlUtilities/IsWhitespaceBetweenSelectedElements.md)(…) | See if this node is whitespace, with the immediately previous and following nodes both XElement objects meeting the predicate |
| static [IsWhitespaceOrCommentBetweenSelectedElements](XmlUtilities/IsWhitespaceOrCommentBetweenSelectedElements.md)(…) | See if this node is a comment or whitespace, with the nearest sibling XElement objects (preceding and following) both existing and passing the predicate |
| static [NeighboringElementsMeetPredicate](XmlUtilities/NeighboringElementsMeetPredicate.md)(…) | See if the closest neighboring elements meeting the predicate |
| static [NeighboringNodesAreElementsMeetingPredicate](XmlUtilities/NeighboringNodesAreElementsMeetingPredicate.md)(…) | See if the closest neighboring nodes are elements that pass the predicate |
| static [NodeIsWhitespaceText](XmlUtilities/NodeIsWhitespaceText.md)(…) | See if this node is just whitespace. |

## See Also

* namespace [PrettyRegistryXml.Core](../PrettyRegistryXml.Core.md)
* [XmlUtilities.cs](../../src/PrettyRegistryXml.Core/XmlUtilities.cs)

<!-- DO NOT EDIT: generated by xmldocmd for PrettyRegistryXml.Core.dll -->
