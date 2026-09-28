using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Security;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// In-memory protection for the mobile API: sign-in attempts per IP address, requests per device token,
    /// and a short-lived check that the token's login is still active (approved, not locked, still a driver).
    /// Counters reset when the site restarts, which is acceptable for throttling.
    /// </summary>
    public static class MobileApiSecurity
    {
        private static readonly TimeSpan LoginWindow = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan AccountCheckFor = TimeSpan.FromMinutes(5);

        private static readonly ConcurrentDictionary<string, Queue<DateTime>> LoginFailures =
            new ConcurrentDictionary<string, Queue<DateTime>>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<long, Queue<DateTime>> Requests = new ConcurrentDictionary<long, Queue<DateTime>>();
        private static readonly ConcurrentDictionary<long, AccountCheck> Accounts = new ConcurrentDictionary<long, AccountCheck>();

        private sealed class AccountCheck
        {
            public DateTime CheckedAt;
            public string Problem;
        }

        /// <summary>Failed sign-ins allowed per IP address in 15 minutes (MobileApi.LoginFailuresPerIp, default 10).</summary>
        public static int LoginFailuresPerIp => ReadInt("MobileApi.LoginFailuresPerIp", 10, 3, 1000);

        /// <summary>Calls allowed per device per minute (MobileApi.RequestsPerMinute, default 120; 0 = no limit).</summary>
        public static int RequestsPerMinute => ReadInt("MobileApi.RequestsPerMinute", 120, 0, 10000);

        /// <summary>A token not used for this many days stops working (MobileApi.TokenIdleDays, default 30; 0 = never).</summary>
        public static int TokenIdleDays => ReadInt("MobileApi.TokenIdleDays", 30, 0, 3650);

        /// <summary>Trust X-Forwarded-Proto for the HTTPS check (only behind a proxy that sets it; MobileApi.TrustForwardedProto).</summary>
        public static bool TrustForwardedProto =>
            bool.TryParse(ConfigurationManager.AppSettings["MobileApi.TrustForwardedProto"], out bool value) && value;

        public static bool IsLoginBlocked(string ip, out TimeSpan retryAfter)
        {
            retryAfter = TimeSpan.Zero;
            if (string.IsNullOrEmpty(ip) || !LoginFailures.TryGetValue(ip, out var queue))
                return false;

            lock (queue)
            {
                Trim(queue, DateTime.UtcNow - LoginWindow);
                if (queue.Count < LoginFailuresPerIp)
                    return false;
                retryAfter = queue.Peek() + LoginWindow - DateTime.UtcNow;
                return true;
            }
        }

        public static void RecordLoginFailure(string ip)
        {
            if (string.IsNullOrEmpty(ip))
                return;
            if (LoginFailures.Count > 5000)
                SweepLoginFailures();
            var queue = LoginFailures.GetOrAdd(ip, _ => new Queue<DateTime>());
            lock (queue)
            {
                Trim(queue, DateTime.UtcNow - LoginWindow);
                queue.Enqueue(DateTime.UtcNow);
            }
        }

        public static void ClearLoginFailures(string ip)
        {
            if (!string.IsNullOrEmpty(ip))
                LoginFailures.TryRemove(ip, out _);
        }

        /// <summary>False when the device has made more than RequestsPerMinute calls in the last minute.</summary>
        public static bool AllowRequest(long tokenId, out TimeSpan retryAfter)
        {
            retryAfter = TimeSpan.Zero;
            int limit = RequestsPerMinute;
            if (limit <= 0)
                return true;

            var queue = Requests.GetOrAdd(tokenId, _ => new Queue<DateTime>());
            lock (queue)
            {
                DateTime now = DateTime.UtcNow;
                Trim(queue, now.AddMinutes(-1));
                if (queue.Count >= limit)
                {
                    retryAfter = queue.Peek().AddMinutes(1) - now;
                    return false;
                }
                queue.Enqueue(now);
                return true;
            }
        }

        /// <summary>
        /// Null when the login behind the token may still use the app; otherwise why not (deleted, not approved,
        /// locked out, or no longer linked to a delivery person). Checked at most every 5 minutes per token.
        /// </summary>
        public static string AccountProblem(long tokenId, string userName)
        {
            if (Accounts.TryGetValue(tokenId, out var cached) && DateTime.UtcNow - cached.CheckedAt < AccountCheckFor)
                return cached.Problem;

            string problem = CheckAccount(userName);
            Accounts[tokenId] = new AccountCheck { CheckedAt = DateTime.UtcNow, Problem = problem };
            return problem;
        }

        /// <summary>Forget cached results after a sign-out or revoke so the next call is checked again.</summary>
        public static void Forget(long tokenId)
        {
            Accounts.TryRemove(tokenId, out _);
            Requests.TryRemove(tokenId, out _);
        }

        private static string CheckAccount(string userName)
        {
            MembershipUser user;
            try { user = Membership.GetUser(userName, false); }
            catch { return null; }

            if (user == null)
                return "This login no longer exists.";
            if (!user.IsApproved)
                return "This login has been deactivated.";
            if (user.IsLockedOut)
                return "This login is locked. Ask an administrator to unlock it.";
            if (!SecurityManager.IsAdminUser(userName) && !new PersonsRepository().GetPersonIdBySecurityUsername(userName).HasValue)
                return "This login is no longer linked to a delivery person.";
            return null;
        }

        private static void SweepLoginFailures()
        {
            DateTime cutoff = DateTime.UtcNow - LoginWindow;
            foreach (var entry in LoginFailures)
            {
                bool stale;
                lock (entry.Value)
                {
                    Trim(entry.Value, cutoff);
                    stale = entry.Value.Count == 0;
                }
                if (stale)
                    LoginFailures.TryRemove(entry.Key, out _);
            }
        }

        private static void Trim(Queue<DateTime> queue, DateTime olderThan)
        {
            while (queue.Count > 0 && queue.Peek() < olderThan)
                queue.Dequeue();
        }

        private static int ReadInt(string key, int fallback, int min, int max)
        {
            return int.TryParse(ConfigurationManager.AppSettings[key], out int value)
                ? Math.Max(min, Math.Min(max, value))
                : fallback;
        }
    }
}
