using System.Management.Automation;

namespace PSProgress.Commands
{
    [Cmdlet(VerbsLifecycle.Stop, "ProgressSession")]
    internal class StopProgressSessionCmdletCommand : PSCmdlet
    {
        #region Parameters

        [Parameter(
            Mandatory = true
        )]
        public ProgressSession Session { get; set; }

        #endregion

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var progressCompleteRecord = new ProgressRecord(activityId: Session.ActivityId, activity: Session.Activity, statusDescription: "Complete")
            {
                RecordType = ProgressRecordType.Completed,
            };

            WriteDebug(ProgressSession.GetDebugMessage(progressCompleteRecord));
            WriteProgress(progressCompleteRecord);
        }
    }
}
