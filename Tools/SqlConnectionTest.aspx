<%@ Page Title="SQL Connection Test" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="SqlConnectionTest.aspx.cs" Inherits="TrackerSQL.Tools.SqlConnectionTest"
    MaintainScrollPositionOnPostback="true" %>

<asp:Content ID="cntSqlConnectionTestHdr" Title="SQL Connection Test" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="cntSqlConnectionTestBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlSqlConnectionTest" runat="server" CssClass="simpleForm page-tone-panel page-tone-tools">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/Table.png" alt="" />
            <div>
                <h1 class="page-tone-title">SQL Connection Test</h1>
                <p class="page-tone-subtitle">Measure connect time and verify TrackerDataSQL (or a trial connection string)</p>
            </div>
        </div>

        <div class="status-message status-info" style="margin-bottom: 12px;">
            This page is available without login so you can diagnose SQL when membership login fails.
            Edits below are for this test only - they are not written to Web.config.
            The password is shown as ********; leave it that way to keep the configured password, or type a new one to try.
        </div>

        <table class="detail-form-table" style="margin-top: 8px; width: 100%;">
            <tr>
                <td style="width: 10em; vertical-align: top;">Connection string</td>
                <td>
                    <asp:TextBox ID="txtConnectionString" runat="server" TextMode="MultiLine"
                        Rows="4" Width="98%" CssClass="col-ro-ddl"
                        ToolTip="Defaults to TrackerDataSQL from Web.config (password masked as ********). Edit to try another string." />
                </td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td>
                    <asp:CheckBox ID="chkRunProbeQuery" runat="server" Checked="true"
                        Text="Also run SELECT @@VERSION / DB_NAME() after Open" />
                </td>
            </tr>
        </table>

        <div class="page-tone-toolbar button-row" style="margin-top: 12px;">
            <asp:Button ID="btnTest" runat="server" Text="Test Connection" CssClass="filter-panel-btn"
                OnClick="btnTest_Click" CausesValidation="false"
                ToolTip="Open SQL connection and report elapsed milliseconds" />
            <asp:Button ID="btnTestContacts" runat="server" Text="Time Contacts Query" CssClass="filter-panel-btn"
                OnClick="btnTestContacts_Click" CausesValidation="false" Enabled="false"
                ToolTip="Run the company-name dropdown query (enabled after a successful connection test)" />
            <asp:Button ID="btnReloadConfig" runat="server" Text="Reload from Web.config" CssClass="filter-panel-btn"
                OnClick="btnReloadConfig_Click" CausesValidation="false"
                ToolTip="Reset the text box to TrackerDataSQL from Web.config" />
            <asp:Button ID="btnBack" runat="server" Text="Back" CssClass="filter-panel-btn"
                OnClick="btnBack_Click" CausesValidation="false"
                ToolTip="Return to System Tools (requires login)" />
        </div>

        <div class="page-tone-footer" style="margin-top: 16px;">
            <div class="status-message" id="pnlStatus" runat="server">
                <asp:Literal ID="ltrlStatus" runat="server" />
            </div>
            <asp:Panel ID="pnlResults" runat="server" Visible="false" CssClass="results-container" style="margin-top: 12px;">
                <asp:Literal ID="ltrlResults" runat="server" />
            </asp:Panel>
        </div>
    </asp:Panel>
</asp:Content>
