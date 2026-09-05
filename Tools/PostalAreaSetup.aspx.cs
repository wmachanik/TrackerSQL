using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;

namespace TrackerSQL.Tools
{
    public partial class PostalAreaSetup : Page
    {
        private readonly SaPostalCodeManager _postalManager = new SaPostalCodeManager();
        private readonly PostalAreaSetupManager _setup = new PostalAreaSetupManager();
        private readonly WooCommerceAreaMappingManager _areaMapping = new WooCommerceAreaMappingManager();
        private List<ListItem> _personItems;

        private const string VsWorkingRows = "PostalSetup.WorkingRows";
        private const string VsSuggested = "PostalSetup.Suggested";
        private const string VsGapGroups = "PostalSetup.GapGroups";
        private const string VsAreaFilter = "PostalSetup.AreaFilter";
        private const string VsOriginal = "PostalSetup.Original";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UserCanManage())
            {
                pnlMain.Visible = false;
                pnlAccessDenied.Visible = true;
                lblAccessDenied.Text = MessageProvider.Get(MessageKeys.SystemPreferences.AccessDenied);
                return;
            }

            if (!IsPostBack)
            {
                EnsureAllSchema();
                BindPersonHint();
                BindCatchAll();
                ReloadWorkingRowsFromDb();
                BindAreasGrid();
                RefreshRowCount();
            }
        }

        private void EnsureAllSchema()
        {
            new PostalSchemaInstaller().EnsureSchema();
            new WooCommerceSettingsManager().EnsureSchema();
        }

        private bool UserCanManage()
        {
            var user = Context?.User;
            return user != null && user.Identity != null && user.Identity.IsAuthenticated &&
                   (user.IsInRole("Administrators") || user.IsInRole("Admin"));
        }

        private void BindPersonHint()
        {
            int id = WooCommerceAreaMappingManager.GetSystemDefaultDeliveryPersonId();
            litPersonHint.Text = "Cape Town (Vehicle) defaults to system delivery person ID "
                + id + " (SystemConstants). Courier areas prefer Cour / FastWay / RegionalSA; Pargo uses Prgo.";
        }

        private void BindCatchAll()
        {
            ddlCatchAllArea.Items.Clear();
            ddlCatchAllArea.Items.Add(new ListItem("(none — leave unmatched)", "0"));
            foreach (var area in _areaMapping.GetAreaDeliveryDefaults() ?? new List<WooAreaDeliveryDefault>())
            {
                ddlCatchAllArea.Items.Add(new ListItem(
                    area.AreaName ?? ("#" + area.AreaID),
                    area.AreaID.ToString(CultureInfo.InvariantCulture)));
            }
            int? def = _areaMapping.GetDefaultImportAreaId();
            string val = def.HasValue && def.Value > 0
                ? def.Value.ToString(CultureInfo.InvariantCulture)
                : "0";
            var item = ddlCatchAllArea.Items.FindByValue(val);
            if (item != null)
                item.Selected = true;
            else
                ddlCatchAllArea.SelectedIndex = 0;
        }

        protected void btnSaveCatchAll_Click(object sender, EventArgs e)
        {
            int areaId = ParseInt(ddlCatchAllArea.SelectedValue);
            _areaMapping.SaveDefaultImportArea(areaId > 0 ? areaId : (int?)null, UserName());
            BindCatchAll();
            SetStatus(areaId > 0
                ? "Catch-all area saved. Unmapped postcodes resolve to that area."
                : "Catch-all cleared. Unmapped postcodes will not auto-assign an area.", false);
            btnSaveCatchAll.Attributes["data-woo-save-ready"] = "0";
        }

        private void RefreshRowCount()
        {
            int n = _postalManager.GetReferenceRowCount();
            litRowCount.Text = n > 0
                ? " Table has <strong>" + n + "</strong> rows."
                : " Table is empty — click Import CSV.";
        }

        private void ReloadWorkingRowsFromDb()
        {
            var rows = _setup.GetGridRows();
            ViewState[VsWorkingRows] = rows;
            ViewState[VsSuggested] = rows.ToDictionary(r => r.AreaID, r => r.SuggestedRanges ?? string.Empty);
            SnapshotOriginal(rows);
        }

        private void SnapshotOriginal(List<PostalAreaSetupRow> rows)
        {
            ViewState[VsOriginal] = SnapshotKey(rows);
        }

        private static string SnapshotKey(IList<PostalAreaSetupRow> rows)
        {
            if (rows == null)
                return string.Empty;
            return string.Join("\n", rows.OrderBy(r => r.AreaID).Select(r =>
                r.AreaID + "|"
                + (r.DefaultPreferredAgentID ?? 0).ToString(CultureInfo.InvariantCulture) + "|"
                + NormalizeRanges(r.PostalRanges)));
        }

        private static string NormalizeRanges(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            return text.Trim().Replace("\r\n", "\n").Replace("\r", "\n");
        }

        private void RefreshSaveReady()
        {
            string original = ViewState[VsOriginal] as string ?? string.Empty;
            bool dirty = !string.Equals(SnapshotKey(GetWorkingRows()), original, StringComparison.Ordinal);
            btnSaveAreas.Attributes["data-woo-save-ready"] = dirty ? "1" : "0";
        }

        private List<PostalAreaSetupRow> GetWorkingRows()
        {
            return ViewState[VsWorkingRows] as List<PostalAreaSetupRow>
                ?? new List<PostalAreaSetupRow>();
        }

        private void MergeCurrentPageEdits()
        {
            var working = GetWorkingRows();
            var byId = working.ToDictionary(r => r.AreaID);
            foreach (GridViewRow row in gvAreas.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow)
                    continue;
                int areaId = Convert.ToInt32(gvAreas.DataKeys[row.RowIndex].Value, CultureInfo.InvariantCulture);
                if (!byId.TryGetValue(areaId, out var target))
                    continue;
                var ddl = (DropDownList)row.FindControl("ddlPerson");
                var txt = (TextBox)row.FindControl("txtRanges");
                target.DefaultPreferredAgentID = ParseInt(ddl != null ? ddl.SelectedValue : null);
                target.PostalRanges = txt != null ? txt.Text : null;
                target.Status = string.IsNullOrWhiteSpace(target.PostalRanges) ? "Empty" : "Mapped";
            }
            ViewState[VsWorkingRows] = working;
        }

        private void BindAreasGrid()
        {
            _personItems = null;
            var working = GetWorkingRows();
            string filter = (ViewState[VsAreaFilter] as string) ?? string.Empty;
            txtAreaFilter.Text = filter;
            IEnumerable<PostalAreaSetupRow> query = working;
            if (!string.IsNullOrWhiteSpace(filter))
            {
                string f = filter.Trim();
                query = working.Where(r =>
                    !string.IsNullOrWhiteSpace(r.AreaName)
                    && r.AreaName.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            var list = query.ToList();
            if (gvAreas.PageIndex > 0 && gvAreas.PageIndex * gvAreas.PageSize >= Math.Max(list.Count, 1))
                gvAreas.PageIndex = 0;
            gvAreas.DataSource = list;
            gvAreas.DataBind();
            RefreshSaveReady();
        }

        protected void gvAreas_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvAreas, e.Row);
        }

        protected void gvAreas_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            MergeCurrentPageEdits();
            gvAreas.PageIndex = e.NewPageIndex;
            BindAreasGrid();
        }

        protected void btnAreaFilter_Click(object sender, EventArgs e)
        {
            MergeCurrentPageEdits();
            ViewState[VsAreaFilter] = (txtAreaFilter.Text ?? string.Empty).Trim();
            gvAreas.PageIndex = 0;
            BindAreasGrid();
        }

        protected void btnAreaFilterClear_Click(object sender, EventArgs e)
        {
            MergeCurrentPageEdits();
            ViewState[VsAreaFilter] = string.Empty;
            txtAreaFilter.Text = string.Empty;
            gvAreas.PageIndex = 0;
            BindAreasGrid();
        }

        protected void gvAreas_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;
            var row = e.Row.DataItem as PostalAreaSetupRow;
            if (row == null)
                return;

            PopulatePerson((DropDownList)e.Row.FindControl("ddlPerson"), row.DefaultPreferredAgentID);

            var lit = (Literal)e.Row.FindControl("litSuggested");
            var btn = (LinkButton)e.Row.FindControl("btnUseSuggested");
            if (lit != null)
            {
                if (row.SuggestedCodeCount > 0)
                {
                    lit.Text = "<span class=\"woo-map-section-note\">"
                        + row.SuggestedCodeCount + " code(s)<br/>"
                        + Server.HtmlEncode(Truncate(row.SuggestedRanges, 120));
                    if (!string.IsNullOrWhiteSpace(row.SuggestedMatchNote))
                        lit.Text += "<br/>" + Server.HtmlEncode(row.SuggestedMatchNote);
                    lit.Text += "</span>";
                    if (btn != null)
                        btn.Visible = true;
                }
                else
                    lit.Text = "<span class=\"woo-map-section-note\">—</span>";
            }
        }

        protected void gvAreas_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "UseSuggested", StringComparison.OrdinalIgnoreCase))
                return;
            MergeCurrentPageEdits();
            int areaId = ParseInt(e.CommandArgument as string);
            if (areaId <= 0)
                return;

            var suggested = ViewState[VsSuggested] as Dictionary<int, string>;
            string text = null;
            if (suggested != null)
                suggested.TryGetValue(areaId, out text);
            if (string.IsNullOrWhiteSpace(text))
            {
                SetStatus("No suggestion for that area.", true);
                return;
            }

            var working = GetWorkingRows();
            var row = working.FirstOrDefault(r => r.AreaID == areaId);
            if (row != null)
            {
                row.PostalRanges = text;
                row.Status = "Mapped";
            }
            ViewState[VsWorkingRows] = working;
            _setup.SaveGrid(working, UserName());
            ReloadWorkingRowsFromDb();
            BindAreasGrid();
            SetStatus("Applied suggested ranges to area #" + areaId + ".", false);
        }

        protected void btnSaveAreas_Click(object sender, EventArgs e)
        {
            MergeCurrentPageEdits();
            int n = _setup.SaveGrid(GetWorkingRows(), UserName());
            ReloadWorkingRowsFromDb();
            BindAreasGrid();
            SetStatus("Saved " + n + " area(s).", false);
        }

        protected void btnFillSuggestions_Click(object sender, EventArgs e)
        {
            MergeCurrentPageEdits();
            var rows = GetWorkingRows();
            int filled = _setup.FillEmptyFromSuggestions(rows, UserName());
            ReloadWorkingRowsFromDb();
            BindAreasGrid();
            SetStatus("Filled " + filled + " empty area(s) from suggestions.", false);
        }

        protected void btnRefreshGrid_Click(object sender, EventArgs e)
        {
            ReloadWorkingRowsFromDb();
            BindAreasGrid();
            SetStatus("Grid refreshed from database.", false);
        }

        protected void btnGapScan_Click(object sender, EventArgs e)
        {
            MergeCurrentPageEdits();
            var analysis = _setup.AnalyseGaps();
            litGapSummary.Text = BuildGapSummaryHtml(analysis);
            ViewState[VsGapGroups] = analysis.Groups ?? new List<PostalGapGroup>();
            gvGaps.DataSource = analysis.Groups;
            gvGaps.DataBind();
            gvConflicts.DataSource = analysis.Conflicts ?? new List<PostalConflictGroup>();
            gvConflicts.DataBind();
        }

        private string BuildGapSummaryHtml(PostalGapAnalysis analysis)
        {
            if (analysis == null)
                return string.Empty;
            int onlyOne = analysis.MappedCodes - analysis.OverlapCodes;
            if (onlyOne < 0)
                onlyOne = 0;
            var sb = new System.Text.StringBuilder();
            sb.Append("<div class=\"woo-map-section-note\"><ul>");
            sb.Append("<li>The SA table has <strong>")
                .Append(analysis.ReferenceCodes)
                .Append("</strong> postcodes.</li>");
            sb.Append("<li><strong>").Append(onlyOne)
                .Append("</strong> are in exactly one area.</li>");
            sb.Append("<li><strong>").Append(analysis.OverlapCodes)
                .Append("</strong> are in two or more areas (see Conflicts).</li>");
            sb.Append("<li><strong>").Append(analysis.UnmappedCodes)
                .Append("</strong> are in no area");
            if (analysis.CatchAllLeftovers > 0 && !string.IsNullOrWhiteSpace(analysis.CatchAllAreaName))
            {
                sb.Append(" — all of those use catch-all <strong>")
                    .Append(Server.HtmlEncode(analysis.CatchAllAreaName))
                    .Append("</strong>, so they are not listed as gaps");
            }
            sb.Append(".</li></ul></div>");
            return sb.ToString();
        }

        protected void gvGaps_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "AssignGap", StringComparison.OrdinalIgnoreCase))
                return;
            int areaId = ParseInt(e.CommandArgument as string);
            var groups = ViewState[VsGapGroups] as List<PostalGapGroup>;
            if (groups == null)
            {
                SetStatus("Re-run Scan for gaps first.", true);
                return;
            }
            var group = groups.FirstOrDefault(g => g.SuggestedAreaID == areaId);
            if (group == null || string.IsNullOrWhiteSpace(group.RangeText))
            {
                SetStatus("Nothing to assign for that group.", true);
                return;
            }
            _setup.AssignGapGroup(areaId, group.RangeText, UserName());
            ReloadWorkingRowsFromDb();
            BindAreasGrid();
            btnGapScan_Click(sender, e);
            SetStatus("Merged " + group.CodeCount + " postcode(s) into " + group.SuggestedAreaName + ".", false);
        }

        protected void btnEnsureSchema_Click(object sender, EventArgs e)
        {
            EnsureAllSchema();
            var postal = new PostalSchemaInstaller().EnsureSchema();
            var woo = new WooCommerceSettingsManager().EnsureSchema();
            SetStatus(postal.Message + " " + woo.Message, !(postal.Succeeded && woo.Succeeded));
            RefreshRowCount();
        }

        protected void btnImportCsv_Click(object sender, EventArgs e)
        {
            var result = _postalManager.ImportFromDefaultCsv(UserName());
            SetStatus(result.Message, !result.Succeeded);
            RefreshRowCount();
            ReloadWorkingRowsFromDb();
            BindAreasGrid();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            gvSearch.DataSource = _postalManager.Search(txtSearch.Text);
            gvSearch.DataBind();
        }

        private void PopulatePerson(DropDownList ddl, int? selectedId)
        {
            if (ddl == null)
                return;
            ddl.Items.Clear();
            foreach (var li in GetPersonItems())
                ddl.Items.Add(new ListItem(li.Text, li.Value));
            if (selectedId.HasValue && selectedId.Value > 0)
            {
                var item = ddl.Items.FindByValue(selectedId.Value.ToString(CultureInfo.InvariantCulture));
                if (item != null)
                    item.Selected = true;
            }
        }

        private List<ListItem> GetPersonItems()
        {
            if (_personItems != null)
                return _personItems;
            _personItems = new List<ListItem>();
            foreach (var person in _setup.GetDeliveryPersons())
            {
                string label = !string.IsNullOrWhiteSpace(person.Abbreviation)
                    ? person.Abbreviation
                    : person.PersonName;
                if (string.IsNullOrWhiteSpace(label))
                    label = person.PersonID.ToString(CultureInfo.InvariantCulture);
                _personItems.Add(new ListItem(label, person.PersonID.ToString(CultureInfo.InvariantCulture)));
            }
            return _personItems;
        }

        private static string Truncate(string text, int max)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= max)
                return text ?? string.Empty;
            return text.Substring(0, max) + "…";
        }

        private static int ParseInt(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;
            return int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) ? n : 0;
        }

        private string UserName()
        {
            return Context?.User?.Identity?.Name ?? "system";
        }

        private void SetStatus(string message, bool isError)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                lblMessage.Visible = false;
                return;
            }
            lblMessage.Visible = true;
            lblMessage.Text = message;
            lblMessage.CssClass = "status-message " + (isError ? "status-error" : "status-success");
        }
    }
}
