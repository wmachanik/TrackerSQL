<%@ Page Title="Items Required" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ItemsRequired.aspx.cs" Inherits="TrackerSQL.Pages.ItemsRequired" %>
<asp:Content ID="cntItemsRequiredHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntItemsRequiredBdy" ContentPlaceHolderID="MainContent" runat="server">
  <asp:ScriptManager ID="smgrRequired" runat="server" />
  <asp:UpdateProgress ID="uppgRequired" runat="server" AssociatedUpdatePanelID="upnlRequired" DisplayAfter="0">
    <ProgressTemplate>
      <div class="status-message status-info" style="margin: 8px 0;">
        <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
        &nbsp;Please wait...
      </div>
    </ProgressTemplate>
  </asp:UpdateProgress>
  <asp:UpdatePanel ID="upnlRequired" runat="server">
    <ContentTemplate>
      <div style="padding: 15px; background-color: #f5f5f5; border-radius: 4px; margin-bottom: 15px;">
        <h3 style="margin-top: 0;">Filter by Date Range</h3>
        <div style="display: flex; gap: 15px; flex-wrap: wrap; align-items: flex-end;">
          <div>
            <label for="txtFromDate">From Date:</label>
            <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date"></asp:TextBox>
          </div>
          <div>
            <label for="txtToDate">To Date:</label>
            <asp:TextBox ID="txtToDate" runat="server" TextMode="Date"></asp:TextBox>
          </div>
          <div>
            <asp:Button ID="btnFilter" runat="server" Text="Apply Filter" CssClass="button" OnClick="btnFilter_Click" />
            <asp:Button ID="btnClearFilter" runat="server" Text="Clear Filter" CssClass="button" OnClick="btnClearFilter_Click" />
          </div>
        </div>
        <div style="margin-top: 10px; color: #666;">
          <asp:Label ID="lblFilterStatus" runat="server" Text="Showing all items (no filter applied)"></asp:Label>
        </div>
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
              CssClass="TblZebra results-table" EmptyDataText="No open order lines for this prep-date range."
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
              CssClass="TblZebra results-table" EmptyDataText="No open order lines for this delivery-date range."
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
    </ContentTemplate>
    <Triggers>
      <asp:AsyncPostBackTrigger ControlID="btnFilter" EventName="Click" />
      <asp:AsyncPostBackTrigger ControlID="btnClearFilter" EventName="Click" />
    </Triggers>
  </asp:UpdatePanel>
</asp:Content>
