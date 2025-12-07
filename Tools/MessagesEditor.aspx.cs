using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using System.Text.RegularExpressions;
using HtmlAgilityPack; // add this
using System.IO;       // for StringWriter

namespace TrackerSQL.Tools
{
    public partial class MessagesEditor : System.Web.UI.Page
    {
        private readonly MessagesResourceManager _manager = new MessagesResourceManager();
        private const string VIEWSTATE_DATA_KEY = "MessagesData";
        private const string VIEWSTATE_FILTER = "MessagesFilter";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!IsUserAdmin())
                {
                    DenyAccess();
                    return;
                }

                ResultsTitleLabel.Text = "Messages (Global Resource Editor)";
                pnlEditor.Visible = true;
                LoadAllData();
                BindGrid();
            }
        }

        private bool IsUserAdmin()
        {
            try
            {
                var user = HttpContext.Current?.User;
                if (user == null) return false;
                return user.IsInRole("Administrators") || user.IsInRole("Admin") || user.IsInRole("AgentManager");
            }
            catch
            {
                return false;
            }
        }

        private void DenyAccess()
        {
            pnlAccessDenied.Visible = true;
            pnlEditor.Visible = false;
            AppLogger.WriteLog(SystemConstants.LogTypes.System,
                "MessagesEditor: Access denied for user " + (HttpContext.Current?.User?.Identity?.Name ?? "unknown"));
        }

        private Dictionary<string, string> AllData
        {
            get { return (Dictionary<string, string>)ViewState[VIEWSTATE_DATA_KEY]; }
            set { ViewState[VIEWSTATE_DATA_KEY] = value; }
        }

        private string CurrentFilter
        {
            get { return (string)ViewState[VIEWSTATE_FILTER] ?? string.Empty; }
            set { ViewState[VIEWSTATE_FILTER] = value; }
        }
        private void LoadAllData()
        {
            try
            {
                // Use unified manager effective values
                AllData = _manager.LoadEffective();
                ltrlStatus.Text = "Loaded " + AllData.Count + " messages.";
            }
            catch (Exception ex)
            {
                ltrlStatus.Text = "Error loading messages: " + ex.Message;
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "MessagesEditor LoadAllData error: " + ex.Message);
                AllData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }
        private IEnumerable<KeyValuePair<string, string>> GetFiltered()
        {
            if (AllData == null)
                return Enumerable.Empty<KeyValuePair<string, string>>();

            if (string.IsNullOrWhiteSpace(CurrentFilter))
                return AllData.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase);

            string f = CurrentFilter.Trim();
            return AllData
                .Where(kv =>
                    kv.Key.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (kv.Value ?? string.Empty).IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase);
        }

        private void BindGrid()
        {
            // DO NOT reset EditIndex here – preserve whatever RowEditing set.
            gvMessages.DataSource = GetFiltered()
                .Select(kv => new { Key = kv.Key, Value = kv.Value })
                .ToList();

            gvMessages.DataBind();

            int count = GetFiltered().Count();
            ltrlStatus.Text = string.IsNullOrWhiteSpace(CurrentFilter)
                ? "Showing " + count + " messages."
                : "Filter '" + CurrentFilter + "' matched " + count + " messages.";

            upnlMessagesEditor.Update();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            CurrentFilter = tbxSearch.Text;
            gvMessages.PageIndex = 0;
            gvMessages.EditIndex = -1;
            BindGrid();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            tbxSearch.Text = string.Empty;
            CurrentFilter = string.Empty;
            gvMessages.PageIndex = 0;
            gvMessages.EditIndex = -1;
            BindGrid();
        }

        protected void gvMessages_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvMessages.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        protected void gvMessages_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvMessages.EditIndex = e.NewEditIndex;
            BindGrid();
        }

        protected void gvMessages_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvMessages.EditIndex = -1;
            BindGrid();
        }

        // Whitelists for safe HTML (expanded to support your examples)
        private static readonly HashSet<string> AllowedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "p","br","a","strong","em","b","i","u","ul","ol","li",
            "h2","div","span",
            "table","thead","tbody","tfoot","tr","td","th"
        };

        // Allowed attributes per tag (style validated separately; href validated)
        private static readonly Dictionary<string, HashSet<string>> AllowedAttributes =
            new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "a",     new HashSet<string>(StringComparer.OrdinalIgnoreCase){ "href","title","target","style" } },
            { "p",     new HashSet<string>(StringComparer.OrdinalIgnoreCase){ "style" } },
            { "h2",    new HashSet<string>(StringComparer.OrdinalIgnoreCase){ "style" } },
            { "div",   new HashSet<string>(StringComparer.OrdinalIgnoreCase){ "style" } },
            { "span",  new HashSet<string>(StringComparer.OrdinalIgnoreCase){ "style" } },
            { "ul",    new HashSet<string>(StringComparer.OrdinalIgnoreCase){ "style" } },
            { "ol",    new HashSet<string>(StringComparer.OrdinalIgnoreCase){ "style" } },
            { "li",    new HashSet<string>(StringComparer.OrdinalIgnoreCase){ "style" } },
            { "table", new HashSet<string>(StringComparer.OrdinalIgnoreCase){ "style","cellpadding","cellspacing" } },
            { "tr",    new HashSet<string>(StringComparer.OrdinalIgnoreCase){ "style" } },
            { "td",    new HashSet<string>(StringComparer.OrdinalIgnoreCase){ "style","colspan","rowspan" } },
            { "th",    new HashSet<string>(StringComparer.OrdinalIgnoreCase){ "style","colspan","rowspan" } }
        };

        private static bool IsSafeHref(string href)
        {
            if (string.IsNullOrWhiteSpace(href)) return false;
            href = href.Trim();

            // Allow relative URLs
            if (Uri.TryCreate(href, UriKind.Relative, out _)) return true;

            // Allow only http/https/mailto absolute URLs
            if (Uri.TryCreate(href, UriKind.Absolute, out var uri))
                return uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase)
                    || uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)
                    || uri.Scheme.Equals("mailto", StringComparison.OrdinalIgnoreCase);

            return false;
        }

        private static bool IsDigits(string s) => !string.IsNullOrWhiteSpace(s) && Regex.IsMatch(s.Trim(), @"^\d+$");

        // px lengths: allow "12px" or up to four values like "10px 0 10px 0"
        private static bool IsPxLengths(string value)
        {
            var parts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0 || parts.Length > 4) return false;
            return parts.All(p => Regex.IsMatch(p, @"^\d{1,3}px$", RegexOptions.IgnoreCase));
        }

        private static bool IsBorderSpec(string value)
        {
            // e.g. "1px solid #ccc"
            var m = Regex.Match(value.Trim(), @"^(?<w>\d{1,3}px)\s+(?<s>solid|dashed|dotted)\s+(?<c>#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})|[a-zA-Z]+)$",
                RegexOptions.IgnoreCase);
            return m.Success;
        }

        private static bool IsColor(string value)
            => Regex.IsMatch(value.Trim(), @"^(#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})|[a-zA-Z]+)$");

        private static bool IsFontWeight(string value)
            => Regex.IsMatch(value.Trim(), @"^(normal|bold|[1-9]00)$", RegexOptions.IgnoreCase);

        private static bool IsFontSize(string value)
            => Regex.IsMatch(value.Trim(), @"^\d{1,2}px$", RegexOptions.IgnoreCase); // simple and safe

        private static bool IsTextAlign(string value)
            => Regex.IsMatch(value.Trim(), @"^(left|right|center|justify)$", RegexOptions.IgnoreCase);

        private static bool IsBorderCollapse(string value)
            => Regex.IsMatch(value.Trim(), @"^(collapse|separate)$", RegexOptions.IgnoreCase);

        private static readonly HashSet<string> AllowedFontFamilies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "Arial","Helvetica","Tahoma","Verdana","Georgia","Times New Roman","sans-serif","serif" };

        private static bool IsFontFamily(string value)
        {
            // Comma-separated list; ensure each is in the allowlist (quotes optional)
            var parts = value.Split(',').Select(p => p.Trim().Trim('\'','"'));
            return parts.All(p => AllowedFontFamilies.Contains(p));
        }

        // Strict CSS allowlist for inline styles
        private static bool IsSafeStyle(string style)
        {
            if (string.IsNullOrWhiteSpace(style)) return false;

            var decls = style.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var rawDecl in decls)
            {
                var parts = rawDecl.Split(new[] { ':' }, 2);
                if (parts.Length != 2) return false;

                var name = parts[0].Trim().ToLowerInvariant();
                var val = parts[1].Trim();

                switch (name)
                {
                    case "color":
                    case "background-color":
                        if (!IsColor(val)) return false;
                        break;

                    case "font-weight":
                        if (!IsFontWeight(val)) return false;
                        break;

                    case "font-size":
                        if (!IsFontSize(val)) return false;
                        break;

                    case "text-align":
                        if (!IsTextAlign(val)) return false;
                        break;

                    case "border":
                    case "border-top":
                    case "border-bottom":
                    case "border-left":
                    case "border-right":
                        if (!IsBorderSpec(val)) return false;
                        break;

                    case "border-radius":
                        if (!Regex.IsMatch(val, @"^\d{1,3}px$", RegexOptions.IgnoreCase)) return false;
                        break;

                    case "padding":
                    case "padding-top":
                    case "padding-right":
                    case "padding-bottom":
                    case "padding-left":
                    case "margin":
                    case "margin-top":
                    case "margin-right":
                    case "margin-bottom":
                    case "margin-left":
                        if (!IsPxLengths(val)) return false;
                        break;

                    case "border-collapse":
                        if (!IsBorderCollapse(val)) return false;
                        break;

                    case "font-family":
                        if (!IsFontFamily(val)) return false;
                        break;

                    default:
                        // Any other CSS property is disallowed
                        return false;
                }
            }
            return true;
        }

        private static string SanitizeMessage(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var doc = new HtmlDocument
            {
                OptionFixNestedTags = true,
                OptionWriteEmptyNodes = true
            };
            doc.LoadHtml(input);

            // Remove script/style entirely
            foreach (var node in doc.DocumentNode.SelectNodes("//script|//style") ?? Enumerable.Empty<HtmlNode>())
                node.Remove();

            // Walk nodes and sanitize
            SanitizeNode(doc.DocumentNode);

            using (var sw = new StringWriter())
            {
                doc.Save(sw);
                var html = sw.ToString();

                // Normalize <br> to <br />
                html = Regex.Replace(html, @"<br(?=[^>]*?)>", "<br />", RegexOptions.IgnoreCase);

                var root = doc.DocumentNode;
                return root.InnerHtml.Trim();
            }
        }

        private static void SanitizeNode(HtmlNode node)
        {
            if (node.NodeType == HtmlNodeType.Element)
            {
                var name = node.Name;

                if (!AllowedTags.Contains(name))
                {
                    if (node.ParentNode != null)
                    {
                        for (int i = node.ChildNodes.Count - 1; i >= 0; i--)
                        {
                            var child = node.ChildNodes[i];
                            node.ParentNode.InsertAfter(child, node);
                        }
                        node.Remove();
                        return;
                    }
                }
                else
                {
                    var allowed = AllowedAttributes.ContainsKey(name) ? AllowedAttributes[name] : null;

                    foreach (var attr in node.Attributes.ToList())
                    {
                        bool keep = allowed != null && allowed.Contains(attr.Name);

                        // Special handling for <a href>
                        if (keep && name.Equals("a", StringComparison.OrdinalIgnoreCase) &&
                            attr.Name.Equals("href", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!IsSafeHref(attr.Value)) keep = false;
                        }

                        // Validate style on allowed tags
                        if (keep && attr.Name.Equals("style", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!IsSafeStyle(attr.Value)) keep = false;
                        }

                        // Validate numeric attributes
                        if (keep && (attr.Name.Equals("colspan", StringComparison.OrdinalIgnoreCase) ||
                                     attr.Name.Equals("rowspan", StringComparison.OrdinalIgnoreCase) ||
                                     attr.Name.Equals("cellpadding", StringComparison.OrdinalIgnoreCase) ||
                                     attr.Name.Equals("cellspacing", StringComparison.OrdinalIgnoreCase)))
                        {
                            if (!IsDigits(attr.Value)) keep = false;
                        }

                        // Drop event handlers, javascript:*, data:* attributes defensively
                        if (attr.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase))
                            keep = false;

                        if (!keep)
                            node.Attributes.Remove(attr);
                    }
                }
            }

            foreach (var child in node.ChildNodes.ToList())
                SanitizeNode(child);
        }

        protected void gvMessages_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            if (!IsUserAdmin()) { DenyAccess(); return; }

            string key = gvMessages.DataKeys[e.RowIndex].Value.ToString();
            GridViewRow row = gvMessages.Rows[e.RowIndex];
            var txt = (TextBox)row.FindControl("txtEditValue");

            string rawVal;
            if (txt != null)
            {
                try
                {
                    rawVal = Request.Unvalidated[txt.UniqueID] ?? txt.Text; // keep raw HTML
                }
                catch
                {
                    rawVal = txt.Text;
                }
            }
            else
            {
                rawVal = string.Empty;
            }

            string newVal = SanitizeMessage(rawVal);

            try
            {
                _manager.Update(key, newVal);
                LoadAllData();
                gvMessages.EditIndex = -1;
                BindGrid();
                ltrlStatus.Text = "Updated '" + key + "'.";
                new showMessageBox(this, "Status", ltrlStatus.Text);
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "MessagesEditor: Updated key '" + key + "'");
            }
            catch (Exception ex)
            {
                ltrlStatus.Text = "Update failed: " + ex.Message;
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "MessagesEditor update error for key '" + key + "': " + ex.Message);
            }
        }

        protected void gvMessages_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            // Preview is HTML-encoded in markup; stored value keeps allowed HTML for emails.
        }
    }
}