using System;
using System.Collections.Generic;
using System.ComponentModel;
using TrackerDotNet.Classes;
using TrackerDotNet.Controls;

namespace TrackerDotNet.Managers
{
    public class RepairManager
    {
        private readonly RepairsTbl _repairsTbl;
        private readonly OrderTbl _orderTbl;
        private readonly CustomersTbl _customersTbl;

        public RepairManager()
        {
            _repairsTbl = new RepairsTbl();
            _orderTbl = new OrderTbl();
            _customersTbl = new CustomersTbl();
        }

        public string HandleStatusChange(RepairsTbl repair)
        {
            bool orderUpdated = true;

            switch (repair.RepairStatusID)
            {
                case 1: // LOGGED
                    orderUpdated = LogNewRepair(repair, true);
                    break;

                case 2: // COLLECTED
                    if (repair.RelatedOrderID == 0)
                    {
                        orderUpdated = LogNewRepair(repair, true);
                    }
                    else
                    {
                        _orderTbl.UpdateIncDeliveryDateBy7(repair.RelatedOrderID);
                    }
                    break;

                case 3: // WORKSHOP
                    HandleWorkshopStatus(repair);
                    break;

                case 6: // READY
                    if (repair.RelatedOrderID > 0)
                    {
                        var nextDeliveryDate = new NextRoastDateByCityTbl()
                            .GetNextDeliveryDate(repair.CustomerID);
                        _orderTbl.UpdateOrderDeliveryDate(nextDeliveryDate, repair.RelatedOrderID);
                    }
                    break;

                case 7: // DONE
                    if (repair.RelatedOrderID > 0)
                    {
                        _orderTbl.UpdateSetDoneByID(true, repair.RelatedOrderID);
                    }
                    break;
            }

            bool updateSuccess = string.IsNullOrEmpty(_repairsTbl.UpdateRepair(repair));
            if (!updateSuccess)
                return MessageProvider.Get(MessageKeys.Repairs.ErrorUpdating);

            // Update order notes with the status note from the database
            string statusNote = new RepairStatusesTbl().GetStatusNote(repair.RepairStatusID);
            if (repair.RelatedOrderID > 0)
            {
                RepairsTbl.UpdateOrderNotesWithRepairStatus(repair, statusNote);
            }

            string emailError = SendStatusNotification(repair);

            if (!string.IsNullOrEmpty(emailError))
                return emailError;

            return null; // Success
        }

        public List<RepairsTbl> GetRepairsByDateFilter(string dateFilter, string repairStatus, string sortBy = "DateLogged DESC")
        {
            DateTime? fromDate = null;
            DateTime? toDate = null;

            var today = TimeZoneUtils.Now().Date;

            switch (dateFilter?.ToUpper())
            {
                case "THISWEEK":
                    var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                    fromDate = startOfWeek;
                    toDate = startOfWeek.AddDays(6);
                    break;

                case "LASTWEEK":
                    var lastWeekStart = today.AddDays(-(int)today.DayOfWeek - 7);
                    fromDate = lastWeekStart;
                    toDate = lastWeekStart.AddDays(6);
                    break;

                case "THISMONTH":
                    fromDate = new DateTime(today.Year, today.Month, 1);
                    toDate = fromDate.Value.AddMonths(1).AddDays(-1);
                    break;

                case "LASTMONTH":
                    var lastMonthStart = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                    fromDate = lastMonthStart;
                    toDate = lastMonthStart.AddMonths(1).AddDays(-1);
                    break;

                case "ALL":
                case null:
                case "":
                    // No date filtering
                    break;

                default:
                    // For custom date ranges, dates should be passed separately
                    break;
            }

            return _repairsTbl.GetRepairsByStatusAndDateRange(sortBy, repairStatus, fromDate, toDate, null, null);
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public List<RepairsTbl> GetRepairsByStatusAndDateRange(string sortBy, string repairStatus, object fromDateObj, object toDateObj, string filterBy, string filterText)
        {
            // Convert objects to DateTime? and call the main method
            DateTime? fromDate = TrackerTools.ConvertToNullableDateTime(fromDateObj);
            DateTime? toDate = TrackerTools.ConvertToNullableDateTime(toDateObj);
            return _repairsTbl.GetRepairsByStatusAndDateRange(sortBy, repairStatus, fromDate, toDate, null, null);
        }
        private string SafeString(string value) => string.IsNullOrWhiteSpace(value) ? "n/a" : value;

        private string SendStatusNotification(RepairsTbl repair)
        {
            var equipTypeTbl = new EquipTypeTbl();
            var repairStatusesTbl = new RepairStatusesTbl();

            var emailSettings = new EmailSettings();
            emailSettings.SetRecipient(repair.ContactEmail);

            var email = new EmailMailKitCls(emailSettings);
            email.AddSysCCFAddress();

            email.SetEmailSubject(MessageProvider.Get(MessageKeys.Repairs.StatusEmailSubject));

            // Get the status note from the database
            string statusNote = repairStatusesTbl.GetStatusNote(repair.RepairStatusID);

            // Format the email body using Messages.resx template

            // Usage in SendStatusNotification:
            string body = MessageProvider.Format(
                MessageKeys.Repairs.StatusEmailBody,
                TrackerTools.SafeString(repair.ContactName, SystemConstants.EmailConstants.DefaultContact),  // {0} = contact name
                TrackerTools.SafeString(equipTypeTbl.GetEquipName(repair.MachineTypeID), SystemConstants.RepairConstants.DefaultEquipName ),   // {1} the equipment being repaired
                TrackerTools.SafeString(repair.MachineSerialNumber),                                         // {2} = equipment serial number
                statusNote,
                TrackerTools.SafeString(repair.JobCardNumber)                                                // {4} the job card assocaited to this repair
            );

            body += MessageProvider.Get(MessageKeys.Repairs.DisclaimerFooter) +
                MessageProvider.Get(MessageProvider.GetEmailSignature());

            email.AddToBody(body);

            bool success = email.SendEmail();

            if (!success)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, MessageProvider.Format(
                    MessageKeys.Email.SendError,
                    repair.ContactEmail,
                    email.LastErrorSummary));
            }

