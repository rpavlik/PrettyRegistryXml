# AttributeGroup class

A list of attribute names, usually combined in a [`GroupChoice`](./GroupChoice.md). They will all be aligned together.

```csharp
public class AttributeGroup : AttributeSequenceItemBase
```

## Public Members

| name | description |
| --- | --- |
| [AttributeGroup](AttributeGroup/AttributeGroup.md)(…) | Create a group of attributes that will all be aligned (or replaced with placeholder spaces) (2 constructors) |
| [AttributeNames](AttributeGroup/AttributeNames.md) { get; set; } |  |
| [AttributeNameSet](AttributeGroup/AttributeNameSet.md) { get; } |  |
| [ExtraSpace](AttributeGroup/ExtraSpace.md) { get; } | Extra space to add to this attribute group's width. |
| override [CountHandledAttributes](AttributeGroup/CountHandledAttributes.md)(…) |  |
| override [CreateWidthComputer](AttributeGroup/CreateWidthComputer.md)() |  |
| override [ToString](AttributeGroup/ToString.md)() | Convert to string |

## See Also

* class [AttributeSequenceItemBase](./AttributeSequenceItemBase.md)
* namespace [PrettyRegistryXml.GroupedAlignment](../PrettyRegistryXml.GroupedAlignment.md)
* [AttributeGroup.cs](../../src/PrettyRegistryXml.GroupedAlignment/AttributeGroup.cs)

<!-- DO NOT EDIT: generated by xmldocmd for PrettyRegistryXml.GroupedAlignment.dll -->
