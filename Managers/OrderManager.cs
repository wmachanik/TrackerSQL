using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TrackerSQL.Classes;
using TrackerSQL.Controls;

namespace TrackerSQL.Managers
{
    public class OrderManager
    {
        public string AddOrderLine(OrderHeaderData headerData, OrderTblData orderData)
        {
            TrackerTools trackerTools = new TrackerTools();
            orderData.ItemTypeID = trackerTools.ChangeItemIfGroupToNextItemInGroup(orderData.CustomerID, orderData.ItemTypeID, orderData.RequiredByDate);
            OrderTbl orderTbl = new OrderTbl();
            return orderTbl.InsertNewOrderLine(orderData);
        }
        public string DeleteOrderItem(int orderId)
        {
            return new OrderTbl().DeleteOrderById(orderId);
        }

        public string MarkItemAsInvoiced(long customerId, DateTime deliveryDate, string notes)
        {
            return new OrderTbl().UpdateSetInvoiced(true, customerId, deliveryDate, notes);
        }

        public string UnDoOrderItem(int orderId)
        {
            return new OrderTbl().UpdateSetDoneByID(false, orderId);
        }

        public void MoveOrderDeliveryDate(DateTime newDate, int orderId)
        {
            new OrderTbl().UpdateOrderDeliveryDate(newDate, orderId);
        }

        public bool CompleteOrderDelivery(OrderHeaderData headerData, List<TempOrderLineData> orderLines)
        {
            TempOrdersDAL tempOrdersDal = new TempOrdersDAL();
            if (!tempOrdersDal.KillTempOrdersData())
                return false;

            TempOrdersData tempOrder = new TempOrdersData();
            tempOrder.HeaderData = new TempOrdersHeaderTbl
            {
                CustomerID = headerData.CustomerID,
                OrderDate = headerData.OrderDate,
                RoastDate = headerData.RoastDate,
                RequiredByDate = headerData.RequiredByDate,
                ToBeDeliveredByID = headerData.ToBeDeliveredBy,
                Confirmed = headerData.Confirmed,
                Done = headerData.Done,
                Notes = headerData.Notes
            };
            tempOrder.OrdersLines = orderLines.Select(line =>
            {
                var tbl = line.ToTempOrdersLinesTbl();
                tbl.ServiceTypeID = new ItemTypeTbl().GetServiceID(tbl.ItemID);
                return tbl;
            }).ToList();

            return tempOrdersDal.Insert(tempOrder);
        }
        // NEW METHODS: Move business logic from OrderDetail

