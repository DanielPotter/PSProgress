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
        public object[] InputObject { get; set; }

        /// <summary>
        /// Specifies the progress session to update.
        /// </summary>
        [Parameter(
            Mandatory = true
        )]
        public ProgressSession Session { get; set; }

        /// <summary>
        /// Specifies the number of items that are expected to be processed. Using this parameter will improve the speed and reduce the overhead of this command.
        /// </summary>
        [Parameter()]
        public uint ExpectedCount { get; set; }

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

            if (MyInvocation.BoundParameters.ContainsKey(nameof(Status)))
            {
                Session.Status = Status;
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(CurrentOperation)))
            {
                Session.CurrentOperation = CurrentOperation;
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(ExpectedCount)))
            {
                Session.Context.ExpectedItemCount = ExpectedCount;
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(RefreshInterval)))
            {
                Session.Context.RefreshInterval = RefreshInterval;
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(DisplayThreshold)))
            {
                Session.Context.DisplayThreshold = DisplayThreshold;
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(MinimumTimeLeftToDisplay)))
            {
                Session.Context.MinimumTimeLeftToDisplay = MinimumTimeLeftToDisplay;
            }
        }

        /// <inheritdoc/>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (InputObject is null)
            {
                return;
            }

            foreach (var item in InputObject)
            {
                if (PassThru)
                {
                    WriteObject(item);
                }

                var progressInfo = Session.Context.AddSample();
                if (progressInfo != null)
                {
                    var progressRecord = Session.CreateProgressRecord(progressInfo, item);
                    WriteDebug(ProgressSession.GetDebugMessage(progressRecord));
                    WriteProgress(progressRecord);
                }
            }
        }
    }
}
