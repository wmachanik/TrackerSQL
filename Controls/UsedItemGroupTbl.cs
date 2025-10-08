// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.UsedItemGroupTbl
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
    public class UsedItemGroupTbl
    {
        private const string CONST_SQL_SELECT = "SELECT UsedItemGroupID, ContactID, GroupItemTypeID, LastItemTypeID, LastItemTypeSortPos, LastItemDateChanged, Notes FROM UsedItemGroupTbl";
        private const string CONST_SQL_INSERT = "INSERT INTO UsedItemGroupTbl (ContactID, GroupItemTypeID, LastItemTypeID, LastItemTypeSortPos, LastItemDateChanged, Notes) VALUES ( ?, ?, ?, ?, ?, ?)";
        private const string CONST_SQL_UPDATE = "UPDATE UsedItemGroupTbl SET ContactID = ?, GroupItemTypeID = ?, LastItemTypeID = ?, LastItemTypeSortPos = ?, LastItemDateChanged = ?, Notes = ? WHERE (UsedItemGroupID = ?)";
        private const string CONST_SQL_DELETE = "DELETE FROM UsedItemGroupTbl WHERE (UsedItemGroupID = ?)";
        private const string CONST_SQL_GETGROUPITEMTYPED = "SELECT UsedItemGroupID, LastItemTypeID, LastItemTypeSortPos, LastItemDateChanged, Notes FROM UsedItemGroupTbl WHERE(ContactID = ?) AND (GroupItemTypeID = ?)";
        private const string CONST_SELECTLASTUSEDITEMID = "SELECT UsedItemGroupID, GroupItemTypeID, LastItemTypeSortPos, Notes FROM UsedItemGroupTbl WHERE ContactID = ? AND LastItemTypeID = ? AND LastItemDateChanged = ?";
        private int _UsedItemGroupID;
        private long _ContactID;
        private int _GroupItemTypeID;
        private int _LastItemTypeID;
        private int _LastItemTypeSortPos;
        private DateTime _LastItemDateChanged;
        private string _Notes;

        public UsedItemGroupTbl()
        {
            this._UsedItemGroupID = SystemConstants.DatabaseConstants.InvalidID;
            this._ContactID = SystemConstants.DatabaseConstants.InvalidID;
            this._GroupItemTypeID = SystemConstants.DatabaseConstants.InvalidID;
            this._LastItemTypeID = SystemConstants.DatabaseConstants.InvalidID;
            this._LastItemTypeSortPos = 0;
            this._LastItemDateChanged = TimeZoneUtils.Now().Date;
            this._Notes = string.Empty;
        }

        public int UsedItemGroupID
        {
            get => this._UsedItemGroupID;
            set => this._UsedItemGroupID = value;
        }

        public long ContactID
        {
            get => this._ContactID;
            set => this._ContactID = value;
        }

        public int GroupItemTypeID
        {
            get => this._GroupItemTypeID;
            set => this._GroupItemTypeID = value;
        }

        public int LastItemTypeID
        {
            get => this._LastItemTypeID;
            set => this._LastItemTypeID = value;
        }

        public int LastItemTypeSortPos
        {
            get => this._LastItemTypeSortPos;
            set => this._LastItemTypeSortPos = value;
        }

        public DateTime LastItemDateChanged
        {
            get => this._LastItemDateChanged;
            set => this._LastItemDateChanged = value;
        }

        public string Notes
        {
            get => this._Notes;
            set => this._Notes = value;
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public List<UsedItemGroupTbl> GetAll(string SortBy)
        {
            List<UsedItemGroupTbl> all = new List<UsedItemGroupTbl>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT UsedItemGroupID, ContactID, GroupItemTypeID, LastItemTypeID, LastItemTypeSortPos, LastItemDateChanged, Notes FROM UsedItemGroupTbl";
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new UsedItemGroupTbl()
                    {
                        UsedItemGroupID = dataReader["UsedItemGroupID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["UsedItemGroupID"]),
                        ContactID = dataReader["ContactID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ContactID"]),
                        GroupItemTypeID = dataReader["GroupItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["GroupItemTypeID"]),
                        LastItemTypeID = dataReader["LastItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["LastItemTypeID"]),
                        LastItemTypeSortPos = dataReader["LastItemTypeSortPos"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["LastItemTypeSortPos"]),
                        LastItemDateChanged = dataReader["LastItemDateChanged"] == DBNull.Value ? TimeZoneUtils.Now() : Convert.ToDateTime(dataReader["LastItemDateChanged"]),
                        Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }

        [DataObjectMethod(DataObjectMethodType.Insert, true)]
        public string Insert(UsedItemGroupTbl pUsedItemGroupTbl)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pUsedItemGroupTbl.ContactID, DbType.Int64, "@ContactID");
            trackerDb.AddParams((object)pUsedItemGroupTbl.GroupItemTypeID, DbType.Int32, "@GroupItemTypeID");
            trackerDb.AddParams((object)pUsedItemGroupTbl.LastItemTypeID, DbType.Int32, "@LastItemTypeID");
            trackerDb.AddParams((object)pUsedItemGroupTbl.LastItemTypeSortPos, DbType.Int32, "@LastItemTypeSortPos");
            trackerDb.AddParams((object)pUsedItemGroupTbl.LastItemDateChanged, DbType.Date, "@LastItemDateChanged");
            trackerDb.AddParams((object)pUsedItemGroupTbl.Notes, DbType.String, "@Notes");
            string str = trackerDb.ExecuteNonQuerySQL("INSERT INTO UsedItemGroupTbl (ContactID, GroupItemTypeID, LastItemTypeID, LastItemTypeSortPos, LastItemDateChanged, Notes) VALUES ( ?, ?, ?, ?, ?, ?)");
            trackerDb.Close();
            return str;
        }

        [DataObjectMethod(DataObjectMethodType.Update, true)]
        public string Update(UsedItemGroupTbl pUsedItemGroupTbl) => this.Update(pUsedItemGroupTbl, 0);

        [DataObjectMethod(DataObjectMethodType.Update, false)]
        public string Update(UsedItemGroupTbl pUsedItemGroupTbl, int pOrignal_UsedItemGroupID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            if (pOrignal_UsedItemGroupID > 0)
                trackerDb.AddWhereParams((object)pOrignal_UsedItemGroupID, DbType.Int32);
            else
                trackerDb.AddWhereParams((object)pUsedItemGroupTbl.UsedItemGroupID, DbType.Int32, "@UsedItemGroupID");
            trackerDb.AddParams((object)pUsedItemGroupTbl.ContactID, DbType.Int64, "@ContactID");
            trackerDb.AddParams((object)pUsedItemGroupTbl.GroupItemTypeID, DbType.Int32, "@GroupItemTypeID");
            trackerDb.AddParams((object)pUsedItemGroupTbl.LastItemTypeID, DbType.Int32, "@LastItemTypeID");
            trackerDb.AddParams((object)pUsedItemGroupTbl.LastItemTypeSortPos, DbType.Int32, "@LastItemTypeSortPos");
            trackerDb.AddParams((object)pUsedItemGroupTbl.LastItemDateChanged, DbType.Date, "@LastItemDateChanged");
            trackerDb.AddParams((object)pUsedItemGroupTbl.Notes, DbType.String, "@Notes");
            string str = trackerDb.ExecuteNonQuerySQL("UPDATE UsedItemGroupTbl SET ContactID = ?, GroupItemTypeID = ?, LastItemTypeID = ?, LastItemTypeSortPos = ?, LastItemDateChanged = ?, Notes = ? WHERE (UsedItemGroupID = ?)");
            trackerDb.Close();
            return str;
        }

        [DataObjectMethod(DataObjectMethodType.Delete, true)]
        public string Delete(UsedItemGroupTbl pUsedItemGroupTbl)
        {
            return this.Delete(pUsedItemGroupTbl.UsedItemGroupID);
        }

        [DataObjectMethod(DataObjectMethodType.Delete, false)]
        public string Delete(int pUsedItemGroupID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pUsedItemGroupID, DbType.Int32, "@UsedItemGroupID");
            string str = trackerDb.ExecuteNonQuerySQL("DELETE FROM UsedItemGroupTbl WHERE (UsedItemGroupID = ?)");
            trackerDb.Close();
            return str;
        }

        public UsedItemGroupTbl ContactLastGroupItem(long pContactID, int pGroupItemTypeID)
        {
            UsedItemGroupTbl usedItemGroupTbl = new UsedItemGroupTbl();
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pContactID, DbType.Int64, "@ContactID");
            trackerDb.AddWhereParams((object)pGroupItemTypeID, DbType.Int32, "@GroupItemTypeID");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT UsedItemGroupID, LastItemTypeID, LastItemTypeSortPos, LastItemDateChanged, Notes FROM UsedItemGroupTbl WHERE(ContactID = ?) AND (GroupItemTypeID = ?)");
            if (dataReader != null)
            {
                if (dataReader.Read())
                {
                    usedItemGroupTbl.UsedItemGroupID = dataReader["UsedItemGroupID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["UsedItemGroupID"]);
                    usedItemGroupTbl.ContactID = pContactID;
                    usedItemGroupTbl.GroupItemTypeID = pGroupItemTypeID;
                    usedItemGroupTbl.LastItemTypeID = dataReader["LastItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["LastItemTypeID"]);
                    usedItemGroupTbl.LastItemTypeSortPos = dataReader["LastItemTypeSortPos"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["LastItemTypeSortPos"]);
                    usedItemGroupTbl.LastItemDateChanged = dataReader["LastItemDateChanged"] == DBNull.Value ? TimeZoneUtils.Now() : Convert.ToDateTime(dataReader["LastItemDateChanged"]);
                    usedItemGroupTbl.Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString();
                }
                dataReader.Close();
            }
            trackerDb.Close();
            return usedItemGroupTbl;
        }

        public ItemGroupTbl GetNextGroupItem(
          long pContactID,
          int pGroupItemTypeID,
          DateTime pDeliveryDate)
        {
            ItemGroupTbl itemGroupTbl = new ItemGroupTbl();
            UsedItemGroupTbl usedItemGroupTbl = new UsedItemGroupTbl();
            UsedItemGroupTbl pUsedItemGroupTbl = this.ContactLastGroupItem(pContactID, pGroupItemTypeID);
            ItemGroupTbl nextGroupItem;
            if (pUsedItemGroupTbl.UsedItemGroupID == SystemConstants.DatabaseConstants.InvalidID)
            {
                nextGroupItem = itemGroupTbl.GetFirstGroupItemType(pGroupItemTypeID);
                pUsedItemGroupTbl.ContactID = pContactID;
                pUsedItemGroupTbl.GroupItemTypeID = pGroupItemTypeID;
                pUsedItemGroupTbl.LastItemTypeID = nextGroupItem.ItemTypeID;
                pUsedItemGroupTbl.LastItemTypeSortPos = nextGroupItem.ItemTypeSortPos;
                pUsedItemGroupTbl.LastItemDateChanged = pDeliveryDate;
                this.Insert(pUsedItemGroupTbl);
            }
            else
            {
                nextGroupItem = itemGroupTbl.GetNextGroupItemType(pGroupItemTypeID, pUsedItemGroupTbl.LastItemTypeSortPos);
                pUsedItemGroupTbl.LastItemTypeID = nextGroupItem.ItemTypeID;
                pUsedItemGroupTbl.LastItemTypeSortPos = nextGroupItem.ItemTypeSortPos;
                pUsedItemGroupTbl.LastItemDateChanged = pDeliveryDate;
                this.Update(pUsedItemGroupTbl);
            }
            return nextGroupItem;
        }

        public bool UpdateIfGroupItem(
          long pContactID,
          int pItemTypeID,
          DateTime pOldDeliveryDate,
          DateTime pNewDeliveryDate)
        {
            bool flag = false;
            if (!pNewDeliveryDate.Equals(pOldDeliveryDate))
            {
                UsedItemGroupTbl usedItemGroupTbl = new UsedItemGroupTbl();
                UsedItemGroupTbl lastUsedItemId = this.GetLastUsedItemID(pContactID, pItemTypeID, pOldDeliveryDate);
                if (lastUsedItemId.UsedItemGroupID != SystemConstants.DatabaseConstants.InvalidID)
                {
                    lastUsedItemId.LastItemDateChanged = pNewDeliveryDate;
                    flag = this.Update(lastUsedItemId) == string.Empty;
                }
            }
            return flag;
        }

        public UsedItemGroupTbl GetLastUsedItemID(long pContactID, int pItemID, DateTime pDeliveryDate)
        {
            UsedItemGroupTbl lastUsedItemId = new UsedItemGroupTbl();
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pContactID, DbType.Int64, "@ContactID");
            trackerDb.AddWhereParams((object)pItemID, DbType.Int32, "@LastItemTypeID");
            trackerDb.AddWhereParams((object)pDeliveryDate, DbType.Date, "@LastItemDateChanged ");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT UsedItemGroupID, GroupItemTypeID, LastItemTypeSortPos, Notes FROM UsedItemGroupTbl WHERE ContactID = ? AND LastItemTypeID = ? AND LastItemDateChanged = ?");
            if (dataReader != null)
            {
                if (dataReader.Read())
                {
                    lastUsedItemId.UsedItemGroupID = dataReader["UsedItemGroupID"] == DBNull.Value ? SystemConstants.DatabaseConstants.InvalidID : Convert.ToInt32(dataReader["UsedItemGroupID"]);
                    lastUsedItemId.ContactID = pContactID;
                    lastUsedItemId.GroupItemTypeID = dataReader["GroupItemTypeID"] == DBNull.Value ? SystemConstants.DatabaseConstants.InvalidID : Convert.ToInt32(dataReader["GroupItemTypeID"]);
                    lastUsedItemId.LastItemTypeID = pItemID;
                    lastUsedItemId.LastItemTypeSortPos = dataReader["LastItemTypeSortPos"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["LastItemTypeSortPos"]);
                    lastUsedItemId.LastItemDateChanged = pDeliveryDate;
                    lastUsedItemId.Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString();
                }
                dataReader.Close();
            }
            trackerDb.Close();
            return lastUsedItemId;
        }

        public int ChangeItemIDToGroupIfItWas(long pContactID, int pItemID, DateTime pDeliveryDate)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pContactID, DbType.Int64, "@ContactID");
            trackerDb.AddWhereParams((object)pItemID, DbType.Int32, "@LastItemTypeID");
            trackerDb.AddWhereParams((object)pDeliveryDate, DbType.Date, "@LastItemDateChanged ");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT UsedItemGroupID, GroupItemTypeID, LastItemTypeSortPos, Notes FROM UsedItemGroupTbl WHERE ContactID = ? AND LastItemTypeID = ? AND LastItemDateChanged = ?");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    pItemID = dataReader["GroupItemTypeID"] == DBNull.Value ? SystemConstants.DatabaseConstants.InvalidID : Convert.ToInt32(dataReader["GroupItemTypeID"]);
                dataReader.Close();
            }
            trackerDb.Close();
            return pItemID;
        }
    }
}