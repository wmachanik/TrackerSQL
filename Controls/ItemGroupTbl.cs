// Decompiled with JetBrains decompiler
// Type: TrackerSQL.control.ItemGroupTbl
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
    public class ItemGroupTbl
    {
        private const string CONST_SQL_SELECT = "SELECT ItemGroupID, GroupItemTypeID, ItemTypeID, ItemTypeSortPos, Enabled, Notes FROM ItemGroupTbl";
        private const string CONST_SQL_SELECTBYGOUPITEMID = "SELECT ItemGroupID, GroupItemTypeID, ItemTypeID, ItemTypeSortPos, Enabled, Notes FROM ItemGroupTbl WHERE GroupItemTypeID=?";
        private const string CONST_SQL_INSERT = "INSERT INTO ItemGroupTbl (GroupItemTypeID, ItemTypeID, ItemTypeSortPos, Enabled, Notes) VALUES ( ?, ?, ?, ?, ?)";
        private const string CONST_SQL_UPDATE = "UPDATE ItemGroupTbl SET GroupItemTypeID = ?, ItemTypeID = ?, ItemTypeSortPos = ?, Enabled = ?, Notes = ? WHERE (ItemGroupID = ?)";
        private const string CONST_SQL_UPDATEITEMSORTPOS = "UPDATE ItemGroupTbl SET ItemTypeSortPos = ? WHERE (GroupItemTypeID = ?) AND (ItemTypeID = ?) ";
        private const string CONST_SQL_DELETE = "DELETE FROM ItemGroupTbl WHERE (ItemGroupID = ?) ";
        private const string CONST_SQL_DELETEITEMFROMGROUP = "DELETE FROM ItemGroupTbl WHERE (GroupItemTypeID = ?) AND (ItemTypeID = ?) ";
        private const string CONST_SELECT_FIRSTINGROUP = "SELECT TOP 1 ItemGroupID, GroupItemTypeID, ItemTypeID, ItemTypeSortPos, Enabled, Notes FROM ItemGroupTbl WHERE (GroupItemTypeID = ?) ORDER BY ItemTypeSortPos";
        private const string CONST_SELECT_PREVINGROUP = "SELECT TOP 1 ItemGroupID, GroupItemTypeID, ItemTypeID, ItemTypeSortPos, Enabled, Notes FROM ItemGroupTbl WHERE (GroupItemTypeID = ?) AND (ItemTypeSortPos < ?) ORDER BY ItemTypeSortPos DESC";
        private const string CONST_SELECT_NEXTINGROUP = "SELECT TOP 1 ItemGroupID, GroupItemTypeID, ItemTypeID, ItemTypeSortPos, Enabled, Notes FROM ItemGroupTbl WHERE (GroupItemTypeID = ?) AND (ItemTypeSortPos > ?) ORDER BY ItemTypeSortPos";
        private const string CONST_SELECT_LASTINGROUP = "SELECT TOP 1 ItemGroupID, GroupItemTypeID, ItemTypeID, ItemTypeSortPos, Enabled, Notes FROM ItemGroupTbl WHERE (GroupItemTypeID = ?) ORDER BY ItemTypeSortPos DESC";
        private const string CONST_SELECT_SORTPOSINGROUP = "SELECT ItemTypeID FROM ItemGroupTbl WHERE (GroupItemTypeID = ?) AND (ItemTypeSortPos = ?)";
        private int _ItemGroupID;
        private int _GroupItemTypeID;
        private int _ItemTypeID;
        private int _ItemTypeSortPos;
        private bool _Enabled;
        private string _Notes;

        public ItemGroupTbl()
        {
            this._ItemGroupID = 0;
            this._GroupItemTypeID = 0;
            this._ItemTypeID = 0;
            this._ItemTypeSortPos = 0;
            this._Enabled = false;
            this._Notes = string.Empty;
        }

        public int ItemGroupID
        {
            get => this._ItemGroupID;
            set => this._ItemGroupID = value;
        }

        public int GroupItemTypeID
        {
            get => this._GroupItemTypeID;
            set => this._GroupItemTypeID = value;
        }

        public int ItemTypeID
        {
            get => this._ItemTypeID;
            set => this._ItemTypeID = value;
        }

        public int ItemTypeSortPos
        {
            get => this._ItemTypeSortPos;
            set => this._ItemTypeSortPos = value;
        }

        public bool Enabled
        {
            get => this._Enabled;
            set => this._Enabled = value;
        }

        public string Notes
        {
            get => this._Notes;
            set => this._Notes = value;
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public List<ItemGroupTbl> GetAll(string SortBy)
        {
            List<ItemGroupTbl> all = new List<ItemGroupTbl>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = $"SELECT ItemGroupID, GroupItemTypeID, ItemTypeID, ItemTypeSortPos, Enabled, Notes FROM ItemGroupTbl ORDER BY {(string.IsNullOrEmpty(SortBy) ? "ItemTypeSortPos" : SortBy)}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new ItemGroupTbl()
                    {
                        ItemGroupID = dataReader["ItemGroupID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemGroupID"]),
                        GroupItemTypeID = dataReader["GroupItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["GroupItemTypeID"]),
                        ItemTypeID = dataReader["ItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeID"]),
                        ItemTypeSortPos = dataReader["ItemTypeSortPos"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeSortPos"]),
                        Enabled = dataReader["Enabled"] != DBNull.Value && Convert.ToBoolean(dataReader["Enabled"]),
                        Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public List<ItemGroupTbl> GetAllByGroupItemTypeID(int pGroupItemID, string SortBy)
        {
            List<ItemGroupTbl> byGroupItemTypeId = new List<ItemGroupTbl>();
            if (!pGroupItemID.Equals(-1))
            {
                TrackerDb trackerDb = new TrackerDb();
                string strSQL = $"SELECT ItemGroupID, GroupItemTypeID, ItemTypeID, ItemTypeSortPos, Enabled, Notes FROM ItemGroupTbl WHERE GroupItemTypeID=? ORDER BY {(string.IsNullOrEmpty(SortBy) ? "ItemTypeSortPos" : SortBy)}";
                trackerDb.AddWhereParams((object)pGroupItemID, DbType.Int32, "@GroupItemTypeID");
                IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
                if (dataReader != null)
                {
                    while (dataReader.Read())
                        byGroupItemTypeId.Add(new ItemGroupTbl()
                        {
                            ItemGroupID = dataReader["ItemGroupID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemGroupID"]),
                            GroupItemTypeID = dataReader["GroupItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["GroupItemTypeID"]),
                            ItemTypeID = dataReader["ItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeID"]),
                            ItemTypeSortPos = dataReader["ItemTypeSortPos"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeSortPos"]),
                            Enabled = dataReader["Enabled"] != DBNull.Value && Convert.ToBoolean(dataReader["Enabled"]),
                            Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString()
                        });
                    dataReader.Close();
                }
                trackerDb.Close();
            }
            return byGroupItemTypeId;
        }

        [DataObjectMethod(DataObjectMethodType.Insert, true)]
        public string InsertItemGroup(ItemGroupTbl pItemGroupTbl)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pItemGroupTbl.GroupItemTypeID, DbType.Int32, "@GroupItemTypeID");
            trackerDb.AddParams((object)pItemGroupTbl.ItemTypeID, DbType.Int32, "@ItemTypeID");
            trackerDb.AddParams((object)pItemGroupTbl.ItemTypeSortPos, DbType.Int32, "@ItemTypeSortPos");
            trackerDb.AddParams((object)pItemGroupTbl.Enabled, DbType.Boolean, "@Enabled");
            trackerDb.AddParams((object)pItemGroupTbl.Notes, DbType.String, "@Notes");
            string str = trackerDb.ExecuteNonQuerySQL("INSERT INTO ItemGroupTbl (GroupItemTypeID, ItemTypeID, ItemTypeSortPos, Enabled, Notes) VALUES ( ?, ?, ?, ?, ?)");
            trackerDb.Close();
            return str;
        }

        [DataObjectMethod(DataObjectMethodType.Update, true)]
        public string UpdateItemGroup(ItemGroupTbl pItemGroupTbl)
        {
            return this.UpdateItemGroup(pItemGroupTbl, 0);
        }

        [DataObjectMethod(DataObjectMethodType.Update, false)]
        public string UpdateItemGroup(ItemGroupTbl pItemGroupTbl, int pOrignal_ItemGroupID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            if (pOrignal_ItemGroupID > 0)
                trackerDb.AddWhereParams((object)pOrignal_ItemGroupID, DbType.Int32);
            else
                trackerDb.AddWhereParams((object)pItemGroupTbl.ItemGroupID, DbType.Int32, "@ItemGroupID");
            trackerDb.AddParams((object)pItemGroupTbl.GroupItemTypeID, DbType.Int32, "@GroupItemTypeID");
            trackerDb.AddParams((object)pItemGroupTbl.ItemTypeID, DbType.Int32, "@ItemTypeID");
            trackerDb.AddParams((object)pItemGroupTbl.ItemTypeSortPos, DbType.Int32, "@ItemTypeSortPos");
            trackerDb.AddParams((object)pItemGroupTbl.Enabled, DbType.Boolean, "@Enabled");
            trackerDb.AddParams((object)pItemGroupTbl.Notes, DbType.String, "@Notes");
            string str = trackerDb.ExecuteNonQuerySQL("UPDATE ItemGroupTbl SET GroupItemTypeID = ?, ItemTypeID = ?, ItemTypeSortPos = ?, Enabled = ?, Notes = ? WHERE (ItemGroupID = ?)");
            trackerDb.Close();
            return str;
        }

        [DataObjectMethod(DataObjectMethodType.Delete, true)]
        public string DeleteItemGroup(ItemGroupTbl pItemGroupTbl)
        {
            return this.DeleteItemGroup(pItemGroupTbl.ItemGroupID);
        }

        [DataObjectMethod(DataObjectMethodType.Delete, false)]
        public string DeleteItemGroup(int pItemGroupID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pItemGroupID, DbType.Int32, "@ItemGroupID");
            string str = trackerDb.ExecuteNonQuerySQL("DELETE FROM ItemGroupTbl WHERE (ItemGroupID = ?) ");
            trackerDb.Close();
            return str;
        }

        public string DeleteGroupItemFromGroup(int pGroupItemID, int pItemTypeID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pGroupItemID, DbType.Int32, "@GroupItemTypeID");
            trackerDb.AddWhereParams((object)pItemTypeID, DbType.Int32, "@ItemTypeID");
            string str = trackerDb.ExecuteNonQuerySQL("DELETE FROM ItemGroupTbl WHERE (GroupItemTypeID = ?) AND (ItemTypeID = ?) ");
            trackerDb.Close();
            return str;
        }

        public string UpdateItemsSortPos(int pSortPos, int pGroupItemID, int pItemTypeID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pSortPos, DbType.Int32, "@SortPos");
            trackerDb.AddWhereParams((object)pGroupItemID, DbType.Int32, "@GroupItemTypeID");
            trackerDb.AddWhereParams((object)pItemTypeID, DbType.Int32, "@ItemTypeID");
            string str = trackerDb.ExecuteNonQuerySQL("UPDATE ItemGroupTbl SET ItemTypeSortPos = ? WHERE (GroupItemTypeID = ?) AND (ItemTypeID = ?) ");
            trackerDb.Close();
            return str;
        }

        public ItemGroupTbl GetFirstGroupItemType(int pGroupItemTypeID)
        {
            ItemGroupTbl firstGroupItemType = new ItemGroupTbl();
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pGroupItemTypeID, DbType.Int32, "GroupItemTypeID");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT TOP 1 ItemGroupID, GroupItemTypeID, ItemTypeID, ItemTypeSortPos, Enabled, Notes FROM ItemGroupTbl WHERE (GroupItemTypeID = ?) ORDER BY ItemTypeSortPos");
            if (dataReader != null)
            {
                if (dataReader.Read())
                {
                    firstGroupItemType.ItemGroupID = dataReader["ItemGroupID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemGroupID"]);
                    firstGroupItemType.GroupItemTypeID = dataReader["GroupItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["GroupItemTypeID"]);
                    firstGroupItemType.ItemTypeID = dataReader["ItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeID"]);
                    firstGroupItemType.ItemTypeSortPos = dataReader["ItemTypeSortPos"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeSortPos"]);
                    firstGroupItemType.Enabled = dataReader["Enabled"] != DBNull.Value && Convert.ToBoolean(dataReader["Enabled"]);
                    firstGroupItemType.Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString();
                }
                dataReader.Close();
            }
            trackerDb.Close();
            return firstGroupItemType;
        }

        public ItemGroupTbl GetPrevGroupItemType(int pGroupItemTypeID, int pItemTypeSortPos)
        {
            ItemGroupTbl prevGroupItemType = new ItemGroupTbl();
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pGroupItemTypeID, DbType.Int32, "GroupItemTypeID");
            trackerDb.AddWhereParams((object)pItemTypeSortPos, DbType.Int32, "ItemTypeSortPos");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT TOP 1 ItemGroupID, GroupItemTypeID, ItemTypeID, ItemTypeSortPos, Enabled, Notes FROM ItemGroupTbl WHERE (GroupItemTypeID = ?) AND (ItemTypeSortPos < ?) ORDER BY ItemTypeSortPos DESC");
            if (dataReader != null)
            {
                if (dataReader.Read())
                {
                    prevGroupItemType.ItemGroupID = dataReader["ItemGroupID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemGroupID"]);
                    prevGroupItemType.GroupItemTypeID = dataReader["GroupItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["GroupItemTypeID"]);
                    prevGroupItemType.ItemTypeID = dataReader["ItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeID"]);
                    prevGroupItemType.ItemTypeSortPos = dataReader["ItemTypeSortPos"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeSortPos"]);
                    prevGroupItemType.Enabled = dataReader["Enabled"] != DBNull.Value && Convert.ToBoolean(dataReader["Enabled"]);
                    prevGroupItemType.Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString();
                }
                else
                    prevGroupItemType = this.GetLastGroupItem(pGroupItemTypeID);
                dataReader.Close();
            }
            trackerDb.Close();
            return prevGroupItemType;
        }

        public ItemGroupTbl GetNextGroupItemType(int pGroupItemTypeID, int pItemTypeSortPos)
        {
            ItemGroupTbl nextGroupItemType = new ItemGroupTbl();
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pGroupItemTypeID, DbType.Int32, "GroupItemTypeID");
            trackerDb.AddWhereParams((object)pItemTypeSortPos, DbType.Int32, "ItemTypeSortPos");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT TOP 1 ItemGroupID, GroupItemTypeID, ItemTypeID, ItemTypeSortPos, Enabled, Notes FROM ItemGroupTbl WHERE (GroupItemTypeID = ?) AND (ItemTypeSortPos > ?) ORDER BY ItemTypeSortPos");
            if (dataReader != null)
            {
                if (dataReader.Read())
                {
                    nextGroupItemType.ItemGroupID = dataReader["ItemGroupID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemGroupID"]);
                    nextGroupItemType.GroupItemTypeID = dataReader["GroupItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["GroupItemTypeID"]);
                    nextGroupItemType.ItemTypeID = dataReader["ItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeID"]);
                    nextGroupItemType.ItemTypeSortPos = dataReader["ItemTypeSortPos"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeSortPos"]);
                    nextGroupItemType.Enabled = dataReader["Enabled"] != DBNull.Value && Convert.ToBoolean(dataReader["Enabled"]);
                    nextGroupItemType.Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString();
                }
                else
                    nextGroupItemType = this.GetFirstGroupItemType(pGroupItemTypeID);
                dataReader.Close();
            }
            trackerDb.Close();
            return nextGroupItemType;
        }

        public int GetSortPosItemInGroup(int pGroupItemTypeID, int pSortPos)
        {
            int sortPosItemInGroup = -1;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pGroupItemTypeID, DbType.Int32, "GroupItemTypeID");
            trackerDb.AddWhereParams((object)pSortPos, DbType.Int32, "ItemTypeSortPos");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT ItemTypeID FROM ItemGroupTbl WHERE (GroupItemTypeID = ?) AND (ItemTypeSortPos = ?)");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    sortPosItemInGroup = dataReader["ItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeID"]);
                dataReader.Close();
            }
            trackerDb.Close();
            return sortPosItemInGroup;
        }

        public ItemGroupTbl GetLastGroupItem(int pGroupItemTypeID)
        {
            ItemGroupTbl lastGroupItem = new ItemGroupTbl();
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pGroupItemTypeID, DbType.Int32, "GroupItemTypeID");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT TOP 1 ItemGroupID, GroupItemTypeID, ItemTypeID, ItemTypeSortPos, Enabled, Notes FROM ItemGroupTbl WHERE (GroupItemTypeID = ?) ORDER BY ItemTypeSortPos DESC");
            if (dataReader != null)
            {
                if (dataReader.Read())
                {
                    lastGroupItem.ItemGroupID = dataReader["ItemGroupID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemGroupID"]);
                    lastGroupItem.GroupItemTypeID = dataReader["GroupItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["GroupItemTypeID"]);
                    lastGroupItem.ItemTypeID = dataReader["ItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeID"]);
                    lastGroupItem.ItemTypeSortPos = dataReader["ItemTypeSortPos"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeSortPos"]);
                    lastGroupItem.Enabled = dataReader["Enabled"] != DBNull.Value && Convert.ToBoolean(dataReader["Enabled"]);
                    lastGroupItem.Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString();
                }
                dataReader.Close();
            }
            trackerDb.Close();
            return lastGroupItem;
        }

        public int GetLastGroupItemSortPos(int pGroupItemTypeID)
        {
            int groupItemSortPos = -1;
            ItemGroupTbl lastGroupItem = this.GetLastGroupItem(pGroupItemTypeID);
            if (lastGroupItem != null)
                groupItemSortPos = lastGroupItem.ItemTypeSortPos;
            return groupItemSortPos;
        }

        public int IncItemSortPos(ItemGroupTbl pItemGroupTbl)
        {
            pItemGroupTbl.ItemTypeSortPos = this.IncItemSortPos(pItemGroupTbl.GroupItemTypeID, pItemGroupTbl.ItemTypeID, pItemGroupTbl.ItemTypeSortPos);
            return pItemGroupTbl.ItemTypeSortPos;
        }

        public int IncItemSortPos(int pGroupItemTypeID, int pThisItemID, int pSortPos)
        {
            int pSortPos1 = pSortPos;
            int groupItemSortPos = this.GetLastGroupItemSortPos(pGroupItemTypeID);
            if (pSortPos < groupItemSortPos)
            {
                ItemGroupTbl nextGroupItemType = this.GetNextGroupItemType(pGroupItemTypeID, pSortPos);
                if (nextGroupItemType != null)
                {
                    pSortPos1 = nextGroupItemType.ItemTypeSortPos;
                    --nextGroupItemType.ItemTypeSortPos;
                    this.UpdateItemGroup(nextGroupItemType);
                }
                this.UpdateItemsSortPos(pSortPos1, pGroupItemTypeID, pThisItemID);
            }
            return pSortPos1;
        }

        public int DecItemSortPos(ItemGroupTbl pItemGroupTbl)
        {
            pItemGroupTbl.ItemTypeSortPos = this.DecItemSortPos(pItemGroupTbl.GroupItemTypeID, pItemGroupTbl.ItemTypeID, pItemGroupTbl.ItemTypeSortPos);
            return pItemGroupTbl.ItemTypeSortPos;
        }

        public int DecItemSortPos(int pGroupItemTypeID, int pThisItemID, int pSortPos)
        {
            int pSortPos1 = pSortPos;
            if (pSortPos > 1)
            {
                ItemGroupTbl prevGroupItemType = this.GetPrevGroupItemType(pGroupItemTypeID, pSortPos);
                if (prevGroupItemType != null)
                {
                    pSortPos1 = prevGroupItemType.ItemTypeSortPos;
                    ++prevGroupItemType.ItemTypeSortPos;
                    this.UpdateItemGroup(prevGroupItemType);
                }
                this.UpdateItemsSortPos(pSortPos1, pGroupItemTypeID, pThisItemID);
            }
            return pSortPos1;
        }
        // Returns all item IDs in the specified group from the database
        public static HashSet<int> GetItemIdsForGroup(int groupItemTypeId)
        {
            var itemIds = new HashSet<int>();
            using (var db = new TrackerDb())
            {
                string sql = "SELECT ItemTypeID FROM ItemGroupTbl WHERE GroupItemTypeID = ?";
                db.AddWhereParams(groupItemTypeId, System.Data.DbType.Int32);
                using (var reader = db.ExecuteSQLGetDataReader(sql, db.WhereParams))
                {
                    while (reader != null && reader.Read())
                    {
                        if (reader["ItemTypeID"] != DBNull.Value)
                            itemIds.Add(System.Convert.ToInt32(reader["ItemTypeID"]));
                    }
                    reader?.Close();
                }
                db.Close();
            }
            return itemIds;
        }
    }
}