            return success ? null : email.LastErrorSummary;
        }

        private bool LogNewRepair(RepairsTbl repair, bool calculateDelivery)
        {

            DateTime delivery = TimeZoneUtils.Now().Date.AddDays(7.0);

            if (repair.RelatedOrderID == 0)
            {
                // Create new order
                var orderData = new OrderTblData
                {
                    CustomerID = repair.CustomerID,
                    ItemTypeID = 36,
                    QuantityOrdered = 1.0,
                    Notes = string.Empty
                };

                if (calculateDelivery)
                {
                    var tools = new TrackerTools();
                    orderData.RoastDate = tools.GetNextRoastDateByCustomerID(repair.CustomerID, ref delivery);
                    var prefs = tools.RetrieveCustomerPrefs(repair.CustomerID);

                    orderData.OrderDate = TimeZoneUtils.Now().Date;
                    orderData.RequiredByDate = delivery;
                    orderData.ToBeDeliveredBy = prefs.PreferredDeliveryByID;

                    if (prefs.RequiresPurchOrder)
                        orderData.PurchaseOrder = SystemConstants.UIConstants.PORequiredText;
                }
                else
                {
                    DateTime today = TimeZoneUtils.Now().Date;
                    orderData.OrderDate = today;
                    orderData.RoastDate = today;
                    orderData.RequiredByDate = delivery;
                }

                _orderTbl.InsertNewOrderLine(orderData);
                repair.RelatedOrderID = _orderTbl.GetLastOrderAdded(
                    orderData.CustomerID,
                    orderData.OrderDate,
                    36);
            }
            else
            {
                // Optionally update delivery date or other fields if needed
                if (calculateDelivery)
                {
                    var tools = new TrackerTools();
                    var prefs = tools.RetrieveCustomerPrefs(repair.CustomerID);
                    DateTime newDelivery = TimeZoneUtils.Now().Date.AddDays(7.0);
                    _orderTbl.UpdateOrderDeliveryDate(newDelivery, repair.RelatedOrderID);
                }
            }
            // UpdateOrderNotesWithRepairStatus - > done later

            return true;
        }

        private void HandleWorkshopStatus(RepairsTbl repair)
        {
            if (repair.RelatedOrderID == 0)
            {
                LogNewRepair(repair, false);
            }
            else
            {
                _orderTbl.UpdateIncDeliveryDateBy7(repair.RelatedOrderID);
            }

            if (!string.IsNullOrEmpty(repair.MachineSerialNumber))
            {
                _customersTbl.SetEquipDetailsIfEmpty(
                    repair.MachineTypeID,
                    repair.MachineSerialNumber,
                    repair.CustomerID);
            }
        }

        public void SetStatusDoneByTempOrder()
        {
            var repairsTbl = new RepairsTbl();
            var tempOrders = repairsTbl.GetListOfRelatedTempOrders();

            if (tempOrders.Count > 0)
            {
                var tempOrdersLinesTbl = new TempOrdersLinesTbl();
                var orderTbl = new OrderTbl();

                foreach (var repair in tempOrders)
                {
                    if (repair.RepairStatusID <= 3)
                    {
                        // For repairs in early stages, delete temp order and extend delivery date
                        tempOrdersLinesTbl.DeleteByOriginalID(repair.RelatedOrderID);
                        orderTbl.UpdateIncDeliveryDateBy7(repair.RelatedOrderID);
                    }
                    else
                    {
                        // For repairs in later stages, mark as done
                        repair.RepairStatusID = 7; // Done status
                        repairsTbl.UpdateRepair(repair);
                    }
                }
            }
        }

        // Other business logic methods...
    }
}