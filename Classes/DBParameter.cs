//------------------------------------------------------------------------------
// TrackerSQL v3.x — DBParameter
// Shared infrastructure / utility: DBParameter.
//------------------------------------------------------------------------------

using System.Data;

//- only form later versions #nullable disable
namespace TrackerSQL.Classes
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
