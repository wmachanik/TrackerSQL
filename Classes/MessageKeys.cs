namespace TrackerSQL.Classes
{
    /// <summary>
    /// Constants for message resource keys to avoid magic strings
    /// </summary>
    public static class MessageKeys
    {
        /// <summary>
        /// Common messages used across the application
        /// </summary>
        public static class Common
        {
            public const string ErrorGeneric = "Common.ErrorGeneric";
            public const string SuccessGeneric = "Common.SuccessGeneric";
        }

        /// <summary>
        /// Customer-related messages
        /// </summary>
        public static class Customer
        {
            public const string NotFound = "Customer.NotFound";
            public const string Disabled = "Customer.Disabled";
            public const string DisabledViaSelfService = "Customer.DisabledViaSelfService";
            public const string DisableError = "Customer.DisableError";
            public const string EquipmentUpdated = "Customer.EquipmentUpdated";
            public const string EquipmentUpdateError = "Customer.EquipmentUpdateError";
            public const string ReminderCountReset = "Customer.ReminderCountReset";
        }

        public static class AwayPeriod
        {
            public const string ConfirmationSubject = "AwayPeriod.ConfirmationSubject";
            public const string Greeting = "AwayPeriod.Greeting";
            public const string ConfirmationInfo = "AwayPeriod.ConfirmationInfo";
            public const string ConfirmationBody = "AwayPeriod.ConfirmationBody";
        }
        /// <summary>
        /// Disable Client feature messages
        /// </summary>
        public static class DisableClient
        {
            public const string PageTitle = "DisableClient.PageTitle";
            public const string ConfirmationHeader = "DisableClient.ConfirmationHeader";
            public const string ConfirmationMessage = "DisableClient.ConfirmationMessage";
            public const string WarningMessage = "DisableClient.WarningMessage";
            public const string ButtonConfirm = "DisableClient.ButtonConfirm";
            public const string ButtonCancel = "DisableClient.ButtonCancel";
            public const string HelpMessage = "DisableClient.HelpMessage";
            public const string SuccessHeader = "DisableClient.SuccessHeader";
            public const string SuccessMessage = "DisableClient.SuccessMessage";
            public const string SuccessDetails = "DisableClient.SuccessDetails";
            public const string ReenableMessage = "DisableClient.ReenableMessage";
            public const string ErrorHeader = "DisableClient.ErrorHeader";
            public const string ErrorInvalidParams = "DisableClient.ErrorInvalidParams";
            public const string ErrorInvalidToken = "DisableClient.ErrorInvalidToken";
            public const string ErrorCustomerNotFound = "DisableClient.ErrorCustomerNotFound";
            public const string ErrorGeneral = "DisableClient.ErrorGeneral";
            
            // Goodbye email messages
            public const string GoodbyeSubject = "DisableClient.GoodbyeSubject";
            public const string GoodbyeHeader = "DisableClient.GoodbyeHeader";
            public const string GoodbyeMessage = "DisableClient.GoodbyeMessage";
            public const string GoodbyeWhatThisMeans = "DisableClient.GoodbyeWhatThisMeans";
            public const string GoodbyeReenableInstructions = "DisableClient.GoodbyeReenableInstructions";
            public const string GoodbyeStillNeedCoffee = "DisableClient.GoodbyeStillNeedCoffee";
            public const string GoodbyeThankYou = "DisableClient.GoodbyeThankYou";
            public const string GoodbyeFooter = "DisableClient.GoodbyeFooter";
            public const string Greeting = "DisableClient.Greeting";
            public const string DisabledMessage = "DisableClient.DisabledMessage";
            
            // Admin notification messages
            public const string AdminSubjectTemplate = "DisableClient.AdminSubjectTemplate";
            public const string AdminBodyHeader = "DisableClient.AdminBodyHeader";
            public const string AdminRecurringFound = "DisableClient.AdminRecurringFound";
            public const string AdminRecurringNone = "DisableClient.AdminRecurringNone";
            public const string AdminManualActionRequired = "DisableClient.AdminManualActionRequired";
            public const string AdminFooter = "DisableClient.AdminFooter";

            public const string RemindersDisabledSubject = "DisableClient.RemindersDisabledSubject";
            public const string RemindersDisabledMessage = "DisableClient.RemindersDisabledMessage";
        }

        /// <summary>
        /// Email-related messages
        /// </summary>
        public static class Email
        {
            public const string SendError = "Email.SendError";
            public const string InvalidAddress = "Email.InvalidAddress";
            public const string ConfirmationSent = "Email.ConfirmationSent";
            public const string ConfirmationSubject = "Email.ConfirmationSubject";
            public const string GreetingFormat = "Email.GreetingFormat";
            public const string NotificationFormat = "Email.NotificationFormat";
            public const string ThankYouMessage = "Email.ThankYouMessage";
            public const string DefaultGreeting = "Email.DefaultGreeting";
            public const string DefaultRecipient = "Email.DefaultRecipient";
            public const string NoValidRecipient = "Email.NoValidRecipient";
            public const string SignatureTemplate = "Email.SignatureTemplate";
            public const string SignatureNoUser = "Email.SignatureNoUser";
        }

        /// <summary>
        /// Security-related messages
        /// </summary>
        public static class Security
        {
            public const string InvalidSecret = "Security.InvalidSecret";
            public const string InvalidSecretConfig = "Security.InvalidSecretConfig";
            public const string NoHttpContext = "Security.NoHttpContext";
            public const string TokenInvalid = "Security.TokenInvalid";
        }

        /// <summary>
        /// Order-related messages
        /// </summary>
        public static class Order
        {
            public const string NotFound = "Order.NotFound";
            public const string Confirmed = "Order.Confirmed";
            public const string Cancelled = "Order.Cancelled";
            public const string Shipped = "Order.Shipped";
            public const string StatusSubject = "Order.StatusSubject";
            public const string StatusDefaultContact = "Order.StatusDefaultContact";
            public const string StatusFooter = "Order.StatusFooter";
            public const string ConfirmatonSubject = "Order.ConfirmationSubject";
            public const string ConfirmationIntro = "Order.ConfirmationIntro";
            public const string ItemFormatBasic = "Order.ItemFormatBasic";
            public const string ItemFormatWithPrep = "Order.ItemFormatWithPrep";
            public const string CompletedTitle = "Order.CompletedTitle";
            public const string CompletedSuccess = "Order.CompletedSuccess";
            public const string CompletedFailed = "Order.CompletedFailed";
            public const string NoTempOrder = "Order.NoTempOrderFound";
            public const string FutureMessage = "Order.FutureMessage";
            public const string EmailFooter = "Order.EmailFooter";

            // Keys for pre-delivery wording (used by OrderDetail emails)
            public const string StatusPreDeliveryBody = "Order.StatusPreDeliveryBody";
            public const string StatusReadyForCollection = "Order.StatusReadyForCollection";
            public const string StatusPendingDelivery = "Order.StatusPendingDelivery";

            // Existing status keys used by OrderDone (post-delivery)
            public const string StatusPostbox = "Order.StatusPostbox";
            public const string StatusDispatched = "Order.StatusDispatched";
            public const string StatusCollected = "Order.StatusCollected";
            public const string StatusDelivered = "Order.StatusDelivered";

            public const string StatusBody = "Order.StatusBody";
            public const string ConfirmationHeader = "Order.ConfirmationHeader";
            public const string ConfirmationDeliveryDate = "Order.ConfirmationDeliveryDate";
            public const string ConfirmationFooter = "Order.ConfirmationFooter";
            public const string ConfirmationPORequired = "Order.ConfirmationPORequired";
            public const string ConfirmationPOReceived = "Order.ConfirmationPOReceived";
        }
        public static class OrderDetail
        {
            public const string PublicViewLoginPrompt = "OrderDetail.PublicViewLoginPrompt";
            public const string RequestChangesButtonText = "OrderDetail.RequestChangesButtonText";
            public const string RequestChangesMailSubject = "OrderDetail.RequestChangesMailSubject";
            public const string RequestChangesMailBody = "OrderDetail.RequestChangesMailBody";
        }
        /// <summary>
        /// Repair-related messages
        /// </summary>
        public static class Repairs
        {
            public const string StatusEmailSubject = "Repairs.StatusEmailSubject";
            public const string StatusEmailBody = "Repairs.StatusEmailBody";
            public const string CollectSwopOutNote = "Repairs.CollectSwopOutNote";
            public const string DisclaimerFooter = "Repairs.DisclaimerFooter";
            public const string StatusUpdateSuccess = "Repairs.StatusUpdateSuccess";
            public const string ErrorUpdating = "Repairs.ErrorUpdating";
        }

        /// <summary>
        /// Coffee checkup and reminder email messages
        /// </summary>
        public static class CoffeeCheckup
        {
            public const string SubjectReminderOnly = "CoffeeCheckup.SubjectReminderOnly";
            public const string SubjectCombined = "CoffeeCheckup.SubjectCombined";
            public const string SubjectRecurring = "CoffeeCheckup.SubjectRecurring";
            public const string SubjectAutoFulfill = "CoffeeCheckup.SubjectAutoFulfill";
            public const string GreetingWithName = "CoffeeCheckup.GreetingWithName";
            public const string GreetingGeneric = "CoffeeCheckup.GreetingGeneric";
            public const string OrderTypeRecurring = "CoffeeCheckup.OrderTypeRecurring";
            public const string OrderTypeAutoFulfill = "CoffeeCheckup.OrderTypeAutoFulfill";
            public const string OrderTypeCombined = "CoffeeCheckup.OrderTypeCombined";
            public const string OrderTypeReminderOnly = "CoffeeCheckup.OrderTypeReminderOnly";
            public const string BodyReminderOnly = "CoffeeCheckup.BodyReminderOnly";
            public const string BodyOrderType = "CoffeeCheckup.BodyOrderType";
            public const string BodyFinalWarning = "CoffeeCheckup.BodyFinalWarning";
            public const string FooterOrderAdded = "CoffeeCheckup.FooterOrderAdded";
            public const string FooterOrderLink = "CoffeeCheckup.FooterOrderLink";
            public const string FooterDisableLink = "CoffeeCheckup.FooterDisableLink";
            public const string ErrorRecurringDateFailure = "CoffeeCheckup.ErrorRecurringDateFailure";
            public const string ErrorTempTableDelete = "CoffeeCheckup.ErrorTempTableDelete";
            public const string ErrorTempTableInsert = "CoffeeCheckup.ErrorTempTableInsert";
            public const string ErrorOrderInsert = "CoffeeCheckup.ErrorOrderInsert";
            public const string ErrorEmailSending = "CoffeeCheckup.ErrorEmailSending";
            public const string ErrorBatchEmailFailure = "CoffeeCheckup.ErrorBatchEmailFailure";
            public const string StatusRemindersProcessed = "CoffeeCheckup.StatusRemindersProcessed";
            public const string StatusBatchSendSuccess = "CoffeeCheckup.StatusBatchSendSuccess";
            public const string StatusBatchSendPartial = "CoffeeCheckup.StatusBatchSendPartial";
            public const string DialogRecurringDateTitle = "CoffeeCheckup.DialogRecurringDateTitle";
            public const string DialogTempTableTitle = "CoffeeCheckup.DialogTempTableTitle";
            public const string DialogEmailStatusTitle = "CoffeeCheckup.DialogEmailStatusTitle";
            public const string HtmlTableIntro = "CoffeeCheckup.HtmlTableIntro";
            public const string HtmlTableStart = "CoffeeCheckup.HtmlTableStart";
            public const string HtmlTableHeader = "CoffeeCheckup.HtmlTableHeader";
            public const string HtmlTableRowNormal = "CoffeeCheckup.HtmlTableRowNormal";
            public const string HtmlTableRowAlt = "CoffeeCheckup.HtmlTableRowAlt";
            public const string HtmlTableCellNormal = "CoffeeCheckup.HtmlTableCellNormal";
            public const string HtmlTableCellAlt = "CoffeeCheckup.HtmlTableCellAlt";
            public const string HtmlTableRowColspan = "CoffeeCheckup.HtmlTableRowColspan";
            public const string TableCompanyContact = "CoffeeCheckup.TableCompanyContact";
            public const string TableNextPrepDate = "CoffeeCheckup.TableNextPrepDate";
            public const string TableNextDispatchDate = "CoffeeCheckup.TableNextDispatchDate";
            public const string TableType = "CoffeeCheckup.TableType";
            public const string TableListOfItems = "CoffeeCheckup.TableListOfItems";
            public const string ClosureAdjustmentPrefix = "CoffeeCheckup.ClosureAdjustmentPrefix";
            public const string UpcomingClosures = "CoffeeCheckup.UpcomingClosures";
            public const string AdjustedDatesLabel = "CoffeeCheckup.AdjustedDatesLabel";
            public const string HolidayClosureEmailNote = "CoffeeCheckup.HolidayClosureEmailNote";
        }

        /// <summary>
        /// Delivery calculation messages
        /// </summary>
        public static class DeliveryCalculation
        {
            public const string CalculatingMonthlyDelivery = "DeliveryCalculation.CalculatingMonthlyDelivery";
            public const string CalculatingWeeklyDelivery = "DeliveryCalculation.CalculatingWeeklyDelivery";
            public const string NextOccurrenceCalculated = "DeliveryCalculation.NextOccurrenceCalculated";
            public const string OptimalDateCalculated = "DeliveryCalculation.OptimalDateCalculated";
            public const string WeeklyDateCalculated = "DeliveryCalculation.WeeklyDateCalculated";
            public const string UsingCityDeliveryDate = "DeliveryCalculation.UsingCityDeliveryDate";
            public const string InvalidRoastDate = "DeliveryCalculation.InvalidRoastDate";
            public const string OptimalCityDateFound = "DeliveryCalculation.OptimalCityDateFound";
            public const string FoundNextDeliveryCycle = "DeliveryCalculation.FoundNextDeliveryCycle";
            public const string UsingFallbackDate = "DeliveryCalculation.UsingFallbackDate";
            public const string RoastDateCalculated = "DeliveryCalculation.RoastDateCalculated";
            public const string DateInPast = "DeliveryCalculation.DateInPast";
            public const string DateTooFarFuture = "DeliveryCalculation.DateTooFarFuture";
            public const string CalculatedOptimalDates = "DeliveryCalculation.CalculatedOptimalDates";
            public const string ErrorCalculatingDelivery = "DeliveryCalculation.ErrorCalculatingDelivery";
            public const string ErrorCalculatingWeekly = "DeliveryCalculation.ErrorCalculatingWeekly";
            public const string ErrorCalculatingOccurrence = "DeliveryCalculation.ErrorCalculatingOccurrence";
            public const string ErrorFindingCityDate = "DeliveryCalculation.ErrorFindingCityDate";
            public const string ErrorFindingClosestDate = "DeliveryCalculation.ErrorFindingClosestDate";
            public const string ErrorCalculatingRoastDate = "DeliveryCalculation.ErrorCalculatingRoastDate";
            public const string ErrorValidatingDate = "DeliveryCalculation.ErrorValidatingDate";
        }
    }
}