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
        public static TimeSpan DefaultRefreshInterval = TimeSpan.FromSeconds(0.5);

        /// <summary>
        /// The default length of time from the first sample that progress should be returned. The value is 1 second.
        /// </summary>
        public static TimeSpan DefaultDisplayThreshold = TimeSpan.FromSeconds(1);

        /// <summary>
        /// The default shortest length of time over which progress should be returned.
        /// </summary>
        public static TimeSpan DefaultMinimumTimeLeftToDisplay = TimeSpan.FromSeconds(2);

        private readonly ProgressSampleCollection _progressSampleCollection = new ProgressSampleCollection();

        private DateTime? _startTime;
        private DateTime? _lastProgressDisplayTime;

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
        public SampledProgressInfo AddSample()
        {
            var now = TimeProvider.GetCurrentTime();
            if (ProcessedItemCount == 0)
            {
                _startTime = now;
            }

            bool writeProgress = true;
            if (RefreshInterval.Ticks > 0 && _lastProgressDisplayTime.HasValue && now - _lastProgressDisplayTime < RefreshInterval)
            {
                writeProgress = false;
            }

            if (writeProgress && !_lastProgressDisplayTime.HasValue && DisplayThreshold.Ticks > 0)
            {
                if (now - _startTime < DisplayThreshold)
                {
                    writeProgress = false;
                }

                if (MinimumTimeLeftToDisplay.Ticks > 0)
                {
                    uint remainingItems = ExpectedItemCount - ProcessedItemCount;
                    if (_progressSampleCollection.AverageInterval.HasValue && remainingItems > 0)
                    {
                        TimeSpan timeRemaining = TimeSpan.FromTicks(_progressSampleCollection.AverageInterval.Value.Ticks * remainingItems);
                        if (timeRemaining < MinimumTimeLeftToDisplay)
                        {
                            writeProgress = false;
                        }
                    }
                }
            }

            SampledProgressInfo sampledProgressInfo = null;
            if (writeProgress)
            {
                _progressSampleCollection.Add(new ProgressSample(ProcessedItemCount, now));
                _lastProgressDisplayTime = now;

                uint remainingItems = ExpectedItemCount - ProcessedItemCount;
                double percentComplete = (double)ProcessedItemCount / ExpectedItemCount;
                TimeSpan? timeRemaining = null;
                if (_progressSampleCollection.AverageInterval.HasValue && remainingItems > 0)
                {
                    timeRemaining = TimeSpan.FromTicks(_progressSampleCollection.AverageInterval.Value.Ticks * remainingItems);
                }
                sampledProgressInfo = new SampledProgressInfo(
                    itemIndex: ProcessedItemCount,
                    remainingItemCount: remainingItems,
                    percentComplete: percentComplete,
                    estimatedTimeRemaining: timeRemaining);
            }

            ProcessedItemCount++;
            return sampledProgressInfo;
        }
    }
}
