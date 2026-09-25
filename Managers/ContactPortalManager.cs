using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Contact portal: invite/login link, scoped data access, change requests, editable fields.
    /// </summary>
    public class ContactPortalManager
    {
        private readonly ContactUserLinkRepository _links = new ContactUserLinkRepository();
        private readonly ContactPortalChangeRequestRepository _requests = new ContactPortalChangeRequestRepository();
        private readonly ContactPortalSettingsRepository _settings = new ContactPortalSettingsRepository();
        private readonly ContactsRepository _contacts = new ContactsRepository();
        private readonly OrdersRepository _orders = new OrdersRepository();
        private readonly RepairsRepository _repairs = new RepairsRepository();
        private readonly RecurringOrdersRepository _recurring = new RecurringOrdersRepository();

        public void EnsureReady()
        {
            new ContactPortalSchemaInstaller().EnsureSchema();
        }

        /// <summary>
        /// ZZName (sundry walk-in) contacts are shared by many people and must never be invited
        /// to the portal or sent a portal link.
        /// </summary>
        public static bool IsPortalExcludedContact(int contactId, string companyName)
        {
            if (contactId == (int)SystemConstants.CustomerConstants.SundryCustomerID)
                return true;
            return !string.IsNullOrWhiteSpace(companyName)
                && companyName.TrimStart().StartsWith(
                    SystemConstants.CustomerConstants.SundryCustomerNamePrefix, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsContactRole(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return false;
            try
            {
                return Roles.IsUserInRole(userName, ContactPortalSchemaInstaller.ContactRoleName);
            }
            catch
            {
                return false;
            }
        }

        public ContactUserLink GetLinkForCurrentUser()
        {
            var user = Membership.GetUser();
            if (user?.ProviderUserKey == null)
                return null;
            return _links.GetByUserId((Guid)user.ProviderUserKey);
        }

        public int GetCurrentContactId()
        {
            int viewAs = GetViewAsContactId();
            if (viewAs > 0)
                return viewAs;
            var link = GetLinkForCurrentUser();
            return link?.ContactID ?? 0;
        }

        private const string ViewAsSessionKey = "ContactPortal.ViewAsContactId";
        private const string ViewAsRequestKey = "ContactPortal.ViewAsResolved";

        public const string ViewAsReadOnlyMessage =
            "Admin preview is read-only: nothing was saved and no email was sent.";

        /// <summary>
        /// Contact an administrator is previewing the portal as (0 when not previewing).
        /// Only honoured for administrators; resolved once per request.
        /// </summary>
        public static int GetViewAsContactId()
        {
            var ctx = HttpContext.Current;
            if (ctx == null)
                return 0;
            if (ctx.Items[ViewAsRequestKey] is int cached)
                return cached;

            int id = 0;
            if (ctx.Session?[ViewAsSessionKey] is int stored && stored > 0 && SecurityManager.IsAdmin())
                id = stored;
            ctx.Items[ViewAsRequestKey] = id;
            return id;
        }

        public static bool IsViewingAs()
        {
            return GetViewAsContactId() > 0;
        }

        /// <summary>Administrator starts a read-only preview of the portal as this contact.</summary>
        public bool StartViewAs(int contactId, out string message)
        {
            var ctx = HttpContext.Current;
            if (ctx?.Session == null || !SecurityManager.IsAdmin())
            {
                message = "Only administrators can preview the Contact Portal.";
                return false;
            }
            Contact contact = contactId > 0 ? _contacts.GetById(contactId) : null;
            if (contact == null)
            {
                message = "Contact not found.";
                return false;
            }

            ctx.Session[ViewAsSessionKey] = contactId;
            ctx.Items.Remove(ViewAsRequestKey);
            AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                "Admin " + ctx.User?.Identity?.Name + " started a Contact Portal preview as contact #" + contactId
                + " (" + (contact.CompanyName ?? string.Empty).Trim() + ")");
            message = null;
            return true;
        }

        /// <summary>Ends the admin preview; returns the contact that was being previewed (0 if none).</summary>
        public static int StopViewAs()
        {
            var ctx = HttpContext.Current;
            if (ctx?.Session == null)
                return 0;
            int id = ctx.Session[ViewAsSessionKey] is int stored ? stored : 0;
            ctx.Session.Remove(ViewAsSessionKey);
            ctx.Items.Remove(ViewAsRequestKey);
            return id;
        }

        public bool MustChangePassword()
        {
            var link = GetLinkForCurrentUser();
            return link != null && link.MustChangePassword;
        }

        public void ClearMustChangePassword()
        {
            var user = Membership.GetUser();
            if (user?.ProviderUserKey == null)
                return;
            _links.SetMustChangePassword((Guid)user.ProviderUserKey, false);
        }

        /// <summary>
        /// Contact changed their own portal password. Pass userName when the auth cookie was only
        /// just issued (first-time set-password link), since Membership.GetUser() cannot see it yet.
        /// </summary>
        public void RecordPasswordChanged(string userName = null)
        {
            MembershipUser user = string.IsNullOrEmpty(userName) ? Membership.GetUser() : Membership.GetUser(userName);
            if (user?.ProviderUserKey == null)
                return;
            Guid uid = (Guid)user.ProviderUserKey;
            var link = _links.GetByUserId(uid);
            if (link == null)
                return;

            bool wasTemporary = link.MustChangePassword;
            _links.SetMustChangePassword(uid, false);
            ContactChangeLogManager.LogSummary(
                link.ContactID,
                ContactChangeLogManager.SourcePortal,
                wasTemporary
                    ? "Contact set their Contact Portal password (replaced temporary password)"
                    : "Contact changed their Contact Portal password",
                user.UserName);

            if (wasTemporary)
                NotifyStaffOfPortalActivation(link.ContactID, user.UserName);
        }

        /// <summary>Tells orders@ that an invited customer has signed in and set their own password.</summary>
        private void NotifyStaffOfPortalActivation(int contactId, string userName)
        {
            if (contactId <= 0 || !ConfigHelper.GetBool("RequestChanges.NotifyEmail", true))
                return;

            var contact = _contacts.GetById(contactId);
            string company = contact?.CompanyName ?? ("Contact #" + contactId);
            NotifyStaff(
                "Contact Portal activated: " + company,
                new[]
                {
                    HttpUtility.HtmlEncode(company) + " (#" + contactId + ") has signed in to the Contact Portal and set their own password.",
                    "Login: " + HttpUtility.HtmlEncode(userName ?? string.Empty),
                    "When: " + TimeZoneUtils.Now().ToString("yyyy-MM-dd HH:mm"),
                    "",
                    StaffLink("Open contact", "~/Pages/ContactDetails.aspx?ID=" + contactId)
                },
                "Portal activation notify (contact " + contactId + ")");
        }

        /// <summary>Emails orders@ (SysCCEmailAddress) and always records the outcome in the system log.</summary>
        private static void NotifyStaff(string subject, IEnumerable<string> htmlLines, string context)
        {
            string to = ConfigHelper.GetString("SysCCEmailAddress", "orders@quaffee.co.za");
            try
            {
                var mail = new EmailMailKitCls { IncludeConfiguredCc = false };
                mail.SetEmailFromTo(sTo: to);
                mail.SetEmailSubject(subject);
                foreach (string line in htmlLines)
                    mail.AddStrAndNewLineToBody(line);

                string target = mail.IsTestMode ? mail.TestRecipientAddress + " (EmailTestMode; normally " + to + ")" : to;
                if (mail.SendEmail())
                    AppLogger.WriteLog(SystemConstants.LogTypes.System, context + ": emailed " + target);
                else
                    AppLogger.WriteLog(SystemConstants.LogTypes.System, context + ": email to " + target + " FAILED: "
                        + (string.IsNullOrWhiteSpace(mail.LastErrorSummary) ? "unknown error" : mail.LastErrorSummary));
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, context + ": email to " + to + " FAILED: " + ex.Message);
            }
        }

        private static string StaffLink(string text, string appRelativeUrl)
        {
            string url;
            try
            {
                url = DisableClientManager.GetApplicationUrl() + "/" + appRelativeUrl.TrimStart('~', '/');
            }
            catch
            {
                return HttpUtility.HtmlEncode(text);
            }
            return "<a href=\"" + HttpUtility.HtmlAttributeEncode(url) + "\">" + HttpUtility.HtmlEncode(text) + "</a>";
        }

        /// <summary>Admin user name for admin invites; "self-service" when a contact requested access themselves.</summary>
        private static string PortalChangedBy()
        {
            string name = ContactChangeLogManager.CurrentUserName();
            return string.Equals(name, "system", StringComparison.OrdinalIgnoreCase) ? "self-service" : name;
        }

        /// <summary>
        /// Records a Contact-role sign-in (auth cookie already set) and returns where to land:
        /// ChangePassword while a temp password is active, else a portal ReturnUrl or Home.
        /// </summary>
        public string CompleteContactSignIn(string userName, string returnUrl)
        {
            MembershipUser mu = Membership.GetUser(userName);
            if (mu?.ProviderUserKey != null)
            {
                Guid uid = (Guid)mu.ProviderUserKey;
                _links.TouchLastLogin(uid);
                AppLogger.WriteLog(SystemConstants.LogTypes.Login, "Portal sign-in: " + userName);

                var link = _links.GetByUserId(uid);
                if (link != null && link.MustChangePassword)
                    return "~/Portal/ChangePassword.aspx";
            }

            if (!string.IsNullOrEmpty(returnUrl)
                && returnUrl.StartsWith("/", StringComparison.Ordinal)
                && !returnUrl.StartsWith("//", StringComparison.Ordinal)
                && returnUrl.IndexOf("/Portal/", StringComparison.OrdinalIgnoreCase) >= 0
                && returnUrl.IndexOf("/Portal/Login.aspx", StringComparison.OrdinalIgnoreCase) < 0)
                return returnUrl;

            return "~/Portal/Home.aspx";
        }

        public void RecordLogin()
        {
            var user = Membership.GetUser();
            if (user?.ProviderUserKey == null)
                return;
            _links.TouchLastLogin((Guid)user.ProviderUserKey);
        }

        private const string ChooseTokenPurpose = "ContactPortal.ChooseContact";
        private static readonly TimeSpan ChooseTokenLifetime = TimeSpan.FromHours(24);

        /// <summary>
        /// Public self-serve invite. One matching enabled contact gets a temporary password;
        /// several get an email listing them, each with a signed link to choose one.
        /// Always returns a generic message (no enumeration of contacts on screen).
        /// </summary>
        public string RequestAccess(string email)
        {
            EnsureReady();
            const string generic = "If that email is on file, we have emailed you a temporary password. "
                + "If it is on more than one account, the email lists them so you can choose which one to use.";

            string address = (email ?? string.Empty).Trim();
            if (address.Length < 5 || address.IndexOf('@') < 1)
                return generic;

            List<Contact> matches;
            try
            {
                matches = _contacts.FindByEmailExact(address) ?? new List<Contact>();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Login, "Portal invite lookup failed: " + ex.Message);
                return generic;
            }

            matches = matches.Where(c => c != null && c.ContactID > 0
                && (c.Enabled == null || c.Enabled.Value)
                && !IsPortalExcludedContact(c.ContactID, c.CompanyName)).ToList();

            if (matches.Count == 0)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                    "Portal invite skipped — no enabled contact for email.");
                return generic;
            }

            if (matches.Count == 1)
                TryInviteContact(matches[0], address, out _);
            else
                SendChooseContactEmail(address, matches);
            return generic;
        }

        /// <summary>
        /// Handles a "choose this account" link from the multi-contact email: verifies the
        /// signed token, then invites that contact (moving the email's portal login to it).
        /// </summary>
        public bool ClaimChosenContact(string token, out string message)
        {
            EnsureReady();
            const string invalid = "That link has expired or is no longer valid. Please request access again.";

            if (!TryReadChooseToken(token, out int contactId, out string address))
            {
                message = invalid;
                return false;
            }

            Contact contact = _contacts.GetById(contactId);
            bool usable = contact != null
                && (contact.Enabled == null || contact.Enabled.Value)
                && (EmailEquals(contact.EmailAddress, address) || EmailEquals(contact.AltEmailAddress, address));
            if (!usable)
            {
                message = invalid;
                return false;
            }

            if (!TryInviteContact(contact, address, out string error))
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                    "Portal choose-contact invite failed for ContactID=" + contactId + ": " + error);
                message = "We could not set up access right now. Please contact Quaffee.";
                return false;
            }

            message = "A temporary password for " + (contact.CompanyName ?? "your account")
                + " has been emailed to you. Open the link in that email to choose your own password.";
            return true;
        }

        private void SendChooseContactEmail(string toAddress, List<Contact> contacts)
        {
            try
            {
                string loginUrl = BuildAppUrl("Portal/Login.aspx");
                var mail = new EmailMailKitCls { IncludeConfiguredCc = false };
                mail.SetEmailFromTo(sTo: toAddress);
                mail.SetEmailSubject("Choose which account to use for My Quaffee");
                mail.AddStrAndNewLineToBody("Hello,");
                mail.AddStrAndNewLineToBody("");
                mail.AddStrAndNewLineToBody("Your email address is on more than one Quaffee account. "
                    + "Click the account you want to use for My Quaffee and we will email you a temporary password for it:");
                mail.AddStrAndNewLineToBody("");
                foreach (Contact c in contacts.OrderBy(x => x.CompanyName ?? string.Empty))
                {
                    string link = loginUrl + "?choose=" + HttpUtility.UrlEncode(CreateChooseToken(c.ContactID, toAddress));
                    string label = string.IsNullOrWhiteSpace(c.CompanyName) ? ("Account #" + c.ContactID) : c.CompanyName.Trim();
                    mail.AddStrAndNewLineToBody("&bull; <a href=\"" + HttpUtility.HtmlAttributeEncode(link) + "\">"
                        + HttpUtility.HtmlEncode(label) + "</a>");
                }
                mail.AddStrAndNewLineToBody("");
                mail.AddStrAndNewLineToBody("These links expire in 24 hours. You can switch accounts later by requesting access again.");
                mail.AddStrAndNewLineToBody("If you did not request this, you can ignore this email.");
                mail.SendEmail();

                AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                    "Portal choose-contact email sent (" + contacts.Count + " contacts).");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                    "Portal choose-contact email failed: " + ex.Message);
            }
        }

        private static string CreateChooseToken(int contactId, string address)
        {
            long expires = DateTime.UtcNow.Add(ChooseTokenLifetime).Ticks;
            string payload = contactId + "|" + expires + "|" + address.Trim();
            byte[] protectedBytes = MachineKey.Protect(System.Text.Encoding.UTF8.GetBytes(payload), ChooseTokenPurpose);
            return HttpServerUtility.UrlTokenEncode(protectedBytes);
        }

        private static bool TryReadChooseToken(string token, out int contactId, out string address)
        {
            contactId = 0;
            address = null;
            if (string.IsNullOrWhiteSpace(token))
                return false;
            try
            {
                byte[] raw = HttpServerUtility.UrlTokenDecode(token.Trim());
                if (raw == null)
                    return false;
                string payload = System.Text.Encoding.UTF8.GetString(MachineKey.Unprotect(raw, ChooseTokenPurpose));
                string[] parts = payload.Split(new[] { '|' }, 3);
                if (parts.Length != 3
                    || !int.TryParse(parts[0], out contactId)
                    || !long.TryParse(parts[1], out long expires)
                    || DateTime.UtcNow.Ticks > expires)
                    return false;
                address = parts[2];
                return contactId > 0 && address.Length > 0;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Login, "Portal choose token rejected: " + ex.Message);
                return false;
            }
        }

        private static bool EmailEquals(string a, string b)
        {
            return string.Equals((a ?? string.Empty).Trim(), (b ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Staff invite for a known contact. Uses EmailAddress, then AltEmailAddress.
        /// The login is tied to this ContactID, so other contacts sharing the email do not block
        /// the invite — they are returned so staff can open and review them. Only an enabled
        /// contact that already holds the portal login for the email blocks; a disabled holder
        /// is moved over to this contact.
        /// </summary>
        public ContactPortalInviteResult InviteContactById(int contactId)
        {
            EnsureReady();
            var result = new ContactPortalInviteResult();
            if (contactId <= 0)
            {
                result.Message = "Save the contact first.";
                return result;
            }

            Contact contact = _contacts.GetById(contactId);
            if (contact == null || contact.ContactID <= 0)
            {
                result.Message = "Contact not found.";
                return result;
            }

            if (IsPortalExcludedContact(contact.ContactID, contact.CompanyName))
            {
                result.Message = "ZZName (sundry) contacts cannot be invited to the Contact Portal.";
                return result;
            }

            string address = PickInviteEmail(contact);
            if (string.IsNullOrEmpty(address))
            {
                result.Message = "Add an email address on this contact first.";
                return result;
            }

            List<Contact> others;
            int loginHolderContactId;
            try
            {
                others = (_contacts.FindByEmailExact(address) ?? new List<Contact>())
                    .Where(c => c != null && c.ContactID > 0 && c.ContactID != contactId)
                    .ToList();
                loginHolderContactId = GetLinkedContactIdForUserName(address);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Login, "Portal staff invite lookup failed: " + ex.Message);
                result.Message = "Could not look up that email.";
                return result;
            }

            Contact holder = null;
            if (loginHolderContactId > 0 && loginHolderContactId != contactId)
            {
                holder = others.FirstOrDefault(c => c.ContactID == loginHolderContactId)
                    ?? _contacts.GetById(loginHolderContactId);
                if (holder != null && others.All(c => c.ContactID != holder.ContactID))
                    others.Add(holder);
            }

            result.OtherContacts = others
                .OrderByDescending(c => c.ContactID == loginHolderContactId)
                .ThenByDescending(c => c.Enabled == null || c.Enabled.Value)
                .ThenBy(c => c.CompanyName ?? string.Empty)
                .Select(c => new ContactPortalEmailConflict
                {
                    ContactID = c.ContactID,
                    CompanyName = c.CompanyName ?? string.Empty,
                    Enabled = c.Enabled == null || c.Enabled.Value,
                    HoldsPortalLogin = c.ContactID == loginHolderContactId
                })
                .ToList();

            bool holderEnabled = holder != null && (holder.Enabled == null || holder.Enabled.Value);
            if (holderEnabled)
            {
                result.Message = "The portal login " + address + " is already linked to "
                    + (holder.CompanyName ?? "another contact") + " (#" + holder.ContactID + "). "
                    + "Open that contact below to review — disable it or change its email, then invite again.";
                return result;
            }

            if (!TryInviteContact(contact, address, out string detail))
            {
                result.Message = detail ?? "Invite failed.";
                return result;
            }

            result.Succeeded = true;
            string msg = "Portal invite sent to " + address + " for contact #" + contactId
                + ". They must change the temporary password on first sign-in.";
            if (holder != null)
                msg += " The login was moved from disabled contact " + (holder.CompanyName ?? string.Empty)
                    + " (#" + holder.ContactID + ").";
            int enabledOthers = result.OtherContacts.Count(c => c.Enabled && !c.HoldsPortalLogin);
            if (enabledOthers > 0)
                msg += " Note: " + enabledOthers + " other enabled contact(s) share this email — "
                    + "the portal only shows this contact's data.";
            result.Message = msg;
            return result;
        }

        /// <summary>ContactID linked to the membership user with this user name, or 0.</summary>
        private int GetLinkedContactIdForUserName(string userName)
        {
            MembershipUser user = Membership.GetUser(userName);
            if (user?.ProviderUserKey == null)
                return 0;
            return _links.GetByUserId((Guid)user.ProviderUserKey)?.ContactID ?? 0;
        }

        public ContactUserLink GetLinkForContact(int contactId)
        {
            if (contactId <= 0) return null;
            EnsureReady();
            return _links.GetByContactId(contactId);
        }

        private static string PickInviteEmail(Contact contact)
        {
            if (contact == null) return null;
            string primary = (contact.EmailAddress ?? string.Empty).Trim();
            if (primary.Length >= 5 && primary.IndexOf('@') >= 1)
                return primary;
            string alt = (contact.AltEmailAddress ?? string.Empty).Trim();
            if (alt.Length >= 5 && alt.IndexOf('@') >= 1)
                return alt;
            return null;
        }

        /// <summary>
        /// Creates/resets Contact-role user, links to contact, emails temp password.
        /// </summary>
        private bool TryInviteContact(Contact contact, string address, out string error)
        {
            error = null;
            if (contact == null || contact.ContactID <= 0 || string.IsNullOrWhiteSpace(address))
            {
                error = "Invalid contact or email.";
                return false;
            }

            if (IsPortalExcludedContact(contact.ContactID, contact.CompanyName))
            {
                error = "ZZName (sundry) contacts cannot be invited to the Contact Portal.";
                AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                    "Portal invite refused for sundry contact " + contact.ContactID);
                return false;
            }

            string userName = address.Trim();
            string tempPassword = Membership.GeneratePassword(10, 1);

            try
            {
                ContactPortalSchemaInstaller.EnsureContactRole();

                MembershipUser existing = Membership.GetUser(userName);
                Guid userId;

                if (existing == null)
                {
                    MembershipCreateStatus status;
                    MembershipUser created = Membership.CreateUser(
                        userName, tempPassword, userName, null, null, true, out status);
                    if (created == null || status != MembershipCreateStatus.Success)
                    {
                        error = status == MembershipCreateStatus.DuplicateEmail
                            ? "That email is already used by an admin Tracker login (different user name) — portal access was not created."
                            : "Could not create portal user (" + status + ").";
                        AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                            "Portal invite CreateUser failed: " + status);
                        return false;
                    }
                    userId = (Guid)created.ProviderUserKey;
                }
                else
                {
                    string[] roles = Roles.GetRolesForUser(existing.UserName) ?? new string[0];
                    if (roles.Any(r => !string.Equals(r, ContactPortalSchemaInstaller.ContactRoleName, StringComparison.OrdinalIgnoreCase)))
                    {
                        error = "That email already has an admin Tracker login — portal access was not created.";
                        AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                            "Portal invite refused — user has staff roles: " + existing.UserName);
                        return false;
                    }

                    userId = (Guid)existing.ProviderUserKey;
                    existing.UnlockUser();
                    string reset = existing.ResetPassword();
                    if (!existing.ChangePassword(reset, tempPassword))
                    {
                        error = "Could not reset the portal password.";
                        AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                            "Portal invite ChangePassword failed for " + existing.UserName);
                        return false;
                    }
                }

                if (!Roles.IsUserInRole(userName, ContactPortalSchemaInstaller.ContactRoleName))
                    Roles.AddUserToRole(userName, ContactPortalSchemaInstaller.ContactRoleName);

                int previousContactId = _links.GetByUserId(userId)?.ContactID ?? 0;
                _links.Upsert(userId, contact.ContactID, mustChangePassword: true);
                SendTempPasswordEmail(userName, contact, tempPassword);

                string by = PortalChangedBy();
                ContactChangeLogManager.LogSummary(
                    contact.ContactID,
                    ContactChangeLogManager.SourcePortal,
                    "Contact Portal access granted to " + userName + " (temporary password emailed)"
                        + (previousContactId > 0 && previousContactId != contact.ContactID
                            ? "; login moved from contact #" + previousContactId
                            : string.Empty),
                    by);
                if (previousContactId > 0 && previousContactId != contact.ContactID)
                {
                    ContactChangeLogManager.LogSummary(
                        previousContactId,
                        ContactChangeLogManager.SourcePortal,
                        "Contact Portal login " + userName + " moved to contact #" + contact.ContactID,
                        by);
                }

                AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                    "Portal invite sent for ContactID=" + contact.ContactID);
                return true;
            }
            catch (Exception ex)
            {
                error = "Invite error: " + ex.Message;
                AppLogger.WriteLog(SystemConstants.LogTypes.Login, "Portal invite error: " + ex.Message);
                return false;
            }
        }

        private void SendTempPasswordEmail(string toAddress, Contact contact, string tempPassword)
        {
            try
            {
                var mail = new EmailMailKitCls { IncludeConfiguredCc = false };
                string name = string.IsNullOrWhiteSpace(contact.ContactFirstName)
                    ? (contact.CompanyName ?? "there")
                    : contact.ContactFirstName.Trim();
                string setPasswordUrl = BuildAppUrl("Portal/ChangePassword.aspx") + "?u=" + HttpUtility.UrlEncode(toAddress);

                mail.SetEmailFromTo(sTo: toAddress);
                mail.SetEmailSubject("Your My Quaffee login");
                mail.AddStrAndNewLineToBody("Hello " + HttpUtility.HtmlEncode(name) + ",");
                mail.AddStrAndNewLineToBody("");
                mail.AddStrAndNewLineToBody("Your My Quaffee login is ready:");
                mail.AddStrAndNewLineToBody("Username: " + HttpUtility.HtmlEncode(toAddress));
                mail.AddStrAndNewLineToBody("Temporary password: " + HttpUtility.HtmlEncode(tempPassword));
                mail.AddStrAndNewLineToBody("");
                mail.AddStrAndNewLineToBody("<a href=\"" + HttpUtility.HtmlAttributeEncode(setPasswordUrl)
                    + "\">Click here to choose your own password</a>. Enter the temporary password above, then your new password, and you will be signed in.");
                mail.AddStrAndNewLineToBody("");
                mail.AddStrAndNewLineToBody("If you did not request this, you can ignore this email.");
                mail.SendEmail();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                    "Portal invite email failed: " + ex.Message);
            }
        }

        /// <summary>Absolute URL for emails, e.g. BuildAppUrl("Account/Login.aspx"); "~/..." when no base URL is known.</summary>
        private static string BuildAppUrl(string relativePath)
        {
            string path = relativePath.TrimStart('~', '/');
            try
            {
                string baseUrl = DisableClientManager.GetApplicationUrl();
                if (!string.IsNullOrWhiteSpace(baseUrl))
                    return baseUrl.TrimEnd('/') + "/" + path;
            }
            catch
            {
                try
                {
                    string configured = ConfigHelper.GetString("ApplicationBaseUrl", string.Empty).Trim();
                    if (!string.IsNullOrEmpty(configured))
                        return configured.TrimEnd('/') + "/" + path;

                    var req = HttpContext.Current?.Request;
                    if (req != null)
                        return req.Url.GetLeftPart(UriPartial.Authority)
                            + VirtualPathUtility.ToAbsolute("~/" + path);
                }
                catch { }
            }
            return "~/" + path;
        }

        /// <summary>
        /// HTML blurb for order / reminder emails: join link, or “you already have access” if linked.
        /// Empty when contactId is invalid or sundry.
        /// </summary>
        public static string BuildEmailPortalBlurbHtml(int contactId)
        {
            if (contactId <= 0)
                return string.Empty;

            try
            {
                string companyName = new ContactsRepository().GetContactNameById(contactId);
                if (IsPortalExcludedContact(contactId, companyName))
                    return string.Empty;

                var mgr = new ContactPortalManager();
                mgr.EnsureReady();
                var link = mgr.GetLinkForContact(contactId);
                string url = BuildAppUrl(link != null ? "Account/Login.aspx" : "Portal/Login.aspx?request=1");
                if (string.IsNullOrWhiteSpace(url) || url.StartsWith("~"))
                    return string.Empty;

                string encodedUrl = HttpUtility.HtmlAttributeEncode(url);
                string displayUrl = HttpUtility.HtmlEncode(url);

                if (link != null)
                {
                    return MessageProvider.Format(
                        MessageKeys.Portal.EmailAlreadyJoined,
                        encodedUrl,
                        displayUrl);
                }

                return MessageProvider.Format(
                    MessageKeys.Portal.EmailJoinInvite,
                    encodedUrl,
                    displayUrl);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                    "Portal email blurb failed for contact " + contactId + ": " + ex.Message);
                return string.Empty;
            }
        }

        public Contact GetOwnContact()
        {
            int id = GetCurrentContactId();
            return id > 0 ? _contacts.GetById(id) : null;
        }

        public HashSet<string> GetEditableFields()
        {
            EnsureReady();
            return ContactPortalSettingsRepository.ParseFields(_settings.Get().EditableContactFields);
        }

        public bool SaveAllowedContactFields(Contact updates, out string error)
        {
            if (IsViewingAs())
            {
                error = ViewAsReadOnlyMessage;
                return false;
            }
            error = null;
            var current = GetOwnContact();
            if (current == null)
            {
                error = "Contact not found.";
                return false;
            }

            var allowed = GetEditableFields();
            if (allowed.Count == 0)
            {
                error = "No contact fields are editable. Please submit a change request.";
                return false;
            }

            Contact before = _contacts.GetById(current.ContactID);
            ApplyAllowed(current, updates, allowed);
            if (!_contacts.Update(current))
            {
                error = "Could not save contact.";
                return false;
            }

            try
            {
                ContactChangeLogManager.LogDiff(
                    current.ContactID,
                    before,
                    current,
                    ContactChangeLogManager.SourcePortal,
                    "Portal contact field update");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "Portal contact change-log failed: " + ex.Message);
            }

            return true;
        }

        private static void ApplyAllowed(Contact target, Contact source, HashSet<string> allowed)
        {
            if (source == null) return;
            if (allowed.Contains("CompanyName")) target.CompanyName = source.CompanyName;
            if (allowed.Contains("ContactTitle")) target.ContactTitle = source.ContactTitle;
            if (allowed.Contains("ContactFirstName")) target.ContactFirstName = source.ContactFirstName;
            if (allowed.Contains("ContactLastName")) target.ContactLastName = source.ContactLastName;
            if (allowed.Contains("ContactAltFirstName")) target.ContactAltFirstName = source.ContactAltFirstName;
            if (allowed.Contains("ContactAltLastName")) target.ContactAltLastName = source.ContactAltLastName;
            if (allowed.Contains("Department")) target.Department = source.Department;
            if (allowed.Contains("BillingAddress")) target.BillingAddress = source.BillingAddress;
            if (allowed.Contains("PostalCode")) target.PostalCode = source.PostalCode;
            if (allowed.Contains("StateOrProvince")) target.StateOrProvince = source.StateOrProvince;
            if (allowed.Contains("PhoneNumber")) target.PhoneNumber = source.PhoneNumber;
            if (allowed.Contains("CellNumber")) target.CellNumber = source.CellNumber;
            if (allowed.Contains("EmailAddress")) target.EmailAddress = source.EmailAddress;
            if (allowed.Contains("AltEmailAddress")) target.AltEmailAddress = source.AltEmailAddress;
        }

        public List<ContactOrderSummary> GetOwnOrders()
        {
            int id = GetCurrentContactId();
            return id > 0 ? _orders.GetSummariesByContactId(id) : new List<ContactOrderSummary>();
        }

        /// <summary>Orders with every line listed (Contact Portal list shows all items, not just the first).</summary>
        public List<ContactOrderSummary> GetOwnOrdersWithItems()
        {
            int id = GetCurrentContactId();
            if (id <= 0)
                return new List<ContactOrderSummary>();

            var orders = _orders.GetSummariesByContactId(id);
            var linesByOrder = _orders.GetPortalOrderLinesByContact(id)
                .GroupBy(l => l.OrderID)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var order in orders)
            {
                if (linesByOrder.TryGetValue(order.OrderID, out var lines))
                    order.ItemLines = lines.Select(l => l.Display).ToList();
            }
            return orders;
        }

        public OrderHeaderData GetOwnOrderHeader(int orderId)
        {
            if (orderId <= 0) return null;
            var header = new OrderManager().GetOrderHeader(orderId);
            if (header == null) return null;
            if ((int)header.CustomerID != GetCurrentContactId())
                return null;
            return header;
        }

        public List<OrderDetailData> GetOwnOrderLines(int orderId)
        {
            if (GetOwnOrderHeader(orderId) == null)
                return new List<OrderDetailData>();
            return new OrderManager().GetOrderLines(orderId) ?? new List<OrderDetailData>();
        }

        /// <summary>Summary row (status, PO) for one of the signed-in contact's orders.</summary>
        public ContactOrderSummary GetOwnOrderSummary(int orderId)
        {
            if (orderId <= 0) return null;
            return GetOwnOrders().FirstOrDefault(o => o.OrderID == orderId);
        }

        public List<ContactPortalOrderLine> GetOwnOrderLinesForDisplay(int orderId)
        {
            if (GetOwnOrderHeader(orderId) == null)
                return new List<ContactPortalOrderLine>();
            return _orders.GetPortalOrderLines(orderId);
        }

        public List<ContactPortalRepairView> GetOwnRepairs()
        {
            int id = GetCurrentContactId();
            return id > 0 ? _repairs.GetPortalViews(id) : new List<ContactPortalRepairView>();
        }

        public ContactPortalRepairView GetOwnRepair(int repairId)
        {
            int id = GetCurrentContactId();
            if (id <= 0 || repairId <= 0) return null;
            return _repairs.GetPortalViews(id, repairId).FirstOrDefault();
        }

        /// <summary>True when the signed-in contact's record is disabled (staff or Disable Client link).</summary>
        public bool IsOwnContactDisabled()
        {
            Contact c = GetOwnContact();
            return c != null && c.Enabled == false;
        }

        /// <summary>True when checkup reminder emails are on for the signed-in contact.</summary>
        public bool AreOwnRemindersEnabled()
        {
            Contact c = GetOwnContact();
            return c != null && c.PredictionDisabled != true;
        }

        /// <summary>
        /// Customer turns checkup reminder emails on or off. Off uses the Disable Client
        /// reminders-only path (Quaffee is notified); on clears PredictionDisabled with a contact note.
        /// </summary>
        public bool SetOwnReminders(bool enabled, out string message)
        {
            if (IsViewingAs())
            {
                message = ViewAsReadOnlyMessage;
                return false;
            }
            int id = GetCurrentContactId();
            if (id <= 0)
            {
                message = "Not signed in as a contact.";
                return false;
            }

            if (HasActiveRecurring())
            {
                message = "You have an active recurring order, so reminder emails cannot be changed here. Please send us a change request below.";
                return false;
            }

            Contact before = _contacts.GetById(id);
            bool ok = enabled
                ? _contacts.SetPredictionDisabled(id, false, addNote: false)
                : DisableClientManager.DisableRemindersFromPortal(id);

            if (!ok)
            {
                message = "Could not update your reminder preference. Please try again or send us a change request.";
                return false;
            }

            ContactChangeLogManager.LogDiff(
                id,
                before,
                _contacts.GetById(id),
                ContactChangeLogManager.SourcePortal,
                enabled ? "Contact turned reminder emails on" : "Contact turned reminder emails off");

            message = enabled
                ? "Reminder emails are on. We will let you know when you may be running low."
                : "Reminder emails are off. You will still receive order and repair updates.";
            return true;
        }

        /// <summary>Courier, waybill and tracking link for one of the contact's orders; null when not dispatched by courier.</summary>
        public ContactPortalCourierInfo GetOwnOrderCourier(int orderId)
        {
            if (GetOwnOrderHeader(orderId) == null)
                return null;

            OrderWaybill waybill;
            try
            {
                waybill = new OrderWaybillRepository().GetByOrderId(orderId);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "Portal courier lookup failed for order " + orderId + ": " + ex.Message);
                return null;
            }

            if (waybill == null || string.IsNullOrWhiteSpace(waybill.WaybillNumber))
                return null;

            var info = new ContactPortalCourierInfo
            {
                WaybillNumber = waybill.WaybillNumber.Trim(),
                CourierName = (waybill.Carrier ?? string.Empty).Trim(),
                DispatchedAt = waybill.DispatchedAt
            };

            CourierService courier = ResolveWaybillCourier(waybill, GetOwnOrderHeader(orderId));
            if (courier != null)
            {
                info.CourierName = courier.ServiceName ?? courier.ServiceCode ?? info.CourierName;
                info.TrackingUrl = courier.BuildTrackingUrl(info.WaybillNumber);
            }

            return info;
        }

        /// <summary>
        /// Courier for tracking: the one saved on the waybill; else a courier whose name matches the
        /// waybill's Carrier text; else the contact's preferred / default courier, the same rule used
        /// when the waybill was captured. Older waybills only have Carrier = the "Courier" delivery person.
        /// </summary>
        private CourierService ResolveWaybillCourier(OrderWaybill waybill, OrderHeaderData header)
        {
            var repo = new CourierServicesRepository();
            CourierService courier = null;
            try
            {
                if (waybill.CourierServiceID.HasValue && waybill.CourierServiceID.Value > 0)
                    courier = repo.GetByIdSafe(waybill.CourierServiceID.Value);

                string carrier = (waybill.Carrier ?? string.Empty).Trim();
                if (courier == null && carrier.Length > 0)
                {
                    courier = repo.GetAll("SortOrder").FirstOrDefault(c => c.IsEnabled && !c.IsNone
                        && (string.Equals(c.ServiceName, carrier, StringComparison.OrdinalIgnoreCase)
                            || string.Equals(c.ServiceCode, carrier, StringComparison.OrdinalIgnoreCase)));
                }

                if (courier == null && header != null)
                {
                    int? preferred = _contacts.GetById((int)header.CustomerID)?.PreferredCourierServiceID;
                    int? id = repo.ResolvePreferredId(preferred, header.ToBeDeliveredBy);
                    if (id.HasValue)
                        courier = repo.GetByIdSafe(id.Value);
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "Portal courier resolve failed: " + ex.Message);
            }
            return courier != null && !courier.IsNone ? courier : null;
        }

        /// <summary>
        /// Online shop (WooCommerce) order behind a Tracker order; null when it did not come from the shop.
        /// ViewUrl is only set when the shop order belongs to a shop account (guest checkouts cannot view it).
        /// </summary>
        public ContactPortalShopOrder GetOwnOrderShopLink(int orderId)
        {
            if (GetOwnOrderHeader(orderId) == null)
                return null;
            try
            {
                WooOrderInfo woo = new WooOrderInfoRepository().GetByTrackerOrderId(orderId);
                if (woo == null || woo.WooOrderId <= 0)
                    return null;

                var result = new ContactPortalShopOrder
                {
                    OrderNumber = string.IsNullOrWhiteSpace(woo.WooOrderNumber)
                        ? woo.WooOrderId.ToString(System.Globalization.CultureInfo.InvariantCulture)
                        : woo.WooOrderNumber.Trim()
                };

                var match = System.Text.RegularExpressions.Regex.Match(woo.RawSnapshotJson ?? string.Empty, "\"customer_id\"\\s*:\\s*(\\d+)");
                bool hasShopAccount = match.Success && long.TryParse(match.Groups[1].Value, out long customerId) && customerId > 0;
                string store = new WooCommerceSettingsManager().GetSettings()?.StoreBaseUrl?.Trim();
                if (hasShopAccount && !string.IsNullOrEmpty(store))
                    result.ViewUrl = store.TrimEnd('/') + "/my-account/view-order/"
                        + woo.WooOrderId.ToString(System.Globalization.CultureInfo.InvariantCulture) + "/";
                return result;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "Portal shop order lookup failed for order " + orderId + ": " + ex.Message);
                return null;
            }
        }

        public List<RecurringOrderSummary> GetOwnRecurring()
        {
            int id = GetCurrentContactId();
            return id > 0 ? _recurring.GetSummariesByContactId(id) : new List<RecurringOrderSummary>();
        }

        /// <summary>
        /// Reminders are locked while an active recurring order exists: the reminder (prediction)
        /// calculation and recurring deliveries can conflict, so only Quaffee may change it.
        /// </summary>
        public bool HasActiveRecurring()
        {
            return GetOwnRecurring().Any(r => r.Enabled == true);
        }

        /// <summary>
        /// The contact's earliest order that is not completed yet; null when every order is done.
        /// Repeat My Last Order is not offered while one exists.
        /// </summary>
        public ContactOrderSummary GetOwnOpenOrder()
        {
            return GetOwnOrders()
                .Where(o => !o.Done)
                .OrderBy(o => o.RequiredByDate ?? DateTime.MaxValue)
                .FirstOrDefault();
        }

        /// <summary>
        /// Items from the contact's last order (or preferences when there is none) with prep/delivery
        /// dates from the contact's area schedule — same rules as a new order in Order Detail.
        /// </summary>
        public ContactPortalRepeatOrderPreview GetRepeatOrderPreview()
        {
            int id = GetCurrentContactId();
            if (id <= 0)
                return null;

            var orderManager = new OrderManager();
            var preview = new ContactPortalRepeatOrderPreview();
            foreach (var item in orderManager.GetLastOrderItems(id, setDates: false))
            {
                preview.Lines.Add(new ContactPortalOrderLine
                {
                    ItemDesc = item.ItemName,
                    Qty = item.Qty,
                    PackagingDesc = item.PackagingName
                });
            }

            ResolveScheduleDates(id, orderManager, out DateTime prep, out DateTime delivery);
            preview.PrepDate = prep;
            preview.DeliveryDate = delivery;
            preview.RequiresPurchaseOrder = new TrackerTools().RetrieveCustomerPrefs(id).RequiresPurchOrder;
            preview.ExistingOrderId = orderManager.FindExistingOrderForHeader(new OrderHeaderData
            {
                CustomerID = id,
                RequiredByDate = delivery
            }) ?? 0;
            return preview;
        }

        /// <summary>
        /// Places a repeat of the contact's last order on the area-schedule date. Requested changes go
        /// into the order notes; the order is left unconfirmed and orders@ is emailed to review it.
        /// </summary>
        public int PlaceRepeatOrder(string purchaseOrder, string requestedChanges, out string message)
        {
            if (IsViewingAs())
            {
                message = ViewAsReadOnlyMessage;
                return 0;
            }
            int id = GetCurrentContactId();
            if (id <= 0)
            {
                message = "Not signed in as a contact.";
                return 0;
            }

            ContactOrderSummary open = GetOwnOpenOrder();
            if (open != null)
            {
                message = "You already have an order in progress (" + ContactPortalDisplay.FormatDate(open.RequiredByDate)
                    + "). Open it in My Orders and use Request A Change if you need anything added.";
                return 0;
            }

            var orderManager = new OrderManager();
            var items = orderManager.GetLastOrderItems(id, setDates: false);
            if (items.Count == 0)
            {
                message = "We could not find a previous order to repeat. Please send us a change request below with what you need.";
                return 0;
            }

            var prefs = new TrackerTools().RetrieveCustomerPrefs(id);
            string po = (purchaseOrder ?? string.Empty).Trim();
            if (po.Length > 30)
                po = po.Substring(0, 30);
            if (prefs.RequiresPurchOrder && po.Length == 0)
            {
                message = "Your account needs a purchase order (PO) number for each order. Please enter it and try again.";
                return 0;
            }

            string changes = (requestedChanges ?? string.Empty).Trim();
            if (changes.Length > 1000)
                changes = changes.Substring(0, 1000);

            ResolveScheduleDates(id, orderManager, out DateTime prep, out DateTime delivery);

            int deliveryBy = prefs.PreferredDeliveryByID > 0
                ? prefs.PreferredDeliveryByID
                : SystemConstants.DeliveryConstants.DefaultDeliveryPersonID;
            int? normalDoW = new PersonsRepository().GetNormalDeliveryDoW(deliveryBy);
            if (normalDoW.HasValue && normalDoW.Value != 0 && normalDoW.Value != (int)delivery.DayOfWeek + 1)
                deliveryBy = SystemConstants.DeliveryConstants.DefaultDeliveryPersonID;

            var header = new OrderHeaderData
            {
                CustomerID = id,
                OrderDate = TimeZoneUtils.Now().Date,
                PrepDate = prep,
                RequiredByDate = delivery,
                ToBeDeliveredBy = deliveryBy,
                PurchaseOrder = po,
                Confirmed = false,
                Notes = "Contact Portal repeat order" + (changes.Length > 0 ? ". Requested changes: " + changes : string.Empty)
            };

            var ensure = orderManager.EnsureOrderHeader(header);
            if (ensure.IsConflict)
            {
                message = "You already have an order for " + ContactPortalDisplay.FormatDate(delivery)
                    + ". Open it in My Orders and use Request A Change if you need anything added.";
                return 0;
            }
            if (!ensure.Success)
            {
                message = "We could not place your order. Please try again or send us a change request.";
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "Portal repeat order header failed for contact " + id + ": " + ensure.Error);
                return 0;
            }

            var lines = items.Select(item => new OrderTblData
            {
                CustomerID = id,
                ItemTypeID = item.ItemID,
                QuantityOrdered = item.Qty,
                PrepTypeID = item.PrepTypeID,
                PackagingID = item.PackagingID
            }).ToList();

            var added = orderManager.AddOrderLines(header, lines, ensure.OrderId);
            if (!added.Success)
            {
                message = "We could not add the items to your order. Please send us a change request and we will sort it out.";
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "Portal repeat order lines failed for order " + ensure.OrderId + ": " + added.Error);
                return 0;
            }

            int orderId = added.OrderId;
            AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                "Portal repeat order #" + orderId + " placed by contact " + id + " for " + delivery.ToString("yyyy-MM-dd") + " (" + lines.Count + " lines)");
            ContactChangeLogManager.LogSummary(
                id,
                ContactChangeLogManager.SourcePortal,
                "Placed repeat order #" + orderId + " for " + delivery.ToString("yyyy-MM-dd")
                    + (changes.Length > 0 ? " with requested changes: " + (changes.Length > 200 ? changes.Substring(0, 199) + "…" : changes) : string.Empty));
            NotifyStaffOfRepeatOrder(orderId, id, delivery, items, po, changes);

            header.OrderID = orderId;
            bool acknowledged = SendRepeatOrderAcknowledgement(id, header, items, changes);

            message = "Thank you, we have received your order for " + ContactPortalDisplay.FormatDate(delivery) + "."
                + " We will endeavour to supply it as ordered, but it may need to change depending on stock and availability."
                + (changes.Length > 0 ? " We will review your requested changes before it goes out." : string.Empty)
                + (acknowledged ? " A copy has been emailed to you." : string.Empty);
            return orderId;
        }

        private bool SendRepeatOrderAcknowledgement(int contactId, OrderHeaderData header,
            List<OrderManager.OrderLineData> items, string changes)
        {
            try
            {
                var contact = _contacts.GetContactEmailDetails(contactId);
                var lines = items.Select(i => new OrderLineData
                {
                    ItemID = i.ItemID,
                    ItemName = i.ItemName,
                    Qty = i.Qty,
                    PackagingID = i.PackagingID,
                    PackagingName = i.PackagingName
                }).ToList();

                bool ok = new OrderDetailManager().SendPortalOrderAcknowledgement(contact, header, lines, changes, out string status);
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "Portal repeat order #" + header.OrderID + ": " + status);
                return ok;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                    "Portal repeat order #" + header.OrderID + ": acknowledgement failed: " + ex.Message);
                return false;
            }
        }

        private static void ResolveScheduleDates(int contactId, OrderManager orderManager, out DateTime prep, out DateTime delivery)
        {
            DateTime today = TimeZoneUtils.Now().Date;
            delivery = today;
            prep = DateTime.MinValue;
            try
            {
                prep = new TrackerTools().GetNextPreparationDateByCustomerID(contactId, ref delivery);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "Portal schedule dates failed for contact " + contactId + ": " + ex.Message);
            }

            if (prep <= DateTime.MinValue || delivery <= DateTime.MinValue)
            {
                var fallback = orderManager.CalculateOrderDates(today);
                prep = fallback.PrepDate;
                delivery = fallback.deliveryDate;
            }
            prep = prep.Date;
            delivery = delivery.Date;
        }

        private void NotifyStaffOfRepeatOrder(int orderId, int contactId, DateTime delivery,
            List<OrderManager.OrderLineData> items, string po, string changes)
        {
            var contact = _contacts.GetById(contactId);
            string company = contact?.CompanyName ?? ("Contact #" + contactId);
            var lines = new List<string>
            {
                HttpUtility.HtmlEncode(company) + " (#" + contactId + ") placed a repeat of their last order in the Contact Portal.",
                "Order #" + orderId + " for " + delivery.ToString("yyyy-MM-dd") + " (left unconfirmed for review)",
                "The contact was emailed an acknowledgement that the order may change with stock and availability. Use Confirm Order in Order Detail once it has been checked.",
                contact?.Enabled == false
                    ? "<strong>This contact is currently disabled.</strong> Re-enable it in Contact Details if they are ordering again."
                    : null,
                string.IsNullOrEmpty(po) ? null : "PO: " + HttpUtility.HtmlEncode(po),
                "",
                "<strong>Items</strong>"
            };
            lines.AddRange(items.Select(i => HttpUtility.HtmlEncode(
                (i.ItemName ?? "(item)") + " × " + i.Qty.ToString("0.###")
                + (string.IsNullOrWhiteSpace(i.PackagingName) ? string.Empty : " (" + i.PackagingName + ")"))));
            if (changes.Length > 0)
            {
                lines.Add("");
                lines.Add("<strong>Requested changes</strong>");
                lines.Add(HttpUtility.HtmlEncode(changes).Replace("\n", "<br />"));
            }
            lines.Add("");
            lines.Add(StaffLink("Open order", "~/Pages/OrderDetail.aspx?OrderID=" + orderId)
                + " &nbsp;|&nbsp; " + StaffLink("Open contact", "~/Pages/ContactDetails.aspx?ID=" + contactId));

            NotifyStaff(
                "Contact Portal order #" + orderId + ": " + company + (changes.Length > 0 ? " (changes requested)" : string.Empty),
                lines.Where(l => l != null),
                "Portal repeat order #" + orderId);
        }

        /// <summary>
        /// Next recurring delivery when a recurring order is active; otherwise the predicted
        /// NextCoffeeBy (recalculated on every Order Done, whether or not reminders are on).
        /// Null when there is no date.
        /// </summary>
        public ContactPortalNextCoffee GetOwnNextCoffee()
        {
            int id = GetCurrentContactId();
            if (id <= 0)
                return null;

            DateTime today = TimeZoneUtils.Now().Date;
            var recurring = GetOwnRecurring().Where(r => r.Enabled == true).ToList();
            if (recurring.Count > 0)
            {
                DateTime? next = recurring
                    .Where(r => r.NextDateRequired.HasValue)
                    .Select(r => r.NextDateRequired.Value.Date)
                    .DefaultIfEmpty()
                    .Min();
                if (!next.HasValue || next.Value == default(DateTime))
                    return null;
                return new ContactPortalNextCoffee { FromRecurring = true, Date = next.Value, IsDueNow = next.Value <= today };
            }

            DateTime? nextBy = new ContactsUsageRepository().GetByContactId(id)?.NextCoffeeBy;
            if (!nextBy.HasValue)
                return null;
            return new ContactPortalNextCoffee { FromRecurring = false, Date = nextBy.Value.Date, IsDueNow = nextBy.Value.Date <= today };
        }

        public RecurringOrder GetOwnRecurring(int recurringOrderId)
        {
            if (recurringOrderId <= 0) return null;
            var r = _recurring.GetById(recurringOrderId);
            if (r == null || !r.ContactID.HasValue || r.ContactID.Value != GetCurrentContactId())
                return null;
            return r;
        }

        public long SubmitChangeRequest(string kind, long? relatedId, string text, out string message)
        {
            EnsureReady();
            message = null;
            if (IsViewingAs())
            {
                message = ViewAsReadOnlyMessage;
                return 0;
            }
            int contactId = GetCurrentContactId();
            if (contactId <= 0)
            {
                message = "Not signed in as a contact.";
                return 0;
            }

            string cleaned = (text ?? string.Empty).Trim();
            if (cleaned.Length < 5)
            {
                message = "Please enter a bit more detail (at least 5 characters).";
                return 0;
            }
            if (cleaned.Length > 2000)
                cleaned = cleaned.Substring(0, 2000);

            // Ownership checks when related
            if (relatedId.HasValue && relatedId.Value > 0)
            {
                if (string.Equals(kind, ContactPortalChangeKinds.Order, StringComparison.OrdinalIgnoreCase)
                    && GetOwnOrderHeader((int)relatedId.Value) == null)
                {
                    message = "Order not found.";
                    return 0;
                }
                if (string.Equals(kind, ContactPortalChangeKinds.Repair, StringComparison.OrdinalIgnoreCase)
                    && GetOwnRepair((int)relatedId.Value) == null)
                {
                    message = "Repair not found.";
                    return 0;
                }
                if (string.Equals(kind, ContactPortalChangeKinds.Recurring, StringComparison.OrdinalIgnoreCase)
                    && GetOwnRecurring((int)relatedId.Value) == null)
                {
                    message = "Recurring order not found.";
                    return 0;
                }
            }

            var user = Membership.GetUser();
            Guid? userId = user?.ProviderUserKey as Guid?;

            long id = _requests.Insert(new ContactPortalChangeRequest
            {
                ContactID = contactId,
                UserId = userId,
                Kind = kind ?? ContactPortalChangeKinds.Contact,
                RelatedId = relatedId,
                RequestText = cleaned
            });

            if (id <= 0)
            {
                message = "Could not save the request.";
                return 0;
            }

            string about = string.Equals(kind, ContactPortalChangeKinds.Contact, StringComparison.OrdinalIgnoreCase)
                ? "contact details"
                : (kind ?? "contact").ToLowerInvariant() + (relatedId.HasValue && relatedId.Value > 0 ? " " + relatedId.Value : string.Empty);
            ContactChangeLogManager.LogSummary(
                contactId,
                ContactChangeLogManager.SourcePortal,
                "Change request #" + id + " (" + about + "): "
                    + (cleaned.Length > 300 ? cleaned.Substring(0, 299) + "…" : cleaned));

            NotifyStaffOfChangeRequest(id, contactId, kind, relatedId, cleaned);
            message = "Change request submitted. Reference #" + id;
            return id;
        }

        private void NotifyStaffOfChangeRequest(long id, int contactId, string kind, long? relatedId, string text)
        {
            if (!ConfigHelper.GetBool("RequestChanges.NotifyEmail", true))
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "Portal change request #" + id + ": not emailed (RequestChanges.NotifyEmail is false)");
                return;
            }

            var contact = _contacts.GetById(contactId);
            string company = contact?.CompanyName ?? ("Contact #" + contactId);
            NotifyStaff(
                "Contact Portal change request #" + id + ": " + company + " (" + kind + ")",
                new[]
                {
                    "Contact Portal change request #" + id,
                    "Contact: " + HttpUtility.HtmlEncode(company) + " (#" + contactId + ")",
                    "About: " + HttpUtility.HtmlEncode(kind) + (relatedId.HasValue && relatedId.Value > 0 ? " " + relatedId.Value : string.Empty),
                    "",
                    HttpUtility.HtmlEncode(text).Replace("\n", "<br />"),
                    "",
                    StaffLink("Open contact", "~/Pages/ContactDetails.aspx?ID=" + contactId)
                        + " &nbsp;|&nbsp; " + StaffLink("Open change requests", "~/Tools/ContactPortalAdmin.aspx")
                },
                "Portal change request #" + id);
        }

        public ContactPortalSettings GetSettings()
        {
            EnsureReady();
            return _settings.Get();
        }

        public bool SaveSettings(string fieldsCsv, string updatedBy)
        {
            EnsureReady();
            return _settings.Save(fieldsCsv, updatedBy);
        }

        /// <summary>Change requests for the staff queue; empty status = all.</summary>
        public List<ContactPortalChangeRequest> ListRequests(string status, int max = 500)
        {
            EnsureReady();
            return _requests.ListByStatus(status ?? string.Empty, max);
        }

        /// <summary>
        /// Marks an open request Done or Rejected (never deleted), records it in the Change Log
        /// and emails the contact the outcome with the optional staff note.
        /// </summary>
        public bool ResolveRequest(long requestId, string status, string staffNote, string resolvedBy, out string message)
        {
            EnsureReady();
            string note = string.IsNullOrWhiteSpace(staffNote) ? null : staffNote.Trim();
            if (note != null && note.Length > 1000)
                note = note.Substring(0, 1000);

            var req = _requests.GetById(requestId);
            if (req == null || req.Status != ContactPortalChangeStatuses.Open
                || !_requests.Resolve(requestId, status, resolvedBy, note))
            {
                message = "Could not update request #" + requestId + " (it may already have been resolved).";
                return false;
            }

            string outcome = status == ContactPortalChangeStatuses.Done ? "done" : "rejected";
            ContactChangeLogManager.LogSummary(
                req.ContactID,
                ContactChangeLogManager.SourcePortal,
                "Change request #" + requestId + " marked " + outcome + (note == null ? string.Empty : ": " + note),
                resolvedBy);

            bool emailed = SendChangeRequestOutcomeEmail(req, status, note, out string emailedTo);
            message = "Request #" + requestId + " marked " + outcome + ". "
                + (emailed ? "The contact was emailed at " + emailedTo + "." : "The contact could not be emailed (see the system log).");
            return true;
        }

        /// <summary>"Contact details", "Order 1234", "Repair 55"… for staff and contact wording.</summary>
        public static string DescribeRequestSubject(string kind, long? relatedId)
        {
            if (string.IsNullOrEmpty(kind) || string.Equals(kind, ContactPortalChangeKinds.Contact, StringComparison.OrdinalIgnoreCase))
                return "Contact details";
            string label = string.Equals(kind, ContactPortalChangeKinds.Recurring, StringComparison.OrdinalIgnoreCase)
                ? "Recurring order"
                : kind;
            return label + (relatedId.HasValue && relatedId.Value > 0 ? " " + relatedId.Value : string.Empty);
        }

        private bool SendChangeRequestOutcomeEmail(ContactPortalChangeRequest req, string status, string note, out string emailedTo)
        {
            emailedTo = null;
            string context = "Portal change request #" + req.RequestID + " outcome";

            string to = null;
            if (req.UserId.HasValue)
                to = Membership.GetUser(req.UserId.Value)?.Email;
            if (string.IsNullOrWhiteSpace(to))
            {
                var link = _links.GetByContactId(req.ContactID);
                if (link != null)
                    to = Membership.GetUser(link.UserId)?.Email;
            }
            var contact = _contacts.GetById(req.ContactID);
            if (string.IsNullOrWhiteSpace(to))
                to = contact?.EmailAddress;
            if (string.IsNullOrWhiteSpace(to))
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, context + ": not emailed (no email address)");
                return false;
            }

            string name = string.IsNullOrWhiteSpace(contact?.ContactFirstName)
                ? (contact?.CompanyName ?? "there")
                : contact.ContactFirstName.Trim();
            bool done = status == ContactPortalChangeStatuses.Done;
            string subject = DescribeRequestSubject(req.Kind, req.RelatedId).ToLowerInvariant();
            if (subject.StartsWith("contact details"))
                subject = "your contact details";

            try
            {
                var mail = new EmailMailKitCls { IncludeConfiguredCc = false };
                mail.SetEmailFromTo(sTo: to.Trim());
                mail.SetEmailSubject("Quaffee: your change request #" + req.RequestID + (done ? " has been done" : " was not actioned"));
                mail.AddStrAndNewLineToBody("Hello " + HttpUtility.HtmlEncode(name) + ",");
                mail.AddStrAndNewLineToBody("");
                mail.AddStrAndNewLineToBody(done
                    ? "We have made the change you requested to " + HttpUtility.HtmlEncode(subject) + "."
                    : "We were not able to make the change you requested to " + HttpUtility.HtmlEncode(subject) + ".");
                if (note != null)
                {
                    mail.AddStrAndNewLineToBody("");
                    mail.AddStrAndNewLineToBody("<strong>Note from Quaffee:</strong> " + HttpUtility.HtmlEncode(note).Replace("\n", "<br />"));
                }
                mail.AddStrAndNewLineToBody("");
                mail.AddStrAndNewLineToBody("<strong>Your request</strong>");
                mail.AddStrAndNewLineToBody(HttpUtility.HtmlEncode(req.RequestText ?? string.Empty).Replace("\n", "<br />"));
                mail.AddStrAndNewLineToBody("");
                mail.AddStrAndNewLineToBody(done
                    ? "You can check your details any time in My Quaffee."
                    : "If you have any questions, just reply to this email or contact us.");

                string target = mail.IsTestMode ? mail.TestRecipientAddress + " (EmailTestMode; normally " + to + ")" : to;
                if (mail.SendEmail())
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.System, context + ": emailed " + target);
                    emailedTo = to.Trim();
                    return true;
                }
                AppLogger.WriteLog(SystemConstants.LogTypes.System, context + ": email to " + target + " FAILED: "
                    + (string.IsNullOrWhiteSpace(mail.LastErrorSummary) ? "unknown error" : mail.LastErrorSummary));
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, context + ": email to " + to + " FAILED: " + ex.Message);
            }
            return false;
        }
    }
}
