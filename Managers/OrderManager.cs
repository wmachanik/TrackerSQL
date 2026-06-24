using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    public class EnsureOrderResult
    {
        public string Error { get; set; } = string.Empty;
        public bool IsConflict { get; set; }
        public int OrderId { get; set; }
        public int ConflictingOrderId { get; set; }
        public bool Success => string.IsNullOrEmpty(Error) && OrderId > 0;
    }

    public class OrderManager
    {
        private readonly OrdersRepository _ordersRepository;
        private readonly TempOrdersHeaderRepository _tempOrdersHeaderRepository;
        private readonly TempOrdersLinesRepository _tempOrdersLinesRepository;
        private readonly ContactsRepository _contactsRepository;
        private readonly ItemsRepository _itemsRepository;
        private readonly ItemPackagingsRepository _itemPackagingsRepository;
        private readonly ContactsItemUsageRepository _contactsItemUsageRepository;
        private readonly PersonsRepository _personsRepository;

        public OrderManager()
        {
            _ordersRepository = new OrdersRepository();
            _tempOrdersHeaderRepository = new TempOrdersHeaderRepository();
            _tempOrdersLinesRepository = new TempOrdersLinesRepository();
            _contactsRepository = new ContactsRepository();
            _itemsRepository = new ItemsRepository();
            _itemPackagingsRepository = new ItemPackagingsRepository();
            _contactsItemUsageRepository = new ContactsItemUsageRepository();
            _personsRepository = new PersonsRepository();
        }

        public class AddOrderLineResult
        {
            public string Error { get; set; } = string.Empty;
            public int OrderId { get; set; }
            public bool Success => string.IsNullOrEmpty(Error) && OrderId > 0;
        }

        public OrderHeaderData GetOrderHeader(int orderId)
        {
            return orderId > 0 ? _ordersRepository.GetOrderHeaderByOrderId(orderId) : null;
        }

        public int GetOrderLineCount(int orderId)
        {
            return _ordersRepository.GetOrderLineCount(orderId);
        }

        /// <summary>
        /// Finds an order for the same contact and required-by date (and notes for sundry).
        /// </summary>
        public int? FindExistingOrderForHeader(OrderHeaderData header)
        {
            if (header == null || header.CustomerID <= 0 || header.RequiredByDate <= DateTime.MinValue)
                return null;

            return _ordersRepository.FindOrderIdByRequiredByDate(
                header.CustomerID,
                header.RequiredByDate,
                header.Notes ?? string.Empty);
        }

        /// <summary>
        /// Creates a new header or returns a conflict when another order already exists for contact + date.
        /// </summary>
        public EnsureOrderResult EnsureOrderHeader(OrderHeaderData header, int? currentOrderId = null, bool useExistingIfFound = false)
        {
            var result = new EnsureOrderResult();
            if (header == null || header.CustomerID <= 0)
            {
                result.Error = "Please select a contact.";
                return result;
            }

            if (header.RequiredByDate <= DateTime.MinValue)
            {
                result.Error = "Required-by date is missing.";
                return result;
            }

            if (header.ToBeDeliveredBy <= 0)
                header.ToBeDeliveredBy = SystemConstants.DeliveryConstants.DefaultDeliveryPersonID;

            int? existingId = FindExistingOrderForHeader(header);
            if (currentOrderId.HasValue && currentOrderId.Value > 0)
            {
                if (existingId.HasValue && existingId.Value != currentOrderId.Value)
                {
                    result.IsConflict = true;
                    result.ConflictingOrderId = existingId.Value;
                    result.Error = $"Another order (#{existingId.Value}) already exists for this contact and delivery date.";
                    return result;
                }

                if (!_ordersRepository.UpdateOrderHeaderByOrderId(currentOrderId.Value, header))
                {
                    result.Error = "Failed to update order header.";
                    return result;
                }

                result.OrderId = currentOrderId.Value;
                return result;
            }

            if (existingId.HasValue)
            {
                if (!useExistingIfFound)
                {
                    result.IsConflict = true;
                    result.ConflictingOrderId = existingId.Value;
                    result.Error = $"Order #{existingId.Value} already exists for this contact and delivery date.";
                    return result;
                }

                result.OrderId = existingId.Value;
                return result;
            }

            int newOrderId = _ordersRepository.InsertOrderHeader(ToOrderTblData(header));
            if (newOrderId <= 0)
            {
                result.Error = "Failed to create order header.";
                return result;
            }

            result.OrderId = newOrderId;
            return result;
        }

        public bool UpdateOrderHeader(int orderId, OrderHeaderData header)
        {
            if (orderId <= 0 || header == null)
                return false;

            header.OrderID = orderId;
            return _ordersRepository.UpdateOrderHeaderByOrderId(orderId, header);
        }

        public AddOrderLineResult AddOrderLineToOrder(int orderId, int itemTypeId, double quantity, int packagingId)
        {
            var result = new AddOrderLineResult { OrderId = orderId };
            if (orderId <= 0)
            {
                result.Error = "Order is not saved yet.";
                return result;
            }

            var header = GetOrderHeader(orderId);
            if (header == null)
            {
                result.Error = "Order not found.";
                return result;
            }

            var line = ToOrderTblData(header);
            line.ItemTypeID = itemTypeId;
            line.QuantityOrdered = quantity;
            line.PackagingID = packagingId;

            line.ItemTypeID = new TrackerTools().ChangeItemIfGroupToNextItemInGroup(
                line.CustomerID, line.ItemTypeID, line.RequiredByDate);

            int lineId = _ordersRepository.AddLineToExistingOrder(orderId, line);
            if (lineId <= 0)
            {
                result.Error = "Failed to add order line.";
                return result;
            }

            return result;
        }

        private static OrderTblData ToOrderTblData(OrderHeaderData header)
        {
            return new OrderTblData
            {
                CustomerID = header.CustomerID,
                OrderDate = header.OrderDate.Date,
                PrepDate = header.PrepDate.Date,
                RequiredByDate = header.RequiredByDate.Date,
                ToBeDeliveredBy = header.ToBeDeliveredBy > 0
                    ? header.ToBeDeliveredBy
                    : SystemConstants.DeliveryConstants.DefaultDeliveryPersonID,
                PurchaseOrder = header.PurchaseOrder ?? string.Empty,
                Notes = header.Notes ?? string.Empty,
                Confirmed = header.Confirmed,
                Done = header.Done,
                InvoiceDone = header.InvoiceDone
            };
        }

        public AddOrderLineResult AddOrderLine(OrderHeaderData headerData, OrderTblData orderData, int? existingOrderId = null)
        {
            return AddOrderLines(headerData, new[] { orderData }, existingOrderId);
        }

        public AddOrderLineResult AddOrderLines(OrderHeaderData headerData, IEnumerable<OrderTblData> lines, int? existingOrderId = null)
        {
            var result = new AddOrderLineResult();
            if (headerData == null)
            {
                result.Error = "Order header data is missing";
                return result;
            }

            if (lines == null)
            {
                result.Error = "No order lines to add";
                return result;
            }

            int orderId = existingOrderId ?? 0;
            var trackerTools = new TrackerTools();

            foreach (var orderData in lines)
            {
                if (orderData == null)
                    continue;

                ApplyHeaderToOrderLine(headerData, orderData);

                orderData.ItemTypeID = trackerTools.ChangeItemIfGroupToNextItemInGroup(
                    orderData.CustomerID, orderData.ItemTypeID, orderData.RequiredByDate);

                if (orderId <= 0)
                {
                    orderId = _ordersRepository.InsertNewOrderLine(orderData);
                    if (orderId <= 0)
                    {
                        result.Error = "Failed to create order";
                        return result;
                    }
                }
                else
                {
                    int lineId = _ordersRepository.AddLineToExistingOrder(orderId, orderData);
                    if (lineId <= 0)
                    {
                        result.Error = "Failed to add order line";
                        return result;
                    }
                }
            }

            result.OrderId = orderId;
            return result;
        }

        private static void ApplyHeaderToOrderLine(OrderHeaderData header, OrderTblData line)
        {
            if (line.CustomerID <= 0 && header.CustomerID > 0)
                line.CustomerID = header.CustomerID;

            if (line.ToBeDeliveredBy <= 0 && header.ToBeDeliveredBy > 0)
                line.ToBeDeliveredBy = header.ToBeDeliveredBy;

            if (line.OrderDate <= DateTime.MinValue && header.OrderDate > DateTime.MinValue)
                line.OrderDate = header.OrderDate;

            if (line.PrepDate <= DateTime.MinValue && header.PrepDate > DateTime.MinValue)
                line.PrepDate = header.PrepDate;

            if (line.RequiredByDate <= DateTime.MinValue && header.RequiredByDate > DateTime.MinValue)
                line.RequiredByDate = header.RequiredByDate;

            if (string.IsNullOrEmpty(line.Notes) && !string.IsNullOrEmpty(header.Notes))
                line.Notes = header.Notes;

            if (string.IsNullOrEmpty(line.PurchaseOrder) && !string.IsNullOrEmpty(header.PurchaseOrder))
                line.PurchaseOrder = header.PurchaseOrder;

            line.Confirmed = header.Confirmed;
            line.Done = header.Done;
            line.InvoiceDone = header.InvoiceDone;
        }

        public string DeleteOrderLine(int orderLineId)
        {
            return _ordersRepository.DeleteOrderLineById(orderLineId) ? string.Empty : "Failed to delete order line";
        }

        public List<ContactLookup> GetContactLookups()
        {
            return _contactsRepository.GetAllCompanyNames();
        }

        public List<Person> GetDeliveryPersons(string sortBy = "Abbreviation")
        {
            return _personsRepository.GetAllEnabled(sortBy);
        }

        public List<OrderItemLookup> GetItemLookups(string sortBy = "")
        {
            return _itemsRepository.GetOrderItemLookups(sortBy);
        }

        public List<OrderPackagingLookup> GetPackagingLookups()
        {
            var list = new List<OrderPackagingLookup>();
            foreach (var packaging in _itemPackagingsRepository.GetAll("ItemPrepDescription"))
            {
                list.Add(new OrderPackagingLookup
                {
                    PackagingID = packaging.ItemPackagingID,
                    Description = packaging.ItemPackagingDesc ?? string.Empty
                });
            }

            return list;
        }

        public List<OrderDetailData> GetOrderLines(int orderId)
        {
            if (orderId <= 0)
                return new List<OrderDetailData>();

            return _ordersRepository.LoadOrderDetailDataByOrderId(orderId);
        }

        public bool UpdateOrderLine(
            long orderLineId,
            long contactId,
            int itemTypeId,
            DateTime deliveryDate,
            double quantityOrdered,
            int packagingId)
        {
            if (orderLineId <= 0)
                return false;

            int resolvedItemId = new TrackerTools().ChangeItemIfGroupToNextItemInGroup(
                contactId, itemTypeId, deliveryDate);

            return _ordersRepository.UpdateOrderLine(
                orderLineId,
                resolvedItemId,
                quantityOrdered,
                packagingId);
        }

        public string DeleteOrderItem(int orderId)
        {
            return _ordersRepository.DeleteOrderById(orderId) ? string.Empty : "Failed to delete order";
        }

        public string MarkItemAsInvoiced(int orderId)
        {
            return _ordersRepository.UpdateSetInvoicedByOrderId(true, orderId)
                ? string.Empty
                : "Failed to mark invoiced";
        }

        public string MarkItemAsInvoiced(long customerId, DateTime deliveryDate, string notes)
        {
            return _ordersRepository.UpdateSetInvoiced(true, customerId, deliveryDate, notes)
                ? string.Empty
                : "Failed to mark invoiced";
        }

        public string UnDoOrderItem(int orderId)
        {
            return _ordersRepository.UpdateSetDoneById(false, orderId) ? string.Empty : "Failed to undo order";
        }

        public void MoveOrderDeliveryDate(DateTime newDate, int orderId)
        {
            _ordersRepository.UpdateOrderDeliveryDate(newDate, orderId);
        }

        public bool CompleteOrderDelivery(OrderHeaderData headerData, List<TempOrderLineData> orderLines)
        {
            TempOrderSession.CleanupCurrentTempOrder();

            var header = new TempOrdersHeader
            {
                ContactID = (int)headerData.CustomerID,
                OrderDate = headerData.OrderDate,
                PrepDate = headerData.PrepDate,
                RequiredByDate = headerData.RequiredByDate,
                ToBeDeliveredByID = headerData.ToBeDeliveredBy,
                Confirmed = headerData.Confirmed,
                Done = headerData.Done,
                Notes = headerData.Notes
            };

            int headerId = _tempOrdersHeaderRepository.InsertHeader(header);
            if (headerId <= 0)
            {
                return false;
            }

            foreach (var line in orderLines)
            {
                int? serviceTypeId = line.ServiceTypeID > 0
                    ? line.ServiceTypeID
                    : _itemsRepository.GetItemServiceTypeId(line.ItemID);

                if (!_tempOrdersLinesRepository.InsertLine(new TempOrdersLine
                {
                    TOHeaderID = headerId,
                    ItemID = line.ItemID,
                    Qty = line.Qty,
                    ItemPackagingID = line.PackagingID,
                    ItemServiceTypeID = serviceTypeId,
                    OriginalOrderID = line.OriginalOrderID
                }))
                {
                    TempOrderSession.CleanupCurrentTempOrder();
                    return false;
                }
            }

            TempOrderSession.BeginOrderDoneWorkflow(
                headerId,
                orderLines.Select(line => line.OriginalOrderID));
            return true;
        }
        // NEW METHODS: Move business logic from OrderDetail

        /// <summary>
        /// Calculates roast and delivery dates based on business rules
        /// </summary>
        public (DateTime PrepDate, DateTime deliveryDate) CalculateOrderDates(DateTime orderDate)
        {
            // Move the complex date calculation logic from InitializeNewOrderMode
            int num = orderDate.DayOfWeek <= DayOfWeek.Tuesday || orderDate.DayOfWeek >= DayOfWeek.Friday ?
                      (orderDate.DayOfWeek >= DayOfWeek.Wednesday ?
                       (orderDate.DayOfWeek >= DayOfWeek.Friday ? (int)(8 - orderDate.DayOfWeek) : (int)(3 - orderDate.DayOfWeek)) :
                       (int)(1 - orderDate.DayOfWeek)) : (int)(3 - orderDate.DayOfWeek);

            DateTime PrepDate = orderDate.AddDays((double)num);
            DateTime deliveryDate = PrepDate.DayOfWeek >= DayOfWeek.Friday ? PrepDate.AddDays(3.0) : PrepDate.AddDays(1.0);

            return (PrepDate, deliveryDate);
        }

        /// <summary>
        /// Sets customer preferences by ID and returns preference data
        /// </summary>
        public CustomerContactResult SetCustomerPreferencesById(string customerId)
        {
            var result = new CustomerContactResult();

            if (int.TryParse(customerId, out int custId))
            {
                result.CustomerFound = true;
                result.CustomerID = custId;

                try
                {
                    TrackerTools.ContactPreferedItems preferences = new TrackerTools().RetrieveCustomerPrefs(custId);
                    result.PreferredDeliveryByID = preferences.PreferredDeliveryByID;
                    result.Success = true;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Error retrieving customer preferences: {ex.Message}";
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, result.ErrorMessage);
                }
            }
            else
            {
                result.CustomerFound = false;
                result.Success = false;
                result.ErrorMessage = $"Invalid customer ID: {customerId}";
            }

            return result;
        }

        /// <summary>
        /// Finds customer by contact information and sets preferences
        /// </summary>
        public CustomerContactResult SetCustomerPreferencesByContact(string companyName, string contactName, string email)
        {
            var result = new CustomerContactResult();

            try
            {
                // Try email lookup first if provided
                if (!string.IsNullOrEmpty(email))
                {
                    var contacts = _contactsRepository.SearchByEmailLike(email);
                    if (contacts.Count > 0)
                    {
                        result.CustomerID = contacts[0].ContactID;
                        result.CustomerFound = true;
                        result.Success = true;
                        result.FoundByEmail = true;

                        // Get preferences for found customer
                        TrackerTools.ContactPreferedItems preferences = new TrackerTools().RetrieveCustomerPrefs(result.CustomerID);
                        result.PreferredDeliveryByID = preferences.PreferredDeliveryByID;
                        return result;
                    }
                }

                // Email lookup failed, use sundry customer with note
                result.CustomerID = SystemConstants.CustomerConstants.SundryCustomerID;
                result.CustomerFound = false;
                result.Success = true;
                result.UseSundryCustomer = true;

                // Build note text for sundry customer
                if (string.IsNullOrEmpty(companyName))
                {
                    result.NoteText = $"{contactName}: ";
                }
                else
                {
                    result.NoteText = $"{companyName}, {contactName}: ";
                }

                if (!string.IsNullOrEmpty(email))
                {
                    result.NoteText += $" [#{email}#]";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Error in customer lookup: {ex.Message}";
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, result.ErrorMessage);
            }

            return result;
        }

        /// <summary>
        /// Processes SKU parameters and creates order items
        /// </summary>
        public string ProcessSKUParameters(Dictionary<string, double> skuParams, long customerId, DateTime deliveryDate, string notes)
        {
            try
            {
                // This would implement the SKU processing logic from NewOrderDetail
                // For now, log the attempt
                string skuList = string.Join(", ", skuParams.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Processing SKU parameters for customer {customerId}: {skuList}");

                // TODO: Implement actual SKU processing logic
                return string.Empty; // Empty string indicates success
            }
            catch (Exception ex)
            {
                string error = $"Error processing SKU parameters: {ex.Message}";
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, error);
                return error;
            }
        }
        /// <summary>
        /// Gets last order items for a customer (returns items to be added, doesn't insert them)
        /// </summary>
        public List<OrderLineData> GetLastOrderItems(long customerId, bool setDates)
        {
            var orderItems = new List<OrderLineData>();

            try
            {
                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"GetLastOrderItems called for customer {customerId}, setDates: {setDates}");

                if (customerId <= 0)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Invalid customer ID: {customerId}");
                    return orderItems;
                }

                // GET customer-specific dates if requested (this updates session with proper dates)
                if (setDates)
                {
                    SetCustomerSpecificDates(customerId);
                }

                // Get last items used for coffee (ServiceTypeID = 2)
                List<ContactsItemUsage> lastItemsUsed = _contactsItemUsageRepository.GetLastItemsUsed((int)customerId, 2);

                if (lastItemsUsed.Count > 0)
                {
                    foreach (ContactsItemUsage itemUsage in lastItemsUsed)
                    {
                        if (itemUsage.ItemProvidedID.HasValue && itemUsage.ItemProvidedID.Value > 0)
                        {
                            var orderLine = CreateOrderLineFromLastUsage(customerId, itemUsage);
                            if (orderLine != null)
                            {
                                orderItems.Add(orderLine);

                                if (!string.IsNullOrEmpty(itemUsage.Notes))
                                {
                                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Last order note for customer {customerId}: {itemUsage.Notes}");
                                }
                            }
                        }
                    }
                }
                else
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"No last items found for customer {customerId}, using customer preferences");

                    // No last items found, use customer preferences
                    TrackerTools trackerTools = new TrackerTools();
                    TrackerTools.ContactPreferedItems customerPrefs = trackerTools.RetrieveCustomerPrefs(customerId);

                    var orderLine = CreateOrderLineFromPreferences(customerId, customerPrefs);
                    if (orderLine != null)
                    {
                        orderItems.Add(orderLine);
                    }
                }

                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"GetLastOrderItems completed for customer {customerId}, found {orderItems.Count} items");
                return orderItems;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Error in GetLastOrderItems for customer {customerId}: {ex.Message}");
                return orderItems;
            }
        }

        /// <summary>
        /// Sets customer-specific roast and delivery dates based on their Area (like SetPrepAndDeliveryValues in NewOrderDetail)
        /// </summary>
        private void SetCustomerSpecificDates(long customerId)
        {
            try
            {
                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Setting customer-specific dates for customer {customerId}");

                TrackerTools trackerTools = new TrackerTools();
                DateTime deliveryDate = DateTime.MinValue; // This will be set by reference
                DateTime PrepDate = trackerTools.GetNextPreperationDateByCustomerID(customerId, ref deliveryDate);
                DateTime orderDate = TimeZoneUtils.Now().Date;

                // Update session with customer-specific dates
                HttpContext context = HttpContext.Current;
                if (context?.Session != null)
                {
                    context.Session[SystemConstants.SessionConstants.BoundDeliveryDate] = deliveryDate.Date;
                    // Note: PrepDate and OrderDate would need session constants if you want to store them
                }

                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Customer-specific dates set - Order: {orderDate:yyyy-MM-dd}, Roast: {PrepDate:yyyy-MM-dd}, Delivery: {deliveryDate:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Error setting customer-specific dates for customer {customerId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates an OrderLineData from last usage (doesn't insert, just creates the data object)
        /// </summary>
        private OrderLineData CreateOrderLineFromLastUsage(long customerId, ContactsItemUsage itemUsage)
        {
            try
            {
                TrackerTools trackerTools = new TrackerTools();
                int finalItemTypeId = trackerTools.ChangeItemIfGroupToNextItemInGroup(
                    customerId,
                    itemUsage.ItemProvidedID ?? 0,
                    DateTime.Now);

                string itemName = _itemsRepository.GetItemDescById(finalItemTypeId);
                int packagingId = itemUsage.ItemPackagingID ?? 0;
                string packagingName = packagingId > 0 ? GetPackagingDesc(packagingId) : string.Empty;

                var orderLine = new OrderLineData
                {
                    ItemID = finalItemTypeId,
                    ItemName = itemName,
                    Qty = itemUsage.QtyProvided ?? 0.0,
                    PackagingID = packagingId,
                    PackagingName = packagingName
                };

                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Created order line from last usage for customer {customerId}: ItemID={orderLine.ItemID}, Qty={orderLine.Qty}");
                return orderLine;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Error creating order line from last usage for customer {customerId}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Creates an OrderLineData from customer preferences (doesn't insert, just creates the data object)
        /// </summary>
        private OrderLineData CreateOrderLineFromPreferences(long customerId, TrackerTools.ContactPreferedItems preferences)
        {
            try
            {
                // Apply group item logic if needed
                TrackerTools trackerTools = new TrackerTools();
                int finalItemTypeId = trackerTools.ChangeItemIfGroupToNextItemInGroup(
                    customerId,
                    preferences.PreferedItem,
                    DateTime.Now);

                // Get item name for display
                string itemName = _itemsRepository.GetItemDescById(finalItemTypeId);
                string packagingName = preferences.PrefPackagingID > 0 ? GetPackagingDesc(preferences.PrefPackagingID) : string.Empty;

                var orderLine = new OrderLineData
                {
                    ItemID = finalItemTypeId,
                    ItemName = itemName,
                    Qty = preferences.PreferedQty,
                    PackagingID = preferences.PrefPackagingID,
                    PackagingName = packagingName
                };

                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Created order line from preferences for customer {customerId}: ItemID={orderLine.ItemID}, Qty={orderLine.Qty}");
                return orderLine;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Error creating order line from preferences for customer {customerId}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Helper method to get packaging description (you might need to implement this if it doesn't exist)
        /// </summary>
        private string GetPackagingDesc(int packagingID)
        {
            try
            {
                return packagingID > 0 ? _itemPackagingsRepository.GetPackagingDescById(packagingID) : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// Adds an order line based on last usage data
        /// </summary>
        private bool AddOrderLineFromLastUsage(long customerId, ContactsItemUsage itemUsage)
        {
            try
            {
                // Get current session data for order header
                var sessionData = GetSessionOrderData();

                OrderTblData orderData = new OrderTblData
                {
                    CustomerID = customerId,
                    OrderDate = sessionData.OrderDate,
                    PrepDate = sessionData.PrepDate,
                    RequiredByDate = sessionData.RequiredByDate,
                    ToBeDeliveredBy = sessionData.ToBeDeliveredBy,
                    PurchaseOrder = sessionData.PurchaseOrder ?? string.Empty,
                    Confirmed = sessionData.Confirmed,
                    InvoiceDone = sessionData.InvoiceDone,
                    Done = sessionData.Done,
                    Notes = sessionData.Notes ?? string.Empty,
                    ItemTypeID = itemUsage.ItemProvidedID ?? 0,
                    QuantityOrdered = itemUsage.QtyProvided ?? 0.0,
                    PackagingID = itemUsage.ItemPackagingID ?? 0
                };

                // Apply group item logic if needed
                TrackerTools trackerTools = new TrackerTools();
                orderData.ItemTypeID = trackerTools.ChangeItemIfGroupToNextItemInGroup(
                    orderData.CustomerID,
                    orderData.ItemTypeID,
                    orderData.RequiredByDate);

                // Insert the order line
                bool success = _ordersRepository.InsertNewOrderLine(orderData) > 0;

                if (success)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Added last order item for customer {customerId}: ItemID={orderData.ItemTypeID}, Qty={orderData.QuantityOrdered}");
                }
                else
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Error adding last order item for customer {customerId}");
                }

                return success;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Error in AddOrderLineFromLastUsage for customer {customerId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Adds an order line based on customer preferences
        /// </summary>
        private bool AddOrderLineFromPreferences(long customerId, TrackerTools.ContactPreferedItems preferences)
        {
            try
            {
                // Get current session data for order header
                var sessionData = GetSessionOrderData();

                OrderTblData orderData = new OrderTblData
                {
                    CustomerID = customerId,
                    OrderDate = sessionData.OrderDate,
                    PrepDate = sessionData.PrepDate,
                    RequiredByDate = sessionData.RequiredByDate,
                    ToBeDeliveredBy = sessionData.ToBeDeliveredBy,
                    PurchaseOrder = sessionData.PurchaseOrder ?? string.Empty,
                    Confirmed = sessionData.Confirmed,
                    InvoiceDone = sessionData.InvoiceDone,
                    Done = sessionData.Done,
                    Notes = sessionData.Notes ?? string.Empty,
                    ItemTypeID = preferences.PreferedItem,
                    QuantityOrdered = preferences.PreferedQty,
                    PackagingID = preferences.PrefPackagingID
                };

                // Apply group item logic if needed
                TrackerTools trackerTools = new TrackerTools();
                orderData.ItemTypeID = trackerTools.ChangeItemIfGroupToNextItemInGroup(
                    orderData.CustomerID,
                    orderData.ItemTypeID,
                    orderData.RequiredByDate);

                // Insert the order line
                bool success = _ordersRepository.InsertNewOrderLine(orderData) > 0;

                if (success)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Added preferred item for customer {customerId}: ItemID={orderData.ItemTypeID}, Qty={orderData.QuantityOrdered}");
                }
                else
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Error adding preferred item for customer {customerId}");
                }

                return success;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Error in AddOrderLineFromPreferences for customer {customerId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets session order data for building order lines
        /// </summary>
        private SessionOrderData GetSessionOrderData()
        {
            var sessionData = new SessionOrderData();

            try
            {
                HttpContext context = HttpContext.Current;
                if (context?.Session != null)
                {
                    // Get data from session
                    if (context.Session[SystemConstants.SessionConstants.BoundCustomerID] != null)
                    {
                        sessionData.CustomerID = (long)context.Session[SystemConstants.SessionConstants.BoundCustomerID];
                    }

                    if (context.Session[SystemConstants.SessionConstants.BoundDeliveryDate] != null)
                    {
                        sessionData.RequiredByDate = (DateTime)context.Session[SystemConstants.SessionConstants.BoundDeliveryDate];
                    }
                    else
                    {
                        sessionData.RequiredByDate = TimeZoneUtils.Now().Date;
                    }

                    if (context.Session[SystemConstants.SessionConstants.BoundNotes] != null)
                    {
                        sessionData.Notes = (string)context.Session[SystemConstants.SessionConstants.BoundNotes];
                    }

                    // Calculate order and roast dates
                    sessionData.OrderDate = TimeZoneUtils.Now().Date;
                    var (PrepDate, deliveryDate) = CalculateOrderDates(sessionData.OrderDate);
                    sessionData.PrepDate = PrepDate;

                    // Use session delivery date if available, otherwise calculated
                    if (sessionData.RequiredByDate == DateTime.MinValue)
                    {
                        sessionData.RequiredByDate = deliveryDate;
                    }

                    // Set default values
                    sessionData.ToBeDeliveredBy = SystemConstants.DeliveryConstants.DefaultDeliveryPersonID;
                    sessionData.Confirmed = true;
                    sessionData.InvoiceDone = false;
                    sessionData.Done = false;
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Error getting session order data: {ex.Message}");
            }

            return sessionData;
        }

        /// <summary>
        /// Helper class for session order data
        /// </summary>
        private class SessionOrderData
        {
            public long CustomerID { get; set; }
            public DateTime OrderDate { get; set; }
            public DateTime PrepDate { get; set; }
            public DateTime RequiredByDate { get; set; }
            public int ToBeDeliveredBy { get; set; }
            public string PurchaseOrder { get; set; }
            public bool Confirmed { get; set; }
            public bool InvoiceDone { get; set; }
            public bool Done { get; set; }
            public string Notes { get; set; }
        }
        /// <summary>
        /// Extracts email address from notes using delimiters
        /// </summary>
        public string ExtractEmailFromNotes(string notes)
        {
            if (string.IsNullOrEmpty(notes)) return string.Empty;

            const string startDelimiter = "[#";
            const string endDelimiter = "#]";

            int startIndex = notes.IndexOf(startDelimiter);
            if (startIndex >= 0)
            {
                int endIndex = notes.IndexOf(endDelimiter, startIndex);
                if (endIndex > startIndex)
                {
                    return notes.Substring(startIndex + startDelimiter.Length,
                                         endIndex - startIndex - startDelimiter.Length);
                }
            }

            return string.Empty;
        }

        // Helper DTO classes
        public class CustomerContactResult
        {
            public bool Success { get; set; }
            public bool CustomerFound { get; set; }
            public long CustomerID { get; set; }
            public int PreferredDeliveryByID { get; set; }
            public bool UseSundryCustomer { get; set; }
            public bool FoundByEmail { get; set; }
            public string NoteText { get; set; }
            public string ErrorMessage { get; set; }
        }
        public class OrderLineData
        {
            public int ItemID { get; set; }
            public string ItemName { get; set; }
            public double Qty { get; set; }
            public int PackagingID { get; set; }
            public string PackagingName { get; set; }
        }
        public class TempOrderLineData
        {
            public int ItemID { get; set; }
            public double Qty { get; set; }
            public int PackagingID { get; set; }
            public int ServiceTypeID { get; set; }
            public int OriginalOrderID { get; set; }

            public TempOrdersLine ToTempOrdersLine(int headerId, int? itemServiceTypeId)
            {
                return new TempOrdersLine
                {
                    TOHeaderID = headerId,
                    ItemID = ItemID,
                    Qty = Qty,
                    ItemPackagingID = PackagingID,
                    ItemServiceTypeID = itemServiceTypeId ?? ServiceTypeID,
                    OriginalOrderID = OriginalOrderID
                };
            }
        }
    }
}
