// Decompiled with JetBrains decompiler
// Type: PrintMasterPage
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

// //- only form later versions #nullable disable --- not for this version of C#
public partial class PrintMasterPage : MasterPage
{
  protected HtmlHead Head1;
  protected ContentPlaceHolder HeadContent;
  protected Image imgQuaffeeLogo;
  protected HtmlForm frmMain;
  protected ContentPlaceHolder MainContent;

  protected void Page_Load(object sender, EventArgs e) => this.Response.Write("  ");

  protected void btnClose_Click(object sender, EventArgs e)
  {
  }
}
