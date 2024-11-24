using System;
using System.Management.Automation;

namespace PSProgress.Commands
{
    [Cmdlet(VerbsData.Update, "ProgressSession")]
    internal class UpdateProgressSessionCmdletCommand : PSCmdlet
    {
        #region Parameters

        [Parameter(
            ValueFromPipeline = true
        )]
        public object[] InputObject { get; set; }

        [Parameter(
            Mandatory = true
        )]
        public ProgressSession Session { get; set; }

        [Parameter()]
        public uint ExpectedCount { get; set; }

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

        [Parameter()]
        public SwitchParameter PassThru { get; set; }

        #endregion

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
