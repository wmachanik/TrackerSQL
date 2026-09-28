using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TrackerSQL.Classes;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Tracks each user's in-progress "order done" workflow in session.
    /// Temp rows are linked to real orders via TempOrdersLinesTbl.OriginalOrderID.
    /// </summary>
    public static class TempOrderSession
    {
        public const string QueryOrderId = "OrderID";

        public static int? GetHeaderId()
        {
            return GetValue(SystemConstants.SessionConstants.TempOrderHeaderId);
        }

        public static int? GetOrderId()
        {
            return GetValue(SystemConstants.SessionConstants.TempOrderId);
        }

        public static void SetHeaderId(int headerId)
        {
            SetValue(SystemConstants.SessionConstants.TempOrderHeaderId, headerId);
        }

        public static void SetOrderId(int orderId)
        {
            SetValue(SystemConstants.SessionConstants.TempOrderId, orderId);
        }

        public static void Clear()
        {
            ClearHeaderId();
            ClearOrderId();
        }

        public static void ClearHeaderId()
        {
            RemoveValue(SystemConstants.SessionConstants.TempOrderHeaderId);
        }

        public static void ClearOrderId()
        {
            RemoveValue(SystemConstants.SessionConstants.TempOrderId);
        }

        // Requests without session state (the mobile REST API) keep the workflow for the current request only.
        private const string ItemsKeyPrefix = "TempOrderSession.";

        private static int? GetValue(string key)
        {
            var context = HttpContext.Current;
            if (context == null)
                return null;

            object value = context.Session != null ? context.Session[key] : context.Items[ItemsKeyPrefix + key];
            return value == null ? (int?)null : Convert.ToInt32(value);
        }

        private static void SetValue(string key, int value)
        {
            var context = HttpContext.Current;
            if (context == null)
                return;

            if (context.Session != null)
                context.Session[key] = value;
            else
                context.Items[ItemsKeyPrefix + key] = value;
        }

        private static void RemoveValue(string key)
        {
            var context = HttpContext.Current;
            if (context == null)
                return;

            if (context.Session != null)
                context.Session.Remove(key);
            else
                context.Items.Remove(ItemsKeyPrefix + key);
        }

        /// <summary>
        /// Resolves the current temp workflow from session, or from OrderID query string if session was lost.
        /// </summary>
        public static bool TryResolve(out int headerId, out int orderId)
        {
            headerId = 0;
            orderId = 0;

            int? sessionHeaderId = GetHeaderId();
            int? sessionOrderId = GetOrderId();

            if (sessionHeaderId.HasValue && sessionHeaderId.Value > 0)
            {
                headerId = sessionHeaderId.Value;
                if (sessionOrderId.HasValue && sessionOrderId.Value > 0)
                    orderId = sessionOrderId.Value;
                else
                {
                    var linkedOrderIds = new TempOrdersLinesRepository().GetOriginalOrderIdsForHeader(headerId);
                    if (linkedOrderIds.Count > 0)
                        orderId = linkedOrderIds[0];
                }

                return headerId > 0;
            }

            int? queryOrderId = GetOrderIdFromQueryString();
            if (!queryOrderId.HasValue || queryOrderId.Value <= 0)
                return false;

            var linesRepository = new TempOrdersLinesRepository();
            int? queryHeaderId = linesRepository.GetHeaderIdByOriginalOrderId(queryOrderId.Value);
            if (!queryHeaderId.HasValue || queryHeaderId.Value <= 0)
                return false;

            headerId = queryHeaderId.Value;
            orderId = queryOrderId.Value;
            SetHeaderId(headerId);
            SetOrderId(orderId);
            return true;
        }

        public static int? GetOrderIdFromQueryString()
        {
            string raw = HttpContext.Current?.Request?.QueryString[QueryOrderId];
            if (int.TryParse(raw, out int orderId) && orderId > 0)
                return orderId;

            return null;
        }

        /// <summary>
        /// Abandons the in-progress temp order for this session (Cancel).
        /// </summary>
        public static void CleanupCurrentTempOrder()
        {
            if (!TryResolve(out int headerId, out int orderId))
            {
                Clear();
                return;
            }

            CleanupCompletedOrder(orderId, headerId);
        }

        /// <summary>
        /// Removes temp rows after order done completes. Uses OriginalOrderID as the authoritative link to OrdersTbl.
        /// </summary>
        public static void CleanupCompletedOrder(int orderId, int? headerId = null)
        {
            var linesRepository = new TempOrdersLinesRepository();
            var headerRepository = new TempOrdersHeaderRepository();

            if (orderId > 0)
            {
                linesRepository.DeleteByOriginalOrderId(orderId);
            }
            else if (headerId.HasValue && headerId.Value > 0)
            {
                foreach (int linkedOrderId in linesRepository.GetOriginalOrderIdsForHeader(headerId.Value))
                {
                    linesRepository.DeleteByOriginalOrderId(linkedOrderId);
                }
            }

            int? resolvedHeaderId = headerId ?? GetHeaderId();
            if (resolvedHeaderId.HasValue && resolvedHeaderId.Value > 0)
            {
                headerRepository.DeleteByHeaderId(resolvedHeaderId.Value);
            }

            Clear();
        }

        public static void BeginOrderDoneWorkflow(int headerId, IEnumerable<int> originalOrderIds)
        {
            SetHeaderId(headerId);

            int orderId = originalOrderIds?
                .FirstOrDefault(id => id > 0) ?? 0;

            if (orderId > 0)
                SetOrderId(orderId);
        }
    }
}
