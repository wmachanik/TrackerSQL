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
    }

    public class MobileDeliverySheet
    {
        public string Date { get; set; }
        public int? PersonId { get; set; }
        public string GeneratedAt { get; set; }
        public List<MobileDeliveryStop> Orders { get; set; } = new List<MobileDeliveryStop>();
    }

    public class MobileDeliveryStop
    {
        public int OrderId { get; set; }
        public int ContactId { get; set; }
        public string Company { get; set; }
        /// <summary>Person to hand the delivery to (contact first + last name; ZZName walk-in name from the order notes).</summary>
        public string ContactName { get; set; }
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
        public int Seq { get; set; }
        public int? DeliveryById { get; set; }
        public string DeliveryBy { get; set; }
        public List<MobileDeliveryItem> Items { get; set; } = new List<MobileDeliveryItem>();
        /// <summary>Repairs linked to this order (collect from or return to the client).</summary>
        public List<MobileStopRepair> Repairs { get; set; }
        public MobileDeliveryProofSummary Proof { get; set; }
    }

    public class MobileDeliveryItem
    {
        public int LineId { get; set; }
        public double Qty { get; set; }
        public string Item { get; set; }
        public string Pack { get; set; }
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
        public string Status { get; set; }
        public string StatusMessage { get; set; }
        public System.DateTime ReceivedAt { get; set; }
        public System.DateTime? CompletedAt { get; set; }
        public string CompletedBy { get; set; }
        public string CompanyName { get; set; }
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
        public System.DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }

    /// <summary>The Driver App settings the phone needs.</summary>
    public class MobileDriverSettings
    {
        public bool RunDoneOnDelivery { get; set; }
        public bool SendConfirmation { get; set; }
        public bool NotesToOffice { get; set; }
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
