// Decompiled with JetBrains decompiler
// Type: TrackerSQL.control.ReoccuringOrderExtData
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

//- only form later versions #nullable disable
using System;

namespace TrackerSQL.Controls
{
    public class ReoccuringOrderExtData : ReoccuringOrderTbl
    {
        private string _CompanyName;
        private string _ItemTypeDesc;
        private string _ReoccuranceTypeDesc;
        private DateTime _PrepDate; // Added: prep/roast date associated with the next delivery

        public ReoccuringOrderExtData()
        {
            _CompanyName = _ItemTypeDesc = _ReoccuranceTypeDesc = string.Empty;
            _PrepDate = DateTime.MinValue; // default until set explicitly
        }

        /// <summary>
        /// Display/company name for the customer owning this recurring pattern.
        /// </summary>
        public string CompanyName
        {
            get => _CompanyName;
            set => _CompanyName = value;
        }

        /// <summary>
        /// Human-readable item description.
        /// </summary>
        public string ItemTypeDesc
        {
            get => _ItemTypeDesc;
            set => _ItemTypeDesc = value;
        }

        /// <summary>
        /// Human-readable recurrence type description.
        /// </summary>
        public string ReoccuranceTypeDesc
        {
            get => _ReoccuranceTypeDesc;
            set => _ReoccuranceTypeDesc = value;
        }

        /// <summary>
        /// The prep (roast) date that pairs with NextDateRequired (delivery date).
        /// This is NOT persisted by the base table; it is an extended/calculated value.
        /// </summary>
        public DateTime PrepDate
        {
            get => _PrepDate;
            set => _PrepDate = value;
        }
    }
}