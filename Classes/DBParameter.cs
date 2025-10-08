// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.classes.DBParameter
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System.Data;

//- only form later versions #nullable disable
namespace TrackerDotNet.Classes
{
    public class DBParameter
    {
        public const string CONST_PARAMNAMEDEFAULT = "?";
        private object _DataValue;
        private DbType _DataDbType;
        private string _ParamName;

        public DBParameter()
        {
            this._DataValue = new object();
            this._DataDbType = DbType.String;
            this._ParamName = "?";
        }

        public object DataValue
        {
            get => this._DataValue;
            set => this._DataValue = value;
        }

        public DbType DataDbType
        {
            get => this._DataDbType;
            set => this._DataDbType = value;
        }

        public string ParamName
        {
            get => this._ParamName;
            set => this._ParamName = value;
        }
    }
}