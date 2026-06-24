<%@ page title="" language="C#" masterpagefile="~/Site.Master" autoeventwireup="true" codebehind="DeliverySheet.aspx.cs" inherits="TrackerSQL.Pages.DeliverySheet" %>

<asp:Content ID="cntDeliveryHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntDeliveryBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smDelivery" runat="server" />
    <asp:Panel ID="pnlDeliveryDate" runat="server">
        <h1>Delivery Sheet</h1>
        <asp:UpdateProgress runat="server" ID="uprgDeliveryFilterBy">
            <progresstemplate>
                <img src="../images/animi/BlueArrowsUpdate.gif" alt="updating" width="16" height="16" />
                updating.....
            </progresstemplate>
        </asp:UpdateProgress>
        <asp:UpdatePanel ID="upnlDeliveryFilterBy" runat="server" ChildrenAsTriggers="true">
            <triggers>
                <asp:AsyncPostBackTrigger ControlID="tbCalendarDate" EventName="TextChanged" />
            </triggers>
            <contenttemplate>
                <div class="filter-toolbar">
                    <div class="filter-section search-controls">
                        <label style="margin-bottom: 0;" for="ddlActivePrepDates">Delivery Date:</label>
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
                        <span style="position: relative; display: inline-block;">
                            <asp:ImageButton ID="btnCalendar" runat="server"  ImageUrl="~/images/imgButtons/CalendarBtn.png"  ToolTip="Pick a delivery date" />
                            <asp:TextBox ID="tbCalendarDate" runat="server" CssClass="filter-panel-btn"
                                Style="width: 0; height: 0; border: none; padding: 0; margin: 0; opacity: 0; position: absolute; left: 0; top: 100%;"
                                AutoPostBack="true" OnTextChanged="tbCalendarDate_TextChanged" />
                            <ajaxtoolkit:calendarextender
                                id="calExtender"
                                runat="server"
                                targetcontrolid="tbCalendarDate"
                                popupbuttonid="btnCalendar"
                                popupposition="BottomLeft"
                                format="yyyy-MM-dd" />
                        </span>
                        <asp:Button ID="btnRefresh" CssClass="filter-panel-btn" Text="Refresh" AccessKey="R" ToolTip="refresh lists (AltShftR)" runat="server" OnClick="btnRefresh_Click" />
                        <asp:Label ID="lblDeliveryBy" runat="server" Text="By:" Visible="false" />
                        <asp:DropDownList ID="ddlDeliveryBy" runat="server" AutoPostBack="true" Visible="false"
                            OnSelectedIndexChanged="ddlDeliveryBy_SelectedIndexChanged" />
                    </div>
                    <div class="filter-section admin-controls">
                        <label for="tbxFindClient">To:</label>
                        <asp:TextBox ID="tbxFindClient" runat="server" OnTextChanged="tbxFindClient_OnTextChanged" AutoPostBack="true" />
                        <asp:Button ID="btnFind" Text="Find" runat="server" OnClick="btnFind_Click" />
                        <asp:Button ID="btnPrint" runat="server" CssClass="hideWhenPrinting" Text="Print" OnClick="btnPrint_Click" AccessKey="P" ToolTip="print sheet (AltShftP)" />
                        <asp:HyperLink ID="hlAddDeliveryItem" ImageUrl="~/images/imgButtons/AddItem.gif" ToolTip="New item(s) to deliver"
                            NavigateUrl="~/Pages/OrderDetail.aspx?NewOrder=true" runat="server" />
                    </div>
                </div>
            </contenttemplate>
        </asp:UpdatePanel>
        <%--<asp:ObjectDataSource ID="odsActivePrepDates" runat="server"
            OldValuesParameterFormatString="original_{0}"
            SelectMethod="GetActiveDeliveryDates"
            TypeName="TrackerSQL.Controls.ActiveDeliveryData"></asp:ObjectDataSource>--%>
        <br />
        <asp:Label ID="lblStatus" runat="server" Visible="false" CssClass="small" />
    </asp:Panel>
    <asp:UpdatePanel ID="upnlDeliveryItems" runat="server" UpdateMode="Conditional">
        <contenttemplate>
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
        </contenttemplate>
        <triggers>
            <asp:AsyncPostBackTrigger ControlID="btnFind" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="tbxFindClient" EventName="TextChanged" />
            <asp:AsyncPostBackTrigger ControlID="btnGo" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="ddlDeliveryBy"
                EventName="SelectedIndexChanged" />
        </triggers>
    </asp:UpdatePanel>

</asp:Content>
