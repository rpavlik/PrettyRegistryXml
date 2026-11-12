// Copyright 2021-2024 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using PrettyRegistryXml.Core;
using PrettyRegistryXml.GroupedAlignment;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;


namespace PrettyRegistryXml.OpenXR
{
    /// <summary>
    /// OpenXR-specific policy for formatting XML.
    /// </summary>
    public class XmlFormatter : XmlFormatterBase
    {
        /// <inheritdoc />
        public override int IndentLevelWidth { get => 4; }

        /// <inheritdoc />
        public override string IndentChars { get => "    "; }

        /// <summary>
        /// Whether we should wrap the attributes of extension tags, a runtime preference set by the command line.
        /// </summary>
        private bool WrapExtensions { get; set; }

        /// <summary>
        /// Whether we should trim the values of attributes, a runtime preference set by the command line, on by default.
        /// </summary>
        private bool TrimAttributes { get; set; }

        /// <summary>
        /// Whether we should normalize spaces in the values of attributes, a runtime preference set by the command line, on by default.
        /// </summary>
        private bool NormalizeAttributeSpaces { get; set; }

        /// <summary>
        /// Whether we should sort the return values, a runtime preference set by the command line, on by default.
        /// </summary>
        private bool SortReturnVals { get; set; }

        /// <summary>
        /// Whether to artificially de-indent extensions by one level - legacy behavior, on by default.
        /// </summary>
        private bool DeindentExtensions { get; set; }

        /// <summary>
        /// Whether to align the names of struct and union members in one column, a runtime preference set by the command line, on by default.
        /// </summary>
        private bool AlignMemberNames { get; set; }

        private readonly ReturnCodeSorter CodeSorter = new ReturnCodeSorter();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Formatting options, typically from command line.</param>
        public XmlFormatter(Options options)
        {
            WrapExtensions = options.WrapExtensions;
            TrimAttributes = options.TrimAttributes;
            NormalizeAttributeSpaces = options.NormalizeAttributeSpaces;
            SortReturnVals = options.SortCodes;
            DeindentExtensions = options.DeindentExtensions;
            AlignMemberNames = options.AlignMemberNames;
        }

        /// <summary>
        /// Checks an element category attribute to tell if it's a known "not single line" element.
        /// See also <see cref="ChildrenShouldBeSingleLine(XElement)"/> where this is used.
        /// </summary>
        /// <param name="element">An element</param>
        /// <returns>true if the element has a "category" attribute and the value is known to be one we don't want to single-line.</returns>
        private static bool IsCategoryNonSingleLine(XElement element)
        {
            var category = element.Attribute("category");
            return category != null
                   && (category.Value == "define" || category.Value == "struct");
        }

        /// <summary>
        /// A set of element names who always act as single-line containers.
        /// See also <see cref="ChildrenShouldBeSingleLine(XElement)"/> where this is used.
        /// </summary>
        private static readonly HashSet<string> singleLineContainers = new HashSet<string>() { "member", "param", "proto" };

        /// <summary>
        /// Determine whether an element and its children should all be on a single line.
        /// </summary>
        /// <param name="e">An element</param>
        /// <returns>true if the element and its children should all be on a single line.</returns>
        private static bool ChildrenShouldBeSingleLine(XElement e)
        {
            // some containers should always have their children on a single line.
            if (singleLineContainers.Contains(e.Name.LocalName))
            {
                return true;
            }

            // some elements should only have their children on a single line when they're a specific usage:
            // lots of elts named "type" but they aren't all the same.
            return e.Name.LocalName == "type"
                   && e.Parent != null
                   && e.Parent.Name.LocalName == "types"
                   && !IsCategoryNonSingleLine(e);
        }

        /// <summary>
        /// Checks an element category attribute to tell if it's a bitmask.
        /// </summary>
        /// <param name="element">An element</param>
        /// <returns>true if the element defines a bitmask</returns>
        private static bool IsBitmask(XElement element)
        {
            var attr = element.Attribute("category");
            return element.Name == "type"
                   && attr != null
                   && attr.Value == "bitmask";
        }

