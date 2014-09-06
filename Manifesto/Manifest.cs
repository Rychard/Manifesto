using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Manifesto
{
    /// <summary>
    /// Represents a collection of files.
    /// </summary>
    public class Manifest
    {
        /// <summary>
        /// Gets or sets the identifier of the instance.
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// Gets or sets a description of this instance.
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// Gets or sets the hash algorithm that was used to generate the hashes for the files in the instance.
        /// </summary>
        public String HashAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets the <seealso cref="DateTime" /> in UTC, that the instance was created or modified.
        /// </summary>
        public DateTime TimestampUtc { get; set; }

        /// <summary>
        /// Gets or sets the contents of the instance.
        /// </summary>
        [XmlArray]
        [XmlArrayItem(ElementName="Entry", Type = typeof(ManifestEntry))] 
        public List<ManifestEntry> Contents { get; set; }
    }
}
