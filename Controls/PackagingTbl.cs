// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.PackagingTbl
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
    public class PackagingTbl
    {
        public const int CONST_PACKID_NA = 0;
        public const int CONST_PACKID_LESS1KG = 2;
        public const int CONST_PACKID_WHITEFILTER = 8;
        public const int CONST_PACKID_BLUEFILTER = 9;
        private const string CONST_SQL_SELECT = "SELECT PackagingID, Description, AdditionalNotes, Symbol, Colour, BGColour FROM PackagingTbl";
        private const string CONST_SQL_INSERT = "INSERT INTO PackagingTbl (Description, AdditionalNotes, Symbol, Colour, BGColour) VALUES (?          , ?               , ?    , ?     , ?)";
        private const string CONST_SQL_UPDATE = "UPDATE PackagingTbl SET Description = ?, AdditionalNotes = ?, Symbol = ?, Colour = ?, BGColour = ? WHERE (PackagingID = ?)";
        private const string CONST_SQL_SELECT_PACKAGINGDESC = "SELECT Description FROM PackagingTbl WHERE PackagingID = ?";
        private int _PackagingID;
        private string _Description;
        private string _AdditionalNotes;
        private string _Symbol;
        private int _Colour;
        private string _BGColour;

        public PackagingTbl()
        {
            this._PackagingID = 0;
            this._Description = string.Empty;
            this._AdditionalNotes = string.Empty;
            this._Symbol = string.Empty;
            this._Colour = 0;
            this._BGColour = string.Empty;
        }

        public int PackagingID
        {
            get => this._PackagingID;
            set => this._PackagingID = value;
        }

        public string Description
        {
            get => this._Description;
            set => this._Description = value;
        }

        public string AdditionalNotes
        {
            get => this._AdditionalNotes;
            set => this._AdditionalNotes = value;
        }

        public string Symbol
        {
            get => this._Symbol;
            set => this._Symbol = value;
        }

        public int Colour
        {
            get => this._Colour;
            set => this._Colour = value;
        }

        public string BGColour
        {
            get => this._BGColour;
            set => this._BGColour = value;
        }

        public List<PackagingTbl> GetAll(string SortBy)
        {
            List<PackagingTbl> all = new List<PackagingTbl>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT PackagingID, Description, AdditionalNotes, Symbol, Colour, BGColour FROM PackagingTbl" + (!string.IsNullOrEmpty(SortBy) ? " ORDER BY " + SortBy : " ORDER BY Description");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new PackagingTbl()
                    {
                        PackagingID = dataReader["PackagingID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PackagingID"]),
                        Description = dataReader["Description"] == DBNull.Value ? string.Empty : dataReader["Description"].ToString(),
                        AdditionalNotes = dataReader["AdditionalNotes"] == DBNull.Value ? string.Empty : dataReader["AdditionalNotes"].ToString(),
                        Symbol = dataReader["Symbol"] == DBNull.Value ? string.Empty : dataReader["Symbol"].ToString(),
                        Colour = dataReader["Colour"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["Colour"]),
                        BGColour = dataReader["BGColour"] == DBNull.Value ? string.Empty : dataReader["BGColour"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }

        public string GetPackagingDesc(int pPackagingID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pPackagingID, DbType.Int32, "@PackagingID");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT Description FROM PackagingTbl WHERE PackagingID = ?");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    empty = dataReader["Description"] == DBNull.Value ? "" : dataReader["Description"].ToString();
                dataReader.Close();
            }
            trackerDb.Close();
            return empty;
        }

        public string InsertPackaging(PackagingTbl objPackagingTbl)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)objPackagingTbl.Description, DbType.String);
            trackerDb.AddParams((object)objPackagingTbl.AdditionalNotes, DbType.String);
            trackerDb.AddParams((object)objPackagingTbl.Symbol, DbType.String);
            trackerDb.AddParams((object)objPackagingTbl.Colour, DbType.Int32);
            trackerDb.AddParams((object)objPackagingTbl.BGColour, DbType.String);
            string str = trackerDb.ExecuteNonQuerySQL("INSERT INTO PackagingTbl (Description, AdditionalNotes, Symbol, Colour, BGColour) VALUES (?          , ?               , ?    , ?     , ?)");
            trackerDb.Close();
            return str;
        }

        public string UpdatePackaging(PackagingTbl objPackagingTbl)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)objPackagingTbl.Description, DbType.String);
            trackerDb.AddParams(string.IsNullOrEmpty(objPackagingTbl.AdditionalNotes) ? (object)string.Empty : (object)objPackagingTbl.AdditionalNotes, DbType.String);
            trackerDb.AddParams(string.IsNullOrEmpty(objPackagingTbl.Symbol) ? (object)string.Empty : (object)objPackagingTbl.Symbol, DbType.String);
            trackerDb.AddParams((object)objPackagingTbl.Colour, DbType.Int32);
            trackerDb.AddParams(string.IsNullOrEmpty(objPackagingTbl.BGColour) ? (object)string.Empty : (object)objPackagingTbl.BGColour, DbType.String);
            trackerDb.AddWhereParams((object)objPackagingTbl.PackagingID, DbType.Int32);
            string str = trackerDb.ExecuteNonQuerySQL("UPDATE PackagingTbl SET Description = ?, AdditionalNotes = ?, Symbol = ?, Colour = ?, BGColour = ? WHERE (PackagingID = ?)");
            trackerDb.Close();
            return str;
        }
    }
}