using System;
using System.Management.Automation;

namespace PSProgress.Commands
{
    [Cmdlet(VerbsCommon.New, "ProgressSession")]
    internal class NewProgressSessionCmdletCommand : PSCmdlet
    {
        #region Parameters

        [Parameter(
            Mandatory = true,
            Position = 0
        )]
        public string Activity { get; set; }

        [Parameter(
            Mandatory = true
        )]
        public uint ExpectedCount { get; set; }

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
