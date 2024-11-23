using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PSProgress
{
    [Cmdlet(VerbsCommunications.Write, "PipelineProgress",
        DefaultParameterSetName = "ManualCount")]
    public class WritePipelineProgressCmdletCommand : PSCmdlet
    {
        #region Fields

        private readonly List<DateTime> _initialTimestamps = new List<DateTime>();
        private TimeSpan _intervalSum = TimeSpan.Zero;

        private TimeSpan? _averageInterval = null;
        private int _index = 0;
        private DateTime _lastRefresh = DateTime.MinValue;
        private DateTime? _startTime = null;
        private bool _hasDisplayed = false;

        private readonly Queue<int> _refreshIndices = new Queue<int>();
        private readonly Queue<DateTime> _refreshTimestamps = new Queue<DateTime>();
        private int _indexDeltaSum = 0;
        private TimeSpan _indexIntervalSum = TimeSpan.Zero;

        private readonly List<object> _allItems = new List<object>();

        #endregion

        #region Parameters

        [Parameter(
            ValueFromPipeline = true)]
        public object[] InputObject { get; set; }

        [Parameter(
            Mandatory = true,
            Position = 0)]
        public string Activity { get; set; }

        [Parameter(
            ParameterSetName = "ManualCount",
            Mandatory = true)]
        public int Count { get; set; }

        [Parameter(
            ParameterSetName = "AutoCount",
            Mandatory = true)]
        public SwitchParameter AutoCount { get; set; }

        [Parameter()]
        public int Id { get; set; }

        [Parameter()]
        public int ParentId { get; set; }

        [Parameter()]
        public ScriptBlock Status { get; set; }

        [Parameter()]
        public ScriptBlock CurrentOperation { get; set; }

        [Parameter()]
        public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromSeconds(0.5);

        [Parameter()]
        public TimeSpan DisplayThreshold { get; set; } = TimeSpan.FromSeconds(1);

        [Parameter()]
        public TimeSpan MinimumTimeLeftToDisplay { get; set; } = TimeSpan.FromSeconds(2);

        #endregion

        #region Overrides

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (AutoCount)
            {
                _allItems.AddRange(InputObject);
                return;
            }

            ProcessItems(InputObject);
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();

            if (AutoCount)
            {
                Count = _allItems.Count;
                if (_allItems.Count == 0)
                {
                    return;
                }

                ProcessItems(_allItems);
            }

            WriteProgress(new ProgressRecord(activityId: Id, activity: Activity, statusDescription: "Complete")
            {
                RecordType = ProgressRecordType.Completed,
            });
        }

        #endregion

        private void ProcessItems(IEnumerable<object> inputItems)
        {
            foreach (var item in inputItems)
            {
                ProcessItem(item);

                WriteObject(item);
            }
        }

        private void ProcessItem(object item)
        {
            if (_index < 50)
            {
                _initialTimestamps.Add(DateTime.Now);

                if (_index > 0)
                {
                    _intervalSum += _initialTimestamps[_index] - _initialTimestamps[_index - 1];
                    _averageInterval = TimeSpan.FromMilliseconds(_intervalSum.TotalMilliseconds / (_initialTimestamps.Count - 1));
                }
                else
                {
                    _startTime = DateTime.Now;
                }
            }

            bool writeProgress = true;
            if (RefreshInterval.Ticks > 0 && DateTime.Now - _lastRefresh < RefreshInterval)
            {
                writeProgress = false;
            }

            if (writeProgress && !_hasDisplayed && DisplayThreshold.Ticks > 0)
            {
                if (DateTime.Now - _startTime < DisplayThreshold)
                {
                    writeProgress = false;
                }

                if (MinimumTimeLeftToDisplay.Ticks > 0)
                {
                    int remainingItems = Count - _index;
                    if (_averageInterval.HasValue && _averageInterval.Value.Ticks > 0 && remainingItems > 0)
                    {
                        TimeSpan timeRemaining = TimeSpan.FromTicks(_averageInterval.Value.Ticks * remainingItems);
                        if (timeRemaining < MinimumTimeLeftToDisplay)
                        {
                            writeProgress = false;
                        }
                    }
                }
            }

            if (writeProgress)
            {
                _hasDisplayed = true;
                _refreshIndices.Enqueue(_index);
                _refreshTimestamps.Enqueue(DateTime.Now);
                while (_refreshIndices.Count > 20)
                {
                    _indexDeltaSum -= _refreshIndices.ElementAt(1) - _refreshIndices.First();
                    _indexIntervalSum -= _refreshTimestamps.ElementAt(1) - _refreshTimestamps.First();
                    _refreshIndices.Dequeue();
                    _refreshTimestamps.Dequeue();
                }

                if (_refreshIndices.Count > 1)
                {
                    _indexDeltaSum += _refreshIndices.Last() - _refreshIndices.ElementAt(_refreshIndices.Count - 2);
                    _indexIntervalSum += _refreshTimestamps.Last() - _refreshTimestamps.ElementAt(_refreshTimestamps.Count - 2);
                }

                if (_index > 50)
                {
                    if (_refreshIndices.Count > 1)
                    {
                        int averageIndexDelta = _indexDeltaSum / (_refreshIndices.Count - 1);
                        TimeSpan averageIndexInterval = TimeSpan.FromMilliseconds(_indexIntervalSum.TotalMilliseconds / (_refreshTimestamps.Count - 1));
                        if (_refreshIndices.Count == 20)
                        {
                            _averageInterval = TimeSpan.FromMilliseconds(averageIndexInterval.TotalMilliseconds / averageIndexDelta);
                        }
                        else
                        {
                            TimeSpan averagedIndexIntervalSum = TimeSpan.FromMilliseconds(averageIndexInterval.TotalMilliseconds / averageIndexDelta * (_refreshTimestamps.Count - 1));
                            _averageInterval = TimeSpan.FromMilliseconds((_intervalSum + averagedIndexIntervalSum).TotalMilliseconds / (_initialTimestamps.Count - 1 + _refreshTimestamps.Count - 1));
                        }
                    }
                }

                _lastRefresh = DateTime.Now;

                string statusDescription;
                if (Status is null)
                {
                    statusDescription = $"{_index} / {Count} ({(double)_index / Count:P})";
                }
                else
                {
                    statusDescription = InvokeCommand.InvokeScript(
                        script: Status.ToString(),
                        useNewScope: false,
                        writeToPipeline: PipelineResultTypes.Output,
                        input: null,
                        args: item).FirstOrDefault()?.ToString() ?? "Processing";
                }

                var progressRecord = new ProgressRecord(activityId: Id, activity: Activity, statusDescription: statusDescription);

                if (MyInvocation.BoundParameters.ContainsKey(nameof(ParentId)))
                {
                    progressRecord.ParentActivityId = ParentId;
                }

                if (CurrentOperation != null)
                {
                    string operationDescription = InvokeCommand.InvokeScript(
                        script: Status.ToString(),
                        useNewScope: false,
                        writeToPipeline: PipelineResultTypes.Output,
                        input: null,
                        args: item).FirstOrDefault()?.ToString() ?? string.Empty;
                    progressRecord.CurrentOperation = operationDescription;
                }

                int remainingItems = Count - _index;
                int percentComplete = (int)((double)_index / Count * 100);
                if (_averageInterval.HasValue && remainingItems > 0)
                {
                    progressRecord.SecondsRemaining = (int)Math.Ceiling(_averageInterval.Value.TotalSeconds * remainingItems);
                }

                progressRecord.PercentComplete = percentComplete;

                WriteProgress(progressRecord);
            }

            _index++;
        }
    }
}
