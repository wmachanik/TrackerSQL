// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.CompanyNames
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
    public class CompanyNames
    {
        private const string CONST_SQL_SELECT = "SELECT [CustomerID], [CompanyName], [enabled] FROM [CustomersTbl] ORDER BY [enabled], [CompanyName]";
        private const string CONST_SQL_SELECTDEMOS = "SELECT [CustomerID], [CompanyName], [enabled] FROM [CustomersTbl] WHERE [CompanyName] LIKE 'DEMO%' ORDER BY [enabled], [CompanyName]";
        private const string CONST_SQL_CUSTOMERNAME_SELECT = "SELECT CompanyName FROM CustomersTbl WHERE (CustomerID = ?)";
        private long _CustomerID;
        private string _CompanyName;
        private bool _enabled;

        public CompanyNames()
        {
            this._CustomerID = 0;
            this._CompanyName = string.Empty;
            this._enabled = false;
        }

        public long CustomerID
        {
            get => this._CustomerID;
            set => this._CustomerID = value;
        }

        public string CompanyName
        {
            get => this._CompanyName;
            set => this._CompanyName = value;
        }

        public bool enabled
        {
            get => this._enabled;
            set => this._enabled = value;
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public List<CompanyNames> GetAll()
        {
            List<CompanyNames> all = new List<CompanyNames>();
            TrackerDb trackerDb = new TrackerDb();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT [CustomerID], [CompanyName], [enabled] FROM [CustomersTbl] ORDER BY [enabled], [CompanyName]");
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new CompanyNames()
                    {
                        CustomerID = Convert.ToInt32(dataReader["CustomerID"]),
                        CompanyName = dataReader["CompanyName"] == DBNull.Value ? "" : dataReader["CompanyName"].ToString(),
                        enabled = dataReader["enabled"] != DBNull.Value && Convert.ToBoolean(dataReader["enabled"])
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public List<CompanyNames> GetAllDemo()
        {
            List<CompanyNames> allDemo = new List<CompanyNames>();
            TrackerDb trackerDb = new TrackerDb();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT [CustomerID], [CompanyName], [enabled] FROM [CustomersTbl] WHERE [CompanyName] LIKE 'DEMO%' ORDER BY [enabled], [CompanyName]");
            if (dataReader != null)
            {
                while (dataReader.Read())
                    allDemo.Add(new CompanyNames()
                    {
                        CustomerID = Convert.ToInt32(dataReader["CustomerID"]),
                        CompanyName = dataReader["CompanyName"] == DBNull.Value ? "" : dataReader["CompanyName"].ToString(),
                        enabled = dataReader["enabled"] != DBNull.Value && Convert.ToBoolean(dataReader["enabled"])
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return allDemo;
        }

        public string GetCompanyNameByCompanyID(long pCustomerID)
        {
            string companyNameByCompanyId = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT CompanyName FROM CustomersTbl WHERE (CustomerID = ?)");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    companyNameByCompanyId = dataReader["CompanyName"] == DBNull.Value ? string.Empty : dataReader["CompanyName"].ToString();
                dataReader.Dispose();
            }
            trackerDb.Close();
            return companyNameByCompanyId;
        }
    }
}