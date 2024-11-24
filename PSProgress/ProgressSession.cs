using System;
using System.Management.Automation;

namespace PSProgress
{
    /// <summary>
    /// A session for tracking progress.
    /// </summary>
    public class ProgressSession
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressSession"/> class.
        /// </summary>
        /// <param name="activity">The text that describes the activity whose progress is being reported.</param>
        /// <param name="activityId">The ID that distinguishes each progress bar from the others.</param>
        public ProgressSession(string activity, int? activityId)
        {
            Activity = activity;
            ActivityId = activityId ?? Math.Abs(activity.GetHashCode());
        }

        /// <summary>
        /// Gets the text that describes the activity whose progress is being reported.
        /// </summary>
        public string Activity { get; }

        /// <summary>
        /// Gets the ID that distinguishes each progress bar from the others.
        /// </summary>
        public int ActivityId { get; }

        /// <summary>
        /// Gets or sets the parent activity of the current activity.
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Gets the number of items that are expected to be processed.
        /// </summary>
        public uint ExpectedItemCount => Context.ExpectedItemCount;

        /// <summary>
        /// Gets or sets the script that creates the text that describes current state of the activity.
        /// </summary>
        public ScriptBlock Status { get; set; }

        /// <summary>
        /// Gets or sets the script block that creates the text that describes the operation that's currently taking place.
        /// </summary>
        public ScriptBlock CurrentOperation { get; set; }

        /// <summary>
        /// Gets or sets the object that tracks the progress for the session.
        /// </summary>
        public ProgressContext Context { get; set; } = new ProgressContext();

        /// <summary>
        /// Createa a progress record for an item that will be processed.
        /// </summary>
        /// <param name="progressInfo">The progress information.</param>
        /// <param name="item">The item to be processed.</param>
        /// <returns></returns>
        public ProgressRecord CreateProgressRecord(SampledProgressInfo progressInfo, object item)
        {
            string statusDescription;
            if (Status is null)
            {
                statusDescription = $"{progressInfo.ItemIndex} / {ExpectedItemCount} ({progressInfo.PercentComplete:P})";
            }
            else
            {
                statusDescription = ScriptBlock.Create("$_ = $args[0]; " + Status.ToString()).InvokeReturnAsIs(item)?.ToString() ?? "Processing";
            }

            var progressRecord = new ProgressRecord(activityId: ActivityId, activity: Activity, statusDescription: statusDescription);

            if (ParentId.HasValue)
            {
                progressRecord.ParentActivityId = ParentId.Value;
            }

            if (CurrentOperation != null)
            {
                string operationDescription = ScriptBlock.Create("$_ = $args[0]; " + CurrentOperation.ToString()).InvokeReturnAsIs(item)?.ToString() ?? string.Empty;
                progressRecord.CurrentOperation = operationDescription;
            }

            if (progressInfo.EstimatedTimeRemaining.HasValue)
            {
                progressRecord.SecondsRemaining = (int)progressInfo.EstimatedTimeRemaining.Value.TotalSeconds;
            }

            progressRecord.PercentComplete = (int)(progressInfo.PercentComplete * 100);

            return progressRecord;
        }

        /// <summary>
        /// Gets a debug message to display for a progress record.
        /// </summary>
        /// <param name="progressRecord">The progress record.</param>
        /// <returns>A string representation of the progress record.</returns>
        public static string GetDebugMessage(ProgressRecord progressRecord)
        {
            if (progressRecord.RecordType == ProgressRecordType.Completed)
            {
                return $"Progress {progressRecord.ActivityId}, Activity=<{progressRecord.Activity}>, Completed";
            }

            return $"Progress {progressRecord.ActivityId}, Activity=<{progressRecord.Activity}>, Status=<{progressRecord.StatusDescription}>, Operation=<{progressRecord.CurrentOperation}>, PercentComplete=<{progressRecord.PercentComplete}>, SecondsRemaining=<{progressRecord.SecondsRemaining}>";
        }
    }
}
