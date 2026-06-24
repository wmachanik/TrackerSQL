using System;
using System.Collections.Generic;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// Standard list-query result for repositories: data plus success/error for UI binding.
    /// </summary>
    public class RepositoryListResult<T>
    {
        public bool Success { get; private set; }

        public string ErrorMessage { get; private set; }

        public List<T> Items { get; private set; }

        private RepositoryListResult()
        {
            Items = new List<T>();
        }

        public static RepositoryListResult<T> Ok(List<T> items)
        {
            return new RepositoryListResult<T>
            {
                Success = true,
                Items = items ?? new List<T>()
            };
        }

        public static RepositoryListResult<T> Fail(string context, Exception ex, string logType = null)
        {
            string message = ex?.Message ?? "Unknown database error.";
            string logMessage = string.IsNullOrWhiteSpace(context)
                ? message
                : context + ": " + message;

            AppLogger.WriteLog(logType ?? SystemConstants.LogTypes.Database, logMessage);
            ApplicationErrorNotifier.Notify(message, context);

            return new RepositoryListResult<T>
            {
                Success = false,
                ErrorMessage = message,
                Items = new List<T>()
            };
        }
    }
}
