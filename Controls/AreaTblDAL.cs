/// <summary>
/// Data access layer for Area-related operations in the TrackerSQL application.
/// Provides methods to retrieve Area information, Area names, Area IDs, and Area preparation/delivery rules.
/// 
/// Implemented routines:
/// - GetAllAreaTblData(string SortBy): Returns a list of all cities, optionally sorted.
/// - GetAreaName(int pAreaID): Returns the Area name for a given Area ID.
/// - GetAreaID(string pAreaName): Returns the Area ID for a given Area name.
/// - GetAreaIdByCustomerId(long customerId): Returns the Area ID for a given customer ID.
/// - GetPrepRulesForArea(int AreaId): Returns all preparation/delivery rules for a given Area.
/// </summary>
// Original from Decompiled with JetBrains decompiler
// Type: TrackerSQL.control.AreaTblDAL
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Controls;

//- only form later versions #nullable disable
namespace TrackerSQL.Controls
{
    [Obsolete("DO NOT USE Comtrols use Models - MIGRATION IN PROGRESS", true)]
    public class AreaTblDAL
    {
        //private const string CONST_CONSTRING = "Tracker08ConnectionString";
        private const string CONST_SQL_SUMMARYDATA = "SELECT ID, AreaName FROM AreaTbl";
        private const string CONST_SQL_SELECTAreaBYID = "SELECT AreaName FROM AreaTbl WHERE ID = ?";
        private const string CONST_SQL_SELECTIDBYAreaBY = "SELECT ID FROM AreaTbl WHERE AreaName Like '?'";
        private const string CONST_SQL_INSERT = "INSERT INTO AreaTbl (ID, AreaName) VALUES (?, ?)";
        public const int CONST_DEFAULT_AreaID = 1;

        public static List<AreaTblData> GetAllAreaTblData(string SortBy)
        {
            List<AreaTblData> allAreaTblData = new List<AreaTblData>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = $"SELECT ID, AreaName FROM AreaTbl ORDER BY {(!string.IsNullOrEmpty(SortBy) ? SortBy : " AreaName")}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    allAreaTblData.Add(new AreaTblData()
                    {
                        ID = Convert.ToInt32(dataReader["ID"]),
                        AreaName = dataReader["AreaName"] == DBNull.Value ? "" : dataReader["AreaName"].ToString()
                    });
            }
            dataReader.Close();
            trackerDb.Close();
            return allAreaTblData;
        }

        public string GetAreaName(int pAreaID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pAreaID, DbType.Int32, "@ID");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT AreaName FROM AreaTbl WHERE ID = ?");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    empty = dataReader["AreaName"] == DBNull.Value ? "" : dataReader["AreaName"].ToString();
                dataReader.Close();
            }
            trackerDb.Close();
            return empty;
        }

        public int GetAreaID(string pAreaName)
        {
            int AreaId = 0;
            TrackerDb trackerDb = new TrackerDb();
            if (!pAreaName.Contains("%"))
                pAreaName = $"%{pAreaName}%";
            trackerDb.AddWhereParams((object)pAreaName, DbType.String, "@AreaName");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT ID FROM AreaTbl WHERE AreaName Like '?'");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    AreaId = dataReader["ID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ID"].ToString());
                dataReader.Close();
            }
            trackerDb.Close();
            return AreaId;
        }

        /// <summary>
        /// Gets the AreaID for a given customer.
        /// </summary>
        public int GetAreaIdByCustomerId(long customerId)
        {
            int AreaId = 0;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams(customerId, DbType.Int64, "@CustomerID");
            IDataReader reader = trackerDb.ExecuteSQLGetDataReader("SELECT AreaID FROM CustomersTbl WHERE CustomerID = ?");
            if (reader != null && reader.Read())
            {
                AreaId = reader["AreaID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["AreaID"]);
                reader.Close();
            }
            trackerDb.Close();
            return AreaId;
        }

        /// <summary>
        /// Gets all prep rules for a Area.
        /// </summary>
        public List<AreaPrepDaysTbl> GetPrepRulesForArea(int AreaId)
        {
            return new AreaPrepDaysTbl().GetAllByAreaId(AreaId);
        }
    }
}
