using System;

namespace Manifesto
{
    /// <summary>
    /// Represents the validation results for a <seealso cref="ManifestEntry"/> instance.
    /// </summary>
    public sealed class ManifestValidatorResult
    {
        /// <summary>
        /// Gets the <see cref="ManifestEntry"/> that this instance represents the status of.
        /// </summary>
        public ManifestEntry ManifestEntry { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the value of the <see cref="Manifesto.ManifestEntry.Hash"/> property is valid.
        /// </summary>
        public Boolean IsValidHash { get; private set; }

        /// <summary>
        /// Gets the absolute path on disk where the file is located.  If the file does not exist, this value will be null.
        /// </summary>
        public String FileLocation { get; private set; }

        public ManifestValidatorResult(ManifestEntry manifestEntry, Boolean isValidHash, String fileLocation)
        {
            this.ManifestEntry = manifestEntry;
            this.IsValidHash = isValidHash;
            this.FileLocation = fileLocation;
        }
    }
}
