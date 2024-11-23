namespace PSProgress.Tests
{
    public class MockDateTimeProvider : IDateTimeProvider
    {
        public DateTime CurrentTime { get; set; }

        public DateTime GetCurrentTime()
        {
            return CurrentTime;
        }
    }
}
