//------------------------------------------------------------------------------
// TrackerSQL v3.x — AreaTblData
// Data access / control type: AreaTblData.
//------------------------------------------------------------------------------

//- only form later versions #nullable disable
using System;

namespace TrackerSQL.Controls
{
    [Obsolete("DO NOT USE Comtrols use Models - MIGRATION IN PROGRESS", true)]
    public class AreaTblData
    {
        private int _ID;
        private string _AreaName;

        public AreaTblData()
        {
            this._ID = 0;
            this._AreaName = string.Empty;
        }

        public int ID
        {
            get => this._ID;
            set => this._ID = value;
        }

        public string AreaName
        {
            get => this._AreaName;
            set => this._AreaName = value;
        }
    }
}
