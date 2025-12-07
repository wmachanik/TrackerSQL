using System;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Handles adapting the Order Detail page to a public (token) read-only view.
    /// SRP: This class is ONLY about transforming the page for public viewing.
    /// </summary>
    public class PublicOrderViewManager
    {
        public class PublicOrderContext
        {
            public long CustomerId { get; set; }
            public DateTime DeliveryDateUtc { get; set; }
            public bool IsPublicReadOnly { get; set; }
        }

        /// <summary>
        /// Applies read-only behavior: disables mutating controls, sets captions, status panel, and binds session.
        /// </summary>
        public void ApplyPublicReadOnlyUI(Page page,
                                          PublicOrderContext ctx,
                                          DetailsView dvOrderHeader,
                                          GridView gvOrderLines,
                                          Literal statusLiteral,
                                          WebControl[] mutationControls,
                                          Action<long, DateTime> sessionBinder)
        {
            if (ctx == null || !ctx.IsPublicReadOnly) return;

            // 1. Header caption
            TrySetPageTitle(page, "Order (View Only)");

            // 2. Lock DetailsView
            if (dvOrderHeader != null)
            {
                dvOrderHeader.ChangeMode(DetailsViewMode.ReadOnly);
                foreach (DataControlField f in dvOrderHeader.Fields)
                {
                    if (f is CommandField cf)
                    {
                        cf.ShowEditButton = cf.ShowDeleteButton = cf.ShowInsertButton = false;
                    }
                }
            }

            // 3. Disable grid editing
            if (gvOrderLines != null)
            {
                // Remove handlers if already attached in code-behind
                gvOrderLines.RowEditing -= page.GetType()
                                               .GetMethod("gvOrderLines_RowEditing",
                                                   System.Reflection.BindingFlags.Instance |
                                                   System.Reflection.BindingFlags.NonPublic)
                                               ?.CreateDelegate(typeof(GridViewEditEventHandler), page) as GridViewEditEventHandler;

                gvOrderLines.RowUpdating -= page.GetType()
                                                .GetMethod("gvOrderLines_RowUpdating",
                                                    System.Reflection.BindingFlags.Instance |
                                                    System.Reflection.BindingFlags.NonPublic)
                                                ?.CreateDelegate(typeof(GridViewUpdateEventHandler), page) as GridViewUpdateEventHandler;

                gvOrderLines.RowCancelingEdit -= page.GetType()
                                                     .GetMethod("gvOrderLines_RowCancelingEdit",
                                                         System.Reflection.BindingFlags.Instance |
                                                         System.Reflection.BindingFlags.NonPublic)
                                                     ?.CreateDelegate(typeof(GridViewCancelEditEventHandler), page) as GridViewCancelEditEventHandler;

                foreach (DataControlField f in gvOrderLines.Columns)
                {
                    if (f is CommandField cf)
                    {
                        cf.ShowEditButton = false;
                        cf.ShowDeleteButton = false;
                    }
                }
            }

            // 4. Hide mutation buttons/panels
            if (mutationControls != null)
            {
                foreach (var ctl in mutationControls)
                {
                    if (ctl != null) ctl.Visible = false;
                }
            }

            // 5. Status panel (mail link + login link)
            if (statusLiteral != null)
            {
                string ordersEmail = ConfigHelper.GetString("OrdersEmail", SystemConstants.EmailConstants.DefaultAdminEmail);
                string loginPrompt = MessageProvider.Get(MessageKeys.OrderDetail.PublicViewLoginPrompt);
                string btnText = MessageProvider.Get(MessageKeys.OrderDetail.RequestChangesButtonText);
                string subjTemplate = MessageProvider.Get(MessageKeys.OrderDetail.RequestChangesMailSubject);
                string bodyTemplate = MessageProvider.Get(MessageKeys.OrderDetail.RequestChangesMailBody);

                string subject = HttpUtility.UrlEncode(string.Format(subjTemplate, ctx.CustomerId, ctx.DeliveryDateUtc.ToString("yyyy-MM-dd")));
                string body = HttpUtility.UrlEncode(string.Format(bodyTemplate, ctx.CustomerId, ctx.DeliveryDateUtc.ToString("yyyy-MM-dd")));

                string mailLink = $"<a class='btn btn-sm btn-secondary' href=\"mailto:{ordersEmail}?subject={subject}&body={body}\">{HttpUtility.HtmlEncode(btnText)}</a>";

                string rawUrl = page.Request?.RawUrl ?? "/";

                statusLiteral.Text =
                    "<div class='alert alert-info' style='margin-top:8px;'>" +
                    HttpUtility.HtmlEncode(loginPrompt) + " " +
                    "<a href='/Account/Login.aspx?ReturnUrl=" + HttpUtility.UrlEncode(rawUrl) + "'>Log in</a> " +
                    "&nbsp;|&nbsp;" + mailLink + "</div>";
            }

            // 6. Session binding (delegate so code-behind decides keys)
            sessionBinder?.Invoke(ctx.CustomerId, ctx.DeliveryDateUtc);

            // 7. Disable all postbacks that mutate – recommended to also guard in page override (already done)
        }

        private void TrySetPageTitle(Page page, string title)
        {
            try { page.Title = title; }
            catch { /* ignore */ }
        }
    }
}