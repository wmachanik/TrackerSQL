<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="HolidayClosures.aspx.cs" Inherits="TrackerDotNet.Tools.HolidayClosures"
    Title="Holiday / Closure Dates" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <h1>Holiday / Closure Dates</h1>
    <asp:ScriptManager ID="smHolidayClosures" runat="server" />

    <asp:UpdateProgress ID="upgHolidayClosures" runat="server" AssociatedUpdatePanelID="upnlHolidayClosures">
        <ProgressTemplate>
            <img src="../images/animi/BlueArrowsUpdate.gif" alt="updating" width="16" height="16" />
            updating ...
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="upnlHolidayClosures" runat="server" UpdateMode="Conditional">
        <ContentTemplate>

            <div class="filter-toolbar">
                <div class="filter-section search-controls">
                    <div class="filter-control">
                        <asp:Label AssociatedControlID="ddlFilterStrategy" Text="Strategy:" runat="server" />
                        <asp:DropDownList ID="ddlFilterStrategy" runat="server" Width="100">
                            <asp:ListItem Text="(all)" Value="" Selected="True" />
                            <asp:ListItem>Forward</asp:ListItem>
                            <asp:ListItem>Backward</asp:ListItem>
                            <asp:ListItem>Skip</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="filter-control">
                        <asp:Label AssociatedControlID="txtFilterText" Text="Text:" runat="server" />
                        <asp:TextBox ID="txtFilterText" runat="server" Width="160" ToolTip="Filter description contains" />
                    </div>
                    <div class="filter-control">
                        <asp:Button ID="btnFilter" runat="server" Text="Filter" OnClick="btnFilter_Click" />
                        <asp:Button ID="btnReset" runat="server" Text="Reset" OnClick="btnReset_Click" />
                    </div>
                </div>

                <div class="filter-section date-controls">
                    <div class="filter-control">
                        <asp:Label AssociatedControlID="ddlDateRange" Text="Range:" runat="server" />
                        <asp:DropDownList ID="ddlDateRange" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlDateRange_SelectedIndexChanged">
                            <asp:ListItem Value="ThisYear" Text="This Year" Selected="True" />
                            <asp:ListItem Value="NextYear" Text="Next Year" />
                            <asp:ListItem Value="LastYear" Text="Last Year" />
                            <asp:ListItem Value="Current6" Text="Current 6 Months" />
                        </asp:DropDownList>
                    </div>
                    <div class="filter-control">
                        <asp:Button ID="btnCopyToNextYear" runat="server" Text="Copy To Next Year"
                            OnClick="btnCopyToNextYear_Click"
                            OnClientClick="return confirm('Copy all displayed closures to the next year?');"
                            ToolTip="Duplicate displayed closures into next year (skips duplicates)" />
                    </div>
                </div>

                <div class="filter-section action-buttons">
                    <asp:Button ID="btnShowAddPanel" runat="server" Text="New (Inline)" OnClick="btnShowAddPanel_Click" />
                    &nbsp;
                    <asp:HyperLink ID="lnkAddDetail" runat="server"
                        ImageUrl="~/images/imgButtons/AddItem.gif"
                        NavigateUrl="HolidayClosureDetail.aspx"
                        Text="Add / Manage (Detail Page)" />
                </div>
            </div>
            <br />

            <asp:Panel ID="pnlAddInline" runat="server" CssClass="simpleForm" Visible="false">
                <fieldset>
                    <legend>New Closure</legend>
                    <table class="TblCoffee">
                        <tr>
                            <td>Date (start)</td>
                            <td>
                                <asp:TextBox ID="txtNewDate" runat="server" Width="100" />
                                <ajaxToolkit:CalendarExtender ID="calNewDate" runat="server"
                                    TargetControlID="txtNewDate" Format="yyyy-MM-dd" FirstDayOfWeek="Monday" />
                            </td>
                            <td>Days</td>
                            <td>
                                <asp:TextBox ID="txtNewDays" runat="server" Width="40" Text="1" /></td>
                            <td>Strategy</td>
                            <td>
                                <asp:DropDownList ID="ddlNewStrategy" runat="server">
                                    <asp:ListItem>Forward</asp:ListItem>
                                    <asp:ListItem>Backward</asp:ListItem>
                                    <asp:ListItem>Skip</asp:ListItem>
                                </asp:DropDownList>
                            </td>
                            <td>
                                <asp:CheckBox ID="chkNewPrep" runat="server" Text="Prep" Checked="true" /></td>
                            <td>
                                <asp:CheckBox ID="chkNewDelivery" runat="server" Text="Delivery" Checked="true" /></td>
                        </tr>
                        <tr>
                            <td>Description</td>
                            <td colspan="5">
                                <asp:TextBox ID="txtNewDesc" runat="server" Width="420" MaxLength="255" />
                            </td>
                            <td colspan="2" style="text-align: right">
                                <asp:Button ID="btnAddInline" runat="server" Text="Add" OnClick="btnAddInline_Click" />
                                &nbsp;
                                <asp:Button ID="btnCancelInline" runat="server" Text="Cancel" OnClick="btnCancelInline_Click" />
                            </td>
                        </tr>
                    </table>
                </fieldset>
                <br />
            </asp:Panel>

            <div class="results-container">
                <asp:GridView ID="gvClosures" runat="server" CssClass="results-table"
                    AutoGenerateColumns="False" AllowSorting="true" AllowPaging="true"
                    PageSize="25" DataKeyNames="ID"
                    OnPageIndexChanging="gvClosures_PageIndexChanging"
                    OnRowDeleting="gvClosures_RowDeleting"
                    OnSorting="gvClosures_Sorting">

                    <Columns>
                        <asp:BoundField DataField="ClosureDate" HeaderText="Start" DataFormatString="{0:yyyy-MM-dd}" SortExpression="ClosureDate" />
                        <asp:BoundField DataField="EndDate" HeaderText="End" DataFormatString="{0:yyyy-MM-dd}" SortExpression="EndDate" />
                        <asp:BoundField DataField="DaysClosed" HeaderText="Days" SortExpression="DaysClosed" />
                        <asp:CheckBoxField DataField="AppliesToPrep" HeaderText="Prep" />
                        <asp:CheckBoxField DataField="AppliesToDelivery" HeaderText="Delivery" />
                        <asp:BoundField DataField="ShiftStrategy" HeaderText="Strategy" SortExpression="ShiftStrategy" />
                        <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                        <asp:TemplateField HeaderText="Edit">
                            <ItemTemplate>
                                <asp:HyperLink ID="lnkEdit" runat="server"
                                    ImageUrl="~/images/imgButtons/EditItem.gif"
                                    NavigateUrl='<%# "HolidayClosureDetail.aspx?ID=" + Eval("ID") %>' Text="edit" />
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center" />
                            <HeaderStyle HorizontalAlign="Center" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Del">
                            <ItemTemplate>
                                <asp:ImageButton ID="btnDelete" runat="server"
                                    ImageUrl="~/images/imgButtons/DelItem.gif"
                                    AlternateText="Delete"
                                    ToolTip="Delete this closure"
                                    CommandName="Delete"
                                    CausesValidation="false"
                                    OnClientClick="return confirm('Delete this closure?');" />
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center" />
                            <HeaderStyle HorizontalAlign="Center" />
                        </asp:TemplateField>
                    </Columns>

                </asp:GridView>
            </div>

            <div class="status-message">
                <asp:Literal ID="ltrlStatus" runat="server" />
            </div>

        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
