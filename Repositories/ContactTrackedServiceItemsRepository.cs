using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class ContactTrackedServiceItemsRepository : RepositoryBase<ContactTrackedServiceItem>
    {
        protected override string TableName => "ContactTrackedServiceItemsTbl";
        protected override string KeyColumn => "ContactTrackedServiceItemsID";

        protected override string CoreColumns =>
            "ContactTrackedServiceItemsID, ContactTypeID, ItemServiceTypeID, Notes";

        public List<ContactTrackedServiceItem> GetByContactTypeId(int contactTypeId)
        {
            const string sql = @"
                SELECT ContactTrackedServiceItemsID, ContactTypeID, ItemServiceTypeID, Notes
                FROM ContactTrackedServiceItemsTbl
                WHERE ContactTypeID = @ContactTypeID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactTypeID", DataValue = contactTypeId, DataDbType = DbType.Int32 }
            };

            var list = new List<ContactTrackedServiceItem>();
            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(DbMapper.Map<ContactTrackedServiceItem>(rdr));
                }
            }

            return list;
        }
    }
}
