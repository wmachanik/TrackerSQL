using System;
using System.Collections.Generic;
using System.ComponentModel;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    public class RepairManager
    {
        private readonly RepairsRepository _repairsRepository;
        private readonly OrdersRepository _ordersRepository;
        private readonly ContactsRepository _contactsRepository;
        private readonly RepairStatusesRepository _repairStatusesRepository;
        private readonly NextPrepDateByAreaRepository _nextPrepDateRepository;
        private readonly TempOrdersLinesRepository _tempOrdersLinesRepository;
        private readonly EquipTypesRepository _equipTypesRepository = new EquipTypesRepository();

        public RepairManager()
        {
            _repairsRepository = new RepairsRepository();
            _ordersRepository = new OrdersRepository();
            _contactsRepository = new ContactsRepository();
            _repairStatusesRepository = new RepairStatusesRepository();
            _nextPrepDateRepository = new NextPrepDateByAreaRepository();
            _tempOrdersLinesRepository = new TempOrdersLinesRepository();
        }

        public string HandleStatusChange(RepairFormData repair)
        {
            switch (repair.RepairStatusID)
            {
                case 1:
                    LogNewRepair(repair, true);
                    break;

                case 2:
                    if (repair.RelatedOrderLineID == 0)
                    {
                        LogNewRepair(repair, true);
                    }
                    else
                    {
                        UpdateRelatedOrderDeliveryInc7(repair.RelatedOrderLineID);
                    }
                    break;

                case 3:
                    HandleWorkshopStatus(repair);
                    break;

                case 6:
                    if (repair.RelatedOrderLineID > 0)
                    {
                        var nextDeliveryDate = _nextPrepDateRepository
                            .GetNextDeliveryDateForContact((int)repair.CustomerID);
                        if (nextDeliveryDate.HasValue)
                        {
                            UpdateRelatedOrderDeliveryDate(repair.RelatedOrderLineID, nextDeliveryDate.Value);
                        }
                    }
                    break;

                case 7:
                    CompleteRelatedOrderIfSoleLine(repair.RelatedOrderLineID);
                    break;
            }

            if (!_repairsRepository.UpdateRepair(ToRepair(repair)))
            {
                return MessageProvider.Get(MessageKeys.Repairs.ErrorUpdating);
            }

            string statusNote = _repairStatusesRepository.GetStatusNote(repair.RepairStatusID);
            if (repair.RelatedOrderLineID > 0)
            {
                UpdateOrderNotesWithRepairStatus(repair, statusNote);
            }

            return SendStatusNotification(repair);
        }

        public List<RepairFormData> GetRepairsByDateFilter(string dateFilter, string repairStatus, string sortBy = "DateLogged DESC")
        {
            ResolveDateRange(dateFilter, null, null, out DateTime? fromDate, out DateTime? toDate);
            return ToRepairFormDataList(_repairsRepository.GetRepairsByStatusAndDateRange(
                sortBy, repairStatus, fromDate, toDate, null, null));
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public List<RepairFormData> GetRepairsByStatusAndDateRange(
            string SortBy,
            string repairStatus,
            string dateFilter,
            string customFromDate,
            string customToDate,
            string filterBy,
            string filterText)
        {
            ResolveDateRange(dateFilter, customFromDate, customToDate, out DateTime? fromDate, out DateTime? toDate);
            return ToRepairFormDataList(_repairsRepository.GetRepairsByStatusAndDateRange(
                SortBy, repairStatus, fromDate, toDate, filterBy, filterText));
        }

        /// <summary>
        /// Maps the UI date-filter dropdown (and optional custom From/To) to an inclusive DateLogged range.
        /// </summary>
        private static void ResolveDateRange(
            string dateFilter,
            string customFromDate,
            string customToDate,
            out DateTime? fromDate,
            out DateTime? toDate)
        {
            fromDate = null;
            toDate = null;
            var today = TimeZoneUtils.Now().Date;
            string filter = (dateFilter ?? "All").Trim();

            switch (filter.ToUpperInvariant())
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

                case "CUSTOM":
                    if (!string.IsNullOrWhiteSpace(customFromDate) &&
                        DateTime.TryParse(customFromDate, out DateTime parsedFrom))
                        fromDate = parsedFrom.Date;

                    if (!string.IsNullOrWhiteSpace(customToDate) &&
                        DateTime.TryParse(customToDate, out DateTime parsedTo))
                        toDate = parsedTo.Date;
                    break;

                case "ALL":
                default:
                    break;
            }
        }

        public RepairFormData GetRepairFormDataById(int repairId)
        {
            var repair = _repairsRepository.GetRepairById(repairId);
            return repair != null ? ToRepairFormData(repair) : null;
        }

        public int CreateRepairForContact(int contactId)
        {
            var contact = _contactsRepository.GetById(contactId);
            if (contact == null) return 0;

            var repair = new Repair
            {
                ContactID = contactId,
                ContactName = contact.ContactFirstName ?? string.Empty,
                ContactEmail = !string.IsNullOrWhiteSpace(contact.EmailAddress) ? contact.EmailAddress : contact.AltEmailAddress,
                EquipTypeID = contact.EquipTypeID,
                EquipSerialNumber = contact.EquipentSN,
                DateLogged = TimeZoneUtils.Now().Date,
                LastStatusChange = TimeZoneUtils.Now(),
                RepairStatusID = 1
            };

            if (!_repairsRepository.InsertRepair(repair))
                return 0;

            return _repairsRepository.GetLastIdInserted(contactId);
        }

        [DataObjectMethod(DataObjectMethodType.Insert)]
        public bool InsertRepair(RepairFormData repair)
        {
            return _repairsRepository.InsertRepair(ToRepair(repair));
        }

        [DataObjectMethod(DataObjectMethodType.Update)]
        public string UpdateRepair(RepairFormData repair, int orig_RepairID)
        {
            repair.RepairID = orig_RepairID;
            repair.LastStatusChange = TimeZoneUtils.Now();
            return _repairsRepository.UpdateRepair(ToRepair(repair), orig_RepairID)
                ? string.Empty
                : MessageProvider.Get(MessageKeys.Repairs.ErrorUpdating);
        }

        [DataObjectMethod(DataObjectMethodType.Delete)]
        public string DeleteRepair(int repairId)
        {
            return _repairsRepository.DeleteRepair(repairId) ? string.Empty : "Failed to delete repair";
        }

        private string SendStatusNotification(RepairFormData repair)
        {
            var emailSettings = new EmailSettings();
            emailSettings.SetRecipient(repair.ContactEmail);

            var email = new EmailMailKitCls(emailSettings);
            email.AddSysCCFAddress();
            email.SetEmailSubject(MessageProvider.Get(MessageKeys.Repairs.StatusEmailSubject));

            string statusNote = _repairStatusesRepository.GetStatusNote(repair.RepairStatusID);
            var equipName = _equipTypesRepository.GetEquipTypeName(repair.MachineTypeID);
            string body = MessageProvider.Format(
                MessageKeys.Repairs.StatusEmailBody,
                TrackerTools.SafeString(repair.ContactName, SystemConstants.EmailConstants.DefaultContact),
                TrackerTools.SafeString(equipName, SystemConstants.RepairConstants.DefaultEquipName),
                TrackerTools.SafeString(repair.MachineSerialNumber),
                statusNote,
                TrackerTools.SafeString(repair.JobCardNumber));

            body += MessageProvider.Get(MessageKeys.Repairs.DisclaimerFooter) +
                MessageProvider.Get(MessageProvider.GetEmailSignature());

            email.AddToBody(body);

            if (!email.SendEmail())
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, MessageProvider.Format(
                    MessageKeys.Email.SendError,
                    repair.ContactEmail,
                    email.LastErrorSummary));
                return email.LastErrorSummary;
            }

            return null;
        }

        private bool LogNewRepair(RepairFormData repair, bool calculateDelivery)
        {
            DateTime delivery = TimeZoneUtils.Now().Date.AddDays(7.0);
            int repairItemId = SystemConstants.ItemConstants.RepairCheckItemID;

            if (repair.RelatedOrderLineID == 0)
            {
                var orderData = new OrderTblData
                {
                    CustomerID = repair.CustomerID,
                    ItemTypeID = repairItemId,
                    QuantityOrdered = 1.0,
                    Notes = string.Empty
                };

                if (calculateDelivery)
                {
                    var tools = new TrackerTools();
                    orderData.PrepDate = tools.GetNextPreparationDateByCustomerID(repair.CustomerID, ref delivery);
                    var prefs = tools.RetrieveCustomerPrefs(repair.CustomerID);

                    orderData.OrderDate = TimeZoneUtils.Now().Date;
                    orderData.RequiredByDate = delivery;
                    orderData.ToBeDeliveredBy = prefs.PreferredDeliveryByID;

                    if (prefs.RequiresPurchOrder)
                    {
                        orderData.PurchaseOrder = SystemConstants.UIConstants.PORequiredText;
                    }
                }
                else
                {
                    DateTime today = TimeZoneUtils.Now().Date;
                    orderData.OrderDate = today;
                    orderData.PrepDate = today;
                    orderData.RequiredByDate = delivery;
                }

                int? existingOrderId = _ordersRepository.FindOpenOrderIdForContactDay(
                    orderData.CustomerID,
                    orderData.RequiredByDate,
                    orderData.PrepDate);

                int lineId;
                if (existingOrderId.HasValue && existingOrderId.Value > 0)
                {
                    lineId = _ordersRepository.AddLineToExistingOrder(existingOrderId.Value, orderData);
                }
                else
                {
                    int orderId = _ordersRepository.InsertOrderHeader(orderData);
                    if (orderId <= 0)
                        return false;

                    lineId = _ordersRepository.InsertOrderLine(
                        orderId,
                        orderData.ItemTypeID,
                        orderData.QuantityOrdered,
                        orderData.PrepTypeID,
                        orderData.PackagingID);
                }

                repair.RelatedOrderLineID = lineId > 0 ? lineId : 0;
            }
            else if (calculateDelivery)
            {
                DateTime newDelivery = TimeZoneUtils.Now().Date.AddDays(7.0);
                UpdateRelatedOrderDeliveryDate(repair.RelatedOrderLineID, newDelivery);
            }

            return true;
        }

        private void HandleWorkshopStatus(RepairFormData repair)
        {
            if (repair.RelatedOrderLineID == 0)
            {
                LogNewRepair(repair, false);
            }
            else
            {
                UpdateRelatedOrderDeliveryInc7(repair.RelatedOrderLineID);
            }

            if (!string.IsNullOrEmpty(repair.MachineSerialNumber))
            {
                _contactsRepository.SetEquipmentIfEmpty(
                    repair.MachineTypeID,
                    repair.MachineSerialNumber,
                    (int)repair.CustomerID);
            }
        }

        public void SetStatusDoneByTempOrder()
        {
            var tempOrders = _repairsRepository.GetListOfRelatedTempOrders();

            foreach (var repair in tempOrders)
            {
                var repairTbl = ToRepairFormData(repair);
                int? orderId = _ordersRepository.GetOrderIdByLineId(repairTbl.RelatedOrderLineID);

                if (repairTbl.RepairStatusID <= 3)
                {
                    if (orderId.HasValue)
                    {
                        _tempOrdersLinesRepository.DeleteByOriginalOrderId(orderId.Value);
                        _ordersRepository.UpdateIncDeliveryDateBy7(orderId.Value);
                    }
                }
                else
                {
                    repair.RepairStatusID = 7;
                    _repairsRepository.UpdateRepair(repair);
                }
            }
        }

        private void CompleteRelatedOrderIfSoleLine(int relatedOrderLineId)
        {
            if (relatedOrderLineId <= 0)
                return;

            int? orderId = _ordersRepository.GetOrderIdByLineId(relatedOrderLineId);
            if (!orderId.HasValue)
                return;

            // Only mark the order Done when this repair line is the sole line.
            if (_ordersRepository.GetOrderLineCount(orderId.Value) <= 1)
            {
                _ordersRepository.UpdateSetDoneById(true, orderId.Value);
            }
        }

        private void UpdateRelatedOrderDeliveryInc7(int relatedOrderLineId)
        {
            int? orderId = _ordersRepository.GetOrderIdByLineId(relatedOrderLineId);
            if (orderId.HasValue)
                _ordersRepository.UpdateIncDeliveryDateBy7(orderId.Value);
        }

        private void UpdateRelatedOrderDeliveryDate(int relatedOrderLineId, DateTime newDate)
        {
            int? orderId = _ordersRepository.GetOrderIdByLineId(relatedOrderLineId);
            if (orderId.HasValue)
                _ordersRepository.UpdateOrderDeliveryDate(newDate, orderId.Value);
        }

        private void UpdateOrderNotesWithRepairStatus(RepairFormData repair, string status)
        {
            if (repair.JobCardNumber.Equals(string.Empty))
            {
                repair.JobCardNumber = "n/a";
                AppLogger.WriteLog(SystemConstants.LogTypes.Repairs,
                    $"Repair with related order line id: {repair.RelatedOrderLineID}, has not Job Card number set!");
            }

            int? orderId = _ordersRepository.GetOrderIdByLineId(repair.RelatedOrderLineID);
            if (!orderId.HasValue)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Repairs,
                    $"Repair with related order line id: {repair.RelatedOrderLineID}, order not found!");
                return;
            }

            var order = _ordersRepository.GetOrderTblDataById(orderId.Value);
            if (order == null)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Repairs,
                    $"Repair with related order line id: {repair.RelatedOrderLineID}, not found in OrdersTbl!");
                return;
            }

            string startTag = SystemConstants.RepairConstants.OrderNotesRepairStatusStartTag;
            int startIdx = order.Notes?.IndexOf(startTag, StringComparison.OrdinalIgnoreCase) ?? -1;
            if (startIdx >= 0)
            {
                int endIdx = order.Notes.IndexOf(SystemConstants.RepairConstants.OrderNoteRepairStatusTagEnd, startIdx);
                if (endIdx > startIdx)
                {
                    string before = order.Notes.Substring(0, startIdx);
                    string after = order.Notes.Substring(endIdx + 1);
                    string newBlock = $"{startTag} {status}{SystemConstants.RepairConstants.OrderNoteRepairStatusTagEnd}";
                    order.Notes = before + newBlock + after;
                }
            }
            else
            {
                order.Notes += $"{startTag} {status}{SystemConstants.RepairConstants.OrderNoteRepairStatusTagEnd}";
            }

            bool success = _ordersRepository.UpdateOrderNotes(orderId.Value, order.Notes);
            if (success)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Repairs,
                    $"Repair with related order line id: {repair.RelatedOrderLineID}, status changed to {status}.");
            }
            else
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Repairs,
                    $"Repair with related order line id: {repair.RelatedOrderLineID}, status update failed.");
            }
        }

        private static Repair ToRepair(RepairFormData repair)
        {
            return new Repair
            {
                RepairID = repair.RepairID,
                ContactID = (int)repair.CustomerID,
                ContactName = repair.ContactName,
                ContactEmail = repair.ContactEmail,
                JobCardNumber = repair.JobCardNumber,
                DateLogged = repair.DateLogged,
                LastStatusChange = repair.LastStatusChange,
                EquipTypeID = repair.MachineTypeID,
                EquipSerialNumber = repair.MachineSerialNumber,
                SwopOutMachineID = repair.SwopOutMachineID,
                EquipConditionID = repair.MachineConditionID,
                TakenFrother = repair.TakenFrother,
                TakenBeanLid = repair.TakenBeanLid,
                TakenWaterLid = repair.TakenWaterLid,
                BrokenFrother = repair.BrokenFrother,
                BrokenBeanLid = repair.BrokenBeanLid,
                BrokenWaterLid = repair.BrokenWaterLid,
                RepairFaultID = repair.RepairFaultID,
                RepairFaultDesc = repair.RepairFaultDesc,
                RepairStatusID = repair.RepairStatusID,
                RelatedOrderLineID = repair.RelatedOrderLineID,
                Notes = repair.Notes
            };
        }

        private static RepairFormData ToRepairFormData(Repair repair)
        {
            return new RepairFormData
            {
                RepairID = repair.RepairID,
                CustomerID = repair.ContactID,
                ContactName = repair.ContactName ?? string.Empty,
                ContactEmail = repair.ContactEmail ?? string.Empty,
                JobCardNumber = repair.JobCardNumber ?? string.Empty,
                DateLogged = repair.DateLogged ?? DateTime.MinValue,
                LastStatusChange = repair.LastStatusChange ?? TimeZoneUtils.Now(),
                MachineTypeID = repair.EquipTypeID ?? 0,
                MachineSerialNumber = repair.EquipSerialNumber ?? string.Empty,
                SwopOutMachineID = repair.SwopOutMachineID ?? 0,
                MachineConditionID = repair.EquipConditionID ?? 0,
                TakenFrother = repair.TakenFrother ?? false,
                TakenBeanLid = repair.TakenBeanLid ?? true,
                TakenWaterLid = repair.TakenWaterLid ?? true,
                BrokenFrother = repair.BrokenFrother ?? false,
                BrokenBeanLid = repair.BrokenBeanLid ?? false,
                BrokenWaterLid = repair.BrokenWaterLid ?? false,
                RepairFaultID = repair.RepairFaultID ?? 0,
                RepairFaultDesc = repair.RepairFaultDesc ?? string.Empty,
                RepairStatusID = repair.RepairStatusID ?? 0,
                RelatedOrderLineID = repair.RelatedOrderLineID ?? 0,
                Notes = repair.Notes ?? string.Empty
            };
        }

        private static List<RepairFormData> ToRepairFormDataList(List<Repair> repairs)
        {
            var list = new List<RepairFormData>();
            if (repairs == null) return list;

            foreach (var repair in repairs)
            {
                list.Add(ToRepairFormData(repair));
            }

            return list;
        }
    }
}
