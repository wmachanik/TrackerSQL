<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="True" CodeBehind="OrderDetail.aspx.cs"
    Inherits="TrackerDotNet.Pages.OrderDetail" MaintainScrollPositionOnPostback="true" %>

<asp:Content ID="cntOrderDetailHdr" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        function ChangeUrl(title, url) {
            if (typeof (history.pushState) != "undefined") {
                var obj = { Title: title, Url: url };
                history.pushState(obj, obj.Title, obj.Url);
            }
            else {
                alert("Browser does not support HTML5.");
            }
        }

        // JavaScript functions for New Order mode
        function showUpdateButton() {
            var div = document.getElementById('divUpdateButton');
            if (div) {
                div.style.display = 'block';
            }
        }

        function hideUpdateButton() {
            var div = document.getElementById('divUpdateButton');
            if (div) {
                div.style.display = 'none';
            }
        }

        // Simple ComboBox enhancement - triggers on Tab key
        function enhanceComboBox() {
            var combo = document.getElementById('<%= cboManualContacts.ClientID %>');
            if (combo) {
                combo.addEventListener('keydown', function (e) {
                    if (e.key === 'Tab' || e.key === 'Enter') {
                        // Small delay to let ComboBox process, then trigger postback if value changed
                        setTimeout(function () {
                            if (combo.value && combo.value !== '0') {
                                __doPostBack('<%= cboManualContacts.ClientID %>', '');
                            }
                        }, 100);
                    }
                });
            }
        }

        // Run after page loads and AJAX updates
        document.addEventListener('DOMContentLoaded', enhanceComboBox);
        if (typeof (Sys) !== 'undefined') {
            Sys.Application.add_load(enhanceComboBox);
        }
    </script>
