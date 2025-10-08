// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.RepairStatusesTbl
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
    public class RepairStatusesTbl
    {
        private const string CONST_SQL_SELECT = "SELECT RepairStatusID, RepairStatusDesc, EmailClient, SortOrder, StatusNote, Notes FROM RepairStatusesTbl";
        //private const string CONST_SQL_SELECT = "SELECT RepairStatusID, RepairStatusDesc, EmailClient, SortOrder, Notes FROM RepairStatusesTbl";
        private const string CONST_SQL_SELECTSTATUSDESC = "SELECT RepairStatusDesc FROM RepairStatusesTbl WHERE (RepairStatusID = ?)";
        private int _RepairStatusID;
        private string _RepairStatusDesc;
        private bool _EmailClient;
        private int _SortOrder;
        private string _StatusNote;
        private string _Notes;

        public RepairStatusesTbl()
        {
            this._RepairStatusID = 0;
            this._RepairStatusDesc = string.Empty;
            this._EmailClient = false;
            this._SortOrder = 0;
            this._Notes = string.Empty;
        }

        public int RepairStatusID
        {
            get => this._RepairStatusID;
            set => this._RepairStatusID = value;
        }

        public string RepairStatusDesc
        {
            get => this._RepairStatusDesc;
            set => this._RepairStatusDesc = value;
        }

        public bool EmailClient
        {
            get => this._EmailClient;
            set => this._EmailClient = value;
        }

        public int SortOrder
        {
            get => this._SortOrder;
            set => this._SortOrder = value;
        }
        public string StatusNote
        {
            get => _StatusNote;
            set => _StatusNote = value;
        }
        public string Notes
        {
            get => this._Notes;
            set => this._Notes = value;
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public List<RepairStatusesTbl> GetAll(string SortBy)
        {
            List<RepairStatusesTbl> all = new List<RepairStatusesTbl>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = $"{CONST_SQL_SELECT} ORDER BY {(string.IsNullOrEmpty(SortBy) ? "SortOrder" : SortBy)}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new RepairStatusesTbl()
                    {
                        RepairStatusID = dataReader["RepairStatusID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["RepairStatusID"]),
                        RepairStatusDesc = dataReader["RepairStatusDesc"] == DBNull.Value ? string.Empty : dataReader["RepairStatusDesc"].ToString(),
                        EmailClient = dataReader["EmailClient"] != DBNull.Value && Convert.ToBoolean(dataReader["EmailClient"]),
                        SortOrder = dataReader["SortOrder"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["SortOrder"]),
                        StatusNote = dataReader["StatusNote"] == DBNull.Value ? string.Empty : dataReader["StatusNote"].ToString(),
                        Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }

        public string GetRepairStatusDesc(int RepairStatusID)
        {
            string repairStatusDesc = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)RepairStatusID, DbType.Int32);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT RepairStatusDesc FROM RepairStatusesTbl WHERE (RepairStatusID = ?)");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    repairStatusDesc = dataReader["RepairStatusDesc"] == DBNull.Value ? string.Empty : dataReader["RepairStatusDesc"].ToString();
                dataReader.Close();
            }
            trackerDb.Close();
            return repairStatusDesc;
        }
        public string GetStatusNote(int repairStatusID)
        {
            string statusNote = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams(repairStatusID, DbType.Int32);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT StatusNote FROM RepairStatusesTbl WHERE (RepairStatusID = ?)");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    statusNote = dataReader["StatusNote"] == DBNull.Value ? string.Empty : dataReader["StatusNote"].ToString();
                dataReader.Close();
            }
            trackerDb.Close();
            return string.IsNullOrWhiteSpace(statusNote) ? GetRepairStatusDesc(repairStatusID) : statusNote;
        }
        [DataObjectMethod(DataObjectMethodType.Insert, true)]
        public void Insert(RepairStatusesTbl pRepairStatusesTbl)
        {
            if (pRepairStatusesTbl == null)
                return;

            try
            {
                if (pRepairStatusesTbl.SortOrder <= 0)
                {
                    using (var dbGet = new TrackerDb())
                    {
                        var rdr = dbGet.ExecuteSQLGetDataReader("SELECT MAX(SortOrder) AS MaxSort FROM RepairStatusesTbl");
                        if (rdr != null)
                        {
                            if (rdr.Read())
                            {
                                int max = rdr["MaxSort"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["MaxSort"]);
                                pRepairStatusesTbl.SortOrder = max + 1;
                            }
                            rdr.Close();
                        }
                        dbGet.Close();
                    }
                }

                using (var db = new TrackerDb())
                {
                    db.AddParams(pRepairStatusesTbl.RepairStatusDesc ?? string.Empty, DbType.String);
                    db.AddParams(pRepairStatusesTbl.EmailClient, DbType.Boolean);
                    db.AddParams(pRepairStatusesTbl.SortOrder, DbType.Int32);
                    db.AddParams(string.IsNullOrWhiteSpace(pRepairStatusesTbl.StatusNote) ? (object)DBNull.Value : pRepairStatusesTbl.StatusNote, DbType.String);
                    db.AddParams(string.IsNullOrWhiteSpace(pRepairStatusesTbl.Notes) ? (object)DBNull.Value : pRepairStatusesTbl.Notes, DbType.String);

                    string err = db.ExecuteNonQuerySQLWithParams(
                        "INSERT INTO RepairStatusesTbl (RepairStatusDesc, EmailClient, SortOrder, StatusNote, Notes) VALUES (?,?,?,?,?)",
                        db.Params);

                    if (string.IsNullOrWhiteSpace(err))
                    {
                        var rdr = db.ExecuteSQLGetDataReader("SELECT @@IDENTITY");
                        if (rdr != null)
                        {
                            if (rdr.Read())
                                pRepairStatusesTbl.RepairStatusID = Convert.ToInt32(rdr[0]);
                            rdr.Close();
                        }
                        AppLogger.WriteLog(SystemConstants.LogTypes.Repairs,
                            $"Repair Status INSERT OK (ID={pRepairStatusesTbl.RepairStatusID}, Desc='{pRepairStatusesTbl.RepairStatusDesc}')");
                    }
                    else
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.System,
                            $"Repair Status INSERT FAILED (Desc='{pRepairStatusesTbl.RepairStatusDesc}'). Error: {err}");
                    }

                    db.Close();
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    $"Repair Status INSERT EXCEPTION (Desc='{pRepairStatusesTbl?.RepairStatusDesc}'): {ex.Message}");
                throw;
            }
        }

        [DataObjectMethod(DataObjectMethodType.Update, true)]
        public void Update(RepairStatusesTbl entity)
        {
            if (entity == null || entity.RepairStatusID <= 0) return;
            try
            {
                using (var db = new TrackerDb())
                {
                    db.AddParams(entity.RepairStatusDesc ?? string.Empty, DbType.String);
                    db.AddParams(entity.EmailClient, DbType.Boolean);
                    db.AddParams(entity.SortOrder, DbType.Int32);
                    db.AddParams(string.IsNullOrWhiteSpace(entity.StatusNote) ? (object)DBNull.Value : entity.StatusNote, DbType.String);
                    db.AddParams(string.IsNullOrWhiteSpace(entity.Notes) ? (object)DBNull.Value : entity.Notes, DbType.String);
                    db.AddWhereParams(entity.RepairStatusID, DbType.Int32);
                    string err = db.ExecuteNonQuerySQLWithParams(
                        "UPDATE RepairStatusesTbl SET RepairStatusDesc=?, EmailClient=?, SortOrder=?, StatusNote=?, Notes=? WHERE RepairStatusID=?",
                        db.Params,
                        db.WhereParams);
                    if (string.IsNullOrWhiteSpace(err))
                        AppLogger.WriteLog(SystemConstants.LogTypes.Repairs, $"Repair Status UPDATE OK (ID={entity.RepairStatusID})");
                    else
                        AppLogger.WriteLog(SystemConstants.LogTypes.System, $"Repair Status UPDATE FAILED (ID={entity.RepairStatusID}) {err}");
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, $"Repair Status UPDATE EXCEPTION (ID={entity.RepairStatusID}) {ex.Message}");
                throw;
            }
        }

        [DataObjectMethod(DataObjectMethodType.Delete, true)]
        public void Delete(int pRepairStatusID)
        {
            try
            {
                using (var db = new TrackerDb())
                {
                    db.AddWhereParams(pRepairStatusID, DbType.Int32);
                    string err = db.ExecuteNonQuerySQLWithParams(
                        "DELETE FROM RepairStatusesTbl WHERE RepairStatusID=?",
                        null,
                        db.WhereParams);

                    if (string.IsNullOrWhiteSpace(err))
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.Repairs,
                            $"Repair Status DELETE OK (ID={pRepairStatusID})");
                    }
                    else
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.System,
                            $"Repair Status DELETE FAILED (ID={pRepairStatusID}). Error: {err}");
                    }

                    db.Close();
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    $"Repair Status DELETE EXCEPTION (ID={pRepairStatusID}): {ex.Message}");
                throw;
            }
        }
    }
}