using System;
using System.Collections.Generic;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// ObjectDataSource adapter for OrderDetail page — loads and updates by OrderID / OrderLineID.
    /// </summary>
    public class OrderDetailDataSource
    {
        private readonly OrdersRepository _ordersRepository = new OrdersRepository();

        public List<OrderHeaderData> LoadOrderSummary(int orderId, int maximumRows, int startRowIndex)
        {
            if (orderId <= 0)
                return new List<OrderHeaderData>();

            var header = _ordersRepository.GetOrderHeaderByOrderId(orderId);
            return header != null
                ? new List<OrderHeaderData> { header }
                : new List<OrderHeaderData>();
        }

        public bool UpdateOrderHeader(
            int orderId,
            long CustomerID,
            DateTime OrderDate,
            DateTime PrepDate,
            int ToBeDeliveredBy,
            DateTime RequiredByDate,
            bool Confirmed,
            bool Done,
            bool InvoiceDone,
            string PurchaseOrder,
            string Notes)
        {
            if (orderId <= 0) return false;

            var header = new OrderHeaderData
            {
                OrderID = orderId,
                CustomerID = CustomerID,
                OrderDate = OrderDate.Date,
                PrepDate = PrepDate.Date,
                ToBeDeliveredBy = ToBeDeliveredBy,
                RequiredByDate = RequiredByDate.Date,
                Confirmed = Confirmed,
                Done = Done,
                InvoiceDone = InvoiceDone,
                PurchaseOrder = PurchaseOrder ?? string.Empty,
                Notes = Notes ?? string.Empty
            };

            return _ordersRepository.UpdateOrderHeaderByOrderId(orderId, header);
        }

        public List<OrderDetailData> LoadOrderDetailData(int orderId, int maximumRows, int startRowIndex)
        {
            if (orderId <= 0)
                return new List<OrderDetailData>();

            return _ordersRepository.LoadOrderDetailDataByOrderId(orderId);
        }

        public bool UpdateOrderLine(
            long OrderLineID,
            long customerId,
            int ItemTypeID,
            DateTime deliveryDate,
            double QuantityOrdered,
            int PackagingID)
        {
            if (OrderLineID <= 0) return false;

            int resolvedItemId = new TrackerTools().ChangeItemIfGroupToNextItemInGroup(
                customerId, ItemTypeID, deliveryDate);

            return _ordersRepository.UpdateOrderLine(
                OrderLineID,
                resolvedItemId,
                QuantityOrdered,
                PackagingID);
        }

        public bool DeleteOrderLine(string orderLineId)
        {
            if (!int.TryParse(orderLineId, out int lineId) || lineId <= 0)
                return false;

            return _ordersRepository.DeleteOrderLineById(lineId);
        }
    }
}
