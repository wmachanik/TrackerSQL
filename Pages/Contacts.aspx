<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Contacts.aspx.cs" Inherits="TrackerSQL.Pages.Contacts" %>

<asp:Content ID="cntContactsHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntContactsBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h1>List of Contacts</h1>
    <asp:ScriptManager ID="smContactSummary" runat="server"></asp:ScriptManager>
    <asp:UpdateProgress ID="uprgContactSummary" runat="server" AssociatedUpdatePanelID="upnlContactSummary">
        <ProgressTemplate>
            <img src="../images/animi/BlueArrowsUpdate.gif" alt="updating" width="16" height="16" />updating.....
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="upnlSelection" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="filter-toolbar">
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
                    <asp:Button ID="btnGon" Text="Go" runat="server" OnClick="btnGon_Click" ToolTip="search for this item" />
                    <asp:Button ID="btnReset" Text="Reset" runat="server"
                        OnClick="btnReset_Click" />
                </div>
                <div class="filter-section admin-controls">
                    <div class="filter-control">
                        <asp:DropDownList ID="ddlContactEnabled" runat="server" AutoPostBack="true">
                            <asp:ListItem Value="-1" Text="both" />
                            <asp:ListItem Selected="True" Value="1" Text="enabled only" />
                            <asp:ListItem Value="0" Text="disabled only" />
                        </asp:DropDownList>
                    </div>

                    <div class="filter-section action-buttons">
                        <asp:HyperLink ImageUrl="~/images/imgButtons/CalendarClock.gif" ToolTip="Contacts Away Times" 
                            NavigateUrl="~/Pages/CustomersAway.aspx" runat="server"  />
                        <asp:HyperLink ImageUrl="~/images/imgButtons/AddItem.gif" ToolTip="New Contact"
                            NavigateUrl="~/Pages/ContactDetails.aspx" runat="server" />
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="tbxFilterBy"
                EventName="TextChanged" />
            <asp:AsyncPostBackTrigger ControlID="btnGon" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
    <br />
    <asp:UpdatePanel ID="upnlContactSummary" runat="server">
        <ContentTemplate>
            <div class="results-container">
                <asp:GridView ID="gvContacts" runat="server" AutoGenerateColumns="False" CssClass="results-table"
                    AllowSorting="True" DataSourceID="odsContactSummaries" AllowPaging="True" CellPadding="0" CellSpacing="0"
                    PageSize="25" EmptyDataText="No contacts found.">
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
                        <asp:BoundField DataField="City" HeaderText="Area" SortExpression="City" ItemStyle-Font-Size="Smaller"
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
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnGon" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnReset" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="ddlContactEnabled" EventName="SelectedIndexChanged" />
        </Triggers>
    </asp:UpdatePanel>

    <asp:ObjectDataSource ID="odsContactSummaries"
        TypeName="TrackerDotNet.Classes.Sql.ContactSummariesRepository"
        SortParameterName="SortBy"
        SelectMethod="GetAllContactSummaries"
        OnSelecting="odsContactSummaries_Selecting"
        OnSelected="odsContactSummaries_Selected"
        runat="server" OldValuesParameterFormatString="original_{0}">
        <SelectParameters>
            <asp:Parameter DefaultValue="CompanyName" Name="SortBy" Type="String" />
            <asp:ControlParameter ControlID="ddlContactEnabled" DefaultValue="-1" Name="IsEnabled" PropertyName="SelectedValue" Type="Int32" />
            <asp:SessionParameter DefaultValue="" Name="WhereFilter" SessionField="ContactSummaryWhereFilter" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>

    <br />
    <asp:UpdatePanel runat="server" UpdateMode="Always">
        <ContentTemplate>
            <asp:Label ID="lblFilter" Text="" runat="server" />
        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>
