using System;
using System.Collections.Generic;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    /// <summary>
    /// Repository for SysDataTbl - system configuration and runtime settings.
    /// </summary>
    public class SysDataRepository : RepositoryBase<SysData>
    {
        private const int SystemDataID = 1;

        protected override string TableName => "SysDataTbl";
        protected override string KeyColumn => "ID";

        protected override string CoreColumns =>
            "ID, DoRecurringOrders, LastRecurringDate, DateLastPrepDateCalcd, MinReminderDate, GroupItemServiceTypeID, InternalContactIDs";

        /// <summary>
        /// Gets the singleton system data record.
        /// </summary>
        public SysData GetSystemData()
        {
            return GetById(SystemDataID);
        }

        /// <summary>
        /// Updates the singleton system data record.
        /// </summary>
        public bool UpdateSystemData(SysData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            if (data.ID == 0)
            {
                data.ID = SystemDataID;
            }

            return Update(data) > 0;
        }

        public DateTime GetMinReminderDate()
        {
            var data = GetSystemData();
            return data?.MinReminderDate ?? DateTime.MinValue;
        }

        public int? GetGroupItemServiceTypeId()
        {
            return GetSystemData()?.GroupItemServiceTypeID;
        }

        public List<int> GetInternalContactIds()
        {
            var contactIds = new List<int>();
            var data = GetSystemData();
            string idsString = data?.InternalContactIDs;

            if (string.IsNullOrWhiteSpace(idsString))
            {
                return contactIds;
            }

            foreach (var idString in idsString.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(idString.Trim(), out int contactId))
                {
                    contactIds.Add(contactId);
                }
            }

            return contactIds;
        }
    }
}