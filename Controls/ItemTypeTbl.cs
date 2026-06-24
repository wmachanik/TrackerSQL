// Decompiled with JetBrains decompiler
// Type: TrackerSQL.control.ItemTypeTbl
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using TrackerSQL.Classes;

//- only form later versions #nullable disable
namespace TrackerSQL.Controls
{
    public class ItemTypeTbl
    {
        public const int CONST_NEEDDESCRIPTION_SORT_ORDER = 10;
        public const int CONST_SERVICEITEMID = 36;
        private const string CONST_SQL_SELECT = "SELECT ItemTypeID, SKU, ItemDesc, ItemEnabled, ItemsCharacteritics, ItemDetail, ServiceTypeID, ReplacementID, ItemShortName, SortOrder, ItemUnitID FROM ItemTypeTbl";
        private const string CONST_SQL_SELECTITEMDESC = "SELECT ItemDesc, ItemEnabled FROM ItemTypeTbl WHERE ItemTypeID = @ItemTypeID";
        private const string CONST_SQL_SELECTITEMTYPEFROMID = "SELECT SKU, ItemDesc, ItemEnabled, ItemsCharacteritics, ItemDetail, ServiceTypeID, ReplacementID, ItemShortName, SortOrder, ItemUnitID FROM ItemTypeTbl WHERE ItemTypeID = @ItemTypeID";
        private const string CONST_SQL_SELECTITEMSKU = "SELECT SKU FROM ItemTypeTbl WHERE ItemTypeID = @ItemTypeID";
        private const string CONST_SQL_SELECTSERVICETYPEID = "SELECT ServiceTypeID FROM ItemTypeTbl WHERE ItemTypeID = @ItemTypeID";
        private const string CONST_SQL_SELECTITEMTYPES = "SELECT ItemTypeID, IIF(ItemEnabled, ItemDesc, '_' + ItemDesc) AS ItemDesc FROM ItemTypeTbl";
        private const string CONST_SQL_SELECT_ITEMDESCISGROUPNAME = "SELECT ItemDesc FROM ItemTypeTbl WHERE ItemDesc = @ItemDesc";
        private const string CONST_SQL_SELECTITEMTYPESNOTINITEMGROUP = "SELECT ItemTypeID, ItemDesc, ItemEnabled FROM ItemTypeTbl WHERE (ServiceTypeId = 2) AND (NOT EXISTS (SELECT ItemTypeID FROM ItemGroupTbl WHERE (ItemGroupTbl.ItemTypeID = ItemTypeTbl.ItemTypeID) AND (GroupItemTypeID = @GroupItemTypeID)))";
        private const string CONST_SQL_ITEMTYPEUNITS = "SELECT ItemUnitsTbl.UnitOfMeasure FROM (ItemUnitsTbl INNER JOIN ItemTypeTbl ON ItemUnitsTbl.ItemUnitID = ItemTypeTbl.ItemUnitID)  WHERE (ItemTypeTbl.ItemTypeID = @ItemTypeID)";
        private const string CONST_SQL_ITEMSORTORDER = "SELECT SortOrder FROM ItemTypeTbl WHERE (ItemTypeTbl.ItemTypeID = @ItemTypeID)";
        private const string CONST_SQL_LISTOFITEMIDSOFSERVICETYPE = "SELECT ItemTypeTbl.ItemTypeID  FROM (ItemTypeTbl LEFT OUTER JOIN ServiceTypesTbl ON ItemTypeTbl.ServiceTypeId = ServiceTypesTbl.ServiceTypeId)  WHERE (ServiceTypesTbl.ServiceTypeId = @ServiceTypeId)";
        private const string CONST_SQL_GETALLITEMSOFSERVICETYPE = "SELECT ItemTypeID, ItemDesc, ItemEnabled, ServiceTypeId FROM ItemTypeTbl WHERE (ServiceTypeId = @ServiceTypeId) AND (ItemEnabled = @ItemEnabled) ORDER BY ItemDesc";
        private const string CONST_SQL_UPDATE = "UPDATE ItemTypeTbl SET SKU = @SKU, ItemDesc = @ItemDesc, ItemEnabled = @ItemEnabled, ItemsCharacteritics = @ItemsCharacteritics, ItemDetail = @ItemDetail, ServiceTypeId = @ServiceTypeId, ReplacementID = @ReplacementID, ItemShortName = @ItemShortName, SortOrder = @SortOrder, ItemUnitID = @ItemUnitID WHERE ItemTypeID = @ItemTypeID";
        private const string CONST_SQL_INSERT = "INSERT INTO ItemTypeTbl (ItemDesc, SKU, ItemEnabled, ItemsCharacteritics, ItemDetail, ServiceTypeId, ReplacementID, ItemShortName, SortOrder, ItemUnitID) VALUES (@ItemDesc, @SKU, @ItemEnabled, @ItemsCharacteritics, @ItemDetail, @ServiceTypeId, @ReplacementID, @ItemShortName, @SortOrder, @ItemUnitID)";
        private const string CONST_SQL_DELETEBYID = "DELETE FROM ItemTypeTbl WHERE ItemTypeID = @ItemTypeID";
        private int _ItemTypeID;
        private string _SKU;
        private string _ItemDesc;
        private bool _ItemEnabled;
        private string _ItemsCharacteritics;
        private string _ItemDetail;
        private int _ServiceTypeID;
        private int _ReplacementID;
        private string _ItemShortName;
        private int _SortOrder;
        private int _ItemUnitID;

