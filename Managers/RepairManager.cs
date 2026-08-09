using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        private readonly PersonsRepository _personsRepository;
        private readonly EquipTypesRepository _equipTypesRepository = new EquipTypesRepository();

        public RepairManager()
        {
            _repairsRepository = new RepairsRepository();
            _ordersRepository = new OrdersRepository();
            _contactsRepository = new ContactsRepository();
            _repairStatusesRepository = new RepairStatusesRepository();
            _nextPrepDateRepository = new NextPrepDateByAreaRepository();
            _tempOrdersLinesRepository = new TempOrdersLinesRepository();
            _personsRepository = new PersonsRepository();
        }

        public string HandleStatusChange(RepairFormData repair)
        {
            if (!_repairsRepository.UpdateRepair(ToRepair(repair)))
            {
                return MessageProvider.Get(MessageKeys.Repairs.ErrorUpdating);
            }

            // A status change only updates the repair and notifies the contact.
            // Related orders, delivery dates, contact equipment and order notes
            // must not be created or modified from this operation.
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

            // Sundry/walk-in (ZZName) repairs start with a blank contact name — the user types
            // the actual person's name, which also ends up in the related order's notes.
            bool isSundry = contactId == SystemConstants.CustomerConstants.SundryCustomerID;

            var repair = new Repair
            {
                ContactID = contactId,
                ContactName = isSundry ? string.Empty : contact.ContactFirstName ?? string.Empty,
                ContactEmail = isSundry ? string.Empty
                    : (!string.IsNullOrWhiteSpace(contact.EmailAddress) ? contact.EmailAddress : contact.AltEmailAddress),
                EquipTypeID = contact.EquipTypeID,
                EquipSerialNumber = contact.EquipentSN,
                DateLogged = TimeZoneUtils.Now().Date,
                LastStatusChange = TimeZoneUtils.Now(),
                RepairStatusID = 1
            };

            // Returns the actual identity — GetLastIdInserted was ambiguous when the same
            // contact had two repairs on one day (it opened the older repair).
            return _repairsRepository.InsertRepairReturnId(repair);
        }

        /// <summary>
        /// Creates the related order (repair-check line, delivery in 7 days) for a repair that
        /// does not have one yet, and sets repair.RelatedOrderLineID. For the sundry (ZZName)
        /// customer the entered contact name is written to the order notes as "Name:" so the
        /// order/delivery system can identify who the order is for; order creation is deferred
        /// until that name has been entered.
        /// Returns a short user-facing note ("" when nothing was needed).
        /// </summary>
        public string EnsureRelatedOrder(RepairFormData repair)
        {
            if (repair == null)
                return string.Empty;

            // Prefer the DB link over a stale hidden field — otherwise a second Save creates
            // another order/line for the same repair.
            if (repair.RepairID > 0)
            {
                var dbRepair = _repairsRepository.GetRepairById(repair.RepairID);
                if (dbRepair?.RelatedOrderLineID != null && dbRepair.RelatedOrderLineID.Value > 0)
                {
                    repair.RelatedOrderLineID = dbRepair.RelatedOrderLineID.Value;
                    return string.Empty;
                }
            }

            if (repair.RelatedOrderLineID > 0)
                return string.Empty;

            if (repair.RepairStatusID == RepairsRepository.DoneStatusId)
                return string.Empty;

            string sundryBlock = ValidateSundryContactName(repair);
            if (!string.IsNullOrEmpty(sundryBlock))
                return sundryBlock;

            if (!LogNewRepair(repair, true) || repair.RelatedOrderLineID <= 0)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Repairs,
                    $"RepairID {repair.RepairID}: could not create the related order.");
                return "The related order could NOT be created — please check the order manually.";
            }

            // Persist RelatedOrderLineID immediately so a later failure (e.g. email) or a
            // second Save cannot create a duplicate related order.
            if (!_repairsRepository.UpdateRepair(ToRepair(repair)))
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Repairs,
                    $"RepairID {repair.RepairID}: related order line {repair.RelatedOrderLineID} created but could not be saved on the repair.");
                return "Related order was created but could not be linked to the repair — please check manually.";
            }

            SyncRelatedOrderNotes(repair);

            bool isSundry = repair.CustomerID == SystemConstants.CustomerConstants.SundryCustomerID;
            AppLogger.WriteLog(SystemConstants.LogTypes.Repairs,
                $"RepairID {repair.RepairID}: related order line {repair.RelatedOrderLineID} created"
                + (isSundry ? $" for {SystemConstants.CustomerConstants.SundryCustomerName} contact '{repair.ContactName.Trim()}'." : "."));
            return "Related order created (delivery in 7 days).";
        }

        /// <summary>
        /// ZZName (sundry) repairs must have a walk-in contact name before save / related order.
        /// Returns an error message, or null/empty when OK.
        /// </summary>
        public string ValidateSundryContactName(RepairFormData repair)
        {
            if (repair == null)
                return null;

            if (repair.CustomerID != SystemConstants.CustomerConstants.SundryCustomerID)
                return null;

            if (!string.IsNullOrWhiteSpace(repair.ContactName))
            {
                repair.ContactName = repair.ContactName.Trim();
                return null;
            }

            return "Enter the contact's name before saving a "
                + SystemConstants.CustomerConstants.SundryCustomerName
                + " repair — it identifies the order on the delivery sheet.";
        }

        /// <summary>
        /// Rewrites the related order's Notes for this repair: ZZName gets "Name:[RepairStatus: …]";
        /// other contacts keep/merge the [RepairStatus: …] tag. Called when the related order is
        /// created and on every Repair Detail save.
        /// </summary>
        public void SyncRelatedOrderNotes(RepairFormData repair)
        {
            if (repair == null || repair.RelatedOrderLineID <= 0)
                return;

            if (string.IsNullOrEmpty(repair.JobCardNumber))
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

            // Sundry notes are owned by the repair flow — rewrite the whole Notes field so the
            // walk-in name and status comment stay in sync with the repair.
            if (repair.CustomerID == SystemConstants.CustomerConstants.SundryCustomerID)
            {
                string notes = BuildOrderNotesForRepair(repair);
                bool ok = _ordersRepository.UpdateOrderNotes(orderId.Value, notes);
                AppLogger.WriteLog(SystemConstants.LogTypes.Repairs,
                    ok
                        ? $"RepairID {repair.RepairID}: order {orderId.Value} notes set to '{notes}'."
                        : $"RepairID {repair.RepairID}: failed to update order {orderId.Value} notes.");
                return;
            }

            string statusNote = _repairStatusesRepository.GetStatusNote(repair.RepairStatusID);
            if (string.IsNullOrWhiteSpace(statusNote))
                statusNote = "Unknown Status";

            UpdateOrderNotesWithRepairStatus(repair, statusNote.Trim());
        }

        /// <summary>Backward-compatible alias — prefer SyncRelatedOrderNotes.</summary>
        public void SyncRelatedOrderRepairStatusNotes(RepairFormData repair)
        {
            SyncRelatedOrderNotes(repair);
        }

        [DataObjectMethod(DataObjectMethodType.Insert)]
        public bool InsertRepair(RepairFormData repair)
        {
            bool inserted = _repairsRepository.InsertRepair(ToRepair(repair));
            if (inserted)
                EnsureRelatedOrderDeliveryPerson(repair.RelatedOrderLineID);
            return inserted;
        }

        [DataObjectMethod(DataObjectMethodType.Update)]
        public string UpdateRepair(RepairFormData repair, int orig_RepairID)
        {
            repair.RepairID = orig_RepairID;
            repair.LastStatusChange = TimeZoneUtils.Now();
            bool updated = _repairsRepository.UpdateRepair(ToRepair(repair), orig_RepairID);
            if (!updated)
                return MessageProvider.Get(MessageKeys.Repairs.ErrorUpdating);

            EnsureRelatedOrderDeliveryPerson(repair.RelatedOrderLineID);
            return string.Empty;
        }

        [DataObjectMethod(DataObjectMethodType.Delete)]
        public string DeleteRepair(int repairId)
        {
            var repair = _repairsRepository.GetRepairById(repairId);
            int relatedOrderLineId = repair?.RelatedOrderLineID ?? 0;

            // Delete the repair first so RelatedOrderLineID no longer references the line.
            if (!_repairsRepository.DeleteRepair(repairId))
                return "Failed to delete repair";

            DeleteRelatedOrderForRepair(relatedOrderLineId);
            return string.Empty;
        }

        /// <summary>
        /// Removes the order line linked to a deleted repair. If that was the only line
        /// on the order, deletes the order (and any temp-order links) as well.
        /// </summary>
        private void DeleteRelatedOrderForRepair(int relatedOrderLineId)
        {
            if (relatedOrderLineId <= 0)
                return;

            int? orderId = _ordersRepository.GetOrderIdByLineId(relatedOrderLineId);
            if (orderId.HasValue)
            {
                if (_ordersRepository.GetOrderLineCount(orderId.Value) <= 1)
                {
                    _tempOrdersLinesRepository.DeleteByOriginalOrderId(orderId.Value);
                    _ordersRepository.DeleteOrderById(orderId.Value);
                }
                else
                {
                    _ordersRepository.DeleteOrderLineById(relatedOrderLineId);
                }
                return;
            }

            // Legacy: RelatedOrderLineID may still hold an OrderID rather than a line id.
            if (_ordersRepository.OrderExists(relatedOrderLineId))
            {
                _tempOrdersLinesRepository.DeleteByOriginalOrderId(relatedOrderLineId);
                _ordersRepository.DeleteOrderById(relatedOrderLineId);
            }
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
                    Notes = BuildOrderNotesForRepair(repair)
                };

                if (calculateDelivery)
                {
                    var tools = new TrackerTools();
                    orderData.PrepDate = tools.GetNextPreparationDateByCustomerID(repair.CustomerID, ref delivery);
                    var prefs = tools.RetrieveCustomerPrefs(repair.CustomerID);

                    orderData.OrderDate = TimeZoneUtils.Now().Date;
                    orderData.RequiredByDate = delivery;
                    int preferredDeliveryPersonId = prefs.PreferredDeliveryByID;
                    if (preferredDeliveryPersonId > 0
                        && !string.IsNullOrWhiteSpace(_personsRepository.GetPersonNameById(preferredDeliveryPersonId)))
                    {
                        orderData.ToBeDeliveredBy = preferredDeliveryPersonId;
                    }
                    else
                    {
                        orderData.ToBeDeliveredBy =
                            SystemConstants.DeliveryConstants.DefaultDeliveryPersonID;
                        if (preferredDeliveryPersonId > 0)
                        {
                            AppLogger.WriteLog(SystemConstants.LogTypes.Repairs,
                                $"RepairID {repair.RepairID}: preferred delivery person ID {preferredDeliveryPersonId} no longer exists; using default person ID {SystemConstants.DeliveryConstants.DefaultDeliveryPersonID}.");
                        }
                    }

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

                // ZZName (sundry) always gets a brand-new standalone order — never reuse an
                // open order for contact 9, even if another ZZName order exists the same day.
                // Other contacts: reuse an open order for that delivery/prep day (add a line),
                // otherwise create a new order header + line.
                int? existingOrderId =
                    orderData.CustomerID == SystemConstants.CustomerConstants.SundryCustomerID
                        ? null
                        : _ordersRepository.FindOpenOrderIdForContactDay(
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

        /// <summary>
        /// Order notes for a repair's related order. Sundry (ZZName) orders carry the person's
        /// name followed by ": " — the delivery sheet reads the walk-in name from notes
        /// (see DeliverySheetManager). Always includes [RepairStatus: …].
        /// </summary>
        private string BuildOrderNotesForRepair(RepairFormData repair)
        {
            string statusNote = _repairStatusesRepository.GetStatusNote(repair.RepairStatusID);
            if (string.IsNullOrWhiteSpace(statusNote))
                statusNote = "Unknown Status";

            string statusBlock = SystemConstants.RepairConstants.OrderNotesRepairStatusStartTag
                + " " + statusNote.Trim()
                + SystemConstants.RepairConstants.OrderNoteRepairStatusTagEnd;

            if (repair.CustomerID == SystemConstants.CustomerConstants.SundryCustomerID
                && !string.IsNullOrWhiteSpace(repair.ContactName))
            {
                // Space after ':' keeps the delivery-sheet name parse distinct from the
                // colon inside [RepairStatus: …].
                return repair.ContactName.Trim() + ": " + statusBlock;
            }

            return statusBlock;
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

                EnsureRelatedOrderDeliveryPerson(repair.RelatedOrderLineID ?? 0);
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

        private void EnsureRelatedOrderDeliveryPerson(int relatedOrderLineId)
        {
            if (relatedOrderLineId <= 0)
                return;

            int? orderId = _ordersRepository.GetOrderIdByLineId(relatedOrderLineId);
            if (!orderId.HasValue)
                return;

            _ordersRepository.EnsureOrderDeliveryPerson(
                orderId.Value,
                SystemConstants.DeliveryConstants.DefaultDeliveryPersonID);
        }

        private void UpdateOrderNotesWithRepairStatus(RepairFormData repair, string status)
        {
            if (string.IsNullOrEmpty(repair.JobCardNumber))
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
            string notes = order.Notes ?? string.Empty;
            int startIdx = notes.IndexOf(startTag, StringComparison.OrdinalIgnoreCase);
            if (startIdx >= 0)
            {
                int endIdx = notes.IndexOf(SystemConstants.RepairConstants.OrderNoteRepairStatusTagEnd, startIdx);
                if (endIdx > startIdx)
                {
                    string before = notes.Substring(0, startIdx);
                    string after = notes.Substring(endIdx + 1);
                    string newBlock = $"{startTag} {status}{SystemConstants.RepairConstants.OrderNoteRepairStatusTagEnd}";
                    order.Notes = before + newBlock + after;
                }
            }
            else
            {
                order.Notes = notes + $"{startTag} {status}{SystemConstants.RepairConstants.OrderNoteRepairStatusTagEnd}";
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
                RelatedOrderID = repair.RelatedOrderID ?? 0,
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

            // Fill RelatedOrderID in one batch so the repairs list can link R/OLID → Order Detail.
            var lineIds = list
                .Where(r => r.RelatedOrderLineID > 0 && r.RelatedOrderID <= 0)
                .Select(r => r.RelatedOrderLineID)
                .Distinct()
                .ToList();
            if (lineIds.Count == 0)
                return list;

            Dictionary<int, int> orderIdsByLine = new OrdersRepository().GetOrderIdsByLineIds(lineIds);
            var ordersRepo = new OrdersRepository();
            foreach (var item in list)
            {
                if (item.RelatedOrderID > 0 || item.RelatedOrderLineID <= 0)
                    continue;

                int orderId;
                if (orderIdsByLine.TryGetValue(item.RelatedOrderLineID, out orderId))
                {
                    item.RelatedOrderID = orderId;
                }
                else if (ordersRepo.OrderExists(item.RelatedOrderLineID))
                {
                    // Legacy: RelatedOrderLineID still holds an OrderID
                    item.RelatedOrderID = item.RelatedOrderLineID;
                }
            }

            return list;
        }
    }
}
