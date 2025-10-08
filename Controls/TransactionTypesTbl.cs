// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.TransactionTypesTbl
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
    public class TransactionTypesTbl
    {
        public const int CONST_INSERT_TRANSACTION_INT = 1;
        public const int CONST_UPDATE_TRANSACTION_INT = 2;
        public const int CONST_ENABLE_TRANSACTION_INT = 3;
        public const int CONST_DISABLE_TRANSACTION_INT = 4;
        public const int CONST_CLOSED_TRANSACTION_INT = 5;
        public const int CONST_DELETE_TRANSACTION_INT = 9;
        private const string CONST_SQL_SELECT = "SELECT TransactionID, TransactionType, Notes FROM TransactionTypesTbl";
        private const string CONST_SQL_SELECTTTANSACTIONBYID = "SELECT [TransactionType] FROM [TransactionTypesTbl] WHERE ([TransactionID] = ?) ";
        private const string CONST_SQL_INSERT = "INSERT INTO TransactionTypesTbl (TransactionID, TransactionType, Notes) VALUES (?,?,?)";
        private const string CONST_SQL_UPDATE = "UPDATE TransactionTypesTbl SET TransactionType = ? , Notes = ? WHERE (TransactionID = ?)";
        private int _TransactionID;
        private string _TransactionType;
        private string _Notes;

        public TransactionTypesTbl()
        {
            this._TransactionID = 0;
            this._TransactionType = string.Empty;
            this._Notes = string.Empty;
        }

        public int TransactionID
        {
            get => this._TransactionID;
            set => this._TransactionID = value;
        }

        public string TransactionType
        {
            get => this._TransactionType;
            set => this._TransactionType = value;
        }

        public string Notes
        {
            get => this._Notes;
            set => this._Notes = value;
        }

        public bool InsertDefaultTransactions()
        {
            bool flag = false;
            List<TransactionTypesTbl> transactionTypesTblList = new List<TransactionTypesTbl>();
            transactionTypesTblList.Add(new TransactionTypesTbl()
            {
                TransactionID = 1,
                TransactionType = "Insert",
                Notes = "Insert Transaction"
            });
            transactionTypesTblList.Add(new TransactionTypesTbl()
            {
                TransactionID = 2,
                TransactionType = "Update",
                Notes = "Update Transaction"
            });
            transactionTypesTblList.Add(new TransactionTypesTbl()
            {
                TransactionID = 3,
                TransactionType = "Enable",
                Notes = "Enable Transaction"
            });
            transactionTypesTblList.Add(new TransactionTypesTbl()
            {
                TransactionID = 4,
                TransactionType = "Disble ",
                Notes = "Disble that are sent Transaction"
            });
            transactionTypesTblList.Add(new TransactionTypesTbl()
            {
                TransactionID = 5,
                TransactionType = "Closed ",
                Notes = "Closed Transaction"
            });
            transactionTypesTblList.Add(new TransactionTypesTbl()
            {
                TransactionID = 9,
                TransactionType = "Delete",
                Notes = "Delete Transaction"
            });
            foreach (TransactionTypesTbl pTransactionType in transactionTypesTblList)
                flag = string.IsNullOrWhiteSpace(this.InsertTransactionType(pTransactionType)) && flag;
            transactionTypesTblList.Clear();
            return flag;
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public List<TransactionTypesTbl> GetAll(string SortBy)
        {
            List<TransactionTypesTbl> all = new List<TransactionTypesTbl>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT TransactionID, TransactionType, Notes FROM TransactionTypesTbl";
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new TransactionTypesTbl()
                    {
                        TransactionID = dataReader["TransactionID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["TransactionID"]),
                        TransactionType = dataReader["TransactionType"] == DBNull.Value ? string.Empty : dataReader["TransactionType"].ToString(),
                        Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }

        public string GetTransactionTypeByID(int pTransactionID)
        {
            string transactionTypeById = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT [TransactionType] FROM [TransactionTypesTbl] WHERE ([TransactionID] = ?) ".Replace("?", pTransactionID.ToString());
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                transactionTypeById = dataReader.Read() ? pTransactionID.ToString() : (dataReader["TransactionType"] == DBNull.Value ? string.Empty : dataReader["TransactionType"].ToString());
                dataReader.Close();
            }
            trackerDb.Close();
            return transactionTypeById;
        }

        [DataObjectMethod(DataObjectMethodType.Insert, true)]
        public string InsertTransactionType(TransactionTypesTbl pTransactionType)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pTransactionType.TransactionID, DbType.Int32);
            trackerDb.AddParams((object)pTransactionType.TransactionType);
            trackerDb.AddParams((object)pTransactionType.Notes);
            string str = trackerDb.ExecuteNonQuerySQL("INSERT INTO TransactionTypesTbl (TransactionID, TransactionType, Notes) VALUES (?,?,?)");
            trackerDb.Close();
            return str;
        }

        [DataObjectMethod(DataObjectMethodType.Update, true)]
        public static void UpdateTransaction(TransactionTypesTbl pTransactionType)
        {
            TransactionTypesTbl.UpdateTransaction(pTransactionType, 0);
        }

        [DataObjectMethod(DataObjectMethodType.Update, false)]
        public static void UpdateTransaction(
          TransactionTypesTbl pTransactionType,
          int pOrignal_TransactionID)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pTransactionType.TransactionType);
            trackerDb.AddParams((object)pTransactionType.Notes);
            if (pOrignal_TransactionID > 0)
                trackerDb.AddWhereParams((object)pOrignal_TransactionID, DbType.Int32);
            else
                trackerDb.AddWhereParams((object)pTransactionType.TransactionID, DbType.Int32);
            trackerDb.ExecuteNonQuerySQL("UPDATE TransactionTypesTbl SET TransactionType = ? , Notes = ? WHERE (TransactionID = ?)");
            trackerDb.Close();
        }
    }
}