        public static class ItemTypeConstants
        {
            public const int NotesSortOrder = 10;
            public const int GroupSortOrder = 15;
            // Add other special sort orders as needed
        }
        public ItemTypeTbl()
        {
            this._ItemTypeID = SystemConstants.DatabaseConstants.InvalidID;
            this._SKU = string.Empty;
            this._ItemDesc = string.Empty;
            this._ItemEnabled = false;
            this._ItemsCharacteritics = string.Empty;
            this._ItemDetail = string.Empty;
            this._ServiceTypeID = 0;
            this._ReplacementID = 0;
            this._ItemShortName = string.Empty;
            this._SortOrder = 0;
            this._ItemUnitID = 0;
        }

        public int ItemTypeID
        {
            get => this._ItemTypeID;
            set => this._ItemTypeID = value;
        }

        public string SKU
        {
            get => this._SKU;
            set => this._SKU = value;
        }

        public string ItemDesc
        {
            get => this._ItemDesc;
            set => this._ItemDesc = value;
        }

        public bool ItemEnabled
        {
            get => this._ItemEnabled;
            set => this._ItemEnabled = value;
        }

        public string ItemsCharacteritics
        {
            get => this._ItemsCharacteritics;
            set => this._ItemsCharacteritics = value;
        }

        public string ItemDetail
        {
            get => this._ItemDetail;
            set => this._ItemDetail = value;
        }

        public int ServiceTypeID
        {
            get => this._ServiceTypeID;
            set => this._ServiceTypeID = value;
        }

        public int ReplacementID
        {
            get => this._ReplacementID;
            set => this._ReplacementID = value;
        }

        public string ItemShortName
        {
            get => this._ItemShortName;
            set => this._ItemShortName = value;
        }

        public int SortOrder
        {
            get => this._SortOrder;
            set => this._SortOrder = value;
        }

