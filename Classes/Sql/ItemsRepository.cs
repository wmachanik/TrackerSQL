using System;
using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Classes.Poco;
using TrackerDotNet.Classes;
using TrackerSQL.Classes;

namespace TrackerDotNet.Classes.Sql
{
    public class ItemsRepository
    {
        public Item GetById(int id)
        {
            string sql = "SELECT ItemID, SKU, ItemDesc, ItemEnabled, ItemDetail, ItemServiceTypeID, ReplacementItemID, ItemUnitID, BasePrice FROM ItemsTbl WHERE ItemID = @Id";
            var p = new List<DBParameter> { new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@Id" } };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                if (rdr != null && rdr.Read()) return Map(rdr);
            }
            return null;
        }

        public List<Item> GetAll()
        {
            return GetAll(null);
        }

        public List<Item> GetAll(string SortBy)
        {
            var list = new List<Item>();
            string sql = "SELECT ItemID, SKU, ItemDesc, ItemEnabled, ItemDetail, ItemServiceTypeID, ReplacementItemID, ItemUnitID, BasePrice FROM ItemsTbl";
            if (!string.IsNullOrWhiteSpace(SortBy))
            {
                sql += " ORDER BY " + SortBy;
            }
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr.Read()) list.Add(Map(rdr));
            }
            return list;
        }

        private Item Map(IDataReader r)
        {
            return new Item
            {
                ItemID = r["ItemID"] == DBNull.Value ? 0 : Convert.ToInt32(r["ItemID"]),
                SKU = r["SKU"] == DBNull.Value ? string.Empty : r["SKU"].ToString(),
                ItemDesc = r["ItemDesc"] == DBNull.Value ? string.Empty : r["ItemDesc"].ToString(),
                ItemEnabled = r["ItemEnabled"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(r["ItemEnabled"]),
                ItemDetail = r["ItemDetail"] == DBNull.Value ? string.Empty : r["ItemDetail"].ToString(),
                ItemServiceTypeID = r["ItemServiceTypeID"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ItemServiceTypeID"]),
                ReplacementItemID = r["ReplacementItemID"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ReplacementItemID"]),
                ItemUnitID = r["ItemUnitID"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ItemUnitID"]),
                BasePrice = r["BasePrice"] == DBNull.Value ? (double?)null : Convert.ToDouble(r["BasePrice"])
            };
        }
    }
}
