namespace PSProgress.Tests
{
    public class MockDateTimeProvider : IDateTimeProvider
    {
        public MockDateTimeProvider()
        {
        }

        public MockDateTimeProvider(DateTime startTime)
        {
            this.CurrentTime = startTime;
        }

        public DateTime CurrentTime { get; set; }

        public DateTime GetCurrentTime()
        {
            return this.CurrentTime;
        }
    }
}
