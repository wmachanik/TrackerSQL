using System;
using System.Collections.Generic;
using TrackerSQL.Controls;
using TrackerSQL.Classes;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Manages all customer-related operations
    /// </summary>
    public class CustomerManager
    {
        /// <summary>
        /// Disables a customer and sends confirmation email
        /// </summary>
        public bool DisableCustomer(long customerId)
        {
            try
            {
                var customersTbl = new CustomersTbl();
                var customer = customersTbl.GetCustomerByCustomerID(customerId);

                if (customer == null)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                        MessageProvider.Format(MessageKeys.Customer.NotFound, customerId));
                    return false;
                }

                string notes = $"Disabled via customer request on {TimeZoneUtils.Now():d}. ";
                customersTbl.DisableCustomer(customerId, notes);

                // Send confirmation email using DisableClientManager
                SendDisableConfirmationEmail(customer);

                AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                    MessageProvider.Format(MessageKeys.Customer.Disabled,
                    customer.CompanyName, customerId));

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

        /// <summary>
        /// Disables a customer via self-service link
        /// </summary>
        public bool DisableCustomerViaSelfService(long customerId)
        {
            try
            {
                var customersTbl = new CustomersTbl();
                var customer = customersTbl.GetCustomerByCustomerID(customerId);

                if (customer == null)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                        MessageProvider.Format(MessageKeys.Customer.NotFound, customerId));
                    return false;
                }

                string notes = $"Disabled via self-service on {TimeZoneUtils.Now():d}. ";
                customersTbl.DisableCustomer(customerId, notes);

                SendDisableConfirmationEmail(customer);

                AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                    MessageProvider.Format(MessageKeys.Customer.DisabledViaSelfService,
                    customer.CompanyName, customerId));

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

        private void SendDisableConfirmationEmail(CustomersTbl customer)
        {
            var email = new EmailMailKitCls(new EmailSettings());

            email.SetEmailSubject(MessageProvider.Format(MessageKeys.DisableClient.GoodbyeSubject,
                customer.CompanyName));

            email.AddToBody(MessageProvider.Format(MessageKeys.DisableClient.Greeting,
                DetermineContactName(customer)));
            email.AddToBody(MessageProvider.Format(MessageKeys.DisableClient.DisabledMessage,
                customer.CompanyName));
            email.AddToBody(MessageProvider.Get(MessageProvider.GetEmailSignature()));

            if (!email.SendEmail())
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, MessageProvider.Format(MessageKeys.Email.SendError,
                    customer.CompanyName, email.LastErrorSummary));
            }
        }

        private string DetermineContactName(CustomersTbl customer)
        {
            if (!string.IsNullOrEmpty(customer.ContactFirstName))
            {
                if (!string.IsNullOrEmpty(customer.ContactAltFirstName))
                    return $"{customer.ContactFirstName} & {customer.ContactAltFirstName}";
                return customer.ContactFirstName;
            }

            if (!string.IsNullOrEmpty(customer.ContactAltFirstName))
                return customer.ContactAltFirstName;

            return "Coffee lover";
        }

        /// <summary>
        /// Gets customer by ID
        /// </summary>
        public CustomersTbl GetCustomer(long customerId)
        {
            var customer = new CustomersTbl().GetCustomerByCustomerID(customerId);
            if (customer == null)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                    MessageProvider.Format(MessageKeys.Customer.NotFound, customerId));
            }
            return customer;
        }

        /// <summary>
        /// Searches for customers by name
        /// </summary>
        public List<CustomersTbl> SearchByName(string namePattern)
        {
            if (string.IsNullOrWhiteSpace(namePattern))
                return new List<CustomersTbl>();

            return new CustomersTbl().GetAllCustomerWithNameLIKE(namePattern);
        }

        /// <summary>
        /// Searches for customers by email
        /// </summary>
        public List<CustomersTbl> SearchByEmail(string emailPattern)
        {
            if (string.IsNullOrWhiteSpace(emailPattern))
                return new List<CustomersTbl>();

            return new CustomersTbl().GetAllCustomerWithEmailLIKE(emailPattern);
        }

        /// <summary>
        /// Updates customer equipment details
        /// </summary>
        public bool UpdateEquipment(long customerId, int equipType, string machineSN)
        {
            var customersTbl = new CustomersTbl();
            string result = customersTbl.SetEquipDetailsIfEmpty(equipType, machineSN, customerId);

            if (string.IsNullOrEmpty(result))
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                    MessageProvider.Format(MessageKeys.Customer.EquipmentUpdated,
                    customerId, equipType, machineSN));
                return true;
            }

            AppLogger.WriteLog("error",
                MessageProvider.Format(MessageKeys.Customer.EquipmentUpdateError,
                customerId, result));
            return false;
        }

        /// <summary>
        /// Resets reminder count for a customer
        /// </summary>
        public void ResetReminderCount(long customerId, bool forceEnable = false)
        {
            var customersTbl = new CustomersTbl();
            // Note: You'll need to add this method to CustomersTbl
            customersTbl.ResetReminderCount(customerId, forceEnable);

            AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                MessageProvider.Format(MessageKeys.Customer.ReminderCountReset,
                customerId, forceEnable));
        }
        /// <summary>
        /// Send an email formation when we add or update an away period for a customer
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public void SendAwayPeriodConfirmationEmail(int customerId, DateTime startDate, DateTime endDate)
        {
            var customer = new CustomersTbl().GetCustomerByCustomerID(customerId);
            if (customer == null)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"Could not send away period confirmation: customer {customerId} not found.");
                return;
            }

            var email = new EmailMailKitCls(new EmailSettings());

            string subject = MessageProvider.Format(MessageKeys.AwayPeriod.ConfirmationSubject, customer.CompanyName);
            string greeting = MessageProvider.Format(MessageKeys.AwayPeriod.Greeting, DetermineContactName(customer));
            string body = MessageProvider.Format(
                MessageKeys.AwayPeriod.ConfirmationBody,
                startDate.ToString("dddd, d MMMM yyyy"),
                endDate.ToString("dddd, d MMMM yyyy")
            );
            string info = MessageProvider.Get(MessageKeys.AwayPeriod.ConfirmationInfo);

            email.SetEmailSubject(subject);
            email.AddToBody(greeting);
            email.AddToBody(body);
            email.AddToBody(info);
            email.AddToBody(MessageProvider.Get(MessageProvider.GetEmailSignature()));
            email.SetEmailFromTo(null, customer.EmailAddress);

            if (!email.SendEmail())
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, MessageProvider.Format(MessageKeys.Email.SendError, customer.CompanyName, email.LastErrorSummary));
            }
        }
        /// <summary>
        /// Returns all customer IDs that are away overlapping the given window.
        /// </summary>
        public HashSet<long> GetAwayCustomerIds(DateTime windowStart, DateTime windowEnd)
        {
            return new CustomersAwayTbl().GetAwayCustomerIds(windowStart, windowEnd);
        }
        /// <summary>
        /// True if the customer is away on the specified date.
        /// </summary>
        public bool IsCustomerAwayOnDate(long customerId, DateTime date)
        {
            return new CustomersAwayTbl().IsCustomerAwayOnDate(customerId, date);
        }
    
        /// <summary>
        /// True if the customer has any away period overlapping the given window.
        /// Overlap: AwayStart <= windowEnd AND AwayEnd >= windowStart
        /// </summary>
        public bool IsCustomerAwayDuringWindow(long customerId, DateTime windowStart, DateTime windowEnd)
        {
            var awayIds = new CustomersAwayTbl().GetAwayCustomerIds(windowStart, windowEnd);
            return awayIds.Contains(customerId);
        }
    }
}