using System;

namespace Manifesto
{
    /// <summary>
    /// Represents the current status of a manifest validation process.
    /// </summary>
    public sealed class ManifestValidatorProgressChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the count of items that have been processed.
        /// </summary>
        public int Processed { get; private set; }

        /// <summary>
        /// Gets the count of items that have not been processed.
        /// </summary>
        public int Remaining { get; private set; }

        /// <summary>
        /// Gets the count of items that have passed validation.
        /// </summary>
        public int Passed { get; private set; }

        /// <summary>
        /// Gets the count of items that have failed validation.
        /// </summary>
        public int Failed { get; private set; }

        /// <summary>
        /// Gets the progress of the validation process as a value between 0 and 1.
        /// </summary>
        public double PercentageComplete { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManifestValidatorProgressChangedEventArgs"/> class.
        /// </summary>
        /// <param name="processed">The number of items that have been processed.</param>
        /// <param name="remaining">The number of items remaining to be processed.</param>
        /// <param name="passed">The number of items that have passed validation.</param>
        /// <param name="failed">The number of items that have failed validation.</param>
        public ManifestValidatorProgressChangedEventArgs(int processed, int remaining, int passed, int failed)
        {
            this.Processed = processed;
            this.Remaining = remaining;
            this.Passed = passed;
            this.Failed = failed;

            var total = processed + remaining;
            var percentageComplete = (processed / (double)total);
            this.PercentageComplete = percentageComplete;
        }
    }
}
