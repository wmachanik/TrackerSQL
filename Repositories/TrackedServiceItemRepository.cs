using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes.Poco;

namespace TrackerSQL.Classes.Sql
{
    /// <summary>
    /// Repository for TrackedServiceItem entity
    /// Auto-generated: 2026-06-02 13:35:41
    /// </summary>
    public class TrackedServiceItemRepository : RepositoryBase<TrackedServiceItem>
    {
        protected override string TableName => "TrackedServiceItemTbl";
        protected override string KeyColumn => "TrackerServiceItemID";

        /// <summary>
        /// Maps a DataReader row to a TrackedServiceItem object
        /// </summary>
        protected override TrackedServiceItem Map(IDataReader reader)
        {
            return DbMapper.Map<TrackedServiceItem>(reader);
        }

        /// <summary>
        /// Gets all TrackedServiceItem records
        /// </summary>
        /// <param name="sortColumn">Column to sort by (optional)</param>
        public List<TrackedServiceItem> GetAll(string sortColumn = null)
        {
            string sql = `$"SELECT * FROM {TableName}"`;
            if (!string.IsNullOrEmpty(sortColumn))
            {
                sql += `$" ORDER BY {sortColumn}"`;
            }

            return ExecuteQuery(sql);
        }

        /// <summary>
        /// Gets a single TrackedServiceItem by ID
        /// </summary>
        public TrackedServiceItem GetById(int id)
        {
            string sql = `$"SELECT * FROM {TableName} WHERE {KeyColumn} = @Id"`;
            var parameters = new List<DBParameter>
            {
                new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@Id" }
            };

            return ExecuteQuerySingle(sql, parameters);
        }

        /// <summary>
        /// Inserts a new TrackedServiceItem record
        /// </summary>
        public int Insert(TrackedServiceItem entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            // TODO: Customize INSERT statement based on POCO properties
            // This is a template - adjust column names as needed
            string sql = @"
                INSERT INTO TrackedServiceItemTbl (
                    -- Add column names here
                )
                VALUES (
                    -- Add parameter names here (@Param1, @Param2, etc.)
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new List<DBParameter>
            {
                // TODO: Add parameters based on POCO properties
                // Example:
                // new DBParameter { DataValue = entity.PropertyName, DataDbType = DbType.String, ParamName = "@PropertyName" }
            };

            return ExecuteScalar<int>(sql, parameters);
        }

        /// <summary>
        /// Updates an existing TrackedServiceItem record
        /// </summary>
        public bool Update(TrackedServiceItem entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            // TODO: Customize UPDATE statement based on POCO properties
            string sql = @"
                UPDATE TrackedServiceItemTbl
                SET 
                    -- Add column = @Param pairs here
                WHERE TrackerServiceItemID = @Id";

            var parameters = new List<DBParameter>
            {
                // TODO: Add parameters based on POCO properties
                // Don't forget to add the ID parameter
            };

            int result = ExecuteNonQuery(sql, parameters);
            return result > 0;
        }

        /// <summary>
        /// Deletes a TrackedServiceItem record by ID
        /// </summary>
        public bool Delete(int id)
        {
            string sql = `$"DELETE FROM {TableName} WHERE {KeyColumn} = @Id"`;
            var parameters = new List<DBParameter>
            {
                new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@Id" }
            };

            int result = ExecuteNonQuery(sql, parameters);
            return result > 0;
        }

        // TODO: Add custom methods from legacy TrackedServiceItemTbl.cs here
        // Example:
        // public int GetCustomCount()
        // {
        //     string sql = "SELECT COUNT(*) FROM TrackedServiceItemTbl WHERE CustomCondition";
        //     return ExecuteScalar<int>(sql);
        // }
    }

        /// <summary>
        /// Gets all TrackedServiceItem records
        /// </summary>
        /// <param name="sortColumn">Column to sort by (optional)</param>
        public List<TrackedServiceItem> GetAll(string sortColumn = null)
        {
            string sql = `$"SELECT * FROM {TableName}"`;
            if (!string.IsNullOrEmpty(sortColumn))
            {
                sql += `$" ORDER BY {sortColumn}"`;
            }

            return ExecuteQuery(sql);
        }}

