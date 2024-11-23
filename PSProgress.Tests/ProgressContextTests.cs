namespace PSProgress.Tests
{
    [TestClass]
    public class ProgressContextTests
    {
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
            var mockDateTimeProvider = new MockDateTimeProvider
            {
                CurrentTime = new DateTime(year: 2024, month: 1, day: 1),
            };

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
    }
}
