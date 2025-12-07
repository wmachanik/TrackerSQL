<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SentRemindersSheet.aspx.cs" Inherits="TrackerSQL.Pages.SentRemindersSheet" %>

<asp:Content ID="cntSentRemindersSheetHdr" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .failedEmailsSection {
            margin-top: 20px;
            padding: 15px;
            border: 2px solid #FF6B6B;
            border-radius: 5px;
            background-color: #FFF5F5;
        }
    </style>
</asp:Content>
<asp:Content ID="cntSentRemindersSheetBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h1>List of Reminders Sent </h1>
    <asp:ScriptManager ID="smSentRemindersSummary" runat="server"></asp:ScriptManager>
    <asp:UpdateProgress ID="uprgSentRemindersSummary" runat="server" AssociatedUpdatePanelID="upnlSentRemindersList">
        <ProgressTemplate>
            <img src="../images/animi/BlueArrowsUpdate.gif" alt="updating" width="16" height="16" />updating.....
        </ProgressTemplate>
    </asp:UpdateProgress>
    <div class="simpleLightBrownForm">
        <asp:UpdatePanel ID="upnlSelection" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                Filter by: 
        <asp:DropDownList ID="ddlFilterByDate" runat="server"
            ToolTip="select which item to search form" AutoPostBack="True" DataTextFormatString="{0:d}"
            DataSourceID="odsDatesSentReminder" DataTextField="Date" DataValueField="Date">
        </asp:DropDownList>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <br />
    <asp:UpdatePanel ID="upnlSentRemindersList" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <!-- Add this before your existing table -->
            <asp:Literal ID="ltrlReminderSummary" runat="server" />
            <br />
            <div class="simpleLightBrownForm" style="padding-left: 1em; padding-right: 1em">
                <asp:GridView ID="gvSentReminders" runat="server" AutoGenerateColumns="False" CssClass="TblZebra"
                    AllowSorting="True" DataSourceID="odsSentRemindersSummarys" AllowPaging="True" PageSize="20">
                    <Columns>
                        <asp:BoundField DataField="ReminderID" HeaderText="ReminderID"
                            SortExpression="ReminderID" Visible="false" />
                        <asp:TemplateField HeaderText="Customer">
                            <EditItemTemplate>
                                <asp:TextBox ID="CustomerTextBox" runat="server" Text='<%# Bind("CustomerID") %>'></asp:TextBox>
                            </EditItemTemplate>
                            <ItemTemplate>
                                <asp:HyperLink ID="CustomerHyperLink" runat="server" Text='<%# GetCompanyName((long)Eval("CustomerID")) %>'
                                    NavigateUrl='<%# Eval("CustomerID", "~/Pages/CustomerDetails.aspx?ID={0}") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="DateSentReminder" HeaderText="Date Reminder Sent"
                            SortExpression="DateSentReminder" DataFormatString="{0:d}"
                            ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Left" />
                        <asp:BoundField DataField="NextPrepDate" HeaderText="Prep Date"
                            SortExpression="NextPrepDate" DataFormatString="{0:d}"
                            ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Left" />
                        <asp:CheckBoxField DataField="ReminderSent" HeaderText="Reminder Sent"
                            ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Left" />
                        <asp:CheckBoxField DataField="HadAutoFulfilItem"
                            HeaderText="Had Auto FulfilItems" SortExpression="HadAutoFulfilItem"
                            ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Left" />
                        <asp:CheckBoxField DataField="HadReoccurItems" HeaderText="Had Reoccur Items"
                            SortExpression="HadReoccurItems" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Center" />
                    </Columns>
                    <RowStyle Font-Size="Large" />
                </asp:GridView>
                <!-- Add this after your existing table -->
                <asp:Literal ID="ltrlReminderFooter" runat="server"></asp:Literal>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="ddlFilterByDate"
                EventName="SelectedIndexChanged" />
        </Triggers>
    </asp:UpdatePanel>
    <br />
    <!-- Failed Emails Section - Only visible when there are failures -->
    <div id="divFailedEmails" runat="server" visible="false" class="failedEmailsSection">
        <h3>⚠️ Failed Email Reminders</h3>
        <p>The following customers could not receive coffee checkup emails:</p>

        <asp:GridView ID="gvFailedEmails" runat="server" CssClass="TblZebra"
            AutoGenerateColumns="false"
            EmptyDataText="No failed emails to display."
            ShowHeader="true"
            BorderStyle="None"
            GridLines="None">
            <Columns>
                <asp:BoundField DataField="CustomerName" HeaderText="Customer"
                    SortExpression="CustomerName"
                    ItemStyle-Width="40%" />
                <asp:BoundField DataField="FailureReason" HeaderText="Failure Reason"
                    SortExpression="FailureReason"
                    ItemStyle-Width="60%" />
            </Columns>
            <EmptyDataTemplate>
                <div style="text-align: center; padding: 20px; color: #666;">
                    No failed emails to display.
           
                </div>
            </EmptyDataTemplate>
        </asp:GridView>

        <div style="margin-top: 15px; text-align: center;">
            <asp:Button ID="btnClearFailures" runat="server" Text="Clear Failed List"
                OnClick="btnClearFailures_Click"
                Style="background-color: #6c757d; color: white; padding: 8px 16px; border: none; border-radius: 4px; cursor: pointer;" />
        </div>

        <div style="margin-top: 10px; font-size: 0.9em; color: #666; text-align: center;">
            <em>💡 Tip: Check customer email addresses and account status for failed entries.</em>
        </div>
    </div>
    <br />
    <asp:ObjectDataSource ID="odsSentRemindersSummarys"
        TypeName="TrackerSQL.Controls.SentRemindersLogTbl"
        SortParameterName="SortBy"
        SelectMethod="GetAllByDate"
        runat="server" OldValuesParameterFormatString="original_{0}">
        <SelectParameters>
            <asp:ControlParameter ControlID="ddlFilterByDate"
                Name="pDateSent" PropertyName="SelectedValue" Type="DateTime" />
            <asp:Parameter Name="SortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsDatesSentReminder" runat="server"
        SelectMethod="GetLast20DatesReminderSent"
        TypeName="TrackerSQL.Controls.SentRemindersLogTbl"></asp:ObjectDataSource>

    <br />
    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Always">
        <ContentTemplate>
            <asp:Label ID="lblFilter" Text="" runat="server" />
        </ContentTemplate>
    </asp:UpdatePanel>

    <%--            <asp:Label ID="CustomerLabel" runat="server" Text='<%# GetCustomerName((long)Eval("CustomerID")) %>' />
    --%>
</asp:Content>
