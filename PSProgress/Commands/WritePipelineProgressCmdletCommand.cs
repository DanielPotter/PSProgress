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

        private ProgressSession? progressSession;

        private readonly List<object> allItems = [];

        private bool autoCountItems;

        #endregion

        #region Parameters

        /// <summary>
        /// Specifies the input objects. Progress will be written for each of these objects.
        /// </summary>
        [Parameter(
            ValueFromPipeline = true
        )]
        public object[] InputObject { get; set; } = [];

        /// <summary>
        /// Specifies the first line of text in the heading above the status bar. This text describes the activity whose progress is being reported.
        /// </summary>
        [Parameter(
            Mandatory = true,
            Position = 0
        )]
        public string Activity { get; set; } = string.Empty;

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
        public ScriptBlock? Status { get; set; }

        /// <summary>
        /// Specifies a script block expression that gets text that describes the operation that's currently taking place. This parameter has no effect when the progress view is set to <c>Minimal</c>.
        /// </summary>
        [Parameter()]
        public ScriptBlock? CurrentOperation { get; set; }

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

        #region Processing Blocks

        /// <inheritdoc/>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            this.progressSession = new ProgressSession(this.Activity, this.MyInvocation.BoundParameters.ContainsKey(nameof(this.Id)) ? this.Id : null);

            if (this.MyInvocation.BoundParameters.ContainsKey(nameof(this.Status)))
            {
                this.progressSession.Status = this.Status;
            }

            if (this.MyInvocation.BoundParameters.ContainsKey(nameof(this.CurrentOperation)))
            {
                this.progressSession.CurrentOperation = this.CurrentOperation;
            }

            if (this.MyInvocation.BoundParameters.ContainsKey(nameof(this.RefreshInterval)))
            {
                this.progressSession.Context.RefreshInterval = this.RefreshInterval;
            }

            if (this.MyInvocation.BoundParameters.ContainsKey(nameof(this.DisplayThreshold)))
            {
                this.progressSession.Context.DisplayThreshold = this.DisplayThreshold;
            }

            if (this.MyInvocation.BoundParameters.ContainsKey(nameof(this.MinimumTimeLeftToDisplay)))
            {
                this.progressSession.Context.MinimumTimeLeftToDisplay = this.MinimumTimeLeftToDisplay;
            }

            if (this.MyInvocation.BoundParameters.ContainsKey(nameof(this.ExpectedCount)))
            {
                this.progressSession.Context.ExpectedItemCount = (uint)this.ExpectedCount;
            }
            else
            {
                this.autoCountItems = true;
            }
        }

        /// <inheritdoc/>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (this.progressSession is null)
            {
                // This should never happen because BeginProcessing is always called before ProcessRecord.
                throw new InvalidOperationException($"Field {nameof(this.progressSession)} is null in {nameof(this.ProcessRecord)}");
            }

            if (this.autoCountItems)
            {
                if (this.InputObject.Length > 0)
                {
                    this.WriteProgress(new ProgressRecord(
                        activityId: this.progressSession.ActivityId,
                        activity: this.progressSession.Activity,
                        statusDescription: this.progressSession.Status?.InvokeInline(this.InputObject[0])?.ToString() ?? "Collecting")
                    {
                        CurrentOperation = "Collecting",
                    });
                }

                this.allItems.AddRange(this.InputObject);
                return;
            }

            foreach (var item in this.InputObject)
            {
                this.progressSession.WriteProgressForItem(item, this.CommandRuntime);
                this.WriteObject(item);
            }
        }

        /// <inheritdoc/>
        protected override void EndProcessing()
        {
            base.EndProcessing();

            if (this.progressSession is null)
            {
                // This should never happen because BeginProcessing is always called before EndProcessing.
                throw new InvalidOperationException($"Field {nameof(this.progressSession)} is null in {nameof(this.EndProcessing)}");
            }

            if (this.autoCountItems)
            {
                this.progressSession.Context.ExpectedItemCount = (uint)this.allItems.Count;
                if (this.allItems.Count == 0)
                {
                    return;
                }

                foreach (var item in this.allItems)
                {
                    this.progressSession.WriteProgressForItem(item, this.CommandRuntime);
                    this.WriteObject(item);
                }
            }

            this.progressSession.Complete(this.CommandRuntime);
        }

        #endregion
    }
}
