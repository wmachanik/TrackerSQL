// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.SectionTypesTbl
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
    public class SectionTypesTbl
    {
        public const int CONST_ORDERS_SECTION_INT = 1;
        public const int CONST_CUSTOMER_SECTION_INT = 2;
        public const int CONST_REMINDERS_SECTION_INT = 3;
        public const int CONST_REPAIRS_SECTION_INT = 4;
        public const int CONST_RECURRING_SECTION_INT = 5;
        public const int CONST_SYSTEM_SECTION_INT = 9;
        private const string CONST_SQL_SELECT = "SELECT SectionID, SectionType, Notes FROM SectionTypesTbl";
        private const string CONST_SQL_SELECTSECTIONBYID = "SELECT SectionType FROM SectionTypesTbl WHERE SectionID = ? ";
        private const string CONST_SQL_INSERT = "INSERT INTO SectionTypesTbl (SectionID, SectionType, Notes) VALUES (?,?,?)";
        private const string CONST_SQL_UPDATE = "UPDATE SectionTypesTbl SET SectionType = ? , Notes = ? WHERE (SectionID = ?)";
        private int _SectionID;
        private string _SectionType;
        private string _Notes;

        public SectionTypesTbl()
        {
            this._SectionID = 0;
            this._SectionType = string.Empty;
            this._Notes = string.Empty;
        }

        public int SectionID
        {
            get => this._SectionID;
            set => this._SectionID = value;
        }

        public string SectionType
        {
            get => this._SectionType;
            set => this._SectionType = value;
        }

        public string Notes
        {
            get => this._Notes;
            set => this._Notes = value;
        }

        public bool InsertDefaultSections()
        {
            bool flag = false;
            List<SectionTypesTbl> sectionTypesTblList = new List<SectionTypesTbl>();
            sectionTypesTblList.Add(new SectionTypesTbl()
            {
                SectionID = 1,
                SectionType = "Order",
                Notes = "Order section"
            });
            sectionTypesTblList.Add(new SectionTypesTbl()
            {
                SectionID = 2,
                SectionType = "Customer",
                Notes = "Customer section"
            });
            sectionTypesTblList.Add(new SectionTypesTbl()
            {
                SectionID = 3,
                SectionType = "Reminders",
                Notes = "Reminders that are sent section"
            });
            sectionTypesTblList.Add(new SectionTypesTbl()
            {
                SectionID = 4,
                SectionType = "Repairs",
                Notes = "Repairs section"
            });
            sectionTypesTblList.Add(new SectionTypesTbl()
            {
                SectionID = 5,
                SectionType = "Recurring",
                Notes = "Recurring Order section"
            });
            sectionTypesTblList.Add(new SectionTypesTbl()
            {
                SectionID = 9,
                SectionType = "System",
                Notes = "System section"
            });
            foreach (SectionTypesTbl pSectionType in sectionTypesTblList)
                flag = string.IsNullOrWhiteSpace(this.InsertSectionType(pSectionType)) && flag;
            sectionTypesTblList.Clear();
            return flag;
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public List<SectionTypesTbl> GetAll(string SortBy)
        {
            List<SectionTypesTbl> all = new List<SectionTypesTbl>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT SectionID, SectionType, Notes FROM SectionTypesTbl";
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new SectionTypesTbl()
                    {
                        SectionID = dataReader["SectionID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["SectionID"]),
                        SectionType = dataReader["SectionType"] == DBNull.Value ? string.Empty : dataReader["SectionType"].ToString(),
                        Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }

        public string GetSectionTypeByID(int pSectionID)
        {
            string sectionTypeById = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pSectionID, DbType.Int32);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT SectionType FROM SectionTypesTbl WHERE SectionID = ? ");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    sectionTypeById = dataReader["SectionType"] == DBNull.Value ? string.Empty : dataReader["SectionType"].ToString();
                dataReader.Close();
            }
            trackerDb.Close();
            return sectionTypeById;
        }

        [DataObjectMethod(DataObjectMethodType.Insert, true)]
        public string InsertSectionType(SectionTypesTbl pSectionType)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pSectionType.SectionID, DbType.Int32);
            trackerDb.AddParams((object)pSectionType.SectionType);
            trackerDb.AddParams((object)pSectionType.Notes);
            string str = trackerDb.ExecuteNonQuerySQL("INSERT INTO SectionTypesTbl (SectionID, SectionType, Notes) VALUES (?,?,?)");
            trackerDb.Close();
            return str;
        }

        [DataObjectMethod(DataObjectMethodType.Update, true)]
        public static void UpdateSection(SectionTypesTbl pSectionType)
        {
            SectionTypesTbl.UpdateSection(pSectionType, 0);
        }

        [DataObjectMethod(DataObjectMethodType.Update, false)]
        public static void UpdateSection(SectionTypesTbl pSectionType, int pOrignal_SectionID)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pSectionType.SectionType);
            trackerDb.AddParams((object)pSectionType.Notes);
            if (pOrignal_SectionID > 0)
                trackerDb.AddWhereParams((object)pOrignal_SectionID, DbType.Int32);
            else
                trackerDb.AddWhereParams((object)pSectionType.SectionID, DbType.Int32);
            trackerDb.ExecuteNonQuerySQL("UPDATE SectionTypesTbl SET SectionType = ? , Notes = ? WHERE (SectionID = ?)");
            trackerDb.Close();
        }
    }
}