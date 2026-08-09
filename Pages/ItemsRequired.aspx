<%@ Page Title="Items Required" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="ItemsRequired.aspx.cs" Inherits="TrackerSQL.Pages.ItemsRequired" %>

<asp:Content ID="cntItemsRequiredHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntItemsRequiredBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smgrRequired" runat="server" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="uppgRequired" runat="server" AssociatedUpdatePanelID="upnlRequired"
        DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info page-tone-progress">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="upnlRequired" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="GoBtn" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="ResetBtn" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="PrevWeekBtn" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="NextWeekBtn" EventName="Click" />
            <asp:PostBackTrigger ControlID="btnBack" />
        </Triggers>
        <ContentTemplate>
            <asp:Panel ID="pnlItemsRequired" runat="server" CssClass="simpleForm page-tone-panel page-tone-required">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/icons8-required-sheet-16.png" alt="" />
                    <div>
                        <h1 class="page-tone-title">Items Required</h1>
                        <p class="page-tone-subtitle">Summary by preparation and delivery day</p>
                    </div>
                </div>

                <div class="page-tone-toolbar">
                    <table class="TblLHCol-brown detail-form-table date-range-toolbar" style="margin: 0;">
                        <tr>
                            <td class="TblLHCol-first">Date From:</td>
                            <td>
                                <asp:TextBox ID="tbxDateFrom" runat="server" CssClass="date-field-clean" />
                                <span class="image-button" title="Pick from date">
                                    <asp:ImageButton ID="btnCalendarFrom" runat="server"
                                        ImageUrl="~/images/imgButtons/CalendarBtn.png"
                                        AlternateText="Calendar" ToolTip="Pick from date" />
                                    <ajaxToolkit:CalendarExtender ID="tbxDateFrom_CalendarExtender" runat="server"
                                        Enabled="True" TargetControlID="tbxDateFrom"
                                        PopupButtonID="btnCalendarFrom" PopupPosition="BottomLeft"
                                        Format="yyyy-MM-dd" />
                                </span>
                            </td>
                        </tr>
                        <tr>
                            <td class="TblLHCol-first">Date To:</td>
                            <td>
                                <asp:TextBox ID="tbxDateTo" runat="server" CssClass="date-field-clean" />
                                <span class="image-button" title="Pick to date">
                                    <asp:ImageButton ID="btnCalendarTo" runat="server"
                                        ImageUrl="~/images/imgButtons/CalendarBtn.png"
                                        AlternateText="Calendar" ToolTip="Pick to date" />
                                    <ajaxToolkit:CalendarExtender ID="tbxDateTo_CalendarExtender" runat="server"
                                        Enabled="True" TargetControlID="tbxDateTo"
                                        PopupButtonID="btnCalendarTo" PopupPosition="BottomLeft"
                                        Format="yyyy-MM-dd" />
                                </span>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2" style="text-align: center; padding-top: 12px;">
                                <div class="button-row">
                                    <asp:Button ID="GoBtn" Text="Go" runat="server" CssClass="filter-panel-btn"
                                        OnClick="GoBtn_Click" ToolTip="Load items for the selected dates" />
                                    <asp:Button ID="ResetBtn" Text="Reset Dates" runat="server" CssClass="filter-panel-btn"
                                        OnClick="ResetBtn_Click" ToolTip="Reset to the current week" />
                                    <asp:Button ID="PrevWeekBtn" Text="Prev Week" runat="server" CssClass="filter-panel-btn"
                                        OnClick="PrevWeekBtn_Click" ToolTip="Move date range back one week" />
                                    <asp:Button ID="NextWeekBtn" Text="Next Week" runat="server" CssClass="filter-panel-btn"
                                        OnClick="NextWeekBtn_Click" ToolTip="Move date range forward one week" />
                                    <asp:Button ID="btnBack" runat="server" Text="Back" CssClass="filter-panel-btn"
                                        OnClick="btnBack_Click" CausesValidation="false" ToolTip="Return to home" />
                                </div>
                            </td>
                        </tr>
                    </table>
                </div>

                <ajaxToolkit:TabContainer ID="tabRequired" runat="server" ActiveTabIndex="0">
                    <ajaxToolkit:TabPanel ID="pnlByPrepDate" runat="server" HeaderText="By Prep Date" TabIndex="0">
                        <HeaderTemplate>Items Summary by Prep Date</HeaderTemplate>
                        <ContentTemplate>
                            <h2>Items Required Summary by Preparation Day</h2>
                            <p>
                                Order qty is in kg. Pack qty = kg / pack size (250g / 275g / 500g / blank = 1kg).
                                Example: 0.5 kg as 250g packs = 2 packs. Total is kg.
                            </p>
                            <asp:GridView ID="gvPreparationDay" runat="server" AutoGenerateColumns="False" ShowFooter="true"
                                CssClass="results-table" EmptyDataText="No open order lines for this prep-date range."
                                OnRowDataBound="gvPreparationDay_RowDataBound">
                                <Columns>
                                    <asp:TemplateField HeaderText="Prep Date" SortExpression="PrepDate">
                                        <ItemTemplate>
                                            <asp:Label ID="lblPrepDate" runat="server" Text='<%# Eval("PrepDate", "{0:yyyy-MM-dd}") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <b>Total</b>
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="ItemDesc" HeaderText="Item Description" SortExpression="ItemDesc" />
                                    <asp:BoundField DataField="ItemPackagingDesc" HeaderText="Packaging" SortExpression="ItemPackagingDesc" />
                                    <asp:TemplateField HeaderText="Packs" SortExpression="PackQty"
                                        ItemStyle-HorizontalAlign="Right" FooterStyle-HorizontalAlign="Right">
                                        <ItemTemplate>
                                            <asp:Label ID="lblPackQty" runat="server" Text='<%# Eval("PackQty", "{0:0.0}") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:Label ID="lblFooterPackQty" runat="server" Font-Bold="true" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Total kg" SortExpression="Qty"
                                        ItemStyle-HorizontalAlign="Right" FooterStyle-HorizontalAlign="Right">
                                        <ItemTemplate>
                                            <asp:Label ID="lblQty" runat="server" Text='<%# Eval("Qty", "{0:0.##}") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:Label ID="lblFooterQty" runat="server" Font-Bold="true" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </ContentTemplate>
                    </ajaxToolkit:TabPanel>
                    <ajaxToolkit:TabPanel ID="tabpnlRequiredDetail" runat="server" HeaderText="Required By Delivery Date" TabIndex="1">
                        <ContentTemplate>
                            <h2>Items Required Detail</h2>
                            <p>
                                Order qty is in kg. Pack qty = kg / pack size (250g / 275g / 500g / blank = 1kg).
                                Example: 0.5 kg as 250g packs = 2 packs. Total is kg.
                            </p>
                            <asp:GridView ID="gvItemsRequiredByDay" runat="server" AutoGenerateColumns="False" ShowFooter="true"
                                CssClass="results-table" EmptyDataText="No open order lines for this delivery-date range."
                                OnRowDataBound="gvItemsRequiredByDay_RowDataBound">
                                <Columns>
                                    <asp:TemplateField HeaderText="Required Date" SortExpression="RequiredByDate">
                                        <ItemTemplate>
                                            <asp:Label ID="lblRequiredByDate" runat="server" Text='<%# Eval("RequiredByDate", "{0:yyyy-MM-dd}") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <b>Total</b>
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="By" SortExpression="Abbreviation">
                                        <ItemTemplate>
                                            <asp:Label ID="lblByAbbreviation" runat="server" Text='<%# Eval("Abbreviation") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="ItemDesc" HeaderText="Item Description" SortExpression="ItemDesc" />
                                    <asp:BoundField DataField="ItemPackagingDesc" HeaderText="Packaging" SortExpression="ItemPackagingDesc" />
                                    <asp:TemplateField HeaderText="Packs" SortExpression="PackQty"
                                        ItemStyle-HorizontalAlign="Right" FooterStyle-HorizontalAlign="Right">
                                        <ItemTemplate>
                                            <asp:Label ID="lblByPackQty" runat="server" Text='<%# Eval("PackQty", "{0:0.0}") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:Label ID="lblByFooterPackQty" runat="server" Font-Bold="true" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Total kg" SortExpression="Qty"
                                        ItemStyle-HorizontalAlign="Right" FooterStyle-HorizontalAlign="Right">
                                        <ItemTemplate>
                                            <asp:Label ID="lblByQty" runat="server" Text='<%# Eval("Qty", "{0:0.##}") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:Label ID="lblByFooterQty" runat="server" Font-Bold="true" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </ContentTemplate>
                    </ajaxToolkit:TabPanel>
                </ajaxToolkit:TabContainer>

                <div class="status-message" id="pnlStatus" runat="server" style="margin-top: 12px;">
                    <asp:Label ID="lblFilterStatus" runat="server" />
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