        private static string? GetPreviousElementName(XText xt)
        {
            var elements = xt.ElementsBeforeSelf();
            if (!elements.Any()) { return null; }
            var sib = elements.Last();
            return sib?.Name.LocalName;
        }

        private static string? GetNextElementName(XText xt)
        {
            var elements = xt.ElementsAfterSelf();
            if (!elements.Any()) { return null; }
            var sib = elements.First();
            return sib?.Name.LocalName;
        }

        /// <inheritdoc/>
        protected override TextWhitespace.Behavior ComputeWhitespaceBehavior(XText t)
        {
            if (t.Parent == null) { return TextWhitespace.Behavior.Preserve; }
            var parentName = t.Parent.Name;
            if (parentName == "name")
            {
                // inside leaf tag
                return TextWhitespace.Behavior.Trim;
            }
            if (parentName == "type" && !t.Parent.HasAttributes)
            {
                // leaf tag for type too.
                return TextWhitespace.Behavior.Trim;
            }


            var previousName = GetPreviousElementName(t);
            var nextName = GetNextElementName(t);
            if (parentName == "proto" || parentName == "param")
            {
                // we are in a command
                if (previousName == null && nextName == "type")
                {
                    // this is before the return type or param type in a function.
                    // Nothing or "const "
                    return TextWhitespace.Behavior.EmptyOrSingleTrailingSpace;
                }
                if (previousName != null && nextName == null)
                {
                    // this is at the very end, tbh shouldn't have anything here.
                    return TextWhitespace.Behavior.Trim;
                }
                if (previousName == "type" && nextName == "name")
                {
                    // between type and name
                    return TextWhitespace.Behavior.Collapse;
                }
            }

            return TextWhitespace.Behavior.Preserve;
        }

        /// <summary>
        /// Checks whether an element is a struct or union type definition.
        /// </summary>
        /// <param name="element">An element</param>
        /// <returns>true if the element is a &lt;type&gt; with category "struct" or "union"</returns>
        private static bool IsStructOrUnion(XElement element)
        {
            var category = element.Attribute("category");
            return element.Name == "type"
                   && category != null
                   && (category.Value == "struct" || category.Value == "union");
        }

        /// <summary>
        /// Escape text the way <see cref="XmlWriter"/> does, so that widths match the output.
        /// </summary>
        /// <param name="value">Text or an attribute value</param>
        /// <param name="isAttribute">Whether the value is an attribute value, which also escapes quotes</param>
        /// <returns>The escaped value</returns>
        private static string EscapeXml(string value, bool isAttribute)
        {
            var escaped = value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
            return isAttribute ? escaped.Replace("\"", "&quot;") : escaped;
        }

        /// <summary>
        /// The width of a member as written up to its &lt;name&gt;.
        /// </summary>
        /// <param name="member">A &lt;member&gt; element</param>
        /// <param name="name">Its &lt;name&gt; child</param>
        /// <param name="includeTrailingWhitespace">Whether to count the whitespace just before &lt;name&gt;</param>
        /// <returns>The number of characters before &lt;name&gt;, or before the whitespace that precedes it</returns>
        private int MemberPrefixWidth(XElement member, XElement name, bool includeTrailingWhitespace = false)
        {
            var prefix = new System.Text.StringBuilder("<");
            prefix.Append(member.Name.LocalName);
            foreach (var attr in member.Attributes())
            {
                prefix.Append(' ').Append(attr.Name.LocalName).Append("=\"").Append(EscapeXml(FormatAttributeValue(attr.Value), true)).Append('"');
            }
            prefix.Append('>');
            foreach (var node in name.NodesBeforeSelf())
            {
                if (node is XText text)
                {
                    // Count text as it will be written: WriteNode applies the whitespace behavior later.
                    // The whitespace just before <name> is the padding being computed, so it stays as is.
                    var value = node == name.PreviousNode
                        ? text.Value
                        : TextWhitespace.ApplyBehaviorNullable(ComputeWhitespaceBehavior(text), text.Value);
                    prefix.Append(EscapeXml(value, false));
                }
                else
                {
                    prefix.Append(node.ToString(SaveOptions.DisableFormatting));
                }
            }
            var written = prefix.ToString();
            return includeTrailingWhitespace ? written.Length : written.TrimEnd().Length;
        }

