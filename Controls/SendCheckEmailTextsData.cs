// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.SendCheckEmailTextsData
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Data;
using TrackerDotNet.Classes;

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class SendCheckEmailTextsData
    {
        private const string CONST_SQL_SELECT = "SELECT SCEMTID, Header, Body, Footer, DateLastChange, Notes FROM SendCheckEmailTextsTbl";
        private const string CONST_SQL_UPDATE = "UPDATE SendCheckEmailTextsTbl SET Header = ?, Body = ?, Footer = ?, DateLastChange = ?, Notes = ? WHERE SCEMTID = ?";
        private int _SCEMTID;
        private string _Header;
        private string _Body;
        private string _Footer;
        private DateTime _DateLastChange;
        private string _Notes;

        public SendCheckEmailTextsData()
        {
            this._SCEMTID = 0;
            this._Header = string.Empty;
            this._Body = string.Empty;
            this._Footer = string.Empty;
            this._DateLastChange = TimeZoneUtils.Now().Date;
            this._Notes = string.Empty;
        }

        public int SCEMTID
        {
            get => this._SCEMTID;
            set => this._SCEMTID = value;
        }

        public string Header
        {
            get => this._Header;
            set => this._Header = value;
        }

        public string Body
        {
            get => this._Body;
            set => this._Body = value;
        }

        public string Footer
        {
            get => this._Footer;
            set => this._Footer = value;
        }

        public DateTime DateLastChange
        {
            get => this._DateLastChange;
            set => this._DateLastChange = value;
        }

        public string Notes
        {
            get => this._Notes;
            set => this._Notes = value;
        }

        public SendCheckEmailTextsData GetTexts()
        {
            SendCheckEmailTextsData texts = new SendCheckEmailTextsData();
            TrackerDb trackerDb = new TrackerDb();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT SCEMTID, Header, Body, Footer, DateLastChange, Notes FROM SendCheckEmailTextsTbl");
            if (dataReader != null)
            {
                if (dataReader.Read())
                {
                    texts.SCEMTID = dataReader["SCEMTID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["SCEMTID"]);
                    texts.Header = dataReader["Header"] == DBNull.Value ? string.Empty : dataReader["Header"].ToString();
                    texts.Body = dataReader["Body"] == DBNull.Value ? string.Empty : dataReader["Body"].ToString();
                    texts.Footer = dataReader["Footer"] == DBNull.Value ? string.Empty : dataReader["Footer"].ToString();
                    texts.DateLastChange = dataReader["DateLastChange"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["DateLastChange"]).Date;
                    texts.Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString();
                }
                dataReader.Close();
            }
            trackerDb.Close();
            return texts;
        }

        public string UpdateTexts(SendCheckEmailTextsData pEmailTextsData, int pOriginalID)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pEmailTextsData.Header, DbType.String, "@Header");
            trackerDb.AddParams((object)pEmailTextsData.Body, DbType.String, "@Body");
            trackerDb.AddParams((object)pEmailTextsData.Footer, DbType.String, "@Footer");
            trackerDb.AddParams((object)TimeZoneUtils.Now().Date, DbType.Date, "@DateLastChange");
            trackerDb.AddParams((object)pEmailTextsData.Notes, DbType.String, "@Notes");
            trackerDb.AddWhereParams((object)pOriginalID, DbType.Int32, "@SCEMTID");
            string str = trackerDb.ExecuteNonQuerySQL("UPDATE SendCheckEmailTextsTbl SET Header = ?, Body = ?, Footer = ?, DateLastChange = ?, Notes = ? WHERE SCEMTID = ?");
            trackerDb.Close();
            return str;
        }
    }
}