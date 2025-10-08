using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using TrackerDotNet.Classes;

namespace TrackerDotNet.Managers
{
    public class DisableClientManager
    {
        private const int MIN_SECRET_LENGTH = 32;
        private const int TOKEN_VALIDITY_HOURS = 24;

        /// <summary>
        /// Generates a secure link for disabling a client
        /// </summary>
        public static string GenerateDisableLink(long customerId)
        {
            ValidateSecret();
            string token = GenerateToken(customerId);
            string baseUrl = GetApplicationUrl();
            return $"{baseUrl}/DisableClient.aspx?{SystemConstants.UrlParameterConstants.CustomerID}={customerId}&token={token}";
        }

        /// <summary>
        /// Validates a disable request token
        /// </summary>
        public static bool ValidateToken(string customerId, string token)
        {
            if (string.IsNullOrEmpty(customerId) || string.IsNullOrEmpty(token))
                return false;

            string expectedToken = GenerateToken(long.Parse(customerId));
            return token.Equals(expectedToken, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Generates a new secure secret key for the application
        /// </summary>
        public static string GenerateNewSecret()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[48]; // 384 bits
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes);
            }
        }

        private static string GenerateToken(long customerId)
        {
            string secret = ConfigurationManager.AppSettings["DisableClientSecret"];
            string dateString = TimeZoneUtils.Now().Date.ToString("yyyyMMdd");
            string input = $"{customerId}:{dateString}:{secret}";

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        private static void ValidateSecret()
        {
            string secret = ConfigurationManager.AppSettings["DisableClientSecret"];
            if (string.IsNullOrEmpty(secret) || secret.Length < MIN_SECRET_LENGTH)
            {
                AppLogger.WriteLog("security", MessageProvider.Get(MessageKeys.Security.InvalidSecret));
                throw new ConfigurationErrorsException(MessageProvider.Get(MessageKeys.Security.InvalidSecretConfig));
            }
        }

        public static string GetApplicationUrl()
        {
            string baseUrl = ConfigHelper.GetString("ApplicationBaseUrl","");
            
            if (string.IsNullOrEmpty(baseUrl))
            {
                var context = System.Web.HttpContext.Current;
                if (context != null)
                {
                    var request = context.Request;
                    string appPath = request.ApplicationPath;
                    if (appPath == "/") appPath = "";
                    
                    baseUrl = string.Format("{0}://{1}{2}",
                        request.Url.Scheme,
                        request.Url.Authority,
                        appPath);
                }
                else
                {
                    AppLogger.WriteLog("error", MessageProvider.Get(MessageKeys.Security.NoHttpContext));
                    throw new InvalidOperationException(MessageProvider.Get(MessageKeys.Security.NoHttpContext));
                }
            }

            return baseUrl.TrimEnd('/');
        }
    }
}