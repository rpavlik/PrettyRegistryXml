# IAttributeSequenceItemAligner.TakeAndHandleAttributes method

Process what we can, outputting appropriate alignments, and remove them from consideration.

```csharp
public bool TakeAndHandleAttributes(IEnumerable<string> attributeNames, 
    out IEnumerable<AttributeAlignment> alignments, out IEnumerable<string> remainingNames)
```

| parameter | description |
| --- | --- |
| attributeNames | The unhandled attribute names left in an element |
| alignments | Output to be populated with alignments, if any apply. |
| remainingNames | A sequence containing items in *attributeNames* we didn't handle, if any, preferably in their original order |

## Return Value

true if we handled any and populated *alignments*

## See Also

* interface [IAttributeSequenceItemAligner](../IAttributeSequenceItemAligner.md)
* namespace [PrettyRegistryXml.GroupedAlignment](../../PrettyRegistryXml.GroupedAlignment.md)

<!-- DO NOT EDIT: generated by xmldocmd for PrettyRegistryXml.GroupedAlignment.dll -->
