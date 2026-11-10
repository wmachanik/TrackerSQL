using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class ContactTypesRepository : RepositoryBase<ContactType>
    {
        private static bool _driverCollectionOnlySeen;
        private static readonly object CollectionOnlySync = new object();
        private static HashSet<int> _collectionOnlyIds;
        private static DateTime _collectionOnlyCachedAt;
        private static readonly TimeSpan CollectionOnlyCacheFor = TimeSpan.FromMinutes(1);

        protected override string TableName => "ContactTypesTbl";
        protected override string KeyColumn => "ContactTypeID";

        protected override string CoreColumns =>
            "ContactTypeID, ContactTypeDesc";

        protected override string LookupColumns =>
            "ContactTypeID, ContactTypeDesc";

        private static bool DriverCollectionOnlyColumnExists()
        {
            if (!_driverCollectionOnlySeen)
            {
                try
                {
                    using (var db = new TrackerSQLDb())
                        _driverCollectionOnlySeen = db.ExecuteScalar<int>(
                            "SELECT CASE WHEN COL_LENGTH(N'dbo.ContactTypesTbl', N'DriverCollectionOnly') IS NULL THEN 0 ELSE 1 END") == 1;
                }
                catch
                {
                    _driverCollectionOnlySeen = false;
                }
            }
            return _driverCollectionOnlySeen;
        }

        public static bool IsDriverCollectionOnly(int? contactTypeId)
        {
            if (!contactTypeId.HasValue || contactTypeId.Value <= 0)
                return false;
            if (!DriverCollectionOnlyColumnExists())
                return false;

            lock (CollectionOnlySync)
            {
                if (_collectionOnlyIds == null || DateTime.UtcNow - _collectionOnlyCachedAt > CollectionOnlyCacheFor)
                {
                    _collectionOnlyIds = new HashSet<int>();
                    try
                    {
                        using (var db = new TrackerSQLDb())
                        using (var rdr = db.ExecuteReader(
                            "SELECT ContactTypeID FROM ContactTypesTbl WHERE DriverCollectionOnly = 1"))
                        {
                            while (rdr.Read())
                            {
                                if (rdr["ContactTypeID"] != DBNull.Value)
                                    _collectionOnlyIds.Add(Convert.ToInt32(rdr["ContactTypeID"]));
                            }
                        }
                    }
                    catch
                    {
                        _collectionOnlyIds = new HashSet<int>();
                    }
                    _collectionOnlyCachedAt = DateTime.UtcNow;
                }

                return _collectionOnlyIds.Contains(contactTypeId.Value);
            }
        }

        public static void InvalidateCollectionOnlyCache()
        {
            lock (CollectionOnlySync)
                _collectionOnlyIds = null;
        }

        public override int Insert(ContactType entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            bool driverFlag = DriverCollectionOnlyColumnExists();

            string sql = driverFlag
                ? @"INSERT INTO ContactTypesTbl (ContactTypeDesc, Notes, DriverCollectionOnly)
                    VALUES (@ContactTypeDesc, @Notes, @DriverCollectionOnly);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);"
                : @"INSERT INTO ContactTypesTbl (ContactTypeDesc, Notes)
                    VALUES (@ContactTypeDesc, @Notes);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactTypeDesc", DataValue = entity.ContactTypeDesc ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Notes", DataValue = entity.Notes ?? string.Empty, DataDbType = DbType.String }
            };
            if (driverFlag)
                parameters.Add(new DBParameter { ParamName = "@DriverCollectionOnly", DataValue = entity.DriverCollectionOnly, DataDbType = DbType.Boolean });

            int id = ExecuteScalar<int>(sql, parameters);
            InvalidateCollectionOnlyCache();
            return id;
        }

        public override int Update(ContactType entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            bool driverFlag = DriverCollectionOnlyColumnExists();

            string sql = driverFlag
                ? @"UPDATE ContactTypesTbl
                    SET ContactTypeDesc = @ContactTypeDesc,
                        Notes = @Notes,
                        DriverCollectionOnly = @DriverCollectionOnly
                    WHERE ContactTypeID = @ContactTypeID"
                : @"UPDATE ContactTypesTbl
                    SET ContactTypeDesc = @ContactTypeDesc,
                        Notes = @Notes
                    WHERE ContactTypeID = @ContactTypeID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactTypeDesc", DataValue = entity.ContactTypeDesc ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Notes", DataValue = entity.Notes ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ContactTypeID", DataValue = entity.ContactTypeID, DataDbType = DbType.Int32 }
            };
            if (driverFlag)
                parameters.Add(new DBParameter { ParamName = "@DriverCollectionOnly", DataValue = entity.DriverCollectionOnly, DataDbType = DbType.Boolean });

            int rows = ExecNonQuery(sql, parameters);
            InvalidateCollectionOnlyCache();
            return rows;
        }
    }
}
