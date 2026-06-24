// Decompiled with JetBrains decompiler
// Type: TrackerSQL.control.AreaTblData
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

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
