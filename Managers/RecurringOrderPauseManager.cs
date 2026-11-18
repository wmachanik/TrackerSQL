using System;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Pauses an enabled recurring order by advancing each line's last-done date
    /// so the next required date falls on or after the chosen resume date.
    /// </summary>
    public class RecurringOrderPauseManager
    {
        private readonly RecurringOrdersRepository _recurringOrdersRepository = new RecurringOrdersRepository();

        public enum PauseInputMode
        {
            Weeks,
            ResumeDate
        }

        /// <summary>
        /// Resolves the date deliveries should resume (first day after the pause window).
        /// </summary>
        public bool TryResolveResumeDate(PauseInputMode mode, int? weeks, DateTime? resumeDate, out DateTime resolved, out string errorMessage)
        {
            resolved = default;
            errorMessage = null;
            DateTime today = TimeZoneUtils.Now().Date;

            if (mode == PauseInputMode.Weeks)
            {
                if (!weeks.HasValue
                    || weeks.Value < SystemConstants.RecurringOrderConstants.PauseMinWeeks
                    || weeks.Value > SystemConstants.RecurringOrderConstants.PauseMaxWeeks)
                {
                    errorMessage = MessageProvider.Format(
                        MessageKeys.RecurringOrder.PauseInvalidWeeks,
                        SystemConstants.RecurringOrderConstants.PauseMinWeeks,
                        SystemConstants.RecurringOrderConstants.PauseMaxWeeks);
                    return false;
                }

                resolved = today.AddDays(weeks.Value * 7);
                return true;
            }

            if (!resumeDate.HasValue)
            {
                errorMessage = MessageProvider.Get(MessageKeys.RecurringOrder.PauseNoInput);
                return false;
            }

            resolved = resumeDate.Value.Date;
            if (resolved <= today)
            {
                errorMessage = MessageProvider.Format(
                    MessageKeys.RecurringOrder.PauseInvalidResumeDate,
                    today.ToString(SystemConstants.FormatConstants.DateFormat));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loads the order, applies the pause, saves, and returns lines adjusted.
        /// </summary>
        public int ApplyPause(int recurringOrderId, DateTime resumeDate, out RecurringOrder savedOrder)
        {
            savedOrder = null;
            if (recurringOrderId <= 0)
                return 0;

            var recurringOrder = _recurringOrdersRepository.GetById(recurringOrderId);
            if (recurringOrder == null)
                return 0;

            int adjusted = _recurringOrdersRepository.ApplyPauseUntilDate(recurringOrder, resumeDate);
            if (adjusted <= 0)
                return 0;

            _recurringOrdersRepository.Update(recurringOrder);
            savedOrder = recurringOrder;
            return adjusted;
        }

        /// <summary>
        /// Contacts Away through the day before deliveries resume (inclusive end date).
        /// </summary>
        public int CreateContactAwayForPause(int contactId, DateTime resumeDate)
        {
            if (contactId <= 0)
                return 0;

            DateTime today = TimeZoneUtils.Now().Date;
            resumeDate = resumeDate.Date;
            if (resumeDate <= today)
                return 0;

            DateTime awayEnd = resumeDate.AddDays(-1);
            if (awayEnd < today)
                awayEnd = today;

            int reasonId = new AwayReasonRepository()
                .ResolveOrCreateReasonId(SystemConstants.AwayReasonConstants.RecurringPause);
            if (reasonId <= 0)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                    "Recurring pause: could not resolve away reason — contact away period not created.");
                return 0;
            }

            var away = new ContactsAwayPeriod
            {
                ContactID = contactId,
                AwayStartDate = today,
                AwayEndDate = awayEnd,
                ReasonID = reasonId
            };

            int awayPeriodId = new ContactsAwayPeriodRepository().Insert(away);
            if (awayPeriodId <= 0)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                    "Recurring pause: contact away insert failed for ContactID=" + contactId + ".");
                return 0;
            }

            AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                "Recurring pause: ContactID=" + contactId + " away period " + awayPeriodId
                + " (" + today.ToString("yyyy-MM-dd") + " to " + awayEnd.ToString("yyyy-MM-dd") + ").");
            return awayPeriodId;
        }
    }
}