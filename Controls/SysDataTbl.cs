//------------------------------------------------------------------------------
// TrackerSQL v3.x — SysDataTbl
// Data access / control type: SysDataTbl.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using TrackerSQL.Classes;

//- only form later versions #nullable disable
namespace TrackerSQL.Controls
{
    public class SysDataTbl
    {
        private const string CONST_SQL_SELECT = "SELECT ID, LastRecurringDate, DoReoccuringOrders, DateLastPrepDateCalcd, MinReminderDate, GroupItemTypeID, InternalCustomerIds FROM SysDataTbl WHERE ID = 1";
        private const string CONST_SQL_SELECTMINREMINDERDATE = "SELECT MinReminderDate FROM SysDataTbl WHERE ID = 1";
        private const string CONST_SQL_SELECTGROUPSERVICETYPEID = "SELECT GroupItemTypeID FROM SysDataTbl WHERE ID = 1";
        private const string CONST_SQL_SELECTINTERNALCUSTOMERIDS = "SELECT InternalCustomerIds FROM SysDataTbl WHERE ID = 1";
        private const string CONST_SQL_UPDATEBYID = "UPDATE SysDataTbl SET LastRecurringDate = ?, DoReoccuringOrders = ?, DateLastPrepDateCalcd = ?, MinReminderDate = ?, GroupItemTypeID = ?, InternalCustomerIds = ? WHERE (SysDataTbl.ID = ?)";


        private int _ID;
        private DateTime _LastRecurringDate;
        private bool _DoReoccuringOrders;
        private DateTime _DateLastPrepDateCalcd;
        private DateTime _MinReminderDate;
        private int _GroupItemTypeID;
        private string _InternalCustomerIds; // ADD this line

        public SysDataTbl()
        {
            this._ID = 0;
            this._LastRecurringDate = TimeZoneUtils.Now().Date;
            this._DoReoccuringOrders = false;
            this._DateLastPrepDateCalcd = TimeZoneUtils.Now().Date;
            this._MinReminderDate = TimeZoneUtils.Now().Date;
            this._GroupItemTypeID = 0;
            this._InternalCustomerIds = string.Empty; // ADD this line
        }

        public int ID
        {
            get => this._ID;
            set => this._ID = value;
        }

        public DateTime LastRecurringDate
        {
            get => this._LastRecurringDate;
            set => this._LastRecurringDate = value;
        }

        public bool DoReoccuringOrders
        {
            get => this._DoReoccuringOrders;
            set => this._DoReoccuringOrders = value;
        }

        public DateTime DateLastPrepDateCalcd
        {
            get => this._DateLastPrepDateCalcd;
            set => this._DateLastPrepDateCalcd = value;
        }

        public DateTime MinReminderDate
        {
            get => this._MinReminderDate;
            set => this._MinReminderDate = value;
        }

        public int GroupItemTypeID
        {
            get => this._GroupItemTypeID;
            set => this._GroupItemTypeID = value;
        }

        // ADD this property
        public string InternalCustomerIds
        {
            get => this._InternalCustomerIds;
            set => this._InternalCustomerIds = value;
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public List<SysDataTbl> GetAll()
        {
            List<SysDataTbl> all = new List<SysDataTbl>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = CONST_SQL_SELECT; // This now includes InternalCustomerIds
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new SysDataTbl()
                    {
                        ID = dataReader["ID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ID"]),
                        LastRecurringDate = dataReader["LastRecurringDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["LastRecurringDate"]).Date,
                        DoReoccuringOrders = dataReader["DoReoccuringOrders"] != DBNull.Value && Convert.ToBoolean(dataReader["DoReoccuringOrders"]),
                        DateLastPrepDateCalcd = dataReader["DateLastPrepDateCalcd"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["DateLastPrepDateCalcd"]).Date,
                        MinReminderDate = dataReader["MinReminderDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["MinReminderDate"]).Date,
                        GroupItemTypeID = dataReader["GroupItemTypeID"] == DBNull.Value ? -1 : Convert.ToInt32(dataReader["GroupItemTypeID"]),
                        InternalCustomerIds = dataReader["InternalCustomerIds"] == DBNull.Value ? string.Empty : dataReader["InternalCustomerIds"].ToString() // ADD this line
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }

        public DateTime GetMinReminderDate()
        {
            DateTime minReminderDate = DateTime.MinValue;
            TrackerDb trackerDb = new TrackerDb();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT MinReminderDate FROM SysDataTbl WHERE ID = 1");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    minReminderDate = dataReader["MinReminderDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["MinReminderDate"]).Date;
                dataReader.Close();
            }
            trackerDb.Close();
            return minReminderDate;
        }
        // ADD this new method
        public List<int> GetInternalCustomerIdsList()
        {
            var customerIds = new List<int>();

            try
            {
                TrackerDb trackerDb = new TrackerDb();
                IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(CONST_SQL_SELECTINTERNALCUSTOMERIDS);

                if (dataReader != null && dataReader.Read())
                {
                    string idsString = dataReader["InternalCustomerIds"] == DBNull.Value ?
                        string.Empty : dataReader["InternalCustomerIds"].ToString();

                    if (!string.IsNullOrWhiteSpace(idsString))
                    {
                        var idStrings = idsString.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var idString in idStrings)
                        {
                            if (int.TryParse(idString.Trim(), out int customerId))
                            {
                                customerIds.Add(customerId);
                            }
                        }
                    }
                    dataReader.Close();
                }
                trackerDb.Close();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"Error getting internal customer IDs: {ex.Message}");
            }

            return customerIds;
        }
        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public string Update(SysDataTbl SysDataItem) => this.Update(SysDataItem, SysDataItem.ID);

        public string Update(SysDataTbl SysDataItem, int orig_ID)
        {
            string str = string.Empty;
            if (orig_ID > 0)
            {
                TrackerDb trackerDb = new TrackerDb();
                trackerDb.AddParams((object)SysDataItem.LastRecurringDate, DbType.Date);
                trackerDb.AddParams((object)SysDataItem.DoReoccuringOrders, DbType.Boolean);
                trackerDb.AddParams((object)SysDataItem.DateLastPrepDateCalcd, DbType.Date);
                trackerDb.AddParams((object)SysDataItem.MinReminderDate, DbType.Date);
                trackerDb.AddParams((object)SysDataItem.GroupItemTypeID, DbType.Int32, "@GroupItemTypeID");
                trackerDb.AddWhereParams((object)orig_ID, DbType.Int32);
                str = trackerDb.ExecuteNonQuerySQL("UPDATE SysDataTbl SET LastRecurringDate = ?, DoReoccuringOrders = ?, DateLastPrepDateCalcd = ?, MinReminderDate = ?, GroupItemTypeID = ?WHERE (SysDataTbl.ID = ?)");
                trackerDb.Close();
            }
            return str;
        }

        public int GetGroupItemTypeID()
        {
            int groupItemTypeId = 0;
            TrackerDb trackerDb = new TrackerDb();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT GroupItemTypeID FROM SysDataTbl WHERE ID = 1");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    groupItemTypeId = dataReader["GroupItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["GroupItemTypeID"]);
                dataReader.Close();
            }
            trackerDb.Close();
            return groupItemTypeId;
        }
    }
}
