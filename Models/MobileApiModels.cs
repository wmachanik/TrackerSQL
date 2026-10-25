using System.Collections.Generic;

namespace TrackerSQL.Models
{
    // JSON shapes for the mobile REST API (/api/v1). Serialised camelCase with nulls omitted,
    // so optional members cost nothing on the wire. Dates are "yyyy-MM-dd"; times are
    // server local time (SAST) as "yyyy-MM-ddTHH:mm:ss".

    public class MobileLoginRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string AppVersion { get; set; }
    }

    public class MobileLoginResponse
    {
        public string Token { get; set; }
        public string ExpiresAt { get; set; }
        public MobileUserInfo User { get; set; }
    }

    public class MobileUserInfo
    {
        public string UserName { get; set; }
        public int? PersonId { get; set; }
        public string PersonName { get; set; }
        public bool IsAdmin { get; set; }
        public List<string> Roles { get; set; }
        public bool AutoCompleteDeliveries { get; set; }
    }

    /// <summary>Resolved per request from the bearer token.</summary>
    public class MobileTokenUser
    {
        public long TokenId { get; set; }
        public string UserName { get; set; }
        public int? PersonId { get; set; }
    }

    public class MobileDeviceToken
    {
        public long TokenID { get; set; }
        public string UserName { get; set; }
        public int? PersonID { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string AppVersion { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public System.DateTime? LastUsedAt { get; set; }
        public System.DateTime ExpiresAt { get; set; }
        public System.DateTime? RevokedAt { get; set; }
    }

    public class MobileDeliveryDate
    {
        public string Date { get; set; }
        public int Orders { get; set; }
        public int Open { get; set; }
        /// <summary>Orders on this date for the signed-in driver (null when not linked to a person).</summary>
        public int? Mine { get; set; }
        /// <summary>Depot collections on this date; only for people ticked Collections in Lookups → People (else null).</summary>
        public int? Collections { get; set; }
    }

    public class MobileDeliverySheet
    {
        public string Date { get; set; }
        public int? PersonId { get; set; }
        public string GeneratedAt { get; set; }
        /// <summary>One driver's sheet: who last set the route order when it was not the driver ("The office changed your order at 10:15").</summary>
        public string RouteNote { get; set; }
        public List<MobileDeliveryStop> Orders { get; set; } = new List<MobileDeliveryStop>();
    }

    /// <summary>The client's GPS point in Tracker, so the app can work out arrival times and say whether a delivery will update it.</summary>
    public class MobileStopGps
    {
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        /// <summary>"pin" (placed by hand), "driver" (from deliveries), "lookup" (address lookup) or "none" (no point yet).</summary>
        public string Kind { get; set; }
        /// <summary>Checked: two deliveries agreed, or a person confirmed it.</summary>
        public bool Checked { get; set; }
        /// <summary>Flagged for the office to check.</summary>
        public bool Review { get; set; }
    }

    public class MobileDeliveryStop
    {
        public int OrderId { get; set; }
        public int ContactId { get; set; }
        public string Company { get; set; }
        /// <summary>Deliver-to short name when the order uses a recipient address (detail title).</summary>
        public string DeliverTo { get; set; }
        /// <summary>Billing contact company when <see cref="DeliverTo"/> is set (detail subtitle).</summary>
        public string AccountCompany { get; set; }
        /// <summary>Person to hand the delivery to (contact first + last name; ZZName walk-in name from the order notes).</summary>
        public string ContactName { get; set; }
        /// <summary>The contact's alternative person (Contact Details), who may also receive or collect it.</summary>
        public string AltContactName { get; set; }
        public string Phone { get; set; }
        public string Cell { get; set; }
        public string Address { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public string Area { get; set; }
        public string Notes { get; set; }
        public string Po { get; set; }
        public bool Done { get; set; }
        public bool Confirmed { get; set; }
        /// <summary>Position in the driver's route for the day (1, 2, 3...), in geohash order.</summary>
        public int Seq { get; set; }
        public int? DeliveryById { get; set; }
        public string DeliveryBy { get; set; }
        public List<MobileDeliveryItem> Items { get; set; } = new List<MobileDeliveryItem>();
        /// <summary>Repairs linked to this order (collect from or return to the client).</summary>
        public List<MobileStopRepair> Repairs { get; set; }
        public MobileDeliveryProofSummary Proof { get; set; }
        /// <summary>The client's GPS point (Kind "none" when Tracker has none yet; null for walk-in orders).</summary>
        public MobileStopGps Gps { get; set; }
        /// <summary>True when deliveries never change the client's point (walk-in, collects or goes by courier).</summary>
        public bool? NoGpsLearning { get; set; }
        /// <summary>True when the client collects it at the depot (Delivered by a "Collect (shop)" person): not on the route.</summary>
        public bool? Collection { get; set; }
    }

    public class MobileDeliveryItem
    {
        public int LineId { get; set; }
        public double Qty { get; set; }
        public string Item { get; set; }
        public string Pack { get; set; }
        /// <summary>The packaging's background colour from Lookups → Packaging ("#RRGGBB"), as on the delivery sheet.</summary>
        public string PackColour { get; set; }
        /// <summary>A repair (Service) line: nothing is handed over, the linked repair says what to do.</summary>
        public bool Repair { get; set; }
    }

    public class MobileStopRepair
    {
        public int Id { get; set; }
        public string JobCard { get; set; }
        public string EquipType { get; set; }
        public string Serial { get; set; }
        public string Fault { get; set; }
        public int? StatusId { get; set; }
        public string Status { get; set; }
    }

    public class MobileDeliveryProofSummary
    {
        public long ProofId { get; set; }
        public string Outcome { get; set; }
        public string Reason { get; set; }
        public string ReceivedBy { get; set; }
        public string MissingItems { get; set; }
        public string DeliveredAt { get; set; }
        public string Status { get; set; }
        public bool HasSignature { get; set; }
        /// <summary>True when it was not handed over at the client's address (collected by the client, dropped off by a supplier).</summary>
        public bool? NotAtClient { get; set; }
    }

    /// <summary>Configurable choices on the phone ("Left at reception", "Nobody there"), edited in Lookups.</summary>
    public class MobileDeliveryOption
    {
        public int Id { get; set; }
        /// <summary>"Delivered" or "NotDelivered".</summary>
        public string Outcome { get; set; }
        public string Text { get; set; }
        /// <summary>The driver must enter who received it or get a signature.</summary>
        public bool NeedsName { get; set; }
        public int SortOrder { get; set; }
        public bool Enabled { get; set; }
        /// <summary>
        /// The driver was not at the client's address (the client collected, a supplier dropped it off): the phone's
        /// position is kept with the proof but never used for the client's GPS point or the delivery order.
        /// </summary>
        public bool NotAtClient { get; set; }
    }

    public class MobileDeliverySyncRequest
    {
        public List<MobileDeliverySyncItem> Deliveries { get; set; }
    }

    public class MobileDeliverySyncItem
    {
        /// <summary>Unique id created on the phone (e.g. a GUID) so a retried upload is not stored twice.</summary>
        public string ClientRef { get; set; }
        public int OrderId { get; set; }
        /// <summary>"Delivered" (default) or "NotDelivered".</summary>
        public string Outcome { get; set; }
        /// <summary>The option the driver picked, e.g. "Left at reception" or "Nobody there".</summary>
        public string Reason { get; set; }
        public string ReceivedBy { get; set; }
        /// <summary>Base64 PNG or JPEG (a data: URL prefix is accepted).</summary>
        public string Signature { get; set; }
        public string Note { get; set; }
        /// <summary>Items that were not handed over (short or to follow), e.g. "2 × Kenya AA (1kg)".</summary>
        public string MissingItems { get; set; }
        /// <summary>Quantity handed over per order line (only needed when it differs from the order).</summary>
        public List<MobileDeliveredLine> Lines { get; set; }
        /// <summary>Email the note to the office (e.g. the client asked for a change to the order).</summary>
        public bool NoteToOffice { get; set; }
        /// <summary>When the driver captured it (ISO 8601; with or without an offset). Defaults to the upload time.</summary>
        public string DeliveredAt { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
        /// <summary>How sure the phone was of Lat/Lng, in metres (smaller is better).</summary>
        public decimal? Accuracy { get; set; }
        /// <summary>The person receiving it ticked that they accept our terms (null from app versions without the box).</summary>
        public bool? TermsAccepted { get; set; }
        /// <summary>The terms wording the app showed when it was ticked.</summary>
        public string Terms { get; set; }
    }

    public class MobileDeliveredLine
    {
        public int LineId { get; set; }
        public double Qty { get; set; }
    }

    public class MobileDeliverySyncResult
    {
        public string ClientRef { get; set; }
        public int OrderId { get; set; }
        public bool Ok { get; set; }
        public bool Duplicate { get; set; }
        public long? ProofId { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }

    public class MobileDeliveryProof
    {
        public long ProofID { get; set; }
        public string ClientRef { get; set; }
        public int OrderID { get; set; }
        public int? ContactID { get; set; }
        public int? PersonID { get; set; }
        public string UserName { get; set; }
        public string Outcome { get; set; }
        public string Reason { get; set; }
        public string ReceivedByName { get; set; }
        public string SignatureContentType { get; set; }
        public bool HasSignature { get; set; }
        public string DriverNote { get; set; }
        public string MissingItems { get; set; }
        public System.DateTime DeliveredAt { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? AccuracyM { get; set; }
        public string Status { get; set; }
        public string StatusMessage { get; set; }
        public System.DateTime ReceivedAt { get; set; }
        public System.DateTime? CompletedAt { get; set; }
        public string CompletedBy { get; set; }
        public string CompanyName { get; set; }
        public bool NotAtClient { get; set; }
        /// <summary>Null when captured by an app version without the terms box.</summary>
        public bool? TermsAccepted { get; set; }
        public string TermsText { get; set; }
        /// <summary>What was handed over, one "2 × Kenya AA (1kg)" per line (null on proofs from before 3.0.3.2).</summary>
        public string HandedOver { get; set; }
        public System.DateTime? PodSentAt { get; set; }
        public string PodSentTo { get; set; }
    }

    public class MobileProofView
    {
        public long ProofId { get; set; }
        public int OrderId { get; set; }
        public int? ContactId { get; set; }
        public string Company { get; set; }
        public string Driver { get; set; }
        public string Outcome { get; set; }
        public string Reason { get; set; }
        public string ReceivedBy { get; set; }
        public string Note { get; set; }
        public string MissingItems { get; set; }
        public string DeliveredAt { get; set; }
        public string ReceivedAt { get; set; }
        public string Status { get; set; }
        public string StatusMessage { get; set; }
        public bool HasSignature { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
        public bool? TermsAccepted { get; set; }
    }

    public class MobileRepair
    {
        public int Id { get; set; }
        public int ContactId { get; set; }
        public string Company { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string JobCard { get; set; }
        public string Logged { get; set; }
        public string LastChange { get; set; }
        public int? EquipTypeId { get; set; }
        public string EquipType { get; set; }
        public string Serial { get; set; }
        public int? SwopOutId { get; set; }
        public int? ConditionId { get; set; }
        public bool TakenFrother { get; set; }
        public bool TakenBeanLid { get; set; }
        public bool TakenWaterLid { get; set; }
        public bool BrokenFrother { get; set; }
        public bool BrokenBeanLid { get; set; }
        public bool BrokenWaterLid { get; set; }
        public int? FaultId { get; set; }
        public string Fault { get; set; }
        public string FaultDesc { get; set; }
        public int? StatusId { get; set; }
        public string Status { get; set; }
        public int? RelatedOrderId { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>Create (POST) or partial update (PUT): only members that are sent are changed.</summary>
    public class MobileRepairSave
    {
        /// <summary>Create only: unique id from the phone so an offline create synced twice makes one repair.</summary>
        public string ClientRef { get; set; }
        public int? ContactId { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string JobCard { get; set; }
        public int? EquipTypeId { get; set; }
        public string Serial { get; set; }
        public int? SwopOutId { get; set; }
        public int? ConditionId { get; set; }
        public bool? TakenFrother { get; set; }
        public bool? TakenBeanLid { get; set; }
        public bool? TakenWaterLid { get; set; }
        public bool? BrokenFrother { get; set; }
        public bool? BrokenBeanLid { get; set; }
        public bool? BrokenWaterLid { get; set; }
        public int? FaultId { get; set; }
        public string FaultDesc { get; set; }
        public int? StatusId { get; set; }
        public string Notes { get; set; }
        /// <summary>Email the contact about the status (default: only when the status changes, like the web app).</summary>
        public bool? Notify { get; set; }
    }

    public class MobileRepairSaveResult
    {
        public bool Ok { get; set; }
        public bool Duplicate { get; set; }
        public string Message { get; set; }
        public MobileRepair Repair { get; set; }
    }

    public class MobileLookupItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }

    public class MobileLookups
    {
        public List<MobileLookupItem> RepairStatuses { get; set; } = new List<MobileLookupItem>();
        public List<MobileLookupItem> Faults { get; set; } = new List<MobileLookupItem>();
        public List<MobileLookupItem> EquipTypes { get; set; } = new List<MobileLookupItem>();
        public List<MobileLookupItem> Conditions { get; set; } = new List<MobileLookupItem>();
        public List<MobileLookupItem> People { get; set; } = new List<MobileLookupItem>();
        public List<MobileDeliveryOption> DeliveryOptions { get; set; } = new List<MobileDeliveryOption>();
        public MobileDriverSettings Settings { get; set; }
    }

    /// <summary>POST /api/v1/delivery/route: the driver's own stop order for a day (empty = back to the suggested order).</summary>
    public class MobileRouteRequest
    {
        public string Date { get; set; }
        public List<int> OrderIds { get; set; }
    }

    /// <summary>System → Driver App settings (MobileApiSettingsTbl, one row).</summary>
    public class MobileApiSettings
    {
        /// <summary>A synced "Delivered" runs the same Order Done steps as the web Done button.</summary>
        public bool RunDoneOnDelivery { get; set; }
        /// <summary>Email the client a delivery confirmation with who received it and the signature attached.</summary>
        public bool SendDeliveryConfirmation { get; set; }
        /// <summary>Drivers may email their delivery note to the office.</summary>
        public bool AllowNotesToOffice { get; set; } = true;
        /// <summary>Where driver notes go; empty means the orders address (SysCCEmailAddress).</summary>
        public string OfficeEmail { get; set; }
        /// <summary>Comma-separated starts of landline numbers (e.g. "021, 011"); the app offers no WhatsApp for them.</summary>
        public string LandlinePrefixes { get; set; }
        /// <summary>What the person receiving a delivery ticks on the app; empty means the built-in wording.</summary>
        public string DeliveryTerms { get; set; }
        /// <summary>Where the full terms are; empty means the built-in quaffee.co.za page.</summary>
        public string TermsUrl { get; set; }
        public System.DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }

    /// <summary>The Driver App settings the phone needs.</summary>
    public class MobileDriverSettings
    {
        public bool RunDoneOnDelivery { get; set; }
        public bool SendConfirmation { get; set; }
        public bool NotesToOffice { get; set; }
        /// <summary>Local-format starts of landline numbers ("021"); +27 21 and 27 21 match them too.</summary>
        public System.Collections.Generic.List<string> LandlinePrefixes { get; set; }
        /// <summary>Driver tracking (System Preferences → GPS &amp; Drivers): when the app sends its position.</summary>
        public MobileTrackingSettings Tracking { get; set; }
        /// <summary>The Android app on this site (Downloads/QuaffeeDriver.json from "npm run apk"); null when there is none.</summary>
        public MobileAppRelease App { get; set; }
        /// <summary>Arrival times on the route (System Preferences → GPS &amp; Drivers).</summary>
        public MobileRouteTimesSettings Route { get; set; }
        /// <summary>The statement the person receiving a delivery ticks before signing (System → Driver App).</summary>
        public string Terms { get; set; }
        /// <summary>The full terms and conditions on the web.</summary>
        public string TermsUrl { get; set; }
    }

    public class MobileRouteTimesSettings
    {
        public int SpeedKmh { get; set; }
        /// <summary>Minutes spent at each stop.</summary>
        public int ServiceMinutes { get; set; }
        public double? DepotLat { get; set; }
        public double? DepotLng { get; set; }
        /// <summary>Times are worked out once the phone is this far from the depot (or after the first delivery).</summary>
        public int LeftDepotMetres { get; set; }
    }

    public class MobileAppRelease
    {
        /// <summary>"0.5.1" (package.json version).</summary>
        public string Version { get; set; }
        public int VersionCode { get; set; }
        /// <summary>Site-relative download address, e.g. "/Downloads/QuaffeeDriver.apk?v=0.5.1".</summary>
        public string Url { get; set; }
    }

    public class MobileTrackingSettings
    {
        public bool Enabled { get; set; }
        /// <summary>Minutes between positions while tracking.</summary>
        public int Minutes { get; set; }
        public bool WeekdaysOnly { get; set; }
        /// <summary>Work hours "HH:mm" (South African time); no positions are sent outside them.</summary>
        public string From { get; set; }
        public string To { get; set; }
    }

    /// <summary>POST api/v1/location: positions queued on the phone (oldest first).</summary>
    public class MobileLocationRequest
    {
        public List<MobileLocationPoint> Points { get; set; }
    }

    public class MobileLocationPoint
    {
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
        /// <summary>GPS accuracy in metres.</summary>
        public decimal? Accuracy { get; set; }
        /// <summary>When the phone took the position (ISO 8601, with offset or Z).</summary>
        public string At { get; set; }
        /// <summary>APP_OPEN or PERIODIC.</summary>
        public string Source { get; set; }
    }

    public class MobileContact
    {
        public int Id { get; set; }
        public string Company { get; set; }
        public string ContactName { get; set; }
        public string Phone { get; set; }
        public string Cell { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public bool Enabled { get; set; }
        public int? EquipTypeId { get; set; }
        public string Serial { get; set; }
    }

    public class MobileApiError
    {
        public string Error { get; set; }
    }

    /// <summary>One API call in the request log (no bodies, passwords, tokens or signatures are stored).</summary>
    public class MobileApiLogEntry
    {
        public long Id { get; set; }
        public System.DateTime At { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public int Status { get; set; }
        public int Ms { get; set; }
        public int? RequestBytes { get; set; }
        public int? ResponseBytes { get; set; }
        public string User { get; set; }
        public long? TokenId { get; set; }
        public string Ip { get; set; }
        public string Agent { get; set; }
        public string Note { get; set; }
    }
}
