// Decompiled with JetBrains decompiler
// Type: TrackerSQL.control.PriceLevelsTbl
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
    public class PriceLevelsTbl
    {
        public const int CONST_DEAFULT_PRICELEVELID = 1;
        private const string CONST_SQL_SELECT = "SELECT PriceLevelID, PriceLevelDesc, PricingFactor, Enabled, Notes FROM PriceLevelsTbl";
        private const string CONST_SQL_SELECTBYDESC = "SELECT PriceLevelID FROM PriceLevelsTbl WHERE (PriceLevelDesc = ?)";
        private const string CONST_SQL_INSERT = "INSERT INTO PriceLevelsTbl (PriceLevelDesc, PricingFactor, Enabled, Notes) VALUES ( ?, ?, ?, ?)";
        private const string CONST_SQL_UPDATE = "UPDATE PriceLevelsTbl SET PriceLevelDesc = ?, PricingFactor = ?, Enabled = ?, Notes = ? WHERE (PriceLevelID = ?)";
        private const string CONST_SQL_DELETE = "DELETE FROM PriceLevelsTbl WHERE (PriceLevelID = ?)";
        private int _PriceLevelID;
        private string _PriceLevelDesc;
        private double _PricingFactor;
        private bool _Enabled;
        private string _Notes;

        public PriceLevelsTbl()
        {
            this._PriceLevelID = 0;
            this._PriceLevelDesc = string.Empty;
            this._PricingFactor = 0.0;
            this._Enabled = false;
            this._Notes = string.Empty;
        }

        public int PriceLevelID
        {
            get => this._PriceLevelID;
            set => this._PriceLevelID = value;
        }

        public string PriceLevelDesc
        {
            get => this._PriceLevelDesc;
            set => this._PriceLevelDesc = value;
        }

        public double PricingFactor
        {
            get => this._PricingFactor;
            set => this._PricingFactor = value;
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
        public List<PriceLevelsTbl> GetAll(string SortBy)
        {
            List<PriceLevelsTbl> all = new List<PriceLevelsTbl>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT PriceLevelID, PriceLevelDesc, PricingFactor, Enabled, Notes FROM PriceLevelsTbl";
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new PriceLevelsTbl()
                    {
                        PriceLevelID = dataReader["PriceLevelID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PriceLevelID"]),
                        PriceLevelDesc = dataReader["PriceLevelDesc"] == DBNull.Value ? string.Empty : dataReader["PriceLevelDesc"].ToString(),
                        PricingFactor = dataReader["PricingFactor"] == DBNull.Value ? 0.0 : Math.Round(Convert.ToDouble(dataReader["PricingFactor"]), 3),
                        Enabled = dataReader["Enabled"] != DBNull.Value && Convert.ToBoolean(dataReader["Enabled"]),
                        Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public int GetPriceLevelIDByDesc(string pPriceLevelDesc)
        {
            int priceLevelIdByDesc = 0;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pPriceLevelDesc, DbType.String, "@PriceLevelDesc");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT PriceLevelID FROM PriceLevelsTbl WHERE (PriceLevelDesc = ?)");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    priceLevelIdByDesc = dataReader["PriceLevelID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PriceLevelID"]);
                dataReader.Close();
            }
            trackerDb.Close();
            return priceLevelIdByDesc;
        }

        [DataObjectMethod(DataObjectMethodType.Insert, true)]
        public string Insert(PriceLevelsTbl pPriceLevelsTbl)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pPriceLevelsTbl.PriceLevelDesc, DbType.String, "@PriceLevelDesc");
            trackerDb.AddParams((object)Math.Round(pPriceLevelsTbl.PricingFactor, SystemConstants.DatabaseConstants.NumDecimalPoints), DbType.Single, "@PricingFactor");
            trackerDb.AddParams((object)pPriceLevelsTbl.Enabled, DbType.Int32, "@Enabled");
            trackerDb.AddParams((object)pPriceLevelsTbl.Notes, DbType.String, "@Notes");
            string str = trackerDb.ExecuteNonQuerySQL("INSERT INTO PriceLevelsTbl (PriceLevelDesc, PricingFactor, Enabled, Notes) VALUES ( ?, ?, ?, ?)");
            trackerDb.Close();
            return str;
        }

        [DataObjectMethod(DataObjectMethodType.Update, true)]
        public string Update(PriceLevelsTbl pPriceLevelsTbl) => this.Update(pPriceLevelsTbl, 0);

        [DataObjectMethod(DataObjectMethodType.Update, false)]
        public string Update(PriceLevelsTbl pPriceLevelsTbl, int pOrignal_PriceLevelID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            if (pOrignal_PriceLevelID > 0)
                trackerDb.AddWhereParams((object)pOrignal_PriceLevelID, DbType.Int32);
            else
                trackerDb.AddWhereParams((object)pPriceLevelsTbl.PriceLevelID, DbType.Boolean, "@PriceLevelID");
            trackerDb.AddParams((object)pPriceLevelsTbl.PriceLevelDesc, DbType.String, "@PriceLevelDesc");
            trackerDb.AddParams((object)Math.Round(pPriceLevelsTbl.PricingFactor, SystemConstants.DatabaseConstants.NumDecimalPoints), DbType.Double, "@PricingFactor");
            trackerDb.AddParams((object)pPriceLevelsTbl.Enabled, DbType.Int32, "@Enabled");
            trackerDb.AddParams((object)pPriceLevelsTbl.Notes, DbType.String, "@Notes");
            string str = trackerDb.ExecuteNonQuerySQL("UPDATE PriceLevelsTbl SET PriceLevelDesc = ?, PricingFactor = ?, Enabled = ?, Notes = ? WHERE (PriceLevelID = ?)");
            trackerDb.Close();
            return str;
        }

        [DataObjectMethod(DataObjectMethodType.Delete, true)]
        public string Delete(PriceLevelsTbl pPriceLevelsTbl) => this.Delete(pPriceLevelsTbl.PriceLevelID);

        [DataObjectMethod(DataObjectMethodType.Delete, false)]
        public string Delete(int pPriceLevelID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pPriceLevelID, DbType.Int32, "@PriceLevelID");
            string str = trackerDb.ExecuteNonQuerySQL("DELETE FROM PriceLevelsTbl WHERE (PriceLevelID = ?)");
            trackerDb.Close();
            return str;
        }
    }
}