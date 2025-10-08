<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CoffeeRequired.aspx.cs" Inherits="TrackerDotNet.Pages.CoffeeRequired" %>
<asp:Content ID="cntCoffeeRequiredHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntCoffeeRequiredBdy" ContentPlaceHolderID="MainContent" runat="server">
  <asp:ScriptManager ID="smgrRequired" runat="server">
  </asp:ScriptManager>
  <asp:UpdateProgress ID="uppgRequired" runat="server">
    <ProgressTemplate>Please Wait</ProgressTemplate>
  </asp:UpdateProgress>
  <asp:UpdatePanel ID="upnlRequired" runat="server">
    <ContentTemplate>
      <ajaxToolkit:TabContainer ID="tabRequired" runat="server" ActiveTabIndex="1">
        <ajaxToolkit:TabPanel ID="pnlByPrepDate" runat="server"  HeaderText="By Prep Date" TabIndex="0">
          <HeaderTemplate>Require Summary by Prepdate</HeaderTemplate> 
          <ContentTemplate>
            <h2>Required Summary by Roasting Day</h2>
            <p>Coffee required using only Roasting Day</p>
            <asp:GridView ID="gvPreperationDay" runat="server" AutoGenerateColumns="False"  ShowFooter="true"
              DataSourceID="sdsRequiredByRoastingDay" CssClass="TblZebra" OnRowDataBound="gvPreperationDay_RowDataBound">
              <Columns>
                <asp:TemplateField HeaderText=" Item Description " SortExpression="ItemDesc">
                  <ItemTemplate>
                    <asp:Label ID="lblGroupTitle" runat="server" Text='<%#Eval("RoastDate", "{0:d}") %>'  />
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
            <asp:SqlDataSource ID="sdsRequiredByRoastingDay" runat="server" 
              ConnectionString="<%$ ConnectionStrings:Tracker08ConnectionString %>" 
              ProviderName="<%$ ConnectionStrings:Tracker08ConnectionString.ProviderName %>" 
              SelectCommand="SELECT OrdersTbl.RoastDate, ItemTypeTbl.ItemDesc, ROUND(SUM(OrdersTbl.QuantityOrdered),2) AS QTY FROM ((OrdersTbl INNER JOIN ItemTypeTbl ON OrdersTbl.ItemTypeID = ItemTypeTbl.ItemTypeID) LEFT OUTER JOIN PersonsTbl ON OrdersTbl.ToBeDeliveredBy = PersonsTbl.PersonID) WHERE (OrdersTbl.Done = false) AND (ItemTypeTbl.ServiceTypeID = 2) GROUP BY OrdersTbl.RoastDate, ItemTypeTbl.ItemDesc">
            </asp:SqlDataSource>
          </ContentTemplate>
        </ajaxToolkit:TabPanel> 
        <ajaxToolkit:TabPanel ID="tabpnlRequiredDetail" runat="server"  HeaderText="Required By Delivery Date" TabIndex="1" >
          <ContentTemplate>
            <h2>Required Detail</h2>
            <p>Below is a list of all the items required on each delviery day, sorted by delivery person</p>
            <asp:GridView ID="gvCoffeeRequireByDay" runat="server" AutoGenerateColumns="False" OnRowDataBound="gvCoffeeRequireByDay_RowDataBound"
              DataSourceID="sdsCoffeeRequiredCalc" CssClass="TblZebra" ShowFooter="true"  > 
              <Columns>
                <asp:TemplateField HeaderText=" By " SortExpression="Abreviation">
                  <FooterTemplate>
                    <b>Total</b>
                  </FooterTemplate>
                  <ItemTemplate>
                    <asp:Label ID="lblByGroupTitle" runat="server" Text='<%#Eval("RequiredByDate", "{0:d}") %>'  />
                    <asp:Label ID="lblByAbreviation" runat="server" Text='<%#Eval("Abreviation") %>' />
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
            <asp:SqlDataSource ID="sdsCoffeeRequiredCalc" runat="server" 
              ConnectionString="<%$ ConnectionStrings:Tracker08ConnectionString %>" 
              ProviderName="<%$ ConnectionStrings:Tracker08ConnectionString.ProviderName %>" 
              SelectCommand="SELECT OrdersTbl.RequiredByDate, PersonsTbl.Abreviation, ItemTypeTbl.ItemDesc, Round(SUM(OrdersTbl.QuantityOrdered),2) AS QTY FROM ((OrdersTbl INNER JOIN ItemTypeTbl ON OrdersTbl.ItemTypeID = ItemTypeTbl.ItemTypeID) LEFT OUTER JOIN PersonsTbl ON OrdersTbl.ToBeDeliveredBy = PersonsTbl.PersonID) WHERE (OrdersTbl.Done = false) GROUP BY OrdersTbl.RequiredByDate, PersonsTbl.Abreviation, ItemTypeTbl.ItemDesc">
            </asp:SqlDataSource>
          </ContentTemplate>
        </ajaxToolkit:TabPanel>
      </ajaxToolkit:TabContainer>
    </ContentTemplate>
  </asp:UpdatePanel>
</asp:Content>
