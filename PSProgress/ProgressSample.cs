using System;

namespace PSProgress
{
    internal readonly struct ProgressSample(uint index, DateTime timestamp)
    {
        public uint Index { get; } = index;

        public DateTime Timestamp { get; } = timestamp;
    }
}
