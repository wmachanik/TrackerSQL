using System;
using System.Collections.Generic;

namespace TrackerSQL.Models
{
    [Serializable]
    public class WooOrderImportUiState
    {
        public List<long> WooOrderIds { get; set; } = new List<long>();
        /// <summary>Slim preview rows for restore without re-fetching Woo orders.</summary>
        public List<WooOrderImportPreviewRow> PreviewRows { get; set; } = new List<WooOrderImportPreviewRow>();
        public int PageIndex { get; set; }
        public string Mode { get; set; }
        public string OrderId { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public bool UpdateExisting { get; set; }
        public string StatusMessage { get; set; }
        public bool StatusIsError { get; set; }
    }
}
