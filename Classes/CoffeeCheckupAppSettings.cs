using System;
using System.Configuration;
using System.Web.Configuration;

namespace TrackerSQL.Classes
{
    /// <summary>Coffee checkup / reminder behaviour from appSettings (editable in System Preferences).</summary>
    public static class CoffeeCheckupAppSettings
    {
        public const string KeyMaxReminders = "CoffeeCheckupMaxReminders";
        public const string KeyDaysPushEachReminder = "CoffeeCheckupDaysPushEachReminder";
        public const string KeyForceReminderDelayCount = "CoffeeCheckupForceReminderDelayCount";
        public const string KeyEscalationStepDays = "CoffeeCheckupEscalationStepDays";
        public const string KeyReminderWindowDays = "CoffeeCheckupReminderWindowDays";

        public static int MaxReminders =>
            ConfigHelper.GetInt(KeyMaxReminders, SystemConstants.CheckupConstants.MaxReminders);

        /// <summary>When &gt; 0, push Next Coffee by this many days after every reminder sent.</summary>
        public static int DaysPushEachReminder =>
            ConfigHelper.GetInt(KeyDaysPushEachReminder, 0);

        /// <summary>After this many reminders (1-based count after increment), escalation may apply if DaysPushEachReminder is 0.</summary>
        public static int ForceReminderDelayCount =>
            ConfigHelper.GetInt(KeyForceReminderDelayCount, SystemConstants.CheckupConstants.ForceReminderDelayCount);

        /// <summary>Days added per escalation step (multiplied by how far past ForceReminderDelayCount).</summary>
        public static int EscalationStepDays =>
            ConfigHelper.GetInt(KeyEscalationStepDays, 10);

        public static int DefaultReminderWindowDays =>
            ConfigHelper.GetInt(KeyReminderWindowDays, SystemConstants.CheckupConstants.DefaultReminderWindowDays);

        public static void LoadIntoFields(
            out int maxReminders,
            out int daysPushEach,
            out int forceDelayCount,
            out int escalationStepDays,
            out int windowDays)
        {
            maxReminders = MaxReminders;
            daysPushEach = DaysPushEachReminder;
            forceDelayCount = ForceReminderDelayCount;
            escalationStepDays = EscalationStepDays;
            windowDays = DefaultReminderWindowDays;
        }

        public static string Save(
            int maxReminders,
            int daysPushEach,
            int forceDelayCount,
            int escalationStepDays,
            int windowDays,
            string appPath)
        {
            if (maxReminders < 1 || maxReminders > 99)
                return "Max reminders must be between 1 and 99.";
            if (daysPushEach < 0 || daysPushEach > 90)
                return "Days to push after each reminder must be between 0 and 90 (0 = use escalation rules).";
            if (forceDelayCount < 1 || forceDelayCount > maxReminders)
                return "Escalation after reminder # must be between 1 and max reminders.";
            if (escalationStepDays < 1 || escalationStepDays > 90)
                return "Escalation step days must be between 1 and 90.";
            if (windowDays < 1 || windowDays > 60)
                return "Default reminder window must be between 1 and 60 days.";

            try
            {
                var config = WebConfigurationManager.OpenWebConfiguration(appPath ?? "/");
                var appSettings = config.AppSettings.Settings;
                SetOrAdd(appSettings, KeyMaxReminders, maxReminders.ToString());
                SetOrAdd(appSettings, KeyDaysPushEachReminder, daysPushEach.ToString());
                SetOrAdd(appSettings, KeyForceReminderDelayCount, forceDelayCount.ToString());
                SetOrAdd(appSettings, KeyEscalationStepDays, escalationStepDays.ToString());
                SetOrAdd(appSettings, KeyReminderWindowDays, windowDays.ToString());
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                return null;
            }
            catch (Exception ex)
            {
                return "Could not save to web.config: " + ex.Message;
            }
        }

        private static void SetOrAdd(KeyValueConfigurationCollection settings, string key, string value)
        {
            if (settings[key] == null)
                settings.Add(key, value);
            else
                settings[key].Value = value;
        }
    }
}