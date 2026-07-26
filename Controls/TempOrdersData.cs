//------------------------------------------------------------------------------
// TrackerSQL v3.x — TempOrdersData
// Data access / control type: TempOrdersData.
//------------------------------------------------------------------------------

using System.Collections.Generic;

//- only form later versions #nullable disable
namespace TrackerSQL.Controls
{
    public class TempOrdersData
    {
        private TempOrdersHeaderTbl _TempOrdersHeaderTbl;
        private List<TempOrdersLinesTbl> _TempOrdersLines;

        public TempOrdersData()
        {
            this._TempOrdersHeaderTbl = new TempOrdersHeaderTbl();
            this._TempOrdersLines = new List<TempOrdersLinesTbl>();
        }

        public TempOrdersHeaderTbl HeaderData
        {
            get => this._TempOrdersHeaderTbl;
            set => this._TempOrdersHeaderTbl = value;
        }

        public List<TempOrdersLinesTbl> OrdersLines
        {
            get => this._TempOrdersLines;
            set => this._TempOrdersLines = value;
        }
    }
}
