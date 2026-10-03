using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Runs the slow part of a driver delivery (Order Done, the Woo update, emails) after the phone has had its reply.
    /// Proofs waiting for Order Done are stored with status Pending, so work cut off by an app restart is picked up
    /// again at the next start, delivery upload or visit to System → Driver App. One proof is completed at a time.
    /// </summary>
    public static class MobileDeliveryCompletionQueue
    {
        private static int _draining;

        /// <summary>Starts working through the Pending proofs unless that is already happening.</summary>
        public static void Kick()
        {
            if (Interlocked.CompareExchange(ref _draining, 1, 0) != 0)
                return;
            if (!Start(Drain))
                Interlocked.Exchange(ref _draining, 0);
        }

        /// <summary>Runs one-off work (e.g. an email) in the background as the given user; failures are logged.</summary>
        public static void Run(string userName, string what, Action work)
        {
            Action safe = () =>
            {
                try
                {
                    work();
                }
                catch (Exception ex)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Delivery, what + " failed: " + ex.Message, userName);
                }
            };
            if (!Start(ct => AsUser(userName, safe)))
                AsUser(userName, safe);
        }

        private static bool Start(Action<CancellationToken> work)
        {
            if (!HostingEnvironment.IsHosted)
                return false;
            HostingEnvironment.QueueBackgroundWorkItem(work);
            return true;
        }

        private static void Drain(CancellationToken ct)
        {
            bool stuck = false;
            var repo = new MobileApiRepository();
            try
            {
                MobileApiSchemaInstaller.EnsureReady();
                var manager = new MobileDeliveryManager();
                var tried = new HashSet<long>();
                MobileDeliveryProof proof;
                while (!ct.IsCancellationRequested && (proof = repo.NextPendingProof()) != null)
                {
                    if (!tried.Add(proof.ProofID))
                    {
                        stuck = true;
                        AppLogger.WriteLog(SystemConstants.LogTypes.Delivery,
                            "Background Order Done: proof " + proof.ProofID + " (order " + proof.OrderID + ") is still waiting; trying again later.");
                        break;
                    }
                    MobileDeliveryProof current = proof;
                    AsUser(current.UserName, () => manager.CompletePending(current));
                }
            }
            catch (Exception ex)
            {
                stuck = true;
                AppLogger.WriteLog(SystemConstants.LogTypes.Delivery, "Background Order Done stopped: " + ex.Message);
            }
            finally
            {
                Interlocked.Exchange(ref _draining, 0);
            }

            // A delivery that arrived just as the loop finished would otherwise wait for the next upload.
            try
            {
                if (!stuck && !ct.IsCancellationRequested && repo.NextPendingProof() != null)
                    Kick();
            }
            catch
            {
            }
        }

        /// <summary>
        /// The Order Done code keeps its workflow in HttpContext.Items and logs the signed-in user, as in an API request
        /// (which has no session either), so background work gets a request-less context for the driver.
        /// </summary>
        private static void AsUser(string userName, Action work)
        {
            HttpContext previous = HttpContext.Current;
            try
            {
                var context = new HttpContext(new HttpRequest(string.Empty, "http://localhost/", string.Empty), new HttpResponse(TextWriter.Null));
                context.User = new GenericPrincipal(
                    new GenericIdentity(string.IsNullOrWhiteSpace(userName) ? "driver app" : userName.Trim()), new string[0]);
                HttpContext.Current = context;
                work();
            }
            finally
            {
                HttpContext.Current = previous;
            }
        }
    }
}
