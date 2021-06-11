// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;


namespace PrettyRegistryXml.GroupedAlignment
{
    public partial class GroupChoice
    {
        private class Aligner : IAttributeSequenceItemAligner
        {
            private readonly GroupChoice groupChoice;
            private readonly Dictionary<AttributeGroup, IAttributeSequenceItemAligner> groupAligners;

            public int FullWidth { get; private init; }

            public Aligner(GroupChoice groupChoice,
                           IEnumerable<KeyValuePair<AttributeGroup, IAttributeSequenceItemAligner>> groupsToAligners)
            {
                groupAligners = new(groupsToAligners);
                FullWidth = (from aligner in groupAligners.Values
                             select aligner.FullWidth).Max();
                this.groupChoice = groupChoice;
            }

            private bool TryGetAlignerForName(string name,
                                              [MaybeNullWhen(false)] out IAttributeSequenceItemAligner aligner)
            {
                if (groupChoice.nameToGroup.TryGetValue(name, out var attributeGroup))
                {
                    return groupAligners.TryGetValue(attributeGroup, out aligner);
                }
                aligner = null;
                return false;
            }

            public bool TakeAndHandleAttributes(IEnumerable<string> attributeNames,
                                                [MaybeNullWhen(false)] out IEnumerable<Core.AttributeAlignment> alignments,
                                                out IEnumerable<string> remainingNames)
            {
                var firstName = attributeNames.First();
                if (TryGetAlignerForName(firstName, out var aligner))
                {
                    bool gotAlignments = aligner.TakeAndHandleAttributes(attributeNames, out var choiceAlignments, out remainingNames);
                    Debug.Assert(gotAlignments && choiceAlignments != null);
                    if (aligner.FullWidth == FullWidth)
                    {
                        alignments = choiceAlignments;
                    }
                    else
                    {
                        // need to increase the last one
                        int additionalPaddingNeeded = FullWidth - aligner.FullWidth;
                        alignments = choiceAlignments.Append(Core.AttributeAlignment.MakePaddingOnly(additionalPaddingNeeded));
                    }
                    return true;
                }
                remainingNames = attributeNames;
                alignments = null;
                return false;
            }
        }
    }
}
