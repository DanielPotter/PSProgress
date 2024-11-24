using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSProgress.Commands
{
    [Cmdlet(VerbsCommunications.Write, "PipelineProgress")]
    public class WritePipelineProgressCmdletCommand : PSCmdlet
    {
        #region Fields

        private ProgressSession _progressSession;

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

            _progressSession = new ProgressSession(Activity, MyInvocation.BoundParameters.ContainsKey(nameof(Id)) ? (int?)Id : null);

            if (MyInvocation.BoundParameters.ContainsKey(nameof(Status)))
            {
                _progressSession.Status = Status;
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(CurrentOperation)))
            {
                _progressSession.CurrentOperation = CurrentOperation;
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(RefreshInterval)))
            {
                _progressSession.Context.RefreshInterval = RefreshInterval;
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(DisplayThreshold)))
            {
                _progressSession.Context.DisplayThreshold = DisplayThreshold;
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(MinimumTimeLeftToDisplay)))
            {
                _progressSession.Context.MinimumTimeLeftToDisplay = MinimumTimeLeftToDisplay;
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(ExpectedCount)))
            {
                _progressSession.Context.ExpectedItemCount = (uint)ExpectedCount;
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
                HandleItem(item);
            }
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();

            if (_autoCountItems)
            {
                _progressSession.Context.ExpectedItemCount = (uint)_allItems.Count;
                if (_allItems.Count == 0)
                {
                    return;
                }

                foreach (var item in _allItems)
                {
                    HandleItem(item);
                }
            }

            WriteProgressInternal(new ProgressRecord(activityId: Id, activity: Activity, statusDescription: "Complete")
            {
                RecordType = ProgressRecordType.Completed,
            });
        }

        #endregion

        private void HandleItem(object item)
        {
            var progressInfo = _progressSession.Context.AddSample();
            if (progressInfo != null)
            {
                var progressRecord = _progressSession.CreateProgressRecord(progressInfo, item);
                WriteProgressInternal(progressRecord);
            }

            WriteObject(item);
        }

        private void WriteProgressInternal(ProgressRecord progressRecord)
        {
            WriteDebug(ProgressSession.GetDebugMessage(progressRecord));
            WriteProgress(progressRecord);
        }
    }
}
