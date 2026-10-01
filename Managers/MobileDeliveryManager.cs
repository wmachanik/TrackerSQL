using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Classes.Geo;
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
            var routeRows = new Dictionary<int, MobileApiRepository.SheetRow>();
            foreach (var row in _repo.GetSheetRows(date, personId))
            {
                if (stop == null || stop.OrderId != row.OrderID)
                {
                    stop = BuildStop(row);
                    sheet.Orders.Add(stop);
                    routeRows[row.OrderID] = row;
                }

                stop.Items.Add(new MobileDeliveryItem
                {
                    LineId = row.OrderLineID,
                    Qty = Math.Round(row.Qty, SystemConstants.DatabaseConstants.NumDecimalPoints),
                    Item = NullIfEmpty(row.ItemDesc),
                    Pack = NullIfEmpty(row.PackDesc),
                    PackColour = HexColour(row.PackColour),
                    Repair = row.ItemServiceTypeID == SystemConstants.ServiceTypeConstants.Service
                             || row.ItemID == SystemConstants.ItemConstants.RepairCheckItemID
                });
            }

            sheet.Orders = PlanRoutes(sheet.Orders, routeRows);

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

        /// <summary>
        /// Logs which stop of the day this was (delivered or not: the driver went there), so the route can learn the driver's
        /// own order. Never fails the sync.
        /// </summary>
        private static void RecordDeliveryOrder(MobileApiRepository.OrderBrief order, MobileDeliveryProof proof, MobileTokenUser user)
        {
            if (!proof.ContactID.HasValue || proof.ContactID.Value == SystemConstants.CustomerConstants.SundryCustomerID)
                return;
            int driver = user?.PersonId ?? order.DeliveryById ?? 0;
            try
            {
                if (!DeliveryVisitRepository.TableSeen())
                    return;
                Tuple<int, int> stop = new DeliveryVisitRepository().RecordVisit(proof.DeliveredAt.Date, driver, proof.ContactID.Value, proof.DeliveredAt);
                AppLogger.WriteLog(SystemConstants.LogTypes.Delivery,
                    "Delivery order: driver " + driver + " " + proof.DeliveredAt.ToString("yyyy-MM-dd HH:mm")
                    + " | contact " + proof.ContactID.Value + " was stop " + stop.Item1 + " of " + stop.Item2
                    + " (order " + order.OrderID + ", " + proof.Outcome + ")", user?.UserName);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Delivery,
                    "Delivery order not recorded for order " + order.OrderID + ": " + ex.Message, user?.UserName);
            }
        }

        /// <summary>
        /// Each driver's stops (already grouped by driver, in geohash order): the order the driver set in the app for the day
        /// (DeliveryRouteTbl), else the order the driver really delivers in (DeliveryVisitTbl), then the driving order from
        /// the depot, then the area order; numbered 1, 2, 3...
        /// Delivered stops stay in the route so the numbers do not shift during the day.
        /// </summary>
        private static List<MobileDeliveryStop> PlanRoutes(List<MobileDeliveryStop> stops, Dictionary<int, MobileApiRepository.SheetRow> routeRows)
        {
            if (stops.Count == 0)
                return stops;
            DeliverySheetRepository.DepotPoint(out double? depotLat, out double? depotLng);

            var result = new List<MobileDeliveryStop>(stops.Count);
            foreach (var driverStops in stops.GroupBy(s => s.DeliveryById ?? 0))
            {
                List<MobileDeliveryStop> route = RoutePlanner.Order(driverStops.ToList(),
                    s => routeRows.TryGetValue(s.OrderId, out var row) ? row.RouteHash : null, depotLat, depotLng);
                route = RoutePlanner.ApplyLearned(route, s => s.ContactId,
                    s => routeRows.TryGetValue(s.OrderId, out var row) && row.LearnedPos.HasValue
                        ? Tuple.Create(row.LearnedPos.Value, row.LearnedVisits) : null);
                route = RoutePlanner.ApplyPlan(route,
                    s => routeRows.TryGetValue(s.OrderId, out var row) && row.PlanSeq.HasValue
                        ? Tuple.Create(row.PlanSeq.Value, row.PlanStops) : null);
                for (int i = 0; i < route.Count; i++)
                    route[i].Seq = i + 1;
                result.AddRange(route);
            }
            return result;
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

        /// <summary>"#RRGGBB" (or "#RGB") from Lookups → Packaging, else null.</summary>
        private static string HexColour(string value)
        {
            string v = (value ?? string.Empty).Trim();
            return System.Text.RegularExpressions.Regex.IsMatch(v, "^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$") ? v.ToUpperInvariant() : null;
        }

        /// <summary>
        /// The driver's own order for the day, set in the app before leaving (orders first to last; several orders at one
        /// client count once). It comes first in the route on the app and the delivery sheet; an empty list goes back to
        /// the suggested order. Returns the number of stops saved.
        /// </summary>
        public int SaveRoute(MobileRouteRequest request, MobileTokenUser user, out string error)
        {
            error = null;
            if (user?.PersonId == null)
            {
                error = "Your Tracker login is not linked to a person (Lookups → People), so there is no route to set.";
                return 0;
            }
            if (request == null || !DateTime.TryParseExact(request.Date ?? string.Empty, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime day))
            {
                error = "date must be yyyy-MM-dd.";
                return 0;
            }
            List<int> orderIds = request.OrderIds ?? new List<int>();
            if (orderIds.Count > 500)
            {
                error = "Send at most 500 orders.";
                return 0;
            }
            if (!DeliveryRouteRepository.TableSeen())
            {
                error = "Tracker is not ready for driver routes yet (SQLCommands-DriverGps-02.xml).";
                return 0;
            }

            var repo = new DeliveryRouteRepository();
            Dictionary<int, int> contactOf = repo.ContactsForOrders(day, orderIds);
            List<int> contacts = orderIds.Where(contactOf.ContainsKey).Select(id => contactOf[id]).Distinct().ToList();
            if (orderIds.Count > 0 && contacts.Count == 0)
            {
                error = "None of those orders are due on " + day.ToString("yyyy-MM-dd") + ".";
                return 0;
            }
            repo.Save(day, user.PersonId.Value, contacts, user.UserName);

            AppLogger.WriteLog(SystemConstants.LogTypes.Delivery, contacts.Count == 0
                ? "Route order: driver " + user.PersonId + " " + day.ToString("yyyy-MM-dd") + " went back to the suggested order"
                : "Route order: driver " + user.PersonId + " " + day.ToString("yyyy-MM-dd") + " set " + contacts.Count + " stops: contacts "
                  + string.Join(", ", contacts), user.UserName);
            return contacts.Count;
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
                AccuracyM = item.Accuracy.HasValue && item.Accuracy.Value > 0 && item.Accuracy.Value < 100000
                    ? Math.Round(item.Accuracy.Value, 1) : (decimal?)null,
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
                + " | at " + proof.DeliveredAt.ToString("yyyy-MM-dd HH:mm")
                + (proof.Latitude.HasValue && proof.Longitude.HasValue
                    ? " | GPS " + proof.Latitude.Value.ToString("0.00000", CultureInfo.InvariantCulture) + ","
                      + proof.Longitude.Value.ToString("0.00000", CultureInfo.InvariantCulture)
                      + (proof.AccuracyM.HasValue ? " +/-" + proof.AccuracyM.Value.ToString("0", CultureInfo.InvariantCulture) + "m" : string.Empty)
                    : " | no GPS"), user?.UserName);

            if (outcome == OutcomeDelivered && proof.ContactID.HasValue && proof.Latitude.HasValue && proof.Longitude.HasValue)
            {
                try
                {
                    new CustomerLocationManager().LearnFromDelivery(proof.ContactID.Value, proofId, proof.DeliveredAt,
                        proof.Latitude.Value, proof.Longitude.Value, proof.AccuracyM, user?.UserName);
                }
                catch (Exception ex)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Delivery,
                        "GPS learn from delivery failed for order " + order.OrderID + ": " + ex.Message, user?.UserName);
                }
            }

            RecordDeliveryOrder(order, proof, user);
            DriverTrackingManager.RecordDelivery(user?.PersonId, proof.Latitude, proof.Longitude, proof.AccuracyM, proof.DeliveredAt, user?.UserName);

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
