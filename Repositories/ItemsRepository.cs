using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Models;
using TrackerSQL.Classes;

namespace TrackerSQL.Repositories
{
    public class ItemsRepository
    {
        /// <summary>
        /// Legacy Access used ORDER BY ItemEnabled (True=-1 before False=0).
        /// SQL Server requires DESC so enabled items (1) appear before disabled (0).
        /// </summary>
        private const string DefaultOrderItemLookupSort = "ItemEnabled DESC, SortOrder, ItemDesc";

        private const string OrderItemLookupSelectSql = @"
                SELECT ItemID,
                       CASE WHEN ItemEnabled = 1 THEN ItemDesc ELSE '_' + ItemDesc END AS ItemDesc,
                       ItemEnabled,
                       SortOrder
                FROM ItemsTbl";

        public Item GetById(int id)
        {
            string sql = "SELECT ItemID, SKU, ItemDesc, ItemEnabled, ItemsCharacteritics, ItemDetail, ItemServiceTypeID, ReplacementItemID, ItemUnitID, BasePrice, ItemShortName, SortOrder, UnitsPerQty FROM ItemsTbl WHERE ItemID = @Id";
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
            string sql = "SELECT ItemID, SKU, ItemDesc, ItemEnabled, ItemsCharacteritics, ItemDetail, ItemServiceTypeID, ReplacementItemID, ItemUnitID, BasePrice, ItemShortName, SortOrder, UnitsPerQty FROM ItemsTbl";
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

        public int Insert(Item item)
        {
            string sql = @"INSERT INTO ItemsTbl (ItemDesc, SKU, ItemEnabled, ItemsCharacteritics, ItemDetail, 
                          ItemServiceTypeID, ReplacementItemID, ItemShortName, SortOrder, UnitsPerQty, ItemUnitID) 
                          VALUES (@ItemDesc, @SKU, @ItemEnabled, @ItemsCharacteritics, @ItemDetail, 
                          @ServiceTypeId, @ReplacementID, @ItemShortName, @SortOrder, @UnitsPerQty, @UoMID)";
            
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ItemDesc", DataValue = item.ItemDesc ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@SKU", DataValue = item.SKU ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ItemEnabled", DataValue = item.ItemEnabled ?? true, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@ItemsCharacteritics", DataValue = item.ItemsCharacteritics ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ItemDetail", DataValue = item.ItemDetail ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ServiceTypeId", DataValue = item.ItemServiceTypeID ?? (object)DBNull.Value, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ReplacementID", DataValue = item.ReplacementItemID ?? (object)DBNull.Value, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ItemShortName", DataValue = item.ItemShortName ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@SortOrder", DataValue = item.SortOrder ?? 1, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@UnitsPerQty", DataValue = item.UnitsPerQty ?? 1.0, DataDbType = DbType.Double },
                new DBParameter { ParamName = "@UoMID", DataValue = item.ItemUnitID ?? (object)DBNull.Value, DataDbType = DbType.Int32 }
            };

