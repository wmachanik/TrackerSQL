<%@ Page Title="Items Required" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ItemsRequired.aspx.cs" Inherits="TrackerSQL.Pages.ItemsRequired" %>
<asp:Content ID="cntItemsRequiredHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntItemsRequiredBdy" ContentPlaceHolderID="MainContent" runat="server">
  <asp:ScriptManager ID="smgrRequired" runat="server">
  </asp:ScriptManager>
  <asp:UpdateProgress ID="uppgRequired" runat="server">
    <ProgressTemplate><img src="../images/animi/BlueArrowsUpdate.gif" alt="updating" width="16" height="16" />Please Wait</ProgressTemplate>
  </asp:UpdateProgress>
  <asp:UpdatePanel ID="upnlRequired" runat="server">
    <ContentTemplate>
      <!-- Date Range Filter Section -->
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
        <ajaxToolkit:TabPanel ID="pnlByPrepDate" runat="server"  HeaderText="By Prep Date" TabIndex="0">
          <HeaderTemplate>Items Summary by Prep Date</HeaderTemplate> 
          <ContentTemplate>
            <h2>Items Required Summary by Preperation Day</h2>
            <p>Items required using only Preperation Day</p>
            <asp:GridView ID="gvPreparationDay" runat="server" AutoGenerateColumns="False"  ShowFooter="true"
              CssClass="TblZebra" OnRowDataBound="gvPreparationDay_RowDataBound">
              <Columns>
                <asp:TemplateField HeaderText=" Item Description " SortExpression="ItemDesc">
                  <ItemTemplate>
                    <asp:Label ID="lblGroupTitle" runat="server" Text='<%#Eval("PrepDate", "{0:d}") %>'  />
                    <asp:Label ID="lblItemDesc" runat="server" Text='<%#Eval("ItemDesc") %>' />
                  </ItemTemplate>
                  <FooterTemplate>
                    <b>Total</b>
                  </FooterTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText=" Quantity " SortExpression="Qty" ItemStyle-HorizontalAlign="Right">
                  <ItemTemplate>
                    <asp:Label ID="lblQty" runat="server"   Text='<%#Eval("Qty") %>' />
                  </ItemTemplate>
                  <FooterTemplate >
                    <asp:Label ID="lblFooterQty" Text="0" runat="server" />
                  </FooterTemplate>
                  <FooterStyle HorizontalAlign="Right" />
                </asp:TemplateField>
              </Columns>
            </asp:GridView>
          </ContentTemplate>
        </ajaxToolkit:TabPanel> 
        <ajaxToolkit:TabPanel ID="tabpnlRequiredDetail" runat="server"  HeaderText="Required By Delivery Date" TabIndex="1" >
          <ContentTemplate>
            <h2>Items Required Detail</h2>
            <p>Below is a list of all the items required on each delivery day, sorted by delivery person</p>
            <asp:GridView ID="gvItemsRequiredByDay" runat="server" AutoGenerateColumns="False" OnRowDataBound="gvItemsRequiredByDay_RowDataBound"
              CssClass="TblZebra" ShowFooter="true"  > 
              <Columns>
                <asp:TemplateField HeaderText=" By " SortExpression="Abbreviation">
                  <FooterTemplate>
                    <b>Total</b>
                  </FooterTemplate>
                  <ItemTemplate>
                    <asp:Label ID="lblByGroupTitle" runat="server" Text='<%#Eval("RequiredByDate", "{0:d}") %>'  />
                    <asp:Label ID="lblByAbbreviation" runat="server" Text='<%#Eval("Abbreviation") %>' />
                  </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText=" Item Description " SortExpression="ItemDesc">
                  <FooterTemplate>
                    &nbsp;
                  </FooterTemplate>
                  <ItemTemplate>
                    <asp:Label ID="lblByItemDesc" runat="server" Text='<%#Eval("ItemDesc") %>' />
                  </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText=" Quantity " SortExpression="Qty">
                  <FooterTemplate >
                    <asp:Label ID="lblByFooterQty" Text="0" runat="server" />
                  </FooterTemplate>
                  <ItemTemplate>
                    <asp:Label ID="lblByQty" runat="server"   Text='<%#Eval("Qty") %>' />
                  </ItemTemplate>
                  <ItemStyle HorizontalAlign="Right" />
                  <FooterStyle HorizontalAlign="Right" />
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
