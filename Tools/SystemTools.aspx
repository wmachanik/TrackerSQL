<%@ Page Title="System Tools" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="SystemTools.aspx.cs" Inherits="TrackerDotNet.Tools.SystemTools" MaintainScrollPositionOnPostback="true" %>

<asp:Content ID="cntSystemToolsHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntSystemToolsBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h1>General System Tools</h1>
    <br />
    <asp:ScriptManager ID="tsmSystemTools" runat="server" />
    <asp:UpdateProgress ID="uprgSystemTools" runat="server" AssociatedUpdatePanelID="upnlSystemToolsButtons" EnableViewState="true" Visible="true">
        <ProgressTemplate>
            <img src="../images/animi/BlueArrowsUpdate.gif" alt="updating" width="16" height="16" />updating.....
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="upnlSystemToolsButtons" runat="server" ChildrenAsTriggers="true" UpdateMode="Always" ViewStateMode="Enabled">
        <ContentTemplate>
            <div class="dashboard-links tools-dashboard">
                <div class="dashboard-links">
                    <div class="dashboard-link">
                        <div class="dashboard-card">
                            <h4>Set Client Type</h4>
                            <p>Update client prediction flags</p>
                            <asp:Button ID="btnSetClientType" runat="server" Text="Open" OnClick="btnSetClientType_Click" />
                        </div>
                    </div>

                    <div class="dashboard-link">
                        <div class="dashboard-card">
                            <h4>XML file to SQL</h4>
                            <p>Import data from XML</p>
                            <asp:Button ID="btnXMLTOSQL" runat="server" Text="Open" PostBackUrl="~/Tools/XMLtoSQL.aspx" />
                        </div>
                    </div>

                    <div class="dashboard-link">
                        <div class="dashboard-card">
                            <h4>Reset Prep/Delivery Date</h4>
                            <p>Recalculate next dates</p>
                            <asp:Button ID="btnResetPrepDates" runat="server" Text="Run" OnClick="btnResetPrepDates_Click" />
                        </div>
                    </div>

                    <div class="dashboard-link">
                        <div class="dashboard-card">
                            <h4>Move Delivery Date</h4>
                            <p>Shift delivery schedule</p>
                            <asp:Button ID="btnMoveDlvryDate" runat="server" Text="Open" PostBackUrl="~/Tools/MoveDeliveryDate.aspx" />
                        </div>
                    </div>

                    <div class="dashboard-link">
                        <div class="dashboard-card">
                            <h4>Holiday / Closure Dates</h4>
                            <p>Manage closure calendar</p>
                            <asp:Button ID="btnHolidayClosures" runat="server" Text="Open" PostBackUrl="~/Tools/HolidayClosures.aspx" ToolTip="Add or remove roast / delivery closure dates" />
                        </div>
                    </div>

                    <div class="dashboard-link">
                        <div class="dashboard-card">
                            <h4>System Data</h4>
                            <p>Manage system settings</p>
                            <asp:Button ID="btnEditSystemData" runat="server" Text="Open" PostBackUrl="~/Tools/SystemData.aspx" />
                        </div>
                    </div>

                    <div class="dashboard-link">
                        <div class="dashboard-card">
                            <h4>Log Viewer</h4>
                            <p>Review system logs</p>
                            <asp:Button ID="btnLogViewer" runat="server" Text="Open" PostBackUrl="~/Tools/LogViewer.aspx" />
                        </div>
                    </div>

                    <div class="dashboard-link">
                        <div class="dashboard-card">
                            <h4>Merge Customers From QB</h4>
                            <p>Sync accounting data</p>
                            <asp:Button ID="btnMergQBAccData" runat="server" Text="Open" PostBackUrl="~/Tools/MergeCustomersFromQB.aspx" />
                        </div>
                    </div>

                    <div class="dashboard-link">
                        <div class="dashboard-card">
                            <h4>Email Diagnostics</h4>
                            <p>Test SMTP/email</p>
                            <asp:Button ID="btnEmailDiagnostics" runat="server" Text="Open" PostBackUrl="~/Tools/EmailDiagnostics.aspx" ToolTip="Test SMTP and email settings" />
                        </div>
                    </div>

                    <div class="dashboard-link">
                        <div class="dashboard-card">
                            <h4>Set Last Order Date</h4>
                            <p>Update recurring orders</p>
                            <asp:Button ID="btnSetLastOrderDate" runat="server" Text="Run" OnClick="btnSetLastOrderDate_Click" />
                        </div>
                    </div>

                    <div class="dashboard-link">
                        <div class="dashboard-card">
                            <h4>Messages Editor</h4>
                            <p>Edit resource messages</p>
                            <asp:Button ID="btnMessagesEditor" runat="server" Text="Open" PostBackUrl="~/Tools/MessagesEditor.aspx" />
                        </div>
                    </div>
                    <div class="dashboard-link">
                        <div class="dashboard-card">
                            <h4>Disable Inactive Clients</h4>
                            <p>Disable clients with no orders in 3+ years</p>
                            <asp:Button ID="btnDisableInactiveClients" runat="server" Text="Run"
                                OnClick="btnDisableInactiveClients_Click"
                                ToolTip="Disables enabled customers whose last usage/order is older than 3 years" />
                        </div>
                    </div>
                </div>
            </div>

            <asp:Panel ID="pnlResultsSection" CssClass="results-container" Visible="false" runat="server">
                <div class="status-message">
                    <asp:Literal ID="ltrlStatus" runat="server" Visible="false" Text="" />
                </div>
                <br />
                <asp:Panel ID="pnlSetClientType" runat="server" Visible="false">
                    <table class="TblWhite" width="100%">
                        <tr valign="top">
                            <td>
                                <asp:Label ID="ResultsTitleLabel" runat="server" CssClass="title" Text="" />
                                <asp:GridView ID="gvResults" runat="server" AutoGenerateColumns="true" AllowSorting="true" CssClass="TblCoffee">
                                </asp:GridView>
                            </td>
                            <td>
                                <asp:GridView ID="gvCustomerTypes" runat="server" AllowSorting="True" CssClass="TblZebra"
                                    AutoGenerateColumns="False" DataSourceID="odsCustomerTypes" Visible="false">
                                    <Columns>
                                        <asp:BoundField DataField="CustTypeID" HeaderText="CustTypeID" SortExpression="CustTypeID" />
                                        <asp:BoundField DataField="CustTypeDesc" HeaderText="CustTypeDesc" SortExpression="CustTypeDesc" />
                                        <asp:BoundField DataField="Notes" HeaderText="Notes" SortExpression="Notes" />
                                    </Columns>
                                </asp:GridView>
                                <asp:ObjectDataSource ID="odsCustomerTypes" runat="server" SortParameterName="SortBy"
                                    SelectMethod="GetAll" TypeName="TrackerDotNet.Controls.CustomerTypeTbl"></asp:ObjectDataSource>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
                <asp:Panel ID="pnlResetPrepDate" runat="server" Visible="false">
                    <asp:GridView ID="gvCityPrepDates" runat="server" AllowPaging="True" CssClass="TblMudZebra"
                        AutoGenerateColumns="False" DataSourceID="sdsCityPrepDates">
                        <Columns>
                            <asp:BoundField DataField="City" HeaderText="City" SortExpression="City" />
                            <asp:BoundField DataField="PreperationDate" DataFormatString="{0:d}" HeaderText="PreperationDate" SortExpression="PreperationDate" />
                            <asp:BoundField DataField="DeliveryDate" DataFormatString="{0:d}" HeaderText="DeliveryDate" SortExpression="DeliveryDate" />
                            <asp:BoundField DataField="NextPreperationDate" DataFormatString="{0:d}" HeaderText="NextPreperationDate" SortExpression="NextPreperationDate" />
                            <asp:BoundField DataField="NextDeliveryDate" DataFormatString="{0:d}" HeaderText="NextDeliveryDate" SortExpression="NextDeliveryDate" />
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="sdsCityPrepDates" runat="server"
                        ConnectionString="<%$ ConnectionStrings:Tracker08ConnectionString %>"
                        ProviderName="<%$ ConnectionStrings:Tracker08ConnectionString.ProviderName %>"
                        SelectCommand="SELECT CityTbl.City, NextRoastDateByCityTbl.PreperationDate, NextRoastDateByCityTbl.DeliveryDate, NextRoastDateByCityTbl.NextPreperationDate, NextRoastDateByCityTbl.NextDeliveryDate FROM (NextRoastDateByCityTbl LEFT OUTER JOIN CityTbl ON NextRoastDateByCityTbl.CityID = CityTbl.ID) ORDER BY CityTbl.City"></asp:SqlDataSource>
                </asp:Panel>
            </asp:Panel>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnSetClientType" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="gvResults" EventName="DataBound" />
            <asp:AsyncPostBackTrigger ControlID="btnResetPrepDates" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="gvCityPrepDates" EventName="DataBound" />
            <asp:AsyncPostBackTrigger ControlID="btnSetLastOrderDate" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnDisableInactiveClients" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