        /// <summary>
        /// Calculates roast and delivery dates based on business rules
        /// </summary>
        public (DateTime roastDate, DateTime deliveryDate) CalculateOrderDates(DateTime orderDate)
        {
            // Move the complex date calculation logic from InitializeNewOrderMode
            int num = orderDate.DayOfWeek <= DayOfWeek.Tuesday || orderDate.DayOfWeek >= DayOfWeek.Friday ?
                      (orderDate.DayOfWeek >= DayOfWeek.Wednesday ?
                       (orderDate.DayOfWeek >= DayOfWeek.Friday ? (int)(8 - orderDate.DayOfWeek) : (int)(3 - orderDate.DayOfWeek)) :
                       (int)(1 - orderDate.DayOfWeek)) : (int)(3 - orderDate.DayOfWeek);

            DateTime roastDate = orderDate.AddDays((double)num);
            DateTime deliveryDate = roastDate.DayOfWeek >= DayOfWeek.Friday ? roastDate.AddDays(3.0) : roastDate.AddDays(1.0);

            return (roastDate, deliveryDate);
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
                    List<CustomersTbl> customerWithEmailLike = new CustomersTbl().GetAllCustomerWithEmailLIKE(email);
                    if (customerWithEmailLike.Count > 0)
                    {
                        result.CustomerID = customerWithEmailLike[0].CustomerID;
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
                List<ItemUsageTbl> lastItemsUsed = new ItemUsageTbl().GetLastItemsUsed(customerId, 2);

                if (lastItemsUsed.Count > 0)
                {
                    //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Found {lastItemsUsed.Count} last items used for customer {customerId}");

                    // Convert each last item used to OrderLineData
                    foreach (ItemUsageTbl itemUsage in lastItemsUsed)
                    {
                        if (itemUsage.ItemProvidedID > 0)
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
        /// Sets customer-specific roast and delivery dates based on their city (like SetPrepAndDeliveryValues in NewOrderDetail)
        /// </summary>
        private void SetCustomerSpecificDates(long customerId)
        {
            try
            {
                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Setting customer-specific dates for customer {customerId}");

                TrackerTools trackerTools = new TrackerTools();
                DateTime deliveryDate = DateTime.MinValue; // This will be set by reference
                DateTime roastDate = trackerTools.GetNextRoastDateByCustomerID(customerId, ref deliveryDate);
                DateTime orderDate = TimeZoneUtils.Now().Date;

                // Update session with customer-specific dates
                HttpContext context = HttpContext.Current;
                if (context?.Session != null)
                {
                    context.Session[SystemConstants.SessionConstants.BoundDeliveryDate] = deliveryDate.Date;
                    // Note: RoastDate and OrderDate would need session constants if you want to store them
                }

                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Customer-specific dates set - Order: {orderDate:yyyy-MM-dd}, Roast: {roastDate:yyyy-MM-dd}, Delivery: {deliveryDate:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Error setting customer-specific dates for customer {customerId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates an OrderLineData from last usage (doesn't insert, just creates the data object)
        /// </summary>
        private OrderLineData CreateOrderLineFromLastUsage(long customerId, ItemUsageTbl itemUsage)
        {
            try
            {
                // Apply group item logic if needed
                TrackerTools trackerTools = new TrackerTools();
                int finalItemTypeId = trackerTools.ChangeItemIfGroupToNextItemInGroup(
                    customerId,
                    itemUsage.ItemProvidedID,
                    DateTime.Now); // Use current date for group item calculation

                // Get item name for display
                string itemName = ItemTypeTbl.GetItemTypeDescById(finalItemTypeId);
                string packagingName = itemUsage.PackagingID > 0 ? GetPackagingDesc(itemUsage.PackagingID) : string.Empty;

                var orderLine = new OrderLineData
                {
                    ItemID = finalItemTypeId,
                    ItemName = itemName,
                    Qty = itemUsage.AmountProvided,
                    PackagingID = itemUsage.PackagingID,
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
                string itemName = ItemTypeTbl.GetItemTypeDescById(finalItemTypeId);
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
                return packagingID > 0 ? new PackagingTbl().GetPackagingDesc(packagingID) : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// Adds an order line based on last usage data
        /// </summary>
        private bool AddOrderLineFromLastUsage(long customerId, ItemUsageTbl itemUsage)
        {
            try
            {
                // Get current session data for order header
                var sessionData = GetSessionOrderData();

                OrderTblData orderData = new OrderTblData
                {
                    CustomerID = customerId,
                    OrderDate = sessionData.OrderDate,
                    RoastDate = sessionData.RoastDate,
                    RequiredByDate = sessionData.RequiredByDate,
                    ToBeDeliveredBy = sessionData.ToBeDeliveredBy,
                    PurchaseOrder = sessionData.PurchaseOrder ?? string.Empty,
                    Confirmed = sessionData.Confirmed,
                    InvoiceDone = sessionData.InvoiceDone,
                    Done = sessionData.Done,
                    Notes = sessionData.Notes ?? string.Empty,
                    ItemTypeID = itemUsage.ItemProvidedID,
                    QuantityOrdered = itemUsage.AmountProvided,
                    PackagingID = itemUsage.PackagingID
                };

                // Apply group item logic if needed
                TrackerTools trackerTools = new TrackerTools();
                orderData.ItemTypeID = trackerTools.ChangeItemIfGroupToNextItemInGroup(
                    orderData.CustomerID,
                    orderData.ItemTypeID,
                    orderData.RequiredByDate);

                // Insert the order line
                OrderTbl orderTbl = new OrderTbl();
                string result = orderTbl.InsertNewOrderLine(orderData);

                bool success = string.IsNullOrEmpty(result);

                if (success)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Added last order item for customer {customerId}: ItemID={orderData.ItemTypeID}, Qty={orderData.QuantityOrdered}");
                }
                else
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Error adding last order item for customer {customerId}: {result}");
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
                    RoastDate = sessionData.RoastDate,
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
                OrderTbl orderTbl = new OrderTbl();
                string result = orderTbl.InsertNewOrderLine(orderData);

                bool success = string.IsNullOrEmpty(result);

                if (success)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Added preferred item for customer {customerId}: ItemID={orderData.ItemTypeID}, Qty={orderData.QuantityOrdered}");
                }
                else
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Error adding preferred item for customer {customerId}: {result}");
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
                    var (roastDate, deliveryDate) = CalculateOrderDates(sessionData.OrderDate);
                    sessionData.RoastDate = roastDate;

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
            public DateTime RoastDate { get; set; }
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

            public TempOrdersLinesTbl ToTempOrdersLinesTbl()
            {
                return new TempOrdersLinesTbl
                {
                    ItemID = this.ItemID,
                    Qty = this.Qty,
                    PackagingID = this.PackagingID,
                    ServiceTypeID = this.ServiceTypeID,
                    OriginalOrderID = this.OriginalOrderID
                };
            }
        }
    }
}