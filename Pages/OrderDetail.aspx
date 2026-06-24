<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="True" CodeBehind="OrderDetail.aspx.cs"
    Inherits="TrackerSQL.Pages.OrderDetail" MaintainScrollPositionOnPostback="true" EnableEventValidation="false" %>

<asp:Content ID="cntOrderDetailHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntOrderDetailBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Literal ID="litPageTitle" runat="server" Text="Order Detail" /></h1>
    <asp:ScriptManager runat="server" ID="scrmOrderDetail" AsyncPostBackTimeout="400" />
    <asp:UpdateProgress ID="udtpOrderDetail" runat="server" DisplayAfter="100" DynamicLayout="true">

    <ProgressTemplate>
            &nbsp;Please Wait: &nbsp;
            <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />&nbsp;...
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:Panel ID="pnlOrderConflict" runat="server" Visible="false" CssClass="simpleLightBrownForm" style="margin-bottom:1em;padding:1em;">
        <asp:Literal ID="litConflictMessage" runat="server" />
        <div style="margin-top:0.75em;">
            <asp:Button ID="btnOpenExistingOrder" runat="server" Text="Open existing order" OnClick="btnOpenExistingOrder_Click" />
            &nbsp;
            <asp:Button ID="btnUseExistingOrder" runat="server" Text="Add lines to existing order" OnClick="btnUseExistingOrder_Click" />
            &nbsp;
            <asp:Button ID="btnDismissConflict" runat="server" Text="Cancel" OnClick="btnDismissConflict_Click" CausesValidation="false" />
        </div>
    </asp:Panel>

    <div class="responsive-layout-container">
        <div class="layout-main-panel">
            <asp:UpdatePanel ID="pnlOrderHeader" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <table class="TblSimple">
                        <tr>
                            <td>
                                <asp:HyperLink runat="server" Text="Contact" ID="hlContactHdr" NavigateUrl="#" />
                            </td>
                            <td>
                                <ajaxToolkit:ComboBox ID="cboContacts" runat="server"
                                    EnableViewState="false"
                                    AutoPostBack="true"
                                    DropDownStyle="DropDown" CaseSensitive="false"
                                    AutoCompleteMode="SuggestAppend" PromptText="----Select name----"
                                    OnSelectedIndexChanged="cboContacts_SelectedIndexChanged">
                                    <asp:ListItem Value="0">none</asp:ListItem>
                                </ajaxToolkit:ComboBox>
                            </td>
                        </tr>
                        <tr>
                            <td>Order Date</td>
                            <td>
                                <asp:TextBox ID="tbxOrderDate" runat="server" AutoPostBack="true" OnTextChanged="HeaderField_Changed" />
                                <ajaxToolkit:CalendarExtender ID="tbxOrderDate_CalendarExtender" runat="server"
                                    Enabled="True" TargetControlID="tbxOrderDate" />
                            </td>
                        </tr>
                        <tr>
                            <td>Prep Date</td>
                            <td>
                                <asp:TextBox ID="tbxPrepDate" runat="server" AutoPostBack="true" OnTextChanged="HeaderField_Changed" />
                                <ajaxToolkit:CalendarExtender ID="tbxPrepDate_CalendarExtender" runat="server"
                                    Enabled="True" TargetControlID="tbxPrepDate" />
                            </td>
                        </tr>
                        <tr>
                            <td>Delivery By</td>
                            <td>
                                <asp:DropDownList ID="ddlToBeDeliveredBy" runat="server"
                                    DataTextField="Abbreviation" DataValueField="PersonID"
                                    AutoPostBack="true" AppendDataBoundItems="true"
                                    OnSelectedIndexChanged="HeaderField_Changed">
                                    <asp:ListItem Value="0">n/a</asp:ListItem>
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td>Required By</td>
                            <td>
                                <asp:TextBox ID="tbxRequiredByDate" runat="server" AutoPostBack="true" OnTextChanged="HeaderField_Changed" />
                                <ajaxToolkit:CalendarExtender ID="tbxRequiredByDate_CalendarExtender" runat="server"
                                    Enabled="True" TargetControlID="tbxRequiredByDate" />
                            </td>
                        </tr>
                        <tr>
                            <td>P/Order</td>
                            <td>
                                <asp:TextBox ID="tbxPurchaseOrder" runat="server" AutoPostBack="true"
                                    Width="20em" OnTextChanged="HeaderField_Changed" />
                            </td>
                        </tr>
                        <tr>
                            <td>Stati</td>
                            <td>
                                <asp:CheckBox ID="cbxConfirmed" TextAlign="Left" Text="Confirmed" runat="server"
                                    Checked="true" AutoPostBack="true" OnCheckedChanged="HeaderField_Changed" />
                                &nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:CheckBox ID="cbxInvoiceDone" TextAlign="Left" Text="Invoiced" runat="server"
                                    AutoPostBack="true" OnCheckedChanged="HeaderField_Changed" />
                                &nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:CheckBox ID="cbxDone" TextAlign="Left" Text="Done" runat="server"
                                    Enabled="false" />
                            </td>
                        </tr>
                        <tr>
                            <td>Notes:</td>
                            <td>
                                <asp:TextBox ID="tbxNotes" runat="server" TextMode="MultiLine" Height="4em"
                                    Width="98%" AutoPostBack="true" OnTextChanged="HeaderField_Changed" />
                            </td>
                        </tr>
                    </table>
                    <div style="display:flex;justify-content:space-between;align-items:center;padding:10px;">
                        <button id="btnUndoHeader" runat="server" type="submit" class="btn btn-undo"
                            onserverclick="btnUndoHeader_Click" causesvalidation="false"
                            title="Revert the last header change" style="display:none;">
                            <asp:Image ID="imgUndoHeader" runat="server" ImageUrl="~/images/imgButtons/Undo.png" AlternateText="" />
                            <span>Undo</span>
                        </button>
                        <asp:Button ID="btnLastOrder" runat="server" Text="Last Order"
                            OnClick="btnLastOrder_Click" ToolTip="Load items from customer's last order"
                            Visible="false" />
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="cboContacts" EventName="SelectedIndexChanged" />
                    <asp:AsyncPostBackTrigger ControlID="btnLastOrder" EventName="Click" />
                </Triggers>
            </asp:UpdatePanel>
        </div>

        <div class="layout-detail-panel">
            <div class="layout-panel-top">
                <asp:UpdatePanel ID="upnlOrderLines" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <asp:GridView ID="gvOrderLines" runat="server" AutoGenerateColumns="False" CssClass="TblFlex"
                            DataKeyNames="OrderLineID"
                            OnRowUpdated="gvOrderLines_RowUpdated" OnRowCommand="gvOrderLines_RowCommand"
                            OnRowEditing="gvOrderLines_RowEditing" OnRowCancelingEdit="gvOrderLines_RowCancelingEdit"
                            OnRowDataBound="gvOrderLines_RowDataBound" OnRowUpdating="gvOrderLines_RowUpdating"
                            EmptyDataText="NO ITEMS ADDED">
                            <Columns>
                                <asp:CommandField ShowEditButton="true" ShowDeleteButton="false"
                                    ButtonType="Image" CancelImageUrl="~/images/imgButtons/CancelItem.gif"
                                    DeleteImageUrl="~/images/imgButtons/DelItem.gif"
                                    EditImageUrl="~/images/imgButtons/EditItem.gif" CausesValidation="false"
                                    UpdateImageUrl="~/images/imgButtons/UpdateItem.gif" InsertVisible="False" />
                                <asp:TemplateField HeaderText="Item" SortExpression="ItemTypeID">
                                    <EditItemTemplate>
                                        <ajaxToolkit:ComboBox ID="cboItemDesc" runat="server"
                                            DataTextField="ItemDesc" DataValueField="ItemTypeID" DropDownStyle="DropDown"
                                            CaseSensitive="false" AutoCompleteMode="SuggestAppend" PromptText="--Select item--"
                                            AppendDataBoundItems="true" SelectedValue='<%# Eval("ItemTypeID") %>'>
                                            <asp:ListItem Text="--Invalid Item--" Value="0" />
                                        </ajaxToolkit:ComboBox>
                                        <asp:HiddenField ID="hdnItemTypeID" runat="server" Value='<%# Eval("ItemTypeID") %>' />
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="lblItemDesc" runat="server" Text='<%# GetItemDescById(Convert.ToInt32(Eval("ItemTypeID"))) %>' />
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
                                            DataTextField="Description" DataValueField="PackagingID"
                                            DropDownStyle="DropDown" CaseSensitive="false" AutoCompleteMode="SuggestAppend"
                                            PromptText="--Select item--" AppendDataBoundItems="true"
                                            SelectedValue='<%# Eval("PackagingID") %>'>
                                            <asp:ListItem Text="n/a" Value="0" />
                                        </ajaxToolkit:ComboBox>
                                        <asp:HiddenField ID="hdnPackagingID" runat="server" Value='<%# Eval("PackagingID") %>' />
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="lblPackagingDesc" runat="server" Text='<%# GetPackagingDesc(Convert.ToInt32(Eval("PackagingID"))) %>' />
                                        <asp:HiddenField ID="hdnPackagingID" runat="server" Value='<%# Eval("PackagingID") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:ImageButton ID="MoveOneDayOnImageButton" AlternateText="+date" CommandName="MoveOneDayOn"
                                            ImageUrl="~/images/imgButtons/MoveOnADay.gif" runat="server"
                                            CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                                            ToolTip="Move this item to next week day" />
                                        <asp:ImageButton ID="DeleteItemImageButton" AlternateText="del" CommandName="DeleteOrder"
                                            ImageUrl="~/images/imgButtons/DelItem.gif"
                                            OnClientClick="return confirm('Are you sure you want to delete this order item?');"
                                            runat="server" CommandArgument='<%# Eval("OrderLineID") %>'
                                            ToolTip="delete this" />
                                        <asp:HiddenField ID="hdnOrderLineID" runat="server" Value='<%# Bind("OrderLineID") %>' />
                                        <asp:HiddenField ID="hdnOrderID" runat="server" Value='<%# Bind("OrderID") %>' />
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:HiddenField ID="hdnOrderLineID" runat="server" Value='<%# Eval("OrderLineID") %>' />
                                        <asp:HiddenField ID="hdnOrderID" runat="server" Value='<%# Eval("OrderID") %>' />
                                    </EditItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate>
                                <div class="simpleLightBrownForm">
                                    <h2>No items added</h2>
                                    Please select a contact and add items
                                </div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <div class="layout-panel-bottom">
                <asp:UpdatePanel ID="upnlNewOrderItem" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="cboContacts" EventName="SelectedIndexChanged" />
                        <asp:AsyncPostBackTrigger ControlID="btnNewItem" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="btnAdd" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="btnCancel" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="tbxNotes" EventName="TextChanged" />
                    </Triggers>
                    <ContentTemplate>
                        <asp:Button ID="btnNewItem" Text="New Item" runat="server" Enabled="false" OnClick="btnNewItem_Click" />
                        <asp:Panel ID="pnlNewItem" runat="server" style="display:none;">
                            <table class="TblFlex" cellpadding="0" cellspacing="0">
                                <thead>
                                    <tr><th>Item</th><th>Qty</th><th>Prep</th></tr>
                                </thead>
                                <tbody>
                                    <tr>
                                        <td>
                                            <ajaxToolkit:ComboBox ID="cboNewItemDesc" runat="server"
                                                DataTextField="ItemDesc" DataValueField="ItemTypeID" DropDownStyle="DropDown"
                                                CaseSensitive="false" AutoCompleteMode="SuggestAppend" PromptText="--Select item--" />
                                        </td>
                                        <td>
                                            <asp:TextBox ID="tbxNewQuantityOrdered" runat="server" Text="1" Width="4em" />
                                        </td>
                                        <td>
                                            <ajaxToolkit:ComboBox ID="cboNewPackaging" runat="server"
                                                DataTextField="Description" DataValueField="PackagingID" DropDownStyle="DropDown"
                                                CaseSensitive="false" AutoCompleteMode="SuggestAppend" PromptText="--Select prep--"
                                                AppendDataBoundItems="true">
                                                <asp:ListItem Text="n/a" Value="0" />
                                            </ajaxToolkit:ComboBox>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                            <asp:Button ID="btnAdd" Text="Add" runat="server" style="display:none;" OnClick="btnAdd_Click" />
                            &nbsp;&nbsp;
                            <asp:Button ID="btnCancel" Text="Cancel" runat="server" style="display:none;" OnClick="btnCancel_Click" />
                        </asp:Panel>
                        &nbsp;&nbsp;<asp:Literal ID="ltrlStatus" Text="" runat="server" />
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
        <div class="layout-footer-panel">
            <asp:UpdatePanel ID="updtButtonPanel" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Button ID="btnNewOrder" runat="server" Text="New Order" AccessKey="N"
                        PostBackUrl="~/Pages/OrderDetail.aspx?NewOrder=true" ToolTip="new order (AltShftN)" />
                    &nbsp;&nbsp;&nbsp;
                    <asp:Button ID="btnConfirmOrder" runat="server" Text="Email Confirmation" AccessKey="E"
                        OnClick="btnConfirmOrder_Click" ToolTip="send Email confirmation (AltShftE)" />
                    &nbsp;&nbsp;&nbsp;
                    <asp:Button ID="btnDlSheet" runat="server" Text="Delivery Sheet"
                        PostBackUrl="~/Pages/DeliverySheet.aspx" AccessKey="S" ToolTip="delivery sheet (AltShftS)" />
                    &nbsp;&nbsp;&nbsp;
                    <asp:Button ID="btnOrderCancelled" runat="server" OnClick="btnCancelled_Click" Text="Cancel Order"
                        ToolTip="cancel this order" />
                    &nbsp;&nbsp;&nbsp;
                    <asp:Button ID="btnUnDoDone" runat="server" Text="UnDo Done" AccessKey="U"
                        OnClick="btnUnDoDone_Click" ToolTip="undo a done order (AltShftU)" />
                    &nbsp;&nbsp;&nbsp;
                    <asp:Button ID="btnOrderDelivered" runat="server" Text="Order Done" AccessKey="D"
                        OnClick="btnOrderDelivered_Click" ToolTip="start order done process (AltShftD)" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
</asp:Content>
