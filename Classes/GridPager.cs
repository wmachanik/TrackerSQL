using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// App-standard GridView pager: "Previous | 1 2 [3] 4 5 | Next" square buttons
    /// (styles: .pager-row / .pager-nav / .pager-btn in Site.css).
    ///
    /// Usage per grid:
    ///   markup:  OnRowCreated="gvX_RowCreated"
    ///            &lt;PagerStyle CssClass="pager-row" /&gt;
    ///            &lt;PagerTemplate&gt;&lt;asp:PlaceHolder ID="plhPager" runat="server" /&gt;&lt;/PagerTemplate&gt;
    ///   code:    protected void gvX_RowCreated(object sender, GridViewRowEventArgs e)
    ///            { GridPager.BuildPager((GridView)sender, e.Row); }
    ///
    /// The existing OnPageIndexChanging handler keeps working unchanged: the buttons raise
    /// the GridView's native "Page" command (arguments "Prev"/"Next"/page number).
    ///
    /// Buttons are rebuilt in RowCreated so they exist on every postback even when the grid
    /// is restored from ViewState without a rebind. They are registered as FULL postbacks:
    /// async partial updates of pager clicks proved unreliable (server paged correctly but
    /// the browser did not always apply the panel update).
    /// </summary>
    public static class GridPager
    {
        /// <summary>Page-number squares shown either side of the current page.</summary>
        private const int WindowSize = 2;

        public static void BuildPager(GridView grid, GridViewRow row)
        {
            if (grid == null || row == null || row.RowType != DataControlRowType.Pager)
                return;

            var pagerPlaceHolder = row.FindControl("plhPager") as PlaceHolder;
            if (pagerPlaceHolder == null)
                return;

            pagerPlaceHolder.Controls.Clear();

            int pageCount = grid.PageCount;
            int currentIndex = grid.PageIndex;

            var pagerNav = new Panel { CssClass = "pager-nav" };
            pagerPlaceHolder.Controls.Add(pagerNav);

            // IDs use "lnkPager…" (not "btn…") so they are not caught by the Site.css
            // *[id*="btn"] ImageButton exclusion that strips borders/backgrounds.
            AddButton(grid, pagerNav, "lnkPagerPrev", "Previous", "Prev",
                enabled: currentIndex > 0, cssClass: "pager-btn pager-btn-edge");

            int windowStart = Math.Max(0, currentIndex - WindowSize);
            int windowEnd = Math.Min(pageCount - 1, currentIndex + WindowSize);

            if (windowStart > 0)
            {
                AddButton(grid, pagerNav, "lnkPagerFirst", "1", "1", enabled: true, cssClass: "pager-btn");
                if (windowStart > 1)
                    AddLabel(pagerNav, "...", "pager-ellipsis");
            }

            for (int pageIndex = windowStart; pageIndex <= windowEnd; pageIndex++)
            {
                string pageNumber = (pageIndex + 1).ToString();
                if (pageIndex == currentIndex)
                    AddLabel(pagerNav, pageNumber, "pager-btn pager-current");
                else
                    AddButton(grid, pagerNav, "lnkPagerPage" + pageNumber, pageNumber, pageNumber,
                        enabled: true, cssClass: "pager-btn");
            }

            if (windowEnd < pageCount - 1)
            {
                if (windowEnd < pageCount - 2)
                    AddLabel(pagerNav, "...", "pager-ellipsis");
                string lastPage = pageCount.ToString();
                AddButton(grid, pagerNav, "lnkPagerLast", lastPage, lastPage, enabled: true, cssClass: "pager-btn");
            }

            AddButton(grid, pagerNav, "lnkPagerNext", "Next", "Next",
                enabled: currentIndex < pageCount - 1, cssClass: "pager-btn pager-btn-edge");
        }

        private static void AddButton(GridView grid, Panel pagerNav, string id, string text,
            string pageArgument, bool enabled, string cssClass)
        {
            if (!enabled)
            {
                AddLabel(pagerNav, text, cssClass + " pager-disabled");
                return;
            }

            var button = new LinkButton
            {
                ID = id,
                Text = text,
                CommandName = "Page",
                CommandArgument = pageArgument,
                CausesValidation = false,
                CssClass = cssClass,
                ToolTip = pageArgument == "Prev" ? "Previous page"
                    : pageArgument == "Next" ? "Next page"
                    : "Go to page " + pageArgument
            };
            pagerNav.Controls.Add(button);

            // Full postback: partial updates of pager clicks were unreliable client-side.
            var scriptManager = ScriptManager.GetCurrent(grid.Page);
            if (scriptManager != null)
                scriptManager.RegisterPostBackControl(button);
        }

        private static void AddLabel(Panel pagerNav, string text, string cssClass)
        {
            pagerNav.Controls.Add(new Label { Text = text, CssClass = cssClass });
        }
    }
}
