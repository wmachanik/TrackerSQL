using System;
using TrackerSQL.Controls;

namespace TrackerSQL.Classes
{
    public static class SystemConstants
    {
        // Database specific constants
        public static class DatabaseConstants
        {
            public const int NumDecimalPoints = 4;
            public const string ConnectionStringName = "Tracker08ConnectionString";
            public const int InvalidID = -1;
            public const string InvalidIDStr = "-1";
            public const string NullDateStr = "1980/01/01";
            public static DateTime SystemMinDate = DateTime.Parse(NullDateStr).Date;
        }
        //formating constants
        public static class FormatConstants
        {
            public const string DateFormat = "dd/MM/yyyy";
            public const string DigitFormat = "0.###";
        }
        // Customer related constants
        public static class CustomerConstants
        {
            public const long SundryCustomerID = 9; // Default customer ID
            public const string SundryCustomerIDStr = "9";
            public const string SundryCustomerName = "ZZName";
            public const string SundryCustomerNamePrefix = "ZZ";
            public const long GeneralOrOtherCustomerID = 9; // Same as sundry
            public const string GeneralOrOtherCustomerIDStr = "9";
            public const int MaxReminders = 10;

        }

        // Customer Type constants
        public static class CustomerTypeConstants
        {
            public const int CoffeeAndMaintenance = 1;
            public const int ServiceContract = 2;
            public const int CoffeeOnly = 3;
            public const int Rental = 4;
            public const int OutrightPurchase = 5;
            public const int ServiceOnly = 6;
            public const int Other = 7;
            public const int GreenCoffeeOnly = 8;
            public const int InfoOnly = 9;
        }

        // Delivery and Personnel constants
        public static class DeliveryConstants
        {
            public const int DefaultDeliveryPersonID = 3;
            public const string DefaultDeliveryPersonAbbr = "SQ";
            public const int CourierDeliveryID = 7;
            public const string CourierDeliveryIDStr = "Cour";
            public const int CollectionID = 10;
            public const string CollectionIDStr = "Cllct";
            public const int ParcelDispatchID = 5;
            public const string ParcelDispatchIDStr = "Prgo";
        }

        // Service Type constants
        public static class ServiceTypeConstants
        {
            public const int Clean = 1;
            public const int Coffee = 2;
            public const int Count = 3;
            public const int Descale = 4;
            public const int Filter = 5;
            public const int SwopCollect = 6;
            public const int SwopStart = 7;
            public const int SwopStop = 8;
            public const int SwopReturn = 9;
            public const int Service = 10;
            public const int OneWeekHoliday = 11;
            public const int TwoWeekHoliday = 12;
            public const int ThreeWeekHoliday = 13;
            public const int OneMonthHoliday = 14;
            public const int SixWeekHoliday = 15;
            public const int TwoMonthHoliday = 16;
            public const int NotApplicable = 17;
            public const int Maintenance = 18;
            public const int GreenBean = 19;
            public const int GroupItem = 21;

            // String versions
            public const string CleanStr = "1";
            public const string CoffeeStr = "2";
            public const string CountStr = "3";
            public const string DescaleStr = "4";
            public const string FilterStr = "5";
            public const string SwopCollectStr = "6";
            public const string SwopStartStr = "7";
            public const string SwopStopStr = "8";
            public const string SwopReturnStr = "9";
            public const string ServiceStr = "10";
        }

        // Payment and Pricing constants
        public static class PaymentConstants
        {
            public const int DefaultPaymentTermID = 5;
            public const int DefaultPriceLevel = 1;
        }

        // City and Location constants
        public static class LocationConstants
        {
            public const int DefaultCityID = 1;
        }

        // Item and Product constants
        public static class ItemConstants
        {
            public const int NoteItemTimeID = 100;
            public const int RepairCheckItemID = 36; // "Should this not rather be a repair?"
            public const string WhiteFilterSKU = "8ClarFltr";
            public const string BlueFilterSKU = "8ClarBlue";
            public const int WaterFilterPackagingID = 8;
            public const int BlueWaterFilterPackagingID = 9;
            public const int DefaultPackagingID = 0; // n/a packaging
        }

