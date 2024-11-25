using System.Management.Automation;

namespace PSProgress.Commands
{
    /// <summary>
    /// Stops an active progress session and removes its progress bar.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Stop, "ProgressSession")]
    public class StopProgressSessionCmdletCommand : PSCmdlet
    {
        #region Parameters

        /// <summary>
        /// Specifies the progress session to stop.
        /// </summary>
        [Parameter(
            Mandatory = true
        )]
        public ProgressSession Session { get; set; }

        #endregion

        /// <inheritdoc/>
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
