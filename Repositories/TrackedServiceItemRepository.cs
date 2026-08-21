using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class TrackedServiceItemRepository : RepositoryBase<TrackedServiceItem>
    {
        protected override string TableName => "TrackedServiceItemsTbl";
        protected override string KeyColumn => "TrackerServiceItemID";

        protected override string CoreColumns =>
            "TrackerServiceItemID, ItemServiceTypeID, TypicalAvePerItem, UsageDateFieldName, UsageAveFieldName, ThisItemSetsDailyAverage, Notes";

        /// <summary>
        /// Prediction drivers: coffee (sets daily average) first, then other tracked services.
        /// </summary>
        public List<TrackedServiceItem> GetAllForPrediction()
        {
            string sql = $@"
                SELECT {CoreColumns}
                FROM {TableName}
                ORDER BY ThisItemSetsDailyAverage DESC, ItemServiceTypeID, TrackerServiceItemID";

            var list = new List<TrackedServiceItem>();
            using (var rdr = ExecReader(sql, null))
            {
                while (rdr != null && rdr.Read())
                    list.Add(DbMapper.Map<TrackedServiceItem>(rdr));
            }

            return list;
        }
    }
}
