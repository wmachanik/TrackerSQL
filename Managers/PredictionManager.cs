using System;
using System.Collections.Generic;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Recalculates ContactsItemsPredictedTbl next-service dates after Order Done.
    /// Port of legacy GeneralTrackerDbTools.UpdatePredictions / CalcAndSaveNextRequiredDates.
    /// </summary>
    public class PredictionManager
    {
        private const int MaxRollingAveValues = 6;

        private readonly ContactUsageLinesRepository _usageLinesRepository;
        private readonly ContactsUsageRepository _contactsUsageRepository;
        private readonly TrackedServiceItemRepository _trackedServiceItemRepository;

        public PredictionManager()
            : this(new ContactUsageLinesRepository(), new ContactsUsageRepository(), new TrackedServiceItemRepository())
        {
        }

        public PredictionManager(
            ContactUsageLinesRepository usageLinesRepository,
            ContactsUsageRepository contactsUsageRepository,
            TrackedServiceItemRepository trackedServiceItemRepository)
        {
            _usageLinesRepository = usageLinesRepository;
            _contactsUsageRepository = contactsUsageRepository;
            _trackedServiceItemRepository = trackedServiceItemRepository;
        }

        /// <summary>
        /// After cup count is saved, recompute NextCoffeeBy and related service dates from usage.
        /// </summary>
        public bool UpdatePredictions(int contactId, int lastCupCount)
        {
            if (contactId <= 0 || lastCupCount <= 0)
                return false;

            DateTime installDate = _usageLinesRepository.GetInstallDate(contactId);
            if (installDate != DateTime.MinValue)
                return CalcAndSaveNextRequiredDates(contactId);

            double daily = CalcAveConsumption(contactId, SystemConstants.ServiceTypeConstants.Coffee, 5.0, true);
            if (daily <= 0)
                daily = 5.0;

            DateTime nextCoffee = TimeZoneUtils.Now().Date.AddDays(20);
            var fallback = new ContactsUsage
            {
                ContactID = contactId,
                LastCupCount = lastCupCount,
                NextCoffeeBy = nextCoffee,
                NextCleanOn = nextCoffee.AddDays(20),
                NextFilterEst = nextCoffee.AddDays(30),
                NextDescaleEst = nextCoffee.AddDays(30),
                NextServiceEst = nextCoffee.AddYears(1),
                DailyConsumption = daily,
                CleanAveCount = 200.0,
                FilterAveCount = 300.0,
                DescaleAveCount = 500.0,
                ServiceAveCount = 10000.0
            };

            bool ok = _contactsUsageRepository.UpdatePredictedServiceFields(contactId, fallback);
            AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                $"PredictionManager: fallback NextCoffeeBy={nextCoffee:yyyy-MM-dd} Contact={contactId} ok={ok}");
            return ok;
        }

        public bool CalcAndSaveNextRequiredDates(int contactId)
        {
            var tracked = _trackedServiceItemRepository.GetAllForPrediction();
            if (tracked == null || tracked.Count == 0)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                    $"PredictionManager: no TrackedServiceItems for Contact={contactId}");
                return false;
            }

            var existing = _contactsUsageRepository.GetByContactId(contactId) ?? new ContactsUsage { ContactID = contactId };
            double dailyAverage = SystemConstants.BusinessConstants.TypicalAverageConsumption;
            int index = 0;

            // Items that set daily average (coffee) first.
            while (index < tracked.Count && tracked[index].ThisItemSetsDailyAverage == true)
            {
                ApplyTrackedItem(contactId, tracked[index], existing, dailyAverage: 0.0, out dailyAverage);
                index++;
            }

            for (; index < tracked.Count; index++)
                ApplyTrackedItem(contactId, tracked[index], existing, dailyAverage, out _);

            bool ok = _contactsUsageRepository.UpdatePredictedServiceFields(contactId, existing);
            AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                $"PredictionManager: Contact={contactId} NextCoffeeBy={existing.NextCoffeeBy:yyyy-MM-dd} Daily={existing.DailyConsumption} ok={ok}");
            return ok;
        }

        /// <summary>
        /// Bulk recalculation of prediction averages and NextCoffeeBy (does not touch recurring dates).
        /// </summary>
        public List<PredictionBulkRecalcResult> RecalculateBulk(bool staleOnly)
        {
            var candidates = _contactsUsageRepository.GetBulkRecalcCandidates(staleOnly)
                ?? new List<PredictionBulkRecalcCandidate>();
            var results = new List<PredictionBulkRecalcResult>();

            foreach (var candidate in candidates)
            {
                var row = new PredictionBulkRecalcResult
                {
                    ContactID = candidate.ContactID,
                    CompanyName = candidate.CompanyName ?? string.Empty,
                    LastCoffeeDate = candidate.LastCoffeeDate.HasValue
                        ? candidate.LastCoffeeDate.Value.ToString("yyyy-MM-dd")
                        : "(none)",
                    PreviousNextCoffee = candidate.PreviousNextCoffeeBy.HasValue
                        ? candidate.PreviousNextCoffeeBy.Value.ToString("yyyy-MM-dd")
                        : "(none)",
                    PreviousDaily = candidate.PreviousDailyConsumption.HasValue
                        ? candidate.PreviousDailyConsumption.Value.ToString("0.####")
                        : "(n/a)"
                };

                try
                {
                    bool ok = CalcAndSaveNextRequiredDates(candidate.ContactID);
                    var after = _contactsUsageRepository.GetByContactId(candidate.ContactID);
                    row.NewNextCoffee = after?.NextCoffeeBy.HasValue == true
                        ? after.NextCoffeeBy.Value.ToString("yyyy-MM-dd")
                        : "(none)";
                    row.NewDaily = after?.DailyConsumption.HasValue == true
                        ? after.DailyConsumption.Value.ToString("0.####")
                        : "(n/a)";

                    if (!ok)
                    {
                        row.Result = "Failed";
                    }
                    else
                    {
                        bool nextChanged = !string.Equals(row.PreviousNextCoffee, row.NewNextCoffee, StringComparison.Ordinal);
                        bool dailyChanged = !string.Equals(row.PreviousDaily, row.NewDaily, StringComparison.Ordinal);
                        row.Changed = nextChanged || dailyChanged;
                        row.Result = row.Changed ? "Changed" : "Unchanged";
                    }
                }
                catch (Exception ex)
                {
                    row.NewNextCoffee = row.PreviousNextCoffee;
                    row.NewDaily = row.PreviousDaily;
                    row.Changed = false;
                    row.Result = "Error: " + ex.Message;
                }

                results.Add(row);
            }

            results.Sort((a, b) =>
            {
                int rankA = ResultRank(a.Result);
                int rankB = ResultRank(b.Result);
                int cmp = rankA.CompareTo(rankB);
                if (cmp != 0)
                    return cmp;
                return string.Compare(a.CompanyName, b.CompanyName, StringComparison.OrdinalIgnoreCase);
            });

            AppLogger.WriteLog(SystemConstants.LogTypes.System,
                "PredictionManager: bulk recalc staleOnly=" + staleOnly
                + " candidates=" + candidates.Count
                + " changed=" + results.FindAll(r => r.Changed).Count);

            return results;
        }

        private static int ResultRank(string result)
        {
            if (string.Equals(result, "Changed", StringComparison.OrdinalIgnoreCase))
                return 0;
            if (!string.IsNullOrEmpty(result) && result.StartsWith("Error", StringComparison.OrdinalIgnoreCase))
                return 1;
            if (string.Equals(result, "Failed", StringComparison.OrdinalIgnoreCase))
                return 2;
            return 3;
        }

        private void ApplyTrackedItem(
            int contactId,
            TrackedServiceItem tracked,
            ContactsUsage target,
            double dailyAverage,
            out double computedDailyAverage)
        {
            computedDailyAverage = dailyAverage;
            int serviceTypeId = tracked.ItemServiceTypeID ?? 0;
            double typical = tracked.TypicalAvePerItem ?? SystemConstants.BusinessConstants.TypicalAverageConsumption;
            bool perDayCalc = dailyAverage <= 0.0;

            double itemAverage = CalcAveConsumption(contactId, serviceTypeId, typical, perDayCalc);
            if (perDayCalc)
                computedDailyAverage = itemAverage;

            var latest = GetLatestUsageData(contactId, serviceTypeId);
            if (latest.UsageDate == DateTime.MinValue)
                latest.UsageDate = _usageLinesRepository.GetInstallDate(contactId);

            int daysToAdd = !perDayCalc
                ? (int)Math.Round(latest.LastQty * itemAverage / dailyAverage, 0)
                : (int)Math.Round(latest.LastQty * 100.0 / itemAverage, 0);

            DateTime nextDate = AddHolidayExtension(contactId, latest.UsageDate.AddDays(daysToAdd), serviceTypeId);
            ApplyField(target, tracked.UsageDateFieldName, nextDate, tracked.UsageAveFieldName, itemAverage);
        }

        private static void ApplyField(
            ContactsUsage target,
            string dateField,
            DateTime nextDate,
            string aveField,
            double average)
        {
            if (string.Equals(dateField, "NextCoffeeBy", StringComparison.OrdinalIgnoreCase))
                target.NextCoffeeBy = nextDate.Date;
            else if (string.Equals(dateField, "NextCleanOn", StringComparison.OrdinalIgnoreCase))
                target.NextCleanOn = nextDate.Date;
            else if (string.Equals(dateField, "NextFilterEst", StringComparison.OrdinalIgnoreCase))
                target.NextFilterEst = nextDate.Date;
            else if (string.Equals(dateField, "NextDescaleEst", StringComparison.OrdinalIgnoreCase))
                target.NextDescaleEst = nextDate.Date;
            else if (string.Equals(dateField, "NextServiceEst", StringComparison.OrdinalIgnoreCase))
                target.NextServiceEst = nextDate.Date;

            if (string.Equals(aveField, "DailyConsumption", StringComparison.OrdinalIgnoreCase))
                target.DailyConsumption = average;
            else if (string.Equals(aveField, "CleanAveCount", StringComparison.OrdinalIgnoreCase))
                target.CleanAveCount = average;
            else if (string.Equals(aveField, "FilterAveCount", StringComparison.OrdinalIgnoreCase))
                target.FilterAveCount = average;
            else if (string.Equals(aveField, "DescaleAveCount", StringComparison.OrdinalIgnoreCase))
                target.DescaleAveCount = average;
            else if (string.Equals(aveField, "ServiceAveCount", StringComparison.OrdinalIgnoreCase))
                target.ServiceAveCount = average;
        }

        private LineUsageData GetLatestUsageData(int contactId, int serviceTypeId)
        {
            var line = _usageLinesRepository.GetLatestUsageLine(contactId, serviceTypeId);
            return new LineUsageData
            {
                LastCount = line?.CupCount ?? 0,
                LastQty = line?.Qty ?? 0.0,
                UsageDate = line?.UsageDate?.Date ?? DateTime.MinValue
            };
        }

        private double CalcAveConsumption(
            int contactId,
            int serviceTypeId,
            double typicalAverageConsumption,
            bool perDayCalc)
        {
            double result = typicalAverageConsumption;
            var samples = new List<double>();
            int sampleCount = 0;
            var lines = _usageLinesRepository.GetLast10UsageLines(contactId, serviceTypeId);
            lines = lines
                .OrderBy(l => l.UsageDate ?? DateTime.MinValue)
                .ToList();

            if (lines.Count > 1)
            {
                DateTime cursorDate = lines[0].UsageDate?.Date ?? DateTime.MinValue;
                int cupCount = lines[0].CupCount ?? 0;

                for (int i = 1; i < lines.Count && sampleCount < MaxRollingAveValues; i++)
                {
                    int lineServiceType = lines[i].ItemServiceTypeID ?? 0;
                    int lineCups = lines[i].CupCount ?? 0;
                    DateTime lineDate = lines[i].UsageDate?.Date ?? DateTime.MinValue;

                    if (cupCount < lineCups)
                    {
                        if (lineServiceType == serviceTypeId && lineDate > cursorDate)
                        {
                            int cupDelta = lineCups - cupCount;
                            if (perDayCalc)
                            {
                                int days = (lineDate - cursorDate).Days;
                                if (days > 0)
                                    samples.Add(Math.Round((double)cupDelta / days, 5));
                            }
                            else
                            {
                                samples.Add(cupDelta);
                            }

                            sampleCount++;
                            cursorDate = lineDate;
                            cupCount = lineCups;
                        }
                        else if (lineServiceType >= 11 && lineServiceType <= 16)
                        {
                            cursorDate = cursorDate.AddDays(HolidayAwayDays(lineServiceType));
                        }
                    }
                }

                if (samples.Count >= MaxRollingAveValues)
                {
                    double max = samples[0];
                    for (int i = 1; i < samples.Count; i++)
                    {
                        if (samples[i] > max)
                            max = samples[i];
                    }
                    samples.Remove(max);
                }

                double sum = samples.Sum();
                if (sum > 0.0 && samples.Count > 0)
                    result = Math.Round(sum / samples.Count, SystemConstants.DatabaseConstants.NumDecimalPoints);
            }
            else
            {
                result = !perDayCalc ? typicalAverageConsumption : 5.0;
            }

            if (result <= 0.0)
                result = typicalAverageConsumption;

            return result;
        }

        private DateTime AddHolidayExtension(int contactId, DateTime date, int serviceTypeId)
        {
            var last10 = _usageLinesRepository.GetLast10UsageLines(contactId, 0);
            for (int i = 0; i < last10.Count && i < MaxRollingAveValues; i++)
            {
                int lineServiceType = last10[i].ItemServiceTypeID ?? 0;
                if (lineServiceType == 0)
                    continue;

                if (lineServiceType >= 11 && lineServiceType <= 16)
                    date = date.AddMonths(HolidayAwayDays(lineServiceType));
                else if (lineServiceType == serviceTypeId)
                    break;
            }

            return date.Date;
        }

        private static int HolidayAwayDays(int serviceTypeId)
        {
            switch (serviceTypeId)
            {
                case 11: return -7;
                case 12: return -14;
                case 13: return -21;
                case 14: return -31;
                case 15: return -42;
                case 16: return -61;
                default: return 0;
            }
        }

        private struct LineUsageData
        {
            public int LastCount;
            public double LastQty;
            public DateTime UsageDate;
        }
    }

    public class PredictionBulkRecalcResult
    {
        public int ContactID { get; set; }
        public string CompanyName { get; set; }
        public string LastCoffeeDate { get; set; }
        public string PreviousNextCoffee { get; set; }
        public string NewNextCoffee { get; set; }
        public string PreviousDaily { get; set; }
        public string NewDaily { get; set; }
        public string Result { get; set; }
        public bool Changed { get; set; }
    }
}
