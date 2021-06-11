// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using static PrettyRegistryXml.Core.AttributeAlignmentExtensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;


namespace PrettyRegistryXml.GroupedAlignment
{
    internal static class Extensions
    {
        public static bool HandleAttributesWithGlobalWidth(this IAttributeSequenceItemAligner aligner,
                                                           int fullWidth,
                                                           IEnumerable<string> attributeNames,
                                                           [MaybeNullWhen(false)] out IEnumerable<Core.AttributeAlignment> alignments,
                                                           out IEnumerable<string> remainingNames)
        {
            if (aligner.TakeAndHandleAttributes(attributeNames, out var choiceAlignments, out remainingNames))
            {
                var alignerFullWidth = choiceAlignments.ComputeFullWidth();

                if (alignerFullWidth == fullWidth)
                {
                    alignments = choiceAlignments;
                }
                else
                {
                    // need to add extra padding
                    int additionalPaddingNeeded = fullWidth - alignerFullWidth;
                    alignments = choiceAlignments.Append(Core.AttributeAlignment.MakePaddingOnly(additionalPaddingNeeded));
                }
                return true;
            }
            alignments = null;
            remainingNames = attributeNames;
            return false;
        }
    }
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
                    return aligner.HandleAttributesWithGlobalWidth(FullWidth,
                                                                   attributeNames,
                                                                   out alignments,
                                                                   out remainingNames);
                }
                remainingNames = attributeNames;
                alignments = null;
                return false;
            }
        }
    }
}
