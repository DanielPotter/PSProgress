using System;

namespace PSProgress
{
    /// <summary>
    /// A class that provides a method for getting the current time.
    /// </summary>
    public class DateTimeProvider : IDateTimeProvider
    {
        /// <summary>
        /// Gets or sets the default date time provider.
        /// </summary>
        /// <remarks>
        /// This property is writable to allow for testing progress commands from PowerShell. It is not recommended to modify this property outside of testing.
        /// </remarks>
        public static IDateTimeProvider Default { get; set; } = new DateTimeProvider();

        public DateTime GetCurrentTime()
        {
            return DateTime.Now;
        }
    }

    /// <summary>
    /// An interface that defines an object that can get the current time.
    /// </summary>
    public interface IDateTimeProvider
    {
        /// <summary>
        /// Gets the current time.
        /// </summary>
        /// <returns>The current time.</returns>
        DateTime GetCurrentTime();
    }
}
