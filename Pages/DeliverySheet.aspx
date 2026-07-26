<%@ Page Title="Delivery Sheet" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="DeliverySheet.aspx.cs" Inherits="TrackerSQL.Pages.DeliverySheet" %>

<asp:Content ID="cntDeliveryHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntDeliveryBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smDelivery" runat="server" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="uprgDelivery" runat="server"
        AssociatedUpdatePanelID="upnlDeliveryItems" DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info page-tone-progress">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="upnlDeliveryItems" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="tbCalendarDate" EventName="TextChanged" />
            <asp:AsyncPostBackTrigger ControlID="ddlActivePrepDates" EventName="SelectedIndexChanged" />
            <asp:AsyncPostBackTrigger ControlID="btnGo" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnFind" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="tbxFindClient" EventName="TextChanged" />
            <asp:AsyncPostBackTrigger ControlID="ddlDeliveryBy" EventName="SelectedIndexChanged" />
            <asp:PostBackTrigger ControlID="btnPrint" />
            <asp:PostBackTrigger ControlID="btnRefresh" />
            <asp:PostBackTrigger ControlID="btnBack" />
        </Triggers>
        <ContentTemplate>
            <asp:Panel ID="pnlDeliveryShell" runat="server" CssClass="simpleForm page-tone-panel page-tone-delivery">
                <asp:Panel ID="pnlDeliveryDate" runat="server">
                    <div class="page-tone-header tool-card-header">
                        <img class="tool-card-icon" src="../images/imgButtons/icons8-delivery-16.png" alt="" />
                        <div>
                            <h1 class="page-tone-title">Delivery Sheet</h1>
                            <p class="page-tone-subtitle">Manage delivery schedules</p>
                        </div>
                    </div>

                    <div class="page-tone-toolbar filter-toolbar">
                        <div class="filter-section search-controls">
                            <asp:Label runat="server" Text="Delivery Date:" AssociatedControlID="ddlActivePrepDates" CssClass="small" />
                            <asp:DropDownList
                                ID="ddlActivePrepDates"
                                runat="server"
                                DataTextField="RequiredByDate"
                                DataTextFormatString="{0:dd-MMM-yyyy (ddd)}"
                                DataValueField="RequiredByDate"
                                AppendDataBoundItems="True"
                                AutoPostBack="true"
                                OnDataBound="ddlActivePrepDates_DataBound"
                                OnSelectedIndexChanged="ddlActivePrepDates_SelectedIndexChanged">
                                <asp:ListItem Value="" Text="--- Select Date ---" />
                            </asp:DropDownList>
                            <asp:Button
                                ID="btnGo"
                                CssClass="filter-panel-btn"
                                runat="server"
                                Text="Go"
                                OnClick="btnGo_Click"
                                AccessKey="G"
                                ToolTip="get the results (AltShftG)" />
                            <span class="image-button" title="Pick a delivery date">
                                <asp:ImageButton ID="btnCalendar" runat="server" ImageUrl="~/images/imgButtons/CalendarBtn.png"
                                    AlternateText="Calendar" ToolTip="Pick a delivery date" />
                                <asp:TextBox ID="tbCalendarDate" runat="server"
                                    Style="width: 0; height: 0; border: none; padding: 0; margin: 0; opacity: 0; position: absolute; left: 0; top: 100%;"
                                    AutoPostBack="true" OnTextChanged="tbCalendarDate_TextChanged" />
                                <ajaxToolkit:CalendarExtender
                                    ID="calExtender"
                                    runat="server"
                                    TargetControlID="tbCalendarDate"
                                    PopupButtonID="btnCalendar"
                                    PopupPosition="BottomLeft"
                                    Format="yyyy-MM-dd" />
                            </span>
                            <asp:Button ID="btnRefresh" CssClass="filter-panel-btn" Text="Refresh" AccessKey="R"
                                ToolTip="refresh lists (AltShftR)" runat="server" OnClick="btnRefresh_Click" />
                            <asp:Label ID="lblDeliveryBy" runat="server" Text="By:" Visible="false" />
                            <asp:DropDownList ID="ddlDeliveryBy" runat="server" AutoPostBack="true" Visible="false"
                                OnSelectedIndexChanged="ddlDeliveryBy_SelectedIndexChanged" />
                        </div>
                        <div class="filter-section admin-controls">
                            <asp:Label runat="server" Text="To:" AssociatedControlID="tbxFindClient" CssClass="small" />
                            <asp:TextBox ID="tbxFindClient" runat="server" OnTextChanged="tbxFindClient_OnTextChanged" AutoPostBack="true" />
                            <asp:Button ID="btnFind" Text="Find" runat="server" CssClass="filter-panel-btn" OnClick="btnFind_Click" />
                            <asp:Button ID="btnPrint" runat="server" CssClass="filter-panel-btn hideWhenPrinting" Text="Print"
                                OnClick="btnPrint_Click" AccessKey="P" ToolTip="print sheet (AltShftP)" />
                            <span class="image-button" title="New order">
                                <img src="../images/imgButtons/GreenPlus.gif" alt="" />
                                <asp:HyperLink ID="hlAddDeliveryItem" runat="server"
                                    Text="New Order"
                                    ToolTip="New item(s) to deliver"
                                    NavigateUrl="~/Pages/OrderDetail.aspx?NewOrder=true" />
                            </span>
                            <asp:Button ID="btnBack" runat="server" Text="Back" CssClass="filter-panel-btn"
                                OnClick="btnBack_Click" CausesValidation="false" ToolTip="Return to home" />
                        </div>
                    </div>
                </asp:Panel>

                <asp:Table ID="tblDeliveries" runat="server" CssClass="TblZebra" Width="100%" CellPadding="0">
                    <asp:TableHeaderRow TableSection="TableHeader">
                        <asp:TableHeaderCell>By</asp:TableHeaderCell>
                        <asp:TableHeaderCell>To</asp:TableHeaderCell>
                        <asp:TableHeaderCell ID="thcReceivedBy" Width="90px">Received By</asp:TableHeaderCell>
                        <asp:TableHeaderCell ID="thcSignature" Width="100px">Signature</asp:TableHeaderCell>
                        <asp:TableHeaderCell>Items</asp:TableHeaderCell>
                        <asp:TableHeaderCell ID="thcInStock">In Stock</asp:TableHeaderCell>
                    </asp:TableHeaderRow>
                </asp:Table>
                <br />
                <div class="TblWrapper">
                    <asp:Table ID="tblTotals" runat="server" CssClass="TblCoffee" Width="100%">
                    </asp:Table>
                </div>
                <div style="text-align: right; width: 98%" class="small">
                    <asp:Label ID="ltrlWhichDate" Text="" runat="server" CssClass="small" />
                </div>

                <div class="status-message" id="pnlStatus" runat="server" style="margin-top: 12px;" visible="false">
                    <asp:Literal ID="ltrlStatus" runat="server" />
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