        /// <summary>
        /// Pad the whitespace before each member's &lt;name&gt; so that all names start in one column,
        /// one space past the longest member. Members already aligned in one column at least that wide
        /// are left alone. The member with a "values" attribute (the <code>XrStructureType</code> member)
        /// keeps its spacing, as in the existing registry.
        /// </summary>
        /// <param name="element">A struct or union &lt;type&gt; element (child of &lt;types&gt;)</param>
        private void AlignStructMemberNames(XElement element)
        {
            var members = (from member in element.Elements("member")
                           where member.Attribute("values") == null // exclude the type member
                           let name = member.Element("name")
                           where name != null
                           select (member, name)).ToList();
            if (members.Count < 2)
            {
                return;
            }
            // we need this per-member width later
            var widths = members.Select(m => MemberPrefixWidth(m.member, m.name)).ToList();

            // where we will align to
            var column = widths.Max() + 1;

            // where members are currently aligned to (including whitespace)
            var existingColumns = members.Select(m => MemberPrefixWidth(m.member, m.name, true)).Distinct().ToList();

            if (existingColumns.Count == 1 && existingColumns[0] >= column)
            {
                // Everything is already aligned to a larger column than we require, leave it alone
                return;
            }

            // Adjust each struct member's alignment
            for (int i = 0; i < members.Count; ++i)
            {
                var padding = FormatterUtilities.MakeSpaces(column - widths[i]);
                if (members[i].name.PreviousNode is XText text)
                {
                    text.Value = text.Value.TrimEnd() + padding;
                }
                else
                {
                    members[i].name.AddBeforeSelf(new XText(padding));
                }
            }
        }

        private static bool IsEnum(XElement element) => element.Name == "enum";

        /// <inheritdoc />
        public override int ComputeLevelAdjust(XNode node)
        {
            if (!DeindentExtensions)
            {
                return 0;
            }
            var extensionsInAncestors = (from el in node.Ancestors()
                                         where el.Name == "extensions"
                                         select el).Any();
            return extensionsInAncestors ? -1 : 0;
        }

        /// <summary>
        /// Our normal alignment, which just adds 2 extra spaces to value attribute widths.
        /// </summary>
        private readonly IAlignmentFinder simpleAlignmentWithExtraValueWidth =
            new SimpleAlignment(new Dictionary<string, int>{
                {"value", 2}
            });

        /// <summary>
        /// Our alignment for interaction profile related things, which adds 2 extra spaces to user_path subpath attribute widths,
        /// and aligns user path tag path attributes with component tag user_path attributes.
        /// </summary>
        private readonly IAlignmentFinder interactionProfileComponentAlignment
            = new GroupedAttributeAlignment(
                // First line up the paths.
                new GroupChoice(
                    // attributes from component tags, 2 extra spaces added
                    new AttributeGroup(2, "user_path", "subpath"),
                    // attribute from user_path tag, 2 extra spaces added
                    new AttributeGroup(2, "path"))
            // Everything else can be aligned normally
            );

        /// <summary>
        /// Our slightly sophisticated way of grouping attributes for alignment in extensions.
        /// </summary>
        private readonly IAlignmentFinder extensionEnumAlignment
            = new GroupedAttributeAlignment(new GroupChoice(new AttributeGroup("value"),
                                                            new AttributeGroup("offset", "dir", "extends"),
                                                            new AttributeGroup("bitpos", "extends")));

