using System;

namespace PSProgress
{
    /// <summary>
    /// Contains progress information and indicates that progress should be displayed.
    /// </summary>
    /// <param name="ItemIndex"> Gets the index of the item that was sampled. </param>
    /// <param name="RemainingItemCount"> Gets the expected number of items that have yet to be processed. </param>
    /// <param name="PercentComplete"> Gets a value between 0 and 1 that represents the completion progress. </param>
    /// <param name="EstimatedTimeRemaining"> Gets a time span that represents the estimated time remaining until all items have been processed, if available. </param>
    public record SampledProgressInfo(
        uint ItemIndex,
        uint RemainingItemCount,
        double PercentComplete,
        TimeSpan? EstimatedTimeRemaining
    );
}
