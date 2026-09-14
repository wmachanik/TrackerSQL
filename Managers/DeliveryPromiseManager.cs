using System;
using System.Collections.Generic;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Website-promised delivery/dispatch dates (customer SLA), separate from operational AreaPrepDays.
    /// </summary>
    public class DeliveryPromiseManager
    {
        public const byte AnyWorkdayDow = 255;

        private readonly DeliveryPromiseRuleRepository _repo = new DeliveryPromiseRuleRepository();

        public DeliveryPromiseResult GetPromisedDate(int? areaId, DateTime orderedAt)
        {
            var result = new DeliveryPromiseResult();
            if (!areaId.HasValue || areaId.Value <= 0)
                return result;

            List<DeliveryPromiseRule> rules = _repo.GetEnabledRulesForArea(areaId.Value);
            if (rules == null || rules.Count == 0)
                return result;

            DeliveryPromiseRule match = rules.FirstOrDefault(r => WindowContains(r, orderedAt));
            if (match == null)
                return result;

            result.FoundRule = true;
            result.PromiseKind = match.PromiseKind;
            result.RuleGroup = match.RuleGroup;
            result.Notes = match.Notes;
            result.PromisedDate = ComputeResultDate(match, orderedAt);
            return result;
        }

        /// <summary>
        /// True when the order was placed today and the scheduled date is sooner than the website promise.
        /// </summary>
        public bool IsSoonerThanPromiseException(
            int? areaId,
            DateTime orderDate,
            DateTime requiredByDate,
            DateTime? now = null)
        {
            DateTime today = (now ?? TimeZoneUtils.Now()).Date;
            if (orderDate.Date != today)
                return false;

            DeliveryPromiseResult promise = GetPromisedDate(areaId, now ?? TimeZoneUtils.Now());
            if (!promise.FoundRule || !promise.PromisedDate.HasValue)
                return false;

            return requiredByDate.Date < promise.PromisedDate.Value.Date;
        }

        private static bool WindowContains(DeliveryPromiseRule rule, DateTime orderedAt)
        {
            if (rule == null)
                return false;

            int minutes = orderedAt.Hour * 60 + orderedAt.Minute;

            // Time-of-day only on workdays (courier rules).
            if (rule.WindowStartDow == AnyWorkdayDow || rule.WindowEndDow == AnyWorkdayDow)
            {
                if (IsWeekend(orderedAt.DayOfWeek))
                    return false;
                return minutes >= rule.WindowStartMinutes && minutes < rule.WindowEndMinutes;
            }

            int start = ToWeekMinutes(rule.WindowStartDow, rule.WindowStartMinutes);
            int end = ToWeekMinutes(rule.WindowEndDow, rule.WindowEndMinutes);
            int now = ToWeekMinutes((byte)orderedAt.DayOfWeek, (short)minutes);

            if (start == end)
                return false;

            if (start < end)
                return now >= start && now < end;

            // Wraps weekend (e.g. Fri 13:00 → Tue 12:00).
            return now >= start || now < end;
        }

        private static DateTime? ComputeResultDate(DeliveryPromiseRule rule, DateTime orderedAt)
        {
            string mode = (rule.ResultMode ?? string.Empty).Trim();
            if (string.Equals(mode, "FixedDow", StringComparison.OrdinalIgnoreCase))
            {
                if (!rule.ResultDow.HasValue)
                    return null;
                return NextOrSameDow(orderedAt.Date, (DayOfWeek)rule.ResultDow.Value, excludeTodayIfPastWindow: false, orderedAt);
            }

            if (string.Equals(mode, "SameWorkday", StringComparison.OrdinalIgnoreCase))
            {
                DateTime d = orderedAt.Date;
                if (IsWeekend(d.DayOfWeek))
                    d = NextWorkday(d);
                d = ApplyCourierAdjustments(d, orderedAt, rule, sameDay: true);
                return d;
            }

            if (string.Equals(mode, "NextWorkday", StringComparison.OrdinalIgnoreCase))
            {
                DateTime d = NextWorkday(orderedAt.Date);
                d = ApplyCourierAdjustments(d, orderedAt, rule, sameDay: false);
                return d;
            }

            return null;
        }

        private static DateTime ApplyCourierAdjustments(
            DateTime candidate,
            DateTime orderedAt,
            DeliveryPromiseRule rule,
            bool sameDay)
        {
            DateTime d = candidate;

            // RSA: Wednesday after noon → Friday (next-workday path already starts Thu;
            // same-day Wed before noon stays Wed unless NoThursday — handled below).
            if (rule.WedAfterNoonToFriday)
            {
                if (sameDay && orderedAt.DayOfWeek == DayOfWeek.Wednesday
                    && (orderedAt.Hour * 60 + orderedAt.Minute) >= 720)
                {
                    d = orderedAt.Date.AddDays(2); // Fri
                }
                else if (!sameDay && orderedAt.DayOfWeek == DayOfWeek.Wednesday)
                {
                    // After noon Wed → next workday would be Thu; website says Fri.
                    d = orderedAt.Date.AddDays(2);
                }
                else if (sameDay && orderedAt.DayOfWeek == DayOfWeek.Thursday)
                {
                    d = orderedAt.Date.AddDays(1); // Fri
                }
            }

            if (rule.NoThursdayDispatch)
            {
                while (d.DayOfWeek == DayOfWeek.Thursday || IsWeekend(d.DayOfWeek))
                    d = d.AddDays(1);
            }
            else
            {
                while (IsWeekend(d.DayOfWeek))
                    d = d.AddDays(1);
            }

            return d;
        }

        /// <summary>
        /// Next occurrence of target DOW on/after order date.
        /// If that DOW is today, use today only when the matching window can still imply "this week's slot";
        /// otherwise advance a week (e.g. ordered Mon for a Mon delivery window that already closed → next Mon).
        /// </summary>
        private static DateTime NextOrSameDow(
            DateTime orderDate,
            DayOfWeek target,
            bool excludeTodayIfPastWindow,
            DateTime orderedAt)
        {
            int delta = ((int)target - (int)orderDate.DayOfWeek + 7) % 7;
            // If result DOW is today, website promise is usually the *upcoming* delivery for the open window,
            // which may be today only when ResultDow == today and we're still before that day's delivery —
            // keep same-day when delta==0 (window matched already implies we're in a slot that yields this DOW).
            if (delta == 0 && excludeTodayIfPastWindow)
                delta = 7;
            return orderDate.AddDays(delta);
        }

        private static DateTime NextWorkday(DateTime fromDate)
        {
            DateTime d = fromDate.AddDays(1);
            while (IsWeekend(d.DayOfWeek))
                d = d.AddDays(1);
            return d;
        }

        private static bool IsWeekend(DayOfWeek dow)
        {
            return dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday;
        }

        private static int ToWeekMinutes(byte dow, short minutesFromMidnight)
        {
            int m = minutesFromMidnight;
            if (m < 0) m = 0;
            if (m > 1440) m = 1440;
            return (dow % 7) * 1440 + m;
        }
    }
}