</asp:Content>
<asp:Content ID="cntOrderDetailBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Literal ID="litPageTitle" runat="server" Text="Order Detail" /></h1>
    <asp:ScriptManager runat="server" ID="scrmOrderDetail" AsyncPostBackTimeout="400" />
    <asp:UpdateProgress ID="udtpOrderDetail" runat="server">
        <ProgressTemplate>
            &nbsp;&nbsp;
      <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />&nbsp;please wait...
        </ProgressTemplate>
    </asp:UpdateProgress>
    <div class="responsive-layout-container">
        <div class="layout-main-panel">
            <asp:UpdatePanel ID="pnlOrderHeader" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:DetailsView ID="dvOrderHeader" runat="server" DataSourceID="odsOrderSummary"
                        AutoGenerateRows="False" CssClass="TblSimple" OnItemUpdated="dvOrderHeader_OnItemUpdated"
                        OnModeChanging="dvOrderHeader_OnModeChanging" OnDataBound="dvOrderHeader_OnDataBound">
                        <EmptyDataTemplate>Return to <a href="DeliverySheet.aspx">delivery sheet...</a></EmptyDataTemplate>
                        <Fields>
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    <asp:HyperLink runat="server" Text="Contact" ID="IDCustomerHdr" NavigateUrl='<%# Bind("CustomerID") == null ? "" : Bind("CustomerID", "~/Pages/CustomerDetails.aspx?ID={0}") %>' />
                                </HeaderTemplate>
                                <EditItemTemplate>
                                    <ajaxToolkit:ComboBox ID="cboContacts" runat="server"
                                        DataSourceID="odsCompanys" DataTextField="CompanyName" DataValueField="CustomerID"
                                        AppendDataBoundItems="true" DropDownStyle="DropDown" CaseSensitive="false"
                                        AutoCompleteMode="SuggestAppend" PromptText="--Select contact--"
                                        SelectedValue='<%# Bind("CustomerID") == null ? "0" : Bind("CustomerID") %>'>
                                        <asp:ListItem Value="0">none</asp:ListItem>
                                    </ajaxToolkit:ComboBox>
                                </EditItemTemplate>
                                <InsertItemTemplate>
                                    <ajaxToolkit:ComboBox ID="cboContacts" runat="server" DataSourceID="sdsCompanys" DataTextField="CompanyName" DataValueField="CustomerID"
                                        AppendDataBoundItems="true" DropDownStyle="DropDown" CaseSensitive="false" AutoCompleteMode="SuggestAppend" PromptText="--Select contact--"
                                        SelectedValue='<%# Bind("CustomerID") == null ? "0" : Bind("CustomerID") %>'>
                                        <asp:ListItem Value="0">none</asp:ListItem>
                                    </ajaxToolkit:ComboBox>
                                </InsertItemTemplate>
                                <ItemTemplate>
                                    <ajaxToolkit:ComboBox ID="cboContacts" runat="server" DataSourceID="sdsCompanys" DataTextField="CompanyName" DataValueField="CustomerID"
                                        AppendDataBoundItems="true" DropDownStyle="Simple" CaseSensitive="false" AutoCompleteMode="SuggestAppend" PromptText="--Select contact--"
                                        Enabled="false" SelectedValue='<%# Bind("CustomerID") == null ? "0" : Bind("CustomerID") %>'>
                                        <asp:ListItem Value="0">none</asp:ListItem>
                                    </ajaxToolkit:ComboBox>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Order Date" SortExpression="OrderDate">
                                <EditItemTemplate>
                                    <asp:TextBox ID="tbxOrderDate" runat="server" Text='<%# Bind("OrderDate", "{0:yyyy-MM-dd}" ) %>' />
                                    <ajaxToolkit:CalendarExtender ID="tbxOrderDate_CalendarExtender" runat="server"
                                        Enabled="True" TargetControlID="tbxOrderDate"></ajaxToolkit:CalendarExtender>
                                </EditItemTemplate>
                                <InsertItemTemplate>
                                    <asp:TextBox ID="tbxOrderDate" runat="server"
                                        Text='<%# Bind("OrderDate", "{0:yyyy-MM-dd}") %>'></asp:TextBox>
                                </InsertItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblOrderDate" runat="server" Text='<%# Bind("OrderDate", "{0:d MMM, ddd, yyyy}") %>' />
                                </ItemTemplate>
                                <ItemStyle Font-Bold="True" />
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Roast Date" SortExpression="RoastDate">
                                <EditItemTemplate>
                                    <asp:TextBox ID="tbxRoastDate" runat="server" Text='<%# Bind("RoastDate", "{0:yyyy-MM-dd}" ) %>' />
                                    <ajaxToolkit:CalendarExtender ID="tbxRoastDate_CalendarExtender" runat="server"
                                        Enabled="True" TargetControlID="tbxRoastDate"></ajaxToolkit:CalendarExtender>
                                </EditItemTemplate>
                                <InsertItemTemplate>
                                    <asp:TextBox ID="tbxRoastDate" runat="server"
                                        Text='<%# Bind("RoastDate", "{0:yyyy-MM-dd}") %>'></asp:TextBox>
                                </InsertItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblRoastDate" runat="server" Text='<%# Bind("RoastDate", "{0:d MMM, ddd, yyyy}") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Delivery By" SortExpression="ToBeDeliveredBy">
                                <EditItemTemplate>
                                    <asp:DropDownList ID="ddlToBeDeliveredBy" runat="server" AppendDataBoundItems="true"
                                        DataSourceID="sdsDeliveryBy" DataTextField="Abreviation" DataValueField="PersonID"
                                        SelectedValue='<%# Bind("ToBeDeliveredBy") == null ? "0" : Bind("ToBeDeliveredBy") %>'>
                                        <asp:ListItem Value="0">n/a</asp:ListItem>
                                    </asp:DropDownList>
                                </EditItemTemplate>
                                <InsertItemTemplate>
                                    <asp:DropDownList ID="ddlToBeDeliveredBy" runat="server" AppendDataBoundItems="true"
                                        DataSourceID="sdsDeliveryBy" DataTextField="Abreviation" DataValueField="PersonID"
                                        SelectedValue='<%# Bind("ToBeDeliveredBy") == null ? "0" : Bind("ToBeDeliveredBy") %>'>
                                        <asp:ListItem Value="0">n/a</asp:ListItem>
                                    </asp:DropDownList>
                                </InsertItemTemplate>
                                <ItemTemplate>
                                    <asp:DropDownList ID="ddlToBeDeliveredBy" runat="server" Enabled="False" AppendDataBoundItems="true"
                                        DataSourceID="sdsDeliveryBy" DataTextField="Abreviation" DataValueField="PersonID"
                                        SelectedValue='<%# Bind("ToBeDeliveredBy") == null ? "0" : Bind("ToBeDeliveredBy") %>'>
                                        <asp:ListItem Value="0">n/a</asp:ListItem>
                                    </asp:DropDownList>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Required Date" SortExpression="RequiredByDate">
                                <EditItemTemplate>
                                    <asp:TextBox ID="tbxRequiredByDate" runat="server" Text='<%# Bind("RequiredByDate", "{0:yyyy-MM-dd}" ) %>' />
                                    <ajaxToolkit:CalendarExtender ID="tbxRequiredByDate_CalendarExtender"
                                        runat="server" Enabled="True" TargetControlID="tbxRequiredByDate"></ajaxToolkit:CalendarExtender>
                                </EditItemTemplate>
                                <InsertItemTemplate>
                                    <asp:TextBox ID="tbxRequiredByDate" runat="server"
                                        Text='<%# Bind("RequiredByDate", "{0:yyyy-MM-dd}") %>'></asp:TextBox>
                                </InsertItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblRequiredByDate" runat="server"
                                        Text='<%# Bind("RequiredByDate", "{0:d MMM, ddd, yyyy }") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Purchase Order" SortExpression="PurchaseOrder">
                                <EditItemTemplate>
                                    <asp:TextBox ID="tbxPurchaseOrder" runat="server" Text='<%# Bind("PurchaseOrder") %>' />
                                </EditItemTemplate>
                                <InsertItemTemplate>
                                    <asp:TextBox ID="tbxPurchaseOrder" runat="server" Text='<%# Bind("PurchaseOrder") %>' />
                                </InsertItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblPurchaseOrder" runat="server" Text='<%# Bind("PurchaseOrder") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Stati" SortExpression="Confirmed">
                                <EditItemTemplate>
                                    <asp:CheckBox ID="cbxConfirmed" TextAlign="Left" Text="Confirmed" runat="server" Checked='<%# Bind("Confirmed") %>' />
                                    &nbsp;&nbsp;&nbsp;<asp:CheckBox ID="cbxInvoiceDone" TextAlign="Left" Text="Invoiced" runat="server" Checked='<%# Bind("InvoiceDone") %>' />
                                    &nbsp;&nbsp;&nbsp;<asp:CheckBox ID="cbxDone" TextAlign="Left" Text="Complete" runat="server" Checked='<%# Bind("Done") %>' Enabled="false" />
                                </EditItemTemplate>
                                <InsertItemTemplate>
                                    <asp:CheckBox ID="cbxConfirmed" TextAlign="Left" Text="Confirmed" runat="server" Checked='<%# Bind("Confirmed") %>' />
                                    &nbsp;&nbsp;&nbsp;<asp:CheckBox ID="cbxInvoiceDone" TextAlign="Left" Text="Invoiced" runat="server" Checked='<%# Bind("InvoiceDone") %>' />
                                    &nbsp;&nbsp;&nbsp;<asp:CheckBox ID="cbxDone" TextAlign="Left" Text="Complete" runat="server" Checked='<%# Bind("Done") %>' />
                                </InsertItemTemplate>
                                <ItemTemplate>
                                    <asp:CheckBox ID="cbxConfirmed" TextAlign="Left" Text="Confirmed" runat="server" Checked='<%# Bind("Confirmed") %>' Enabled="false" />
                                    &nbsp;&nbsp;&nbsp;<asp:CheckBox ID="cbxInvoiceDone" TextAlign="Left" Text="Invoiced" runat="server" Checked='<%# Bind("InvoiceDone") %>' Enabled="false" />
                                    &nbsp;&nbsp;&nbsp;<asp:CheckBox ID="cbxDone" TextAlign="Left" Text="Complete" runat="server" Checked='<%# Bind("Done") %>' Enabled="false" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Notes" SortExpression="Notes">
                                <EditItemTemplate>
                                    <asp:TextBox ID="tbxNotes" runat="server" Text='<%# Bind("Notes") %>'
                                        TextMode="MultiLine" Width="95%"
                                        OnTextChanged="dvOrderHeader_tbxNotes_TextChanged"
                                        AutoPostBack="true" />
                                </EditItemTemplate>
                                <InsertItemTemplate>
                                    <asp:TextBox ID="tbxNotes" runat="server" Text='<%# Bind("Notes") %>'
                                        TextMode="MultiLine"
                                        OnTextChanged="dvOrderHeader_tbxNotes_TextChanged"
                                        AutoPostBack="true" />
                                </InsertItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblNotes" runat="server" Text='<%# Bind("Notes") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:CommandField ControlStyle-CssClass="padded" ShowEditButton="True" ButtonType="Image" EditImageUrl="~/images/imgButtons/EditButton.gif"
                                UpdateImageUrl="~/images/imgButtons/UpdateButton.gif" CancelImageUrl="~/images/imgButtons/CancelButton.gif" />
                        </Fields>
                    </asp:DetailsView>
                </ContentTemplate>
            </asp:UpdatePanel>
            <!-- NEW MANUAL HEADER MODE (for new orders) -->
            <asp:Panel ID="pnlManualHeader" runat="server" Visible="false">
                <asp:UpdatePanel ID="upnlNewOrderSummary" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <table class="TblSimple">
                            <tr>
                                <td>
                                    <asp:HyperLink runat="server" Text="Contact" ID="IDCustomerHdrManual"
                                        NavigateUrl='<%# Bind("CustomerID") == null ? "." : Bind("CustomerID", "~/Pages/CustomerDetails.aspx?ID={0}") %>' />
                                </td>
                                <td>
                                    <ajaxToolkit:ComboBox ID="cboManualContacts" runat="server"
                                        DataSourceID="odsCompanys" DataTextField="CompanyName" DataValueField="CustomerID"
                                        AutoPostBack="true" AppendDataBoundItems="true"
                                        DropDownStyle="DropDown" CaseSensitive="false"
                                        AutoCompleteMode="SuggestAppend" PromptText="----Select name----"
                                        OnSelectedIndexChanged="cboManualContacts_SelectedIndexChanged">
                                        <asp:ListItem Value="0">none</asp:ListItem>
                                    </ajaxToolkit:ComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td>Order Date</td>
                                <td>
                                    <asp:TextBox ID="tbxManualOrderDate" runat="server" Text="" AutoPostBack="true" />
                                    <ajaxToolkit:CalendarExtender ID="tbxManualOrderDate_CalendarExtender" runat="server"
                                        Enabled="True" TargetControlID="tbxManualOrderDate"></ajaxToolkit:CalendarExtender>
                                </td>
                            </tr>
                            <tr>
                                <td>Roast Date</td>
                                <td>
                                    <asp:TextBox ID="tbxManualRoastDate" runat="server" Text="" AutoPostBack="true" />
                                    <ajaxToolkit:CalendarExtender ID="tbxManualRoastDate_CalendarExtender" runat="server"
                                        Enabled="True" TargetControlID="tbxManualRoastDate"></ajaxToolkit:CalendarExtender>
                                </td>
                            </tr>
                            <tr>
                                <td>Delivery By</td>
                                <td>
                                    <asp:DropDownList ID="ddlManualToBeDeliveredBy" runat="server"
                                        DataSourceID="sdsDeliveryBy" DataTextField="Abreviation"
                                        DataValueField="PersonID" AutoPostBack="true" AppendDataBoundItems="true">
                                        <asp:ListItem Value="0">n/a</asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td>Required By</td>
                                <td>
                                    <asp:TextBox ID="tbxManualRequiredByDate" runat="server" AutoPostBack="true" />
                                    <ajaxToolkit:CalendarExtender ID="tbxManualRequiredByDate_CalendarExtender"
                                        runat="server" Enabled="True" TargetControlID="tbxManualRequiredByDate"></ajaxToolkit:CalendarExtender>
                                </td>
                            </tr>
                            <tr>
                                <td>P/Order</td>
                                <td>
                                    <asp:TextBox ID="tbxManualPurchaseOrder" runat="server" AutoPostBack="true"
                                        Width="20em" />
                                </td>
                            </tr>
                            <tr>
                                <td>Stati</td>
                                <td>
                                    <asp:CheckBox ID="cbxManualConfirmed" TextAlign="Left" Text="Confirmed" runat="server" Checked="true" AutoPostBack="true" />
                                    &nbsp;&nbsp;&nbsp;&nbsp;
                       
                                    <asp:CheckBox ID="cbxManualInvoiceDone" TextAlign="Left" Text="Invoiced" runat="server" Checked="false" AutoPostBack="true" />
                                    &nbsp;&nbsp;&nbsp;&nbsp;
                       
                                    <asp:CheckBox ID="cbxManualDone" TextAlign="Left" Text="Done" runat="server" Checked="false" AutoPostBack="true"
                                        OnTextChanged="tbxManualNotes_TextChanged" />
                                </td>
                            </tr>
                            <tr>
                                <td>Notes:</td>
                                <td>
                                    <asp:TextBox ID="tbxManualNotes" runat="server" TextMode="MultiLine" Height="4em" AutoPostBack="true"
                                        Width="98%" OnTextChanged="tbxManualNotes_TextChanged" />
                                </td>
                            </tr>
                        </table>
                        <div id="divLastOrder" style="text-align: center; padding: 10px;">
                            <asp:Button ID="btnLastOrder" runat="server" Text="Last Order"
                                OnClick="btnLastOrder_Click" ToolTip="Load items from customer's last order"
                                Visible="false" />
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="cboManualContacts" EventName="SelectedIndexChanged" />
                        <asp:AsyncPostBackTrigger ControlID="btnLastOrder" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="tbxManualOrderDate" EventName="TextChanged" />
                        <asp:AsyncPostBackTrigger ControlID="tbxManualRoastDate" EventName="TextChanged" />
                        <asp:AsyncPostBackTrigger ControlID="tbxManualRequiredByDate" EventName="TextChanged" />
                        <asp:AsyncPostBackTrigger ControlID="tbxManualNotes" EventName="TextChanged" />
                    </Triggers>
                </asp:UpdatePanel>
            </asp:Panel>
        </div>

        <div class="layout-detail-panel">
            <div class="layout-panel-top">
                <asp:UpdatePanel ID="upnlOrderLines" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <asp:GridView ID="gvOrderLines" runat="server" AutoGenerateColumns="False" CssClass="TblFlex"
                            OnRowUpdated="gvOrderLines_RowUpdated" OnRowCommand="gvOrderLines_RowCommand"
                            OnRowEditing="gvOrderLines_RowEditing" OnRowCancelingEdit="gvOrderLines_RowCancelingEdit"
                            OnRowDataBound="gvOrderLines_RowDataBound" OnRowUpdating="gvOrderLines_RowUpdating"
                            DataSourceID="odsOrderDetail" EmptyDataText="NO ITEMS ADDED">
                            <Columns>
                                <asp:CommandField ShowEditButton="true" ShowDeleteButton="false"
                                    ButtonType="Image" CancelImageUrl="~/images/imgButtons/CancelItem.gif"
                                    DeleteImageUrl="~/images/imgButtons/DelItem.gif"
                                    EditImageUrl="~/images/imgButtons/EditItem.gif" CausesValidation="false"
                                    UpdateImageUrl="~/images/imgButtons/UpdateItem.gif" InsertVisible="False"
                                    EditText="Edit" UpdateText="go" CancelText="no" />
                                <asp:TemplateField HeaderText="Item" SortExpression="ItemTypeID">
                                    <EditItemTemplate>
                                        <ajaxToolkit:ComboBox ID="cboItemDesc" runat="server"
                                            DataSourceID="odsItemTypes" DataTextField="ItemDesc" DataValueField="ItemTypeID" DropDownStyle="DropDown"
                                            CaseSensitive="false" AutoCompleteMode="SuggestAppend" PromptText="--Select item--"
                                            SelectedValue='<%# Bind("ItemTypeID") == null ? "0" : Bind("ItemTypeID") %>'>
                                            <asp:ListItem Text="--Invalid Item--" Value="0" />
                                        </ajaxToolkit:ComboBox>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="lblItemDesc" runat="server" Text='<%# TrackerDotNet.Controls.ItemTypeTbl.GetItemTypeDescById(Convert.ToInt32(Eval("ItemTypeID"))) %>' />
                                        <asp:HiddenField ID="hdnItemTypeID" runat="server" Value='<%# Bind("ItemTypeID") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="QTY" SortExpression="QuantityOrdered">
                                    <EditItemTemplate>
                                        <asp:TextBox ID="tbxQuantityOrdered" runat="server" Text='<%# Bind("QuantityOrdered") %>' Width="4em" MaxLength="8" />
                                        <asp:Label ID="lblItemUoM" runat="server" Text='<%# GetItemUoMObj(Eval("ItemTypeID")) %>' CssClass="small" />
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="lblQuantityOrdered" runat="server" Text='<%# String.Format("{0:0.###}",Eval("QuantityOrdered")) %>' />
                                        <asp:Label ID="lblItemUoM" runat="server" Text='<%# GetItemUoMObj(Eval("ItemTypeID")) %>' CssClass="small" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Prep" SortExpression="PackagingID">
                                    <EditItemTemplate>
                                        <ajaxToolkit:ComboBox ID="cboPackaging" runat="server"
                                            DataSourceID="sdsPackagingTypes" DataTextField="Description" DataValueField="PackagingID"
                                            DropDownStyle="DropDown" CaseSensitive="false" AutoCompleteMode="SuggestAppend"
                                            PromptText="--Select item--" AppendDataBoundItems="true">
                                            <asp:ListItem Text="n/a" Value="0" />
                                        </ajaxToolkit:ComboBox>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="lblPackagingDesc" runat="server" Text='<%# GetPackagingDesc(Convert.ToInt32(Eval("PackagingID"))) %>' />
                                        <asp:HiddenField ID="hdnPackagingID" runat="server" Value='<%# Bind("PackagingID") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:ImageButton ID="MoveOneDayOnImageButton" AlternateText="+date" CommandName="MoveOneDayOn" ImageUrl="~/images/imgButtons/MoveOnADay.gif"
                                            runat="server" CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                                            ToolTip="Move this item to next week day" />
                                        <asp:ImageButton ID="DeleteItemImageButton" AlternateText="del" CommandName="DeleteOrder" ImageUrl="~/images/imgButtons/DelItem.gif"
                                            OnClientClick="return confirm('Are you sure you want to delete this order item?');"
                                            runat="server" CommandArgument='<%# (Eval("OrderID") != null) ? Eval("OrderID") : 0 %>'
                                            ToolTip="delete this" />
                                        <asp:HiddenField ID="hdnOrderID" runat="server" Value='<%# Bind("OrderID") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate>
                                <div class="simpleLightBrownForm">
                                    <h2>No items added</h2>
                                    Please select a customer and add items
                                </div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="gvOrderLines" EventName="RowEditing" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
            <div class="layout-panel-bottom">
                <%--- New item --%>
                <asp:UpdatePanel ID="upnlNewOrderItem" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Button ID="btnNewItem" Text="New Item" runat="server"
                            OnClick="btnNewItem_Click" />
                        <asp:Panel ID="pnlNewItem" runat="server" Visible="false">
                            <table class="TblFlex" cellpadding="0" cellspacing="0">
                                <thead>
                                    <tr>
                                        <th>Item</th>
                                        <th>Qty</th>
                                        <th>Prep</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr>
                                        <td>
                                            <ajaxToolkit:ComboBox ID="cboNewItemDesc" runat="server" DataSourceID="odsItemTypes"
                                                DataTextField="ItemDesc" DataValueField="ItemTypeID" DropDownStyle="DropDown"
                                                CaseSensitive="false" AutoCompleteMode="SuggestAppend" PromptText="--Select item--">
                                            </ajaxToolkit:ComboBox>
                                        </td>
                                        <td>
                                            <asp:TextBox ID="tbxNewQuantityOrdered" runat="server" Text='1' Width="4em" />
                                        </td>
                                        <td>
                                            <ajaxToolkit:ComboBox ID="cboNewPackaging" runat="server" DataSourceID="sdsPackagingTypes"
                                                DataTextField="Description" DataValueField="PackagingID" DropDownStyle="DropDown"
                                                CaseSensitive="false" AutoCompleteMode="SuggestAppend" PromptText="--Select prep--"
                                                AppendDataBoundItems="true">
                                                <asp:ListItem Text="n/a" Value="0" />
                                            </ajaxToolkit:ComboBox>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                            <asp:Button ID="btnAdd" Text="Add" runat="server" Visible="false"
                                OnClick="btnAdd_Click" />&nbsp;&nbsp;
                            <asp:Button ID="btnCancel" Text="Cancel" runat="server" Visible="false" OnClick="btnCancel_Click" />
                        </asp:Panel>
                        &nbsp;&nbsp;<asp:Literal ID="ltrlStatus" Text="" runat="server" />
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
        <div class="layout-footer-panel">
            <asp:UpdatePanel ID="updtButtonPanel" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Button ID="btnNewOrder" runat="server" Text="New Order" AccessKey="N" PostBackUrl="~/Pages/OrderDetail.aspx?NewOrder=true"
                        OnClick="btnNewOrder_Click" ToolTip="new order (AltShftN)" />&nbsp;&nbsp;&nbsp;
                    <asp:Button ID="btnConfirmOrder" runat="server" Text="Email Confirmation" AccessKey="E" OnClick="btnConfirmOrder_Click"
                        ToolTip="send Email confirmation (AltShftE)" />&nbsp;&nbsp;&nbsp;
                    <asp:Button ID="btnDlSheet" runat="server" Text="Delivery Sheet" PostBackUrl="~/Pages/DeliverySheet.aspx" AccessKey="S"
                        ToolTip="new order (AltShftS)" />&nbsp;&nbsp;&nbsp;
                    <asp:Button ID="btnOrderCancelled" runat="server" OnClick="btnCancelled_Click" Text="Cancel Order"
                        ToolTip="xancel this order" />&nbsp;&nbsp;&nbsp;
                    <asp:Button ID="btnUnDoDone" runat="server" Text="UnDo Done" AccessKey="U" OnClick="btnUnDoDone_Click"
                        ToolTip="undo a done order (AltShftU)" />&nbsp;&nbsp;&nbsp;
                    <asp:Button ID="btnOrderDelivered" runat="server" Text="Order Done" AccessKey="D" OnClick="btnOrderDelivered_Click"
                        ToolTip="start order done process (AltShftD)" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>


    <asp:ObjectDataSource ID="odsOrderSummary" runat="server" TypeName="TrackerDotNet.Controls.OrderItemTbl"
        EnablePaging="True" SelectMethod="LoadOrderSummary"
        StartRowIndexParameterName="StartRowIndex"
        MaximumRowsParameterName="MaximumRows"
        OldValuesParameterFormatString="original_{0}"
        OnUpdated="odsOrderSummary_OnUpdated"
        UpdateMethod="UpdateOrderDetails">
        <SelectParameters>
            <asp:SessionParameter DefaultValue="-1" Name="CustomerID" SessionField="BoundCustomerID" Type="Int64" />
            <asp:SessionParameter DefaultValue="" Name="DeliveryDate" SessionField="BoundDeliveryDate" Type="DateTime" />
            <asp:SessionParameter DefaultValue="&quot;&quot;" Name="Notes" SessionField="BoundNotes" Type="String" />
            <asp:Parameter Name="MaximumRows" Type="Int32" />
            <asp:Parameter Name="StartRowIndex" Type="Int32" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="CustomerID" Type="Int64" />
            <asp:Parameter Name="OrderDate" Type="DateTime" />
            <asp:Parameter Name="RoastDate" Type="DateTime" />
            <asp:Parameter Name="ToBeDeliveredBy" Type="Int32" />
            <asp:Parameter Name="RequiredByDate" Type="DateTime" />
            <asp:Parameter Name="Confirmed" Type="Boolean" />
            <asp:Parameter Name="Done" Type="Boolean" />
            <asp:Parameter Name="InvoiceDone" Type="Boolean" />
            <asp:Parameter Name="PurchaseOrder" Type="String" />
            <asp:Parameter Name="Notes" Type="String" />
            <asp:SessionParameter DefaultValue="-1" Name="OriginalCustomerID" SessionField="BoundCustomerID" Type="Int64" />
            <asp:SessionParameter DefaultValue="" Name="OriginalDeliveryDate" SessionField="BoundDeliveryDate" Type="DateTime" />
            <asp:SessionParameter DefaultValue="&quot;&quot;" Name="OriginalNotes" SessionField="BoundNotes" Type="String" />
        </UpdateParameters>
    </asp:ObjectDataSource>
    <%--
