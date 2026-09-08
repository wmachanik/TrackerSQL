using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// Shared status-message CSS: status-message + status-error / status-success / status-info.
    /// </summary>
    public static class StatusMessageHelper
    {
        public static void Set(Panel panel, Literal literal, string message, bool? isError)
        {
            if (panel == null || literal == null)
                return;

            if (string.IsNullOrWhiteSpace(message))
            {
                panel.Visible = false;
                literal.Text = string.Empty;
                panel.CssClass = "status-message";
                return;
            }

            panel.Visible = true;
            literal.Text = message;
            panel.CssClass = CssFor(isError);
        }

        public static void Set(HtmlGenericControl panel, Literal literal, string message, bool? isError)
        {
            if (panel == null || literal == null)
                return;

            if (string.IsNullOrWhiteSpace(message))
            {
                panel.Visible = false;
                literal.Text = string.Empty;
                panel.Attributes["class"] = "status-message";
                return;
            }

            panel.Visible = true;
            literal.Text = message;
            panel.Attributes["class"] = CssFor(isError);
        }

        public static void Set(Panel panel, Label label, string message, bool? isError)
        {
            if (panel == null || label == null)
                return;

            if (string.IsNullOrWhiteSpace(message))
            {
                panel.Visible = false;
                label.Text = string.Empty;
                panel.CssClass = "status-message";
                return;
            }

            panel.Visible = true;
            label.Text = message;
            panel.CssClass = CssFor(isError);
        }

        private static string CssFor(bool? isError)
        {
            if (isError == true)
                return "status-message status-error";
            if (isError == false)
                return "status-message status-success";
            return "status-message status-info";
        }
    }
}
