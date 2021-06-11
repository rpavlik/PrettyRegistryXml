// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Xml;

namespace PrettyRegistryXml.Core.Extensions
{
    /// <summary>
    /// Extensions for <see cref="XmlWriter" />
    /// </summary>
    public static class XmlWriterExtensions
    {

        /// <summary>
        /// Extension method to clone <see cref="XmlWriter.Settings"/> or create a new <see cref="XmlWriterSettings"/> if it is null.
        /// </summary>
        /// <returns>A new <see cref="XmlWriterSettings"/> object</returns>
        public static XmlWriterSettings CloneOrCreateSettings(this XmlWriter writer)
        {
            if (writer.Settings != null)
            {
                return writer.Settings.Clone();
            }
            return new XmlWriterSettings();
        }
    }
}
