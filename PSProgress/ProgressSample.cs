using System;

namespace PSProgress
{
    internal readonly struct ProgressSample
    {
        public ProgressSample(uint index, DateTime timestamp)
        {
            Index = index;
            Timestamp = timestamp;
        }

        public uint Index { get; }

        public DateTime Timestamp { get; }
    }
}
