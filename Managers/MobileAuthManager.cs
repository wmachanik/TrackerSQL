using System;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Mobile app sign-in: staff sign in once with their normal Tracker login and get a device token
    /// (only its SHA-256 hash is stored). Drivers are the users linked to a person in Lookups → People.
    /// </summary>
    public class MobileAuthManager
    {
        private static readonly TimeSpan TouchInterval = TimeSpan.FromMinutes(10);
        private readonly MobileApiRepository _repo = new MobileApiRepository();

        public static int TokenDays => ReadInt("MobileApi.TokenDays", 90, 1, 3650);

        /// <summary>When true, a synced "Delivered" runs the same Order Done steps as the web Done button (System → Driver App).</summary>
        public static bool AutoCompleteDeliveries => MobileApiSettingsManager.Current.RunDoneOnDelivery;

        public static bool RequireHttps => ReadBool("MobileApi.RequireHttps", true);

        /// <summary>Days to keep the API request log; 0 turns request logging off.</summary>
        public static int LogDays => ReadInt("MobileApi.LogDays", 30, 0, 365);

        /// <param name="ip">Caller's IP address; too many failed sign-ins from one address are refused for a while.</param>
        /// <param name="throttled">True when refused because of too many failed attempts (reply 429).</param>
        public MobileLoginResponse Login(MobileLoginRequest request, string ip, out string error, out bool throttled)
        {
            error = null;
            throttled = false;
            MobileApiSchemaInstaller.EnsureReady();

            if (MobileApiSecurity.IsLoginBlocked(ip, out TimeSpan retryAfter))
            {
                throttled = true;
                error = "Too many failed sign-ins from this network. Try again in "
                    + Math.Max(1, (int)Math.Ceiling(retryAfter.TotalMinutes)) + " minutes.";
                return null;
            }

            string typed = request?.UserName?.Trim();
            if (string.IsNullOrEmpty(typed) || string.IsNullOrEmpty(request.Password))
            {
                error = "Username and password are required.";
                return null;
            }

            string userName = SignInNameResolver.Resolve(typed);
            if (!Membership.ValidateUser(userName, request.Password))
            {
                MobileApiSecurity.RecordLoginFailure(ip);
                var user = Membership.GetUser(userName);
                error = user != null && user.IsLockedOut
                    ? "This login is locked after too many failed attempts. Ask an administrator to unlock it."
                    : "Incorrect username or password.";
                AppLogger.WriteLog(SystemConstants.LogTypes.Login, "Mobile API sign-in failed for '" + typed + "' from " + ip + ".", typed);
                return null;
            }
            MobileApiSecurity.ClearLoginFailures(ip);

            var membershipUser = Membership.GetUser(userName);
            if (membershipUser != null)
                userName = membershipUser.UserName;

            bool isAdmin = SecurityManager.IsAdminUser(userName);
            int? personId = new PersonsRepository().GetPersonIdBySecurityUsername(userName);
            if (!personId.HasValue && !isAdmin)
            {
                error = "Your login is not linked to a delivery person. Ask an administrator to link it in Lookups (People).";
                AppLogger.WriteLog(SystemConstants.LogTypes.Login, "Mobile API sign-in refused (not linked to a person).", userName);
                return null;
            }

            return IssueToken(userName, personId, request.DeviceId, request.DeviceName, request.AppVersion);
        }

        /// <summary>Token for a staff member already signed in to the web app (API Tester page); no password needed.</summary>
        public MobileLoginResponse IssueTokenForWebUser(string userName, string deviceName)
        {
            MobileApiSchemaInstaller.EnsureReady();
            int? personId = new PersonsRepository().GetPersonIdBySecurityUsername(userName);
            return IssueToken(userName, personId, "web-" + userName, deviceName, "web");
        }

        private MobileLoginResponse IssueToken(string userName, int? personId, string deviceId, string deviceName, string appVersion)
        {
            string token = NewToken();
            DateTime expires = TimeZoneUtils.Now().AddDays(TokenDays);
            _repo.RevokeDeviceTokens(userName, deviceId, userName + " (signed in again)");
            _repo.InsertToken(Hash(token), userName, personId, deviceId, deviceName, appVersion, expires);

            AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                "Mobile API sign-in on device '" + (deviceName ?? deviceId ?? "unknown") + "'.", userName);

            return new MobileLoginResponse
            {
                Token = token,
                ExpiresAt = expires.ToString("yyyy-MM-ddTHH:mm:ss"),
                User = BuildUserInfo(userName, personId)
            };
        }

        /// <summary>
        /// The token's user, or null with <paramref name="problem"/> saying why: unknown, expired or revoked token,
        /// not used for TokenIdleDays, or the login was deactivated, locked or unlinked from its delivery person.
        /// </summary>
        public MobileTokenUser Validate(string token, out string problem)
        {
            problem = "Sign in again: the token is missing, expired or revoked.";
            if (string.IsNullOrWhiteSpace(token) || token.Length > 200)
                return null;

            MobileApiSchemaInstaller.EnsureReady();
            var row = _repo.FindActiveToken(Hash(token.Trim()));
            if (row == null)
                return null;

            DateTime now = TimeZoneUtils.Now();
            int idleDays = MobileApiSecurity.TokenIdleDays;
            if (idleDays > 0 && now - (row.LastUsedAt ?? row.CreatedAt) > TimeSpan.FromDays(idleDays))
            {
                _repo.RevokeToken(row.TokenID, "not used for " + idleDays + " days");
                AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                    "Mobile API token for '" + (row.DeviceName ?? row.DeviceId) + "' expired after " + idleDays + " days unused.", row.UserName);
                problem = "Sign in again: this phone has not been used for " + idleDays + " days.";
                return null;
            }

            string account = MobileApiSecurity.AccountProblem(row.TokenID, row.UserName);
            if (account != null)
            {
                problem = account;
                return null;
            }

            if (!row.LastUsedAt.HasValue || now - row.LastUsedAt.Value > TouchInterval)
                _repo.TouchToken(row.TokenID);

            problem = null;
            return new MobileTokenUser { TokenId = row.TokenID, UserName = row.UserName, PersonId = row.PersonID };
        }

        public void Logout(MobileTokenUser user)
        {
            if (user == null)
                return;
            _repo.RevokeToken(user.TokenId, user.UserName);
            MobileApiSecurity.Forget(user.TokenId);
            AppLogger.WriteLog(SystemConstants.LogTypes.Login, "Mobile API sign-out.", user.UserName);
        }

        public bool RevokeToken(long tokenId, string revokedBy)
        {
            MobileApiSecurity.Forget(tokenId);
            return _repo.RevokeToken(tokenId, revokedBy) > 0;
        }

        public MobileUserInfo BuildUserInfo(string userName, int? personId)
        {
            string[] roles;
            try { roles = Roles.GetRolesForUser(userName); }
            catch { roles = new string[0]; }

            return new MobileUserInfo
            {
                UserName = userName,
                PersonId = personId,
                PersonName = personId.HasValue ? new PersonsRepository().GetPersonNameById(personId.Value) : null,
                IsAdmin = SecurityManager.IsAdminUser(userName),
                Roles = roles.ToList(),
                AutoCompleteDeliveries = AutoCompleteDeliveries
            };
        }

        private static string NewToken()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static string Hash(string token)
        {
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
                var sb = new StringBuilder(64);
                foreach (byte b in hash)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private static int ReadInt(string key, int fallback, int min, int max)
        {
            return int.TryParse(ConfigurationManager.AppSettings[key], out int value)
                ? Math.Max(min, Math.Min(max, value))
                : fallback;
        }

        private static bool ReadBool(string key, bool fallback)
        {
            return bool.TryParse(ConfigurationManager.AppSettings[key], out bool value) ? value : fallback;
        }
    }
}
