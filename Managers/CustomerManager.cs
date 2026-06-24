using System;
using System.Collections.Generic;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Manages all contact/customer-related operations.
    /// </summary>
    public class CustomerManager
    {
        private readonly ContactsRepository _contactsRepository;
        private readonly ContactsAwayPeriodRepository _awayPeriodRepository;

        public CustomerManager()
        {
            _contactsRepository = new ContactsRepository();
            _awayPeriodRepository = new ContactsAwayPeriodRepository();
        }

        public bool DisableCustomer(long customerId)
        {
            try
            {
                var contact = _contactsRepository.GetById((int)customerId);
                if (contact == null)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                        MessageProvider.Format(MessageKeys.Customer.NotFound, customerId));
                    return false;
                }

                string notes = $"Disabled via customer request on {TimeZoneUtils.Now():d}. ";
                _contactsRepository.DisableContact((int)customerId, notes);
                SendDisableConfirmationEmail(contact);

                AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                    MessageProvider.Format(MessageKeys.Customer.Disabled,
                    contact.CompanyName, customerId));

                return true;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog("error",
                    MessageProvider.Format(MessageKeys.Customer.DisableError,
                    customerId, ex.Message));
                throw;
            }
        }

        public bool DisableCustomerViaSelfService(long customerId)
        {
            try
            {
                var contact = _contactsRepository.GetById((int)customerId);
                if (contact == null)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                        MessageProvider.Format(MessageKeys.Customer.NotFound, customerId));
                    return false;
                }

                string notes = $"Disabled via self-service on {TimeZoneUtils.Now():d}. ";
                _contactsRepository.DisableContact((int)customerId, notes);
                SendDisableConfirmationEmail(contact);

                AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                    MessageProvider.Format(MessageKeys.Customer.DisabledViaSelfService,
                    contact.CompanyName, customerId));

                return true;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog("error",
                    MessageProvider.Format(MessageKeys.Customer.DisableError,
                    customerId, ex.Message));
                throw;
            }
        }

        private void SendDisableConfirmationEmail(Contact contact)
        {
            var email = new EmailMailKitCls(new EmailSettings());

            email.SetEmailSubject(MessageProvider.Format(MessageKeys.DisableClient.GoodbyeSubject,
                contact.CompanyName));

            email.AddToBody(MessageProvider.Format(MessageKeys.DisableClient.Greeting,
                DetermineContactName(contact)));
            email.AddToBody(MessageProvider.Format(MessageKeys.DisableClient.DisabledMessage,
                contact.CompanyName));
            email.AddToBody(MessageProvider.Get(MessageProvider.GetEmailSignature()));

            if (!email.SendEmail())
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, MessageProvider.Format(MessageKeys.Email.SendError,
                    contact.CompanyName, email.LastErrorSummary));
            }
        }

        private static string DetermineContactName(Contact contact)
        {
            if (!string.IsNullOrEmpty(contact.ContactFirstName))
            {
                if (!string.IsNullOrEmpty(contact.ContactAltFirstName))
                {
                    return $"{contact.ContactFirstName} & {contact.ContactAltFirstName}";
                }

                return contact.ContactFirstName;
            }

            if (!string.IsNullOrEmpty(contact.ContactAltFirstName))
            {
                return contact.ContactAltFirstName;
            }

            return "Coffee lover";
        }

        public Contact GetCustomer(long customerId)
        {
            var contact = _contactsRepository.GetById((int)customerId);
            if (contact == null)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                    MessageProvider.Format(MessageKeys.Customer.NotFound, customerId));
            }

            return contact;
        }

        public List<Contact> SearchByName(string namePattern)
        {
            if (string.IsNullOrWhiteSpace(namePattern))
            {
                return new List<Contact>();
            }

            return _contactsRepository.SearchByContactNameLike(namePattern);
        }

        public List<Contact> SearchByEmail(string emailPattern)
        {
            if (string.IsNullOrWhiteSpace(emailPattern))
            {
                return new List<Contact>();
            }

            return _contactsRepository.SearchByEmailLike(emailPattern);
        }

        public bool UpdateEquipment(long customerId, int equipType, string machineSN)
        {
            bool success = _contactsRepository.SetEquipmentIfEmpty(equipType, machineSN, (int)customerId);

            if (success)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                    MessageProvider.Format(MessageKeys.Customer.EquipmentUpdated,
                    customerId, equipType, machineSN));
                return true;
            }

            AppLogger.WriteLog("error",
                MessageProvider.Format(MessageKeys.Customer.EquipmentUpdateError,
                customerId, "SetEquipmentIfEmpty returned false"));
            return false;
        }

        public void ResetReminderCount(long customerId, bool forceEnable = false)
        {
            _contactsRepository.ResetReminderCount((int)customerId, forceEnable);

            AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                MessageProvider.Format(MessageKeys.Customer.ReminderCountReset,
                customerId, forceEnable));
        }

        public void SendAwayPeriodConfirmationEmail(int customerId, DateTime startDate, DateTime endDate)
        {
            var contact = _contactsRepository.GetById(customerId);
            if (contact == null)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                    $"Could not send away period confirmation: customer {customerId} not found.");
                return;
            }

            var email = new EmailMailKitCls(new EmailSettings());

            string subject = MessageProvider.Format(MessageKeys.AwayPeriod.ConfirmationSubject, contact.CompanyName);
            string greeting = MessageProvider.Format(MessageKeys.AwayPeriod.Greeting, DetermineContactName(contact));
            string body = MessageProvider.Format(
                MessageKeys.AwayPeriod.ConfirmationBody,
                startDate.ToString("dddd, d MMMM yyyy"),
                endDate.ToString("dddd, d MMMM yyyy"));
            string info = MessageProvider.Get(MessageKeys.AwayPeriod.ConfirmationInfo);

            email.SetEmailSubject(subject);
            email.AddToBody(greeting);
            email.AddToBody(body);
            email.AddToBody(info);
            email.AddToBody(MessageProvider.Get(MessageProvider.GetEmailSignature()));
            email.SetEmailFromTo(null, contact.EmailAddress);

            if (!email.SendEmail())
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                    MessageProvider.Format(MessageKeys.Email.SendError, contact.CompanyName, email.LastErrorSummary));
            }
        }

        public HashSet<long> GetAwayCustomerIds(DateTime windowStart, DateTime windowEnd)
        {
            return _awayPeriodRepository.GetAwayContactIds(windowStart, windowEnd);
        }

        public bool IsCustomerAwayOnDate(long customerId, DateTime date)
        {
            return _awayPeriodRepository.IsContactAwayOnDate(customerId, date);
        }

        public bool IsCustomerAwayDuringWindow(long customerId, DateTime windowStart, DateTime windowEnd)
        {
            var awayIds = _awayPeriodRepository.GetAwayContactIds(windowStart, windowEnd);
            return awayIds.Contains(customerId);
        }
    }
}
