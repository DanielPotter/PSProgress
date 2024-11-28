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
        [ValidateNotNull]
        public ProgressSession? Session { get; set; }

        #endregion

        #region Processing Blocks

        /// <inheritdoc/>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (this.Session is null)
            {
                // This should never happen because PowerShell will validate that Session is not null.
                throw new PSInvalidOperationException($"Property {nameof(this.Session)} is null in {nameof(this.ProcessRecord)}");
            }

            this.Session.Complete(this.CommandRuntime);
        }

        #endregion
    }
}
