using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Api
{
    [RoutePrefix("api/v1")]
    public class StatusController : MobileApiController
    {
        /// <summary>No sign-in needed: lets the app check it can reach the server.</summary>
        [HttpGet, Route("ping")]
        public HttpResponseMessage Ping()
        {
            return JsonResponse(HttpStatusCode.OK, new
            {
                ok = true,
                api = "v1",
                version = ConfigurationManager.AppSettings["Version"],
                serverTime = TimeZoneUtils.Now().ToString("yyyy-MM-ddTHH:mm:ss")
            });
        }
    }

    [RoutePrefix("api/v1/auth")]
    public class AuthController : MobileApiController
    {
        [HttpPost, Route("login")]
        public HttpResponseMessage Login([FromBody] MobileLoginRequest request)
        {
            var response = new MobileAuthManager().Login(request, System.Web.HttpContext.Current?.Request.UserHostAddress,
                out string error, out bool throttled);
            string device = string.IsNullOrWhiteSpace(request?.DeviceName) ? "" : " on " + request.DeviceName.Trim();
            LogNote(response == null
                ? "Sign-in failed for '" + request?.UserName?.Trim() + "'" + device + ": " + error
                : "Signed in as " + response.User.UserName + device);
            if (response != null)
                return JsonResponse(HttpStatusCode.OK, response);
            return Error(throttled ? (HttpStatusCode)429 : HttpStatusCode.Unauthorized, error);
        }

        [HttpPost, Route("logout"), MobileAuthorize]
        public HttpResponseMessage Logout()
        {
            LogNote("Signed out (token revoked)");
            new MobileAuthManager().Logout(CurrentUser);
            return JsonResponse(HttpStatusCode.OK, new { ok = true });
        }

        [HttpGet, Route("me"), MobileAuthorize]
        public HttpResponseMessage Me()
        {
            return JsonResponse(HttpStatusCode.OK, new MobileAuthManager().BuildUserInfo(CurrentUser.UserName, CurrentUser.PersonId));
        }
    }

    [RoutePrefix("api/v1/delivery"), MobileAuthorize]
    public class DeliveryController : MobileApiController
    {
        /// <summary>Delivery dates with order counts ("mine" = orders for the signed-in driver).</summary>
        [HttpGet, Route("dates")]
        public HttpResponseMessage Dates()
        {
            return JsonWithETag(new MobileDeliveryManager().GetDates(CurrentUser));
        }

        /// <summary>
        /// The day's delivery sheet for offline use. person = "me" (default), "all" or a PersonID.
        /// Send the last ETag in If-None-Match to get 304 when nothing changed.
        /// </summary>
        [HttpGet, Route("sheet")]
        public HttpResponseMessage Sheet(string date = null, string person = "me")
        {
            if (!TryParseDate(date, out DateTime day))
                return Error(HttpStatusCode.BadRequest, "date must be yyyy-MM-dd.");

            int? personId;
            string p = (person ?? "me").Trim();
            if (p.Equals("all", StringComparison.OrdinalIgnoreCase))
                personId = null;
            else if (p.Equals("me", StringComparison.OrdinalIgnoreCase) || p.Length == 0)
                personId = CurrentUser.PersonId;
            else if (int.TryParse(p, out int id) && id > 0)
                personId = id;
            else
                return Error(HttpStatusCode.BadRequest, "person must be \"me\", \"all\" or a person id.");

            if (personId != CurrentUser.PersonId)
                LogNote(personId.HasValue ? "Viewed person " + personId + "'s deliveries" : "Viewed everyone's deliveries");

            var sheet = new MobileDeliveryManager().GetSheet(day, personId);
            return JsonWithETag(sheet, new { sheet.Date, sheet.PersonId, sheet.Orders });
        }

        /// <summary>Upload deliveries captured offline (received-by name, signature, outcome). Safe to retry: clientRef de-duplicates.</summary>
        [HttpPost, Route("sync")]
        public HttpResponseMessage Sync([FromBody] MobileDeliverySyncRequest request)
        {
            var results = new MobileDeliveryManager().Sync(request, CurrentUser, out string error);
            if (error != null)
                return Error(HttpStatusCode.BadRequest, error);

            int saved = results.Count(r => r.Ok && !r.Duplicate), duplicates = results.Count(r => r.Duplicate), failed = results.Count(r => !r.Ok);
            LogNote(results.Count + " deliveries: " + saved + " saved, " + duplicates + " already received, " + failed + " failed"
                + (failed > 0 ? " (" + string.Join("; ", results.Where(r => !r.Ok).Take(3).Select(r => "order " + r.OrderId + ": " + r.Message)) + ")" : "")
                + (saved > 0 ? " - orders " + string.Join(", ", results.Where(r => r.Ok && !r.Duplicate).Take(10).Select(r => r.OrderId)) : ""));
            return JsonResponse(HttpStatusCode.OK, new { results, autoComplete = MobileAuthManager.AutoCompleteDeliveries });
        }

        /// <summary>Proof captured for a date (orders due that day or delivered that day) or for one order. Administrators only.</summary>
        [HttpGet, Route("proofs"), MobileAuthorize(AdminOnly = true)]
        public HttpResponseMessage Proofs(string date = null, int? orderId = null)
        {
            DateTime? day = null;
            if (!orderId.HasValue)
            {
                if (!TryParseDate(date, out DateTime parsed))
                    return Error(HttpStatusCode.BadRequest, "date must be yyyy-MM-dd.");
                day = parsed;
            }
            return JsonResponse(HttpStatusCode.OK, new MobileDeliveryManager().ListProofs(day, orderId));
        }

        [HttpGet, Route("proofs/{proofId:long}/signature"), MobileAuthorize(AdminOnly = true)]
        public HttpResponseMessage Signature(long proofId)
        {
            if (!new MobileDeliveryManager().TryGetSignature(proofId, out byte[] image, out string contentType))
                return Error(HttpStatusCode.NotFound, "No signature for proof " + proofId + ".");

            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(image) };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrEmpty(contentType) ? "image/png" : contentType);
            response.Headers.CacheControl = new CacheControlHeaderValue { Private = true, NoStore = true };
            return response;
        }
    }

    [RoutePrefix("api/v1/repairs"), MobileAuthorize]
    public class RepairsController : MobileApiController
    {
        /// <summary>open=true (default) hides Done repairs; since=yyyy-MM-ddTHH:mm:ss returns only repairs changed after that time.</summary>
        [HttpGet, Route("")]
        public HttpResponseMessage List(bool open = true, int? contactId = null, string since = null, int max = 100)
        {
            DateTime? changedSince = null;
            if (!string.IsNullOrWhiteSpace(since))
            {
                if (!DateTime.TryParse(since, System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out DateTime parsed))
                    return Error(HttpStatusCode.BadRequest, "since must be yyyy-MM-ddTHH:mm:ss.");
                changedSince = parsed;
            }
            return JsonWithETag(new MobileRepairManager().List(open, contactId, changedSince, max));
        }

        [HttpGet, Route("{id:int}")]
        public HttpResponseMessage Get(int id)
        {
            var repair = new MobileRepairManager().Get(id);
            return repair == null
                ? Error(HttpStatusCode.NotFound, "Repair " + id + " was not found.")
                : JsonResponse(HttpStatusCode.OK, repair);
        }

        [HttpPost, Route("")]
        public HttpResponseMessage Create([FromBody] MobileRepairSave save)
        {
            var result = new MobileRepairManager().Create(save, CurrentUser);
            LogNote(!result.Ok ? "Repair not created: " + result.Message
                : (result.Duplicate ? "Repair already created: " : "Repair created: ") + result.Repair?.Id + " for contact " + result.Repair?.ContactId);
            HttpStatusCode status = !result.Ok ? HttpStatusCode.BadRequest
                : result.Duplicate ? HttpStatusCode.OK : HttpStatusCode.Created;
            return JsonResponse(status, result);
        }

        /// <summary>Partial update: only the members sent are changed. POST is accepted too for hosts that block PUT.</summary>
        [AcceptVerbs("PUT", "POST"), Route("{id:int}")]
        public HttpResponseMessage Update(int id, [FromBody] MobileRepairSave save)
        {
            var result = new MobileRepairManager().Update(id, save, CurrentUser);
            LogNote(result.Ok ? "Repair " + id + " updated" : "Repair " + id + " not updated: " + result.Message);
            return JsonResponse(result.Ok ? HttpStatusCode.OK : HttpStatusCode.BadRequest, result);
        }
    }

    [RoutePrefix("api/v1"), MobileAuthorize]
    public class LookupsController : MobileApiController
    {
        /// <summary>Repair statuses, faults, machine types, conditions and delivery people (cache on the phone; use the ETag).</summary>
        [HttpGet, Route("lookups")]
        public HttpResponseMessage Lookups()
        {
            MobileApiSchemaInstaller.EnsureReady();
            return JsonWithETag(new MobileApiRepository().GetLookups());
        }

        [HttpGet, Route("contacts")]
        public HttpResponseMessage Contacts(string q = null, int max = 20, bool disabled = false)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
                return Error(HttpStatusCode.BadRequest, "q needs at least 2 characters.");
            return JsonResponse(HttpStatusCode.OK, new MobileApiRepository().SearchContacts(q, disabled, max));
        }

        [HttpGet, Route("contacts/{id:int}")]
        public HttpResponseMessage Contact(int id)
        {
            var contact = new MobileApiRepository().GetContact(id);
            return contact == null
                ? Error(HttpStatusCode.NotFound, "Contact " + id + " was not found.")
                : JsonResponse(HttpStatusCode.OK, contact);
        }
    }

    [RoutePrefix("api/v1/devices"), MobileAuthorize(AdminOnly = true)]
    public class DevicesController : MobileApiController
    {
        [HttpGet, Route("")]
        public HttpResponseMessage List()
        {
            MobileApiSchemaInstaller.EnsureReady();
            var now = TimeZoneUtils.Now();
            var list = new MobileApiRepository().ListTokens().Select(t => new
            {
                id = t.TokenID,
                userName = t.UserName,
                personId = t.PersonID,
                deviceName = t.DeviceName,
                deviceId = t.DeviceId,
                appVersion = t.AppVersion,
                createdAt = t.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                lastUsedAt = t.LastUsedAt?.ToString("yyyy-MM-ddTHH:mm:ss"),
                expiresAt = t.ExpiresAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                active = !t.RevokedAt.HasValue && t.ExpiresAt > now
            });
            return JsonResponse(HttpStatusCode.OK, list);
        }

        [HttpPost, Route("{id:long}/revoke")]
        public HttpResponseMessage Revoke(long id)
        {
            bool ok = new MobileAuthManager().RevokeToken(id, CurrentUser.UserName);
            if (ok)
                LogNote("Revoked device token " + id);
            return ok
                ? JsonResponse(HttpStatusCode.OK, new { ok = true })
                : Error(HttpStatusCode.NotFound, "Device token " + id + " was not found or is already revoked.");
        }
    }

    [RoutePrefix("api/v1"), MobileAuthorize(AdminOnly = true)]
    public class RequestLogController : MobileApiController
    {
        /// <summary>Recent API calls, newest first. user = one username; problems=true shows only errors (status 400+).</summary>
        [HttpGet, Route("log")]
        public HttpResponseMessage Log(int max = 200, string user = null, bool problems = false)
        {
            MobileApiSchemaInstaller.EnsureReady();
            return JsonResponse(HttpStatusCode.OK, new
            {
                keepDays = MobileAuthManager.LogDays,
                entries = new MobileApiRepository().ListRequestLog(max, user, problems)
            });
        }
    }
}
