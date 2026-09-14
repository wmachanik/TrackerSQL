using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Tools
{
    public partial class DeliveryPromiseRules : Page
    {
        private readonly DeliveryPromiseRuleRepository _repo = new DeliveryPromiseRuleRepository();
        private readonly AreasRepository _areasRepo = new AreasRepository();
        private readonly WooCommerceSettingsManager _settings = new WooCommerceSettingsManager();

        private const string SortExpKey = "DeliveryPromise_SortExp";
        private const string SortDirKey = "DeliveryPromise_SortDir";

        private static readonly string[] DowNames =
        {
            "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"
        };

        private Dictionary<int, string> _areaNameCache;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UserCanManage())
            {
                pnlMain.Visible = false;
                pnlAccessDenied.Visible = true;
                lblAccessDenied.Text = MessageProvider.Get(MessageKeys.SystemPreferences.AccessDenied);
                return;
            }

            pnlAccessDenied.Visible = false;
            _settings.EnsureSchemaOnce();

            if (!IsPostBack)
            {
                BindFilterGroups();
                FillDowDropdown(ddlNewStartDow, 1);
                FillDowDropdown(ddlNewEndDow, 2);
                FillDowDropdown(ddlNewResultDow, 1, includeAnyWorkday: false);
                BindNewAreaDropdown();
                BindGrid();
            }
        }

        private bool UserCanManage()
        {
            var user = Context?.User;
            return user != null && user.Identity != null && user.Identity.IsAuthenticated &&
                   (user.IsInRole("Administrators") || user.IsInRole("Admin"));
        }

        private void SetStatus(string message, bool? isError)
        {
            ltrlStatus.Text = message ?? string.Empty;
            if (pnlStatus == null)
                return;

            if (string.IsNullOrWhiteSpace(message))
            {
                pnlStatus.Attributes["class"] = "status-message";
                return;
            }

            if (isError == true)
                pnlStatus.Attributes["class"] = "status-message status-error";
            else if (isError == false)
                pnlStatus.Attributes["class"] = "status-message status-success";
            else
                pnlStatus.Attributes["class"] = "status-message status-info";
        }

        private string SortExpression
        {
            get => ViewState[SortExpKey] as string ?? "SortOrder";
            set => ViewState[SortExpKey] = value;
        }

        private string SortDirection
        {
            get => ViewState[SortDirKey] as string ?? "ASC";
            set => ViewState[SortDirKey] = value;
        }

        private void BindFilterGroups()
        {
            string selected = ddlFilterGroup.SelectedValue;
            ddlFilterGroup.Items.Clear();
            ddlFilterGroup.Items.Add(new ListItem("(all)", ""));
            foreach (string g in _repo.GetDistinctRuleGroups())
                ddlFilterGroup.Items.Add(new ListItem(g, g));

            if (!string.IsNullOrEmpty(selected) && ddlFilterGroup.Items.FindByValue(selected) != null)
                ddlFilterGroup.SelectedValue = selected;
        }

        private void BindNewAreaDropdown()
        {
            FillAreaDropdown(ddlNewArea, null);
        }

        private void FillAreaDropdown(DropDownList ddl, int? selectedAreaId)
        {
            if (ddl == null)
                return;

            ddl.Items.Clear();
            ddl.Items.Add(new ListItem("(none)", ""));
            foreach (var area in _areasRepo.GetAll("AreaName") ?? new List<Area>())
            {
                if (area == null || area.AreaID <= 0)
                    continue;
                ddl.Items.Add(new ListItem(area.AreaName ?? ("#" + area.AreaID), area.AreaID.ToString()));
            }

            if (selectedAreaId.HasValue && selectedAreaId.Value > 0)
            {
                string v = selectedAreaId.Value.ToString();
                if (ddl.Items.FindByValue(v) != null)
                    ddl.SelectedValue = v;
            }
        }

        private static void FillDowDropdown(DropDownList ddl, byte selected, bool includeAnyWorkday = true)
        {
            if (ddl == null)
                return;

            ddl.Items.Clear();
            for (byte i = 0; i <= 6; i++)
                ddl.Items.Add(new ListItem(DowNames[i], i.ToString(CultureInfo.InvariantCulture)));
            if (includeAnyWorkday)
                ddl.Items.Add(new ListItem("AnyWD", "255"));

            string v = selected.ToString(CultureInfo.InvariantCulture);
            if (ddl.Items.FindByValue(v) != null)
                ddl.SelectedValue = v;
            else if (includeAnyWorkday && selected == 255)
                ddl.SelectedValue = "255";
        }

        private List<DeliveryPromiseRule> GetFilteredRules()
        {
            if (!_repo.TableExists())
                return new List<DeliveryPromiseRule>();

            IEnumerable<DeliveryPromiseRule> rules = _repo.GetAll(includeDisabled: true);
            if (!chkShowDisabled.Checked)
                rules = rules.Where(r => r.Enabled);

            string group = ddlFilterGroup.SelectedValue;
            if (!string.IsNullOrWhiteSpace(group))
                rules = rules.Where(r => string.Equals(r.RuleGroup, group, StringComparison.OrdinalIgnoreCase));

            string q = (txtSearch.Text ?? string.Empty).Trim();
            if (q.Length > 0)
            {
                rules = rules.Where(r =>
                    Contains(r.RuleGroup, q)
                    || Contains(r.AreaMatchName, q)
                    || Contains(r.Notes, q)
                    || Contains(r.PromiseKind, q)
                    || Contains(r.ResultMode, q)
                    || (r.AreaID.HasValue && Contains(ResolveAreaName(r.AreaID.Value), q)));
            }

            return ApplySort(rules).ToList();
        }

        private IEnumerable<DeliveryPromiseRule> ApplySort(IEnumerable<DeliveryPromiseRule> rules)
        {
            bool desc = string.Equals(SortDirection, "DESC", StringComparison.OrdinalIgnoreCase);
            switch (SortExpression)
            {
                case "RuleID":
                    return desc ? rules.OrderByDescending(r => r.RuleID) : rules.OrderBy(r => r.RuleID);
                case "RuleGroup":
                    return desc
                        ? rules.OrderByDescending(r => r.RuleGroup).ThenByDescending(r => r.SortOrder)
                        : rules.OrderBy(r => r.RuleGroup).ThenBy(r => r.SortOrder);
                case "AreaMatchName":
                    return desc
                        ? rules.OrderByDescending(r => r.AreaMatchName).ThenByDescending(r => r.SortOrder)
                        : rules.OrderBy(r => r.AreaMatchName).ThenBy(r => r.SortOrder);
                case "PromiseKind":
                    return desc
                        ? rules.OrderByDescending(r => r.PromiseKind).ThenByDescending(r => r.SortOrder)
                        : rules.OrderBy(r => r.PromiseKind).ThenBy(r => r.SortOrder);
                case "Notes":
                    return desc
                        ? rules.OrderByDescending(r => r.Notes).ThenByDescending(r => r.SortOrder)
                        : rules.OrderBy(r => r.Notes).ThenBy(r => r.SortOrder);
                case "SortOrder":
                default:
                    return desc
                        ? rules.OrderByDescending(r => r.RuleGroup).ThenByDescending(r => r.SortOrder).ThenByDescending(r => r.RuleID)
                        : rules.OrderBy(r => r.RuleGroup).ThenBy(r => r.SortOrder).ThenBy(r => r.RuleID);
            }
        }

        private static bool Contains(string haystack, string needle)
        {
            return !string.IsNullOrEmpty(haystack)
                && haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void BindGrid()
        {
            if (!_repo.TableExists())
            {
                gvRules.DataSource = new List<DeliveryPromiseRule>();
                gvRules.DataBind();
                SetStatus("DeliveryPromiseRuleTbl is missing. Open Woo Mapping once to create schema, or run SQLCommands-DeliveryPromise-01.xml.", true);
                return;
            }

            var list = GetFilteredRules();
            if (gvRules.PageIndex > 0 && gvRules.PageIndex * gvRules.PageSize >= Math.Max(list.Count, 1))
                gvRules.PageIndex = 0;

            gvRules.DataSource = list;
            gvRules.DataBind();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            gvRules.EditIndex = -1;
            gvRules.PageIndex = 0;
            BindGrid();
            SetStatus(string.Empty, null);
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            chkShowDisabled.Checked = false;
            if (ddlFilterGroup.Items.Count > 0)
                ddlFilterGroup.SelectedIndex = 0;
            gvRules.EditIndex = -1;
            gvRules.PageIndex = 0;
            SortExpression = "SortOrder";
            SortDirection = "ASC";
            BindFilterGroups();
            BindGrid();
            SetStatus(string.Empty, null);
        }

        protected void btnShowAddPanel_Click(object sender, EventArgs e)
        {
            pnlAddInline.Visible = true;
            BindNewAreaDropdown();
        }

        protected void btnCancelAdd_Click(object sender, EventArgs e)
        {
            pnlAddInline.Visible = false;
            ClearAddForm();
        }

        private void ClearAddForm()
        {
            txtNewGroup.Text = string.Empty;
            txtNewAreaName.Text = string.Empty;
            if (ddlNewArea.Items.Count > 0) ddlNewArea.SelectedIndex = 0;
            if (ddlNewStartDow.Items.FindByValue("1") != null) ddlNewStartDow.SelectedValue = "1";
            if (ddlNewEndDow.Items.FindByValue("2") != null) ddlNewEndDow.SelectedValue = "2";
            txtNewStartTime.Text = "12:00";
            txtNewEndTime.Text = "12:00";
            ddlNewKind.SelectedIndex = 0;
            ddlNewResultMode.SelectedIndex = 0;
            if (ddlNewResultDow.Items.FindByValue("1") != null) ddlNewResultDow.SelectedValue = "1";
        }

        protected void gvRules_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvRules.EditIndex = -1;
            gvRules.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        protected void gvRules_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (string.Equals(SortExpression, e.SortExpression, StringComparison.OrdinalIgnoreCase))
                SortDirection = string.Equals(SortDirection, "ASC", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
            else
            {
                SortExpression = e.SortExpression;
                SortDirection = "ASC";
            }

            gvRules.EditIndex = -1;
            BindGrid();
        }

        protected void gvRules_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvRules.EditIndex = e.NewEditIndex;
            BindGrid();
        }

        protected void gvRules_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvRules.EditIndex = -1;
            BindGrid();
        }

        protected void gvRules_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            var rule = e.Row.DataItem as DeliveryPromiseRule;
            if (rule == null)
                return;

            if ((e.Row.RowState & DataControlRowState.Edit) != DataControlRowState.Edit)
                return;

            var ddlArea = e.Row.FindControl("ddlArea") as DropDownList;
            FillAreaDropdown(ddlArea, rule.AreaID);

            FillDowDropdown(e.Row.FindControl("ddlStartDow") as DropDownList, rule.WindowStartDow);
            FillDowDropdown(e.Row.FindControl("ddlEndDow") as DropDownList, rule.WindowEndDow);
            FillDowDropdown(e.Row.FindControl("ddlResultDow") as DropDownList, rule.ResultDow ?? (byte)1, includeAnyWorkday: false);

            var ddlKind = e.Row.FindControl("ddlPromiseKind") as DropDownList;
            if (ddlKind != null && !string.IsNullOrEmpty(rule.PromiseKind) && ddlKind.Items.FindByValue(rule.PromiseKind) != null)
                ddlKind.SelectedValue = rule.PromiseKind;

            var ddlMode = e.Row.FindControl("ddlResultMode") as DropDownList;
            if (ddlMode != null && !string.IsNullOrEmpty(rule.ResultMode) && ddlMode.Items.FindByValue(rule.ResultMode) != null)
                ddlMode.SelectedValue = rule.ResultMode;
        }

        protected void gvRules_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int ruleId = Convert.ToInt32(gvRules.DataKeys[e.RowIndex].Value);
            GridViewRow row = gvRules.Rows[e.RowIndex];

            var existing = _repo.GetById(ruleId);
            if (existing == null)
            {
                SetStatus("Rule not found.", true);
                gvRules.EditIndex = -1;
                BindGrid();
                return;
            }

            try
            {
                existing.RuleGroup = GetText(row, "txtRuleGroup");
                existing.AreaMatchName = GetText(row, "txtAreaMatchName");
                existing.SortOrder = ParseInt(GetText(row, "txtSortOrder"), existing.SortOrder);
                existing.AreaID = GetSelectedAreaId(row, "ddlArea");
                existing.WindowStartDow = GetSelectedByte(row, "ddlStartDow", existing.WindowStartDow);
                existing.WindowStartMinutes = ParseTimeMinutes(GetText(row, "txtStartTime"), existing.WindowStartMinutes);
                existing.WindowEndDow = GetSelectedByte(row, "ddlEndDow", existing.WindowEndDow);
                existing.WindowEndMinutes = ParseTimeMinutes(GetText(row, "txtEndTime"), existing.WindowEndMinutes);

                var ddlKind = row.FindControl("ddlPromiseKind") as DropDownList;
                if (ddlKind != null && !string.IsNullOrEmpty(ddlKind.SelectedValue))
                    existing.PromiseKind = ddlKind.SelectedValue;

                var ddlMode = row.FindControl("ddlResultMode") as DropDownList;
                if (ddlMode != null && !string.IsNullOrEmpty(ddlMode.SelectedValue))
                    existing.ResultMode = ddlMode.SelectedValue;

                if (string.Equals(existing.ResultMode, "FixedDow", StringComparison.OrdinalIgnoreCase))
                    existing.ResultDow = GetSelectedByte(row, "ddlResultDow", existing.ResultDow ?? (byte)1);
                else
                    existing.ResultDow = null;

                var chkNoThu = row.FindControl("chkNoThu") as CheckBox;
                var chkWedFri = row.FindControl("chkWedFri") as CheckBox;
                var chkEnabled = row.FindControl("chkEnabled") as CheckBox;
                if (chkNoThu != null) existing.NoThursdayDispatch = chkNoThu.Checked;
                if (chkWedFri != null) existing.WedAfterNoonToFriday = chkWedFri.Checked;
                if (chkEnabled != null) existing.Enabled = chkEnabled.Checked;

                existing.Notes = GetText(row, "txtNotes");

                if (!_repo.Update(existing))
                {
                    SetStatus("Update failed.", true);
                    return;
                }

                gvRules.EditIndex = -1;
                BindFilterGroups();
                BindGrid();
                SetStatus("Rule updated.", false);
            }
            catch (Exception ex)
            {
                SetStatus("Update error: " + ex.Message, true);
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "DeliveryPromiseRules update: " + ex.Message);
            }
        }

        protected void gvRules_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int ruleId = Convert.ToInt32(gvRules.DataKeys[e.RowIndex].Value);
            if (_repo.Delete(ruleId))
            {
                gvRules.EditIndex = -1;
                BindFilterGroups();
                BindGrid();
                SetStatus("Rule deleted.", false);
            }
            else
            {
                SetStatus("Delete failed.", true);
            }
        }

        protected void btnAddRule_Click(object sender, EventArgs e)
        {
            try
            {
                var rule = new DeliveryPromiseRule
                {
                    RuleGroup = (txtNewGroup.Text ?? string.Empty).Trim(),
                    AreaMatchName = (txtNewAreaName.Text ?? string.Empty).Trim(),
                    AreaID = string.IsNullOrEmpty(ddlNewArea.SelectedValue)
                        ? (int?)null
                        : int.Parse(ddlNewArea.SelectedValue, CultureInfo.InvariantCulture),
                    SortOrder = 100,
                    WindowStartDow = ParseByte(ddlNewStartDow.SelectedValue, 1),
                    WindowStartMinutes = ParseTimeMinutes(txtNewStartTime.Text, 12 * 60),
                    WindowEndDow = ParseByte(ddlNewEndDow.SelectedValue, 2),
                    WindowEndMinutes = ParseTimeMinutes(txtNewEndTime.Text, 12 * 60),
                    PromiseKind = ddlNewKind.SelectedValue,
                    ResultMode = ddlNewResultMode.SelectedValue,
                    ResultDow = string.Equals(ddlNewResultMode.SelectedValue, "FixedDow", StringComparison.OrdinalIgnoreCase)
                        ? ParseByte(ddlNewResultDow.SelectedValue, 1)
                        : (byte?)null,
                    Enabled = true,
                    Notes = null
                };

                if (string.IsNullOrWhiteSpace(rule.RuleGroup))
                {
                    SetStatus("Group is required.", true);
                    return;
                }

                if (string.IsNullOrWhiteSpace(rule.AreaMatchName) && !string.IsNullOrEmpty(ddlNewArea.SelectedItem?.Text)
                    && ddlNewArea.SelectedValue != "")
                {
                    rule.AreaMatchName = ddlNewArea.SelectedItem.Text;
                }

                if (string.IsNullOrWhiteSpace(rule.AreaMatchName))
                {
                    SetStatus("Area name is required.", true);
                    return;
                }

                int id = _repo.Insert(rule);
                if (id <= 0)
                {
                    SetStatus("Insert failed.", true);
                    return;
                }

                ClearAddForm();
                pnlAddInline.Visible = false;
                BindFilterGroups();
                BindGrid();
                SetStatus("Rule added (ID " + id + ").", false);
            }
            catch (Exception ex)
            {
                SetStatus("Add error: " + ex.Message, true);
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "DeliveryPromiseRules add: " + ex.Message);
            }
        }

        protected string FormatAreaDisplay(object dataItem)
        {
            var rule = dataItem as DeliveryPromiseRule;
            if (rule == null)
                return "—";

            string match = rule.AreaMatchName;
            string resolved = rule.AreaID.HasValue ? ResolveAreaName(rule.AreaID.Value) : null;

            if (!string.IsNullOrWhiteSpace(resolved)
                && !string.IsNullOrWhiteSpace(match)
                && !string.Equals(resolved, match, StringComparison.OrdinalIgnoreCase))
                return resolved + " · " + match;

            if (!string.IsNullOrWhiteSpace(resolved))
                return resolved;
            if (!string.IsNullOrWhiteSpace(match))
                return match;
            return "—";
        }

        protected string FormatWindowRange(object dataItem)
        {
            var rule = dataItem as DeliveryPromiseRule;
            if (rule == null)
                return "—";

            return FormatDow(rule.WindowStartDow) + " " + FormatTime(rule.WindowStartMinutes)
                + " – "
                + FormatDow(rule.WindowEndDow) + " " + FormatTime(rule.WindowEndMinutes);
        }

        protected string FormatPromise(object dataItem)
        {
            var rule = dataItem as DeliveryPromiseRule;
            if (rule == null)
                return "—";

            string kind = string.IsNullOrWhiteSpace(rule.PromiseKind) ? "Delivery" : rule.PromiseKind;
            string mode = rule.ResultMode ?? string.Empty;

            if (string.Equals(mode, "FixedDow", StringComparison.OrdinalIgnoreCase))
                return "→ " + FormatDow(rule.ResultDow ?? 0) + " " + kind;
            if (string.Equals(mode, "SameWorkday", StringComparison.OrdinalIgnoreCase))
                return "→ same workday " + kind;
            if (string.Equals(mode, "NextWorkday", StringComparison.OrdinalIgnoreCase))
                return "→ next workday " + kind;
            return kind + (string.IsNullOrEmpty(mode) ? string.Empty : " (" + mode + ")");
        }

        protected string FormatTime(object minutesObj)
        {
            if (minutesObj == null || minutesObj == DBNull.Value)
                return "00:00";
            short minutes = Convert.ToInt16(minutesObj);
            return FormatTime(minutes);
        }

        private static string FormatTime(short minutes)
        {
            if (minutes < 0) minutes = 0;
            if (minutes > 24 * 60) minutes = (short)(24 * 60);
            int h = minutes / 60;
            int m = minutes % 60;
            return h.ToString("00", CultureInfo.InvariantCulture) + ":" + m.ToString("00", CultureInfo.InvariantCulture);
        }

        protected string FormatFlags(object noThuObj, object wedFriObj, object enabledObj)
        {
            var parts = new List<string>();
            if (noThuObj != null && Convert.ToBoolean(noThuObj)) parts.Add("NoThu");
            if (wedFriObj != null && Convert.ToBoolean(wedFriObj)) parts.Add("Wed→Fri");
            if (enabledObj == null || !Convert.ToBoolean(enabledObj)) parts.Add("Off");
            return parts.Count == 0 ? "—" : string.Join(", ", parts);
        }

        private string ResolveAreaName(int areaId)
        {
            if (areaId <= 0)
                return null;
            if (_areaNameCache == null)
            {
                _areaNameCache = new Dictionary<int, string>();
                foreach (var a in _areasRepo.GetAll("AreaName") ?? new List<Area>())
                {
                    if (a != null && a.AreaID > 0 && !_areaNameCache.ContainsKey(a.AreaID))
                        _areaNameCache[a.AreaID] = a.AreaName;
                }
            }
            return _areaNameCache.TryGetValue(areaId, out string name) ? name : null;
        }

        private static string FormatDow(byte dow)
        {
            if (dow == 255) return "AnyWD";
            if (dow <= 6) return DowNames[dow];
            return dow.ToString(CultureInfo.InvariantCulture);
        }

        private static string GetText(Control row, string id)
        {
            var tb = row.FindControl(id) as TextBox;
            return tb == null ? string.Empty : (tb.Text ?? string.Empty).Trim();
        }

        private static int? GetSelectedAreaId(Control row, string id)
        {
            var ddl = row.FindControl(id) as DropDownList;
            if (ddl == null || string.IsNullOrEmpty(ddl.SelectedValue))
                return null;
            if (int.TryParse(ddl.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int idVal) && idVal > 0)
                return idVal;
            return null;
        }

        private static byte GetSelectedByte(Control row, string id, byte fallback)
        {
            var ddl = row.FindControl(id) as DropDownList;
            if (ddl == null || string.IsNullOrEmpty(ddl.SelectedValue))
                return fallback;
            return ParseByte(ddl.SelectedValue, fallback);
        }

        private static int ParseInt(string text, int fallback)
        {
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : fallback;
        }

        private static byte ParseByte(string text, byte fallback)
        {
            return byte.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte v) ? v : fallback;
        }

        private static short ParseTimeMinutes(string text, short fallback)
        {
            if (string.IsNullOrWhiteSpace(text))
                return fallback;

            text = text.Trim();
            if (text.Contains(":"))
            {
                string[] parts = text.Split(':');
                if (parts.Length >= 2
                    && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int h)
                    && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int m)
                    && h >= 0 && h <= 24 && m >= 0 && m < 60)
                {
                    int total = h * 60 + m;
                    if (total > 24 * 60) total = 24 * 60;
                    return (short)total;
                }
            }

            if (short.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out short minutes))
                return minutes;

            return fallback;
        }
    }
}
