using System;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class SystemPreferencesHdrRepository : RepositoryBase<SystemPreferencesHdr>
    {
        private const int SingletonId = 1;

        protected override string TableName => "SystemPreferencesHdrTbl";
        protected override string KeyColumn => "PrefsID";

        protected override string CoreColumns =>
            "PrefsID, WooCommerceEnabled, WooWizardCompleted, UpdatedAt, UpdatedBy";

        public SystemPreferencesHdr GetHeader()
        {
            return GetById(SingletonId) ?? new SystemPreferencesHdr { PrefsID = SingletonId };
        }

        public bool SaveHeader(SystemPreferencesHdr header, string updatedBy)
        {
            if (header == null) throw new ArgumentNullException(nameof(header));
            header.PrefsID = SingletonId;
            header.UpdatedAt = DateTime.UtcNow;
            header.UpdatedBy = updatedBy ?? string.Empty;

            var existing = GetById(SingletonId);
            if (existing == null)
                return Insert(header) > 0;

            return Update(header) > 0;
        }

        public bool TableExists()
        {
            try
            {
                using (var db = CreateDb())
                {
                    int n = db.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM sys.tables WHERE name = N'SystemPreferencesHdrTbl'");
                    return n > 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
