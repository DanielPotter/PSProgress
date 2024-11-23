using System;
using System.Collections.Generic;
using System.Linq;

namespace PSProgress
{
    internal class ProgressSampleCollection
    {
        private readonly Queue<ProgressSample> _sampleQueue = new Queue<ProgressSample>();

        private uint _indexDeltaSum;
        private TimeSpan _indexIntervalSum;

        public int Capacity { get; } = 20;

        public int Count => _sampleQueue.Count;

        public TimeSpan? AverageInterval { get; private set; }

        public void Add(ProgressSample sample)
        {
            _sampleQueue.Enqueue(sample);

            while (_sampleQueue.Count >= Capacity)
            {
                ProgressSample firstSample = _sampleQueue.First();
                ProgressSample secondSample = _sampleQueue.ElementAt(1);
                _indexDeltaSum -= secondSample.Index - firstSample.Index;
                _indexIntervalSum -= secondSample.Timestamp - firstSample.Timestamp;
                _sampleQueue.Dequeue();
            }

            if (_sampleQueue.Count > 1)
            {
                ProgressSample lastSample = _sampleQueue.Last();
                ProgressSample secondToLastSample = _sampleQueue.ElementAt(_sampleQueue.Count - 2);
                _indexDeltaSum += lastSample.Index - secondToLastSample.Index;
                _indexIntervalSum += lastSample.Timestamp - secondToLastSample.Timestamp;

                long averageIndexDelta = _indexDeltaSum / (Count - 1);
                TimeSpan averageIndexInterval = TimeSpan.FromMilliseconds(_indexIntervalSum.TotalMilliseconds / (Count - 1));
                AverageInterval = TimeSpan.FromMilliseconds(averageIndexInterval.TotalMilliseconds / averageIndexDelta);
            }
        }
    }
}
