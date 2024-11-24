using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSProgress.Commands
{
    /// <summary>
    /// Displays a progress bar that updates as the items in the pipeline are processed.
    /// </summary>
    /// <para>
    /// The <c>Write-PipelineProgress</c> cmdlet displays a progress bar that depicts the status of processing a pipeline of objects.
    /// </para>
    /// <example>
    ///   <summary>Simple Progress Bar</summary>
    ///   <code>
    ///   1..10 | Write-PipelineProgress -Activity "Activity" | ForEach-Object {
    ///       # Simulate a task.
    ///       Start-Sleep -Seconds 1
    ///   }
    ///   </code>
    /// </example>
    /// <example>
    ///   <summary>Progress Bar with Custom Status</summary>
    ///   <code>
    ///   Get-ChildItem *.json -Recurse | Write-PipelineProgress -Activity "Read JSON Files" -Status { $_.Name } | ForEach-Object {
    ///       # Simulate parsing the file.
    ///       Start-Sleep -Seconds 1
    ///   }
    ///   </code>
    /// </example>
    /// <example>
    ///   <summary>Progress Bar with Expected Number of Items</summary>
    ///   <code>
    ///   $taskList = 1..1000000
    ///   $taskList | Write-PipelineProgress -Activity "Process Many Items" -ExpectedCount $taskList.Count | ForEach-Object {
    ///       # Simulate a task.
    ///       Start-Sleep -Milliseconds 10
    ///   }
    ///   </code>
    /// </example>
    [Cmdlet(VerbsCommunications.Write, "PipelineProgress")]
    [OutputType(typeof(object))]
    public class WritePipelineProgressCmdletCommand : PSCmdlet
    {
        #region Fields

        private ProgressSession _progressSession;

        private readonly List<object> _allItems = new List<object>();

        private bool _autoCountItems;

        #endregion

        #region Parameters

        /// <summary>
        /// Specifies the input objects. Progress will be written for each of these objects.
        /// </summary>
        [Parameter(
            ValueFromPipeline = true
        )]
        public object[] InputObject { get; set; }

        /// <summary>
        /// Specifies the first line of text in the heading above the status bar. This text describes the activity whose progress is being reported.
        /// </summary>
        [Parameter(
            Mandatory = true,
            Position = 0
        )]
        public string Activity { get; set; }

        /// <summary>
        /// Specifies the number of items that are expected to be processed. Using this parameter will improve the speed and reduce the overhead of this command.
        /// </summary>
        [Parameter()]
        public int ExpectedCount { get; set; }

        /// <summary>
        /// Specifies an ID that distinguishes each progress bar from the others. Use this parameter when you are creating more than one progress bar in a single command. If the progress bars don't have different IDs, they're superimposed instead of being displayed in a series. Negative values aren't allowed.
        /// </summary>
        [Parameter()]
        public int Id { get; set; }

        /// <summary>
        /// Specifies the parent activity of the current activity. Use the value <c>-1</c> if the current activity has no parent activity.
        /// </summary>
        [Parameter()]
        public int ParentId { get; set; }

        /// <summary>
        /// Specifies a script block expression that gets text that describes the current state of the activity, given the object being processed.
        /// </summary>
        [Parameter()]
        public ScriptBlock Status { get; set; }

        /// <summary>
        /// Specifies a script block expression that gets text that describes the operation that's currently taking place. This parameter has no effect when the progress view is set to <c>Minimal</c>.
        /// </summary>
        [Parameter()]
        public ScriptBlock CurrentOperation { get; set; }

        /// <summary>
        /// Specifies the interval at which progress should be returned.
        /// </summary>
        [Parameter()]
        public TimeSpan RefreshInterval { get; set; } = ProgressContext.DefaultRefreshInterval;

        /// <summary>
        /// Specifies the length of time from the first sample that progress should be returned.
        /// </summary>
        [Parameter()]
        public TimeSpan DisplayThreshold { get; set; } = ProgressContext.DefaultDisplayThreshold;

        /// <summary>
        /// Specifies the shortest length of time over which progress should be returned. Set this to a longer time to avoid displaying progress moments from completion.
        /// </summary>
        [Parameter()]
        public TimeSpan MinimumTimeLeftToDisplay { get; set; } = ProgressContext.DefaultMinimumTimeLeftToDisplay;

        #endregion

        #region Overrides

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (_autoCountItems)
            {
                if (InputObject.Length > 0)
                {
                    WriteProgress(new ProgressRecord(
                        activityId: _progressSession.ActivityId,
                        activity: _progressSession.Activity,
                        statusDescription: _progressSession.Status?.InvokeInline(InputObject[0])?.ToString() ?? "Collecting")
                    {
                        CurrentOperation = "Collecting",
                    });
                }

                _allItems.AddRange(InputObject);
                return;
            }

            foreach (var item in InputObject)
            {
                HandleItem(item);
            }
        }

        /// <inheritdoc/>
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

            WriteProgressInternal(new ProgressRecord(
                activityId: _progressSession.ActivityId,
                activity: _progressSession.Activity,
                statusDescription: "Complete")
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
