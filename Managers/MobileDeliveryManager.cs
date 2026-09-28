using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Delivery sheet for the driver app and offline delivery sync (received-by name, signature, outcome).
    /// By default a synced delivery is stored as proof for the office to complete (Order Done) in the web app;
    /// System → Driver App can run the Order Done steps immediately and email the client a confirmation.
    /// </summary>
    public class MobileDeliveryManager
    {
        public const string OutcomeDelivered = "Delivered";
        public const string OutcomeNotDelivered = "NotDelivered";

        public const string StatusReceived = "Received";
        public const string StatusCompleted = "Completed";
        public const string StatusCompleteFailed = "CompleteFailed";
        public const string StatusAlreadyDone = "AlreadyDone";

        private const int MaxSignatureBytes = 256 * 1024;
        private const int MaxBatch = 100;

        private readonly MobileApiRepository _repo = new MobileApiRepository();

        public List<MobileDeliveryDate> GetDates(MobileTokenUser user)
        {
            MobileApiSchemaInstaller.EnsureReady();
            return _repo.GetDeliveryDates(TimeZoneUtils.Now().Date, user?.PersonId);
        }

        public MobileDeliverySheet GetSheet(DateTime date, int? personId)
        {
            MobileApiSchemaInstaller.EnsureReady();
            var sheet = new MobileDeliverySheet
            {
                Date = date.ToString("yyyy-MM-dd"),
                PersonId = personId,
                GeneratedAt = TimeZoneUtils.Now().ToString("yyyy-MM-ddTHH:mm:ss")
            };

            MobileDeliveryStop stop = null;
            foreach (var row in _repo.GetSheetRows(date, personId))
            {
                if (stop == null || stop.OrderId != row.OrderID)
                {
                    stop = BuildStop(row);
                    sheet.Orders.Add(stop);
                }

                stop.Items.Add(new MobileDeliveryItem
                {
                    LineId = row.OrderLineID,
                    Qty = Math.Round(row.Qty, SystemConstants.DatabaseConstants.NumDecimalPoints),
                    Item = NullIfEmpty(row.ItemDesc),
                    Pack = NullIfEmpty(row.PackDesc),
                    Repair = row.ItemServiceTypeID == SystemConstants.ServiceTypeConstants.Service
                             || row.ItemID == SystemConstants.ItemConstants.RepairCheckItemID
                });
            }

            if (sheet.Orders.Count > 0)
            {
                var stops = sheet.Orders.ToDictionary(o => o.OrderId);
                foreach (var pair in _repo.GetSheetRepairs(date, personId))
                {
                    if (!stops.TryGetValue(pair.Key, out MobileDeliveryStop owner))
                        continue;
                    if (owner.Repairs == null)
                        owner.Repairs = new List<MobileStopRepair>();
                    owner.Repairs.Add(pair.Value);
                }
            }
            return sheet;
        }

        private static MobileDeliveryStop BuildStop(MobileApiRepository.SheetRow row)
        {
            bool sundry = row.ContactID == SystemConstants.CustomerConstants.SundryCustomerID;
            string contactName = sundry
                ? SundryNameFromNotes(row.Notes)
                : (row.FirstName + " " + row.LastName).Trim();

            return new MobileDeliveryStop
            {
                OrderId = row.OrderID,
                ContactId = row.ContactID,
                Company = row.CompanyName,
                ContactName = NullIfEmpty(contactName),
                Phone = NullIfEmpty(row.Phone),
                Cell = NullIfEmpty(row.Cell),
                Address = NullIfEmpty(row.Address),
                Province = NullIfEmpty(row.Province),
                PostalCode = NullIfEmpty(row.PostalCode),
                Area = NullIfEmpty(row.AreaName),
                Notes = NullIfEmpty(row.Notes),
                Po = NullIfEmpty(row.PurchaseOrder),
                Done = row.Done,
                Confirmed = row.Confirmed,
                Seq = row.DeliveryOrder,
                DeliveryById = row.DeliveryById,
                DeliveryBy = NullIfEmpty(row.DeliveryBy),
                Proof = row.ProofID.HasValue
                    ? new MobileDeliveryProofSummary
                    {
                        ProofId = row.ProofID.Value,
                        Outcome = row.ProofOutcome,
                        Reason = NullIfEmpty(row.ProofReason),
                        ReceivedBy = NullIfEmpty(row.ProofReceivedBy),
                        MissingItems = NullIfEmpty(row.ProofMissingItems),
                        DeliveredAt = row.ProofDeliveredAt?.ToString("yyyy-MM-ddTHH:mm:ss"),
                        Status = row.ProofStatus,
                        HasSignature = row.ProofHasSignature
                    }
                    : null
            };
        }

        /// <summary>ZZName walk-in orders carry the person's name in the notes before the first colon.</summary>
        private static string SundryNameFromNotes(string notes)
        {
            string text = (notes ?? string.Empty).Trim();
            int colon = text.IndexOf(':');
            return colon > 0 ? text.Substring(0, colon).Trim() : text;
        }

        public List<MobileDeliverySyncResult> Sync(MobileDeliverySyncRequest request, MobileTokenUser user, out string error)
        {
            error = null;
            var results = new List<MobileDeliverySyncResult>();
            var items = request?.Deliveries;
            if (items == null || items.Count == 0)
            {
                error = "No deliveries were sent.";
                return results;
            }
            if (items.Count > MaxBatch)
            {
                error = "Send at most " + MaxBatch + " deliveries per request.";
                return results;
            }

            MobileApiSchemaInstaller.EnsureReady();
            foreach (var item in items)
            {
                MobileDeliverySyncResult result;
                try
                {
                    result = SyncOne(item, user);
                }
                catch (Exception ex)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Delivery,
                        "Mobile delivery sync failed for order " + item?.OrderId + ": " + ex.Message, user?.UserName);
                    result = Fail(item, "Server error: " + ex.Message);
                }
                results.Add(result);
            }
            return results;
        }

        private MobileDeliverySyncResult SyncOne(MobileDeliverySyncItem item, MobileTokenUser user)
        {
            if (item == null)
                return Fail(null, "Empty delivery.");

            string clientRef = (item.ClientRef ?? string.Empty).Trim();
            if (clientRef.Length == 0 || clientRef.Length > 64)
                return Fail(item, "clientRef is required (up to 64 characters, e.g. a GUID created on the phone).");

            var existing = _repo.GetProofByClientRef(clientRef);
            if (existing != null)
                return Duplicate(item, existing);

            if (item.OrderId <= 0)
                return Fail(item, "orderId is required.");

            var order = _repo.GetOrderBrief(item.OrderId);
            if (order == null)
                return Fail(item, "Order " + item.OrderId + " was not found.");

            string outcome = NormaliseOutcome(item.Outcome);
            if (outcome == null)
                return Fail(item, "outcome must be \"Delivered\" or \"NotDelivered\".");

            byte[] signature = null;
            string signatureType = null;
            if (!string.IsNullOrWhiteSpace(item.Signature)
                && !TryDecodeSignature(item.Signature, out signature, out signatureType, out string signatureError))
                return Fail(item, signatureError);

            string receivedBy = (item.ReceivedBy ?? string.Empty).Trim();
            string reason = (item.Reason ?? string.Empty).Trim();
            string missingItems = (item.MissingItems ?? string.Empty).Trim();
            if (outcome == OutcomeDelivered && receivedBy.Length == 0 && signature == null && reason.Length == 0)
                return Fail(item, "A delivered order needs how it was delivered, the received-by name or a signature.");
            if (outcome == OutcomeNotDelivered && reason.Length == 0 && string.IsNullOrWhiteSpace(item.Note))
                return Fail(item, "Say why it was not delivered.");

            var proof = new MobileDeliveryProof
            {
                ClientRef = clientRef,
                OrderID = order.OrderID,
                ContactID = order.ContactID > 0 ? order.ContactID : (int?)null,
                PersonID = user?.PersonId,
                UserName = user?.UserName,
                Outcome = outcome,
                Reason = NullIfEmpty(reason),
                ReceivedByName = receivedBy,
                SignatureContentType = signatureType,
                DriverNote = item.Note,
                MissingItems = outcome == OutcomeDelivered ? NullIfEmpty(missingItems) : null,
                DeliveredAt = ParseDeliveredAt(item.DeliveredAt),
                Latitude = ValidCoordinate(item.Lat, 90),
                Longitude = ValidCoordinate(item.Lng, 180),
                Status = outcome == OutcomeDelivered && order.Done ? StatusAlreadyDone : StatusReceived,
                StatusMessage = outcome == OutcomeDelivered && order.Done ? "The order was already marked done in Tracker." : null
            };

            long proofId;
            try
            {
                proofId = _repo.InsertProof(proof, signature);
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                var raced = _repo.GetProofByClientRef(clientRef);
                if (raced != null)
                    return Duplicate(item, raced);
                throw;
            }

            AppLogger.WriteLog(SystemConstants.LogTypes.Delivery,
                "Order " + order.OrderID + " | Contact=" + order.ContactID + " | Mobile " + outcome
                + (reason.Length > 0 ? " | " + reason : string.Empty)
                + (receivedBy.Length > 0 ? " | received by '" + receivedBy + "'" : string.Empty)
                + (signature != null ? " | signature" : string.Empty)
                + (proof.MissingItems != null ? " | not handed over: " + proof.MissingItems : string.Empty)
                + " | at " + proof.DeliveredAt.ToString("yyyy-MM-dd HH:mm"), user?.UserName);

            var result = new MobileDeliverySyncResult
            {
                ClientRef = clientRef,
                OrderId = order.OrderID,
                Ok = true,
                ProofId = proofId,
                Status = proof.Status,
                Message = proof.StatusMessage ?? (outcome == OutcomeDelivered
                    ? (proof.MissingItems != null
                        ? "Delivery received with items still to follow; the office will sort out the order."
                        : "Delivery received; the office will complete the order.")
                    : "Not-delivered report received.")
            };

            var settings = Settings;
            bool sendConfirmation = outcome == OutcomeDelivered && settings.SendDeliveryConfirmation;

            // A short delivery is left for the office: Order Done would close items that are still to follow.
            if (outcome == OutcomeDelivered && !order.Done && proof.MissingItems == null && settings.RunDoneOnDelivery)
            {
                bool completed = TryCompleteOrder(order, proof.DeliveredAt, !sendConfirmation, out string message);
                string status = completed ? StatusCompleted : StatusCompleteFailed;
                _repo.UpdateProofStatus(proofId, status, message, user?.UserName);
                result.Status = status;
                result.Message = message;
            }

            var emails = new MobileDeliveryEmailManager();
            if (sendConfirmation)
            {
                string why = TrySend(() => emails.SendConfirmation(order, proof, signature, signatureType, item.Lines));
                result.Message += why == null ? " Confirmation emailed to the client." : " No confirmation email: " + why + ".";
            }

            if (item.NoteToOffice && !string.IsNullOrWhiteSpace(proof.DriverNote))
            {
                string driver = user?.PersonId != null ? new PersonsRepository().GetPersonNameById(user.PersonId.Value) : null;
                string why = TrySend(() => emails.SendNoteToOffice(order, proof, string.IsNullOrWhiteSpace(driver) ? user?.UserName : driver));
                result.Message += why == null ? " Note emailed to the office." : " Note not emailed: " + why + ".";
            }

            return result;
        }

        /// <summary>Overridable for tests; System → Driver App settings.</summary>
        protected virtual MobileApiSettings Settings => MobileApiSettingsManager.Current;

        private static string TrySend(Func<string> send)
        {
            try
            {
                return send();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, "Driver app email failed: " + ex.Message);
                return "the email failed (" + ex.Message + ")";
            }
        }

        /// <summary>Runs the web Delivery Sheet "Done" steps: temp order from the order, then Order Done (optionally with the delivered email).</summary>
        private static bool TryCompleteOrder(MobileApiRepository.OrderBrief order, DateTime deliveredAt, bool sendDeliveredEmail, out string message)
        {
            try
            {
                if (!new OrderManager().CompleteOrderDeliveryByOrderId(order.OrderID))
                {
                    TempOrderSession.CleanupCurrentTempOrder();
                    message = "Delivery saved, but the order could not be prepared for Order Done; complete it in Tracker.";
                    return false;
                }

                var done = OrderDoneManager.CompleteOrder(
                    order.ContactID, deliveredAt.Date, null, null, sendDeliveredEmail ? MessageKeys.Order.StatusDelivered : null);
                if (!done.Success)
                {
                    TempOrderSession.CleanupCurrentTempOrder();
                    message = "Delivery saved, but Order Done failed: " + done.Message;
                    return false;
                }

                message = done.Message;
                return true;
            }
            catch (Exception ex)
            {
                try { TempOrderSession.CleanupCurrentTempOrder(); } catch { }
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                    "Mobile auto-complete failed for order " + order.OrderID + ": " + ex.Message);
                message = "Delivery saved, but Order Done failed: " + ex.Message;
                return false;
            }
        }

        public List<MobileProofView> ListProofs(DateTime? date, int? orderId)
        {
            MobileApiSchemaInstaller.EnsureReady();
            var people = new PersonsRepository();
            var names = new Dictionary<int, string>();
            return _repo.ListProofs(date, orderId).Select(p =>
            {
                string driver = p.UserName;
                if (p.PersonID.HasValue)
                {
                    if (!names.TryGetValue(p.PersonID.Value, out string name))
                        names[p.PersonID.Value] = name = people.GetPersonNameById(p.PersonID.Value);
                    if (!string.IsNullOrWhiteSpace(name))
                        driver = name;
                }
                return new MobileProofView
                {
                    ProofId = p.ProofID,
                    OrderId = p.OrderID,
                    ContactId = p.ContactID,
                    Company = NullIfEmpty(p.CompanyName),
                    Driver = NullIfEmpty(driver),
                    Outcome = p.Outcome,
                    Reason = NullIfEmpty(p.Reason),
                    ReceivedBy = NullIfEmpty(p.ReceivedByName),
                    Note = NullIfEmpty(p.DriverNote),
                    MissingItems = NullIfEmpty(p.MissingItems),
                    DeliveredAt = p.DeliveredAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    ReceivedAt = p.ReceivedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    Status = p.Status,
                    StatusMessage = NullIfEmpty(p.StatusMessage),
                    HasSignature = p.HasSignature,
                    Lat = p.Latitude,
                    Lng = p.Longitude
                };
            }).ToList();
        }

        public bool TryGetSignature(long proofId, out byte[] image, out string contentType)
        {
            MobileApiSchemaInstaller.EnsureReady();
            return _repo.TryGetSignature(proofId, out image, out contentType);
        }

        private static string NormaliseOutcome(string outcome)
        {
            string value = (outcome ?? string.Empty).Replace(" ", string.Empty).Replace("-", string.Empty).Trim();
            if (value.Length == 0 || value.Equals(OutcomeDelivered, StringComparison.OrdinalIgnoreCase))
                return OutcomeDelivered;
            if (value.Equals(OutcomeNotDelivered, StringComparison.OrdinalIgnoreCase))
                return OutcomeNotDelivered;
            return null;
        }

        private static bool TryDecodeSignature(string raw, out byte[] bytes, out string contentType, out string error)
        {
            bytes = null;
            contentType = null;
            error = null;

            string data = raw.Trim();
            int comma = data.IndexOf(',');
            if (data.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && comma > 0)
                data = data.Substring(comma + 1);

            if (data.Length > MaxSignatureBytes * 4 / 3 + 8)
            {
                error = "The signature image is too large (max " + (MaxSignatureBytes / 1024) + " KB).";
                return false;
            }

            try
            {
                bytes = Convert.FromBase64String(data);
            }
            catch (FormatException)
            {
                error = "The signature is not valid base64.";
                return false;
            }

            if (bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
                contentType = "image/png";
            else if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                contentType = "image/jpeg";
            else if (bytes.Length >= 12 && bytes[0] == 'R' && bytes[1] == 'I' && bytes[2] == 'F' && bytes[3] == 'F'
                     && bytes[8] == 'W' && bytes[9] == 'E' && bytes[10] == 'B' && bytes[11] == 'P')
                contentType = "image/webp";
            else
            {
                bytes = null;
                error = "The signature must be a PNG, JPEG or WebP image.";
                return false;
            }
            return true;
        }

        /// <summary>ISO time from the phone. With an offset (or Z) it is converted to Tracker time; without one it is taken as Tracker time.</summary>
        private static DateTime ParseDeliveredAt(string value)
        {
            DateTime now = TimeZoneUtils.Now();
            if (string.IsNullOrWhiteSpace(value))
                return now;

            string text = value.Trim();
            bool hasOffset = text.EndsWith("Z", StringComparison.OrdinalIgnoreCase)
                || System.Text.RegularExpressions.Regex.IsMatch(text, @"[+-]\d{2}:?\d{2}$");

            DateTime result;
            if (hasOffset && DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset dto))
                result = TimeZoneUtils.ConvertUtcToUserZone(dto.UtcDateTime);
            else if (!DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                return now;

            // Phone clocks can be wrong; never store a time more than a day ahead or before 2020.
            if (result > now.AddDays(1) || result.Year < 2020)
                return now;
            return result;
        }

        private static decimal? ValidCoordinate(decimal? value, int limit)
        {
            if (!value.HasValue || value.Value < -limit || value.Value > limit)
                return null;
            return Math.Round(value.Value, 6);
        }

        private static MobileDeliverySyncResult Duplicate(MobileDeliverySyncItem item, MobileDeliveryProof existing)
        {
            return new MobileDeliverySyncResult
            {
                ClientRef = existing.ClientRef,
                OrderId = existing.OrderID,
                Ok = true,
                Duplicate = true,
                ProofId = existing.ProofID,
                Status = existing.Status,
                Message = "Already received" + (string.IsNullOrWhiteSpace(existing.StatusMessage) ? "." : ": " + existing.StatusMessage)
            };
        }

        private static MobileDeliverySyncResult Fail(MobileDeliverySyncItem item, string message)
        {
            return new MobileDeliverySyncResult
            {
                ClientRef = item?.ClientRef,
                OrderId = item?.OrderId ?? 0,
                Ok = false,
                Message = message
            };
        }

        private static string NullIfEmpty(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }
    }
}
