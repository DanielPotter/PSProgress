using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSProgress.Commands
{
    [Cmdlet(VerbsCommunications.Write, "PipelineProgress")]
    public class WritePipelineProgressCmdletCommand : PSCmdlet
    {
        #region Fields

        private readonly ProgressContext _progressContext = new ProgressContext();

        private readonly List<object> _allItems = new List<object>();

        private bool _autoCountItems;

        #endregion

        #region Parameters

        [Parameter(
            ValueFromPipeline = true)]
        public object[] InputObject { get; set; }

        [Parameter(
            Mandatory = true,
            Position = 0)]
        public string Activity { get; set; }

        [Parameter()]
        public int ExpectedCount { get; set; }

        [Parameter()]
        public int Id { get; set; }

        [Parameter()]
        public int ParentId { get; set; }

        [Parameter()]
        public ScriptBlock Status { get; set; }

        [Parameter()]
        public ScriptBlock CurrentOperation { get; set; }

        [Parameter()]
        public TimeSpan RefreshInterval { get; set; } = ProgressContext.DefaultRefreshInterval;

        [Parameter()]
        public TimeSpan DisplayThreshold { get; set; } = ProgressContext.DefaultDisplayThreshold;

        [Parameter()]
        public TimeSpan MinimumTimeLeftToDisplay { get; set; } = ProgressContext.DefaultMinimumTimeLeftToDisplay;

        #endregion

        #region Overrides

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            if (MyInvocation.BoundParameters.ContainsKey(nameof(RefreshInterval)))
            {
                _progressContext.RefreshInterval = RefreshInterval;
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(DisplayThreshold)))
            {
                _progressContext.DisplayThreshold = DisplayThreshold;
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(MinimumTimeLeftToDisplay)))
            {
                _progressContext.MinimumTimeLeftToDisplay = MinimumTimeLeftToDisplay;
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(ExpectedCount)))
            {
                _progressContext.ExpectedItemCount = (uint)ExpectedCount;
            }
            else
            {
                _autoCountItems = true;
            }
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (_autoCountItems)
            {
                _allItems.AddRange(InputObject);
                return;
            }

            foreach (var item in InputObject)
            {
                HandleProgressSample(item, _progressContext.AddSample());
                WriteObject(item);
            }
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();

            if (_autoCountItems)
            {
                _progressContext.ExpectedItemCount = (uint)_allItems.Count;
                if (_allItems.Count == 0)
                {
                    return;
                }

                foreach (var item in _allItems)
                {
                    HandleProgressSample(item, _progressContext.AddSample());
                    WriteObject(item);
                }
            }

            WriteProgressInternal(new ProgressRecord(activityId: Id, activity: Activity, statusDescription: "Complete")
            {
                RecordType = ProgressRecordType.Completed,
            });
        }

        #endregion

        private void HandleProgressSample(object item, SampledProgressInfo progressInfo)
        {
            if (progressInfo is null)
            {
                return;
            }

            string statusDescription;
            if (Status is null)
            {
                statusDescription = $"{progressInfo.ItemIndex} / {ExpectedCount} ({progressInfo.PercentComplete:P})";
            }
            else
            {
                statusDescription = ScriptBlock.Create("$_ = $args[0]; " + Status.ToString()).InvokeReturnAsIs(item)?.ToString() ?? "Processing";
            }

            var progressRecord = new ProgressRecord(activityId: Id, activity: Activity, statusDescription: statusDescription);

            if (MyInvocation.BoundParameters.ContainsKey(nameof(ParentId)))
            {
                progressRecord.ParentActivityId = ParentId;
            }

            if (CurrentOperation != null)
            {
                string operationDescription = ScriptBlock.Create("$_ = $args[0]; " + CurrentOperation.ToString()).InvokeReturnAsIs(item)?.ToString() ?? string.Empty;
                progressRecord.CurrentOperation = operationDescription;
            }

            if (progressInfo.EstimatedTimeRemaining.HasValue)
            {
                progressRecord.SecondsRemaining = (int)progressInfo.EstimatedTimeRemaining.Value.TotalSeconds;
            }

            progressRecord.PercentComplete = (int)(progressInfo.PercentComplete * 100);

            WriteProgressInternal(progressRecord);
        }

        private void WriteProgressInternal(ProgressRecord progressRecord)
        {
            if (progressRecord.RecordType == ProgressRecordType.Completed)
            {
                WriteDebug($"Progress {progressRecord.ActivityId}, Activity=<{progressRecord.Activity}>, Completed");
            }
            else
            {
                WriteDebug($"Progress {progressRecord.ActivityId}, Activity=<{progressRecord.Activity}>, Status=<{progressRecord.StatusDescription}>, Operation=<{progressRecord.CurrentOperation}>, PercentComplete=<{progressRecord.PercentComplete}>, SecondsRemaining=<{progressRecord.SecondsRemaining}>");
            }

            WriteProgress(progressRecord);
        }
    }
}
