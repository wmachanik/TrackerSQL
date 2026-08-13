<%@ Page Title="Weekly Summary" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="PreperationSummary.aspx.cs" Inherits="TrackerSQL.Pages.PreperationSummary" %>

<asp:Content ID="cntPreSummaryHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntPreSummaryBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager runat="server" ID="scrmOrderDetail" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="udtpPrepSummary" runat="server"
        AssociatedUpdatePanelID="udtpnlPrepSummary" DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info page-tone-progress">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel runat="server" ID="udtpnlPrepSummary" ChildrenAsTriggers="true" UpdateMode="Conditional">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="GoBtn" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="ResetBtn" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="PrevWeekBtn" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="NextWeekBtn" EventName="Click" />
            <asp:PostBackTrigger ControlID="btnBack" />
        </Triggers>
        <ContentTemplate>
            <asp:Panel ID="pnlPrepSummary" runat="server" CssClass="simpleForm page-tone-panel page-tone-summary">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/icons8-weekly-summary-16.png" alt="" />
                    <div>
                        <h1 class="page-tone-title">Weekly Summary</h1>
                        <p class="page-tone-subtitle">View weekly preparation summary</p>
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
                            <td class="TblLHCol-first">Select By:</td>
                            <td style="text-align: left;">
                                <asp:DropDownList ID="ddlFilterByPrepDate" runat="server"
                                    ToolTip="Prep Date or Delivery/Required By Date">
                                    <asp:ListItem Selected="True" Value="PrepDate" Text="Prep Date" />
                                    <asp:ListItem Value="RequiredByDate" Text="Delivery/Required By Date" />
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2" style="text-align: center; padding-top: 12px;">
                                <div class="button-row">
                                    <asp:Button ID="GoBtn" Text="Go" runat="server" CssClass="filter-panel-btn"
                                        OnClick="GoBtn_Click" ToolTip="Load summary for the selected dates" />
                                    <asp:Button ID="ResetBtn" Text="Reset Dates" runat="server" CssClass="filter-panel-btn"
                                        OnClick="ResetBtn_Click" ToolTip="Reset to the current week" />
                                    <asp:Button ID="PrevWeekBtn" Text="Prev Week" runat="server" CssClass="filter-panel-btn"
                                        OnClick="BackBtn_Click" ToolTip="Move date range back one week" />
                                    <asp:Button ID="NextWeekBtn" Text="Next Week" runat="server" CssClass="filter-panel-btn"
                                        OnClick="ForwardBtn_Click" ToolTip="Move date range forward one week" />
                                    <span class="image-button" title="Return to home">
                                        <asp:ImageButton ID="btnBack" runat="server"
                                            ImageUrl="~/images/imgButtons/Back.gif"
                                            AlternateText="Back"
                                            ToolTip="Return to home"
                                            OnClick="btnBack_Click"
                                            CausesValidation="false" />
                                    </span>
                                </div>
                            </td>
                        </tr>
                    </table>
                </div>

                <div class="results-container" style="margin-top: 8px;">
                    <asp:GridView ID="gvPreperationSummary" runat="server" AutoGenerateColumns="False"
                        CssClass="results-table table-auto-width" OnRowDataBound="gvPreperationSummary_RowDataBound"
                        ShowFooter="True">
                        <Columns>
                            <asp:TemplateField HeaderText="Item Description" SortExpression="ItemDesc">
                                <ItemTemplate>
                                    <asp:Label ID="lblItemDesc" runat="server" Text='<%# Eval("ItemDesc") %>' />
                                </ItemTemplate>
                                <FooterTemplate>
                                    <asp:Label ID="lblTotalDesc" Text="Total" runat="server" Font-Bold="true" />
                                </FooterTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Quantity" SortExpression="Quantity"
                                ItemStyle-HorizontalAlign="Right" FooterStyle-HorizontalAlign="Right">
                                <ItemTemplate>
                                    <asp:Label ID="lblQty" runat="server" Text='<%# Eval("Quantity") %>' />
                                </ItemTemplate>
                                <FooterTemplate>
                                    <asp:Label ID="lblTotalQty" Text="" runat="server" />
                                </FooterTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    <asp:Label ID="lblDescHdr" Text="" runat="server" />
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblDescItem" Text="" runat="server" />
                                </ItemTemplate>
                                <FooterTemplate>
                                    <asp:Label ID="lblDescFotter" Text="" runat="server" />
                                </FooterTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="status-message status-info" style="padding: 16px;">
                                Select a date range with a valid preparation date, then click Go.
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                    <asp:Literal ID="ltrlDates" Text="" runat="server" Mode="PassThrough" />
                </div>

                <div class="status-message" id="pnlStatus" runat="server" style="margin-top: 12px;" visible="false">
                    <asp:Literal ID="ltrlStatus" runat="server" />
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
