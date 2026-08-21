using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class WooPackagingServiceTypeRepository : RepositoryBase<WooPackagingServiceType>
    {
        protected override string TableName => "WooPackagingServiceTypeTbl";
        protected override string KeyColumn => "PackagingServiceTypeID";

        public List<WooPackagingServiceType> GetAllLinks()
        {
            var list = new List<WooPackagingServiceType>();
            const string sql = "SELECT PackagingServiceTypeID, ItemPackagingID, ItemServiceTypeID FROM WooPackagingServiceTypeTbl";
            using (var rdr = ExecReader(sql))
            {
                while (rdr != null && rdr.Read())
                    list.Add(DbMapper.Map<WooPackagingServiceType>(rdr));
            }
            return list;
        }

        /// <summary>
        /// Packaging IDs allowed for a service type. Empty list means unrestricted (all packaging).
        /// </summary>
        public HashSet<int> GetAllowedPackagingIds(int itemServiceTypeId)
        {
            var all = GetAllLinks();
            if (all.Count == 0)
                return new HashSet<int>();

            var allowed = new HashSet<int>();
            foreach (var link in all)
            {
                if (itemServiceTypeId <= 0 || link.ItemServiceTypeID == itemServiceTypeId)
                    allowed.Add(link.ItemPackagingID);
            }
            return allowed;
        }

        public override int Insert(WooPackagingServiceType entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            const string sql = @"
INSERT INTO WooPackagingServiceTypeTbl (ItemPackagingID, ItemServiceTypeID)
VALUES (@ItemPackagingID, @ItemServiceTypeID);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ItemPackagingID", DataValue = entity.ItemPackagingID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ItemServiceTypeID", DataValue = entity.ItemServiceTypeID, DataDbType = DbType.Int32 }
            };
            return ExecuteScalar<int>(sql, parameters);
        }
    }
}
