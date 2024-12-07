namespace PSProgress.Tests
{
    [TestClass]
    public class ProgressContextTests
    {
        public static readonly DateTime InitialTime = new(year: 2024, month: 1, day: 1);

        [TestMethod]
        public void AddSample_FirstSample_NoProgressInfo()
        {
            var progressContext = new ProgressContext();

            SampledProgressInfo? progressInfo = progressContext.AddSample();

            Assert.IsNull(progressInfo);
        }

        [TestMethod]
        public void AddSample_50of100Samples_Progress()
        {
            var mockDateTimeProvider = new MockDateTimeProvider(InitialTime);
            var progressContext = new ProgressContext
            {
                TimeProvider = mockDateTimeProvider,
                ExpectedItemCount = 100,
            };

            SampledProgressInfo? progressInfo = null;
            for (int index = 0; index < 51; index++)
            {
                progressInfo = progressContext.AddSample();
                mockDateTimeProvider.CurrentTime += TimeSpan.FromSeconds(1);
            }

            Assert.AreEqual(expected: 51, actual: (int)progressContext.ProcessedItemCount);

            Assert.IsNotNull(progressInfo);
            Assert.AreEqual(expected: 50, actual: (int)progressInfo.ItemIndex);
            Assert.AreEqual(expected: 50, actual: (int)progressInfo.RemainingItemCount);
            Assert.AreEqual(expected: 0.5, actual: progressInfo.PercentComplete);
            Assert.IsTrue(progressInfo.EstimatedTimeRemaining.HasValue);
            Assert.AreEqual(expected: 50, progressInfo.EstimatedTimeRemaining.Value.TotalSeconds);
        }

        [TestMethod]
        public void AddSample_CalledBetweenRefreshIntervals_ReturnsProgress()
        {
            var mockDateTimeProvider = new MockDateTimeProvider(InitialTime);
            var progressContext = new ProgressContext
            {
                TimeProvider = mockDateTimeProvider,
                DisplayThreshold = TimeSpan.Zero,
                RefreshInterval = TimeSpan.FromSeconds(2),
            };

            Assert.IsNotNull(progressContext.CheckTime(), "The first collected item should prompt for progress.");

            mockDateTimeProvider.CurrentTime += TimeSpan.FromSeconds(1);

            Assert.IsNotNull(progressContext.AddSample(), "The first sampled item should prompt for progress.");
        }

        [TestMethod]
        public void CheckTime_CalledTwiceWithNoDisplayThresholdOrRefreshInterval_ReturnsProgressInfo()
        {
            var mockDateTimeProvider = new MockDateTimeProvider(InitialTime);
            var progressContext = new ProgressContext
            {
                TimeProvider = mockDateTimeProvider,
                DisplayThreshold = TimeSpan.Zero,
                RefreshInterval = TimeSpan.Zero,
            };

            progressContext.CheckTime();

            var progressInfo = progressContext.CheckTime();

            Assert.IsNotNull(progressInfo);
        }

        [TestMethod]
        public void CheckTime_FirstCall_ReturnsNull()
        {
            var mockDateTimeProvider = new MockDateTimeProvider(InitialTime);
            var progressContext = new ProgressContext
            {
                TimeProvider = mockDateTimeProvider,
            };

            var progressInfo = progressContext.CheckTime();

            Assert.IsNull(progressInfo);
        }

        [TestMethod]
        public void CheckTime_CalledTwiceBeforeDisplayThreshold_ReturnsNull()
        {
            var mockDateTimeProvider = new MockDateTimeProvider(InitialTime);
            var progressContext = new ProgressContext
            {
                TimeProvider = mockDateTimeProvider,
                DisplayThreshold = TimeSpan.FromSeconds(1),
            };

            progressContext.CheckTime();
            mockDateTimeProvider.CurrentTime += TimeSpan.FromSeconds(0.1);

            var progressInfo = progressContext.CheckTime();

            Assert.IsNull(progressInfo);
        }
    }
}
