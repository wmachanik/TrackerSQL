/// <summary>
/// Data access layer for city-related operations in the TrackerDotNet application.
/// Provides methods to retrieve city information, city names, city IDs, and city preparation/delivery rules.
/// 
/// Implemented routines:
/// - GetAllCityTblData(string SortBy): Returns a list of all cities, optionally sorted.
/// - GetCityName(int pCityID): Returns the city name for a given city ID.
/// - GetCityID(string pCityName): Returns the city ID for a given city name.
/// - GetCityIdByCustomerId(long customerId): Returns the city ID for a given customer ID.
/// - GetPrepRulesForCity(int cityId): Returns all preparation/delivery rules for a given city.
/// </summary>
// Original from Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.CityTblDAL
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Classes;
using TrackerDotNet.Controls;

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class CityTblDAL
    {
        //private const string CONST_CONSTRING = "Tracker08ConnectionString";
        private const string CONST_SQL_SUMMARYDATA = "SELECT ID, City FROM CityTbl";
        private const string CONST_SQL_SELECTCITYBYID = "SELECT City FROM CityTbl WHERE ID = ?";
        private const string CONST_SQL_SELECTIDBYCITYBY = "SELECT ID FROM CityTbl WHERE City Like '?'";
        private const string CONST_SQL_INSERT = "INSERT INTO CityTbl (ID, City) VALUES (?, ?)";
        public const int CONST_DEFAULT_CITYID = 1;

        public static List<CityTblData> GetAllCityTblData(string SortBy)
        {
            List<CityTblData> allCityTblData = new List<CityTblData>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = $"SELECT ID, City FROM CityTbl ORDER BY {(!string.IsNullOrEmpty(SortBy) ? SortBy : " City")}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    allCityTblData.Add(new CityTblData()
                    {
                        ID = Convert.ToInt32(dataReader["ID"]),
                        City = dataReader["City"] == DBNull.Value ? "" : dataReader["City"].ToString()
                    });
            }
            dataReader.Close();
            trackerDb.Close();
            return allCityTblData;
        }

        public string GetCityName(int pCityID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCityID, DbType.Int32, "@ID");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT City FROM CityTbl WHERE ID = ?");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    empty = dataReader["City"] == DBNull.Value ? "" : dataReader["City"].ToString();
                dataReader.Close();
            }
            trackerDb.Close();
            return empty;
        }

        public int GetCityID(string pCityName)
        {
            int cityId = 0;
            TrackerDb trackerDb = new TrackerDb();
            if (!pCityName.Contains("%"))
                pCityName = $"%{pCityName}%";
            trackerDb.AddWhereParams((object)pCityName, DbType.String, "@City");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT ID FROM CityTbl WHERE City Like '?'");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    cityId = dataReader["ID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ID"].ToString());
                dataReader.Close();
            }
            trackerDb.Close();
            return cityId;
        }

        /// <summary>
        /// Gets the CityID for a given customer.
        /// </summary>
        public int GetCityIdByCustomerId(long customerId)
        {
            int cityId = 0;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams(customerId, DbType.Int64, "@CustomerID");
            IDataReader reader = trackerDb.ExecuteSQLGetDataReader("SELECT City FROM CustomersTbl WHERE CustomerID = ?");
            if (reader != null && reader.Read())
            {
                cityId = reader["City"] == DBNull.Value ? 0 : Convert.ToInt32(reader["City"]);
                reader.Close();
            }
            trackerDb.Close();
            return cityId;
        }

        /// <summary>
        /// Gets all prep rules for a city.
        /// </summary>
        public List<CityPrepDaysTbl> GetPrepRulesForCity(int cityId)
        {
            return new CityPrepDaysTbl().GetAllByCityId(cityId);
        }
    }
}