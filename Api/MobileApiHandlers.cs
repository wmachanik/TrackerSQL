using System;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Security;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;

namespace TrackerSQL.Api
{
    /// <summary>Resolves "Authorization: Bearer &lt;token&gt;" to the signed-in staff user. The forms cookie is ignored.</summary>
    public class MobileApiAuthHandler : DelegatingHandler
    {
        public const string UserKey = "MobileApi.User";
        public const string AuthProblemKey = "MobileApi.AuthProblem";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = HttpContext.Current;
            if (context != null)
            {
                // Return 401 JSON instead of a redirect to the sign-in page.
                context.Response.SuppressFormsAuthenticationRedirect = true;
                context.Response.TrySkipIisCustomErrors = true;
            }

            if (MobileAuthManager.RequireHttps && !IsSecure(request))
                return MobileApiController.ErrorResponse(request, HttpStatusCode.Forbidden, "HTTPS is required.");

            var auth = request.Headers.Authorization;
            if (auth != null && string.Equals(auth.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase))
            {
                MobileTokenUser user = new MobileAuthManager().Validate(auth.Parameter, out string problem);
                if (user == null)
                    request.Properties[AuthProblemKey] = problem;
                else
                {
                    if (!MobileApiSecurity.AllowRequest(user.TokenId, out TimeSpan retryAfter))
                    {
                        request.Properties[UserKey] = user;
                        var tooMany = MobileApiController.ErrorResponse(request, (HttpStatusCode)429,
                            "Too many requests from this phone. Try again in a minute.");
                        tooMany.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(Math.Max(1, Math.Ceiling(retryAfter.TotalSeconds))));
                        return tooMany;
                    }

                    request.Properties[UserKey] = user;

                    string[] roles;
                    try { roles = Roles.GetRolesForUser(user.UserName); }
                    catch { roles = new string[0]; }

                    var principal = new GenericPrincipal(new GenericIdentity(user.UserName, "MobileToken"), roles);
                    request.GetRequestContext().Principal = principal;
                    Thread.CurrentPrincipal = principal;
                    if (context != null)
                        context.User = principal;
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private static bool IsSecure(HttpRequestMessage request)
        {
            if (request.RequestUri.Scheme == Uri.UriSchemeHttps || request.IsLocal())
                return true;
            return MobileApiSecurity.TrustForwardedProto
                && request.Headers.TryGetValues("X-Forwarded-Proto", out var values)
                && values.Any(v => string.Equals(v, "https", StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>Requires a valid device token (and optionally an administrator).</summary>
    public class MobileAuthorizeAttribute : AuthorizationFilterAttribute
    {
        public bool AdminOnly { get; set; }

        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            var request = actionContext.Request;
            if (!request.Properties.TryGetValue(MobileApiAuthHandler.UserKey, out object value) || !(value is MobileTokenUser user))
            {
                string problem = request.Properties.TryGetValue(MobileApiAuthHandler.AuthProblemKey, out object reason) ? reason as string : null;
                var response = MobileApiController.ErrorResponse(request, HttpStatusCode.Unauthorized,
                    problem ?? "Sign in again: the token is missing, expired or revoked.");
                response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Bearer"));
                actionContext.Response = response;
                return;
            }

            if (AdminOnly && !SecurityManager.IsAdminUser(user.UserName))
                actionContext.Response = MobileApiController.ErrorResponse(request, HttpStatusCode.Forbidden,
                    "Administrators only.");
        }
    }

    /// <summary>Gzip for responses (when the phone sends Accept-Encoding: gzip) and for gzip request bodies.</summary>
    public class MobileApiCompressionHandler : DelegatingHandler
    {
        private const int MinBytesToCompress = 860;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Content != null && request.Content.Headers.ContentEncoding.Any(e => e.Equals("gzip", StringComparison.OrdinalIgnoreCase)))
                request.Content = await DecompressAsync(request.Content);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.Content == null
                || response.Content.Headers.ContentEncoding.Any()
                || !request.Headers.AcceptEncoding.Any(e => e.Value.Equals("gzip", StringComparison.OrdinalIgnoreCase)))
                return response;

            string mediaType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
            if (mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return response;

            byte[] body = await response.Content.ReadAsByteArrayAsync();
            if (body.Length < MinBytesToCompress)
                return response;

            byte[] zipped;
            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
                    gzip.Write(body, 0, body.Length);
                zipped = output.ToArray();
            }

            var content = new ByteArrayContent(zipped);
            foreach (var header in response.Content.Headers.Where(h => !h.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)))
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            content.Headers.ContentEncoding.Add("gzip");
            response.Content = content;
            response.Headers.Vary.Add("Accept-Encoding");
            return response;
        }

