<%@ Page Title="Order Done" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="OrderDone.aspx.cs" Inherits="TrackerSQL.Pages.OrderDone" %>

<asp:Content ID="cntOrderDoneHdr" ContentPlaceHolderID="HeadContent" runat="server">
    <%-- Keep HeadContent free of <%= %> — ScriptManager cannot modify <head> when it contains code blocks. --%>
</asp:Content>

<asp:Content ID="cntOrderDoneBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smOrderDone" runat="server" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="updtprgOrderDone" runat="server" AssociatedUpdatePanelID="updtpnlOrderDone"
        DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info page-tone-progress">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="updtpnlOrderDone" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnDone" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="rbtnSendConfirm" EventName="SelectedIndexChanged" />
            <asp:PostBackTrigger ControlID="btnCancel" />
            <asp:PostBackTrigger ControlID="btnReturnToDeliveres" />
        </Triggers>
        <ContentTemplate>
            <asp:Panel ID="pnlOrderShell" runat="server" CssClass="simpleForm page-tone-panel page-tone-orders">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/icons8-order-completed-16.png" alt="Order Done" />
                    <div>
                        <h1 class="page-tone-title">Order Done</h1>
                        <p class="page-tone-subtitle">Confirm delivery and update contact usage</p>
                    </div>
                </div>

                <asp:Panel ID="pnlOrderDetails" runat="server">
                    <asp:FormView ID="fvOrderDone" runat="server" DataSourceID="odsOrderDoneHeader"
                        CssClass="detail-form-table" Width="100%">
                        <ItemTemplate>
                            <table class="TblCoffee detail-form-table" style="width: 100%; margin-top: 8px;">
                                <tr>
                                    <td>Company</td>
                                    <td>
                                        <asp:Label ID="CompanyNameLabel" runat="server" Text='<%# Eval("CompanyName") %>' />
                                        <asp:Label ID="CustomerIDLabel" runat="server" Text='<%# Eval("CustomerID") %>' Visible="false" />
                                    </td>
                                    <td>Delivery date</td>
                                    <td>
                                        <asp:TextBox ID="ByDateTextBox" runat="server"
                                            Text='<%# Eval("RequiredByDate", "{0:yyyy-MM-dd}") %>' Width="10em" />
                                    </td>
                                </tr>
                            </table>
                        </ItemTemplate>
                    </asp:FormView>

                    <div class="results-container" style="margin-top: 12px;">
                        <asp:GridView ID="gvOrderDoeLines" runat="server" AllowSorting="True" AutoGenerateColumns="False"
                            DataKeyNames="TOLineID" CssClass="results-table" DataSourceID="odsOrderDoneLines"
                            EmptyDataText="No lines on this delivery.">
                            <Columns>
                                <asp:BoundField DataField="TOLineID" HeaderText="ID" Visible="false" InsertVisible="False"
                                    ReadOnly="True" SortExpression="TOLineID" />
                                <asp:TemplateField HeaderText="Item" SortExpression="ItemID">
                                    <EditItemTemplate>
                                        <asp:DropDownList ID="ddlItemDesc" runat="server" DataSourceID="odsItemTypes"
                                            DataTextField="ItemDesc" DataValueField="ItemTypeID"
                                            AppendDataBoundItems="True" SelectedValue='<%# Bind("ItemID") %>'>
                                            <asp:ListItem Value="0">n/a</asp:ListItem>
                                        </asp:DropDownList>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlItemDesc" runat="server" DataSourceID="odsItemTypes"
                                            DataTextField="ItemDesc" DataValueField="ItemTypeID" Enabled="false"
                                            AppendDataBoundItems="True"
                                            SelectedValue='<%# Eval("ItemID") == null ? "0" : Eval("ItemID").ToString() %>'>
                                            <asp:ListItem Value="0">n/a</asp:ListItem>
                                        </asp:DropDownList>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="Qty" HeaderText="Qty" SortExpression="Qty">
                                    <ItemStyle HorizontalAlign="Right" Width="4em" />
                                </asp:BoundField>
                                <asp:TemplateField HeaderText="Packaging" SortExpression="PackagingID">
                                    <EditItemTemplate>
                                        <asp:DropDownList ID="ddlPackaging" runat="server" AppendDataBoundItems="true"
                                            DataSourceID="odsPackagingTypes" DataTextField="Description"
                                            DataValueField="PackagingID" SelectedValue='<%# Bind("PackagingID") %>'>
                                            <asp:ListItem Value="0">n/a</asp:ListItem>
                                        </asp:DropDownList>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlPackaging" runat="server" AppendDataBoundItems="true"
                                            DataSourceID="odsPackagingTypes" DataTextField="Description"
                                            DataValueField="PackagingID" Enabled="false"
                                            SelectedValue='<%# Eval("PackagingID") == null ? "0" : Eval("PackagingID").ToString() %>'>
                                            <asp:ListItem Value="0">n/a</asp:ListItem>
                                        </asp:DropDownList>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:CommandField ButtonType="Button" ShowDeleteButton="True" ShowEditButton="True"
                                    HeaderText="Action" ControlStyle-CssClass="filter-panel-btn" />
                            </Columns>
                        </asp:GridView>
                    </div>

                    <table class="TblCoffee detail-form-table" style="width: 100%; margin-top: 16px;">
                        <tr>
                            <td style="width: 8em;">Stock (kg)</td>
                            <td style="width: 8em;">
                                <asp:TextBox ID="tbxStock" runat="server" Width="5em" CssClass="small" />
                            </td>
                            <td rowspan="4" style="vertical-align: top; padding-left: 16px;">
                                <asp:RadioButtonList ID="rbtnSendConfirm" runat="server" CssClass="small"
                                    AutoPostBack="true" OnSelectedIndexChanged="rbtnSendConfirm_SelectedIndexChanged">
                                    <asp:ListItem Text="No confirmation email" Value="none" />
                                    <asp:ListItem Text="Send 'in the post box' message" Value="postbox" />
                                    <asp:ListItem Text="Send 'order dispatch' message" Value="dispatched" />
                                    <asp:ListItem Text="Send 'order collected' message" Value="collected" />
                                    <asp:ListItem Text="Send 'order delivered' message" Value="done" Selected="True" />
                                </asp:RadioButtonList>
                            </td>
                        </tr>
                        <tr>
                            <td>Cup count</td>
                            <td>
                                <asp:TextBox ID="tbxCount" runat="server" Width="5em" CssClass="small" />
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:Panel ID="pnlTracking" runat="server" Visible="false" CssClass="page-tone-subtitle" style="margin-top: 8px;">
                                    <asp:Label ID="lblTracking" runat="server" AssociatedControlID="tbxTrackingNumber"
                                        Text="Tracking / waybill" />
                                    <asp:TextBox ID="tbxTrackingNumber" runat="server" Width="16em" MaxLength="100"
                                        CssClass="small" style="display: block; margin-top: 4px;" />
                                    <asp:RequiredFieldValidator ID="rfvTracking" runat="server" ControlToValidate="tbxTrackingNumber"
                                        Enabled="false" Display="Dynamic" CssClass="status-error"
                                        ErrorMessage="Tracking / waybill number is required for Pargo and courier." />
                                    <p class="page-tone-subtitle" style="margin: 6px 0 0; font-size: 0.9em;">
                                        Required for Pargo / courier. Tracker emails this number. If the order came from WooCommerce,
                                        a customer note is added in Woo — the Woo order is <strong>not</strong> completed.
                                    </p>
                                </asp:Panel>
                            </td>
                        </tr>
                    </table>

                    <div class="button-row" style="margin-top: 16px;">
                        <asp:Button ID="btnDone" Text="Done" runat="server" AccessKey="D" CssClass="filter-panel-btn"
                            OnClick="btnDone_Click" ToolTip="Mark order delivered and update usage (Alt+D)" />
                        <span class="image-button" title="Cancel and return to delivery sheet">
                            <asp:ImageButton ID="btnCancel" runat="server"
                                ImageUrl="~/images/imgButtons/Back.gif"
                                AlternateText="Back"
                                ToolTip="Cancel and return to delivery sheet"
                                OnClick="btnCancel_Click"
                                CausesValidation="false" />
                        </span>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlCustomerDetailsUpdated" runat="server" Visible="false">
                    <h2 class="page-tone-subtitle" style="font-size: 1.1em; margin: 12px 0 8px;">Contact updated</h2>
                    <asp:Label ID="tbxCustomerName" Text="" runat="server" CssClass="small" style="font-weight: 600;" />
                    <div class="results-container" style="margin-top: 12px; overflow-x: auto;">
                        <asp:DataGrid ID="dgCustomerUsage" runat="server" CssClass="results-table small" Width="100%">
                            <Columns>
                                <asp:BoundColumn DataField="CustomerID" Visible="false" />
                                <asp:BoundColumn DataField="LastCupCount" HeaderText="Last Count" ItemStyle-HorizontalAlign="Right" />
                                <asp:BoundColumn DataField="NextCoffeeBy" HeaderText="Next coffee" DataFormatString="{0:d}" />
                                <asp:BoundColumn DataField="DailyConsumption" HeaderText="Daily use" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:0.##}" />
                                <asp:BoundColumn DataField="NextCleanOn" HeaderText="Next clean" DataFormatString="{0:d}" />
                                <asp:BoundColumn DataField="CleanAveCount" HeaderText="Clean avg" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:0.##}" />
                                <asp:BoundColumn DataField="NextFilterEst" HeaderText="Next filter" DataFormatString="{0:d}" />
                                <asp:BoundColumn DataField="FilterAveCount" HeaderText="Filter avg" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:0.##}" />
                                <asp:BoundColumn DataField="NextDescaleEst" HeaderText="Next descale" DataFormatString="{0:d}" />
                                <asp:BoundColumn DataField="DescaleAveCount" HeaderText="Descale avg" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:0.##}" />
                                <asp:BoundColumn DataField="NextServiceEst" HeaderText="Next service" DataFormatString="{0:d}" />
                                <asp:BoundColumn DataField="ServiceAveCount" HeaderText="Service avg" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:0.##}" />
                            </Columns>
                        </asp:DataGrid>
                    </div>
                    <div class="button-row" style="margin-top: 16px;">
                        <asp:Button ID="btnReturnToDeliveres" Text="Return to Delivery Sheet" AccessKey="D"
                            runat="server" CssClass="filter-panel-btn" OnClick="btnReturnToDeliveres_Click" />
                    </div>
                </asp:Panel>

                <div class="status-message" id="pnlStatus" runat="server" style="margin-top: 12px;">
                    <asp:Literal ID="ltrlStatus" runat="server" />
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>

    <asp:ObjectDataSource ID="odsOrderDoneHeader" runat="server"
        TypeName="TrackerSQL.Managers.OrderDoneDataSource"
        SelectMethod="GetHeader">
        <SelectParameters>
            <asp:SessionParameter Name="toHeaderId" SessionField="TempOrderHeaderId" Type="Int32" DefaultValue="0" />
        </SelectParameters>
    </asp:ObjectDataSource>

    <asp:ObjectDataSource ID="odsOrderDoneLines" runat="server"
        TypeName="TrackerSQL.Managers.OrderDoneDataSource"
        SelectMethod="GetLines"
        UpdateMethod="UpdateLine"
        DeleteMethod="DeleteLine">
        <SelectParameters>
            <asp:SessionParameter Name="toHeaderId" SessionField="TempOrderHeaderId" Type="Int32" DefaultValue="0" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="TOLineID" Type="Int32" />
            <asp:Parameter Name="ItemID" Type="Int32" />
            <asp:Parameter Name="Qty" Type="Single" />
            <asp:Parameter Name="PackagingID" Type="Int32" />
        </UpdateParameters>
        <DeleteParameters>
            <asp:Parameter Name="TOLineID" Type="Int32" />
        </DeleteParameters>
    </asp:ObjectDataSource>

    <asp:ObjectDataSource ID="odsItemTypes" runat="server"
        TypeName="TrackerSQL.Managers.OrderLookupDataSource"
        SelectMethod="GetItems">
        <SelectParameters>
            <asp:Parameter Name="sortBy" Type="String" DefaultValue="" />
        </SelectParameters>
    </asp:ObjectDataSource>

    <asp:ObjectDataSource ID="odsPackagingTypes" runat="server"
        TypeName="TrackerSQL.Managers.OrderLookupDataSource"
        SelectMethod="GetPackagingTypes" />
</asp:Content>
