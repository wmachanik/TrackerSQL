<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PreperationSummary.aspx.cs" Inherits="TrackerDotNet.Pages.PreperationSummary" %>

<asp:Content ID="cntPreSummaryHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntPreSummaryBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Preperation Summary</h1>
    <asp:ScriptManager runat="server" ID="scrmOrderDetail" />
    <asp:UpdateProgress ID="udtpPrepSummary" runat="server">
        <ProgressTemplate>
            &nbsp;&nbsp;
      <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel runat="server" ID="udtpnlPrepSummary" ChildrenAsTriggers="true" UpdateMode="Always">
        <ContentTemplate>
            <div class="filter-toolbar">
                <div class="filter-block">
                    <table class="TblLHCol-brown">
                        <tr>
                            <td class="TblLHCol-first">Date From:</td>
                            <td>
                                <asp:TextBox ID="tbxDateFrom" runat="server" Text='<%# TrackerDotNet.Classes.TimeZoneUtils.Now() %>' />
                                <ajaxToolkit:CalendarExtender ID="tbxDateFrom_CalendarExtender" runat="server"
                                    Enabled="True" TargetControlID="tbxDateFrom"></ajaxToolkit:CalendarExtender>
                            </td>
                        </tr>
                        <tr>
                            <td class="TblLHCol-first">Date To:</td>
                            <td>
                                <asp:TextBox ID="tbxDateTo" runat="server" Text='<%# Bind("RoastDate", "{0:yyyy-MM-dd}" ) %>' />
                                <ajaxToolkit:CalendarExtender ID="tbxDateTo_CalendarExtender" runat="server"
                                    Enabled="True" TargetControlID="tbxDateTo"></ajaxToolkit:CalendarExtender>
                            </td>
                        </tr>
                        <tr>
                            <td class="TblLHCol-first">Select By:</td>
                            <td style="text-align: left">
                                <asp:DropDownList ID="ddlFilterByRoastDate" runat="server" ToolTip="Check for Roast Date - uncheck for Delivery Date">
                                    <asp:ListItem Selected="True" Value="RoastDate" Text="Roast Date" />
                                    <asp:ListItem Value="ReqDate" Text="Delivery/Dispatch Date" />
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2" style="text-align: center">&nbsp;
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2" style="text-align: center">
                                <asp:Button ID="GoBtn" Text="Go" runat="server" OnClick="GoBtn_Click" />&nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:Button ID="ResetBtn" Text="Reset Dates" runat="server" OnClick="ResetBtn_Click" />&nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:Button ID="BackBtn" Text="Back" runat="server" OnClick="BackBtn_Click" />&nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:Button ID="ForwardBtn" Text="Forward" runat="server" OnClick="ForwardBtn_Click" />
                            </td>
                        </tr>
                    </table>
                </div>
            </div>
            <br />
            <div class="results-container">
                <asp:GridView ID="gvPreperationSummary" runat="server" AutoGenerateColumns="False"
                    CssClass="results-table" OnRowDataBound="gvPreperationSummary_RowDataBound"
                    EmptyDataText="Please Select a Date range with a valid prep date"
                    ShowFooter="True">
                    <Columns>
                        <asp:TemplateField HeaderText=" Item Description " SortExpression="ItemDesc">
                            <ItemTemplate>
                                <asp:Label ID="lblItemDesc" runat="server" Text='<%#Eval("ItemDesc") %>' />
                            </ItemTemplate>
                            <FooterTemplate>
                                <asp:Label ID="lblTotalDesc" Text="Total" runat="server" Font-Bold="true" />
                            </FooterTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText=" Quantity " SortExpression="Quantity" ItemStyle-HorizontalAlign="Right" FooterStyle-HorizontalAlign="Right">
                            <ItemTemplate>
                                <asp:Label ID="lblQty" runat="server" Text='<%#Eval("Quantity") %>' />
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
                </asp:GridView>
                <br />
                <asp:Literal ID="ltrlDates" Text="" runat="server" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>
