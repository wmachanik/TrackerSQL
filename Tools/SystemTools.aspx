<%@ Page Title="System Tools" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="SystemTools.aspx.cs" Inherits="TrackerSQL.Tools.SystemTools" MaintainScrollPositionOnPostback="true" %>

<asp:Content ID="cntSystemToolsHdr" title="System Tools" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntSystemToolsBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="tsmSystemTools" runat="server" />

    <asp:Panel ID="pnlSystemTools" runat="server" CssClass="simpleForm page-tone-panel page-tone-tools">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/Toolbox.png" alt="" />
            <div>
                <h1 class="page-tone-title">System Tools</h1>
                <p class="page-tone-subtitle">General maintenance and diagnostics</p>
            </div>
        </div>

        <asp:UpdateProgress ID="uprgSystemTools" runat="server" AssociatedUpdatePanelID="upnlSystemToolsButtons"
            DisplayAfter="0" EnableViewState="true" Visible="true">
            <ProgressTemplate>
                <div class="status-message status-info page-tone-progress">
                    <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                    &nbsp;Please wait...
                </div>
            </ProgressTemplate>
        </asp:UpdateProgress>

        <asp:UpdatePanel ID="upnlSystemToolsButtons" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional" ViewStateMode="Enabled">
            <ContentTemplate>
                <div class="dashboard-links tools-dashboard">
                    <div class="dashboard-links">

                        <div class="dashboard-link tool-tone-xml">
                            <div class="dashboard-card">
                                <div class="tool-card-header">
                                    <img class="tool-card-icon" src="../images/imgButtons/Table.png" alt="" />
                                    <h4>XML file to SQL</h4>
                                </div>
                                <p>Import data from XML</p>
                                <asp:Button ID="btnXMLTOSQL" runat="server" Text="Open" PostBackUrl="~/Tools/XMLtoSQL.aspx" />
                            </div>
                        </div>

                        <div class="dashboard-link tool-tone-reset">
                            <div class="dashboard-card">
                                <div class="tool-card-header">
                                    <img class="tool-card-icon" src="../images/imgButtons/Copilot_ResetPrepDate_32.png" alt="" />
                                    <h4>Reset Prep/Delivery Date</h4>
                                </div>
                                <p>Recalculate next dates</p>
                                <asp:Button ID="btnResetPrepDates" runat="server" Text="Run" OnClick="btnResetPrepDates_Click" />
                            </div>
                        </div>

                        <div class="dashboard-link tool-tone-move">
                            <div class="dashboard-card">
                                <div class="tool-card-header">
                                    <img class="tool-card-icon" src="../images/imgButtons/icons8-move-date.png" alt="" />
                                    <h4>Move Delivery Date</h4>
                                </div>
                                <p>Shift delivery schedule</p>
                                <asp:Button ID="btnMoveDlvryDate" runat="server" Text="Open" PostBackUrl="~/Tools/MoveDeliveryDate.aspx" />
                            </div>
                        </div>

                        <div class="dashboard-link tool-tone-holiday">
                            <div class="dashboard-card">
                                <div class="tool-card-header">
                                    <img class="tool-card-icon" src="../images/imgButtons/Calendar.gif" alt="" />
                                    <h4>Holiday / Closure Dates</h4>
                                </div>
                                <p>Manage closure calendar</p>
                                <asp:Button ID="btnHolidayClosures" runat="server" Text="Open" PostBackUrl="~/Tools/HolidayClosures.aspx" ToolTip="Add or remove roast / delivery closure dates" />
                            </div>
                        </div>

                        <div class="dashboard-link tool-tone-sysdata">
                            <div class="dashboard-card">
                                <div class="tool-card-header">
                                    <img class="tool-card-icon" src="../images/imgButtons/Toolbox.png" alt="" />
                                    <h4>System Preferences</h4>
                                </div>
                                <p>General settings and WooCommerce integration</p>
                                <asp:Button ID="btnEditSystemData" runat="server" Text="Open" PostBackUrl="~/Tools/SystemPreferences.aspx" />
                            </div>
                        </div>

                        <div class="dashboard-link tool-tone-sysdata" id="pnlWooMappingTool" runat="server">
                            <div class="dashboard-card">
                                <div class="tool-card-header">
                                    <img class="tool-card-icon" src="../images/imgButtons/Toolbox.png" alt="" />
                                    <h4>WooCommerce Mapping</h4>
                                </div>
                                <p><asp:Literal ID="litWooMappingToolBlurb" runat="server" Text="Categories, item SKU maps, and enabled sync" /></p>
                                <asp:Button ID="btnWooMapping" runat="server" Text="Open" OnClick="btnWooMapping_Click" />
                            </div>
                        </div>

                        <div class="dashboard-link tool-tone-logs">
                            <div class="dashboard-card">
                                <div class="tool-card-header">
                                    <img class="tool-card-icon" src="../images/imgButtons/View.png" alt="" />
                                    <h4>Log Viewer</h4>
                                </div>
                                <p>Review system logs</p>
                                <asp:Button ID="btnLogViewer" runat="server" Text="Open" PostBackUrl="~/Tools/LogViewer.aspx" />
                            </div>
                        </div>

                        <div class="dashboard-link tool-tone-email">
                            <div class="dashboard-card">
                                <div class="tool-card-header">
                                    <img class="tool-card-icon" src="../images/imgButtons/World.gif" alt="" />
                                    <h4>Email Diagnostics</h4>
                                </div>
                                <p>Test SMTP/email</p>
                                <asp:Button ID="btnEmailDiagnostics" runat="server" Text="Open" PostBackUrl="~/Tools/EmailDiagnostics.aspx" ToolTip="Test SMTP and email settings" />
                            </div>
                        </div>

                        <div class="dashboard-link tool-tone-lastorder">
                            <div class="dashboard-card">
                                <div class="tool-card-header">
                                    <img class="tool-card-icon" src="../images/imgButtons/icons8-set-min-date-30.png" alt="" />
                                    <h4>Set Last Recurring Order Date</h4>
                                </div>
                                <p>Sync DateLastDone from item usage</p>
                                <asp:Button ID="btnSetLastOrderDate" runat="server" Text="Run" OnClick="btnSetLastOrderDate_Click" />
                            </div>
                        </div>

                        <div class="dashboard-link tool-tone-messages">
                            <div class="dashboard-card">
                                <div class="tool-card-header">
                                    <img class="tool-card-icon" src="../images/imgButtons/EditItem.gif" alt="" />
                                    <h4>Messages Editor</h4>
                                </div>
                                <p>Edit resource messages</p>
                                <asp:Button ID="btnMessagesEditor" runat="server" Text="Open" PostBackUrl="~/Tools/MessagesEditor.aspx" />
                            </div>
                        </div>

                        <div class="dashboard-link tool-tone-backup">
                            <div class="dashboard-card">
                                <div class="tool-card-header">
                                    <img class="tool-card-icon" src="../images/imgButtons/icons8-data-backup-30.png" alt="" />
                                    <h4>Database Backup</h4>
                                </div>
                                <p>Backup OtterDb to App_Data\Backup</p>
                                <asp:Button ID="btnDatabaseBackup" runat="server" Text="Open"
                                    PostBackUrl="~/Tools/DatabaseBackup.aspx"
                                    ToolTip="Create and manage timestamped database backups" />
                            </div>
                        </div>

                        <div class="dashboard-link tool-tone-sysdata">
                            <div class="dashboard-card">
                                <div class="tool-card-header">
                                    <img class="tool-card-icon" src="../images/imgButtons/Table.png" alt="" />
                                    <h4>SQL Connection Test</h4>
                                </div>
                                <p>Time Open() and verify SQL connectivity</p>
                                <asp:Button ID="btnSqlConnectionTest" runat="server" Text="Open"
                                    PostBackUrl="~/Tools/SqlConnectionTest.aspx"
                                    ToolTip="Measure SQL connect time; works without login" />
                            </div>
                        </div>

                        <div class="dashboard-link tool-tone-disable">
                            <div class="dashboard-card">
                                <div class="tool-card-header">
                                    <img class="tool-card-icon" src="../images/imgButtons/LockItem.gif" alt="" />
                                    <h4>Disable Inactive Clients</h4>
                                </div>
                                <p>Disable clients with no orders in 3+ years</p>
                                <asp:Button ID="btnDisableInactiveClients" runat="server" Text="Run"
                                    OnClick="btnDisableInactiveClients_Click"
                                    ToolTip="Disables enabled customers whose last usage/order is older than 3 years" />
                            </div>
                        </div>

                    </div>
                </div>

                <asp:Panel ID="pnlResultsSection" CssClass="results-container page-tone-results" Visible="false" runat="server">
                    <asp:Panel ID="pnlToolResults" runat="server" Visible="false">
                        <asp:Label ID="ResultsTitleLabel" runat="server" CssClass="title" Text="" />
                        <asp:GridView ID="gvResults" runat="server" AutoGenerateColumns="true" AllowSorting="true" CssClass="results-table">
                        </asp:GridView>
                    </asp:Panel>
                    <asp:Panel ID="pnlResetPrepDate" runat="server" Visible="false">
                        <asp:GridView ID="gvAreaPrepDates" runat="server" AllowPaging="True" CssClass="TblMudZebra"
                            AutoGenerateColumns="False">
                            <Columns>
                                <asp:BoundField DataField="Area" HeaderText="Area" SortExpression="Area" />
                                <asp:BoundField DataField="PreparationDate" DataFormatString="{0:d}" HeaderText="Preparation Date" SortExpression="PreparationDate" />
                                <asp:BoundField DataField="DeliveryDate" DataFormatString="{0:d}" HeaderText="Delivery Date" SortExpression="DeliveryDate" />
                                <asp:BoundField DataField="NextPreparationDate" DataFormatString="{0:d}" HeaderText="Next Preparation Date" SortExpression="NextPreparationDate" />
                                <asp:BoundField DataField="NextDeliveryDate" DataFormatString="{0:d}" HeaderText="Next Delivery Date" SortExpression="NextDeliveryDate" />
                            </Columns>
                        </asp:GridView>
                    </asp:Panel>
                    <div class="page-tone-footer">
                        <div class="status-message" id="pnlStatus" runat="server">
                            <asp:Literal ID="ltrlStatus" runat="server" />
                        </div>
                    </div>
                </asp:Panel>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnResetPrepDates" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnSetLastOrderDate" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnDisableInactiveClients" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>
    </asp:Panel>
</asp:Content>
