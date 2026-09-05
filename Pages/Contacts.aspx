<%@ Page Title="Contacts" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Contacts.aspx.cs" Inherits="TrackerSQL.Pages.Contacts" %>

<asp:Content ID="cntContactsHdr" title="Contacts" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="cntContactsBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smContactSummary" runat="server" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="uprgContactSummary" runat="server"
        AssociatedUpdatePanelID="upnlContactSummary" DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info" style="margin: 8px 0;">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="upnlContactSummary" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <asp:Panel ID="pnlContacts" runat="server" CssClass="simpleForm page-tone-panel page-tone-contacts">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/icons8-new-contact-48.png" alt="" />
                    <div>
                        <h1 class="page-tone-title">Contacts</h1>
                        <p class="page-tone-subtitle">Manage contact information and accounts</p>
                    </div>
                </div>

                <div class="page-tone-toolbar filter-toolbar">
                    <div class="filter-section search-controls">
                        <div class="filter-control">
                            <label for="<%=ddlFilterBy.ClientID%>">Filter by:</label>
                            <asp:DropDownList ID="ddlFilterBy" runat="server" ToolTip="select which item to search form">
                                <asp:ListItem Value="0" Selected="True" Text="none" />
                                <asp:ListItem Value="CompanyName" Text="Company Name" />
                                <asp:ListItem Value="ContactFirstName" Text="First Name" />
                                <asp:ListItem Value="EmailAddress" Text="Email" />
                                <asp:ListItem Value="PeopleTbl.Abbreviation" Text="Delivery By" />
                                <asp:ListItem Value="AreasTbl.AreaName" Text="Area" />
                                <asp:ListItem Value="EquipTypesTbl.EquipTypeName" Text="Equipment Type" />
                                <asp:ListItem Value="ContactsTbl.MachineSN" Text="Equipment S/N" />
                                <asp:ListItem Value="ContactID" Text="Contact ID" />
                            </asp:DropDownList>
                        </div>
                        <div class="filter-control">
                            <asp:TextBox ID="tbxFilterBy" runat="server"
                                ToolTip="add '%' to beginning to find contains"
                                OnTextChanged="tbxFilterBy_TextChanged" />
                        </div>
                        <asp:Button ID="btnGon" Text="Go" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnGon_Click" ToolTip="search for this item" />
                        <asp:Button ID="btnReset" Text="Reset" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnReset_Click" ToolTip="Clear filters" />
                    </div>
                    <div class="filter-section admin-controls">
                        <div class="filter-control">
                            <asp:DropDownList ID="ddlContactEnabled" runat="server" AutoPostBack="true"
                                OnSelectedIndexChanged="ddlContactEnabled_SelectedIndexChanged">
                                <asp:ListItem Value="-1" Text="both" />
                                <asp:ListItem Selected="True" Value="1" Text="enabled only" />
                                <asp:ListItem Value="0" Text="disabled only" />
                            </asp:DropDownList>
                        </div>
                        <div class="filter-section action-buttons toolbar-icon-row">
                            <asp:HyperLink ImageUrl="~/images/imgButtons/CalendarClock.gif" ToolTip="Contacts Away Times"
                                NavigateUrl="~/Pages/ContactsAway.aspx" runat="server" />
                            <asp:HyperLink ImageUrl="~/images/imgButtons/Add-Contact-Card.png" ToolTip="New contact"
                                NavigateUrl="~/Pages/ContactDetails.aspx" runat="server" />
                            <span class="image-button" title="Return to home">
                                <asp:ImageButton ID="btnBack" runat="server"
                                    ImageUrl="~/images/imgButtons/Back.gif"
                                    AlternateText="Back"
                                    ToolTip="Return to home"
                                    OnClick="btnBack_Click"
                                    CausesValidation="false" />
                            </span>
                        </div>
                    </div>
                </div>

                <div class="results-container" style="margin-top: 8px;">
                    <asp:GridView ID="gvContacts" runat="server" AutoGenerateColumns="False" CssClass="results-table"
                        AllowSorting="True" AllowPaging="True" CellPadding="0" CellSpacing="0"
                        PageSize="25" EmptyDataText="No contacts found."
                        OnPageIndexChanging="gvContacts_PageIndexChanging"
                        OnSorting="gvContacts_Sorting"
                        OnRowCreated="gvContacts_RowCreated">
                        <PagerStyle CssClass="pager-row" />
                        <PagerTemplate>
                            <asp:PlaceHolder ID="plhPager" runat="server" />
                        </PagerTemplate>
                        <Columns>
                            <asp:HyperLinkField DataNavigateUrlFields="CustomerID" DataNavigateUrlFormatString="~/Pages/ContactDetails.aspx?ID={0}"
                                DataTextField="CompanyName" HeaderText="Company Name" SortExpression="CompanyName"
                                HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1" />
                            <asp:BoundField DataField="CustomerID" HeaderText="ID" SortExpression="CustomerID" Visible="false" />
                            <asp:BoundField DataField="CompanyName" HeaderText="Company Name" SortExpression="CompanyName" Visible="false"
                                HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1" />
                            <asp:BoundField DataField="ContactFirstName" HeaderText="First Name" SortExpression="ContactFirstName"
                                HeaderStyle-CssClass="col-priority-2" ItemStyle-CssClass="col-priority-2" />
                            <asp:BoundField DataField="ContactLastName" HeaderText="Last Name" SortExpression="ContactLastName"
                                HeaderStyle-CssClass="col-priority-4" ItemStyle-CssClass="col-priority-4" />
                            <asp:BoundField DataField="AreaName" HeaderText="Area" SortExpression="AreaName" ItemStyle-Font-Size="Smaller"
                                HeaderStyle-CssClass="col-priority-3" ItemStyle-CssClass="col-priority-3" />
                            <asp:BoundField DataField="PhoneNumber" HeaderText="Phone" SortExpression="PhoneNumber" HeaderStyle-Font-Size="Small" ItemStyle-Font-Size="Small"
                                HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1" />
                            <asp:BoundField DataField="EmailAddress" HeaderText="Email Address"
                                HeaderStyle-CssClass="col-priority-3" ItemStyle-CssClass="col-priority-3" ItemStyle-Font-Size="Smaller" />
                            <asp:BoundField DataField="DeliveryBy" HeaderText="Delivery By" SortExpression="DeliveryBy"
                                HeaderStyle-CssClass="col-priority-3" ItemStyle-CssClass="col-priority-3" />
                            <asp:BoundField DataField="EquipTypeName" HeaderText="Equipment" SortExpression="EquipTypeName" ItemStyle-Font-Size="Smaller"
                                HeaderStyle-CssClass="col-priority-4" ItemStyle-CssClass="col-priority-4" />
                            <asp:BoundField DataField="MachineSN" HeaderText="Equip S/N" SortExpression="MachineSN" ItemStyle-Font-Size="Smaller"
                                HeaderStyle-CssClass="col-priority-5" ItemStyle-CssClass="col-priority-5" />
                            <asp:CheckBoxField DataField="autofulfill" HeaderText="Auto" SortExpression="autofulfill"
                                HeaderStyle-CssClass="col-priority-5" ItemStyle-CssClass="col-priority-5" />
                            <asp:CheckBoxField DataField="enabled" HeaderText="Enabled" SortExpression="enabled"
                                HeaderStyle-CssClass="col-priority-4" ItemStyle-CssClass="col-priority-4" />
                            <asp:HyperLinkField DataNavigateUrlFields="CustomerID" HeaderText="Order" ItemStyle-HorizontalAlign="Center"
                                HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1"
                                DataNavigateUrlFormatString="~/Pages/OrderDetail.aspx?CoID={0}&LastOrder=Y" Text="+last" />
                        </Columns>
                    </asp:GridView>
                </div>

                <div class="status-message" id="pnlStatus" runat="server" style="margin-top: 12px;">
                    <asp:Label ID="lblFilter" Text="" runat="server" />
                </div>
            </asp:Panel>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="tbxFilterBy" EventName="TextChanged" />
            <%-- Search/reset are FULL postbacks — async panel updates proved unreliable. --%>
            <asp:PostBackTrigger ControlID="btnGon" />
            <asp:PostBackTrigger ControlID="btnReset" />
            <asp:AsyncPostBackTrigger ControlID="ddlContactEnabled" EventName="SelectedIndexChanged" />
            <asp:PostBackTrigger ControlID="btnBack" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
