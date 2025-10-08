// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.classes.showMessageBox
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System.Web.UI;

//- only form later versions #nullable disable
namespace TrackerDotNet.Classes
{
    public class showMessageBox
    {
        public showMessageBox(Page pPage, string pTitle, string pMessage)
        {
            if (pPage == null)
                return;
            string script = $"showAppMessage('{pMessage}');";
            ScriptManager.RegisterStartupScript(pPage, pPage.GetType(), pTitle, script, true);
        }
    }
}