// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.ItemContactRequires
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class ItemContactRequires
    {
        private int _TCIID;
        private int _ItemID;
        private long _CustomerID;
        private double _ItemQty;
        private int _ItemPrepID;
        private int _ItemPackagID;
        private bool _AutoFulfill;
        private bool _ReoccurOrder;
        private int _ReoccurID;

        public ItemContactRequires()
        {
            this._TCIID = 0;
            this._ItemID = 0;
            this._CustomerID = 0;
            this._ItemQty = 0.0;
            this._ItemPrepID = this._ItemPackagID = 0;
            this._AutoFulfill = this._ReoccurOrder = false;
            this._ReoccurID = 0;
        }

        public int TCIID
        {
            get => this._TCIID;
            set => this._TCIID = value;
        }

        public int ItemID
        {
            get => this._ItemID;
            set => this._ItemID = value;
        }

        public long CustomerID
        {
            get => this._CustomerID;
            set => this._CustomerID = value;
        }

        public double ItemQty
        {
            get => this._ItemQty;
            set => this._ItemQty = value;
        }

        public int ItemPrepID
        {
            get => this._ItemPrepID;
            set => this._ItemPrepID = value;
        }

        public int ItemPackagID
        {
            get => this._ItemPackagID;
            set => this._ItemPackagID = value;
        }

        public bool AutoFulfill
        {
            get => this._AutoFulfill;
            set => this._AutoFulfill = value;
        }

        public bool ReoccurOrder
        {
            get => this._ReoccurOrder;
            set => this._ReoccurOrder = value;
        }

        public int ReoccurID
        {
            get => this._ReoccurID;
            set => this._ReoccurID = value;
        }
    }
}