        private static async Task<HttpContent> DecompressAsync(HttpContent compressed)
        {
            var output = new MemoryStream();
            using (var input = await compressed.ReadAsStreamAsync())
            using (var gzip = new GZipStream(input, CompressionMode.Decompress))
                await gzip.CopyToAsync(output);
            output.Position = 0;

            var content = new StreamContent(output);
            foreach (var header in compressed.Headers.Where(h =>
                         !h.Key.Equals("Content-Encoding", StringComparison.OrdinalIgnoreCase)
                         && !h.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)))
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            return content;
        }
    }

    /// <summary>
    /// CORS for web-view apps (Capacitor / Ionic send an Origin such as capacitor://localhost).
    /// Native HTTP clients send no Origin and are unaffected. Origins: appSetting MobileApi.CorsOrigins.
    /// </summary>
    public class MobileApiCorsHandler : DelegatingHandler
    {
        private const string DefaultOrigins = "https://localhost,capacitor://localhost";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string origin = request.Headers.TryGetValues("Origin", out var values) ? values.FirstOrDefault() : null;
            bool allowed = !string.IsNullOrEmpty(origin) && IsAllowed(origin);

            HttpResponseMessage response;
            if (allowed && request.Method == HttpMethod.Options)
            {
                response = request.CreateResponse(HttpStatusCode.NoContent);
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Authorization, Content-Type, Content-Encoding, If-None-Match");
                response.Headers.Add("Access-Control-Max-Age", "86400");
            }
            else
            {
                response = await base.SendAsync(request, cancellationToken);
            }

            if (allowed)
            {
                response.Headers.Add("Access-Control-Allow-Origin", origin);
                response.Headers.Add("Access-Control-Expose-Headers", "ETag");
                response.Headers.Vary.Add("Origin");
            }
            return response;
        }

        private static bool IsAllowed(string origin)
        {
            string setting = ConfigurationManager.AppSettings["MobileApi.CorsOrigins"];
            string list = string.IsNullOrWhiteSpace(setting) ? DefaultOrigins : setting;
            return list.Split(',').Select(o => o.Trim().TrimEnd('/'))
                .Any(o => o == "*" || string.Equals(o, origin.TrimEnd('/'), StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Hardening headers on every API reply: replies are never cached by browsers or proxies (unless the action set its own
    /// Cache-Control), never framed or sniffed, and HTTPS replies tell the phone to keep using HTTPS.
    /// </summary>
    public class MobileApiSecurityHeadersHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.Headers.CacheControl == null)
                response.Headers.CacheControl = new CacheControlHeaderValue { NoStore = true, Private = true };
            response.Headers.TryAddWithoutValidation("X-Content-Type-Options", "nosniff");
            response.Headers.TryAddWithoutValidation("X-Frame-Options", "DENY");
            response.Headers.TryAddWithoutValidation("Referrer-Policy", "no-referrer");
            if (request.RequestUri.Scheme == Uri.UriSchemeHttps)
                response.Headers.TryAddWithoutValidation("Strict-Transport-Security", "max-age=31536000");
            return response;
        }
    }

    /// <summary>
    /// Records every API call in MobileApiRequestLogTbl: time, user, method, path, status, duration, bytes, IP and a short
    /// note. Bodies and headers are never stored, so passwords, tokens and signatures stay out of the log.
    /// Kept for MobileApi.LogDays (0 turns logging off).
    /// </summary>
    public class MobileApiLogHandler : DelegatingHandler
    {
        public const string NoteKey = "MobileApi.LogNote";

        private static readonly object PurgeSync = new object();
        private static DateTime _lastPurge = DateTime.MinValue;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            int logDays = MobileAuthManager.LogDays;
            string path = request.RequestUri.AbsolutePath;
            if (logDays <= 0 || path.EndsWith("/api/v1/log", StringComparison.OrdinalIgnoreCase))
                return await base.SendAsync(request, cancellationToken);

            var watch = System.Diagnostics.Stopwatch.StartNew();
            var entry = new MobileApiLogEntry
            {
                At = TimeZoneUtils.Now(),
                Method = request.Method.Method,
                Path = request.RequestUri.PathAndQuery,
                RequestBytes = (int?)request.Content?.Headers.ContentLength,
                Ip = HttpContext.Current?.Request.UserHostAddress,
                Agent = request.Headers.UserAgent.ToString()
            };

            HttpResponseMessage response = null;
            try
            {
                response = await base.SendAsync(request, cancellationToken);
                return response;
            }
            finally
            {
                watch.Stop();
                entry.Ms = (int)Math.Min(int.MaxValue, watch.ElapsedMilliseconds);
                entry.Status = response == null ? 500 : (int)response.StatusCode;
                entry.ResponseBytes = (int?)response?.Content?.Headers.ContentLength;

                if (request.Properties.TryGetValue(MobileApiAuthHandler.UserKey, out object value) && value is MobileTokenUser user)
                {
                    entry.User = user.UserName;
                    entry.TokenId = user.TokenId;
                }
                if (request.Properties.TryGetValue(NoteKey, out object note))
                    entry.Note = note as string;
                if (entry.Note == null && response != null && entry.Status >= 400)
                    entry.Note = await ReadErrorAsync(response);

                Write(entry, logDays);
            }
        }

        private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
        {
            try
            {
                var content = response.Content;
                if (content == null || content.Headers.ContentEncoding.Any() || (content.Headers.ContentLength ?? 0) > 2000)
                    return null;
                var error = Newtonsoft.Json.JsonConvert.DeserializeObject<MobileApiError>(await content.ReadAsStringAsync());
                return error?.Error;
            }
            catch
            {
                return null;
            }
        }

        private static void Write(MobileApiLogEntry entry, int logDays)
        {
            bool purge = false;
            lock (PurgeSync)
            {
                if (DateTime.UtcNow - _lastPurge > TimeSpan.FromHours(6))
                {
                    _lastPurge = DateTime.UtcNow;
                    purge = true;
                }
            }

            Task.Run(() =>
            {
                try
                {
                    MobileApiSchemaInstaller.EnsureReady();
                    var repo = new Repositories.MobileApiRepository();
                    repo.InsertRequestLog(entry);
                    if (purge)
                        repo.PurgeRequestLog(TimeZoneUtils.Now().AddDays(-logDays));
                }
                catch (Exception ex)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.System, "Mobile API request log write failed: " + ex.Message, "mobile");
                }
            });
        }
    }

    /// <summary>Logs unhandled API errors and returns {"error": "..."} instead of an HTML page.</summary>
    public class MobileApiExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            Exception ex = context.Exception;
            string user = context.Request.Properties.TryGetValue(MobileApiAuthHandler.UserKey, out object value)
                ? (value as MobileTokenUser)?.UserName
                : null;
            AppLogger.WriteLog(SystemConstants.LogTypes.System,
                "Mobile API error on " + context.Request.Method + " " + context.Request.RequestUri.AbsolutePath + ": " + ex,
                user ?? "mobile");

            string message = context.Request.IsLocal() ? ex.Message : "The server could not complete the request.";
            context.Response = MobileApiController.ErrorResponse(context.Request, HttpStatusCode.InternalServerError, message);
        }
    }
}
