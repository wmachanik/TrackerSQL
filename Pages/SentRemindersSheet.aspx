<%@ Page Title="Sent Reminders" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="SentRemindersSheet.aspx.cs" Inherits="TrackerSQL.Pages.SentRemindersSheet" %>

<asp:Content ID="cntSentRemindersSheetHdr" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .failedEmailsSection {
            margin-top: 20px;
            padding: 15px;
            border: 2px solid #FF6B6B;
            border-radius: 5px;
            background-color: #FFF5F5;
        }
        .reminder-summary-stats {
            display: flex;
            gap: 20px;
            flex-wrap: wrap;
            margin-top: 8px;
        }
    </style>
</asp:Content>

<asp:Content ID="cntSentRemindersSheetBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smSentRemindersSummary" runat="server" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="uprgSentRemindersSummary" runat="server"
        AssociatedUpdatePanelID="upnlSentRemindersList" DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info" style="margin: 8px 0;">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="upnlSentRemindersList" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <asp:Panel ID="pnlReminders" runat="server" CssClass="simpleForm page-tone-panel page-tone-reminders">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/View.png" alt="" />
                    <div>
                        <h1 class="page-tone-title">Reminder History</h1>
                        <p class="page-tone-subtitle">View sent reminders and notifications</p>
                    </div>
                </div>

                <div class="page-tone-toolbar button-row">
                    <asp:Label ID="lblFilterByDate" runat="server" Text="Filter by date:" AssociatedControlID="ddlFilterByDate" CssClass="small" />
                    <asp:DropDownList ID="ddlFilterByDate" runat="server"
                        ToolTip="Select which date to view"
                        AutoPostBack="True" DataTextFormatString="{0:d}"
                        OnSelectedIndexChanged="ddlFilterByDate_SelectedIndexChanged" />
                    &nbsp;
                    <asp:Button ID="btnRefresh" runat="server" Text="Refresh" CssClass="filter-panel-btn"
                        OnClick="btnRefresh_Click" CausesValidation="false"
                        ToolTip="Reload reminders for the selected date" />
                    <span class="image-button" title="Return to home">
                        <asp:ImageButton ID="btnBack" runat="server"
                            ImageUrl="~/images/imgButtons/Back.gif"
                            AlternateText="Back"
                            ToolTip="Return to home"
                            OnClick="btnBack_Click"
                            CausesValidation="false" />
                    </span>
                </div>

                <asp:Literal ID="ltrlReminderSummary" runat="server" Mode="PassThrough" />

                <div style="margin-top: 12px;">
                    <asp:GridView ID="gvSentReminders" runat="server" AutoGenerateColumns="False" CssClass="TblZebra"
                        AllowSorting="True" AllowPaging="True" PageSize="20"
                        OnPageIndexChanging="gvSentReminders_PageIndexChanging">
                        <Columns>
                            <asp:BoundField DataField="ReminderID" HeaderText="ReminderID"
                                SortExpression="ReminderID" Visible="false" />
                            <asp:TemplateField HeaderText="Customer">
                                <ItemTemplate>
                                    <asp:HyperLink ID="CustomerHyperLink" runat="server" Text='<%# GetCompanyName((long)Eval("CustomerID")) %>'
                                        NavigateUrl='<%# Eval("CustomerID", "~/Pages/ContactDetails.aspx?ID={0}") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="DateSentReminder" HeaderText="Date Reminder Sent"
                                SortExpression="DateSentReminder" DataFormatString="{0:d}"
                                ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Left" />
                            <asp:BoundField DataField="NextPreparationDate" HeaderText="Prep Date"
                                SortExpression="NextPreparationDate" DataFormatString="{0:d}"
                                ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Left" />
                            <asp:CheckBoxField DataField="ReminderSent" HeaderText="Reminder Sent"
                                ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Left" />
                            <asp:CheckBoxField DataField="HadAutoFulfilItem"
                                HeaderText="Had Auto FulfilItems" SortExpression="HadAutoFulfilItem"
                                ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Left" />
                            <asp:CheckBoxField DataField="HadRecurringItems" HeaderText="Had Recurring Items"
                                SortExpression="HadRecurringItems" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Center" />
                        </Columns>
                        <RowStyle Font-Size="Large" />
                        <EmptyDataTemplate>
                            <div class="status-message status-info" style="padding: 16px;">
                                No reminders for this date.
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                    <asp:Literal ID="ltrlReminderFooter" runat="server" Mode="PassThrough" />
                </div>

                <asp:Panel ID="pnlFailedEmails" runat="server" Visible="false" CssClass="failedEmailsSection">
                    <h3>Failed email reminders</h3>
                    <p>The following customers could not receive coffee checkup emails:</p>

                    <asp:GridView ID="gvFailedEmails" runat="server" CssClass="TblZebra"
                        AutoGenerateColumns="false"
                        EmptyDataText="No failed emails to display."
                        ShowHeader="true"
                        BorderStyle="None"
                        GridLines="None">
                        <Columns>
                            <asp:BoundField DataField="CustomerName" HeaderText="Customer"
                                SortExpression="CustomerName" ItemStyle-Width="40%" />
                            <asp:BoundField DataField="FailureReason" HeaderText="Failure Reason"
                                SortExpression="FailureReason" ItemStyle-Width="60%" />
                        </Columns>
                    </asp:GridView>

                    <div class="button-row" style="margin-top: 15px;">
                        <asp:Button ID="btnClearFailures" runat="server" Text="Clear Failed List" CssClass="filter-panel-btn"
                            OnClick="btnClearFailures_Click"
                            ToolTip="Clear the failed-email list for today" />
                    </div>
                    <p class="small" style="margin-top: 10px;">
                        Tip: Check customer email addresses and account status for failed entries.
                    </p>
                </asp:Panel>

                <div class="status-message" id="pnlStatus" runat="server" style="margin-top: 12px;">
                    <asp:Literal ID="ltrlStatus" runat="server" />
                </div>
            </asp:Panel>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="ddlFilterByDate" EventName="SelectedIndexChanged" />
            <asp:AsyncPostBackTrigger ControlID="btnRefresh" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnClearFailures" EventName="Click" />
            <asp:PostBackTrigger ControlID="btnBack" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