            using (var db = new TrackerSQLDb())
            {
                return db.ExecuteNonQuery(sql, parameters);
            }
        }

        public int Update(Item item)
        {
            string sql = @"UPDATE ItemsTbl SET ItemDesc = @ItemDesc, SKU = @SKU, ItemEnabled = @ItemEnabled, 
                          ItemsCharacteritics = @ItemsCharacteritics, ItemDetail = @ItemDetail, 
                          ItemServiceTypeID = @ServiceTypeId, ReplacementItemID = @Replacement, 
                          ItemShortName = @ItemShortName, SortOrder = @SortOrder, UnitsPerQty = @UnitsPerQty, 
                          ItemUnitID = @UoMID WHERE ItemID = @ItemID";
            
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ItemDesc", DataValue = item.ItemDesc ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@SKU", DataValue = item.SKU ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ItemEnabled", DataValue = item.ItemEnabled ?? true, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@ItemsCharacteritics", DataValue = item.ItemsCharacteritics ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ItemDetail", DataValue = item.ItemDetail ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ServiceTypeId", DataValue = item.ItemServiceTypeID ?? (object)DBNull.Value, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@Replacement", DataValue = item.ReplacementItemID ?? (object)DBNull.Value, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ItemShortName", DataValue = item.ItemShortName ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@SortOrder", DataValue = item.SortOrder ?? 1, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@UnitsPerQty", DataValue = item.UnitsPerQty ?? 1.0, DataDbType = DbType.Double },
                new DBParameter { ParamName = "@UoMID", DataValue = item.ItemUnitID ?? (object)DBNull.Value, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ItemID", DataValue = item.ItemID, DataDbType = DbType.Int32 }
            };

            using (var db = new TrackerSQLDb())
            {
                return db.ExecuteNonQuery(sql, parameters);
            }
        }

        public int Delete(int itemId)
        {
            string sql = "DELETE FROM ItemsTbl WHERE ItemID = @ItemID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ItemID", DataValue = itemId, DataDbType = DbType.Int32 }
            };

            using (var db = new TrackerSQLDb())
            {
                return db.ExecuteNonQuery(sql, parameters);
            }
        }

        public int? GetItemServiceTypeId(int itemId)
        {
            var item = GetById(itemId);
            return item?.ItemServiceTypeID;
        }

        public int GetServiceTypeForItem(int itemId)
        {
            return GetItemServiceTypeId(itemId) ?? 0;
        }

        public List<int> GetItemIdsByServiceType(int serviceTypeId)
        {
            var list = new List<int>();
            const string sql = "SELECT ItemID FROM ItemsTbl WHERE ItemServiceTypeID = @ServiceTypeID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ServiceTypeID", DataValue = serviceTypeId, DataDbType = DbType.Int32 }
            };

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    if (rdr["ItemID"] != DBNull.Value)
                    {
                        list.Add(Convert.ToInt32(rdr["ItemID"]));
                    }
                }
            }

            return list;
        }

        public string GetItemDescById(int itemId)
        {
            var item = GetById(itemId);
            if (item == null)
                return string.Empty;

            return LookupFormatter.FormatLookupText(item.ItemDesc, item.ItemEnabled);
        }

        public int GetItemSortOrder(int itemId)
        {
            var item = GetById(itemId);
            return item?.SortOrder ?? 0;
        }

        public string GetItemSku(int itemId)
        {
            var item = GetById(itemId);
            return item?.SKU ?? string.Empty;
        }

        public string GetItemUnitOfMeasure(int itemId)
        {
            var item = GetById(itemId);
            if (item?.ItemUnitID == null || item.ItemUnitID <= 0)
            {
                return string.Empty;
            }

            return new ItemUnitsRepository().GetUnitOfMeasure(item.ItemUnitID.Value);
        }

        public string GetItemUnitDescription(int itemId)
        {
            var item = GetById(itemId);
            if (item?.ItemUnitID == null || item.ItemUnitID <= 0)
            {
                return string.Empty;
            }

            return new ItemUnitsRepository().GetUnitDescription(item.ItemUnitID.Value);
        }

        public List<OrderItemLookup> GetOrderItemLookups(string sortBy)
        {
            var list = new List<OrderItemLookup>();
            string orderBy = string.IsNullOrWhiteSpace(sortBy) ? DefaultOrderItemLookupSort : sortBy;
            string sql = OrderItemLookupSelectSql + " ORDER BY " + orderBy;

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(new OrderItemLookup
                    {
                        ItemTypeID = rdr["ItemID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ItemID"]),
                        ItemDesc = rdr["ItemDesc"] == DBNull.Value ? string.Empty : rdr["ItemDesc"].ToString()
                    });
                }
            }

            return list;
        }

        public List<OrderItemLookup> GetAllGroupTypeItems()
        {
            var list = new List<OrderItemLookup>();
            int? groupServiceTypeId = new SysDataRepository().GetGroupItemServiceTypeId();

            // Prefer SysData group service type; also include explicit "Group Item" rows
            const string sqlByServiceType = @"
                SELECT ItemID,
                       CASE WHEN ItemEnabled = 1 THEN ItemDesc ELSE '_' + ItemDesc END AS ItemDesc
                FROM ItemsTbl
                WHERE (@ServiceTypeID > 0 AND ItemServiceTypeID = @ServiceTypeID)
                   OR ItemsCharacteritics LIKE N'Group Item%'
                ORDER BY ItemEnabled DESC, SortOrder, ItemDesc";

            var parameters = new List<DBParameter>
            {
                new DBParameter
                {
                    ParamName = "@ServiceTypeID",
                    DataValue = groupServiceTypeId.GetValueOrDefault(0),
                    DataDbType = DbType.Int32
                }
            };

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sqlByServiceType, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    int id = rdr["ItemID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ItemID"]);
                    if (id <= 0 || list.Exists(x => x.ItemTypeID == id))
                        continue;

                    list.Add(new OrderItemLookup
                    {
                        ItemTypeID = id,
                        ItemDesc = rdr["ItemDesc"]?.ToString() ?? string.Empty
                    });
                }
            }

            // Fallback: groups already used as GroupItemServiceTypeID in membership rows
            if (list.Count == 0)
            {
                const string sqlFromMembership = @"
                    SELECT i.ItemID,
                           CASE WHEN i.ItemEnabled = 1 THEN i.ItemDesc ELSE '_' + ISNULL(i.ItemDesc, '') END AS ItemDesc
                    FROM ItemsTbl i
                    WHERE i.ItemID IN (SELECT DISTINCT GroupItemServiceTypeID FROM ItemGroupsTbl WHERE GroupItemServiceTypeID IS NOT NULL)
                    ORDER BY i.ItemEnabled DESC, i.SortOrder, i.ItemDesc";

                using (var db = new TrackerSQLDb())
                using (var rdr = db.ExecuteReader(sqlFromMembership))
                {
                    while (rdr != null && rdr.Read())
                    {
                        list.Add(new OrderItemLookup
                        {
                            ItemTypeID = rdr["ItemID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ItemID"]),
                            ItemDesc = rdr["ItemDesc"]?.ToString() ?? string.Empty
                        });
                    }
                }
            }

            return list;
        }

        public List<OrderItemLookup> GetItemsNotInGroup(int groupReferenceItemId)
        {
            if (groupReferenceItemId <= 0)
                return new List<OrderItemLookup>();

            var list = new List<OrderItemLookup>();
            const string sql = @"
                SELECT i.ItemID,
                       CASE WHEN i.ItemEnabled = 1 THEN i.ItemDesc ELSE '_' + i.ItemDesc END AS ItemDesc,
                       i.ItemEnabled
                FROM ItemsTbl i
                WHERE i.ItemServiceTypeID = @CoffeeServiceType
                  AND NOT EXISTS (
                      SELECT 1 FROM ItemGroupsTbl g
                      WHERE g.ItemID = i.ItemID AND g.GroupItemServiceTypeID = @GroupItemServiceTypeID)
                ORDER BY i.ItemEnabled DESC, i.SortOrder, i.ItemDesc";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@CoffeeServiceType", DataValue = SystemConstants.ServiceTypeConstants.Coffee, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@GroupItemServiceTypeID", DataValue = groupReferenceItemId, DataDbType = DbType.Int32 }
            };

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(new OrderItemLookup
                    {
                        ItemTypeID = rdr["ItemID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ItemID"]),
                        ItemDesc = rdr["ItemDesc"]?.ToString() ?? string.Empty,
                        ItemEnabled = rdr["ItemEnabled"] == DBNull.Value || Convert.ToBoolean(rdr["ItemEnabled"])
                    });
                }
            }

            return list;
        }

        public bool GroupNameExists(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName)) return false;

            int? groupServiceTypeId = new SysDataRepository().GetGroupItemServiceTypeId();
            if (!groupServiceTypeId.HasValue) return false;

            const string sql = @"
                SELECT COUNT(1) FROM ItemsTbl
                WHERE ItemDesc = @ItemDesc AND ItemServiceTypeID = @ServiceTypeID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ItemDesc", DataValue = groupName.Trim(), DataDbType = DbType.String },
                new DBParameter { ParamName = "@ServiceTypeID", DataValue = groupServiceTypeId.Value, DataDbType = DbType.Int32 }
            };

            using (var db = new TrackerSQLDb())
            {
                return db.ExecuteScalar<int>(sql, parameters) > 0;
            }
        }

        public int InsertGroupReferenceItem(Item item)
        {
            return Insert(item);
        }

        public bool UpdateGroupReferenceItem(Item item)
        {
            return Update(item) > 0;
        }

        private Item Map(IDataReader r)
        {
            return new Item
            {
                ItemID = r["ItemID"] == DBNull.Value ? 0 : Convert.ToInt32(r["ItemID"]),
                SKU = r["SKU"] == DBNull.Value ? string.Empty : r["SKU"].ToString(),
                ItemDesc = r["ItemDesc"] == DBNull.Value ? string.Empty : r["ItemDesc"].ToString(),
                ItemEnabled = r["ItemEnabled"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(r["ItemEnabled"]),
                ItemsCharacteritics = r["ItemsCharacteritics"] == DBNull.Value ? string.Empty : r["ItemsCharacteritics"].ToString(),
                ItemDetail = r["ItemDetail"] == DBNull.Value ? string.Empty : r["ItemDetail"].ToString(),
                ItemServiceTypeID = r["ItemServiceTypeID"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ItemServiceTypeID"]),
                ReplacementItemID = r["ReplacementItemID"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ReplacementItemID"]),
                ItemUnitID = r["ItemUnitID"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ItemUnitID"]),
                BasePrice = r["BasePrice"] == DBNull.Value ? (double?)null : Convert.ToDouble(r["BasePrice"]),
                ItemShortName = r["ItemShortName"] == DBNull.Value ? string.Empty : r["ItemShortName"].ToString(),
                SortOrder = r["SortOrder"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["SortOrder"]),
                UnitsPerQty = r["UnitsPerQty"] == DBNull.Value ? (double?)null : Convert.ToDouble(r["UnitsPerQty"])
            };
        }
    }
}
