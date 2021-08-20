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
            private readonly GroupChoice groupChoice;
            private readonly Dictionary<AttributeGroup, IAttributeSequenceItemWidthComputer> groupWidthComputers;

            public WidthComputer(GroupChoice groupChoice, AttributeGroup[] groups)
            {
                this.groupChoice = groupChoice;
                groupWidthComputers = new(from attrGroup in groups
                                          select KeyValuePair.Create(attrGroup, attrGroup.CreateWidthComputer()));
            }

            public IEnumerable<NameLengthPair> TakeAndHandleAttributes(IEnumerable<NameLengthPair> attributes)
            {
                var attrNames = (from attr in attributes
                                 select attr.Name).ToList();
                // find the option that handles the most.
                var (bestGroup, bestNumHandled) = groupChoice.FindBestMatchingGroup(attrNames);
                if (bestNumHandled > 0)
                {
                    return groupWidthComputers[bestGroup].TakeAndHandleAttributes(attributes);
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