        /// <summary>
        /// This is the recursive part that contains most of the "policy"
        /// </summary>
        /// <param name="writer">Your writer</param>
        /// <param name="element">The element to write</param>
        protected override void WriteElement(XmlWriter writer, XElement element)
        {
            // Setup work: Sort return values if desired.
            if (element.Name == "command" && SortReturnVals)
            {
                var success = element.Attribute("successcodes");
                if (success != null)
                {
                    success.Value = CodeSorter.SortReturnCodeString(success.Value);
                }

                var error = element.Attribute("errorcodes");
                if (error != null)
                {
                    error.Value = CodeSorter.SortReturnCodeString(error.Value);
                }
            }

            if (AlignMemberNames && IsStructOrUnion(element))
            {
                AlignStructMemberNames(element);
            }

            // Now, only one of these paths should execute.
            if (ChildrenShouldBeSingleLine(element))
            {
                WriteSingleLineElement(writer, element);
            }
            else if (element.Name == "require" && element.Parent != null && element.Parent.Name == "feature")
            {
                // Simple alignment in feature requirements (core)
                WriteElementWithAlignedChildElts(writer, element);
            }
            else if (element.Name == "require" && element.Parent != null && element.Parent.Name == "extension")
            {
                // Beautiful alignment of the enums in extensions.
                WriteElementWithAlignedChildAttrsInGroups(writer,
                                                          element,
                                                          extensionEnumAlignment,
                                                          (XElement element) => element.Name == "enum");
            }
            else if (element.Name == "tags" && element.HasElements)
            {
                WriteElementWithAlignedChildElts(writer, element);
            }
            else if (element.Name == "enums" && element.HasElements)
            {
                // Give some extra width to the value field
                // and don't let comments break up our alignment groups.
                WriteElementWithAlignedChildAttrsInGroups(writer,
                                                          element,
                                                          simpleAlignmentWithExtraValueWidth,
                                                          IsEnum,
                                                          n => XmlUtilities.IsWhitespaceOrCommentBetweenSelectedElements(n, IsEnum));
            }
            else if (element.Name == "types")
            {
                WriteElementWithAlignedChildAttrsInGroups(writer, element, IsBitmask);
            }
            else if (element.Name == "interaction_profile" && element.HasElements)
            {
                // These are interaction profile declarations - grouping because there are two child tag names
                WriteElementWithAlignedChildAttrsInGroups(writer,
                                                          element,
                                                          interactionProfileComponentAlignment,
                                                          (XElement element) => element.Name == "component");
            }
            else if (element.Name == "extend" && element.Attribute("interaction_profile_path") != null)
            {
                // These are extensions to an interaction profile.
                WriteElementWithAlignedChildElts(writer, element, interactionProfileComponentAlignment);
            }
            else if (WrapExtensions && element.Name == "extension")
            {
                // This will change the format! (for the better, probably, though)
                // Also, missing indent at level "extensions" so we adjust by -1
                WriteElementWithAttrNewlines(writer, element, -1);
            }
            else
            {
                base.WriteElement(writer, element);
            }

        }

        /// <inheritdoc />
        protected override void WriteAttributes(XmlWriter writer, XElement e)
        {
            foreach (var attr in e.Attributes())
            {
                writer.WriteAttributeString(attr.Name.LocalName, attr.Name.NamespaceName, FormatAttributeValue(attr.Value));
            }
        }

        /// <summary>
        /// An attribute value as <see cref="WriteAttributes(XmlWriter, XElement)"/> writes it.
        /// </summary>
        /// <param name="value">The attribute value</param>
        /// <returns>The value, trimmed and with spaces normalized if those options are set</returns>
        private string FormatAttributeValue(string value)
        {
            if (TrimAttributes)
            {
                value = value.Trim();
            }
            if (NormalizeAttributeSpaces)
            {
                value = Regex.Replace(value, "\\s+", " ");
            }
            return value;
        }

        /// <inheritdoc />
        protected override XText CleanWhitespaceNode(XText whitespaceText)
            => FormatterUtilities.RegenerateIndentation(this, whitespaceText);
    }
}

