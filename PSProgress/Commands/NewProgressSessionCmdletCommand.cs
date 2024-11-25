using System;
using System.Management.Automation;

namespace PSProgress.Commands
{
    /// <summary>
    /// Creates an updatable progress session.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "ProgressSession")]
    [OutputType(typeof(ProgressSession))]
    public class NewProgressSessionCmdletCommand : PSCmdlet
    {
        #region Parameters

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
        [Parameter(
            Mandatory = true
        )]
        public uint ExpectedCount { get; set; }

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
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var session = new ProgressSession(Activity, MyInvocation.BoundParameters.ContainsKey(nameof(Id)) ? (int?)Id : null)
            {
                ParentId = MyInvocation.BoundParameters.ContainsKey(nameof(ParentId)) ? (int?)ParentId : null,
                Status = Status,
                CurrentOperation = CurrentOperation,
                Context = new ProgressContext
                {
                    DisplayThreshold = DisplayThreshold,
                    ExpectedItemCount = ExpectedCount,
                    MinimumTimeLeftToDisplay = MinimumTimeLeftToDisplay,
                    RefreshInterval = RefreshInterval,
                },
            };

            WriteObject(session);
        }

        #endregion
    }
}
