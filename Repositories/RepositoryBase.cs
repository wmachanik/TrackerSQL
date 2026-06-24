using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public abstract class RepositoryBase<T>
        where T : new()
    {
        protected abstract string TableName { get; }
        protected abstract string KeyColumn { get; }

        // Optional specialized projections (derived repos can override). If null/empty -> "*"
        protected virtual string CoreColumns => null;          // minimal/core columns for key lookups
        protected virtual string LookupColumns => null;        // id + display text (aliased to match POCO)

        protected TrackerSQLDb CreateDb() => new TrackerSQLDb();

        private static string SelectListOrAll(string projection)
            => string.IsNullOrWhiteSpace(projection) ? "*" : projection;

        protected int ExecNonQuery(string sql, List<DBParameter> parameters = null)
        {
            using (var db = CreateDb())
            {
                return db.ExecuteNonQuery(sql, parameters);
            }
        }
        protected TResult ExecuteQuerySingle<TResult>(string sql, List<DBParameter> parameters = null) where TResult : new()
        {
            using (var db = CreateDb())
            {
                return db.ExecuteQuerySingle<TResult>(sql, parameters);
            }
        }

        protected TResult ExecuteScalar<TResult>(string sql, List<DBParameter> parameters = null)
        {
            using (var db = CreateDb())
            {
                return db.ExecuteScalar<TResult>(sql, parameters);
            }
        }
        // General full-column fetch by id
        public virtual T GetById(int id)
        {
            string sql = $"SELECT * FROM {TableName} WHERE {KeyColumn} = @Id";

            var p = new List<DBParameter>
            {
                new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@Id"}
            };

            return ExecuteQuerySingle<T>(sql, p);
        }

        public virtual T GetById(long id)
        {
            string sql = $"SELECT * FROM {TableName} WHERE {KeyColumn} = @Id";

            var p = new List<DBParameter> {
                new DBParameter {DataValue = id, DataDbType = DbType.Int64, ParamName = "@Id" }};

            return ExecuteQuerySingle<T>(sql, p);
        }

        // Minimal/core columns fetch by id (repos can override CoreColumns for performance)
        public virtual T GetKeyColsById(int id)
        {
            string cols = SelectListOrAll(CoreColumns);
            string sql = $"SELECT {cols} FROM {TableName} WHERE {KeyColumn} = @Id";

            var p = new List<DBParameter>
            {
                new DBParameter {DataValue = id, DataDbType = DbType.Int32, ParamName = "@Id" }
            };

            return ExecuteQuerySingle<T>(sql, p);
        }

        public virtual List<T> GetAll()
        {
            return GetAll(null);
        }

        // Added overload to support ObjectDataSource with SortBy parameter
        public virtual List<T> GetAll(string SortBy)
        {
            var list = new List<T>();
            string sql = $"SELECT * FROM {TableName}";
            if (!string.IsNullOrWhiteSpace(SortBy))
            {
                sql += " ORDER BY " + SortBy;
            }
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr.Read()) list.Add(DbMapper.Map<T>(rdr));
            }
            return list;
        }

        // Lookup values for dropdowns/combo-boxes (repos can override LookupColumns to project id + text)
        public virtual List<T> GetLookupValues(string sortBy = null)
        {
            var list = new List<T>();
            string cols = SelectListOrAll(LookupColumns);
            string sql = $"SELECT {cols} FROM {TableName}";
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                sql += " ORDER BY " + sortBy;
            }
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr.Read()) list.Add(DbMapper.Map<T>(rdr));
            }
            return list;
        }

        /// <summary>
        /// Gets formatted lookup list for dropdowns with standard formatting:
        /// - Disabled items prefixed with "_" to sort to bottom
        /// - Sorted by enabled status first, then by display text
        /// Only works if T implements ILookupEntity
        /// </summary>
        /// <returns>List of entities formatted for dropdown display</returns>
        public virtual List<T> GetLookupList()
        {
            // If T implements ILookupEntity, use enhanced formatting
            if (typeof(ILookupEntity).IsAssignableFrom(typeof(T)))
            {
                var list = GetLookupValues();

                // Sort: enabled items first (alphabetically), then disabled items (alphabetically)
                var lookupEntities = list.Cast<ILookupEntity>().ToList();
                var sorted = lookupEntities
                    .OrderByDescending(e => e.IsEnabled() ?? true)  // Enabled first
                    .ThenBy(e => e.GetDisplayText())                // Then alphabetically
                    .Cast<T>()
                    .ToList();

                return sorted;
            }

            // Fallback: just return lookup values sorted
            return GetLookupValues();
        }

        /// <summary>
        /// Gets all enabled records suitable for active lookups
        /// Override in derived repositories if "Enabled" column name differs
        /// </summary>
        /// <returns>List of enabled entities only</returns>
        public virtual List<T> GetAllEnabled(string sortBy = null)
        {
            var list = new List<T>();
            string sql = $"SELECT * FROM {TableName} WHERE Enabled = 1";
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                sql += " ORDER BY " + sortBy;
            }
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr.Read()) list.Add(DbMapper.Map<T>(rdr));
            }
            return list;
        }
        protected IDataReader ExecReader(string sql, List<DBParameter> parameters = null)
        {
            var db = CreateDb();

            // ExecuteReader uses CommandBehavior.CloseConnection,
            // so disposing the reader will close the SQL connection.
            // Do NOT wrap db in using here, otherwise the connection may close
            // before the caller reads the data.
            return db.ExecuteReader(sql, parameters);
        }
        /// <summary>
        /// GetMappedProperties() A method to retrieve properties of the entity type T
        /// </summary>
        /// <param name="includeKey">Whether to include the key column in the result</param>
        /// <returns>Enumerable of PropertyInfo objects </returns>
        protected virtual IEnumerable<PropertyInfo> GetMappedProperties(bool includeKey = false)
        {
            return typeof(T).GetProperties()
                .Where(p =>
                    (includeKey || !string.Equals(p.Name, KeyColumn, StringComparison.OrdinalIgnoreCase))
                    && p.CanRead
                    && p.CanWrite);
        }
        /// <summary>
        /// Inserts a new entity record
        /// Derived repositories MUST override this to provide proper INSERT statement
        /// </summary>
        /// <param name="entity">Entity to insert</param>
        /// <returns>ID of newly inserted record, or -1 on error</returns>
        public virtual int Insert(T entity)
        {
            var props = GetMappedProperties(); // exclude key by default

            var columnNames = string.Join(", ", props.Select(p => p.Name));
            var paramNames = string.Join(", ", props.Select(p => "@" + p.Name));

            string sql = $@"INSERT INTO {TableName} ({columnNames}) VALUES ({paramNames}); " +
                "SELECT SCOPE_IDENTITY();";

            var parameters = props.Select(p => new DBParameter
            {
                ParamName = "@" + p.Name,
                DataValue = p.GetValue(entity) ?? DBNull.Value,
                DataDbType = DbType.Object // improve later if needed
            }).ToList();

            return ExecuteScalar<int>(sql, parameters);
        }
        /// <summary>
        /// Updates an existing entity record
        /// Derived repositories MUST override this to provide proper UPDATE statement
        /// </summary>
        /// <param name="entity">Entity to update</param>
        /// <returns>True if update successful, false otherwise</returns>
        public virtual int Update(T entity)
        {
            var props = GetMappedProperties(); // exclude key

            var setClause = string.Join(", ",
                props.Select(p => $"{p.Name} = @{p.Name}"));

            string sql = $@"
        UPDATE {TableName}
        SET {setClause}
        WHERE {KeyColumn} = @{KeyColumn}";

            // include ALL props for parameters (including key)
            var allProps = GetMappedProperties(includeKey: true);

            var parameters = allProps.Select(p => new DBParameter
            {
                ParamName = "@" + p.Name,
                DataValue = p.GetValue(entity) ?? DBNull.Value,
                DataDbType = DbType.Object
            }).ToList();

            int result = ExecNonQuery(sql, parameters);
            return result;
        }
        /// <summary>
        /// Deletes an entity record by ID
        /// Default implementation uses TableName and KeyColumn.
        /// Override in derived repository if custom delete logic needed.
        /// </summary>
        /// <param name="id">ID of record to delete</param>
        /// <returns>True if delete successful, false otherwise</returns>
        public virtual bool Delete(int id)
        {
            string sql = $"DELETE FROM {TableName} WHERE {KeyColumn} = @Id";
            var parameters = new List<DBParameter>
            {
                new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@Id" }
            };

            int result = ExecNonQuery(sql, parameters);
            return result > 0;
        }

        public virtual bool Delete(long id)
        {
            string sql = $"DELETE FROM {TableName} WHERE {KeyColumn} = @Id";

            var parameters = new List<DBParameter>
            { new DBParameter { DataValue = id, DataDbType = DbType.Int64, ParamName = "@Id" }};

            int result = ExecNonQuery(sql, parameters);
            return result > 0;
        }

    }
}
