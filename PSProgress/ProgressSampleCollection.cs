using System;
using System.Collections.Generic;
using System.Linq;

namespace PSProgress
{
    internal class ProgressSampleCollection
    {
        private readonly Queue<ProgressSample> sampleQueue = new Queue<ProgressSample>();

        private uint indexDeltaSum;
        private TimeSpan indexIntervalSum;

        public int Capacity { get; } = 20;

        public int Count => this.sampleQueue.Count;

        public TimeSpan? AverageInterval { get; private set; }

        public void Add(ProgressSample sample)
        {
            this.sampleQueue.Enqueue(sample);

            while (this.sampleQueue.Count >= this.Capacity)
            {
                ProgressSample firstSample = this.sampleQueue.First();
                ProgressSample secondSample = this.sampleQueue.ElementAt(1);
                this.indexDeltaSum -= secondSample.Index - firstSample.Index;
                this.indexIntervalSum -= secondSample.Timestamp - firstSample.Timestamp;
                this.sampleQueue.Dequeue();
            }

            if (this.sampleQueue.Count > 1)
            {
                ProgressSample lastSample = this.sampleQueue.Last();
                ProgressSample secondToLastSample = this.sampleQueue.ElementAt(this.sampleQueue.Count - 2);
                this.indexDeltaSum += lastSample.Index - secondToLastSample.Index;
                this.indexIntervalSum += lastSample.Timestamp - secondToLastSample.Timestamp;

                long averageIndexDelta = this.indexDeltaSum / (this.Count - 1);
                var averageIndexInterval = TimeSpan.FromMilliseconds(this.indexIntervalSum.TotalMilliseconds / (this.Count - 1));
                this.AverageInterval = TimeSpan.FromMilliseconds(averageIndexInterval.TotalMilliseconds / averageIndexDelta);
            }
        }
    }
}
