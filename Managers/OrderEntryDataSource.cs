using System.Collections.Generic;
using System.ComponentModel;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// ObjectDataSource adapter for OrderEntry page.
    /// </summary>
    public class OrderEntryDataSource
    {
        private readonly OrdersRepository _ordersRepository = new OrdersRepository();

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<OrderEntryListItem> GetDistinctOrders(bool pOrderDone, string pSearchFor, string pSearchValue)
        {
            return _ordersRepository.GetOrderEntryList(pOrderDone, pSearchFor, pSearchValue);
        }

        [DataObjectMethod(DataObjectMethodType.Update)]
        public bool UpdateOrderData(OrderEntryListItem newOrderData)
        {
            return UpdateOrderData(newOrderData, newOrderData.OrderID);
        }

        public bool UpdateOrderData(OrderEntryListItem newOrderData, long orig_OrderID)
        {
            if (newOrderData == null || orig_OrderID <= 0)
            {
                return false;
            }

            return _ordersRepository.UpdateOrderEntry(newOrderData, (int)orig_OrderID);
        }
    }
}
