// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.Pages.CoffeeRequired
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Web.UI;
using System.Web.UI.WebControls;

//- only form later versions #nullable disable
namespace TrackerDotNet.Pages
{
    public partial class CoffeeRequired : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void gvPreperationDay_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Label control1 = (Label)e.Row.FindControl("lblGroupTitle");
                Label control2 = (Label)e.Row.FindControl("lblQty");
                string text = control1.Text;
                double num1 = Convert.ToDouble(control2.Text);
                string str1 = (string)this.ViewState["GroupTitle"];
                double num2 = this.ViewState["GroupTotal"] == null ? 0.0 : (double)this.ViewState["GroupTotal"];
                double num3;
                if (str1 == text)
                {
                    num3 = num2 + num1;
                    control1.Visible = false;
                    control1.Text = string.Empty;
                }
                else
                {
                    string str2 = text;
                    this.ViewState["GroupTitle"] = (object)str2;
                    control1.Visible = true;
                    string str3 = $"{(num2 == 0.0 ? "</td><td></td>" : $"<b>Total</b></td><td align='right'><b>{num2}</b></td>")}</tr><tr><td colspan='2'><b>Prep Date</b>: {str2}</td></tr><tr><td>";
                    control1.Text = str3;
                    num3 = num1;
                }
                this.ViewState["GroupTotal"] = (object)num3;
            }
            else
            {
                if (e.Row.RowType != DataControlRowType.Footer)
                    return;
                double num = this.ViewState["GroupTotal"] == null ? 0.0 : (double)this.ViewState["GroupTotal"];
                ((Label)e.Row.FindControl("lblFooterQty")).Text = num.ToString();
            }
        }

        protected void gvCoffeeRequireByDay_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Label control1 = (Label)e.Row.FindControl("lblByGroupTitle");
                Label control2 = (Label)e.Row.FindControl("lblByAbreviation");
                Label control3 = (Label)e.Row.FindControl("lblByQty");
                string str1 = $"{control1.Text} ({control2.Text})";
                double num1 = Convert.ToDouble(control3.Text);
                string str2 = (string)this.ViewState["GroupByTitle"];
                double num2 = this.ViewState["GroupByTotal"] == null ? 0.0 : (double)this.ViewState["GroupByTotal"];
                double num3;
                if (str2 == str1)
                {
                    num3 = num2 + num1;
                    control1.Visible = false;
                    control1.Text = string.Empty;
                }
                else
                {
                    string str3 = str1;
                    this.ViewState["GroupByTitle"] = (object)str3;
                    control1.Visible = true;
                    string str4 = $"{(num2 == 0.0 ? "</td><td></td>" : $"<b>Total</b></td><td colspan='2' align='right'><b>{num2}</b></td>")}</tr><tr><td colspan='3'><b>Required Date (By)</b>: {str3}</td></tr><tr><td>";
                    control1.Text = str4;
                    num3 = num1;
                }
                this.ViewState["GroupByTotal"] = (object)num3;
            }
            else
            {
                if (e.Row.RowType != DataControlRowType.Footer)
                    return;
                double num = this.ViewState["GroupByTotal"] == null ? 0.0 : (double)this.ViewState["GroupByTotal"];
                ((Label)e.Row.FindControl("lblByFooterQty")).Text = num.ToString();
            }
        }
    }
}