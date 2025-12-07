<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="OrderDone.aspx.cs"
    Inherits="TrackerSQL.Pages.OrderDone" %>

<asp:Content ID="cntOrderDoneHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntOrderDoneBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smOrderDone" runat="server" />
    <asp:UpdateProgress ID="updtprgOrderDone" runat="server" AssociatedUpdatePanelID="updtpnlOrderDone">
        <ProgressTemplate>
            <img src="../images/animi/QuaffeeProgress.gif" alt="progress" />
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="updtpnlOrderDone" runat="server" ChildrenAsTriggers="true">
        <ContentTemplate>
            <asp:Panel ID="pnlOrderDetails" runat="server">
                <h2>Order Delivered</h2>
                <div class="responsive-layout-container">
                    <div class="results-table">
                        <asp:FormView ID="fvOrderDone" runat="server" DataSourceID="sdsOrderDoneHeader"
                            BackColor="#DEBA84" BorderColor="#DEBA84" BorderStyle="None" BorderWidth="1px"
                            CellPadding="4" CellSpacing="2" GridLines="Both" CssClass="TblFlex">
                            <RowStyle BackColor="#FFF7E7" ForeColor="#292909" />
                            <HeaderStyle BackColor="#A55129" Font-Bold="True" ForeColor="White" />
                            <ItemTemplate>
                                <table>
                                    <tr>
                                        <td class="TblLHCol-first">CompanyName</td>
                                        <td>
                                            <asp:Label ID="CompanyNameLabel" runat="server" Text='<%# Eval("CompanyName") %>' />
                                            &nbsp;(<asp:Label ID="CustomerIDLabel" runat="server" Text='<%# Eval("CustomerID") %>' />)
                </td>
                                        <td class="TblLHCol-first">DeliveryDate:</td>
                                        <td>
                                            <asp:TextBox ID="ByDateTextBox" runat="server" Text='<%# Eval("RequiredByDate", "{0:d}") %>' />
                                        </td>
                                    </tr>
                                </table>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <table>
                                    <tbody>
                                        <tr>
                                            <td class="TblLHCol-first">CompanyName</td>
                                            <td>
                                                <asp:Label ID="CompanyNameLabel" runat="server" Text='<%# Eval("CompanyName") %>' />&nbsp;
                                              (<asp:Label ID="CustomerIDLabel" runat="server" Text='<%# Eval("CustomerID") %>' />)
                                            </td>
                                            <td class="TblLHCol-first">DeliveryDate:</td>
                                            <td>
                                                <asp:TextBox ID="ByDateTextBox" runat="server" Text='<%# Bind("RequiredByDate", "{0:d}") %>' /></td>
                                        </tr>
                                    </tbody>
                                </table>
                            </EditItemTemplate>
                            <PagerStyle ForeColor="#8C4510" HorizontalAlign="Center" />
                            <RowStyle BackColor="#FFF7E7" ForeColor="#8C4510" />
                        </asp:FormView>
                        <div style="padding-top: 12px" class="layout-detail-panel">
                            <asp:GridView ID="gvOrderDoeLines" runat="server" AllowSorting="True" AutoGenerateColumns="False"
                                DataKeyNames="TOLineID" CssClass="TblWhite" DataSourceID="sdsOrderDoneLines">
                                <Columns>
                                    <asp:BoundField DataField="TOLineID" HeaderText="ID" Visible="false" InsertVisible="False"
                                        ReadOnly="True" SortExpression="TOLineID" />
                                    <asp:TemplateField HeaderText="Item" SortExpression="ItemID">
                                        <EditItemTemplate>
                                            <asp:DropDownList ID="ddlItemDesc" runat="server" DataSourceID="sdsItemTypes" DataTextField="ItemDesc"
                                                DataValueField="ItemTypeID" AppendDataBoundItems="True" SelectedValue='<%# Bind("ItemID") %>'>
                                                <asp:ListItem Value="0">n/a</asp:ListItem>
                                            </asp:DropDownList>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlItemDesc" runat="server" DataSourceID="sdsItemTypes" DataTextField="ItemDesc" Enabled="false"
                                                DataValueField="ItemTypeID" AppendDataBoundItems="True" SelectedValue='<%# Eval("ItemID") == null ? "0" : Eval("ItemID").ToString() %>'>
                                                <asp:ListItem Value="0">n/a</asp:ListItem>
                                            </asp:DropDownList>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="Qty" HeaderText="Quantity" SortExpression="Qty" ItemStyle-HorizontalAlign="Center">
                                        <ItemStyle HorizontalAlign="Right" VerticalAlign="Middle" Width="4em" />
                                    </asp:BoundField>
                                    <asp:TemplateField HeaderText="Packaging" SortExpression="PackagingID">
                                        <EditItemTemplate>
                                            <asp:DropDownList ID="ddlPackaging" runat="server" AppendDataBoundItems="true" DataSourceID="sdsPackagingTypes"
                                                DataTextField="Description" DataValueField="PackagingID" SelectedValue='<%# Bind("PackagingID")  %>'>
                                                <asp:ListItem Value="0">n/a</asp:ListItem>
                                            </asp:DropDownList>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlPackaging" runat="server" AppendDataBoundItems="true" DataSourceID="sdsPackagingTypes" Enabled="false"
                                                DataTextField="Description" DataValueField="PackagingID" SelectedValue='<%#  Eval("PackagingID") == null ? "0" : Eval("PackagingID").ToString()  %>'>
                                                <asp:ListItem Value="0">n/a</asp:ListItem>
                                            </asp:DropDownList>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:CommandField ButtonType="Button" ShowDeleteButton="True" ShowEditButton="True" HeaderText="Action" />
                                </Columns>
                            </asp:GridView>
                        </div>
                        <br />
                    </div>
                </div>
                <div class="responsive-layout-container">
                    <div class="layout-panel-top">
                        <table class="TblFlex">
                            <tbody>
                                <tr>
                                    <td>Stock:</td>
                                    <td>
                                        <asp:TextBox ID="tbxStock" runat="server" Width="5em" /></td>
                                    <td rowspan="2" valign="middle">
                                        <asp:RadioButtonList ID="rbtnSendConfirm" runat="server" CssClass="TblWhite">
                                            <asp:ListItem Text="none" Value="none" />
                                            <asp:ListItem Text="send 'its in the post box' message" Value="postbox" />
                                            <asp:ListItem Text="send 'order dispatch' message" Value="dispatched" />
                                            <asp:ListItem Text="send 'order collected' message" Value="collected" />
                                            <asp:ListItem Text="send 'order delivered' message" Value="done" Selected="true" />
                                        </asp:RadioButtonList>
                                </tr>
                                <tr>
                                    <td>Cup Count:</td>
                                    <td>
                                        <asp:TextBox ID="tbxCount" runat="server" Width="5em" /></td>
                                </tr>
                            </tbody>
                        </table>
                        <br />
                        <div class="status-message">
                            <asp:Literal ID="ltrlStatus" Text="" runat="server" />
                        </div>
                    </div>
                    <div class="layout-footer-panel button-toolbar-rounded ">
                        <asp:Button ID="btnDone" Text="Done" runat="server" AccessKey="D" OnClick="btnDone_Click" /><br
                            style="padding-top: 2px" />
                        <asp:Button ID="btnCancel" Text="Cancel" runat="server" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlCustomerDetailsUpdated" runat="server" Visible="false">
                <div class="responsive-layout-container">
                    <div>
                        <h2>Customer Updated:</h2>
                        <br />
                        <asp:Label ID="tbxCustomerName" Text="" runat="server" /></td>
                    </div>
                    <br />
                    <div>
                        <asp:DataGrid ID="dgCustomerUsage" runat="server" CssClass="TblFlex small">
                            <Columns>
                                <asp:BoundColumn DataField="CustomerID" Visible="false" />
                                <asp:BoundColumn DataField="LastCupCount" HeaderText="Last Count" ItemStyle-HorizontalAlign="Right" />
                                <asp:BoundColumn DataField="NextCoffeeBy" HeaderText="NextCoffeeBy" DataFormatString="{0:d}" />
                                <asp:BoundColumn DataField="DailyConsumption" HeaderText="DailyConsumption" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:0.##}" />
                                <asp:BoundColumn DataField="NextCleanOn" HeaderText="NextCleanEst" DataFormatString="{0:d}" />
                                <asp:BoundColumn DataField="CleanAveCount" HeaderText="CleanAveCount" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:0.##}" />
                                <asp:BoundColumn DataField="NextFilterEst" HeaderText="NextFilterEst" DataFormatString="{0:d}" />
                                <asp:BoundColumn DataField="FilterAveCount" HeaderText="FilterAveCount" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:0.##}" />
                                <asp:BoundColumn DataField="NextDescaleEst" HeaderText="NextDescaleEst" DataFormatString="{0:d}" />
                                <asp:BoundColumn DataField="DescaleAveCount" HeaderText="DescaleAveCount" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:0.##}" />
                                <asp:BoundColumn DataField="NextServiceEst" HeaderText="NextServiceEst" DataFormatString="{0:d}" />
                                <asp:BoundColumn DataField="ServiceAveCount" HeaderText="ServiceAveCount" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:0.##}" />
                            </Columns>
                        </asp:DataGrid>
                    </div>
                    <br />
                    <div class="button-container-auto">
                        <asp:Button ID="btnReturnToDeliveres" Text="Return to Delivery Sheet" AccessKey="D"
                            runat="server" OnClick="btnReturnToDeliveres_Click" />
                    </div>
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:SqlDataSource ID="sdsOrderDoneHeader" runat="server"
        ConnectionString="<%$ ConnectionStrings:Tracker08ConnectionString %>"
        ProviderName="<%$ ConnectionStrings:Tracker08ConnectionString.ProviderName %>"
        SelectCommand="SELECT c.CompanyName, h.CustomerID, h.RequiredByDate
                   FROM TempOrdersHeaderTbl h
                   INNER JOIN CustomersTbl c ON h.CustomerID = c.CustomerID
                   WHERE h.CustomerID = ?">
        <SelectParameters>
            <asp:Parameter Name="CustomerID" Type="Int32" DefaultValue="0" />
        </SelectParameters>
    </asp:SqlDataSource>
    <asp:SqlDataSource ID="sdsOrderDoneLines" runat="server"
        CancelSelectOnNullParameter="True"
        ConnectionString="<%$ ConnectionStrings:Tracker08ConnectionString %>"
        ProviderName="<%$ ConnectionStrings:Tracker08ConnectionString.ProviderName %>"
        SelectCommand="SELECT [ItemID], [Qty], [PackagingID], [TOLineID] FROM [TempOrdersLinesTbl] "
        DeleteCommand="DELETE FROM [TempOrdersLinesTbl] WHERE [TOLineID] = ?"
        InsertCommand="INSERT INTO [TempOrdersLinesTbl] ([ItemID], [Qty], [PackagingID], [TOLineID]) VALUES (?, ?, ?, ?)"
        UpdateCommand="UPDATE [TempOrdersLinesTbl] SET [ItemID] = ?, [Qty] = ?, [PackagingID] = ? WHERE [TOLineID] = ?">
        <DeleteParameters>
            <asp:Parameter Name="TOLineID" Type="Int32" />
        </DeleteParameters>
        <InsertParameters>
            <asp:Parameter Name="ItemID" Type="Int32" />
            <asp:Parameter Name="Qty" Type="Single" />
            <asp:Parameter Name="PackagingID" Type="Int32" />
            <asp:Parameter Name="TOLineID" Type="Int32" />
        </InsertParameters>
        <UpdateParameters>
            <asp:Parameter Name="ItemID" Type="Int32" />
            <asp:Parameter Name="Qty" Type="Single" />
            <asp:Parameter Name="PackagingID" Type="Int32" />
            <asp:Parameter Name="TOLineID" Type="Int32" />
        </UpdateParameters>
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="sdsItemTypes" runat="server"
        ConnectionString="<%$ ConnectionStrings:Tracker08ConnectionString %>"
        ProviderName="<%$ ConnectionStrings:Tracker08ConnectionString.ProviderName %>"
        SelectCommand="SELECT [ItemTypeID], [ItemDesc] FROM [ItemTypeTbl] ORDER BY [ItemEnabled], [SortOrder], [ItemDesc]"></asp:SqlDataSource>

    <asp:SqlDataSource ID="sdsPackagingTypes" runat="server"
        ConnectionString="<%$ ConnectionStrings:Tracker08ConnectionString %>"
        ProviderName="<%$ ConnectionStrings:Tracker08ConnectionString.ProviderName %>"
        SelectCommand="SELECT [PackagingID], [Description] FROM [PackagingTbl] ORDER BY [Description]"></asp:SqlDataSource>
</asp:Content>
