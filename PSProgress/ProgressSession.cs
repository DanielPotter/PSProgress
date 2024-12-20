using System;
using System.Management.Automation;

namespace PSProgress
{
    /// <summary>
    /// A session for tracking progress.
    /// </summary>
    /// <param name="activity">The text that describes the activity whose progress is being reported.</param>
    /// <param name="activityId">The ID that distinguishes each progress bar from the others.</param>
    public class ProgressSession(string activity, int? activityId)
    {
        /// <summary>
        /// Gets the text that describes the activity whose progress is being reported.
        /// </summary>
        public string Activity { get; } = activity;

        /// <summary>
        /// Gets the ID that distinguishes each progress bar from the others.
        /// </summary>
        public int ActivityId { get; } = activityId ?? Math.Abs(activity.GetHashCode());

        /// <summary>
        /// Gets or sets the parent activity of the current activity.
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Gets the number of items that are expected to be processed.
        /// </summary>
        public uint ExpectedItemCount => this.Context.ExpectedItemCount;

        /// <summary>
        /// Gets or sets the script that creates the text that describes current state of the activity.
        /// </summary>
        public ScriptBlock? Status { get; set; }

        /// <summary>
        /// Gets or sets the script block that creates the text that describes the operation that's currently taking place.
        /// </summary>
        public ScriptBlock? CurrentOperation { get; set; }

        /// <summary>
        /// Gets or sets the object that tracks the progress for the session.
        /// </summary>
        public ProgressContext Context { get; set; } = new();

        /// <summary>
        /// Create a progress record for an item that will be processed.
        /// </summary>
        /// <param name="progressInfo">The progress information.</param>
        /// <param name="item">The item to be processed.</param>
        /// <returns>A new <see cref="ProgressRecord"/> instance that represents the <paramref name="progressInfo"/>.</returns>
        public ProgressRecord CreateProgressRecord(SampledProgressInfo progressInfo, object item)
        {
            string statusDescription;
            if (this.Status is null)
            {
                statusDescription = $"{progressInfo.ItemIndex} / {this.ExpectedItemCount} ({progressInfo.PercentComplete:P})";
            }
            else
            {
                statusDescription = this.Status.InvokeInline(item)?.ToString() ?? "Processing";
            }

            var progressRecord = new ProgressRecord(activityId: this.ActivityId, activity: this.Activity, statusDescription: statusDescription);

            if (this.ParentId.HasValue)
            {
                progressRecord.ParentActivityId = this.ParentId.Value;
            }

            if (this.CurrentOperation is not null)
            {
                string operationDescription = this.CurrentOperation.InvokeInline(item)?.ToString() ?? string.Empty;
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
