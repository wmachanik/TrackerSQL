// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.MachineConditionsTbl
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
    public class MachineConditionsTbl
    {
        private const string CONST_SQL_SELECT = "SELECT MachineConditionID, ConditionDesc, SortOrder, Notes FROM MachineConditionsTbl";
        private int _MachineConditionID;
        private string _ConditionDesc;
        private int _SortOrder;
        private string _Notes;

        public MachineConditionsTbl()
        {
            this._MachineConditionID = 0;
            this._ConditionDesc = string.Empty;
            this._SortOrder = 0;
            this._Notes = string.Empty;
        }

        public int MachineConditionID
        {
            get => this._MachineConditionID;
            set => this._MachineConditionID = value;
        }

        public string ConditionDesc
        {
            get => this._ConditionDesc;
            set => this._ConditionDesc = value;
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

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public List<MachineConditionsTbl> GetAll(string SortBy)
        {
            List<MachineConditionsTbl> all = new List<MachineConditionsTbl>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = $"SELECT MachineConditionID, ConditionDesc, SortOrder, Notes FROM MachineConditionsTbl ORDER BY {(string.IsNullOrEmpty(SortBy) ? "SortOrder" : SortBy)}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new MachineConditionsTbl()
                    {
                        MachineConditionID = dataReader["MachineConditionID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["MachineConditionID"]),
                        ConditionDesc = dataReader["ConditionDesc"] == DBNull.Value ? string.Empty : dataReader["ConditionDesc"].ToString(),
                        SortOrder = dataReader["SortOrder"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["SortOrder"]),
                        Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }
    }
}