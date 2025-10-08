// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.EquipTypeTbl
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Classes;

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class EquipTypeTbl
    {
        private const string CONST_SQL_SELECT = "SELECT EquipTypeId, EquipTypeName, EquipTypeDesc FROM EquipTypeTbl";
        private const string CONST_SQL_SELECTEQUIPNAME = "SELECT EquipTypeName FROM EquipTypeTbl WHERE (EquipTypeId = ?)";
        private const string CONST_SQL_UPDATE = "UPDATE EquipTypeTbl SET EquipTypeName = ?, EquipTypeDesc = ? WHERE (EquipTypeId = ?)";
        private const string CONST_SQL_INSERT = "INSERT INTO EquipTypeTbl (EquipTypeName, EquipTypeDesc) VALUES (?,?)";
        private int _EquipTypeId;
        private string _EquipTypeName;
        private string _EquipTypeDesc;

        public EquipTypeTbl()
        {
            this._EquipTypeId = 0;
            this._EquipTypeName = string.Empty;
            this._EquipTypeDesc = string.Empty;
        }

        public int EquipTypeId
        {
            get => this._EquipTypeId;
            set => this._EquipTypeId = value;
        }

        public string EquipTypeName
        {
            get => this._EquipTypeName;
            set => this._EquipTypeName = value;
        }

        public string EquipTypeDesc
        {
            get => this._EquipTypeDesc;
            set => this._EquipTypeDesc = value == null ? string.Empty : value;
        }

        public List<EquipTypeTbl> GetAll(string SortBy)
        {
            List<EquipTypeTbl> all = new List<EquipTypeTbl>();
            string strSQL = "SELECT EquipTypeId, EquipTypeName, EquipTypeDesc FROM EquipTypeTbl" + (!string.IsNullOrEmpty(SortBy) ? " ORDER BY " + SortBy : " ORDER BY EquipTypeName");
            TrackerDb trackerDb = new TrackerDb();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new EquipTypeTbl()
                    {
                        EquipTypeId = dataReader["EquipTypeId"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["EquipTypeId"]),
                        EquipTypeName = dataReader["EquipTypeName"] == DBNull.Value ? string.Empty : dataReader["EquipTypeName"].ToString(),
                        EquipTypeDesc = dataReader["EquipTypeDesc"] == DBNull.Value ? string.Empty : dataReader["EquipTypeDesc"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }

        public string GetEquipName(int pEquipID)
        {
            string equipName = "tba";
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pEquipID, DbType.Int32);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT EquipTypeName FROM EquipTypeTbl WHERE (EquipTypeId = ?)");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    equipName = dataReader["EquipTypeName"] == DBNull.Value ? string.Empty : dataReader["EquipTypeName"].ToString();
                dataReader.Close();
            }
            trackerDb.Close();
            return equipName;
        }

        public static void UpdateEquipItem(EquipTypeTbl objEquipType)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)objEquipType.EquipTypeName, DbType.String, "@EquipTypeName");
            trackerDb.AddParams((object)objEquipType.EquipTypeDesc, DbType.String, "@EquipTypeDesc");
            trackerDb.AddParams((object)objEquipType.EquipTypeId, DbType.String);
            trackerDb.ExecuteNonQuerySQL("UPDATE EquipTypeTbl SET EquipTypeName = ?, EquipTypeDesc = ? WHERE (EquipTypeId = ?)");
            trackerDb.Close();
        }

        public string InsertEquipItem(string pEquipTypeName, string pEquipTypeDesc)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pEquipTypeName, DbType.String, "@EquipTypeName");
            trackerDb.AddParams((object)pEquipTypeDesc, DbType.String, "@EquipTypeDesc");
            string str = trackerDb.ExecuteNonQuerySQL("INSERT INTO EquipTypeTbl (EquipTypeName, EquipTypeDesc) VALUES (?,?)");
            trackerDb.Close();
            return str;
        }

        public void InsertEquipObj(EquipTypeTbl objEquipType)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)objEquipType.EquipTypeName, DbType.String);
            trackerDb.AddParams((object)objEquipType.EquipTypeDesc, DbType.String);
            trackerDb.ExecuteNonQuerySQL("INSERT INTO EquipTypeTbl (EquipTypeName, EquipTypeDesc) VALUES (?,?)");
            trackerDb.Close();
        }
    }
}