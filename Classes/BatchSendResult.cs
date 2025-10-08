namespace TrackerDotNet.Classes
{
    /// <summary>
    /// Represents the result of a batch email sending operation
    /// </summary>
    public class BatchSendResult
    {
        public BatchSendResult()
        {
            TotalSent = 0;
            TotalFailed = 0;
            IsSuccess = false;
            ErrorMessage = string.Empty;
        }

        /// <summary>
        /// Number of emails successfully sent
        /// </summary>
        public int TotalSent { get; set; }

        /// <summary>
        /// Number of emails that failed to send
        /// </summary>
        public int TotalFailed { get; set; }

        /// <summary>
        /// Overall success status of the batch operation
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Error message if the operation failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Combines this result with another batch result
        /// </summary>
        /// <param name="other">The other batch result to combine</param>
        public void Combine(BatchSendResult other)
        {
            if (other == null) return;

            TotalSent += other.TotalSent;
            TotalFailed += other.TotalFailed;
            
            // Overall success is true only if both are successful
            IsSuccess = IsSuccess && other.IsSuccess;
            
            // Combine error messages
            if (!string.IsNullOrEmpty(other.ErrorMessage))
            {
                if (string.IsNullOrEmpty(ErrorMessage))
                    ErrorMessage = other.ErrorMessage;
                else
                    ErrorMessage += "; " + other.ErrorMessage;
            }
        }

        /// <summary>
        /// Gets a human-readable status message
        /// </summary>
        /// <returns>Status message string</returns>
        public string GetStatusMessage()
        {
            if (IsSuccess)
            {
                return $"Successfully sent {TotalSent} emails";
            }
            else if (TotalSent > 0)
            {
                return $"Partially successful: {TotalSent} sent, {TotalFailed} failed. Errors: {ErrorMessage}";
            }
            else
            {
                return $"Failed to send {TotalFailed} emails. Error: {ErrorMessage}";
            }
        }

        /// <summary>
        /// Gets the total number of emails processed
        /// </summary>
        public int TotalProcessed => TotalSent + TotalFailed;
    }
}