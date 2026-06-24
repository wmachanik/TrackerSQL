using System.Collections.Generic;
using System.ComponentModel;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// ObjectDataSource adapter for OrderDone page temp-order header and lines.
    /// </summary>
    public class OrderDoneDataSource
    {
        private readonly TempOrdersHeaderRepository _headerRepository = new TempOrdersHeaderRepository();
        private readonly TempOrdersLinesRepository _linesRepository = new TempOrdersLinesRepository();

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<OrderDoneHeaderView> GetHeader(int toHeaderId)
        {
            var header = _headerRepository.GetOrderDoneHeaderView(toHeaderId);
            return header != null
                ? new List<OrderDoneHeaderView> { header }
                : new List<OrderDoneHeaderView>();
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<OrderDoneLineView> GetLines(int toHeaderId)
        {
            return _linesRepository.GetOrderDoneLines(toHeaderId);
        }

        [DataObjectMethod(DataObjectMethodType.Update)]
        public bool UpdateLine(int TOLineID, int ItemID, float Qty, int PackagingID)
        {
            return _linesRepository.UpdateOrderDoneLine(TOLineID, ItemID, Qty, PackagingID);
        }

        [DataObjectMethod(DataObjectMethodType.Delete)]
        public bool DeleteLine(int TOLineID)
        {
            return _linesRepository.DeleteOrderDoneLine(TOLineID);
        }
    }
}
