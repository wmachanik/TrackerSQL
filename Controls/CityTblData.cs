// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.CityTblData
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class CityTblData
    {
        private int _ID;
        private string _City;

        public CityTblData()
        {
            this._ID = 0;
            this._City = string.Empty;
        }

        public int ID
        {
            get => this._ID;
            set => this._ID = value;
        }

        public string City
        {
            get => this._City;
            set => this._City = value;
        }
    }
}