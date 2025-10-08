// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.RepairFaultsTbl
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
    public class RepairFaultsTbl
    {
        private const string CONST_SQL_SELECT = "SELECT RepairFaultID, RepairFaultDesc, SortOrder, Notes FROM RepairFaultsTbl";
        private const string CONST_SQL_SELECTFAULTDESC = "SELECT RepairFaultDesc FROM RepairFaultsTbl WHERE (RepairFaultID = ?)";
        private int _RepairFaultID;
        private string _RepairFaultDesc;
        private int _SortOrder;
        private string _Notes;

        public RepairFaultsTbl()
        {
            this._RepairFaultID = 0;
            this._RepairFaultDesc = string.Empty;
            this._SortOrder = 0;
            this._Notes = string.Empty;
        }

        public int RepairFaultID
        {
            get => this._RepairFaultID;
            set => this._RepairFaultID = value;
        }

        public string RepairFaultDesc
        {
            get => this._RepairFaultDesc;
            set => this._RepairFaultDesc = value;
        }

        public int SortOrder
        {
            get => this._SortOrder;
            set => this._SortOrder = value;
        }

        public string Notes
        {
            get => this._Notes;
            set => this._Notes = value;
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public List<RepairFaultsTbl> GetAll(string SortBy)
        {
            List<RepairFaultsTbl> all = new List<RepairFaultsTbl>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = $"SELECT RepairFaultID, RepairFaultDesc, SortOrder, Notes FROM RepairFaultsTbl ORDER BY {(string.IsNullOrEmpty(SortBy) ? "SortOrder" : SortBy)}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new RepairFaultsTbl()
                    {
                        RepairFaultID = dataReader["RepairFaultID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["RepairFaultID"]),
                        RepairFaultDesc = dataReader["RepairFaultDesc"] == DBNull.Value ? string.Empty : dataReader["RepairFaultDesc"].ToString(),
                        SortOrder = dataReader["SortOrder"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["SortOrder"]),
                        Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }

        public string GetRepairFaultDesc(int RepairFaultID)
        {
            string repairFaultDesc = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)RepairFaultID, DbType.Int32);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT RepairFaultDesc FROM RepairFaultsTbl WHERE (RepairFaultID = ?)");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    repairFaultDesc = dataReader["RepairFaultDesc"] == DBNull.Value ? string.Empty : dataReader["RepairFaultDesc"].ToString();
                dataReader.Close();
            }
            trackerDb.Close();
            return repairFaultDesc;
        }
    }
}