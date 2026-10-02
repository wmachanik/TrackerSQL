using System;
using System.Web;
using TrackerSQL.Classes;
using TrackerSQL.Managers;

namespace TrackerSQL.Handlers
{
    /// <summary>
    /// The proof of delivery PDF for a driver-app delivery: ProofOfDelivery.ashx?id=ProofID (add &amp;download=1 to save it).
    /// Tracker staff only; contact-portal logins are refused.
    /// </summary>
    public class ProofOfDelivery : IHttpHandler
    {
        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();
            string user = context.User?.Identity?.Name;
            if (context.User?.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                context.Response.StatusCode = 401;
                return;
            }
            if (ContactPortalManager.IsContactRole(user))
            {
                context.Response.StatusCode = 403;
                return;
            }
            if (!long.TryParse(context.Request.QueryString["id"], out long proofId) || proofId <= 0)
            {
                Fail(context, 400, "Which proof? Use ?id=ProofID.");
                return;
            }

            try
            {
                var manager = new ProofOfDeliveryManager();
                ProofOfDeliveryManager.PodDocument doc = manager.Load(proofId, out string error);
                if (doc == null)
                {
                    Fail(context, 404, error);
                    return;
                }
                byte[] pdf = manager.BuildPdf(doc);
                bool download = context.Request.QueryString["download"] == "1";
                context.Response.ContentType = "application/pdf";
                context.Response.AddHeader("Content-Disposition",
                    (download ? "attachment" : "inline") + "; filename=\"" + ProofOfDeliveryManager.FileName(doc) + "\"");
                context.Response.BinaryWrite(pdf);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Delivery, "Proof of delivery PDF " + proofId + " failed: " + ex.Message, user);
                Fail(context, 500, "The proof of delivery could not be made: " + ex.Message);
            }
        }

        private static void Fail(HttpContext context, int status, string message)
        {
            context.Response.StatusCode = status;
            context.Response.ContentType = "text/plain";
            context.Response.Write(message);
        }
    }
}
