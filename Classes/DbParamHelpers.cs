using System;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// Parameter helpers for SQL Server inserts/updates after Access migration.
    /// </summary>
    public static class DbParamHelpers
    {
        /// <summary>
        /// Optional FK columns: Access often stored 0 for "none". SQL Server FKs reject 0 — send NULL.
        /// </summary>
        public static object FkOrDbNull(int? value)
        {
            return value.HasValue && value.Value > 0 ? (object)value.Value : DBNull.Value;
        }

        /// <summary>
        /// Optional FK when the model uses non-nullable int (0 means none).
        /// </summary>
        public static object FkOrDbNull(int value)
        {
            return value > 0 ? (object)value : DBNull.Value;
        }
    }
}
