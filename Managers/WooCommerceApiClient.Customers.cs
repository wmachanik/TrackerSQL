using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TrackerSQL.Classes;

namespace TrackerSQL.Managers
{
    public partial class WooCommerceApiClient
    {
        public class WooCustomerDto
        {
            public long Id { get; set; }
            public string Email { get; set; }
            /// <summary>Primary role from Woo customers API (often "customer" even when member is also assigned).</summary>
            public string Role { get; set; }
            public IList<string> Roles { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }

            public string RolesDisplay =>
                Roles != null && Roles.Count > 0
                    ? string.Join(", ", Roles.OrderBy(r => r, StringComparer.OrdinalIgnoreCase))
                    : Role;
        }

        /// <param name="rolesToProbe">Extra Woo role slugs to detect via role= filter (e.g. member).</param>
        public WooCustomerDto FindCustomerByEmail(ApiCredentials creds, string email, out string detail,
            IEnumerable<string> rolesToProbe = null)
        {
            detail = null;
            if (creds == null || string.IsNullOrWhiteSpace(email))
            {
                detail = "Email is required.";
                return null;
            }

            string trimmed = email.Trim();
            string path = "/wp-json/wc/v3/customers?email="
                + Uri.EscapeDataString(trimmed)
                + "&per_page=5&role=all";
            try
            {
                JToken json = GetJson(creds, path);
                if (json == null || json.Type != JTokenType.Array)
                {
                    detail = "Unexpected customer response.";
                    return null;
                }
                var arr = (JArray)json;
                if (arr.Count == 0)
                {
                    detail = "No Woo customer with that email.";
                    return null;
                }
                WooCustomerDto customer = null;
                foreach (JToken item in arr)
                {
                    var parsed = ParseCustomer(item);
                    if (parsed == null)
                        continue;
                    if (customer == null
                        || string.Equals(parsed.Email, trimmed, StringComparison.OrdinalIgnoreCase))
                    {
                        customer = parsed;
                        break;
                    }
                }
                customer = customer ?? ParseCustomer(arr[0]);
                if (customer != null)
                    EnrichCustomerRoles(creds, customer, trimmed, rolesToProbe);
                return customer;
            }
            catch (Exception ex)
            {
                detail = ex.Message;
                return null;
            }
        }

        /// <summary>Sets the Woo customer role (e.g. member). Requires WordPress application password on shops using Assign Roles.</summary>
        public bool SetCustomerRole(ApiCredentials creds, long customerId, string role, string customerEmail,
            WordPressRoleCredentials wpRoleCreds, out string detail)
        {
            detail = null;
            if (creds == null || customerId <= 0 || string.IsNullOrWhiteSpace(role))
            {
                detail = "Customer id and role are required.";
                return false;
            }

            string roleSlug = role.Trim().ToLowerInvariant();
            string email = (customerEmail ?? string.Empty).Trim();

            if (TryGrantRoleViaTrackerEndpoint(creds, email, roleSlug, out detail))
            {
                if (VerifyCustomerHasRole(creds, email, customerId, roleSlug))
                    return true;
                detail = "Role API accepted the change but the shop still does not show " + role + ".";
                return false;
            }

            if (wpRoleCreds != null)
            {
                if (!TryGrantRoleViaWordPressAppPassword(wpRoleCreds, creds, email, customerId, roleSlug, out detail))
                    return false;
                if (VerifyCustomerHasRole(creds, email, customerId, roleSlug))
                    return true;
                detail = "WordPress accepted the role change but the shop still does not show "
                    + role + ". Check the role slug and that the login can edit users.";
                return false;
            }

            if (EnsureWordPressUserHasRole(creds, customerId, roleSlug, out detail)
                && VerifyCustomerHasRole(creds, email, customerId, roleSlug))
                return true;

            string path = "/wp-json/wc/v3/customers/" + customerId.ToString(CultureInfo.InvariantCulture);
            string body = new JObject { ["role"] = roleSlug }.ToString(Formatting.None);
            try
            {
                SendJson(creds, HttpMethod.Put, path, body);
            }
            catch (Exception ex)
            {
                detail = ex.Message;
                return false;
            }

            if (VerifyCustomerHasRole(creds, email, customerId, roleSlug))
                return true;

            detail = RoleGrantConfigHint;
            return false;
        }