SelecT:
          <asp:QueryStringParameter DefaultValue="1" Name="CustomerID" QueryStringField="CustomerID" Type="Int64" />
          <asp:QueryStringParameter Name="DeliveryDate" QueryStringField="DeliveryDate" Type="DateTime" />
          <asp:QueryStringParameter Name="Notes" QueryStringField="Notes" Type="String" />
Update:
          <asp:QueryStringParameter DefaultValue="1" Name="OriginalCustomerID" QueryStringField="CustomerID" Type="Int64" />
          <asp:QueryStringParameter Name="OriginalDeliveryDate" QueryStringField="DeliveryDate" Type="DateTime" />
          <asp:QueryStringParameter Name="OriginalNotes" QueryStringField="Notes" Type="String" />


    --%>
    <!-- Replace the SqlDataSource with ObjectDataSource -->
    <asp:ObjectDataSource ID="odsCompanys" runat="server"
        SelectMethod="GetAllCustomerNames"
        TypeName="TrackerDotNet.Controls.CustomersTbl" />

    <!-- Remove or comment out the old SqlDataSource -->
    <!--
<asp:SqlDataSource ID="sdsCompanys" runat="server"
    ConnectionString="<%$ ConnectionStrings:Tracker08ConnectionString %>"
    ProviderName="<%$ ConnectionStrings:Tracker08ConnectionString.ProviderName %>"
    SelectCommand="SELECT [CompanyName], [CustomerID] FROM [CustomersTbl] ORDER BY [enabled], [CompanyName]"></asp:SqlDataSource>
