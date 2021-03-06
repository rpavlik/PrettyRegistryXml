# BaseReturnCodeSorterWithSpecialCodes class

Base utility class for sorting return codes when some are "special", with some provided codes always in a given order, and the rest alphabetical after that.

```csharp
public abstract class BaseReturnCodeSorterWithSpecialCodes : BaseReturnCodeSorter
```

## Public Members

| name | description |
| --- | --- |
| [BaseReturnCodeSorterWithSpecialCodes](BaseReturnCodeSorterWithSpecialCodes/BaseReturnCodeSorterWithSpecialCodes.md)() | Constructor - processes your PresorterSpecialCodes |
| abstract [PresortedSpecialCodes](BaseReturnCodeSorterWithSpecialCodes/PresortedSpecialCodes.md) { get; } |  |
| override [SortReturnCodes](BaseReturnCodeSorterWithSpecialCodes/SortReturnCodes.md)(…) | Sorts an enumerable of return codes. |

## See Also

* class [BaseReturnCodeSorter](./BaseReturnCodeSorter.md)
* namespace [PrettyRegistryXml.Core](../PrettyRegistryXml.Core.md)
* [BaseReturnCodeSorterWithSpecialCodes.cs](../../src/PrettyRegistryXml.Core/BaseReturnCodeSorterWithSpecialCodes.cs)

<!-- DO NOT EDIT: generated by xmldocmd for PrettyRegistryXml.Core.dll -->
