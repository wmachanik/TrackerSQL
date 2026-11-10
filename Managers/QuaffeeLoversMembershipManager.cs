using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Quaffee Lovers: after N Woo-imported orders, grant the shop member role and send a welcome email.
    /// Call from Woo import, Contact Details, and the invite system tool.
    /// </summary>
    public class QuaffeeLoversMembershipManager
    {
        private readonly WooCommerceSettingsManager _settings = new WooCommerceSettingsManager();
        private readonly WooOrderInfoRepository _wooOrders = new WooOrderInfoRepository();
        private readonly WooContactInfoRepository _wooContacts = new WooContactInfoRepository();
        private readonly QuaffeeLoversGrantRepository _grants = new QuaffeeLoversGrantRepository();
        private readonly ContactsRepository _contacts = new ContactsRepository();
        private readonly QuaffeeLoversEmailManager _email = new QuaffeeLoversEmailManager();
        private readonly WooCommerceApiClient _api = new WooCommerceApiClient();

        public void EnsureReady()
        {
            new WooCommerceSchemaInstaller().EnsureSchema();
        }

        public bool IsFeatureEnabled()
        {
            EnsureReady();
            var s = _settings.GetSettings();
            return s != null && s.QuaffeeLoversEnabled && _settings.IsIntegrationEnabled();
        }

        public int GetOrderThreshold()
        {
            var s = _settings.GetSettings() ?? new WooCommerceSettings();
            int t = s.QuaffeeLoversOrderThreshold;
            return t > 0 ? t : 3;
        }

        public string GetTargetRole()
        {
            var s = _settings.GetSettings() ?? new WooCommerceSettings();
            string role = (s.QuaffeeLoversWooRole ?? "member").Trim();
            return string.IsNullOrEmpty(role) ? "member" : role;
        }

        public int CountWooOrders(int contactId) => _wooOrders.CountImportedOrdersForContact(contactId);

        public QuaffeeLoversContactStatus GetContactStatus(int contactId)
        {
            var status = new QuaffeeLoversContactStatus
            {
                ContactID = contactId,
                FeatureEnabled = IsFeatureEnabled(),
                OrderThreshold = GetOrderThreshold(),
                TargetRole = GetTargetRole()
            };
            if (contactId <= 0)
                return status;

            Contact c = _contacts.GetById(contactId);
            if (c != null)
            {
                status.CompanyName = c.CompanyName;
                status.Email = PickEmail(c);
            }

            status.WooOrderCount = CountWooOrders(contactId);
            status.ThresholdMet = status.WooOrderCount >= status.OrderThreshold;

            var grant = _grants.GetByContactId(contactId);
            var info = _wooContacts.GetByContactId(contactId);
            if (grant != null)
            {
                status.WelcomeEmailSentUtc = grant.WelcomeEmailSentUtc;
                status.RoleGrantedUtc = grant.RoleGrantedUtc;
                if (!string.IsNullOrWhiteSpace(grant.WooRoleAtGrant))
                    status.ShopRole = grant.WooRoleAtGrant.Trim();
                if (grant.WooCustomerId.HasValue && grant.WooCustomerId.Value > 0)
                {
                    status.WooCustomerId = grant.WooCustomerId.Value;
                    status.HasWooCustomer = true;
                }
                if (!string.IsNullOrWhiteSpace(grant.Notes) && grant.Notes.IndexOf("No Woo customer", StringComparison.OrdinalIgnoreCase) >= 0)
                    status.HasWooCustomer = false;
                status.QuaffeeLoversMember = IsMemberOnShop(status.ShopRole, status.TargetRole);
                status.AlreadyMemberOnWoo = status.QuaffeeLoversMember;
            }
            else if (info != null)
            {
                if (info.WooCustomerId.HasValue && info.WooCustomerId.Value > 0)
                {
                    status.WooCustomerId = info.WooCustomerId.Value;
                    status.HasWooCustomer = true;
                }
                status.QuaffeeLoversMember = info.QuaffeeLoversMember;
                status.AlreadyMemberOnWoo = info.QuaffeeLoversMember;
            }

            status.StatusLabel = BuildStatusLabel(status);
            status.StatusTooltip = BuildStatusTooltip(status);
            return status;
        }

        /// <summary>After a Woo order is imported — update counts only; welcome email and role are manual.</summary>
        public QuaffeeLoversProcessResult TryAfterOrderImported(int contactId, string matchEmail, long wooCustomerId,
            string updatedBy)
        {
            var result = new QuaffeeLoversProcessResult();
            if (contactId <= 0 || contactId == (int)SystemConstants.CustomerConstants.SundryCustomerID)
            {
                result.Message = "Not a regular contact.";
                return result;
            }

            EnsureReady();
            int count = CountWooOrders(contactId);
            result.WooOrderCount = count;
            int threshold = GetOrderThreshold();
            result.ThresholdMet = count >= threshold;

            var existing = _grants.GetByContactId(contactId);
            if (existing != null && existing.DismissedUtc.HasValue)
            {
                existing.WooOrderCount = count;
                if (!string.IsNullOrWhiteSpace(matchEmail))
                    existing.MatchEmail = matchEmail.Trim();
                _grants.Save(existing);
            }
            else if (result.ThresholdMet && IsFeatureEnabled())
            {
                string email = !string.IsNullOrWhiteSpace(matchEmail) ? matchEmail.Trim() : PickEmail(_contacts.GetById(contactId));
                RecordGrant(contactId,
                    wooCustomerId > 0 ? wooCustomerId : (long?)null,
                    email, count, GetTargetRole(), false, null, null, updatedBy,
                    notes: "Eligible after Woo import — invite from Quaffee Lovers or Contact Details.",
                    existing);
            }

            RefreshWooContactInfo(contactId, wooCustomerId, matchEmail, count, existing);
            result.Succeeded = true;
            result.Message = result.ThresholdMet
                ? count + " Woo orders (at or above threshold " + threshold + ") — invite when ready."
                : count + " Woo orders (threshold " + threshold + ").";
            return result;
        }

        /// <summary>Contact Details — look up customer on the shop and refresh status.</summary>
        public QuaffeeLoversProcessResult TrySyncFromWoo(int contactId, string userName)
        {
            var result = new QuaffeeLoversProcessResult();
            if (contactId <= 0 || contactId == (int)SystemConstants.CustomerConstants.SundryCustomerID)
            {
                result.Message = "Not a regular contact.";
                return result;
            }

            EnsureReady();
            if (!_settings.IsIntegrationEnabled())
            {
                result.Message = "WooCommerce integration is not enabled.";
                return result;
            }

            if (!_settings.TryGetApiCredentials(out WooCommerceApiClient.ApiCredentials creds, out string credErr))
            {
                result.Message = credErr ?? "Woo API credentials are not available.";
                return result;
            }

            Contact contact = _contacts.GetById(contactId);
            string email = PickEmail(contact);
            if (string.IsNullOrWhiteSpace(email))
            {
                result.Message = "No email address to match a Woo customer.";
                return result;
            }

            int count = CountWooOrders(contactId);
            string targetRole = GetTargetRole();
            var existing = _grants.GetByContactId(contactId);

            var customer = _api.FindCustomerByEmail(creds, email, out string lookupDetail, new[] { targetRole });
            if (customer == null)
            {
                result.Message = lookupDetail ?? "No Woo customer with that email.";
                RecordGrant(contactId, null, email, count, targetRole, false, null, null, userName,
                    notes: result.Message, existing);
                RefreshWooContactInfo(contactId, 0, email, count, existing);
                return result;
            }

            result.FoundOnWoo = true;
            result.ShopRole = customer.RolesDisplay;
            bool alreadyMember = WooCommerceApiClient.CustomerHasRole(customer, targetRole);
            result.AlreadyMemberOnWoo = alreadyMember;
            result.WooOrderCount = count;
            result.ThresholdMet = count >= GetOrderThreshold();
            result.Succeeded = true;

            RecordGrant(contactId, customer.Id, email, count, customer.RolesDisplay, alreadyMember, existing?.RoleGrantedUtc,
                existing?.WelcomeEmailSentUtc, userName,
                notes: "Shop check: role " + (customer.RolesDisplay ?? "?") + ", target " + targetRole + ".", existing);

            RefreshWooContactInfo(contactId, customer.Id, email, count,
                new QuaffeeLoversGrant
                {
                    WooRoleAtGrant = customer.RolesDisplay,
                    AlreadyMemberOnWoo = alreadyMember,
                    WelcomeEmailSentUtc = existing?.WelcomeEmailSentUtc,
                    RoleGrantedUtc = existing?.RoleGrantedUtc
                });

            if (alreadyMember)
                result.Message = "On shop as \"" + result.ShopRole + "\" (has " + targetRole + " role).";
            else
                result.Message = "On shop as \"" + customer.RolesDisplay + "\" — not \"" + targetRole + "\" yet. "
                    + "You can still use Invite selected to grant the role.";
            return result;
        }

        /// <summary>Contact Details — invite when at threshold; always asks via UI confirm before this runs.</summary>
        public QuaffeeLoversProcessResult TryInviteIfEligible(int contactId, string userName)
        {
            Contact c = _contacts.GetById(contactId);
            long wooCustomerId = _wooContacts.GetByContactId(contactId)?.WooCustomerId ?? 0;
            return ProcessContact(contactId, c != null ? PickEmail(c) : null, wooCustomerId, userName,
                sendWelcomeIfNew: true, manualInvite: false);
        }

        /// <summary>Removes shop member role and clears Tracker membership flags.</summary>
        public QuaffeeLoversProcessResult TryDisableMember(int contactId, string userName)
        {
            var result = new QuaffeeLoversProcessResult();
            if (contactId <= 0 || contactId == (int)SystemConstants.CustomerConstants.SundryCustomerID)
            {
                result.Message = "Not a regular contact.";
                return result;
            }

            EnsureReady();
            if (!_settings.IsIntegrationEnabled())
            {
                result.Message = "WooCommerce integration is not enabled.";
                return result;
            }

            if (!_settings.TryGetApiCredentials(out WooCommerceApiClient.ApiCredentials creds, out string credErr))
            {
                result.Message = credErr ?? "Woo API credentials are not available.";
                return result;
            }

            Contact contact = _contacts.GetById(contactId);
            var existing = _grants.GetByContactId(contactId);
            var info = _wooContacts.GetByContactId(contactId);
            string email = existing?.MatchEmail ?? PickEmail(contact);
            long wooCustomerId = info?.WooCustomerId ?? existing?.WooCustomerId ?? 0;

            WooCommerceApiClient.WooCustomerDto customer = null;
            if (wooCustomerId > 0)
            {
                customer = _api.FindCustomerByEmail(creds, email, out string _, new[] { GetTargetRole() });
            }
            else if (!string.IsNullOrWhiteSpace(email))
                customer = _api.FindCustomerByEmail(creds, email, out string _, new[] { GetTargetRole() });

            if (customer == null)
            {
                result.Message = "No Woo customer found for this contact.";
                return result;
            }

            string targetRole = GetTargetRole();
            bool wasMember = WooCommerceApiClient.CustomerHasRole(customer, targetRole);
            _settings.TryGetWordPressRoleCredentials(out WooCommerceApiClient.WordPressRoleCredentials wpRoleCreds, out _);
            if (wasMember && !_api.RemoveCustomerRole(creds, customer.Id, targetRole, email, wpRoleCreds, out string roleErr))
            {
                result.Message = roleErr ?? "Could not remove the shop member role.";
                return result;
            }

            int count = CountWooOrders(contactId);
            var grant = existing ?? new QuaffeeLoversGrant { ContactID = contactId, RecordedBy = userName };
            grant.WooCustomerId = customer.Id;
            grant.MatchEmail = email;
            grant.WooOrderCount = count;
            grant.WooRoleAtGrant = "customer";
            grant.AlreadyMemberOnWoo = false;
            grant.RoleGrantedUtc = null;
            grant.Notes = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
                + " Member disabled by " + userName + (wasMember ? " (shop role set to customer)." : " (was not member on shop).");
            _grants.Save(grant);

            RefreshWooContactInfo(contactId, customer.Id, email, count,
                new QuaffeeLoversGrant
                {
                    WooRoleAtGrant = "customer",
                    AlreadyMemberOnWoo = false,
                    WelcomeEmailSentUtc = grant.WelcomeEmailSentUtc
                });

            result.Succeeded = true;
            result.Message = wasMember
                ? "Shop role set to customer; membership cleared in Tracker."
                : "Not a member on the shop; Tracker membership flags cleared.";
            AppLogger.WriteLog("woo", "Quaffee Lovers disabled for contact " + contactId, userName);
            return result;
        }

        /// <summary>Contact Details — grant member + welcome (ignores order threshold).</summary>
        public QuaffeeLoversProcessResult TryForceGrantMember(int contactId, string userName) =>
            TryWelcomeContact(contactId, userName);

        /// <summary>Contact Details button — force welcome / grant.</summary>
        public QuaffeeLoversProcessResult TryWelcomeContact(int contactId, string userName)
        {
            Contact c = _contacts.GetById(contactId);
            long wooCustomerId = _wooContacts.GetByContactId(contactId)?.WooCustomerId ?? 0;
            return ProcessContact(contactId, c != null ? PickEmail(c) : null, wooCustomerId, userName,
                sendWelcomeIfNew: true, manualInvite: true);
        }

        public List<QuaffeeLoversInviteRow> ScanInviteCandidates() =>
            _grants.ListPendingInvites(GetOrderThreshold());

        /// <summary>Invite tool — look up each contact on Woo and refresh stored shop role / member flag.</summary>
        public QuaffeeLoversShopCheckResult RefreshShopRoles(IEnumerable<int> contactIds, string userName)
        {
            var result = new QuaffeeLoversShopCheckResult();
            if (contactIds == null)
                return result;

            if (!_settings.IsIntegrationEnabled())
            {
                result.Errors.Add("WooCommerce integration is not enabled.");
                return result;
            }

            if (!_settings.TryGetApiCredentials(out WooCommerceApiClient.ApiCredentials _, out string credErr))
            {
                result.Errors.Add(credErr ?? "Woo API credentials are not available.");
                return result;
            }

            string targetRole = GetTargetRole();
            foreach (int contactId in contactIds.Distinct())
            {
                if (contactId <= 0 || contactId == (int)SystemConstants.CustomerConstants.SundryCustomerID)
                    continue;

                result.Checked++;
                QuaffeeLoversProcessResult sync = TrySyncFromWoo(contactId, userName);
                if (!string.IsNullOrWhiteSpace(sync.Message) && !sync.FoundOnWoo && !sync.Succeeded)
                    result.Errors.Add(contactId + ": " + sync.Message);
                else if (!sync.FoundOnWoo)
                    result.NotOnShop++;
                else if (sync.AlreadyMemberOnWoo || WooCommerceApiClient.RoleIsMember(sync.ShopRole, targetRole))
                    result.AlreadyMember++;
                else
                    result.OnShopNotMember++;
            }

            return result;
        }

        public QuaffeeLoversProcessResult InviteContact(int contactId, string userName) =>
            TryWelcomeContact(contactId, userName);

        public void DismissContact(int contactId, string userName, string notes = null)
        {
            if (!QuaffeeLoversGrantRepository.TableSeen() || contactId <= 0)
                return;
            var grant = _grants.GetByContactId(contactId) ?? new QuaffeeLoversGrant { ContactID = contactId };
            grant.DismissedUtc = DateTime.UtcNow;
            grant.DismissedBy = userName;
            if (!string.IsNullOrWhiteSpace(notes))
                grant.Notes = notes.Trim();
            grant.WooOrderCount = CountWooOrders(contactId);
            _grants.Save(grant);
            AppLogger.WriteLog("woo", "Quaffee Lovers dismissed for contact " + contactId, userName);
        }

        private QuaffeeLoversProcessResult ProcessContact(int contactId, string matchEmail, long wooCustomerId,
            string userName, bool sendWelcomeIfNew, bool manualInvite)
        {
            var result = new QuaffeeLoversProcessResult();
            if (contactId <= 0 || contactId == (int)SystemConstants.CustomerConstants.SundryCustomerID)
            {
                result.Message = "Not a regular contact.";
                return result;
            }

            if (!IsFeatureEnabled())
            {
                result.Message = "Quaffee Lovers is turned off in WooCommerce setup.";
                return result;
            }

            EnsureReady();
            int count = CountWooOrders(contactId);
            result.WooOrderCount = count;
            int threshold = GetOrderThreshold();
            result.ThresholdMet = count >= threshold;

            string role = GetTargetRole();
            var existing = _grants.GetByContactId(contactId);
            if (existing != null && existing.DismissedUtc.HasValue && !manualInvite)
            {
                result.AlreadyRecorded = true;
                result.Message = "This contact was dismissed from the invite list.";
                RefreshWooContactInfo(contactId, wooCustomerId, matchEmail, count, existing);
                return result;
            }

            if (existing != null && !manualInvite && IsMemberOnShop(existing.WooRoleAtGrant, role))
            {
                result.AlreadyRecorded = true;
                result.AlreadyMemberOnWoo = true;
                result.Succeeded = true;
                result.Message = existing.WelcomeEmailSentUtc.HasValue
                    ? "Already a Quaffee Lovers member on the shop."
                    : "Already a Quaffee Lovers member on the shop (welcome email not sent yet).";
                RefreshWooContactInfo(contactId, wooCustomerId, matchEmail, count, existing);
                return result;
            }

            if (!result.ThresholdMet && !manualInvite)
            {
                result.Message = string.Format(CultureInfo.InvariantCulture,
                    "Needs {0} Woo orders ({1} so far).", threshold, count);
                RefreshWooContactInfo(contactId, wooCustomerId, matchEmail, count, existing);
                return result;
            }

            if (!_settings.TryGetApiCredentials(out WooCommerceApiClient.ApiCredentials creds, out string credErr))
            {
                result.Message = credErr ?? "Woo API credentials are not available.";
                return result;
            }

            Contact contact = _contacts.GetById(contactId);
            string email = !string.IsNullOrWhiteSpace(matchEmail) ? matchEmail.Trim() : PickEmail(contact);
            if (string.IsNullOrWhiteSpace(email))
            {
                result.Message = "No email address to match a Woo customer.";
                return result;
            }

            var customer = _api.FindCustomerByEmail(creds, email, out string lookupDetail, new[] { role });
            if (customer == null)
            {
                result.Message = lookupDetail ?? "No Woo customer for that email.";
                RecordGrant(contactId, wooCustomerId > 0 ? wooCustomerId : (long?)null, email, count, role,
                    alreadyMember: false, roleGrantedUtc: null, welcomeSentUtc: null, userName,
                    notes: result.Message, existing);
                return result;
            }

            if (wooCustomerId <= 0)
                wooCustomerId = customer.Id;

            bool alreadyMember = WooCommerceApiClient.CustomerHasRole(customer, role);
            result.AlreadyMemberOnWoo = alreadyMember;

            DateTime? grantedAt = null;
            if (!alreadyMember)
            {
                _settings.TryGetWordPressRoleCredentials(out WooCommerceApiClient.WordPressRoleCredentials wpRoleCreds, out _);
                if (_api.SetCustomerRole(creds, customer.Id, role, email, wpRoleCreds, out string grantDetail))
                {
                    result.RoleGranted = true;
                    grantedAt = DateTime.UtcNow;
                }
                else
                {
                    result.Message = grantDetail ?? "Could not set the Woo member role.";
                    RecordGrant(contactId, customer.Id, email, count, role, false, null, null, userName,
                        notes: result.Message, existing);
                    return result;
                }
            }

            DateTime? welcomeUtc = existing?.WelcomeEmailSentUtc;
            if (sendWelcomeIfNew && !welcomeUtc.HasValue)
            {
                if (_email.SendWelcome(contact, count, out string mailErr))
                {
                    result.WelcomeEmailSent = true;
                    welcomeUtc = DateTime.UtcNow;
                }
                else if (!alreadyMember)
                    result.Message = (result.Message ?? string.Empty) + " Welcome email failed: " + mailErr;
            }

            string rolesOnShop = customer.RolesDisplay ?? role;
            if (result.RoleGranted)
            {
                var refreshed = _api.FindCustomerByEmail(creds, email, out string _, new[] { role });
                if (refreshed != null && !string.IsNullOrWhiteSpace(refreshed.RolesDisplay))
                    rolesOnShop = refreshed.RolesDisplay;
            }

            bool memberOnShop = alreadyMember || result.RoleGranted;
            RecordGrant(contactId, customer.Id, email, count, rolesOnShop, memberOnShop, grantedAt, welcomeUtc, userName,
                manualInvite ? "Manual invite from Contact Details or system tool." : "Auto after Woo import.",
                existing);

            result.Succeeded = true;
            result.AlreadyRecorded = memberOnShop && welcomeUtc.HasValue;
            if (string.IsNullOrEmpty(result.Message))
            {
                if (alreadyMember && !result.WelcomeEmailSent)
                    result.Message = "Already a member on the shop; welcome email was already sent.";
                else if (alreadyMember)
                    result.Message = "Already a member on the shop; sent thank-you email.";
                else
                    result.Message = "Granted Quaffee Lovers member role and sent welcome email.";
            }

            RefreshWooContactInfo(contactId, customer.Id, email, count,
                new QuaffeeLoversGrant
                {
                    WooRoleAtGrant = rolesOnShop,
                    AlreadyMemberOnWoo = memberOnShop,
                    WelcomeEmailSentUtc = welcomeUtc,
                    RoleGrantedUtc = grantedAt
                });

            AppLogger.WriteLog("woo",
                string.Format(CultureInfo.InvariantCulture,
                    "Quaffee Lovers contact {0}: orders={1}, granted={2}, alreadyMember={3}, welcome={4}",
                    contactId, count, result.RoleGranted, alreadyMember, result.WelcomeEmailSent),
                userName);
            WooCommerceUserLog.Write("Quaffee Lovers",
                "Contact " + contactId + ": " + result.Message, userName);

            return result;
        }

        private void RecordGrant(int contactId, long? wooCustomerId, string email, int count, string role,
            bool alreadyMember, DateTime? roleGrantedUtc, DateTime? welcomeSentUtc, string userName, string notes,
            QuaffeeLoversGrant existing)
        {
            var grant = existing ?? new QuaffeeLoversGrant { ContactID = contactId, RecordedBy = userName };
            grant.WooCustomerId = wooCustomerId;
            grant.MatchEmail = email;
            grant.WooOrderCount = count;
            grant.WooRoleAtGrant = role;
            grant.AlreadyMemberOnWoo = alreadyMember;
            if (roleGrantedUtc.HasValue)
                grant.RoleGrantedUtc = roleGrantedUtc;
            if (welcomeSentUtc.HasValue)
                grant.WelcomeEmailSentUtc = welcomeSentUtc;
            if (string.IsNullOrEmpty(grant.RecordedBy))
                grant.RecordedBy = userName;
            if (!string.IsNullOrWhiteSpace(notes))
                grant.Notes = notes.Trim();
            if (welcomeSentUtc.HasValue)
            {
                grant.InviteSentUtc = welcomeSentUtc;
                grant.InviteSentBy = userName;
            }
            _grants.Save(grant);
        }

        private void RefreshWooContactInfo(int contactId, long wooCustomerId, string email, int count,
            QuaffeeLoversGrant grantHint)
        {
            bool member = grantHint != null
                && IsMemberOnShop(grantHint.WooRoleAtGrant ?? string.Empty, GetTargetRole());
            _wooContacts.UpsertImportStats(contactId,
                wooCustomerId > 0 ? wooCustomerId : (long?)null,
                email, count, member, DateTime.UtcNow);
        }

        /// <summary>True only when the shop role string includes the configured member role.</summary>
        public static bool IsMemberOnShop(string shopRole, string targetRole) =>
            WooCommerceApiClient.RoleIsMember(shopRole, targetRole);

        private static string PickEmail(Contact contact)
        {
            if (contact == null)
                return null;
            if (!string.IsNullOrWhiteSpace(contact.EmailAddress))
                return contact.EmailAddress.Trim();
            if (!string.IsNullOrWhiteSpace(contact.AltEmailAddress))
                return contact.AltEmailAddress.Trim();
            return null;
        }

        private static string BuildStatusLabel(QuaffeeLoversContactStatus s)
        {
            if (!s.FeatureEnabled)
                return string.Empty;
            if (s.QuaffeeLoversMember)
                return "Member";
            if (s.WelcomeEmailSentUtc.HasValue && s.HasWooCustomer != false)
                return "Welcome sent";
            if (s.HasWooCustomer == false)
                return "Not on shop";
            if (!s.ThresholdMet)
                return s.WooOrderCount + "/" + s.OrderThreshold;
            return "Eligible";
        }

        private static string BuildStatusTooltip(QuaffeeLoversContactStatus s)
        {
            var lines = new List<string>();
            if (!s.FeatureEnabled)
                return string.Empty;
            lines.Add(s.WooOrderCount + " Woo-imported order(s); threshold " + s.OrderThreshold + ".");
            if (!string.IsNullOrWhiteSpace(s.Email))
                lines.Add("Match email: " + s.Email);
            if (s.HasWooCustomer == true)
            {
                lines.Add("Woo customer" + (s.WooCustomerId.HasValue ? " #" + s.WooCustomerId.Value : "") + ".");
                if (!string.IsNullOrWhiteSpace(s.ShopRole))
                    lines.Add("Shop role: " + s.ShopRole + " (target: " + s.TargetRole + ").");
            }
            else if (s.HasWooCustomer == false)
                lines.Add("No Woo customer for this email yet — use Check Woo.");
            if (s.QuaffeeLoversMember)
            {
                if (s.AlreadyMemberOnWoo && !s.RoleGrantedUtc.HasValue)
                    lines.Add("Was already a member on the shop.");
                if (s.RoleGrantedUtc.HasValue)
                    lines.Add("Member role granted " + s.RoleGrantedUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm") + ".");
                if (s.WelcomeEmailSentUtc.HasValue)
                    lines.Add("Welcome email sent " + s.WelcomeEmailSentUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm") + ".");
            }
            else
            {
                if (s.RoleGrantedUtc.HasValue)
                    lines.Add("Tracker recorded a grant attempt but the shop does not show the "
                        + s.TargetRole + " role — use Check Woo or Force member.");
                if (s.WelcomeEmailSentUtc.HasValue)
                    lines.Add("Welcome email was sent " + s.WelcomeEmailSentUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                        + " but they are not a member on the shop yet.");
                if (s.ThresholdMet && s.FeatureEnabled)
                    lines.Add("Eligible — use Check Woo, Invite member, or Force member.");
            }
            return string.Join(Environment.NewLine, lines);
        }
    }
}
