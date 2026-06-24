using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class ViewMyOrder : Page
    {
        private long _custId;
        private DateTime _deliveryUtc;
        private bool _tokenValid;
        private readonly IOrderChangeRequestService _changeReqService = new OrderChangeRequestLoggingService();
        private readonly OrdersRepository _ordersRepository = new OrdersRepository();
        private readonly ContactsRepository _contactsRepository = new ContactsRepository();
        private readonly ItemsRepository _itemsRepository = new ItemsRepository();
        private readonly ItemPackagingsRepository _packagingsRepository = new ItemPackagingsRepository();
        private bool _hadQueryDate;
        private DateTime _queryDeliveryDate;
        private string _queryNotes;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            string token = Request.QueryString["t"];
            if (OrderViewTokenHelper.TryValidateCustomerDeliveryToken(token, out long c, out DateTime dUtc))
            {
                _tokenValid = true;
                _custId = c;
                _deliveryUtc = dUtc.Date;
            }

            if (long.TryParse(Request.QueryString["CustomerID"], out long qCust) && qCust > 0)
            {
                if (_tokenValid && qCust != _custId)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                        $"ViewMyOrder WARNING: Query CustomerID {qCust} differs from token CustomerID {_custId}. Ignoring query value for security.");
                }
                else if (!_tokenValid)
                {
                    _custId = qCust;
                }
            }

            string qsDelivery = Request.QueryString["DeliveryDate"];
            if (!string.IsNullOrWhiteSpace(qsDelivery))
            {
                if (DateTime.TryParse(qsDelivery, out DateTime qDate))
                {
                    _hadQueryDate = true;
                    _queryDeliveryDate = qDate.Date;
                }
            }

            _queryNotes = Request.QueryString["Notes"];
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!_tokenValid)
            {
                ShowError("Invalid or expired order link.");
                return;
            }

            if (!IsPostBack)
            {
                try
                {
                    LoadOrder();
                }
                catch (Exception ex)
                {
                    ShowError("Error loading order: " + ex.Message);
                }
            }
        }

        private void ShowError(string msg)
        {
            phError.Visible = true;
            phOrder.Visible = false;
            errBox.InnerText = msg;
        }

        private void LoadOrder()
        {
            if (_custId <= 0)
            {
                ShowError("Invalid customer reference.");
                return;
            }

            TimeZoneInfo appTz = TimeZoneInfo.FindSystemTimeZoneById(
                ConfigHelper.GetString("AppTimeZoneId", "South Africa Standard Time"));

            DateTime tokenUtc = _deliveryUtc.Date;
            DateTime tokenAsLocal = tokenUtc;
            DateTime tokenConvertedLocal = TimeZoneInfo.ConvertTimeFromUtc(tokenUtc, appTz).Date;

            var tried = new StringBuilder();

            var dynamicList = new List<DateTime>();
            if (_hadQueryDate) dynamicList.Add(_queryDeliveryDate);
            if (_hadQueryDate)
            {
                dynamicList.Add(_queryDeliveryDate.AddDays(1));
                dynamicList.Add(_queryDeliveryDate.AddDays(-1));
            }
            dynamicList.Add(tokenAsLocal);
            if (tokenConvertedLocal != tokenAsLocal) dynamicList.Add(tokenConvertedLocal);
            dynamicList.Add(tokenAsLocal.AddDays(1));
            dynamicList.Add(tokenAsLocal.AddDays(-1));

            var candidates = dynamicList.Distinct().ToList();

            if (!string.IsNullOrWhiteSpace(_queryNotes))
            {
                foreach (var cand in candidates)
                {
                    var lines = _ordersRepository.GetPublicOrderLines(_custId, cand, _queryNotes);
                    tried.AppendLine($"(NOTES) Tried {cand:yyyy-MM-dd} rows={lines.Count}");
                    if (lines.Count > 0)
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                            $"ViewMyOrder: notes-match {cand:yyyy-MM-dd} tokenUtc={tokenUtc:yyyy-MM-dd}");
                        RenderOrder(lines, cand);
                        return;
                    }
                }
            }

            foreach (var cand in candidates)
            {
                var lines = _ordersRepository.GetPublicOrderLines(_custId, cand, null);
                tried.AppendLine($"Tried {cand:yyyy-MM-dd} rows={lines.Count}");
                if (lines.Count > 0)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                        $"ViewMyOrder: match {cand:yyyy-MM-dd} tokenUtc={tokenUtc:yyyy-MM-dd} queryDate={(_hadQueryDate ? _queryDeliveryDate.ToString("yyyy-MM-dd") : "n/a")}");
                    RenderOrder(lines, cand);
                    return;
                }
            }

            DateTime rangeCenter = _hadQueryDate ? _queryDeliveryDate : tokenAsLocal;
            var rangeLines = _ordersRepository.GetPublicOrderLinesInRange(
                _custId,
                rangeCenter.AddDays(-1),
                rangeCenter.AddDays(1));
            tried.AppendLine($"Range fallback center={rangeCenter:yyyy-MM-dd} rows={rangeLines.Count}");

            if (rangeLines.Count > 0)
            {
                DateTime chosen = rangeLines
                    .GroupBy(line => line.RequiredByDate.Date)
                    .OrderByDescending(g => g.Count())
                    .ThenBy(g => g.Key)
                    .First().Key;

                var finalLines = rangeLines.Where(line => line.RequiredByDate.Date == chosen).ToList();

                AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                    $"ViewMyOrder: range fallback chose {chosen:yyyy-MM-dd} tokenUtc={tokenUtc:yyyy-MM-dd}");
                RenderOrder(finalLines, chosen);
                return;
            }

            string qDateStr = _hadQueryDate ? _queryDeliveryDate.ToString("yyyy-MM-dd") : "n/a";
            AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                "ViewMyOrder: NO MATCH. " +
                $"TokenUtc={tokenUtc:yyyy-MM-dd} QueryDate={qDateStr} Attempts:\n{tried}");

            ShowError("No order lines found for that delivery date.");
        }

        private void RenderOrder(IList<PublicOrderLineView> lines, DateTime effectiveDeliveryLocal)
        {
            string customerName = _contactsRepository.GetContactNameById((int)_custId);
            if (string.IsNullOrEmpty(customerName))
            {
                ShowError("Customer not found.");
                return;
            }

            string sharedNotes = string.Empty;
            var sbRows = new StringBuilder();
            int lineCount = 0;

            foreach (var line in lines)
            {
                lineCount++;
                string itemDesc;
                try { itemDesc = _itemsRepository.GetItemDescById(line.ItemTypeID); }
                catch { itemDesc = "Item " + line.ItemTypeID; }

                string uom = string.Empty;
                try { uom = _itemsRepository.GetItemUnitOfMeasure(line.ItemTypeID); } catch { }

                string pkgDesc = "-";
                if (line.PackagingID > 0)
                {
                    try { pkgDesc = _packagingsRepository.GetPackagingDescById(line.PackagingID) ?? "-"; } catch { }
                }

                if (string.IsNullOrEmpty(sharedNotes) && !string.IsNullOrEmpty(line.Notes))
                    sharedNotes = line.Notes;

                sbRows.Append("<tr>")
                      .Append("<td>").Append(HttpUtility.HtmlEncode(itemDesc)).Append("</td>")
                      .Append("<td>").Append(HttpUtility.HtmlEncode(pkgDesc)).Append("</td>")
                      .Append("<td>").Append(line.QuantityOrdered.ToString("0.###")).Append("</td>")
                      .Append("<td>").Append(HttpUtility.HtmlEncode(uom)).Append("</td>")
                      .Append("</tr>");
            }

            hdrTitle.InnerText = "Order for " + customerName;
            hdrMeta.InnerHtml =
                $"Customer ID: {_custId} &nbsp;|&nbsp; Delivery: {effectiveDeliveryLocal:yyyy-MM-dd}<br/>" +
                $"Token UTC Date: {_deliveryUtc:yyyy-MM-dd}";

            if (!string.IsNullOrWhiteSpace(sharedNotes))
            {
                litHeaderNotes.Text =
                    $"<div class='warn'><strong>Notes:</strong> {HttpUtility.HtmlEncode(sharedNotes)}</div>";
            }

            tbodyLines.InnerHtml = sbRows.ToString();
            litTotalLines.Text = lineCount.ToString();

            phOrder.Visible = true;
            phError.Visible = false;
        }

        protected void btnSubmitChange_Click(object sender, EventArgs e)
        {
            if (!_tokenValid)
            {
                ShowError("Token invalid.");
                return;
            }

            string raw = tbxChangeRequest.Text ?? string.Empty;
            if (string.IsNullOrWhiteSpace(raw))
            {
                litChangeResult.Text = "<div class='status'>Please enter details for the change request.</div>";
                return;
            }

            DateTime next = (DateTime?)Session["CRQ_Next_VM"] ?? DateTime.MinValue;
            if (DateTime.UtcNow < next)
            {
                litChangeResult.Text = "<div class='status'>Please wait before sending another request.</div>";
                return;
            }

            string decoded = UrlTextDecoder.DecodePossiblyUrlEncoded(raw)
                                           .Replace("\\r\\n", "\r\n")
                                           .TrimEnd();

            var submission = new ChangeRequestSubmission
            {
                CustomerID = _custId,
                DeliveryDateUtc = _deliveryUtc.Date,
                RequestTextRaw = decoded,
                SourceIp = Request.UserHostAddress,
                UserAgent = Request.UserAgent,
                TokenRef = null
            };

            try
            {
                var result = _changeReqService.Submit(submission);
                if (result.Success)
                {
                    Session["CRQ_Next_VM"] = DateTime.UtcNow.AddSeconds(60);
                    tbxChangeRequest.Text = string.Empty;
                    litChangeResult.Text = "<div class='ok'>Change request submitted.</div>";
                }
                else
                {
                    litChangeResult.Text = "<div class='status'>Failed: " + HttpUtility.HtmlEncode(result.Message) + "</div>";
                }
            }
            catch (Exception ex)
            {
                litChangeResult.Text = "<div class='status'>Error: " + HttpUtility.HtmlEncode(ex.Message) + "</div>";
            }
        }
    }
}
