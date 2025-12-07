// Decompiled with JetBrains decompiler
// Type: TrackerSQL.Tools.SystemTools
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

using AjaxControlToolkit;
using AjaxControlToolkit.HtmlEditor.ToolbarButtons;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Controls;

//- only form later versions #nullable disable
namespace TrackerSQL.Tools
{
    public partial class SystemTools : System.Web.UI.Page
    {
        private const int CONST_MINMONTHS = 3;
        private StreamWriter _ColsStream;
        //--- these are in the desiugn file, but commented out for now
        //protected UpdateProgress uprgSystemTools;
        //protected UpdatePanel upnlSystemToolsButtons;
        //protected Button btnSetClientType;
        //protected Button btnXMLTOSQL;
        //protected Button btnResetPrepDates;
        //protected Button btnMoveDlvryDate;
        //protected Button btnEditSystemData;
        //protected Button btnCreateUpdateLogTables;
        //protected Button btnMergQBAccData;
        //protected Literal ltrlStatus;
        //protected Panel pnlSetClientType;
        //protected Label ResultsTitleLabel;
        //protected GridView gvResults;
        //protected GridView gvCustomerTypes;
        //protected ObjectDataSource odsCustomerTypes;
        //protected Panel pnlResetPrepDate;
        //protected GridView gvCityPrepDates;
        //protected SqlDataSource sdsCityPrepDates;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                pnlSetClientType.Visible = false;
                gvResults.Visible = false;
                ltrlStatus.Visible = false;
                ResultsTitleLabel.Visible = false;
                pnlResultsSection.Visible = false;
            }
            // Ensure Messages Editor button only shows for allowed roles every request
            SetMessagesEditorButtonVisibility();
        }
        private void SetMessagesEditorButtonVisibility()
        {
            try
            {
                // Defensive: control may not exist if markup not deployed yet
                if (btnMessagesEditor == null) return;

                var user = Context?.User;
                bool canSee =
                    user != null &&
                    (user.IsInRole("Administrators") ||
                     user.IsInRole("Admin") ||
                     user.IsInRole("AgentManager"));

                btnMessagesEditor.Visible = canSee;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "SystemTools: Error setting MessagesEditor button visibility: " + ex.Message);
            }
        }
        private List<int> GetAllCoffeeClientTypes()
        {
            List<int> coffeeClientTypes = new List<int>()
            {
                3,
                1,
                8,
                5,
                4,
                2
            };
            coffeeClientTypes.Sort();
            return coffeeClientTypes;
        }

        private List<int> GetAllServiceOnlyClientTypes()
        {
            List<int> serviceOnlyClientTypes = new List<int>() { 6 };
            serviceOnlyClientTypes.Sort();
            return serviceOnlyClientTypes;
        }

        private SystemTools.ContactsUpdated CheckCoffeeCustomerIsOne(
          ContactType pCustomer,
          SystemTools.ContactsUpdated pContact)
        {
            ClientUsageLinesTbl clientUsageLinesTbl = new ClientUsageLinesTbl();
            ItemUsageTbl itemUsageTbl = new ItemUsageTbl();
            DateTime minValue = DateTime.MinValue;
            ClientUsageLinesTbl latestUsageData = clientUsageLinesTbl.GetLatestUsageData(pCustomer.CustomerID, 2);
            if (latestUsageData != null)
            {
                DateTime customerInstallDate = latestUsageData.GetCustomerInstallDate(pCustomer.CustomerID);
                if (latestUsageData.LineDate <= customerInstallDate)
                {
                    if (itemUsageTbl.GetLastMaintenanceItem(pCustomer.CustomerID) == null)
                    {
                        pContact.ContactTypeID = 9;
                        pContact.PredictionDisabled = true;
                    }
                    else
                        pContact.ContactTypeID = 6;
                }
                else if (customerInstallDate.AddMonths(3) <= latestUsageData.LineDate)
                {
                    if (itemUsageTbl.GetLastMaintenanceItem(pCustomer.CustomerID) == null)
                        pContact.ContactTypeID = 3;
                    else if (pContact.ContactTypeID == 3)
                        pContact.ContactTypeID = 1;
                }
            }
            else
                pContact.ContactTypeID = itemUsageTbl.GetLastMaintenanceItem(pCustomer.CustomerID) == null ? 9 : 6;
            return pContact;
        }

        private SystemTools.ContactsUpdated CheckNoneCoffeeCustomer(
          ContactType pCustomer,
          SystemTools.ContactsUpdated pContact)
        {
            ClientUsageLinesTbl clientUsageLinesTbl = new ClientUsageLinesTbl();
            ItemUsageTbl itemUsageTbl = new ItemUsageTbl();
            List<ItemUsageTbl> lastItemsUsed = itemUsageTbl.GetLastItemsUsed(pCustomer.CustomerID, 2);
            ItemUsageTbl lastMaintenanceItem = itemUsageTbl.GetLastMaintenanceItem(pCustomer.CustomerID);
            if (lastItemsUsed.Count > 0)
            {
                DateTime customerInstallDate = clientUsageLinesTbl.GetCustomerInstallDate(pCustomer.CustomerID);
                if (clientUsageLinesTbl.LineDate >= customerInstallDate)
                {
                    if (lastMaintenanceItem == null)
                        pContact.ContactTypeID = 9;
                }
                else if (lastMaintenanceItem == null)
                    pContact.ContactTypeID = 3;
            }
            else if (lastMaintenanceItem == null)
                pContact.ContactTypeID = 9;
            lastItemsUsed.Clear();
            return pContact;
        }

        protected void btnSetClientType_Click(object sender, EventArgs e)
        {
            this.pnlResultsSection.Visible = true;
            this.pnlSetClientType.Visible = true;
            this.gvCustomerTypes.Visible = true;
            this.gvResults.Visible = true;
            this.ltrlStatus.Visible = true;
            this.ResultsTitleLabel.Visible = true;
            List<SystemTools.ContactsUpdated> contactsUpdatedList = new List<SystemTools.ContactsUpdated>();
            ClientUsageLinesTbl clientUsageLinesTbl = new ClientUsageLinesTbl();
            ItemUsageTbl itemUsageTbl = new ItemUsageTbl();
            ContactType contactType = new ContactType();
            string fileName = $"SetClientType_{TimeZoneUtils.Now():ddMMyyyy_HHmm}.txt";
            string filePath = Server.MapPath("~/App_Data/" + fileName);
            this._ColsStream = new StreamWriter(filePath, false);
            this._ColsStream.WriteLine("Task, Company Name, origType, newType, PredDisabled");
            List<ContactType> contactTypeList = (List<ContactType>)null;
            int index = 0;
            try
            {
                contactTypeList = contactType.GetAllContacts("CompanyName");
                List<int> coffeeClientTypes = this.GetAllCoffeeClientTypes();
                this.GetAllServiceOnlyClientTypes();
                for (; index < contactTypeList.Count; ++index)
                {
                    SystemTools.ContactsUpdated pContact = new SystemTools.ContactsUpdated();
                    if (contactTypeList[index].CustomerTypeID != 9)
                    {
                        pContact.ContactName = contactTypeList[index].CompanyName;
                        pContact.ContactTypeID = contactTypeList[index].CustomerTypeID;
                        pContact.origContactTypeID = contactTypeList[index].CustomerTypeID;
                        pContact.PredictionDisabled = contactTypeList[index].PredictionDisabled;
                        if (pContact.ContactTypeID == 0)
                            pContact.ContactTypeID = 3;
                        SystemTools.ContactsUpdated contactsUpdated = !coffeeClientTypes.Contains(pContact.ContactTypeID) ? this.CheckNoneCoffeeCustomer(contactTypeList[index], pContact) : this.CheckCoffeeCustomerIsOne(contactTypeList[index], pContact);
                        if (!contactsUpdated.ContactTypeID.Equals(contactsUpdated.origContactTypeID))
                        {
                            contactTypeList[index].CustomerTypeID = contactsUpdated.ContactTypeID;
                            contactTypeList[index].PredictionDisabled = contactsUpdated.PredictionDisabled;
                            string str = contactTypeList[index].UpdateContact(contactTypeList[index]);
                            contactsUpdatedList.Add(contactsUpdated);
                            if (string.IsNullOrEmpty(str))
                                this._ColsStream.WriteLine("Added {0}-{1}: {2}, {3}, {4}, {5}", (object)index, (object)contactsUpdatedList.Count, (object)contactsUpdated.ContactName, (object)contactsUpdated.origContactTypeID, (object)contactsUpdated.ContactTypeID, (object)contactsUpdated.PredictionDisabled);
                            else
                                this._ColsStream.WriteLine("Error {0} Adding: {1}, {2}, {3}, {4}, {5}", (object)str, (object)index, (object)str, (object)contactsUpdated.ContactName, (object)contactsUpdated.origContactTypeID, (object)contactsUpdated.ContactTypeID, (object)contactsUpdated.PredictionDisabled);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string pMessage = ex.Message;
                string sessionErrorString = new TrackerTools().GetTrackerSessionErrorString();
                if (!string.IsNullOrWhiteSpace(sessionErrorString))
                    pMessage = $"{pMessage} TTError: {sessionErrorString}";
                showMessageBox showMessageBox = new showMessageBox(this.Page, "Error", pMessage);
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Set Client Type Error: " + pMessage);
                if (contactTypeList != null)
                    this._ColsStream.WriteLine("ERROR AT: {0}, Name: {1}, ID: {2}, Pred: {3}", (object)index, (object)contactTypeList[index].CompanyName, (object)contactTypeList[index].CustomerTypeID, (object)contactTypeList[index].PredictionDisabled);
                else
                    this._ColsStream.WriteLine("null customers");
                this._ColsStream.WriteLine("Error:" + pMessage);
                throw;
            }
            finally
            {
                this._ColsStream.Close();
            }
            if (contactsUpdatedList.Count == 0)
                this.ResultsTitleLabel.Text = "No updates were necessary.";
            else
            {
                string msg = $"A Total of {contactsUpdatedList.Count}, contacts were updated";
                showMessageBox showMessageBox1 = new showMessageBox(this.Page, "Info", msg);
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Set Client Type: " + msg);

                pnlResultsSection.Visible = true;
                this.ltrlStatus.Text = $"A Total of {contactsUpdatedList.Count}, contacts were updated";
                this.ltrlStatus.Visible = true;
                this.ResultsTitleLabel.Text = "Set client type results";
                this.gvResults.DataSource = (object)contactsUpdatedList;
                this.gvResults.DataBind();
            }
        }

        protected void btnResetPrepDates_Click(object sender, EventArgs e)
        {
            pnlResultsSection.Visible = true;
            this.pnlResetPrepDate.Visible = true;
            new TrackerTools().SetNextRoastDateByCity();
            this.ltrlStatus.Text = "SystemTools: Prep/Delivery dates reset.";
            this.ltrlStatus.Visible = true;
            AppLogger.WriteLog(SystemConstants.LogTypes.System, "SystemTools: Prep/Delivery dates reset.");
            this.sdsCityPrepDates.DataBind();
            this.gvCityPrepDates.DataBind();
        }

        //protected void btnCreateUpdateLogTables_Click(object sender, EventArgs e)
        //{
        //    TrackerTools trackerTools = new TrackerTools();
        //    trackerTools.ClearTrackerSessionErrorString();
        //    TrackerDb trackerDb = new TrackerDb();
        //    trackerDb.CreateIfDoesNotExists("LogTbl");
        //    this.pnlResultsSection.Visible = true;
        //    this.ltrlStatus.Visible = true;
        //    this.ltrlStatus.Text = "Log table checked";
        //    if (trackerTools.IsTrackerSessionErrorString())
        //    {
        //        Literal ltrlStatus = this.ltrlStatus;
        //        ltrlStatus.Text = $"{ltrlStatus.Text} - Error: {trackerTools.GetTrackerSessionErrorString()}";
        //        trackerTools.ClearTrackerSessionErrorString();
        //    }
        //    List<string> stringList = new PersonsTbl().SecurityUsersNotInPeopleTbl();
        //    this.pnlSetClientType.Visible = true;
        //    this.gvCustomerTypes.Visible = false;
        //    this.gvResults.DataSource = (object)stringList;
        //    this.ResultsTitleLabel.Text = "Security Users not in People Table ";
        //    if (trackerTools.IsTrackerSessionErrorString())
        //    {
        //        Literal ltrlStatus = this.ltrlStatus;
        //        ltrlStatus.Text = $"{ltrlStatus.Text} - Error: {trackerTools.GetTrackerSessionErrorString()}";
        //        trackerTools.ClearTrackerSessionErrorString();
        //    }
        //    this.gvResults.DataBind();
        //    if (trackerDb.CreateIfDoesNotExists("SectionTypesTbl"))
        //    {
        //        this.ltrlStatus.Text += "; Section Types table checked";
        //        if (trackerTools.IsTrackerSessionErrorString())
        //        {
        //            Literal ltrlStatus = this.ltrlStatus;
        //            ltrlStatus.Text = $"{ltrlStatus.Text} - Error: {trackerTools.GetTrackerSessionErrorString()}";
        //            trackerTools.ClearTrackerSessionErrorString();
        //        }
        //        if (new SectionTypesTbl().InsertDefaultSections())
        //            this.ltrlStatus.Text += " - default sections added.";
        //        if (trackerTools.IsTrackerSessionErrorString())
        //        {
        //            Literal ltrlStatus = this.ltrlStatus;
        //            ltrlStatus.Text = $"{ltrlStatus.Text} - Error: {trackerTools.GetTrackerSessionErrorString()}";
        //            trackerTools.ClearTrackerSessionErrorString();
        //        }
        //    }
        //    if (trackerDb.CreateIfDoesNotExists("TransactionTypesTbl"))
        //    {
        //        this.ltrlStatus.Text += "; Transaction Types table checked";
        //        if (trackerTools.IsTrackerSessionErrorString())
        //        {
        //            Literal ltrlStatus = this.ltrlStatus;
        //            ltrlStatus.Text = $"{ltrlStatus.Text} - Error: {trackerTools.GetTrackerSessionErrorString()}";
        //            trackerTools.ClearTrackerSessionErrorString();
        //        }
        //        if (new TransactionTypesTbl().InsertDefaultTransactions())
        //            this.ltrlStatus.Text += " - default Transactions added.";
        //        if (trackerTools.IsTrackerSessionErrorString())
        //        {
        //            Literal ltrlStatus = this.ltrlStatus;
        //            ltrlStatus.Text = $"{ltrlStatus.Text} - Error: {trackerTools.GetTrackerSessionErrorString()}";
        //            trackerTools.ClearTrackerSessionErrorString();
        //        }
        //    }
        //    trackerDb.Close();
        //}
        protected void btnSetLastOrderDate_Click(object sender, EventArgs e)
        {
            AppLogger.WriteLog(SystemConstants.LogTypes.System, "SystemTools: btnSetLastOrderDate_Click started.");

            System.Threading.Thread.Sleep(2000); // For progress bar test

            var reoccurDal = new TrackerSQL.Controls.ReoccuringOrderDAL();
            var orders = reoccurDal.GetAll(TrackerSQL.Controls.ReoccuringOrderDAL.CONST_ENABLEDONLY);

            AppLogger.WriteLog(SystemConstants.LogTypes.System, $"SystemTools: Found {orders.Count} enabled recurring orders.");

            var results = new List<RecurringOrderUpdateResult>();
            int updatedCount = 0;

            foreach (var order in orders)
            {
                DateTime lastOrderDate = order.DateLastDone;
                string updateResult = reoccurDal.SetReoccuringOrderDates(lastOrderDate, order.ReoccuringOrderID, true);

                results.Add(new RecurringOrderUpdateResult
                {
                    OrderID = order.ReoccuringOrderID,
                    ContactName = order.CompanyName,
                    Item = order.ItemTypeDesc,
                    LastOrderDate = lastOrderDate.ToString("yyyy-MM-dd"),
                    UpdateResult = updateResult
                });

                if (updateResult != null && !updateResult.StartsWith("Error"))
                    updatedCount++;
            }

            ResultsTitleLabel.Text = $"Set Last Order Date Results: {updatedCount} updated.";
            gvResults.DataSource = results;
            gvResults.DataBind();

            pnlResultsSection.Visible = true;
            pnlSetClientType.Visible = true;

            AppLogger.WriteLog(SystemConstants.LogTypes.System, $"SystemTools: SetLastOrderDate updated {updatedCount} recurring orders.");

            // Show message box to user
            string msg = updatedCount > 0
                ? $"A Total of {updatedCount} recurring orders were updated."
                : "No recurring orders were updated.";
            showMessageBox showMessageBox1 = new showMessageBox(this.Page, "Info", msg);
        }
        protected void btnDisableInactiveClients_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime cutoff = TimeZoneUtils.Now().AddYears(-3).Date;

                // 1) Find enabled customers with no orders in 3+ years (or never)
                var toDisable = new List<InactiveClientResult>();
                var db = new TrackerDb();
                db.AddWhereParams(cutoff, DbType.Date, "@Cutoff");

                string selectSql = "SELECT C.CustomerID, C.CompanyName, X.LastOrderDate FROM CustomersTbl AS C " +
                    "LEFT JOIN ( SELECT O.CustomerID, " +
                    "           MAX(IIF(O.RequiredByDate IS NOT NULL, O.RequiredByDate, O.OrderDate)) AS LastOrderDate " +
                    "           FROM OrdersTbl AS O GROUP BY O.CustomerID ) AS X ON X.CustomerID = C.CustomerID " +
                    "WHERE C.[enabled] = true AND (X.LastOrderDate IS NULL OR X.LastOrderDate < ?)";

                var rdr = db.ExecuteSQLGetDataReader(selectSql);
                if (rdr != null)
                {
                    while (rdr.Read())
                    {
                        var last = rdr["LastOrderDate"] == DBNull.Value
                            ? (DateTime?)null
                            : Convert.ToDateTime(rdr["LastOrderDate"]).Date;

                        toDisable.Add(new InactiveClientResult
                        {
                            CustomerID = rdr["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["CustomerID"]),
                            CompanyName = rdr["CompanyName"] == DBNull.Value ? string.Empty : rdr["CompanyName"].ToString(),
                            LastOrderDate = last?.ToString("yyyy-MM-dd") ?? "(none)"
                        });
                    }
                    rdr.Close();
                }
                db.Close();

                // If none, show message and exit
                if (toDisable.Count == 0)
                {
                    pnlResultsSection.Visible = true;
                    pnlSetClientType.Visible = true;
                    ResultsTitleLabel.Text = "Disable Inactive Clients: No eligible customers found.";
                    ltrlStatus.Text = "No active customers are older than 3 years without orders.";
                    ltrlStatus.Visible = true;
                    gvResults.DataSource = null;
                    gvResults.DataBind();
                    new showMessageBox(this.Page, "Info", "No eligible customers found.");
                    return;
                }

                // 2) Disable them in bulk; do NOT modify Notes
                var dbu = new TrackerDb();

                // Bind cutoff as positional "?" parameter
                dbu.AddWhereParams(cutoff, DbType.Date, "@Cutoff");

                // Disable customers that have no orders on/after cutoff (covers both "no orders" and "all orders older than cutoff")
                string updateSql =
                    "UPDATE CustomersTbl AS C SET C.[enabled] = false " +
                    "WHERE C.[enabled] = true AND NOT EXISTS (" +
                    "  SELECT 1 FROM OrdersTbl AS O " +
                    "  WHERE O.CustomerID = C.CustomerID " +
                    "    AND IIF(O.RequiredByDate IS NOT NULL, O.RequiredByDate, O.OrderDate) >= ?" +
                    ")";

                // Execute and surface any error to the UI
                string updateErr = dbu.ExecuteNonQuerySQLWithParams(updateSql, null, dbu.WhereParams);
                int affected = dbu.numRecs;
                dbu.Close();

                if (!string.IsNullOrEmpty(updateErr))
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.System,
                        "SystemTools: Disable inactive clients UPDATE failed: " + updateErr);

                    pnlResultsSection.Visible = true;
                    pnlSetClientType.Visible = true;
                    gvResults.Visible = false;
                    ResultsTitleLabel.Visible = true;
                    ltrlStatus.Visible = true;

                    ResultsTitleLabel.Text = "Disable Inactive Clients: Update failed";
                    ltrlStatus.Text = updateErr;
                    new showMessageBox(this.Page, "Error",
                        "An error occurred disabling inactive clients: " + updateErr);
                    return;
                }

                // Success: show results
                pnlResultsSection.Visible = true;
                pnlSetClientType.Visible = true;
                gvResults.Visible = true;
                ltrlStatus.Visible = true;
                ResultsTitleLabel.Visible = true;

                // Use 'affected' for actual updated count; show the list we selected earlier
                ResultsTitleLabel.Text = $"Disabled {affected} clients (no orders since before {cutoff:yyyy-MM-dd}).";
                ltrlStatus.Text = "Disable Inactive Clients completed.";
                gvResults.DataSource = toDisable; // AutoGenerateColumns = true
                gvResults.DataBind();

                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    $"SystemTools: Disabled {affected} inactive clients.");
                new showMessageBox(this.Page, "Info", $"Disabled {affected} inactive clients.");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "SystemTools: Disable inactive clients error: " + ex.Message);
                new showMessageBox(this.Page, "Error", "An error occurred disabling inactive clients: " + ex.Message);
            }
        }

        private class ContactsUpdated
        {
            private string _ContactName;
            private int _ContactTypeID;
            private int _origContactTypeID;
            private bool _PredictionDisabled;

            public ContactsUpdated()
            {
                this._ContactName = string.Empty;
                this._ContactTypeID = this._origContactTypeID = int.MinValue;
                this._PredictionDisabled = false;
            }

            public string ContactName
            {
                get => this._ContactName;
                set => this._ContactName = value;
            }

            public int ContactTypeID
            {
                get => this._ContactTypeID;
                set => this._ContactTypeID = value;
            }

            public int origContactTypeID
            {
                get => this._origContactTypeID;
                set => this._origContactTypeID = value;
            }

            public bool PredictionDisabled
            {
                get => this._PredictionDisabled;
                set => this._PredictionDisabled = value;
            }
        }
        public class RecurringOrderUpdateResult
        {
            public int OrderID { get; set; }
            public string ContactName { get; set; }
            public string Item { get; set; }
            public string LastOrderDate { get; set; }
            public string UpdateResult { get; set; }
        }
        private class InactiveClientResult
        {
            public int CustomerID { get; set; }
            public string CompanyName { get; set; }
            public string LastOrderDate { get; set; }
        }

    }

}