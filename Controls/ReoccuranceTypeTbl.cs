// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.ReoccuranceTypeTbl
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
    public class ReoccuranceTypeTbl
    {
        // Keep existing constants for backward compatibility
        public const int CONST_WEEKTYPEID = 1;
        public const int CONST_DAYOFMONTHID = 5;
        
        /// <summary>
        /// Enum for recurring order types - centralized definition
        /// </summary>
        public enum RecurrenceType
        {
            Weekly = CONST_WEEKTYPEID,      // 1
            Monthly = CONST_DAYOFMONTHID    // 5
        }
        
        private const string CONST_SQL_SELECT = "SELECT ID, Type FROM ReoccuranceTypeTbl";
        private int _ID;
        private string _Type;

        public ReoccuranceTypeTbl()
        {
            this._ID = 0;
            this._Type = string.Empty;
        }

        public int ID
        {
            get => this._ID;
            set => this._ID = value;
        }

        public string Type
        {
            get => this._Type;
            set => this._Type = value;
        }

        /// <summary>
        /// Helper method to get RecurrenceType from ID
        /// </summary>
        public static RecurrenceType GetRecurrenceType(int id)
        {
            if (Enum.IsDefined(typeof(RecurrenceType), id))
                return (RecurrenceType)id;
            
            throw new ArgumentException($"Unknown recurrence type ID: {id}");
        }
        
        /// <summary>
        /// Helper method to check if a recurrence type ID is valid
        /// </summary>
        public static bool IsValidRecurrenceType(int id)
        {
            return Enum.IsDefined(typeof(RecurrenceType), id);
        }

        public List<ReoccuranceTypeTbl> GetAll()
        {
            List<ReoccuranceTypeTbl> all = new List<ReoccuranceTypeTbl>();
            string strSQL = "SELECT ID, Type FROM ReoccuranceTypeTbl";
            TrackerDb trackerDb = new TrackerDb();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new ReoccuranceTypeTbl()
                    {
                        ID = dataReader["ID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ID"]),
                        Type = dataReader["Type"] == DBNull.Value ? string.Empty : dataReader["Type"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }
    }
}