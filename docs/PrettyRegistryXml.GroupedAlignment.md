# PrettyRegistryXml.GroupedAlignment assembly

## PrettyRegistryXml.GroupedAlignment namespace

| public type | description |
| --- | --- |
| class [AlignedTrailer](PrettyRegistryXml.GroupedAlignment/AlignedTrailer.md) | Alignment of the longest set of trailing attributes, just as in SimpleAlignment |
| class [AttributeGroup](PrettyRegistryXml.GroupedAlignment/AttributeGroup.md) | A list of attribute names, usually combined in a [`GroupChoice`](PrettyRegistryXml.GroupedAlignment/GroupChoice.md) |
| abstract class [AttributeSequenceItemBase](PrettyRegistryXml.GroupedAlignment/AttributeSequenceItemBase.md) | Base class for non-trailer implementations of [`IAttributeSequenceItem`](PrettyRegistryXml.GroupedAlignment/IAttributeSequenceItem.md) |
| abstract class [AttributeSequenceTrailerBase](PrettyRegistryXml.GroupedAlignment/AttributeSequenceTrailerBase.md) | Base class for trailer implementations of [`IAttributeSequenceItem`](PrettyRegistryXml.GroupedAlignment/IAttributeSequenceItem.md) |
| class [GroupChoice](PrettyRegistryXml.GroupedAlignment/GroupChoice.md) | A choice between some disjoint collections of attribute names represented by [`AttributeGroup`](PrettyRegistryXml.GroupedAlignment/AttributeGroup.md) |
| class [GroupedAttributeAlignment](PrettyRegistryXml.GroupedAlignment/GroupedAttributeAlignment.md) | A more complex alignment: there are groups of attributes that may alternate. |
| interface [IAttributeSequenceItem](PrettyRegistryXml.GroupedAlignment/IAttributeSequenceItem.md) | Configuration for a single item in the attribute sequence, which may be a choice between groups of attributes, or just a group of attributes. |
| interface [IAttributeSequenceItemAligner](PrettyRegistryXml.GroupedAlignment/IAttributeSequenceItemAligner.md) | The result of [`IAttributeSequenceItemWidthComputer`](PrettyRegistryXml.GroupedAlignment/IAttributeSequenceItemWidthComputer.md) when it is all done and ready to actually align. |
| interface [IAttributeSequenceItemWidthComputer](PrettyRegistryXml.GroupedAlignment/IAttributeSequenceItemWidthComputer.md) | The state of an [`IAttributeSequenceItem`](PrettyRegistryXml.GroupedAlignment/IAttributeSequenceItem.md) while it is determining the alignment widths. |
| class [NameLengthPair](PrettyRegistryXml.GroupedAlignment/NameLengthPair.md) | An attribute name and length |
| class [UnalignedTrailer](PrettyRegistryXml.GroupedAlignment/UnalignedTrailer.md) | Unaligned handling of the trailing attributes. |

<!-- DO NOT EDIT: generated by xmldocmd for PrettyRegistryXml.GroupedAlignment.dll -->