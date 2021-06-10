// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;


namespace PrettyRegistryXml.GroupedAlignment
{
    public partial class GroupChoice
    {
        private class WidthComputer : IAttributeSequenceItemWidthComputer
        {
            private GroupChoice groupChoice;
            private Dictionary<AttributeGroup, IAttributeSequenceItemWidthComputer> groupWidthComputers;

            private bool TryGetWidthComputerForName(string attributeName,
                                                    [MaybeNullWhen(false)] out IAttributeSequenceItemWidthComputer widthComputer)
            {
                if (groupChoice.nameToGroup.TryGetValue(attributeName, out var attributeGroup))
                {
                    return groupWidthComputers.TryGetValue(attributeGroup, out widthComputer);
                }
                widthComputer = null;
                return false;
            }

            public WidthComputer(GroupChoice groupChoice, AttributeGroup[] groups)
            {
                this.groupChoice = groupChoice;
                groupWidthComputers = new(from attrGroup in groups
                                          select KeyValuePair.Create(attrGroup, attrGroup.CreateWidthComputer()));
            }

            public IEnumerable<NameLengthPair> TakeAndHandleAttributes(IEnumerable<NameLengthPair> attributes)
            {
                var firstName = attributes.First().Name;
                if (TryGetWidthComputerForName(firstName, out var widthComputer))
                {
                    return widthComputer.TakeAndHandleAttributes(attributes);
                }
                // nothing for us here.
                return attributes;
            }

            public IAttributeSequenceItemAligner Finish() => new Aligner(groupChoice, from groupAndState in groupWidthComputers
                                                                                      let aligner = groupAndState.Value.Finish()
                                                                                      select KeyValuePair.Create(groupAndState.Key, aligner));
        }
    }
}
