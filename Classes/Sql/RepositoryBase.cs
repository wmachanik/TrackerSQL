using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerDotNet.Classes;

namespace TrackerDotNet.Classes.Sql
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

        protected IDataReader ExecReader(string sql, List<DBParameter> parameters = null)
        {
            var db = CreateDb();
            // reader will own connection; caller must dispose
            return db.ExecuteReader(sql, parameters);
        }

        // General full-column fetch by id
        public virtual T GetById(int id)
        {
            string sql = $"SELECT * FROM {TableName} WHERE {KeyColumn} = @Id";
            var p = new List<DBParameter> { new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@Id" } };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                if (rdr != null && rdr.Read())
                {
                    return DbMapper.Map<T>(rdr);
                }
            }
            return default(T);
        }

        // Minimal/core columns fetch by id (repos can override CoreColumns for performance)
        public virtual T GetKeyColsById(int id)
        {
            string cols = SelectListOrAll(CoreColumns);
            string sql = $"SELECT {cols} FROM {TableName} WHERE {KeyColumn} = @Id";
            var p = new List<DBParameter> { new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@Id" } };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                if (rdr != null && rdr.Read())
                {
                    return DbMapper.Map<T>(rdr);
                }
            }
            return default(T);
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
    }
}
