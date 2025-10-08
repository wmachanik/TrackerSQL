// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.CustomerTypeTbl
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
    public class CustomerTypeTbl
    {
        public const int CONST_COFFEEANDMAINT = 1;
        public const int CONST_SERVICE_CONTRACT = 2;
        public const int CONST_COFFEE_ONLY = 3;
        public const int CONST_RENTAL = 4;
        public const int CONST_OUTRIGHT_PURCHASE = 5;
        public const int CONST_SERVICE_ONLY = 6;
        public const int CONST_OTHER = 7;
        public const int CONST_GREEN_COFFEE_ONLY = 8;
        public const int CONST_INFO_ONLY = 9;
        private const string CONST_SQL_SELECT = "SELECT CustTypeID, CustTypeDesc, Notes FROM CustomerTypeTbl";
        private int _CustTypeID;
        private string _CustTypeDesc;
        private string _Notes;

        public CustomerTypeTbl()
        {
            this._CustTypeID = 0;
            this._CustTypeDesc = string.Empty;
            this._Notes = string.Empty;
        }

        public int CustTypeID
        {
            get => this._CustTypeID;
            set => this._CustTypeID = value;
        }

        public string CustTypeDesc
        {
            get => this._CustTypeDesc;
            set => this._CustTypeDesc = value;
        }

        public string Notes
        {
            get => this._Notes;
            set => this._Notes = value;
        }

        public List<CustomerTypeTbl> GetAll(string SortBy)
        {
            List<CustomerTypeTbl> all = new List<CustomerTypeTbl>();
            string strSQL = "SELECT CustTypeID, CustTypeDesc, Notes FROM CustomerTypeTbl";
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            TrackerDb trackerDb = new TrackerDb();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new CustomerTypeTbl()
                    {
                        CustTypeID = dataReader["CustTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustTypeID"]),
                        CustTypeDesc = dataReader["CustTypeDesc"] == DBNull.Value ? string.Empty : dataReader["CustTypeDesc"].ToString(),
                        Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }
    }
}