// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.SiteMaster
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace TrackerDotNet
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string currentPage = Request.AppRelativeCurrentExecutionFilePath.ToUpper();

            bool parentSelected = false;

            foreach (MenuItem menuItem in NavigationMenu.Items)
            {
                // Compare URLs without query strings
                string menuUrl = ResolveUrl(menuItem.NavigateUrl).ToUpper();
                if (currentPage == menuUrl)
                {
                    menuItem.Selected = true;
                    parentSelected = true;
                }
                else
                {
                    foreach (MenuItem childItem in menuItem.ChildItems)
                    {
                        string childUrl = ResolveUrl(childItem.NavigateUrl).ToUpper();
                        if (currentPage == childUrl)
                        {
                            childItem.Selected = true;
                            menuItem.Selected = true; // Select parent too
                            parentSelected = true;
                        }
                    }
                }
            }

            // Special case for home page
            if ((currentPage.Contains("DEFAULT.ASPX") || currentPage.EndsWith("/")) && !parentSelected)
            {
                NavigationMenu.Items[0].Selected = true;
            }
        }
    }
}