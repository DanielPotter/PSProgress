using System;

namespace PSProgress
{
    /// <summary>
    /// Contains progress information and indicates that progress should be displayed.
    /// </summary>
    public class SampledProgressInfo
    {
        public SampledProgressInfo(uint itemIndex, uint remainingItemCount, double percentComplete, TimeSpan? estimatedTimeRemaining)
        {
            ItemIndex = itemIndex;
            RemainingItemCount = remainingItemCount;
            PercentComplete = percentComplete;
            EstimatedTimeRemaining = estimatedTimeRemaining;
        }

        public uint ItemIndex { get; }

        public uint RemainingItemCount { get; }

        public double PercentComplete { get; }

        public TimeSpan? EstimatedTimeRemaining { get; }
    }
}
