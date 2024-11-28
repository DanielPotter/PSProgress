using System;

namespace PSProgress
{
    /// <summary>
    /// A class that tracks uniform progress.
    /// </summary>
    public class ProgressContext
    {
        /// <summary>
        /// The default interval at which progress should be returned. The value is 0.5 seconds.
        /// </summary>
        public static TimeSpan DefaultRefreshInterval { get; } = TimeSpan.FromSeconds(0.5);

        /// <summary>
        /// The default length of time from the first sample that progress should be returned. The value is 1 second.
        /// </summary>
        public static TimeSpan DefaultDisplayThreshold { get; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// The default shortest length of time over which progress should be returned.
        /// </summary>
        public static TimeSpan DefaultMinimumTimeLeftToDisplay { get; } = TimeSpan.FromSeconds(2);

        private readonly ProgressSampleCollection progressSampleCollection = new();

        private DateTime? startTime;
        private DateTime? lastProgressDisplayTime;

        /// <summary>
        /// Gets the total number of samples that have been added.
        /// </summary>
        public uint ProcessedItemCount { get; private set; }

        /// <summary>
        /// Gets or sets the expected number of samples that will be added.
        /// </summary>
        public uint ExpectedItemCount { get; set; }

        /// <summary>
        /// Gets or sets the interval at which progress should be returned.
        /// </summary>
        public TimeSpan RefreshInterval { get; set; } = DefaultRefreshInterval;

        /// <summary>
        /// Gets or sets the length of time from the first sample that progress should be returned.
        /// </summary>
        public TimeSpan DisplayThreshold { get; set; } = DefaultDisplayThreshold;

        /// <summary>
        /// Gets or sets the shortest length of time over which progress should be returned. Set this to a longer time to avoid displaying progress moments from completion.
        /// </summary>
        public TimeSpan MinimumTimeLeftToDisplay { get; set; } = DefaultMinimumTimeLeftToDisplay;

        /// <summary>
        /// Gets or sets the date time provider to use. Set this property to allow for testing this class.
        /// </summary>
        public IDateTimeProvider TimeProvider { get; set; } = DateTimeProvider.Default;

        /// <summary>
        /// Samples the current time to determine whether progress should be displayed.
        /// </summary>
        /// <returns>An object containing progress information if progress should be displayed; otherwise, <see langword="null"/>.</returns>
        public SampledProgressInfo? AddSample()
        {
            DateTime now = this.TimeProvider.GetCurrentTime();
            if (this.ProcessedItemCount == 0)
            {
                this.startTime = now;
            }

            bool writeProgress = true;
            if (this.RefreshInterval.Ticks > 0 && this.lastProgressDisplayTime.HasValue && now - this.lastProgressDisplayTime < this.RefreshInterval)
            {
                writeProgress = false;
            }

            if (writeProgress && !this.lastProgressDisplayTime.HasValue && this.DisplayThreshold.Ticks > 0)
            {
                if (now - this.startTime < this.DisplayThreshold)
                {
                    writeProgress = false;
                }

                if (this.MinimumTimeLeftToDisplay.Ticks > 0)
                {
                    uint remainingItems = this.ExpectedItemCount - this.ProcessedItemCount;
                    if (this.progressSampleCollection.AverageInterval.HasValue && remainingItems > 0)
                    {
                        var timeRemaining = TimeSpan.FromTicks(this.progressSampleCollection.AverageInterval.Value.Ticks * remainingItems);
                        if (timeRemaining < this.MinimumTimeLeftToDisplay)
                        {
                            writeProgress = false;
                        }
                    }
                }
            }

            SampledProgressInfo? sampledProgressInfo = null;
            if (writeProgress)
            {
                this.progressSampleCollection.Add(new ProgressSample(this.ProcessedItemCount, now));
                this.lastProgressDisplayTime = now;

                uint remainingItems = this.ExpectedItemCount - this.ProcessedItemCount;
                double percentComplete = (double)this.ProcessedItemCount / this.ExpectedItemCount;
                TimeSpan? timeRemaining = null;
                if (this.progressSampleCollection.AverageInterval.HasValue && remainingItems > 0)
                {
                    timeRemaining = TimeSpan.FromTicks(this.progressSampleCollection.AverageInterval.Value.Ticks * remainingItems);
                }
                sampledProgressInfo = new SampledProgressInfo(
                    ItemIndex: this.ProcessedItemCount,
                    RemainingItemCount: remainingItems,
                    PercentComplete: percentComplete,
                    EstimatedTimeRemaining: timeRemaining);
            }

            this.ProcessedItemCount++;
            return sampledProgressInfo;
        }
    }
}
