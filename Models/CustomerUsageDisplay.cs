using System;
using TrackerSQL.Classes;

namespace TrackerSQL.Models
{
    /// <summary>
    /// Grid-friendly view of predicted contact usage (legacy grid bound to CustomerID).
    /// </summary>
    public class CustomerUsageDisplay
    {
        public long CustomerID { get; set; }
        public int LastCupCount { get; set; }
        public DateTime NextCoffeeBy { get; set; }
        public DateTime NextCleanOn { get; set; }
        public DateTime NextFilterEst { get; set; }
        public DateTime NextDescaleEst { get; set; }
        public DateTime NextServiceEst { get; set; }
        public double DailyConsumption { get; set; }
        public double FilterAveCount { get; set; }
        public double DescaleAveCount { get; set; }
        public double ServiceAveCount { get; set; }
        public double CleanAveCount { get; set; }

        public static CustomerUsageDisplay FromContactsUsage(ContactsUsage usage)
        {
            if (usage == null)
            {
                return new CustomerUsageDisplay();
            }

            var today = TimeZoneUtils.Now().Date;
            return new CustomerUsageDisplay
            {
                CustomerID = usage.ContactID,
                LastCupCount = usage.LastCupCount,
                NextCoffeeBy = usage.NextCoffeeBy?.Date ?? today,
                NextCleanOn = usage.NextCleanOn?.Date ?? today,
                NextFilterEst = usage.NextFilterEst?.Date ?? today,
                NextDescaleEst = usage.NextDescaleEst?.Date ?? today,
                NextServiceEst = usage.NextServiceEst?.Date ?? today,
                DailyConsumption = usage.DailyConsumption ?? 0.0,
                FilterAveCount = usage.FilterAveCount ?? 0.0,
                DescaleAveCount = usage.DescaleAveCount ?? 0.0,
                ServiceAveCount = usage.ServiceAveCount ?? 0.0,
                CleanAveCount = usage.CleanAveCount ?? 0.0
            };
        }
    }
}
