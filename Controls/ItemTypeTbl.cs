// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.ItemTypeTbl
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using TrackerDotNet.Classes;

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class ItemTypeTbl
    {
        public const int CONST_NEEDDESCRIPTION_SORT_ORDER = 10;
        public const int CONST_SERVICEITEMID = 36;
        private const string CONST_SQL_SELECT = "SELECT ItemTypeID, SKU, ItemDesc, ItemEnabled, ItemsCharacteritics, ItemDetail, ServiceTypeID, ReplacementID, ItemShortName, SortOrder, ItemUnitID FROM ItemTypeTbl";
        private const string CONST_SQL_SELECTITEMDESC = "SELECT ItemDesc, ItemEnabled FROM ItemTypeTbl WHERE ItemTypeID = ?";
        private const string CONST_SQL_SELECTITEMTYPEFROMID = "SELECT SKU, ItemDesc, ItemEnabled, ItemsCharacteritics, ItemDetail, ServiceTypeID, ReplacementID, ItemShortName, SortOrder, ItemUnitID FROM ItemTypeTbl WHERE ItemTypeID = ?";
        private const string CONST_SQL_SELECTITEMSKU = "SELECT SKU FROM ItemTypeTbl WHERE ItemTypeID = ?";
        private const string CONST_SQL_SELECTSERVICETYPEID = "SELECT ServiceTypeID FROM ItemTypeTbl WHERE ItemTypeID = ?";
        private const string CONST_SQL_SELECTITEMTYPES = "SELECT ItemTypeID, IIF(ItemEnabled, ItemDesc, '_' + ItemDesc) AS ItemDesc FROM ItemTypeTbl";
        private const string CONST_SQL_SELECT_ITEMDESCISGROUPNAME = "SELECT ItemDesc FROM ItemTypeTbl WHERE ItemDesc = ?";
        private const string CONST_SQL_SELECTITEMTYPESNOTINITEMGROUP = "SELECT ItemTypeID, ItemDesc, ItemEnabled FROM ItemTypeTbl WHERE (ServiceTypeId = 2) AND (NOT EXISTS (SELECT ItemTypeID FROM ItemGroupTbl WHERE (ItemGroupTbl.ItemTypeID = ItemTypeTbl.ItemTypeID) AND (GroupItemTypeID = ?)))";
        private const string CONST_SQL_ITEMTYPEUNITS = "SELECT ItemUnitsTbl.UnitOfMeasure FROM (ItemUnitsTbl INNER JOIN ItemTypeTbl ON ItemUnitsTbl.ItemUnitID = ItemTypeTbl.ItemUnitID)  WHERE (ItemTypeTbl.ItemTypeID = ?)";
        private const string CONST_SQL_ITEMSORTORDER = "SELECT SortOrder FROM ItemTypeTbl WHERE (ItemTypeTbl.ItemTypeID = ?)";
        private const string CONST_SQL_LISTOFITEMIDSOFSERVICETYPE = "SELECT ItemTypeTbl.ItemTypeID  FROM (ItemTypeTbl LEFT OUTER JOIN ServiceTypesTbl ON ItemTypeTbl.ServiceTypeId = ServiceTypesTbl.ServiceTypeId)  WHERE (ServiceTypesTbl.ServiceTypeId = ?)";
        private const string CONST_SQL_GETALLITEMSOFSERVICETYPE = "SELECT ItemTypeID, ItemDesc, ItemEnabled, ServiceTypeId FROM ItemTypeTbl WHERE (ServiceTypeId = ?) AND (ItemEnabled = ?) ORDER BY ItemDesc";
        private const string CONST_SQL_UPDATE = "UPDATE ItemTypeTbl SET SKU = ? , ItemDesc = ?, ItemEnabled = ?, ItemsCharacteritics = ?, ItemDetail = ?,  ServiceTypeId = ?, ReplacementID = ?, ItemShortName = ?, SortOrder = ?, ItemUnitID = ? WHERE ItemTypeID = ?";
        private const string CONST_SQL_INSERT = "INSERT INTO ItemTypeTbl (ItemDesc, SKU, ItemEnabled, ItemsCharacteritics, ItemDetail, ServiceTypeId, ReplacementID, ItemShortName, SortOrder, ItemUnitID)  VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
        private const string CONST_SQL_DELETEBYID = "DELETE FROM ItemTypeTbl WHERE ItemTypeID = ?";
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
            TrackerDb trackerDb = new TrackerDb();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
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
                dataReader.Close();
            }
            trackerDb.Close();
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
            TrackerDb trackerDb = new TrackerDb();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    allItemDesc.Add(new ItemTypeTbl()
                    {
                        ItemTypeID = dataReader["ItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeID"]),
                        ItemDesc = dataReader["ItemDesc"] == DBNull.Value ? string.Empty : dataReader["ItemDesc"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
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
                string strSQL = "SELECT ItemTypeID, ItemDesc, ItemEnabled FROM ItemTypeTbl WHERE (ServiceTypeId = 2) AND (NOT EXISTS (SELECT ItemTypeID FROM ItemGroupTbl WHERE (ItemGroupTbl.ItemTypeID = ItemTypeTbl.ItemTypeID) AND (GroupItemTypeID = ?)))" + (!string.IsNullOrEmpty(SortBy) ? " ORDER BY " + SortBy : " ORDER BY ItemEnabled, SortOrder, ItemDesc");
                TrackerDb trackerDb = new TrackerDb();
                trackerDb.AddWhereParams((object)pGroupItemTypeID, DbType.Int32, "@GroupItemTypeID");
                IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
                if (dataReader != null)
                {
                    while (dataReader.Read())
                        itemsNotInItemGroup.Add(new ItemTypeTbl()
                        {
                            ItemTypeID = dataReader["ItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeID"]),
                            ItemDesc = dataReader["ItemDesc"] == DBNull.Value ? string.Empty : dataReader["ItemDesc"].ToString(),
                            ItemEnabled = dataReader["ItemEnabled"] != DBNull.Value && Convert.ToBoolean(dataReader["ItemEnabled"])
                        });
                    dataReader.Close();
                }
                trackerDb.Close();
            }
            return itemsNotInItemGroup;
        }
        // Static cache: ItemTypeID -> (ItemDesc, ItemEnabled)
        [DataObjectMethod(DataObjectMethodType.Select)]
        public static string GetItemTypeDescById(int pItemID, bool pCheckIfSoldOut = true)
        {
            string itemTypeDesc = string.Empty;
            bool enabled = true;
            using (var trackerDb = new TrackerDb())
            {
                trackerDb.AddWhereParams(pItemID, DbType.Int32, "@ItemTypeID");
                using (var dataReader = trackerDb.ExecuteSQLGetDataReader(CONST_SQL_SELECTITEMDESC))  // "SELECT ItemDesc, ItemEnabled FROM ItemTypeTbl WHERE ItemTypeID = ?"))
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
        //    TrackerDb trackerDb = new TrackerDb();
        //    trackerDb.AddWhereParams((object)pItemID, DbType.Int32, "@ItemTypeID");
        //    IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(CONST_SQL_SELECTITEMDESC); //   "SELECT ItemDesc, ItemEnabled FROM ItemTypeTbl WHERE ItemTypeID = ?");
        //    if (dataReader != null)
        //    {
        //        if (dataReader.Read())
        //        {
        //            itemTypeDesc = dataReader["ItemDesc"] == DBNull.Value ? string.Empty : dataReader["ItemDesc"].ToString();
        //            if (pCheckIfSoldOut && (dataReader["ItemEnabled"] == DBNull.Value || !Convert.ToBoolean(dataReader["ItemEnabled"])))
        //                itemTypeDesc += " SOLD OUT";
        //        }
        //        dataReader.Close();
        //    }
        //    trackerDb.Close();
        //    return itemTypeDesc;
        //}

        public ItemTypeTbl GetItemTypeFromID(int pItemTypeID)
        {
            ItemTypeTbl itemTypeFromId = new ItemTypeTbl();
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pItemTypeID, DbType.Int32, "@ItemTypeID");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT SKU, ItemDesc, ItemEnabled, ItemsCharacteritics, ItemDetail, ServiceTypeID, ReplacementID, ItemShortName, SortOrder, ItemUnitID FROM ItemTypeTbl WHERE ItemTypeID = ?");
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
            dataReader.Close();
            trackerDb.Close();
            return itemTypeFromId;
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public string GetItemTypeSKU(int pItemID)
        {
            string itemTypeSku = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pItemID, DbType.Int32, "@ItemTypeID");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT SKU FROM ItemTypeTbl WHERE ItemTypeID = ?");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    itemTypeSku = dataReader["SKU"] == DBNull.Value ? string.Empty : dataReader["SKU"].ToString();
                dataReader.Close();
            }
            trackerDb.Close();
            return itemTypeSku;
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public int GetServiceID(int pItemID)
        {
            int serviceId = 0;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pItemID, DbType.Int32, "@ItemTypeID");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT ServiceTypeID FROM ItemTypeTbl WHERE ItemTypeID = ?");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    serviceId = dataReader["ServiceTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ServiceTypeID"]);
                dataReader.Close();
            }
            trackerDb.Close();
            return serviceId;
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public string GetItemUnitOfMeasure(int pItemID)
        {
            string itemUnitOfMeasure = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pItemID, DbType.Int32);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT ItemUnitsTbl.UnitOfMeasure FROM (ItemUnitsTbl INNER JOIN ItemTypeTbl ON ItemUnitsTbl.ItemUnitID = ItemTypeTbl.ItemUnitID)  WHERE (ItemTypeTbl.ItemTypeID = ?)");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    itemUnitOfMeasure = dataReader["UnitOfMeasure"] == DBNull.Value ? string.Empty : dataReader["UnitOfMeasure"].ToString();
                dataReader.Dispose();
            }
            trackerDb.Close();
            return itemUnitOfMeasure;
        }

        public int GetItemSortOrder(int pItemID)
        {
            int itemSortOrder = 0;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pItemID, DbType.Int32);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT SortOrder FROM ItemTypeTbl WHERE (ItemTypeTbl.ItemTypeID = ?)");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    itemSortOrder = dataReader["SortOrder"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["SortOrder"]);
                dataReader.Dispose();
            }
            trackerDb.Close();
            return itemSortOrder;
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<int> GetAllItemIDsofServiceType(int pServiceTypeID)
        {
            List<int> idsofServiceType = new List<int>();
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pServiceTypeID, DbType.Int32);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT ItemTypeTbl.ItemTypeID  FROM (ItemTypeTbl LEFT OUTER JOIN ServiceTypesTbl ON ItemTypeTbl.ServiceTypeId = ServiceTypesTbl.ServiceTypeId)  WHERE (ServiceTypesTbl.ServiceTypeId = ?)");
            if (dataReader != null)
            {
                while (dataReader.Read())
                    idsofServiceType.Add((int)dataReader["ItemTypeID"]);
                dataReader.Dispose();
            }
            trackerDb.Close();
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
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pServiceTypeID, DbType.Int32);
            trackerDb.AddWhereParams((object)pEnabled, DbType.Boolean);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT ItemTypeID, ItemDesc, ItemEnabled, ServiceTypeId FROM ItemTypeTbl WHERE (ServiceTypeId = ?) AND (ItemEnabled = ?) ORDER BY ItemDesc");
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
                dataReader.Dispose();
            }
            trackerDb.Close();
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
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)NewItemType.SKU, DbType.String);
            trackerDb.AddParams((object)NewItemType.ItemDesc, DbType.String);
            trackerDb.AddParams((object)NewItemType.ItemEnabled, DbType.Boolean);
            trackerDb.AddParams((object)NewItemType.ItemsCharacteritics, DbType.String);
            trackerDb.AddParams((object)NewItemType.ItemDetail, DbType.String);
            trackerDb.AddParams((object)NewItemType.ServiceTypeID, DbType.Int32);
            trackerDb.AddParams((object)NewItemType.ReplacementID, DbType.Int32);
            trackerDb.AddParams((object)NewItemType.ItemShortName, DbType.String);
            trackerDb.AddParams((object)NewItemType.SortOrder, DbType.Int32);
            trackerDb.AddParams((object)NewItemType.ItemUnitID, DbType.Int32);
            trackerDb.AddWhereParams((object)original_ItemTypeID, DbType.Int32);
            bool flag = trackerDb.ExecuteNonQuerySQL("UPDATE ItemTypeTbl SET SKU = ? , ItemDesc = ?, ItemEnabled = ?, ItemsCharacteritics = ?, ItemDetail = ?,  ServiceTypeId = ?, ReplacementID = ?, ItemShortName = ?, SortOrder = ?, ItemUnitID = ? WHERE ItemTypeID = ?") == string.Empty;
            trackerDb.Close();
            return flag;
        }

        [DataObjectMethod(DataObjectMethodType.Insert)]
        public bool InsertItem(ItemTypeTbl NewItemType)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)NewItemType.ItemDesc, DbType.String, "@ItemDesc");
            trackerDb.AddParams((object)NewItemType.SKU, DbType.String, "@SKU");
            trackerDb.AddParams((object)NewItemType.ItemEnabled, DbType.Boolean, "@ItemEnabled");
            trackerDb.AddParams((object)NewItemType.ItemsCharacteritics, DbType.String, "@ItemsCharacteritics");
            trackerDb.AddParams((object)NewItemType.ItemDetail, DbType.String, "@ItemDetail");
            trackerDb.AddParams((object)NewItemType.ServiceTypeID, DbType.Int32, "@ServiceTypeID");
            trackerDb.AddParams((object)NewItemType.ReplacementID, DbType.Int32, "@ReplacementID");
            trackerDb.AddParams((object)NewItemType.ItemShortName, DbType.String, "@ItemShortName");
            trackerDb.AddParams((object)NewItemType.SortOrder, DbType.Int32, "@SortOrder");
            trackerDb.AddParams((object)NewItemType.ItemUnitID, DbType.Int32, "@ItemUnitID");
            bool flag = trackerDb.ExecuteNonQuerySQL("INSERT INTO ItemTypeTbl (ItemDesc, SKU, ItemEnabled, ItemsCharacteritics, ItemDetail, ServiceTypeId, ReplacementID, ItemShortName, SortOrder, ItemUnitID)  VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)") == string.Empty;
            trackerDb.Close();
            return flag;
        }

        [DataObjectMethod(DataObjectMethodType.Delete)]
        public void DeleteItem(int pItemTypeID)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pItemTypeID, DbType.Int32);
            trackerDb.ExecuteNonQuerySQL("DELETE FROM ItemTypeTbl WHERE ItemTypeID = ?");
            trackerDb.Close();
        }

        public bool GroupOfThisNameExists(string pGroupName)
        {
            bool flag = false;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pGroupName, DbType.String, "@ItemDesc");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT ItemDesc FROM ItemTypeTbl WHERE ItemDesc = ?");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    flag = dataReader["ItemDesc"] != DBNull.Value && dataReader["ItemDesc"].Equals((object)pGroupName);
                dataReader.Dispose();
            }
            trackerDb.Close();
            return flag;
        }
        // Returns the ServiceTypeID for a given item
        public static int GetServiceTypeForItem(int itemId)
        {
            using (var db = new TrackerDb())
            {
                string sql = "SELECT ServiceTypeID FROM ItemTypeTbl WHERE ItemTypeID = ?";
                db.AddWhereParams(itemId, System.Data.DbType.Int32);
                using (var reader = db.ExecuteSQLGetDataReader(sql, db.WhereParams))
                {
                    if (reader != null && reader.Read() && reader["ServiceTypeID"] != DBNull.Value)
                        return System.Convert.ToInt32(reader["ServiceTypeID"]);
                    reader?.Close();
                }
                db.Close();
            }
            // Fallback to Coffee if not found
            return SystemConstants.ServiceTypeConstants.Coffee;
        }
    }
}
