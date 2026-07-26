//------------------------------------------------------------------------------
// TrackerSQL v3.x — ItemUnitsTbl
// Data access / control type: ItemUnitsTbl.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using TrackerSQL.Classes;

//- only form later versions #nullable disable
namespace TrackerSQL.Controls
{
    public class ItemUnitsTbl
    {
        private const string CONST_SQL_SELECT = "SELECT ItemUnitID, UnitOfMeasure, UnitDescription FROM ItemUnitsTbl";
        private int _ItemUnitID;
        private string _UnitOfMeasure;
        private string _UnitDescription;

        public ItemUnitsTbl()
        {
            this._ItemUnitID = 0;
            this._UnitOfMeasure = string.Empty;
            this._UnitDescription = string.Empty;
        }

        public int ItemUnitID
        {
            get => this._ItemUnitID;
            set => this._ItemUnitID = value;
        }

        public string UnitOfMeasure
        {
            get => this._UnitOfMeasure;
            set => this._UnitOfMeasure = value;
        }

        public string UnitDescription
        {
            get => this._UnitDescription;
            set => this._UnitDescription = value;
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public List<ItemUnitsTbl> GetAll(string SortBy)
        {
            List<ItemUnitsTbl> all = new List<ItemUnitsTbl>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT ItemUnitID, UnitOfMeasure, UnitDescription FROM ItemUnitsTbl";
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new ItemUnitsTbl()
                    {
                        ItemUnitID = dataReader["ItemUnitID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemUnitID"]),
                        UnitOfMeasure = dataReader["UnitOfMeasure"] == DBNull.Value ? string.Empty : dataReader["UnitOfMeasure"].ToString(),
                        UnitDescription = dataReader["UnitDescription"] == DBNull.Value ? string.Empty : dataReader["UnitDescription"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }
    }
}
