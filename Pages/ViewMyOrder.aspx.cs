using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using TrackerDotNet.Classes;
using TrackerDotNet.Controls;
using TrackerDotNet.Managers;

namespace TrackerDotNet.Pages
{
    public partial class ViewMyOrder : Page
    {
        private long _custId;
        private DateTime _deliveryUtc;
        private bool _tokenValid;
        private readonly IOrderChangeRequestService _changeReqService = new OrderChangeRequestLoggingService();
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

        private DataTable FetchOrderLinesExact(long customerId, DateTime requiredDateLocal)
        {
            const string sql = @"
SELECT OrderID, ItemTypeID, QuantityOrdered, PackagingID, RequiredByDate, Notes
FROM OrderTbl
WHERE CustomerID = ?
  AND RequiredByDate = ?
ORDER BY OrderID";

            using (var db = new TrackerDb())
            {
                db.AddWhereParams(customerId, DbType.Int64);
                db.AddWhereParams(requiredDateLocal.Date, DbType.Date);

                var ds = db.ReturnDataSet(sql, db.WhereParams);
                if (ds == null || ds.Tables.Count == 0) return new DataTable();
                var table = ds.Tables["objDataSet"] ?? ds.Tables[0];
                return table;
            }
        }

        private DataTable FetchOrderLinesRange(long customerId, DateTime centerLocal)
        {
            const string sql = @"
SELECT OrderID, ItemTypeID, QuantityOrdered, PackagingID, RequiredByDate, Notes
FROM OrderTbl
WHERE CustomerID = ?
  AND RequiredByDate BETWEEN ? AND ?
ORDER BY RequiredByDate, OrderID";

            using (var db = new TrackerDb())
            {
                db.AddWhereParams(customerId, DbType.Int64);
                db.AddWhereParams(centerLocal.AddDays(-1).Date, DbType.Date);
                db.AddWhereParams(centerLocal.AddDays(1).Date, DbType.Date);

                var ds = db.ReturnDataSet(sql, db.WhereParams);
                if (ds == null || ds.Tables.Count == 0) return new DataTable();
                var table = ds.Tables["objDataSet"] ?? ds.Tables[0];
                return table;
            }
        }

        private DataTable FetchOrderLinesExactWithNotes(long customerId, DateTime requiredDateLocal, string notes)
        {
            const string sql = @"
SELECT OrderID, ItemTypeID, QuantityOrdered, PackagingID, RequiredByDate, Notes
FROM OrderTbl
WHERE CustomerID = ?
  AND RequiredByDate = ?
  AND Notes = ?
ORDER BY OrderID";
            using (var db = new TrackerDb())
            {
                db.AddWhereParams(customerId, DbType.Int64);
                db.AddWhereParams(requiredDateLocal.Date, DbType.Date);
                db.AddWhereParams(notes ?? string.Empty, DbType.String);
                var ds = db.ReturnDataSet(sql, db.WhereParams);
                if (ds == null || ds.Tables.Count == 0) return new DataTable();
                return ds.Tables["objDataSet"] ?? ds.Tables[0];
            }
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

            var dynamicList = new System.Collections.Generic.List<DateTime>();
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
                    var dtN = FetchOrderLinesExactWithNotes(_custId, cand, _queryNotes);
                    tried.AppendLine($"(NOTES) Tried {cand:yyyy-MM-dd} rows={dtN.Rows.Count}");
                    if (dtN.Rows.Count > 0)
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                            $"ViewMyOrder: notes-match {cand:yyyy-MM-dd} tokenUtc={tokenUtc:yyyy-MM-dd}");
                        RenderOrder(dtN, cand);
                        return;
                    }
                }
            }

            foreach (var cand in candidates)
            {
                var dt = FetchOrderLinesExact(_custId, cand);
                tried.AppendLine($"Tried {cand:yyyy-MM-dd} rows={dt.Rows.Count}");
                if (dt.Rows.Count > 0)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                        $"ViewMyOrder: match {cand:yyyy-MM-dd} tokenUtc={tokenUtc:yyyy-MM-dd} queryDate={(_hadQueryDate ? _queryDeliveryDate.ToString("yyyy-MM-dd") : "n/a")}");
                    RenderOrder(dt, cand);
                    return;
                }
            }

            DateTime rangeCenter = _hadQueryDate ? _queryDeliveryDate : tokenAsLocal;
            var rangeDt = FetchOrderLinesRange(_custId, rangeCenter);
            tried.AppendLine($"Range fallback center={rangeCenter:yyyy-MM-dd} rows={rangeDt.Rows.Count}");

            if (rangeDt.Rows.Count > 0)
            {
                DateTime chosen = rangeDt.AsEnumerable()
                    .GroupBy(r => Convert.ToDateTime(r["RequiredByDate"]).Date)
                    .OrderByDescending(g => g.Count())
                    .ThenBy(g => g.Key)
                    .First().Key;

                var final = rangeDt.Select($"RequiredByDate = #{chosen:MM/dd/yyyy}#");
                var finalTable = rangeDt.Clone();
                foreach (var r in final) finalTable.ImportRow(r);

                AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                    $"ViewMyOrder: range fallback chose {chosen:yyyy-MM-dd} tokenUtc={tokenUtc:yyyy-MM-dd}");
                RenderOrder(finalTable, chosen);
                return;
            }

            string qDateStr = _hadQueryDate ? _queryDeliveryDate.ToString("yyyy-MM-dd") : "n/a";
            AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                "ViewMyOrder: NO MATCH. " +
                $"TokenUtc={tokenUtc:yyyy-MM-dd} QueryDate={qDateStr} Attempts:\n{tried}");

            ShowError("No order lines found for that delivery date.");
        }

        private void RenderOrder(DataTable dt, DateTime effectiveDeliveryLocal)
        {
            string customerName;
            try
            {
                customerName = CustomersTbl.GetCustomerNameById(_custId);
            }
            catch
            {
                customerName = null;
            }

            if (string.IsNullOrEmpty(customerName))
            {
                ShowError("Customer not found.");
                return;
            }

            var itemTbl = new ItemTypeTbl();
            var packTbl = new PackagingTbl();

            string sharedNotes = string.Empty;
            var sbRows = new StringBuilder();
            int lineCount = 0;

            foreach (DataRow r in dt.Rows)
            {
                lineCount++;
                int itemTypeId = Convert.ToInt32(r["ItemTypeID"]);
                double qty = Convert.ToDouble(r["QuantityOrdered"]);
                int packagingId = r["PackagingID"] == DBNull.Value ? 0 : Convert.ToInt32(r["PackagingID"]);

                string itemDesc;
                try { itemDesc = ItemTypeTbl.GetItemTypeDescById(itemTypeId); }
                catch { itemDesc = "Item " + itemTypeId; }

                string uom = string.Empty;
                try { uom = itemTbl.GetItemUnitOfMeasure(itemTypeId); } catch { }

                string pkgDesc = "-";
                if (packagingId > 0)
                {
                    try { pkgDesc = packTbl.GetPackagingDesc(packagingId) ?? "-"; } catch { }
                }

                if (string.IsNullOrEmpty(sharedNotes) && r["Notes"] != DBNull.Value)
                    sharedNotes = Convert.ToString(r["Notes"]);

                sbRows.Append("<tr>")
                      .Append("<td>").Append(HttpUtility.HtmlEncode(itemDesc)).Append("</td>")
                      .Append("<td>").Append(HttpUtility.HtmlEncode(pkgDesc)).Append("</td>")
                      .Append("<td>").Append(qty.ToString("0.###")).Append("</td>")
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