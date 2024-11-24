using System;

namespace PSProgress
{
    /// <summary>
    /// Contains progress information and indicates that progress should be displayed.
    /// </summary>
    public class SampledProgressInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SampledProgressInfo"/> class.
        /// </summary>
        /// <param name="itemIndex">The index of the item that was sampled.</param>
        /// <param name="remainingItemCount">The expected number of items that have yet to be processed.</param>
        /// <param name="percentComplete">A value between 0 and 1 that represents the completion progress.</param>
        /// <param name="estimatedTimeRemaining">A time span that represents the estimated time remaining until all items have been processed, if available.</param>
        public SampledProgressInfo(uint itemIndex, uint remainingItemCount, double percentComplete, TimeSpan? estimatedTimeRemaining)
        {
            ItemIndex = itemIndex;
            RemainingItemCount = remainingItemCount;
            PercentComplete = percentComplete;
            EstimatedTimeRemaining = estimatedTimeRemaining;
        }

        /// <summary>
        /// Gets the index of the item that was sampled.
        /// </summary>
        public uint ItemIndex { get; }

        /// <summary>
        /// Gets the expected number of items that have yet to be processed.
        /// </summary>
        public uint RemainingItemCount { get; }

        /// <summary>
        /// Gets a value between 0 and 1 that represents the completion progress.
        /// </summary>
        public double PercentComplete { get; }

        /// <summary>
        /// Gets a time span that represents the estimated time remaining until all items have been processed, if available.
        /// </summary>
        public TimeSpan? EstimatedTimeRemaining { get; }
    }
}
