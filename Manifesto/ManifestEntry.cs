using System;
using System.Xml.Serialization;

namespace Manifesto
{
    /// <summary>
    /// Represents a single file entry in the contents of a <seealso cref="Manifest"/>.
    /// </summary>
    public class ManifestEntry
    {
        /// <summary>
        /// Gets or sets the hash of the file.
        /// </summary>
        [XmlAttribute("Hash")]
        public String Hash { get; set; }

        /// <summary>
        /// Gets or sets the size (in bytes) of the file.
        /// </summary>
        [XmlAttribute("Size")]
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the relative location of the file.
        /// </summary>
        [XmlAttribute("File")]
        public String File { get; set; }

        /// <summary>
        /// Gets or sets a remote location where the file can be obtained.
        /// </summary>
        [XmlAttribute("Remote")]
        public String Remote { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManifestEntry"/> class.
        /// </summary>
        public ManifestEntry()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManifestEntry"/> class.
        /// </summary>
        /// <param name="hash">The file's computed hash.</param>
        /// <param name="size">The size of the file, in bytes.</param>
        /// <param name="file">The location of the file on the local machine.  This value is usually a relative path, although an absolute path can be used as well.</param>
        /// <param name="remote">The remote location where the file can be obtained.  This value is usually an HTTP URL.</param>
        public ManifestEntry(String hash, long size, String file, String remote)
        {
            // If the values are empty strings, set them to null.
            // Null values aren't included in the serialized output.
            if (String.IsNullOrWhiteSpace(hash)) { hash = null; }
            if (String.IsNullOrWhiteSpace(file)) { file = null; }
            if (String.IsNullOrWhiteSpace(remote)) { remote = null; }
            this.Hash = hash;
            this.Size = size;
            this.File = file;
            this.Remote = remote;
        }
    }
}
