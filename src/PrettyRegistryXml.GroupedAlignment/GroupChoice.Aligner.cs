// Copyright 2021-2025 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static PrettyRegistryXml.Core.AttributeAlignmentExtensions;


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
        private sealed class Aligner : IAttributeSequenceItemAligner
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

            public bool TakeAndHandleAttributes(IEnumerable<string> attributeNames,
                                                [MaybeNullWhen(false)] out IEnumerable<Core.AttributeAlignment> alignments,
                                                out IEnumerable<string> remainingNames)
            {
                // find the option that handles the most.
                var (bestGroup, bestNumHandled) = groupChoice.FindBestMatchingGroup(attributeNames);
                if (bestNumHandled > 0)
                {
                    var bestAligner = groupAligners[bestGroup];
                    if (bestAligner.TakeAndHandleAttributes(attributeNames,
                                                            out var ourAlignments,
                                                            out remainingNames))
                    {
                        if (bestAligner.FullWidth < FullWidth)
                        {
                            alignments = ourAlignments.Append(
                                Core.AttributeAlignment.MakePaddingOnly(FullWidth - bestAligner.FullWidth));
                        }
                        else
                        {
                            alignments = ourAlignments;
                        }
                        return true;
                    }
                }
                remainingNames = attributeNames;
                alignments = null;
                return false;
            }
        }
    }
}
