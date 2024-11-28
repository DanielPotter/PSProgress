using System;
using System.Management.Automation;

namespace PSProgress.Commands
{
    /// <summary>
    /// Updates a progress session.
    /// </summary>
    [Cmdlet(VerbsData.Update, "ProgressSession")]
    [OutputType(typeof(object))]
    public class UpdateProgressSessionCmdletCommand : PSCmdlet
    {
        #region Parameters

        /// <summary>
        /// Specifies the input objects. Progress will be written for each of these objects.
        /// </summary>
        [Parameter(
            ValueFromPipeline = true
        )]
        public object[]? InputObject { get; set; }

        /// <summary>
        /// Specifies the progress session to update.
        /// </summary>
        [Parameter(
            Mandatory = true
        )]
        [ValidateNotNull]
        public ProgressSession? Session { get; set; }

        /// <summary>
        /// Specifies the number of items that are expected to be processed. Using this parameter will improve the speed and reduce the overhead of this command.
        /// </summary>
        [Parameter()]
        public uint ExpectedCount { get; set; }

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

        /// <summary>
        /// Specifies that each input object should be written back to the pipeline.
        /// </summary>
        [Parameter()]
        public SwitchParameter PassThru { get; set; }

        #endregion

        /// <inheritdoc/>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            if (this.Session is null)
            {
                // This should never happen because PowerShell will validate that Session is not null.
                throw new PSInvalidOperationException($"Property {nameof(this.Session)} is null in {nameof(this.BeginProcessing)}");
            }

            if (this.MyInvocation.BoundParameters.ContainsKey(nameof(this.Status)))
            {
                this.Session.Status = this.Status;
            }

            if (this.MyInvocation.BoundParameters.ContainsKey(nameof(this.CurrentOperation)))
            {
                this.Session.CurrentOperation = this.CurrentOperation;
            }

            if (this.MyInvocation.BoundParameters.ContainsKey(nameof(this.ExpectedCount)))
            {
                this.Session.Context.ExpectedItemCount = this.ExpectedCount;
            }

            if (this.MyInvocation.BoundParameters.ContainsKey(nameof(this.RefreshInterval)))
            {
                this.Session.Context.RefreshInterval = this.RefreshInterval;
            }

            if (this.MyInvocation.BoundParameters.ContainsKey(nameof(this.DisplayThreshold)))
            {
                this.Session.Context.DisplayThreshold = this.DisplayThreshold;
            }

            if (this.MyInvocation.BoundParameters.ContainsKey(nameof(this.MinimumTimeLeftToDisplay)))
            {
                this.Session.Context.MinimumTimeLeftToDisplay = this.MinimumTimeLeftToDisplay;
            }
        }

        /// <inheritdoc/>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (this.Session is null)
            {
                // This should never happen because PowerShell will validate that Session is not null.
                throw new PSInvalidOperationException($"Property {nameof(this.Session)} is null in {nameof(this.ProcessRecord)}");
            }

            if (this.InputObject is null)
            {
                return;
            }

            foreach (var item in this.InputObject)
            {
                if (this.PassThru)
                {
                    this.WriteObject(item);
                }

                var progressInfo = this.Session.Context.AddSample();
                if (progressInfo is not null)
                {
                    var progressRecord = this.Session.CreateProgressRecord(progressInfo, item);
                    this.WriteDebug(ProgressSession.GetDebugMessage(progressRecord));
                    this.WriteProgress(progressRecord);
                }
            }
        }
    }
}