        /// <summary>Removes a role via WordPress (keeps customer when stripping member).</summary>
        public bool RemoveCustomerRole(ApiCredentials creds, long customerId, string roleToRemove, string customerEmail,
            WordPressRoleCredentials wpRoleCreds, out string detail)
        {
            detail = null;
            if (creds == null || customerId <= 0 || string.IsNullOrWhiteSpace(roleToRemove))
            {
                detail = "Customer id and role are required.";
                return false;
            }

            string remove = roleToRemove.Trim().ToLowerInvariant();
            string email = (customerEmail ?? string.Empty).Trim();

            if (TryRemoveRoleViaTrackerEndpoint(creds, email, remove, out detail))
            {
                if (!VerifyCustomerHasRole(creds, email, customerId, remove))
                    return true;
                detail = "Role API accepted the change but the shop still shows " + remove + ".";
                return false;
            }

            if (wpRoleCreds != null)
            {
                if (!TryResolveWordPressUser(wpRoleCreds, creds, email, customerId, out long wpUserId, out IList<string> roles, out detail))
                    return false;

                roles = roles
                    .Where(r => !string.Equals(r, remove, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (!roles.Any(r => string.Equals(r, "customer", StringComparison.OrdinalIgnoreCase)))
                    roles.Add("customer");

                if (!SetWordPressUserRolesWithAppPassword(wpRoleCreds, wpUserId, roles, out detail))
                    return false;

                if (!VerifyCustomerHasRole(creds, email, customerId, remove))
                    return true;

                detail = "WordPress accepted the role change but the shop still shows " + remove + ".";
                return false;
            }

            var rolesFromWoo = TryGetWordPressUserRoles(creds, customerId, email);
            if (rolesFromWoo == null || rolesFromWoo.Count == 0)
            {
                detail = "Could not read WordPress roles for this user. " + RoleGrantConfigHint;
                return false;
            }

            rolesFromWoo = rolesFromWoo
                .Where(r => !string.Equals(r, remove, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (!rolesFromWoo.Any(r => string.Equals(r, "customer", StringComparison.OrdinalIgnoreCase)))
                rolesFromWoo.Add("customer");

            if (!SetWordPressUserRoles(creds, customerId, rolesFromWoo, out detail))
                return false;

            if (!VerifyCustomerHasRole(creds, email, customerId, remove))
                return true;

            detail = RoleGrantConfigHint;
            return false;
        }

        public static bool CustomerHasRole(WooCustomerDto customer, string targetRole)
        {
            if (customer == null)
                return false;
            if (customer.Roles != null && customer.Roles.Count > 0)
            {
                foreach (string role in customer.Roles)
                {
                    if (RoleIsMember(role, targetRole))
                        return true;
                }
                return false;
            }
            return RoleIsMember(customer.Role, targetRole);
        }

        public static bool RoleIsMember(string actualRole, string targetRole)
        {
            if (string.IsNullOrWhiteSpace(targetRole))
                return false;
            string target = targetRole.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(actualRole))
                return false;

            foreach (string part in actualRole.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string role = part.Trim().ToLowerInvariant();
                if (role == target)
                    return true;
            }
            return false;
        }

        private const string RoleGrantConfigHint =
            "Shop member role was not applied. Install the Tracker mu-plugin on the shop "
            + "(Documentation/WooIntegration/wordpress-mu-plugin-tracker-user-roles.php) "
            + "or configure WordPress application password under WooCommerce Mapping → Quaffee Lovers.";

        private bool VerifyCustomerHasRole(ApiCredentials creds, string email, long customerId, string roleSlug)
        {
            if (creds == null || customerId <= 0 || string.IsNullOrWhiteSpace(roleSlug))
                return false;
            return WooCustomerMatchesRoleFilter(creds, email, customerId, roleSlug.Trim().ToLowerInvariant());
        }

        private bool TryGrantRoleViaWordPressAppPassword(WordPressRoleCredentials wpCreds, ApiCredentials wooCreds,
            string email, long wooCustomerId, string roleSlug, out string detail)
        {
            detail = null;
            if (wpCreds == null)
            {
                detail = "WordPress role credentials are required.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                detail = "Email is required to resolve the WordPress user.";
                return false;
            }

            if (!TryResolveWordPressUser(wpCreds, wooCreds, email, wooCustomerId, out long wpUserId, out IList<string> roles, out detail))
                return false;

            if (roles.Any(r => string.Equals(r, roleSlug, StringComparison.OrdinalIgnoreCase)))
                return true;

            if (!roles.Any(r => string.Equals(r, "customer", StringComparison.OrdinalIgnoreCase)))
                roles.Add("customer");
            roles.Add(roleSlug);
            return SetWordPressUserRolesWithAppPassword(wpCreds, wpUserId, roles, out detail);
        }

        /// <summary>
        /// Resolves a WP user without listing all users (list_users is blocked on many shops).
        /// Tries direct id lookup (Woo customer id, username suffix) then email search.
        /// </summary>
        private bool TryResolveWordPressUser(WordPressRoleCredentials wpCreds, ApiCredentials wooCreds, string email,
            long wooCustomerId, out long userId, out IList<string> roles, out string detail)
        {
            userId = 0;
            roles = null;
            detail = null;
            if (wpCreds == null || string.IsNullOrWhiteSpace(email))
            {
                detail = "Email is required.";
                return false;
            }

            string wantEmail = email.Trim();
            var candidateIds = new List<long>();
            void AddId(long id)
            {
                if (id > 0 && !candidateIds.Contains(id))
                    candidateIds.Add(id);
            }

            if (wooCustomerId > 0)
                AddId(wooCustomerId);
            if (wooCreds != null && wooCustomerId > 0)
            {
                string username = TryGetWooCustomerUsername(wooCreds, wooCustomerId);
                long? fromUsername = TryParseUserIdFromWooUsername(username);
                if (fromUsername.HasValue)
                    AddId(fromUsername.Value);
            }

            foreach (long candidate in candidateIds)
            {
                if (!TryReadWordPressUserByIdWithAppPassword(wpCreds, candidate, wantEmail, out roles, out _))
                    continue;
                userId = candidate;
                AppLogger.WriteLog("woo", "WP user resolved by id " + candidate + " for " + wantEmail);
                return true;
            }

            if (TryResolveWordPressUserByEmailSearch(wpCreds, wantEmail, out userId, out roles, out detail))
                return true;

            detail = "Could not resolve WordPress user for " + wantEmail + " (tried ids "
                + (candidateIds.Count > 0
                    ? string.Join(", ", candidateIds)
                    : "none")
                + "). This shop blocks listing users — ensure the WordPress login and application password are saved, "
                + "or check that the Woo customer username suffix matches the WordPress user id.";
            return false;
        }

        private static long? TryParseUserIdFromWooUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;
            int dash = username.LastIndexOf('-');
            if (dash < 0 || dash >= username.Length - 1)
                return null;
            string tail = username.Substring(dash + 1);
            return long.TryParse(tail, NumberStyles.Integer, CultureInfo.InvariantCulture, out long id) && id > 0
                ? (long?)id
                : null;
        }

        private string TryGetWooCustomerUsername(ApiCredentials creds, long customerId)
        {
            if (creds == null || customerId <= 0)
                return null;
            try
            {
                string path = "/wp-json/wc/v3/customers/"
                    + customerId.ToString(CultureInfo.InvariantCulture);
                JToken json = GetJson(creds, path);
                return json?.Type == JTokenType.Object ? ((JObject)json).Value<string>("username") : null;
            }
            catch
            {
                return null;
            }
        }

        private bool TryReadWordPressUserByIdWithAppPassword(WordPressRoleCredentials wpCreds, long wpUserId,
            string expectEmail, out IList<string> roles, out string detail)
        {
            roles = null;
            detail = null;
            if (wpCreds == null || wpUserId <= 0)
            {
                detail = "WordPress user id is required.";
                return false;
            }

            string path = "/wp-json/wp/v2/users/" + wpUserId.ToString(CultureInfo.InvariantCulture) + "?context=edit";
            try
            {
                JToken json = SendJsonWithUserAppPassword(wpCreds, HttpMethod.Get, path, null);
                if (json == null || json.Type != JTokenType.Object)
                {
                    detail = "Unexpected WordPress user response.";
                    return false;
                }

                var obj = (JObject)json;
                string userEmail = obj.Value<string>("email");
                if (!string.IsNullOrWhiteSpace(expectEmail)
                    && !string.IsNullOrWhiteSpace(userEmail)
                    && !string.Equals(userEmail.Trim(), expectEmail.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    detail = "WordPress user " + wpUserId + " email mismatch.";
                    return false;
                }

                var roleArr = obj["roles"] as JArray;
                if (roleArr == null || roleArr.Count == 0)
                {
                    detail = "WordPress user has no roles.";
                    return false;
                }
                roles = roleArr
                    .Select(r => r?.ToString())
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .Select(r => r.Trim().ToLowerInvariant())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                return true;
            }
            catch (Exception ex)
            {
                detail = ex.Message;
                return false;
            }
        }

        private bool TryResolveWordPressUserByEmailSearch(WordPressRoleCredentials wpCreds, string email, out long userId,
            out IList<string> roles, out string detail)
        {
            userId = 0;
            roles = null;
            detail = null;
            if (wpCreds == null || string.IsNullOrWhiteSpace(email))
            {
                detail = "Email is required.";
                return false;
            }

            string path = "/wp-json/wp/v2/users?search="
                + Uri.EscapeDataString(email.Trim())
                + "&context=edit&per_page=20";
            try
            {
                JToken json = SendJsonWithUserAppPassword(wpCreds, HttpMethod.Get, path, null);
                if (json == null || json.Type != JTokenType.Array)
                {
                    detail = "Unexpected WordPress users response.";
                    return false;
                }

                string want = email.Trim();
                foreach (JToken item in (JArray)json)
                {
                    if (item?.Type != JTokenType.Object)
                        continue;
                    var obj = (JObject)item;
                    string userEmail = obj.Value<string>("email");
                    if (string.IsNullOrWhiteSpace(userEmail)
                        || !string.Equals(userEmail.Trim(), want, StringComparison.OrdinalIgnoreCase))
                        continue;

                    userId = obj.Value<long?>("id") ?? 0;
                    if (userId <= 0)
                        continue;

                    var roleArr = obj["roles"] as JArray;
                    if (roleArr == null || roleArr.Count == 0)
                    {
                        detail = "WordPress user has no roles.";
                        return false;
                    }
                    roles = roleArr
                        .Select(r => r?.ToString())
                        .Where(r => !string.IsNullOrWhiteSpace(r))
                        .Select(r => r.Trim().ToLowerInvariant())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    AppLogger.WriteLog("woo", "WP user resolved by email search for " + want + " -> id " + userId);
                    return true;
                }

                detail = "No WordPress user with email " + want + ".";
                return false;
            }
            catch (Exception ex)
            {
                detail = ex.Message;
                AppLogger.WriteLog("woo", "WP user search for " + email + ": " + ex.Message);
                return false;
            }
        }

        private bool SetWordPressUserRolesWithAppPassword(WordPressRoleCredentials wpCreds, long userId, IList<string> roles,
            out string detail)
        {
            detail = null;
            if (wpCreds == null || userId <= 0)
            {
                detail = "WordPress user id is required.";
                return false;
            }
            if (roles == null || roles.Count == 0)
            {
                detail = "At least one role is required.";
                return false;
            }

            string path = "/wp-json/wp/v2/users/" + userId.ToString(CultureInfo.InvariantCulture);
            var body = new JObject
            {
                ["roles"] = new JArray(roles.Distinct(StringComparer.OrdinalIgnoreCase).Select(r => r.Trim().ToLowerInvariant()))
            };
            try
            {
                SendJsonWithUserAppPassword(wpCreds, HttpMethod.Put, path, body.ToString(Formatting.None));
                AppLogger.WriteLog("woo", "WP roles set for user " + userId + ": " + string.Join(", ", roles));
                return true;
            }
            catch (Exception ex)
            {
                detail = ex.Message;
                AppLogger.WriteLog("woo", "WP roles set failed for user " + userId + ": " + ex.Message);
                return false;
            }
        }

        private const string WordPressNotLoggedInHint =
            "WordPress did not accept the login. Use an Application Password (Users → trackeradmin → Profile → Application Passwords), "
            + "not your normal admin password; paste it without spaces; username must be the login name (e.g. trackeradmin). "
            + "Wordfence often disables application passwords site-wide: Wordfence → All Options → Brute Force Protection → "
            + "uncheck “Disable WordPress application passwords”, then create a new app password. "
            + "If the Profile page never shows Application Passwords, that setting is still blocking them. "
            + "Otherwise the server may be dropping the Authorization header (Apache/LiteSpeed — pass HTTP_AUTHORIZATION to PHP).";

        private JToken SendJsonWithUserAppPassword(WordPressRoleCredentials wpCreds, HttpMethod method, string pathAndQuery,
            string jsonBody)
        {
            if (wpCreds == null)
                throw new ArgumentNullException(nameof(wpCreds));
            string baseUrl = ResolveCanonicalStoreBaseUrl(wpCreds.StoreBaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
                throw new InvalidOperationException("Store URL is required.");
            if (string.IsNullOrWhiteSpace(wpCreds.Username) || string.IsNullOrWhiteSpace(wpCreds.ApplicationPassword))
                throw new InvalidOperationException("WordPress username and application password are required.");

            string user = wpCreds.Username.Trim();
            string pass = NormalizeApplicationPassword(wpCreds.ApplicationPassword);
            Exception last = null;

            try
            {
                return SendWordPressJsonRequest(baseUrl, pathAndQuery, method, jsonBody, user, pass);
            }
            catch (Exception ex)
            {
                last = ex;
            }

            try
            {
                return SendWordPressJsonWithCredentialHandler(baseUrl, pathAndQuery, method, jsonBody, user, pass);
            }
            catch (Exception ex)
            {
                last = ex;
            }

            string msg = last?.Message ?? "WordPress request failed.";
            if (msg.IndexOf("rest_not_logged_in", StringComparison.OrdinalIgnoreCase) >= 0)
                msg += " " + WordPressNotLoggedInHint;
            throw new InvalidOperationException(msg, last);
        }

        private static JToken SendWordPressJsonRequest(string baseUrl, string pathAndQuery, HttpMethod method,
            string jsonBody, string user, string pass)
        {
            string authValue = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(user + ":" + pass));
            string url = baseUrl.TrimEnd('/') + pathAndQuery;
            const int maxRedirects = 6;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            for (int redirect = 0; redirect <= maxRedirects; redirect++)
            {
                using (var handler = new HttpClientHandler { AllowAutoRedirect = false })
                using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(60) })
                {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.ParseAdd("TrackerSQL-WooClient/3.0");

                using (var request = new HttpRequestMessage(method, url))
                {
                request.Headers.TryAddWithoutValidation("Authorization", authValue);
                if (jsonBody != null)
                    request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                using (HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult())
                {
                if ((int)response.StatusCode >= 300 && (int)response.StatusCode < 400 && redirect < maxRedirects)
                {
                    Uri next = response.Headers.Location;
                    if (next == null)
                        break;
                    if (!next.IsAbsoluteUri)
                        next = new Uri(new Uri(url), next);
                    url = next.ToString();
                    AppLogger.WriteLog("woo", "WP auth redirect → " + url);
                    continue;
                }

                string body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException(
                        "HTTP " + (int)response.StatusCode + " — " + FormatErrorBody(body));
                if (string.IsNullOrWhiteSpace(body))
                    return new JObject();
                return JToken.Parse(body);
                }
                }
                }
            }

            throw new InvalidOperationException("Too many redirects reaching WordPress.");
        }

        private static JToken SendWordPressJsonWithCredentialHandler(string baseUrl, string pathAndQuery,
            HttpMethod method, string jsonBody, string user, string pass)
        {
            string url = baseUrl.TrimEnd('/') + pathAndQuery;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            using (var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                Credentials = new NetworkCredential(user, pass),
                PreAuthenticate = true
            })
            using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(60) })
            {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.ParseAdd("TrackerSQL-WooClient/3.0");

            using (var request = new HttpRequestMessage(method, url))
            {
            if (jsonBody != null)
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            using (HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult())
            {
            string body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    "HTTP " + (int)response.StatusCode + " — " + FormatErrorBody(body));
            if (string.IsNullOrWhiteSpace(body))
                return new JObject();
            return JToken.Parse(body);
            }
            }
            }
        }

        private void EnrichCustomerRoles(ApiCredentials creds, WooCustomerDto customer, string email,
            IEnumerable<string> rolesToProbe)
        {
            if (customer == null || customer.Id <= 0)
                return;

            var merged = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(customer.Role))
                merged.Add(customer.Role.Trim().ToLowerInvariant());

            if (rolesToProbe != null)
            {
                foreach (string probe in rolesToProbe.Where(r => !string.IsNullOrWhiteSpace(r)).Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    string slug = probe.Trim().ToLowerInvariant();
                    if (merged.Contains(slug))
                        continue;
                    if (WooCustomerMatchesRoleFilter(creds, email, customer.Id, slug))
                        merged.Add(slug);
                }
            }

            customer.Roles = merged.ToList();
            AppLogger.WriteLog("woo",
                "Shop roles for customer " + customer.Id + " (" + (email ?? customer.Email) + "): "
                + customer.RolesDisplay);
        }

        /// <summary>Woo returns the customer when role=member if member is among their WordPress roles.</summary>
        private bool WooCustomerMatchesRoleFilter(ApiCredentials creds, string email, long customerId, string roleSlug)
        {
            if (creds == null || customerId <= 0 || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(roleSlug))
                return false;
            try
            {
                string path = "/wp-json/wc/v3/customers?email="
                    + Uri.EscapeDataString(email.Trim())
                    + "&role=" + Uri.EscapeDataString(roleSlug.Trim().ToLowerInvariant())
                    + "&per_page=10";
                JToken json = GetJson(creds, path);
                if (json == null || json.Type != JTokenType.Array)
                    return false;
                foreach (JToken item in (JArray)json)
                {
                    var parsed = ParseCustomer(item);
                    if (parsed != null && parsed.Id == customerId)
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog("woo", "Role probe " + roleSlug + " for customer " + customerId + ": " + ex.Message);
                return false;
            }
        }

        private IList<string> TryGetWordPressUserRoles(ApiCredentials creds, long userId, string email)
        {
            if (creds == null || userId <= 0)
                return null;

            IList<string> byId = ReadWordPressUserRoles(creds, "/wp-json/wp/v2/users/"
                + userId.ToString(CultureInfo.InvariantCulture) + "?context=edit");
            if (byId != null && byId.Count > 0)
                return byId;

            if (!string.IsNullOrWhiteSpace(email))
            {
                IList<string> bySearch = ReadWordPressUserRoles(creds, "/wp-json/wp/v2/users?search="
                    + Uri.EscapeDataString(email.Trim()) + "&context=edit&per_page=10");
                if (bySearch != null && bySearch.Count > 0)
                    return bySearch;
            }

            return null;
        }

        private IList<string> ReadWordPressUserRoles(ApiCredentials creds, string path)
        {
            try
            {
                JToken json = GetJson(creds, path);
                if (json == null)
                    return null;

                JArray roles = null;
                if (json.Type == JTokenType.Object)
                    roles = ((JObject)json)["roles"] as JArray;
                else if (json.Type == JTokenType.Array)
                {
                    foreach (JToken item in (JArray)json)
                    {
                        if (item?.Type != JTokenType.Object)
                            continue;
                        roles = ((JObject)item)["roles"] as JArray;
                        if (roles != null && roles.Count > 0)
                            break;
                    }
                }

                if (roles == null || roles.Count == 0)
                    return null;
                return roles
                    .Select(r => r?.ToString())
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .Select(r => r.Trim().ToLowerInvariant())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog("woo", "WP users roles " + path + ": " + ex.Message);
                return null;
            }
        }

        private bool EnsureWordPressUserHasRole(ApiCredentials creds, long userId, string role, out string detail)
        {
            detail = null;
            var roles = TryGetWordPressUserRoles(creds, userId, null);
            if (roles == null)
                return false;

            if (roles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase)))
                return true;

            if (!roles.Any(r => string.Equals(r, "customer", StringComparison.OrdinalIgnoreCase)))
                roles.Add("customer");
            roles.Add(role);
            return SetWordPressUserRoles(creds, userId, roles, out detail);
        }

        private bool SetWordPressUserRoles(ApiCredentials creds, long userId, IList<string> roles, out string detail)
        {
            detail = null;
            if (roles == null || roles.Count == 0)
            {
                detail = "At least one role is required.";
                return false;
            }

            string path = "/wp-json/wp/v2/users/" + userId.ToString(CultureInfo.InvariantCulture);
            var body = new JObject
            {
                ["roles"] = new JArray(roles.Distinct(StringComparer.OrdinalIgnoreCase).Select(r => r.Trim().ToLowerInvariant()))
            };
            try
            {
                SendJson(creds, HttpMethod.Put, path, body.ToString(Formatting.None));
                return true;
            }
            catch (Exception ex)
            {
                detail = ex.Message;
                return false;
            }
        }

        private const string TrackerRolePluginHint =
            "Install Documentation/WooIntegration/wordpress-mu-plugin-tracker-user-roles.php "
            + "into wp-content/mu-plugins/ on the shop (uses your existing Woo API keys).";

        /// <summary>GET /wc-tracker/v1/ping — same auth as WooCommerce REST keys.</summary>
        public WordPressRoleTestResult TestTrackerRoleEndpoint(ApiCredentials creds)
        {
            var result = new WordPressRoleTestResult();
            if (creds == null)
            {
                result.Detail = "Woo API credentials are required.";
                return result;
            }

            try
            {
                JToken json = GetJson(creds, "/wp-json/wc-tracker/v1/ping");
                if (json == null || json.Type != JTokenType.Object)
                {
                    result.Detail = "Unexpected response from role grant API.";
                    return result;
                }

                var obj = (JObject)json;
                if (obj.Value<bool?>("ok") != true)
                {
                    result.Detail = "Role grant API returned not OK.";
                    return result;
                }

                result.Succeeded = true;
                result.UserId = obj.Value<long?>("user_id");
                result.UserName = obj.Value<string>("login");
                var roles = obj["roles"] as JArray;
                if (roles != null && roles.Count > 0)
                    result.Roles = string.Join(", ", roles.Select(r => r?.ToString()).Where(r => !string.IsNullOrWhiteSpace(r)));
                result.Detail = string.Format(CultureInfo.InvariantCulture,
                    "Role grant API OK — Woo keys authenticate as {0} (#{1}). Roles: {2}.",
                    result.UserName ?? "(unknown)",
                    result.UserId,
                    string.IsNullOrWhiteSpace(result.Roles) ? "(none)" : result.Roles);
            }
            catch (Exception ex)
            {
                result.Succeeded = false;
                if (IsTrackerEndpointNotInstalled(ex.Message))
                    result.Detail = "Role grant API not installed on the shop. " + TrackerRolePluginHint;
                else
                    result.Detail = ex.Message;
            }

            return result;
        }

        private bool TryGrantRoleViaTrackerEndpoint(ApiCredentials creds, string email, string roleSlug, out string detail)
        {
            detail = null;
            if (creds == null || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(roleSlug))
            {
                detail = "Email and role are required.";
                return false;
            }

            string body = new JObject
            {
                ["email"] = email.Trim(),
                ["add_role"] = roleSlug.Trim().ToLowerInvariant()
            }.ToString(Formatting.None);

            try
            {
                JToken json = SendJson(creds, HttpMethod.Put, "/wp-json/wc-tracker/v1/users/roles", body);
                string roles = json?["roles"] != null ? string.Join(", ", json["roles"]) : null;
                AppLogger.WriteLog("woo", "Tracker role API granted " + roleSlug + " for " + email
                    + (string.IsNullOrWhiteSpace(roles) ? string.Empty : " → " + roles));
                return true;
            }
            catch (Exception ex)
            {
                detail = IsTrackerEndpointNotInstalled(ex.Message)
                    ? TrackerRolePluginHint
                    : ex.Message;
                return false;
            }
        }

        private bool TryRemoveRoleViaTrackerEndpoint(ApiCredentials creds, string email, string roleSlug, out string detail)
        {
            detail = null;
            if (creds == null || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(roleSlug))
            {
                detail = "Email and role are required.";
                return false;
            }

            string body = new JObject
            {
                ["email"] = email.Trim(),
                ["remove_role"] = roleSlug.Trim().ToLowerInvariant()
            }.ToString(Formatting.None);

            try
            {
                SendJson(creds, HttpMethod.Put, "/wp-json/wc-tracker/v1/users/roles", body);
                AppLogger.WriteLog("woo", "Tracker role API removed " + roleSlug + " for " + email);
                return true;
            }
            catch (Exception ex)
            {
                detail = IsTrackerEndpointNotInstalled(ex.Message)
                    ? TrackerRolePluginHint
                    : ex.Message;
                return false;
            }
        }

        private static bool IsTrackerEndpointNotInstalled(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return false;
            return message.IndexOf("rest_no_route", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("No route was found", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("HTTP 404", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>Confirms WordPress application-password login (GET /users/me).</summary>
        public WordPressRoleTestResult TestWordPressRoleConnection(WordPressRoleCredentials creds)
        {
            var result = new WordPressRoleTestResult();
            if (creds == null || string.IsNullOrWhiteSpace(creds.Username)
                || string.IsNullOrWhiteSpace(creds.ApplicationPassword))
            {
                result.Detail = "WordPress username and application password are required.";
                return result;
            }

            try
            {
                JToken json = SendJsonWithUserAppPassword(creds, HttpMethod.Get,
                    "/wp-json/wp/v2/users/me?context=edit", null);
                if (json == null || json.Type != JTokenType.Object)
                {
                    result.Detail = "Unexpected WordPress response.";
                    return result;
                }

                var obj = (JObject)json;
                result.UserId = obj.Value<long?>("id");
                result.UserName = obj.Value<string>("name");
                string slug = obj.Value<string>("slug");
                var roleArr = obj["roles"] as JArray;
                if (roleArr != null && roleArr.Count > 0)
                    result.Roles = string.Join(", ", roleArr.Select(r => r?.ToString()).Where(r => !string.IsNullOrWhiteSpace(r)));

                bool likelyCanGrant = roleArr != null && roleArr.Any(r =>
                {
                    string role = (r?.ToString() ?? string.Empty).Trim().ToLowerInvariant();
                    return role == "administrator" || role == "shop_manager";
                });

                result.Succeeded = true;
                result.Detail = string.Format(CultureInfo.InvariantCulture,
                    "WordPress login OK — {0} (#{1}, @{2}). Roles: {3}. {4}",
                    result.UserName ?? creds.Username.Trim(),
                    result.UserId,
                    slug ?? creds.Username.Trim(),
                    string.IsNullOrWhiteSpace(result.Roles) ? "(none)" : result.Roles,
                    likelyCanGrant
                        ? "This account should be able to grant member roles."
                        : "Warning: this role may not be able to edit other users.");
            }
            catch (Exception ex)
            {
                result.Succeeded = false;
                result.Detail = ex.Message;
            }

            return result;
        }

        private static WooCustomerDto ParseCustomer(JToken token)
        {
            if (token == null || token.Type != JTokenType.Object)
                return null;
            var obj = (JObject)token;
            long id = obj.Value<long?>("id") ?? 0;
            if (id <= 0)
                return null;
            return new WooCustomerDto
            {
                Id = id,
                Email = obj.Value<string>("email"),
                Role = obj.Value<string>("role"),
                FirstName = obj.Value<string>("first_name"),
                LastName = obj.Value<string>("last_name")
            };
        }
    }
}