-->
    <asp:ObjectDataSource ID="odsOrderDetail" runat="server"
        TypeName="TrackerDotNet.Controls.OrderDetailDAL" SelectMethod="LoadOrderDetailData"
        UpdateMethod="UpdateOrderDetails"
        StartRowIndexParameterName="StartRowIndex"
        MaximumRowsParameterName="MaximumRows"
        OldValuesParameterFormatString="original_{0}"
        DeleteMethod="DeleteOrderDetails">
        <DeleteParameters>
            <asp:Parameter Name="OrderID" Type="Int64" />
        </DeleteParameters>
        <UpdateParameters>
            <asp:Parameter Name="OrderID" Type="Int64" />
            <asp:SessionParameter DefaultValue="-1" Name="CustomerID" SessionField="BoundCustomerID" Type="Int64" />
            <asp:Parameter Name="ItemTypeID" Type="Int32" />
            <asp:SessionParameter DefaultValue="" Name="DeliveryDate" SessionField="BoundDeliveryDate" Type="DateTime" />
            <asp:Parameter Name="QuantityOrdered" Type="Double" />
            <asp:Parameter Name="PackagingID" Type="Int32" />
        </UpdateParameters>
        <SelectParameters>
            <asp:SessionParameter DefaultValue="1" Name="CustomerID" SessionField="BoundCustomerID" Type="Int64" />
            <asp:SessionParameter DefaultValue="" Name="DeliveryDate" SessionField="BoundDeliveryDate" Type="DateTime" />
            <asp:SessionParameter DefaultValue="&quot;&quot;" Name="Notes" SessionField="BoundNotes" Type="String" />
            <asp:Parameter Name="MaximumRows" Type="Int32" />
            <asp:Parameter Name="StartRowIndex" Type="Int32" />
        </SelectParameters>
    </asp:ObjectDataSource>

    <br />

    <asp:SqlDataSource ID="sdsDeliveryBy" runat="server"
        ConnectionString="<%$ ConnectionStrings:Tracker08ConnectionString %>"
        ProviderName="<%$ ConnectionStrings:Tracker08ConnectionString.ProviderName %>"
        SelectCommand="SELECT [PersonID], [Abreviation] FROM [PersonsTbl] ORDER BY [Enabled], [Abreviation]"></asp:SqlDataSource>
    <asp:ObjectDataSource ID="odsItemTypes" runat="server" SelectMethod="GetAllItemDesc" TypeName="TrackerDotNet.Controls.ItemTypeTbl" />

    <%--   SelectCommand="SELECT [ItemTypeID], [ItemDesc] FROM [ItemTypeTbl] ORDER BY [ItemEnabled], [SortOrder], [ItemDesc]"> --%>
    <asp:SqlDataSource ID="sdsPackagingTypes" runat="server"
        ConnectionString="<%$ ConnectionStrings:Tracker08ConnectionString %>"
        ProviderName="<%$ ConnectionStrings:Tracker08ConnectionString.ProviderName %>"
        SelectCommand="SELECT [PackagingID], [Description] FROM [PackagingTbl] ORDER BY [Description]"></asp:SqlDataSource>

    <%--
  <asp:Label ID="lblCustomerID" Text="" runat="server" />&nbsp;
  <asp:Label ID="lblDeliveryDate" Text="" runat="server" />

  <br />
    --%>
</asp:Content>
