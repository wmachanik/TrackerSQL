// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.TempOrdersData
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System.Collections.Generic;

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
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