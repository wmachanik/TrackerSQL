// Decompiled with JetBrains decompiler
// Type: EmailCls
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using TrackerDotNet.Classes;

// #nullable disable --- not for this version of C#
public class EmailCls
{
  public const string CONST_APPSETTING_FROMEMAILKEY = "SysEmailFrom";
  public EmailCls.SendLeagacyMailResults myResults;
  private MailMessage myMsg;
  private StringBuilder sbMsgBody = new StringBuilder();

  public EmailCls()
  {
    if (this.myMsg != null)
      return;
    this.myMsg = new MailMessage();
    this.myMsg.IsBodyHtml = true;
    this.myMsg.Body = "";
    this.myResults.sResult = "";
    this.myResults.sID = "";
    this.myMsg.From = new MailAddress(ConfigurationManager.AppSettings["SysEmailFrom"]);
  }

  public virtual bool SetLegacyEmailFromTo(string sFrom, string sTo)
  {
    try
    {
      sFrom = sFrom.Trim();
      sTo = sTo.Trim();
      if (!sFrom.Contains("@") || !sTo.Contains("@"))
        return false;
      this.myMsg.From = new MailAddress(sFrom);
      this.myMsg.To.Add(new MailAddress(sTo));
      return true;
    }
    catch
    {
      return false;
    }
  }

  public virtual bool SetLegacyEmailFrom(string sFrom)
  {
    try
    {
      sFrom = sFrom.Trim();
      if (!sFrom.Contains("@"))
        return false;
      this.myMsg.From = new MailAddress(sFrom);
      return true;
    }
    catch
    {
      return false;
    }
  }

  public virtual bool SetLegacyEmailTo(string sTo) => this.SetLegacyEmailTo(sTo, false);

  public virtual bool SetLegacyEmailTo(string sTo, bool pOverWrite)
  {
    try
    {
      sTo = sTo.Trim();
      if (!sTo.Contains("@"))
        return false;
      if (pOverWrite)
        this.myMsg.To.Clear();
      this.myMsg.To.Add(new MailAddress(sTo));
      return true;
    }
    catch
    {
      return false;
    }
  }

  public virtual void SetLegacyEmailCC(string sCC) => this.myMsg.CC.Add(new MailAddress(sCC));

  public virtual void SetLegacyEmailBCC(string sBCC) => this.myMsg.Bcc.Add(new MailAddress(sBCC));

  public virtual void SetLegacyEmailSubject(string sSubject) => this.myMsg.Subject = sSubject;

  public virtual void AddToLegacyEmailBody(string sBody) => this.sbMsgBody.Append(sBody);

  public virtual void AddStrAndNewLineToLegacyEmailBody(string sBody)
  {
    this.sbMsgBody.Append(sBody);
    this.sbMsgBody.Append("<br />");
  }

  public virtual void AddFormatToLegacyEmailBody(string pFormat, object pObj1)
  {
    this.sbMsgBody.AppendFormat(pFormat, pObj1);
  }

  public virtual void AddFormatToLegacyEmailBody(string pFormat, object pObj1, object pObj2)
  {
    this.sbMsgBody.AppendFormat(pFormat, pObj1, pObj2);
  }

  public virtual void AddFormatToLegacyEmailBody(string pFormat, object pObj1, object pObj2, object Obj3)
  {
    this.sbMsgBody.AppendFormat(pFormat, pObj1, pObj2, Obj3);
  }

  public virtual void AddLegacyEmailPDFAttachment(string pRelativePath)
  {
    this.myMsg.Attachments.Add(new Attachment(HttpContext.Current.Server.MapPath(pRelativePath), "application/pdf"));
  }

  public virtual bool SendLegacyEmail() => this.SendLegacyEmail(false);

  public virtual bool SendLegacyEmail(bool bUseGoogle)
  {
    try
    {
      NetworkCredential networkCredential;
      SmtpClient smtpClient;
      if (bUseGoogle)
      {
        string appSetting1 = ConfigurationManager.AppSettings["GMailLogIn"];
        string appSetting2 = ConfigurationManager.AppSettings["GMailPassword"];
        string appSetting3 = ConfigurationManager.AppSettings["GMailSMTP"];
        this.myMsg.CC.Add(new MailAddress(this.myMsg.From.Address, "CC to " + this.myMsg.From.Address));
        this.myMsg.Subject = $"FROM: {this.myMsg.From.Address} sent: {TimeZoneUtils.Now().Date.ToShortDateString()} re:{this.myMsg.Subject}";
        this.myMsg.From = new MailAddress(appSetting1, "Reply to " + this.myMsg.From.Address);
        networkCredential = new NetworkCredential(appSetting1, appSetting2);
        smtpClient = new SmtpClient(appSetting3, 587);
        smtpClient.EnableSsl = true;
        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        smtpClient.UseDefaultCredentials = false;
      }
      else
      {
        string appSetting4 = ConfigurationManager.AppSettings["EMailLogIn"];
        string appSetting5 = ConfigurationManager.AppSettings["EmailPassword"];
        string appSetting6 = ConfigurationManager.AppSettings["EMailSMTP"];
        int int32 = ConfigurationManager.AppSettings["EmailPort"] != null ? Convert.ToInt32(ConfigurationManager.AppSettings["EmailPort"]) : 0;
        bool flag = ConfigurationManager.AppSettings["EMailSSLEnabled"] != null && string.Equals(ConfigurationManager.AppSettings["EMailSSLEnabled"], "true", StringComparison.OrdinalIgnoreCase);
        this.myMsg.Sender = new MailAddress(appSetting4);
        if (int32 == 0)
        {
          smtpClient = new SmtpClient(appSetting6);
        }
        else
        {
          smtpClient = new SmtpClient(appSetting6, int32);
          smtpClient.EnableSsl = flag;
        }
        smtpClient.UseDefaultCredentials = appSetting5 != "";
        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        networkCredential = !smtpClient.UseDefaultCredentials ? new NetworkCredential() : new NetworkCredential(appSetting4, appSetting5);
      }
      smtpClient.Credentials = (ICredentialsByHost) networkCredential;
      this.myMsg.Body = this.sbMsgBody.ToString();
      smtpClient.Send(this.myMsg);
      this.myResults.sID = "xxx";
      this.myResults.sResult = "";
      return true;
    }
    catch (Exception ex)
    {
      this.myResults.sResult = "ERROR: " + ex.Message;
      return !bUseGoogle && this.SendLegacyEmail(true);
    }
  }

  public string MsgBody
  {
    get => this.myMsg != null ? this.sbMsgBody.ToString() : "";
    set
    {
      if (this.myMsg == null)
        return;
      if (this.sbMsgBody.Length >= 1)
        this.sbMsgBody.Remove(0, this.sbMsgBody.Length - 1);
      this.sbMsgBody.Append(value);
    }
  }

  public virtual void Kill()
  {
    if (this.myMsg == null)
      return;
    this.myMsg.Dispose();
    this.myMsg = (MailMessage) null;
  }

  public struct SendLeagacyMailResults
  {
    public string sResult;
    public string sID;
  }
}