        public int ItemUnitID
        {
            get => this._ItemUnitID;
            set => this._ItemUnitID = value;
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<ItemTypeTbl> GetAll(string SortBy)
        {
            List<ItemTypeTbl> all = new List<ItemTypeTbl>();
            string strSQL = "SELECT ItemTypeID, SKU, ItemDesc, ItemEnabled, ItemsCharacteritics, ItemDetail, ServiceTypeID, ReplacementID, ItemShortName, SortOrder, ItemUnitID FROM ItemTypeTbl" + (!string.IsNullOrEmpty(SortBy) ? " ORDER BY " + SortBy : " ORDER BY ItemEnabled, SortOrder, ItemDesc");

            using (var db = new TrackerSQLDb())
            {
                using (IDataReader dataReader = db.ExecuteReader(strSQL))
                {
                    if (dataReader != null)
                    {
                        while (dataReader.Read())
                            all.Add(new ItemTypeTbl()
                            {
                                ItemTypeID = dataReader["ItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeID"]),
                                SKU = dataReader["SKU"] == DBNull.Value ? string.Empty : dataReader["SKU"].ToString(),
                                ItemDesc = dataReader["ItemDesc"] == DBNull.Value ? string.Empty : dataReader["ItemDesc"].ToString(),
                                ItemEnabled = dataReader["ItemEnabled"] != DBNull.Value && Convert.ToBoolean(dataReader["ItemEnabled"]),
                                ItemsCharacteritics = dataReader["ItemsCharacteritics"] == DBNull.Value ? string.Empty : dataReader["ItemsCharacteritics"].ToString(),
                                ItemDetail = dataReader["ItemDetail"] == DBNull.Value ? string.Empty : dataReader["ItemDetail"].ToString(),
                                ServiceTypeID = dataReader["ServiceTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ServiceTypeID"]),
                                ReplacementID = dataReader["ReplacementID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ReplacementID"]),
                                ItemShortName = dataReader["ItemShortName"] == DBNull.Value ? string.Empty : dataReader["ItemShortName"].ToString(),
                                SortOrder = dataReader["SortOrder"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["SortOrder"]),
                                ItemUnitID = dataReader["ItemUnitID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemUnitID"])
                            });
                    }
                }
            }

            return all;
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public List<ItemTypeTbl> GetAll()
        {
            return this.GetAll("ItemDesc");
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<ItemTypeTbl> GetAllItemDesc() => this.GetAllItemDesc("");

        public List<ItemTypeTbl> GetAllItemDesc(string SortBy)
        {
            List<ItemTypeTbl> allItemDesc = new List<ItemTypeTbl>();
            string strSQL = "SELECT ItemTypeID, IIF(ItemEnabled, ItemDesc, '_' + ItemDesc) AS ItemDesc FROM ItemTypeTbl" + (!string.IsNullOrEmpty(SortBy) ? " ORDER BY " + SortBy : " ORDER BY ItemEnabled, SortOrder, ItemDesc");

            using (var db = new TrackerSQLDb())
            {
                using (IDataReader dataReader = db.ExecuteReader(strSQL))
                {
                    if (dataReader != null)
                    {
                        while (dataReader.Read())
                            allItemDesc.Add(new ItemTypeTbl()
                            {
                                ItemTypeID = dataReader["ItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeID"]),
                                ItemDesc = dataReader["ItemDesc"] == DBNull.Value ? string.Empty : dataReader["ItemDesc"].ToString()
                            });
                    }
                }
            }

            return allItemDesc;
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<ItemTypeTbl> GetAllItemsNotInItemGroup(int pGroupItemTypeID)
        {
            return this.GetAllItemsNotInItemGroup(pGroupItemTypeID, "");
        }

        public List<ItemTypeTbl> GetAllItemsNotInItemGroup(int pGroupItemTypeID, string SortBy)
        {
            List<ItemTypeTbl> itemsNotInItemGroup = new List<ItemTypeTbl>();
            if (!pGroupItemTypeID.Equals(-1))
            {
                string strSQL = "SELECT ItemTypeID, ItemDesc, ItemEnabled FROM ItemTypeTbl WHERE (ServiceTypeId = 2) AND (NOT EXISTS (SELECT ItemTypeID FROM ItemGroupTbl WHERE (ItemGroupTbl.ItemTypeID = ItemTypeTbl.ItemTypeID) AND (GroupItemTypeID = @GroupItemTypeID)))" + (!string.IsNullOrEmpty(SortBy) ? " ORDER BY " + SortBy : " ORDER BY ItemEnabled, SortOrder, ItemDesc");

                using (var db = new TrackerSQLDb())
                {
                    var parameters = new List<DBParameter>
                    {
                        new DBParameter { DataValue = pGroupItemTypeID, DataDbType = DbType.Int32, ParamName = "@GroupItemTypeID" }
                    };

                    using (IDataReader dataReader = db.ExecuteReader(strSQL, parameters))
                    {
                        if (dataReader != null)
                        {
                            while (dataReader.Read())
                                itemsNotInItemGroup.Add(new ItemTypeTbl()
                                {
                                    ItemTypeID = dataReader["ItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeID"]),
                                    ItemDesc = dataReader["ItemDesc"] == DBNull.Value ? string.Empty : dataReader["ItemDesc"].ToString(),
                                    ItemEnabled = dataReader["ItemEnabled"] != DBNull.Value && Convert.ToBoolean(dataReader["ItemEnabled"])
                                });
                        }
                    }
                }
            }
            return itemsNotInItemGroup;
        }
        // Static cache: ItemTypeID -> (ItemDesc, ItemEnabled)
        [DataObjectMethod(DataObjectMethodType.Select)]
        public static string GetItemTypeDescById(int pItemID, bool pCheckIfSoldOut = true)
        {
            string itemTypeDesc = string.Empty;
            bool enabled = true;

            using (var db = new TrackerSQLDb())
            {
                var parameters = new List<DBParameter>
                {
                    new DBParameter { DataValue = pItemID, DataDbType = DbType.Int32, ParamName = "@ItemTypeID" }
                };

                using (var dataReader = db.ExecuteReader(CONST_SQL_SELECTITEMDESC, parameters))
                {
                    if (dataReader != null && dataReader.Read())
                    {
                        itemTypeDesc = dataReader["ItemDesc"] == DBNull.Value ? string.Empty : dataReader["ItemDesc"].ToString();
                        enabled = dataReader["ItemEnabled"] != DBNull.Value && Convert.ToBoolean(dataReader["ItemEnabled"]);
                    }
                }
            }

            if (pCheckIfSoldOut && !enabled)
                return itemTypeDesc + " SOLD OUT";
            return itemTypeDesc;
        }

        //public string GetItemTypeDescById(int pItemID, bool pCheckIfSoldOut)
        //{
        //    string itemTypeDesc = string.Empty;
        //    using (var trackerDb = new TrackerSQLDb())`n    {;
        //    trackerDb.AddWhereParams((object)pItemID, DbType.Int32, "@ItemTypeID");
        //    IDataReader dataReader = trackerDb.ExecuteReader(sql, parameters) // TODO: Add parameters list; //   "SELECT ItemDesc, ItemEnabled FROM ItemTypeTbl WHERE ItemTypeID = ?");
        //    if (dataReader != null)
        //    {
        //        if (dataReader.Read())
        //        {
        //            itemTypeDesc = dataReader["ItemDesc"] == DBNull.Value ? string.Empty : dataReader["ItemDesc"].ToString();
        //            if (pCheckIfSoldOut && (dataReader["ItemEnabled"] == DBNull.Value || !Convert.ToBoolean(dataReader["ItemEnabled"])))
        //                itemTypeDesc += " SOLD OUT";
        //        }
        //        } // Auto-dispose
        //    }
        //    } // Auto-dispose
        //    return itemTypeDesc;
        //}

        public ItemTypeTbl GetItemTypeFromID(int pItemTypeID)
        {
            ItemTypeTbl itemTypeFromId = new ItemTypeTbl();

            using (var db = new TrackerSQLDb())
            {
                var parameters = new List<DBParameter>
                {
                    new DBParameter { DataValue = pItemTypeID, DataDbType = DbType.Int32, ParamName = "@ItemTypeID" }
                };

                using (IDataReader dataReader = db.ExecuteReader("SELECT SKU, ItemDesc, ItemEnabled, ItemsCharacteritics, ItemDetail, ServiceTypeID, ReplacementID, ItemShortName, SortOrder, ItemUnitID FROM ItemTypeTbl WHERE ItemTypeID = @ItemTypeID", parameters))
                {
                    if (dataReader.Read())
                    {
                        itemTypeFromId.ItemTypeID = pItemTypeID;
                        itemTypeFromId.SKU = dataReader["SKU"] == DBNull.Value ? string.Empty : dataReader["SKU"].ToString();
                        itemTypeFromId.ItemDesc = dataReader["ItemDesc"] == DBNull.Value ? string.Empty : dataReader["ItemDesc"].ToString();
                        itemTypeFromId.ItemEnabled = dataReader["ItemEnabled"] != DBNull.Value && Convert.ToBoolean(dataReader["ItemEnabled"]);
                        itemTypeFromId.ItemsCharacteritics = dataReader["ItemsCharacteritics"] == DBNull.Value ? string.Empty : dataReader["ItemsCharacteritics"].ToString();
                        itemTypeFromId.ItemDetail = dataReader["ItemDetail"] == DBNull.Value ? string.Empty : dataReader["ItemDetail"].ToString();
                        itemTypeFromId.ServiceTypeID = dataReader["ServiceTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ServiceTypeID"]);
                        itemTypeFromId.ReplacementID = dataReader["ReplacementID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ReplacementID"]);
                        itemTypeFromId.ItemShortName = dataReader["ItemShortName"] == DBNull.Value ? string.Empty : dataReader["ItemShortName"].ToString();
                        itemTypeFromId.SortOrder = dataReader["SortOrder"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["SortOrder"]);
                        itemTypeFromId.ItemUnitID = dataReader["ItemUnitID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemUnitID"]);
                    }
                }
            }

            return itemTypeFromId;
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public string GetItemTypeSKU(int pItemID)
        {
            string itemTypeSku = string.Empty;

            using (var db = new TrackerSQLDb())
            {
                var parameters = new List<DBParameter>
                {
                    new DBParameter { DataValue = pItemID, DataDbType = DbType.Int32, ParamName = "@ItemTypeID" }
                };

                using (IDataReader dataReader = db.ExecuteReader("SELECT SKU FROM ItemTypeTbl WHERE ItemTypeID = @ItemTypeID", parameters))
                {
                    if (dataReader != null && dataReader.Read())
                        itemTypeSku = dataReader["SKU"] == DBNull.Value ? string.Empty : dataReader["SKU"].ToString();
                }
            }

            return itemTypeSku;
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public int GetServiceID(int pItemID)
        {
            int serviceId = 0;

            using (var db = new TrackerSQLDb())
            {
                var parameters = new List<DBParameter>
                {
                    new DBParameter { DataValue = pItemID, DataDbType = DbType.Int32, ParamName = "@ItemTypeID" }
                };

                using (IDataReader dataReader = db.ExecuteReader("SELECT ServiceTypeID FROM ItemTypeTbl WHERE ItemTypeID = @ItemTypeID", parameters))
                {
                    if (dataReader != null && dataReader.Read())
                        serviceId = dataReader["ServiceTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ServiceTypeID"]);
                }
            }

            return serviceId;
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public string GetItemUnitOfMeasure(int pItemID)
        {
            string itemUnitOfMeasure = string.Empty;

            using (var db = new TrackerSQLDb())
            {
                var parameters = new List<DBParameter>
                {
                    new DBParameter { DataValue = pItemID, DataDbType = DbType.Int32, ParamName = "@ItemTypeID" }
                };

                using (IDataReader dataReader = db.ExecuteReader("SELECT ItemUnitsTbl.UnitOfMeasure FROM (ItemUnitsTbl INNER JOIN ItemTypeTbl ON ItemUnitsTbl.ItemUnitID = ItemTypeTbl.ItemUnitID) WHERE (ItemTypeTbl.ItemTypeID = @ItemTypeID)", parameters))
                {
                    if (dataReader != null && dataReader.Read())
                        itemUnitOfMeasure = dataReader["UnitOfMeasure"] == DBNull.Value ? string.Empty : dataReader["UnitOfMeasure"].ToString();
                }
            }

            return itemUnitOfMeasure;
        }

        public int GetItemSortOrder(int pItemID)
        {
            int itemSortOrder = 0;

            using (var db = new TrackerSQLDb())
            {
                var parameters = new List<DBParameter>
                {
                    new DBParameter { DataValue = pItemID, DataDbType = DbType.Int32, ParamName = "@ItemTypeID" }
                };

                using (IDataReader dataReader = db.ExecuteReader("SELECT SortOrder FROM ItemTypeTbl WHERE (ItemTypeTbl.ItemTypeID = @ItemTypeID)", parameters))
                {
                    if (dataReader != null && dataReader.Read())
                        itemSortOrder = dataReader["SortOrder"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["SortOrder"]);
                }
            }

            return itemSortOrder;
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<int> GetAllItemIDsofServiceType(int pServiceTypeID)
        {
            List<int> idsofServiceType = new List<int>();

            using (var db = new TrackerSQLDb())
            {
                var parameters = new List<DBParameter>
                {
                    new DBParameter { DataValue = pServiceTypeID, DataDbType = DbType.Int32, ParamName = "@ServiceTypeId" }
                };

                using (IDataReader dataReader = db.ExecuteReader("SELECT ItemTypeTbl.ItemTypeID FROM (ItemTypeTbl LEFT OUTER JOIN ServiceTypesTbl ON ItemTypeTbl.ServiceTypeId = ServiceTypesTbl.ServiceTypeId) WHERE (ServiceTypesTbl.ServiceTypeId = @ServiceTypeId)", parameters))
                {
                    if (dataReader != null)
                    {
                        while (dataReader.Read())
                            idsofServiceType.Add((int)dataReader["ItemTypeID"]);
                    }
                }
            }

            return idsofServiceType;
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<ItemTypeTbl> GetAllItemsofServiceType(int pServiceTypeID)
        {
            return this.GetAllItemsofServiceType(pServiceTypeID, true);
        }

        public List<ItemTypeTbl> GetAllItemsofServiceType(int pServiceTypeID, bool pEnabled)
        {
            List<ItemTypeTbl> itemsofServiceType = new List<ItemTypeTbl>();

            using (var db = new TrackerSQLDb())
            {
                var parameters = new List<DBParameter>
                {
                    new DBParameter { DataValue = pServiceTypeID, DataDbType = DbType.Int32, ParamName = "@ServiceTypeId" },
                    new DBParameter { DataValue = pEnabled, DataDbType = DbType.Boolean, ParamName = "@ItemEnabled" }
                };

                using (IDataReader dataReader = db.ExecuteReader("SELECT ItemTypeID, ItemDesc, ItemEnabled, ServiceTypeId FROM ItemTypeTbl WHERE (ServiceTypeId = @ServiceTypeId) AND (ItemEnabled = @ItemEnabled) ORDER BY ItemDesc", parameters))
                {
                    if (dataReader != null)
                    {
                        while (dataReader.Read())
                            itemsofServiceType.Add(new ItemTypeTbl()
                            {
                                ItemTypeID = dataReader["ItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeID"]),
                                ItemDesc = dataReader["ItemDesc"] == DBNull.Value ? string.Empty : dataReader["ItemDesc"].ToString(),
                                ItemEnabled = dataReader["ItemEnabled"] != DBNull.Value && Convert.ToBoolean(dataReader["ItemEnabled"]),
                                ServiceTypeID = dataReader["ServiceTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ServiceTypeID"])
                            });
                    }
                }
            }

            return itemsofServiceType;
        }

        public List<ItemTypeTbl> GetAllGroupTypeItems()
        {
            int groupItemTypeId = new SysDataTbl().GetGroupItemTypeID();
            return this.GetAllItemsofServiceType(groupItemTypeId);
        }

        [DataObjectMethod(DataObjectMethodType.Update)]
        public bool UpdateItem(ItemTypeTbl NewItemType)
        {
            return this.UpdateItem(NewItemType, NewItemType.ItemTypeID);
        }

        public bool UpdateItem(ItemTypeTbl NewItemType, int original_ItemTypeID)
        {
            using (var db = new TrackerSQLDb())
            {
                var parameters = new List<DBParameter>
                {
                    new DBParameter { DataValue = NewItemType.SKU, DataDbType = DbType.String, ParamName = "@SKU" },
                    new DBParameter { DataValue = NewItemType.ItemDesc, DataDbType = DbType.String, ParamName = "@ItemDesc" },
                    new DBParameter { DataValue = NewItemType.ItemEnabled, DataDbType = DbType.Boolean, ParamName = "@ItemEnabled" },
                    new DBParameter { DataValue = NewItemType.ItemsCharacteritics, DataDbType = DbType.String, ParamName = "@ItemsCharacteritics" },
                    new DBParameter { DataValue = NewItemType.ItemDetail, DataDbType = DbType.String, ParamName = "@ItemDetail" },
                    new DBParameter { DataValue = NewItemType.ServiceTypeID, DataDbType = DbType.Int32, ParamName = "@ServiceTypeId" },
                    new DBParameter { DataValue = NewItemType.ReplacementID, DataDbType = DbType.Int32, ParamName = "@ReplacementID" },
                    new DBParameter { DataValue = NewItemType.ItemShortName, DataDbType = DbType.String, ParamName = "@ItemShortName" },
                    new DBParameter { DataValue = NewItemType.SortOrder, DataDbType = DbType.Int32, ParamName = "@SortOrder" },
                    new DBParameter { DataValue = NewItemType.ItemUnitID, DataDbType = DbType.Int32, ParamName = "@ItemUnitID" },
                    new DBParameter { DataValue = original_ItemTypeID, DataDbType = DbType.Int32, ParamName = "@ItemTypeID" }
                };

                int result = db.ExecuteNonQuery("UPDATE ItemTypeTbl SET SKU = @SKU, ItemDesc = @ItemDesc, ItemEnabled = @ItemEnabled, ItemsCharacteritics = @ItemsCharacteritics, ItemDetail = @ItemDetail, ServiceTypeId = @ServiceTypeId, ReplacementID = @ReplacementID, ItemShortName = @ItemShortName, SortOrder = @SortOrder, ItemUnitID = @ItemUnitID WHERE ItemTypeID = @ItemTypeID", parameters);
                bool flag = result >= 0;

                if (!flag)
                {
                    AppLogger.WriteLog("ItemTypeTbl", $"Failed to update item ItemTypeID: {original_ItemTypeID}");
                }

                return flag;
            }
        }

        [DataObjectMethod(DataObjectMethodType.Insert)]
        public bool InsertItem(ItemTypeTbl NewItemType)
        {
            using (var db = new TrackerSQLDb())
            {
                var parameters = new List<DBParameter>
                {
                    new DBParameter { DataValue = NewItemType.ItemDesc, DataDbType = DbType.String, ParamName = "@ItemDesc" },
                    new DBParameter { DataValue = NewItemType.SKU, DataDbType = DbType.String, ParamName = "@SKU" },
                    new DBParameter { DataValue = NewItemType.ItemEnabled, DataDbType = DbType.Boolean, ParamName = "@ItemEnabled" },
                    new DBParameter { DataValue = NewItemType.ItemsCharacteritics, DataDbType = DbType.String, ParamName = "@ItemsCharacteritics" },
                    new DBParameter { DataValue = NewItemType.ItemDetail, DataDbType = DbType.String, ParamName = "@ItemDetail" },
                    new DBParameter { DataValue = NewItemType.ServiceTypeID, DataDbType = DbType.Int32, ParamName = "@ServiceTypeId" },
                    new DBParameter { DataValue = NewItemType.ReplacementID, DataDbType = DbType.Int32, ParamName = "@ReplacementID" },
                    new DBParameter { DataValue = NewItemType.ItemShortName, DataDbType = DbType.String, ParamName = "@ItemShortName" },
                    new DBParameter { DataValue = NewItemType.SortOrder, DataDbType = DbType.Int32, ParamName = "@SortOrder" },
                    new DBParameter { DataValue = NewItemType.ItemUnitID, DataDbType = DbType.Int32, ParamName = "@ItemUnitID" }
                };

                int result = db.ExecuteNonQuery("INSERT INTO ItemTypeTbl (ItemDesc, SKU, ItemEnabled, ItemsCharacteritics, ItemDetail, ServiceTypeId, ReplacementID, ItemShortName, SortOrder, ItemUnitID) VALUES (@ItemDesc, @SKU, @ItemEnabled, @ItemsCharacteritics, @ItemDetail, @ServiceTypeId, @ReplacementID, @ItemShortName, @SortOrder, @ItemUnitID)", parameters);
                bool flag = result >= 0;

                if (!flag)
                {
                    AppLogger.WriteLog("ItemTypeTbl", $"Failed to insert item: {NewItemType.ItemDesc}");
                }

                return flag;
            }
        }

        [DataObjectMethod(DataObjectMethodType.Delete)]
        public void DeleteItem(int pItemTypeID)
        {
            using (var db = new TrackerSQLDb())
            {
                var parameters = new List<DBParameter>
                {
                    new DBParameter { DataValue = pItemTypeID, DataDbType = DbType.Int32, ParamName = "@ItemTypeID" }
                };

                int result = db.ExecuteNonQuery("DELETE FROM ItemTypeTbl WHERE ItemTypeID = @ItemTypeID", parameters);

                if (result < 0)
                {
                    AppLogger.WriteLog("ItemTypeTbl", $"Failed to delete item ItemTypeID: {pItemTypeID}");
                }
            }
        }

        public bool GroupOfThisNameExists(string pGroupName)
        {
            bool flag = false;

            using (var db = new TrackerSQLDb())
            {
                var parameters = new List<DBParameter>
                {
                    new DBParameter { DataValue = pGroupName, DataDbType = DbType.String, ParamName = "@ItemDesc" }
                };

                using (IDataReader dataReader = db.ExecuteReader("SELECT ItemDesc FROM ItemTypeTbl WHERE ItemDesc = @ItemDesc", parameters))
                {
                    if (dataReader != null && dataReader.Read())
                        flag = dataReader["ItemDesc"] != DBNull.Value && dataReader["ItemDesc"].Equals((object)pGroupName);
                }
            }

            return flag;
        }
        // Returns the ServiceTypeID for a given item
        public static int GetServiceTypeForItem(int itemId)
        {
            using (var db = new TrackerSQLDb())
            {
                string sql = "SELECT ServiceTypeID FROM ItemTypeTbl WHERE ItemTypeID = @ItemTypeID";
                var parameters = new List<DBParameter>
                {
                    new DBParameter { DataValue = itemId, DataDbType = System.Data.DbType.Int32, ParamName = "@ItemTypeID" }
                };

                using (var reader = db.ExecuteReader(sql, parameters))
                {
                    if (reader != null && reader.Read() && reader["ServiceTypeID"] != DBNull.Value)
                        return System.Convert.ToInt32(reader["ServiceTypeID"]);
                }
            }

            // Fallback to Coffee if not found
            return SystemConstants.ServiceTypeConstants.Coffee;
        }
    }
}
