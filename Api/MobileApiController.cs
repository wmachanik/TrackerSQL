using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Api
{
    /// <summary>Base for /api/v1 controllers: the signed-in device user, JSON errors and ETag responses.</summary>
    public abstract class MobileApiController : ApiController
    {
        protected MobileTokenUser CurrentUser =>
            Request.Properties.TryGetValue(MobileApiAuthHandler.UserKey, out object value) ? value as MobileTokenUser : null;

        public static HttpResponseMessage ErrorResponse(HttpRequestMessage request, HttpStatusCode status, string message)
        {
            return JsonResponse(status, new MobileApiError { Error = message });
        }

        protected HttpResponseMessage Error(HttpStatusCode status, string message)
        {
            return ErrorResponse(Request, status, message);
        }

        /// <summary>Short note shown against this call in the request log (never put passwords, tokens or bodies here).</summary>
        protected void LogNote(string note)
        {
            Request.Properties[MobileApiLogHandler.NoteKey] = note;
        }

        protected static HttpResponseMessage JsonResponse(HttpStatusCode status, object value)
        {
            return new HttpResponseMessage(status)
            {
                Content = new StringContent(JsonConvert.SerializeObject(value, WebApiConfig.JsonSettings), Encoding.UTF8, "application/json")
            };
        }

        /// <summary>
        /// JSON with an ETag computed from <paramref name="hashSource"/> (default: the value). When the phone
        /// sends the same ETag in If-None-Match the reply is an empty 304, so unchanged data costs no download.
        /// </summary>
        protected HttpResponseMessage JsonWithETag(object value, object hashSource = null)
        {
            string json = JsonConvert.SerializeObject(value, WebApiConfig.JsonSettings);
            string source = hashSource == null ? json : JsonConvert.SerializeObject(hashSource, WebApiConfig.JsonSettings);
            var etag = new EntityTagHeaderValue("\"" + Hash(source) + "\"");

            HttpResponseMessage response;
            if (Request.Headers.IfNoneMatch.Any(t => t.Tag == etag.Tag))
                response = new HttpResponseMessage(HttpStatusCode.NotModified);
            else
                response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

            response.Headers.ETag = etag;
            response.Headers.CacheControl = new CacheControlHeaderValue { Private = true, NoCache = true };
            return response;
        }

        protected static bool TryParseDate(string value, out DateTime date)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Equals("today", StringComparison.OrdinalIgnoreCase))
            {
                date = TimeZoneUtils.Now().Date;
                return true;
            }
            return DateTime.TryParseExact(value.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        private static string Hash(string text)
        {
            using (var sha = SHA1.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
                return Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            }
        }
    }
}
