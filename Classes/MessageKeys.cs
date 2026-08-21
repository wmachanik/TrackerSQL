using System;

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
        /// Recurring-order contact notification emails (add / update / disable).
        /// </summary>
        public static class RecurringOrder
        {
            public const string AddedEmailSubject = "RecurringOrder.AddedEmailSubject";
            public const string AddedEmailBody = "RecurringOrder.AddedEmailBody";
            public const string UpdatedEmailSubject = "RecurringOrder.UpdatedEmailSubject";
            public const string UpdatedEmailBody = "RecurringOrder.UpdatedEmailBody";
            public const string DisabledEmailSubject = "RecurringOrder.DisabledEmailSubject";
            public const string DisabledEmailBody = "RecurringOrder.DisabledEmailBody";
            public const string NoEmailAddress = "RecurringOrder.NoEmailAddress";
            public const string ItemsListEmpty = "RecurringOrder.ItemsListEmpty";
            public const string ItemsListItem = "RecurringOrder.ItemsListItem";
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
            public const string BodyLastRecurringOrder = "CoffeeCheckup.BodyLastRecurringOrder";
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
            public const string TableNextPreparationDate = "CoffeeCheckup.TableNextPreparationDate";
            [Obsolete("Use TableNextPreparationDate")]
            public const string TableNextPreperationDate = TableNextPreparationDate;
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
            public const string UsingAreaDeliveryDate = "DeliveryCalculation.UsingAreaDeliveryDate";
            public const string InvalidPrepDate = "DeliveryCalculation.InvalidPrepDate";
            public const string OptimalAreaDateFound = "DeliveryCalculation.OptimalAreaDateFound";
            public const string FoundNextDeliveryCycle = "DeliveryCalculation.FoundNextDeliveryCycle";
            public const string UsingFallbackDate = "DeliveryCalculation.UsingFallbackDate";
            public const string PrepDateCalculated = "DeliveryCalculation.PrepDateCalculated";
            public const string DateInPast = "DeliveryCalculation.DateInPast";
            public const string DateTooFarFuture = "DeliveryCalculation.DateTooFarFuture";
            public const string CalculatedOptimalDates = "DeliveryCalculation.CalculatedOptimalDates";
            public const string ErrorCalculatingDelivery = "DeliveryCalculation.ErrorCalculatingDelivery";
            public const string ErrorCalculatingWeekly = "DeliveryCalculation.ErrorCalculatingWeekly";
            public const string ErrorCalculatingOccurrence = "DeliveryCalculation.ErrorCalculatingOccurrence";
            public const string ErrorFindingAreaDate = "DeliveryCalculation.ErrorFindingAreaDate";
            public const string ErrorFindingClosestDate = "DeliveryCalculation.ErrorFindingClosestDate";
            public const string ErrorCalculatingPrepDate = "DeliveryCalculation.ErrorCalculatingPrepDate";
            public const string ErrorValidatingDate = "DeliveryCalculation.ErrorValidatingDate";
        }

        /// <summary>
        /// System Preferences page (replaces System Data).
        /// </summary>
        public static class SystemPreferences
        {
            public const string PageTitle = "SystemPreferences.PageTitle";
            public const string PageSubtitle = "SystemPreferences.PageSubtitle";
            public const string NavGeneral = "SystemPreferences.NavGeneral";
            public const string NavWooCommerce = "SystemPreferences.NavWooCommerce";
            public const string SectionsHeading = "SystemPreferences.SectionsHeading";
            public const string AccessDenied = "SystemPreferences.AccessDenied";
            public const string GeneralSaved = "SystemPreferences.GeneralSaved";
        }

        /// <summary>
        /// WooCommerce integration UI / wizard / connection messages.
        /// </summary>
        public static class WooCommerce
        {
            public const string SectionTitle = "WooCommerce.SectionTitle";
            public const string EnableButton = "WooCommerce.EnableButton";
            public const string DisableButton = "WooCommerce.DisableButton";
            public const string StartWizard = "WooCommerce.StartWizard";
            public const string RerunWizard = "WooCommerce.RerunWizard";
            public const string WizardStepPrepTitle = "WooCommerce.WizardStepPrepTitle";
            public const string WizardStepPrepBody = "WooCommerce.WizardStepPrepBody";
            public const string WizardStepSchemaTitle = "WooCommerce.WizardStepSchemaTitle";
            public const string WizardStepSchemaBody = "WooCommerce.WizardStepSchemaBody";
            public const string WizardStepCredentialsTitle = "WooCommerce.WizardStepCredentialsTitle";
            public const string WizardStepTestTitle = "WooCommerce.WizardStepTestTitle";
            public const string WizardStepTestBody = "WooCommerce.WizardStepTestBody";
            public const string WizardStepOptionsTitle = "WooCommerce.WizardStepOptionsTitle";
            public const string WizardStepFinishTitle = "WooCommerce.WizardStepFinishTitle";
            public const string WizardStepFinishBody = "WooCommerce.WizardStepFinishBody";
            public const string OpenMappingAfterEnable = "WooCommerce.OpenMappingAfterEnable";
            public const string LabelStoreUrl = "WooCommerce.LabelStoreUrl";
            public const string LabelAdminUrl = "WooCommerce.LabelAdminUrl";
            public const string LabelConsumerKey = "WooCommerce.LabelConsumerKey";
            public const string LabelConsumerSecret = "WooCommerce.LabelConsumerSecret";
            public const string LabelCategoryMode = "WooCommerce.LabelCategoryMode";
            public const string LabelDispatchIds = "WooCommerce.LabelDispatchIds";
            public const string LabelTrackingRequired = "WooCommerce.LabelTrackingRequired";
            public const string SecretSavedPlaceholder = "WooCommerce.SecretSavedPlaceholder";
            public const string ButtonNext = "WooCommerce.ButtonNext";
            public const string ButtonBack = "WooCommerce.ButtonBack";
            public const string ButtonEnsureSchema = "WooCommerce.ButtonEnsureSchema";
            public const string ButtonSaveCredentials = "WooCommerce.ButtonSaveCredentials";
            public const string ButtonTestConnection = "WooCommerce.ButtonTestConnection";
            public const string ButtonFinish = "WooCommerce.ButtonFinish";
            public const string ButtonCancelWizard = "WooCommerce.ButtonCancelWizard";
            public const string SchemaOk = "WooCommerce.SchemaOk";
            public const string SchemaFailed = "WooCommerce.SchemaFailed";
            public const string CredentialsSaved = "WooCommerce.CredentialsSaved";
            public const string TestOk = "WooCommerce.TestOk";
            public const string TestFailed = "WooCommerce.TestFailed";
            public const string FinishBlockedNoTest = "WooCommerce.FinishBlockedNoTest";
            public const string FinishOk = "WooCommerce.FinishOk";
            public const string CryptoKeyMissing = "WooCommerce.CryptoKeyMissing";
            public const string IntegrationEnabled = "WooCommerce.IntegrationEnabled";
            public const string IntegrationDisabled = "WooCommerce.IntegrationDisabled";
            public const string CategoryModeAll = "WooCommerce.CategoryModeAll";
            public const string ButtonShowSecret = "WooCommerce.ButtonShowSecret";
            public const string ButtonHideSecret = "WooCommerce.ButtonHideSecret";
            public const string ProgressLabel = "WooCommerce.ProgressLabel";
            public const string ProgressStep1 = "WooCommerce.ProgressStep1";
            public const string ProgressStep2 = "WooCommerce.ProgressStep2";
            public const string ProgressStep3 = "WooCommerce.ProgressStep3";
            public const string ProgressStep4 = "WooCommerce.ProgressStep4";
            public const string ProgressStep5 = "WooCommerce.ProgressStep5";
            public const string ProgressStep6 = "WooCommerce.ProgressStep6";
            public const string WizardResumed = "WooCommerce.WizardResumed";
            public const string CredentialsSavedKeepBlank = "WooCommerce.CredentialsSavedKeepBlank";
            public const string SavedKeyHint = "WooCommerce.SavedKeyHint";
            public const string EnableNextStepHint = "WooCommerce.EnableNextStepHint";
            public const string ButtonEnableNow = "WooCommerce.ButtonEnableNow";
            public const string DisableHint = "WooCommerce.DisableHint";
            public const string Phase2ComingSoon = "WooCommerce.Phase2ComingSoon";
            public const string Phase2OpenMapping = "WooCommerce.Phase2OpenMapping";
            public const string MapNeedWooEnabled = "WooCommerce.MapNeedWooEnabled";
            public const string MapPageTitle = "WooCommerce.MapPageTitle";
            public const string MapPageSubtitle = "WooCommerce.MapPageSubtitle";
            public const string MapTabCategories = "WooCommerce.MapTabCategories";
            public const string MapTabAttrParents = "WooCommerce.MapTabAttrParents";
            public const string MapTabAttrVariants = "WooCommerce.MapTabAttrVariants";
            public const string MapTabMappings = "WooCommerce.MapTabMappings";
            public const string MapTabSavedMaps = "WooCommerce.MapTabSavedMaps";
            public const string MapTabMissingSku = "WooCommerce.MapTabMissingSku";
            public const string MapTabSync = "WooCommerce.MapTabSync";
            public const string MapCatHelp = "WooCommerce.MapCatHelp";
            public const string MapAttrParentsHelp = "WooCommerce.MapAttrParentsHelp";
            public const string MapAttrOptionsHelp = "WooCommerce.MapAttrOptionsHelp";
            public const string MapAttrVariantsLocked = "WooCommerce.MapAttrVariantsLocked";
            public const string MapPullAttrParents = "WooCommerce.MapPullAttrParents";
            public const string MapSyncAttrParents = "WooCommerce.MapSyncAttrParents";
            public const string MapPullAttrParentsOk = "WooCommerce.MapPullAttrParentsOk";
            public const string MapSaveAttrParents = "WooCommerce.MapSaveAttrParents";
            public const string MapAttrParentsSaved = "WooCommerce.MapAttrParentsSaved";
            public const string MapAttrNeedParents = "WooCommerce.MapAttrNeedParents";
            public const string MapMapHelp = "WooCommerce.MapMapHelp";
            public const string MapSavedMapsHelp = "WooCommerce.MapSavedMapsHelp";
            public const string MapSavedMapsPageInfo = "WooCommerce.MapSavedMapsPageInfo";
            public const string MapMissingSkuHelp = "WooCommerce.MapMissingSkuHelp";
            public const string MapFindSku = "WooCommerce.MapFindSku";
            public const string MapFindSkuBtn = "WooCommerce.MapFindSkuBtn";
            public const string MapClearFindSku = "WooCommerce.MapClearFindSku";
            public const string MapPullPageInfo = "WooCommerce.MapPullPageInfo";
            public const string MapPullPageInfoFiltered = "WooCommerce.MapPullPageInfoFiltered";
            public const string MapMissingSkuPageInfo = "WooCommerce.MapMissingSkuPageInfo";
            public const string MapDestNotMapped = "WooCommerce.MapDestNotMapped";
            public const string MapDestCreateParent = "WooCommerce.MapDestCreateParent";
            public const string MapDestCreateVariant = "WooCommerce.MapDestCreateVariant";
            public const string MapDestNotes = "WooCommerce.MapDestNotes";
            public const string MapNeedDestination = "WooCommerce.MapNeedDestination";
            public const string MapSaveSelected = "WooCommerce.MapSaveSelected";
            public const string MapSaveSelectedOk = "WooCommerce.MapSaveSelectedOk";
            public const string MapSaveSelectedNone = "WooCommerce.MapSaveSelectedNone";
            public const string MapSaveSelectedTip = "WooCommerce.MapSaveSelectedTip";
            public const string MapColAppliedTip = "WooCommerce.MapColAppliedTip";
            public const string MapColImportTip = "WooCommerce.MapColImportTip";
            public const string MapColModeTip = "WooCommerce.MapColModeTip";
            public const string MapColDestinationTip = "WooCommerce.MapColDestinationTip";
            public const string MapColMatchTip = "WooCommerce.MapColMatchTip";
            public const string MapColVariantTip = "WooCommerce.MapColVariantTip";
            public const string MapColSkuTip = "WooCommerce.MapColSkuTip";
            public const string MapColQtyTip = "WooCommerce.MapColQtyTip";
            public const string MapColPackTip = "WooCommerce.MapColPackTip";
            public const string MapColItemSkuTip = "WooCommerce.MapColItemSkuTip";
            public const string MapColSortTip = "WooCommerce.MapColSortTip";
            public const string MapImportCellTip = "WooCommerce.MapImportCellTip";
            public const string MapModeCellTip = "WooCommerce.MapModeCellTip";
            public const string MapDestCellTip = "WooCommerce.MapDestCellTip";
            public const string MapWriteMissingSkus = "WooCommerce.MapWriteMissingSkus";
            public const string MapMissingSkuWriteOk = "WooCommerce.MapMissingSkuWriteOk";
            public const string MapMissingSkuWritePartial = "WooCommerce.MapMissingSkuWritePartial";
            public const string MapSyncHelp = "WooCommerce.MapSyncHelp";
            public const string MapPullCategories = "WooCommerce.MapPullCategories";
            public const string MapSyncCategories = "WooCommerce.MapSyncCategories";
            public const string MapPullCategoriesOk = "WooCommerce.MapPullCategoriesOk";
            public const string MapPullCatsConfirm = "WooCommerce.MapPullCatsConfirm";
            public const string MapSyncCatsConfirm = "WooCommerce.MapSyncCatsConfirm";
            public const string MapCatPageInfo = "WooCommerce.MapCatPageInfo";
            public const string MapUnsavedLeave = "WooCommerce.MapUnsavedLeave";
            public const string MapBackToPreferences = "WooCommerce.MapBackToPreferences";
            public const string MapPullProducts = "WooCommerce.MapPullProducts";
            public const string MapSyncProducts = "WooCommerce.MapSyncProducts";
            public const string MapResetCatalog = "WooCommerce.MapResetCatalog";
            public const string MapResetCatalogOk = "WooCommerce.MapResetCatalogOk";
            public const string MapPullProductsOk = "WooCommerce.MapPullProductsOk";
            public const string MapPullProductsCapWarn = "WooCommerce.MapPullProductsCapWarn";
            public const string MapImportModeVariants = "WooCommerce.MapImportModeVariants";
            public const string MapImportModeParentItem = "WooCommerce.MapImportModeParentItem";
            public const string MapImportModeParentNotes = "WooCommerce.MapImportModeParentNotes";
            public const string MapImportModeExclude = "WooCommerce.MapImportModeExclude";
            public const string MapImportModeInherit = "WooCommerce.MapImportModeInherit";
            public const string MapSkuExistsInItems = "WooCommerce.MapSkuExistsInItems";
            public const string MapSkuWooWriteFailed = "WooCommerce.MapSkuWooWriteFailed";
            public const string MapExpandAll = "WooCommerce.MapExpandAll";
            public const string MapCollapseAll = "WooCommerce.MapCollapseAll";
            public const string MapPullAttributes = "WooCommerce.MapPullAttributes";
            public const string MapSyncAttributes = "WooCommerce.MapSyncAttributes";
            public const string MapPullAttributesOk = "WooCommerce.MapPullAttributesOk";
            public const string MapSaveAttributeMaps = "WooCommerce.MapSaveAttributeMaps";
            public const string MapAttrSaved = "WooCommerce.MapAttrSaved";
            public const string MapExistingTitle = "WooCommerce.MapExistingTitle";
            public const string MapSaveIncludes = "WooCommerce.MapSaveIncludes";
            public const string MapCatModeSaved = "WooCommerce.MapCatModeSaved";
            public const string MapCatRowSaved = "WooCommerce.MapCatRowSaved";
            public const string MapIncludesSaved = "WooCommerce.MapIncludesSaved";
            public const string MapSaved = "WooCommerce.MapSaved";
            public const string MapItemCreated = "WooCommerce.MapItemCreated";
            public const string MapDeleted = "WooCommerce.MapDeleted";
            public const string MapDryPush = "WooCommerce.MapDryPush";
            public const string MapPushEnabled = "WooCommerce.MapPushEnabled";
        }
    }
}