        // Business Logic constants
        public static class BusinessConstants
        {
            public const double TypicalAverageConsumption = 5.0;
            public const double TypicalCleanConsumption = 200.0;
            public const double TypicalDescaleConsumption = 500.0;
            public const double TypicalFilterConsumption = 300.0;
            public const int TypicalCupsPerKg = 100;
            public const int MaxRollingAverageValues = 6;
        }

        // Session Management constants
        public static class SessionConstants
        {
            public const string DataAccessError = "DataAccessError";
            public const string BoundCustomerID = "BoundCustomerID";
            public const string BoundDeliveryDate = "BoundDeliveryDate";
            public const string BoundNotes = "BoundNotes";
            public const string BoundOldDeliveryDate = "BoundOldDeliveryDate";
            public const string RunningOnMobile = "RunningOnMoble";
        }

        // UI and Display constants
        public static class UIConstants
        {
            public const string PORequiredText = "!!!PO required!!!";
            public const string PORequiredShort = "!!!";
            public const string RepairWarningText = "Should this not rather be a repair?";
            public const string PackingCheckText = "Please check packing setting";
            public const string LastOrderGroupNote = "last order used a group item, so next item in group selected.";
        }

        // Timeout and Performance constants
        public static class PerformanceConstants
        {
            public const int AsyncPostBackTimeout = 400; // milliseconds
            public const int TimerInterval = 500; // milliseconds
            public const int LogFileMaxLines = 5000;
        }

        // GridView Column constants (for pages that use magic numbers)
        public static class GridColumnConstants
        {
            // Repairs GridView columns
            public const int RepairContactNameCol = 4;
            public const int RepairJobCardCol = 5;
            public const int RepairEquipmentCol = 6;
            public const int RepairMachineSnCol = 7;
            public const int RepairFaultCol = 8;
            public const int RepairFaultDescCol = 9;
            public const int RepairOrderIdCol = 10;

            // Order GridView columns
            public const int OrderIDCol = 4;
            public const int BackgroundColorCol = 4;
        }

        // User-specific constants
        public static class UserConstants
        {
            public const string AdminUserName = "warren"; // User with special privileges
        }

        // URL Parameter constants
        public static class UrlParameterConstants
        {
            public const string CustomerID = "CoID";
            public const string CustomerName = "Name";
            public const string CompanyName = "CoName";
            public const string Email = "EMail";
            public const string LastOrder = "LastOrder";
            public const string SKU1 = "SKU1";
            public const string RepairID = "RepairID";
        }
        // Log categories for logging purposes
        public static class LogTypes
        {
            public const string Customers = "customers";
            public const string Repairs = "repairs";
            public const string Orders = "orders";
            public const string SendCheckup = "sendcheckup";
            public const string Email = "email";
            public const string Delivery = "delivery";
            public const string Login = "login";
            public const string System = "system";
            public const string Database = "database";
            // Add more as needed for your application
        }

        public static class RepairConstants
        {
            public const string OrderNotesRepairStatusStartTag = "[RepairStatus:";
           // public const string OrderNoteRepairStatusDelimiter = ".";
            public const string OrderNoteRepairStatusTagEnd = "]";
            public const string DefaultEquipName = "unknown";
        }

        public static class EmailConstants
        {
            public const string DefaultContact = "Coffee Lover";
            public const string DefaultAdminEmail = "orders@quaffee.co.za";
        }
        public static class CheckupConstants
        {
            public const int ForceReminderDelayCount = 4;
            public const int MaxReminders = 7;
            public const int DefaultReminderWindowDays = 9;
            public const int DefaultMinimumMonthlyRecurringDays = 20;
        }

        public static class HolidayClosureConstants
        {
            // AppSettings key name
            public const string ImminentWindowDaysSettingKey = "HolidayClosureImminentDays";
            // Fallback default if setting missing or invalid
            public const int DefaultImminentWindowDays = 4;
        }
    }
}