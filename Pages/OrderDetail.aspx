<%@ Page Title="Order Detail" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="True" CodeBehind="OrderDetail.aspx.cs"
    Inherits="TrackerSQL.Pages.OrderDetail" MaintainScrollPositionOnPostback="true" EnableEventValidation="false" %>
<%@ Import Namespace="TrackerSQL.Classes" %>

<asp:Content ID="cntOrderDetailHdr" title="Order Detail" ContentPlaceHolderID="HeadContent" runat="server">
    <%-- Keep HeadContent free of <%= %> — ScriptManager cannot modify <head> when it contains code blocks. --%>
</asp:Content>
<asp:Content ID="cntOrderDetailBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager runat="server" ID="scrmOrderDetail" AsyncPostBackTimeout="400" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="udtpOrderDetail" runat="server" DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info page-tone-progress">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="upnlSaveReturnRedirect" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:HiddenField ID="hdnSaveReturnRedirectUrl" runat="server" Value="" />
        </ContentTemplate>
    </asp:UpdatePanel>

    <div id="orderDetailSaveReturnOverlay" class="order-detail-save-return-overlay" style="display: none;" aria-live="polite" aria-busy="true">
        <div class="order-detail-save-return-message">
            Saving and returning...
            <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
        </div>
    </div>

    <asp:Panel ID="pnlOrderShell" runat="server" CssClass="simpleForm page-tone-panel page-tone-orders">
                <div class="page-tone-header tool-card-header">
                    <asp:Image ID="imgPageToneIcon" runat="server" CssClass="tool-card-icon"
                        ImageUrl="~/images/imgButtons/icons8-view-orders-16.png" AlternateText="" />
                    <div>
                        <asp:UpdatePanel ID="upnlPageTitle" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
                            <ContentTemplate>
                                <h1 class="page-tone-title">Order: <asp:Literal ID="litPageTitle" runat="server" Text="Order Detail" /></h1>
                                <p class="page-tone-subtitle"><asp:Literal ID="litPageSubtitle" runat="server" Text="Manage existing orders and deliveries" /></p>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>

    <div class="order-detail-stack">
        <asp:UpdatePanel ID="upnlOrderConflict" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true" CssClass="instruction-dialog-host">
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnAdd" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnLastOrder" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnSaveHeader" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnSaveAndReturn" EventName="Click" />
            </Triggers>
            <ContentTemplate>
                <asp:Panel ID="pnlOrderConflictShell" runat="server" Visible="false" CssClass="instruction-dialog-shell">
                    <div class="responsive-layout-container compact">
                        <asp:Panel ID="pnlOrderConflict" runat="server" CssClass="instruction-dialog instruction-dialog--attention">
                            <div class="instruction-dialog-message">
                                <asp:Literal ID="litConflictMessage" runat="server" />
                            </div>
                            <asp:Panel ID="pnlAddLineConflictActions" runat="server" CssClass="instruction-dialog-actions action-bar button-row">
                                <asp:Button ID="btnUseExistingOrder" runat="server" Text="Merge with existing order"
                                    OnClick="btnUseExistingOrder_Click" CssClass="filter-panel-btn" />
                                <asp:Button ID="btnCreateNewOrderAnyway" runat="server" Text="Create new order anyway"
                                    OnClick="btnCreateNewOrderAnyway_Click" CausesValidation="false" CssClass="filter-panel-btn" />
                                <asp:Button ID="btnOpenExistingOrder" runat="server" Text="Open existing order"
                                    OnClick="btnOpenExistingOrder_Click" CausesValidation="false" CssClass="filter-panel-btn" />
                                <asp:Button ID="btnDismissConflict" runat="server" Text="Cancel (drop line)"
                                    OnClick="btnDismissConflict_Click" CausesValidation="false" CssClass="filter-panel-btn" />
                            </asp:Panel>
                            <asp:Panel ID="pnlMergePromptActions" runat="server" Visible="false"
                                CssClass="instruction-dialog-actions action-bar button-row">
                                <asp:Button ID="btnConfirmMergeDuplicate" runat="server" Text="Yes, merge orders"
                                    OnClick="btnConfirmMergeDuplicate_Click" CssClass="filter-panel-btn"
                                    CausesValidation="false"
                                    ToolTip="Move lines from the other order into this one" />
                                <asp:Button ID="btnOpenDuplicateOrder" runat="server" Text="Open the other order"
                                    OnClick="btnOpenDuplicateOrder_Click" CssClass="filter-panel-btn"
                                    CausesValidation="false" />
                                <asp:Button ID="btnDismissMergePrompt" runat="server" Text="Not now"
                                    OnClick="btnDismissMergePrompt_Click" CssClass="filter-panel-btn"
                                    CausesValidation="false"
                                    ToolTip="Keep working on this order — Merge stays available in the footer" />
                            </asp:Panel>
                        </asp:Panel>
                    </div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>

        <div class="responsive-layout-container">
        <div class="layout-main-panel">
            <asp:UpdatePanel ID="pnlOrderHeader" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <table class="TblSimple detail-form-table">
                        <tr>
                            <td>
                                <asp:HyperLink runat="server" Text="Contact" ID="hlContactHdr" NavigateUrl="#" />
                            </td>
                            <td>
                                <ajaxToolkit:ComboBox ID="cboContacts" runat="server"
                                    AutoPostBack="true"
                                    DropDownStyle="DropDown" CaseSensitive="false"
                                    AutoCompleteMode="Suggest" PromptText="----Select name----"
                                    OnSelectedIndexChanged="cboContacts_SelectedIndexChanged">
                                    <asp:ListItem Value="0">none</asp:ListItem>
                                </ajaxToolkit:ComboBox>
                                <asp:HiddenField ID="hdnSelectedContactId" runat="server" Value="" />
                            </td>
                        </tr>
                        <tr>
                            <td>Order Date</td>
                            <td>
                                <asp:TextBox ID="tbxOrderDate" runat="server" />
                                <ajaxToolkit:CalendarExtender ID="tbxOrderDate_CalendarExtender" runat="server"
                                    Enabled="True" TargetControlID="tbxOrderDate"
                                    OnClientDateSelectionChanged="orderHeaderCalendarDateChanged" />
                            </td>
                        </tr>
                        <tr>
                            <td>Prep Date</td>
                            <td>
                                <asp:TextBox ID="tbxPrepDate" runat="server" />
                                <ajaxToolkit:CalendarExtender ID="tbxPrepDate_CalendarExtender" runat="server"
                                    Enabled="True" TargetControlID="tbxPrepDate"
                                    OnClientDateSelectionChanged="orderHeaderCalendarDateChanged" />
                            </td>
                        </tr>
                        <tr>
                            <td>Delivery By</td>
                            <td>
                                <asp:DropDownList ID="ddlToBeDeliveredBy" runat="server"
                                    DataTextField="Abbreviation" DataValueField="PersonID"
                                    AppendDataBoundItems="true">
                                    <asp:ListItem Value="0">n/a</asp:ListItem>
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td>Required By</td>
                            <td>
                                <asp:TextBox ID="tbxRequiredByDate" runat="server" />
                                <ajaxToolkit:CalendarExtender ID="tbxRequiredByDate_CalendarExtender" runat="server"
                                    Enabled="True" TargetControlID="tbxRequiredByDate"
                                    OnClientDateSelectionChanged="orderHeaderCalendarDateChanged" />
                            </td>
                        </tr>
                        <tr>
                            <td>P/Order</td>
                            <td>
                                <asp:TextBox ID="tbxPurchaseOrder" runat="server" Width="20em" />
                            </td>
                        </tr>
                        <tr>
                            <td>Stati</td>
                            <td class="status-flags">
                                <span class="status-flag">
                                    <asp:CheckBox ID="cbxConfirmed" TextAlign="Right" Text="Confirmed" runat="server"
                                        Checked="true" />
                                </span>
                                <span class="status-flag">
                                    <asp:CheckBox ID="cbxInvoiceDone" TextAlign="Right" Text="Invoiced" runat="server" />
                                </span>
                                <span class="status-flag">
                                    <asp:CheckBox ID="cbxDone" TextAlign="Right" Text="Done" runat="server"
                                        Enabled="false" />
                                </span>
                            </td>
                        </tr>
                        <tr id="trWaybill" runat="server" visible="false">
                            <td>Waybill</td>
                            <td>
                                <asp:Label ID="lblDispatchStatus" runat="server" CssClass="status-flag" style="margin-right: 8px;" />
                                <asp:Label ID="lblWaybill" runat="server" Font-Bold="true" />
                            </td>
                        </tr>
                        <tr>
                            <td>Notes:</td>
                            <td>
                                <asp:TextBox ID="tbxNotes" runat="server" TextMode="MultiLine" Height="4em"
                                    Width="98%"
                                    ToolTip="For ZZName / sundry orders, enter notes to enable New Item" />
                            </td>
                        </tr>
                    </table>
                    <asp:HiddenField ID="hdnHeaderDirty" runat="server" Value="0" />
                    <div class="order-detail-toolbar button-row">
                        <asp:Button ID="btnSaveHeader" runat="server" Text="Save"
                            CssClass="filter-panel-btn order-detail-btn order-detail-save-btn"
                            OnClick="btnSaveHeader_Click" CausesValidation="false"
                            ToolTip="Save your changes to this order" Enabled="false" />
                        <asp:Button ID="btnSaveAndReturn" runat="server" Text="Save &amp; Return"
                            CssClass="filter-panel-btn order-detail-btn order-detail-save-btn"
                            OnClientClick="return window.orderHeaderPrepareReturnNavigation ? window.orderHeaderPrepareReturnNavigation() : true;"
                            OnClick="btnSaveAndReturn_Click" CausesValidation="false"
                            ToolTip="Save your changes and return to the previous page" Enabled="false" />
                        <button id="btnUndoHeader" runat="server" type="submit"
                            class="btn btn-undo order-detail-btn order-detail-btn-icon"
                            onserverclick="btnUndoHeader_Click" causesvalidation="false"
                            title="Undo the last saved change to this order" style="display:none;">
                            <asp:Image ID="imgUndoHeader" runat="server" ImageUrl="~/images/imgButtons/Undo.png" AlternateText="Undo last saved change" />
                        </button>
                        <asp:Button ID="btnLastOrder" runat="server" Text="Last Order"
                            CssClass="filter-panel-btn order-detail-btn order-detail-last-btn" OnClick="btnLastOrder_Click"
                            ToolTip="Load items from this contact's previous order" Visible="false" />
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="cboContacts" EventName="SelectedIndexChanged" />
                    <asp:AsyncPostBackTrigger ControlID="btnLastOrder" EventName="Click" />
                    <asp:AsyncPostBackTrigger ControlID="btnSaveHeader" EventName="Click" />
                    <asp:AsyncPostBackTrigger ControlID="btnSaveAndReturn" EventName="Click" />
                </Triggers>
            </asp:UpdatePanel>
        </div>

        <div class="layout-detail-panel">
            <div class="layout-panel-top">
                <asp:UpdatePanel ID="upnlOrderLines" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <asp:GridView ID="gvOrderLines" runat="server" AutoGenerateColumns="False"
                            CssClass="results-table in-panel-grid no-sticky-last"
                            DataKeyNames="OrderLineID"
                            UseAccessibleHeader="true"
                            OnRowUpdated="gvOrderLines_RowUpdated" OnRowCommand="gvOrderLines_RowCommand"
                            OnRowEditing="gvOrderLines_RowEditing" OnRowCancelingEdit="gvOrderLines_RowCancelingEdit"
                            OnRowDataBound="gvOrderLines_RowDataBound" OnRowUpdating="gvOrderLines_RowUpdating"
                            ShowHeaderWhenEmpty="false">
                            <Columns>
                                <asp:TemplateField HeaderText="&nbsp;" HeaderStyle-CssClass="col-cmd" ItemStyle-CssClass="col-cmd">
                                    <ItemTemplate>
                                        <asp:ImageButton ID="btnEditLine" runat="server" CommandName="Edit" CausesValidation="false"
                                            ImageUrl="~/images/imgButtons/EditItem.gif" AlternateText="Edit"
                                            ToolTip="Edit this line" />
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:ImageButton ID="btnUpdateLine" runat="server" CommandName="Update" CausesValidation="false"
                                            ImageUrl="~/images/imgButtons/UpdateItem.gif" AlternateText="Save"
                                            ToolTip="Save line" />
                                        <asp:ImageButton ID="btnCancelLine" runat="server" CommandName="Cancel" CausesValidation="false"
                                            ImageUrl="~/images/imgButtons/CancelItem.gif" AlternateText="Undo"
                                            ToolTip="Cancel edit" />
                                    </EditItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Item" SortExpression="ItemTypeID">
                                    <EditItemTemplate>
                                        <ajaxToolkit:ComboBox ID="cboItemDesc" runat="server"
                                            DataTextField="ItemDesc" DataValueField="ItemTypeID" DropDownStyle="DropDown"
                                            CaseSensitive="false" AutoCompleteMode="SuggestAppend" PromptText="--Select item--"
                                            AppendDataBoundItems="true">
                                            <asp:ListItem Text="--Invalid Item--" Value="0" />
                                        </ajaxToolkit:ComboBox>
                                        <asp:HiddenField ID="hdnItemTypeID" runat="server" Value='<%# Eval("ItemTypeID") %>' />
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="lblItemDesc" runat="server" Text='<%# GetItemDescById(Convert.ToInt32(Eval("ItemTypeID"))) %>' />
                                        <asp:HiddenField ID="hdnItemTypeID" runat="server" Value='<%# Bind("ItemTypeID") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="QTY" SortExpression="QuantityOrdered"
                                    HeaderStyle-CssClass="col-tight" ItemStyle-CssClass="col-tight">
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
                                            PromptText="--Select item--" AppendDataBoundItems="true">
                                            <asp:ListItem Text="n/a" Value="0" />
                                        </ajaxToolkit:ComboBox>
                                        <asp:HiddenField ID="hdnPackagingID" runat="server" Value='<%# Eval("PackagingID") %>' />
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="lblPackagingDesc" runat="server" Text='<%# GetPackagingDesc(Convert.ToInt32(Eval("PackagingID"))) %>' />
                                        <asp:HiddenField ID="hdnPackagingID" runat="server" Value='<%# Eval("PackagingID") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderStyle-CssClass="col-cmd" ItemStyle-CssClass="col-cmd">
                                    <ItemTemplate>
                                        <asp:ImageButton ID="MoveOneDayOnImageButton" AlternateText="+date" CommandName="MoveOneDayOn"
                                            ImageUrl="~/images/imgButtons/MoveOnADay.gif" runat="server"
                                            CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                                            ToolTip="Reschedule this line to the next delivery day (prep unchanged)" />
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
                                <div class="empty-data">
                                    Please add items to the order.
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
                        <asp:AsyncPostBackTrigger ControlID="btnUseExistingOrder" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="btnCreateNewOrderAnyway" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="btnDismissConflict" EventName="Click" />
                    </Triggers>
                    <ContentTemplate>
                        <asp:Button ID="btnNewItem" Text="New Item" runat="server" CssClass="filter-panel-btn"
                            Enabled="false" OnClick="btnNewItem_Click"
                            ToolTip="Add a new line item" />
                        <asp:Panel ID="pnlNewItem" runat="server" CssClass="new-order-item-form" Visible="false">
                            <table class="TblFlex new-order-item-table" cellpadding="0" cellspacing="0">
                                <thead>
                                    <tr>
                                        <th>Item</th>
                                        <th>Qty</th>
                                        <th>Prep</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr>
                                        <td data-label="Item">
                                            <ajaxToolkit:ComboBox ID="cboNewItemDesc" runat="server"
                                                DataTextField="ItemDesc" DataValueField="ItemTypeID" DropDownStyle="DropDown"
                                                CaseSensitive="false" AutoCompleteMode="SuggestAppend" PromptText="--Select item--" />
                                        </td>
                                        <td data-label="Qty">
                                            <asp:TextBox ID="tbxNewQuantityOrdered" runat="server" Text="1" Width="4em" />
                                        </td>
                                        <td data-label="Prep">
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
                            <div class="new-order-item-actions button-row">
                                <asp:Button ID="btnAdd" Text="Add" runat="server" CssClass="filter-panel-btn" OnClick="btnAdd_Click" />
                                <asp:Button ID="btnCancel" Text="Cancel" runat="server" CssClass="filter-panel-btn" OnClick="btnCancel_Click" />
                            </div>
                        </asp:Panel>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
        <div class="layout-footer-panel">
            <asp:UpdatePanel ID="updtButtonPanel" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
                <Triggers>
                    <asp:PostBackTrigger ControlID="btnNewOrder" />
                    <asp:PostBackTrigger ControlID="btnDlSheet" />
                    <asp:PostBackTrigger ControlID="btnOrderCancelled" />
                    <asp:PostBackTrigger ControlID="btnOrderDelivered" />
                    <asp:PostBackTrigger ControlID="btnBack" />
                </Triggers>
                <ContentTemplate>
                    <div class="button-row order-detail-actions" style="margin-top: 8px;">
                        <asp:Button ID="btnNewOrder" runat="server" Text="New Order" AccessKey="N"
                            CssClass="filter-panel-btn"
                            OnClick="btnNewOrder_Click"
                            CausesValidation="false"
                            ToolTip="new order (AltShftN)" />
                        <asp:Button ID="btnConfirmOrder" runat="server" Text="Email Confirmation" AccessKey="E"
                            CssClass="filter-panel-btn"
                            OnClick="btnConfirmOrder_Click" ToolTip="send Email confirmation (AltShftE)" />
                        <asp:Button ID="btnMerge" runat="server" Text="Merge" Visible="false"
                            CssClass="filter-panel-btn"
                            OnClick="btnMerge_Click" CausesValidation="false"
                            ToolTip="Merge another order for this contact and delivery date into this order" />
                        <asp:Button ID="btnDlSheet" runat="server" Text="Delivery Sheet"
                            CssClass="filter-panel-btn"
                            PostBackUrl="~/Pages/DeliverySheet.aspx" AccessKey="S" ToolTip="delivery sheet (AltShftS)" />
                        <asp:Button ID="btnOrderCancelled" runat="server" OnClick="btnCancelled_Click" Text="Cancel Order"
                            CssClass="filter-panel-btn"
                            ToolTip="cancel this order" />
                        <asp:Button ID="btnUnDoDone" runat="server" Text="UnDo Done" AccessKey="U"
                            CssClass="filter-panel-btn"
                            OnClick="btnUnDoDone_Click" ToolTip="undo a done order (AltShftU)" />
                        <asp:Button ID="btnOrderDelivered" runat="server" Text="Order Done" AccessKey="D"
                            CssClass="filter-panel-btn"
                            OnClick="btnOrderDelivered_Click"
                            OnClientClick="return (window.TrackerUnsaved && TrackerUnsaved.confirmSaveThenContinue) ? TrackerUnsaved.confirmSaveThenContinue('You have unsaved changes. Save them and continue to Order Done?') : true;"
                            ToolTip="start order done process (AltShftD)" />
                        <span class="image-button" title="Return without saving">
                            <asp:ImageButton ID="btnBack" runat="server"
                                ImageUrl="~/images/imgButtons/Back.gif"
                                AlternateText="Back"
                                ToolTip="Return without saving"
                                OnClick="btnBack_Click"
                                CausesValidation="false"
                                OnClientClick="return (window.TrackerUnsaved && TrackerUnsaved.confirmLeave) ? TrackerUnsaved.confirmLeave() : true;" />
                        </span>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div class="layout-status-panel">
            <asp:UpdatePanel ID="upnlStatus" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
                <ContentTemplate>
                    <asp:Panel ID="pnlStatusMessage" runat="server" CssClass="status-message" style="margin-top: 12px;">
                        <asp:Literal ID="ltrlStatus" Text="" runat="server" />
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
    </div>
    </asp:Panel>

    <%-- Init in MainContent (not Head) so <%= ClientID %> does not block ScriptManager --%>
    <script type="text/javascript">
        // CalendarExtender sets the textbox without firing input/change — mark header dirty explicitly.
        function orderHeaderCalendarDateChanged(sender, args) {
            if (window.TrackerUnsaved && TrackerUnsaved.markDirty) {
                TrackerUnsaved.markDirty();
            } else if (window.orderHeaderMarkDirty) {
                orderHeaderMarkDirty();
            }
        }

        // Shared dirty guard (Scripts/unsavedChanges.js via Site.Master)
        TrackerUnsaved.init({
            dirtyFieldId: '<%= hdnHeaderDirty.ClientID %>',
            rootId: '<%= pnlOrderHeader.ClientID %>',
            unsavedStatusMessage: '<%= HttpUtility.JavaScriptStringEncode(HeaderUnsavedStatusMessage) %>',
            statusLiteralId: '<%= ltrlStatus.ClientID %>',
            statusPanelId: '<%= pnlStatusMessage.ClientID %>',
            saveButtonSelector: '.order-detail-save-btn',
            leaveMessage: 'You have unsaved changes. Leave without saving?',
            saveThenContinueMessage: 'You have unsaved changes. Save them and continue to Order Done?',
            aliases: {
                markDirty: 'orderHeaderMarkDirty',
                clearDirty: 'orderHeaderClearDirty',
                markDirtyFromServer: 'orderHeaderMarkDirtyFromServer',
                wireFields: 'orderHeaderWireFields',
                allowNavigate: 'orderHeaderAllowNavigateFn',
                confirmLeave: 'orderHeaderConfirmLeave',
                confirmSaveThenContinue: 'orderHeaderConfirmSaveThenContinue'
            }
        });

        // ZZName / sundry: enable New Item as soon as Notes length > 1 (no wait for postback).
        (function () {
            var sundryId = '<%= SystemConstants.CustomerConstants.SundryCustomerIDStr %>';
            var sundryPrefix = '<%= SystemConstants.CustomerConstants.SundryCustomerNamePrefix %>';
            var notesId = '<%= tbxNotes.ClientID %>';
            var contactHiddenId = '<%= hdnSelectedContactId.ClientID %>';
            var newItemBtnId = '<%= btnNewItem.ClientID %>';
            var contactsComboId = '<%= cboContacts.ClientID %>';

            function byId(id) {
                return id ? document.getElementById(id) : null;
            }

            function getContactId() {
                var h = byId(contactHiddenId);
                if (h && h.value && parseInt(h.value, 10) > 0)
                    return String(parseInt(h.value, 10));

                // Ajax ComboBox posts as _HiddenField / select — try common siblings
                var combo = byId(contactsComboId);
                if (combo) {
                    if (combo.tagName === 'SELECT' && combo.value && combo.value !== '0')
                        return String(combo.value);
                    var hidden = document.getElementById(contactsComboId + '_HiddenField');
                    if (hidden && hidden.value && hidden.value !== '0')
                        return String(hidden.value);
                }
                return '';
            }

            function getContactName() {
                var combo = byId(contactsComboId);
                if (!combo) return '';
                if (combo.tagName === 'SELECT' && combo.selectedIndex >= 0)
                    return (combo.options[combo.selectedIndex].text || '').trim();
                if (combo.tagName === 'INPUT')
                    return (combo.value || '').trim();
                var input = document.getElementById(contactsComboId + '_TextBox')
                    || (combo.querySelector && combo.querySelector('input[type="text"]'));
                return input ? (input.value || '').trim() : '';
            }

            function isSundryContact() {
                var id = getContactId();
                if (id === sundryId) return true;
                var name = getContactName();
                return name.toUpperCase().indexOf(sundryPrefix.toUpperCase()) === 0;
            }

            function notesOk() {
                var notes = byId(notesId);
                if (!notes) return false;
                return (notes.value || '').trim().length > 1;
            }

            function hasContact() {
                var id = getContactId();
                if (id && id !== '0') return true;
                var name = getContactName();
                return !!(name && name.toLowerCase() !== 'none' && name.indexOf('Select') < 0);
            }

            function setNewItemEnabled(enabled) {
                var btn = byId(newItemBtnId);
                if (!btn) return;
                btn.disabled = !enabled;
                if (enabled) {
                    btn.removeAttribute('disabled');
                    btn.classList.remove('aspNetDisabled');
                    btn.style.opacity = '';
                    btn.style.pointerEvents = '';
                } else {
                    btn.setAttribute('disabled', 'disabled');
                    btn.classList.add('aspNetDisabled');
                    btn.style.opacity = '0.55';
                    btn.style.pointerEvents = 'none';
                }
            }

            function syncNewItemButton() {
                if (!hasContact()) {
                    setNewItemEnabled(false);
                    return;
                }
                if (isSundryContact()) {
                    setNewItemEnabled(notesOk());
                    return;
                }
                setNewItemEnabled(true);
            }

            window.orderDetailSyncNewItemButton = syncNewItemButton;

            function wire() {
                var notes = byId(notesId);
                if (notes) {
                    ['input', 'keyup', 'change', 'blur', 'paste'].forEach(function (evt) {
                        notes.addEventListener(evt, function () {
                            window.setTimeout(syncNewItemButton, 0);
                        });
                    });
                }
                syncNewItemButton();
            }

            function onReady(fn) {
                if (document.readyState === 'loading')
                    document.addEventListener('DOMContentLoaded', fn);
                else
                    fn();
            }

            onReady(function () {
                wire();
                if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                    var prm = Sys.WebForms.PageRequestManager.getInstance();
                    prm.add_endRequest(function () {
                        wire();
                        syncNewItemButton();
                    });
                }
            });
        })();

        // OrderDetail-only: Save & Return overlay / redirect (uses TrackerUnsaved for leave guard)
        (function () {
            var saveReturnBtnId = '<%= btnSaveAndReturn.ClientID %>';
            var saveReturnPending = false;
            var saveReturnSafetyTimer = null;

            function suppressBeforeUnload() {
                TrackerUnsaved.allowNavigate();
                window.orderHeaderAllowNavigate = true;
            }

            window.orderHeaderPrepareReturnNavigation = function () {
                saveReturnPending = true;
                suppressBeforeUnload();
                scheduleSaveReturnSafetyCheck();
                return true;
            };

            function showSaveReturnOverlay() {
                var overlay = document.getElementById('orderDetailSaveReturnOverlay');
                if (overlay) {
                    overlay.style.display = 'flex';
                }
            }

            function hideSaveReturnOverlay() {
                var overlay = document.getElementById('orderDetailSaveReturnOverlay');
                if (overlay) {
                    overlay.style.display = 'none';
                }
                saveReturnPending = false;
                clearSaveReturnSafetyCheck();
            }

            function clearSaveReturnSafetyCheck() {
                if (saveReturnSafetyTimer) {
                    clearTimeout(saveReturnSafetyTimer);
                    saveReturnSafetyTimer = null;
                }
            }

            function scheduleSaveReturnSafetyCheck() {
                clearSaveReturnSafetyCheck();
                saveReturnSafetyTimer = setTimeout(function () {
                    var prm = window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager
                        ? Sys.WebForms.PageRequestManager.getInstance()
                        : null;
                    if (saveReturnPending && (!prm || !prm.get_isInAsyncPostBack())) {
                        hideSaveReturnOverlay();
                    }
                }, 10000);
            }

            function tryRedirectAfterSaveAndReturn() {
                var redirectField = document.getElementById('<%= hdnSaveReturnRedirectUrl.ClientID %>');
                if (!redirectField) {
                    return false;
                }

                var redirectUrl = (redirectField.value || '').trim();
                if (!redirectUrl) {
                    return false;
                }

                redirectField.value = '';
                suppressBeforeUnload();
                showSaveReturnOverlay();
                window.location.replace(redirectUrl);
                return true;
            }

            function wireSaveAndReturnNavigation() {
                var saveReturnBtn = document.getElementById(saveReturnBtnId);
                if (!saveReturnBtn || saveReturnBtn.getAttribute('data-return-wired') === '1') {
                    return;
                }

                saveReturnBtn.setAttribute('data-return-wired', '1');
                saveReturnBtn.addEventListener('mousedown', function () {
                    saveReturnPending = true;
                    suppressBeforeUnload();
                    scheduleSaveReturnSafetyCheck();
                }, true);
            }

            function initSaveReturnAsyncHandlers() {
                if (!window.Sys || !Sys.WebForms || !Sys.WebForms.PageRequestManager) {
                    return false;
                }

                var prm = Sys.WebForms.PageRequestManager.getInstance();
                if (prm.get_isInAsyncPostBack()) {
                    return true;
                }

                if (prm._orderDetailSaveReturnHandlersAttached) {
                    return true;
                }

                prm._orderDetailSaveReturnHandlersAttached = true;

                prm.add_beginRequest(function (sender, args) {
                    var postBackElement = args.get_postBackElement();
                    if (postBackElement && postBackElement.id === saveReturnBtnId) {
                        saveReturnPending = true;
                        suppressBeforeUnload();
                        showSaveReturnOverlay();
                    }
                });

                prm.add_endRequest(function () {
                    if (tryRedirectAfterSaveAndReturn()) {
                        return;
                    }

                    if (saveReturnPending) {
                        hideSaveReturnOverlay();
                    }

                    TrackerUnsaved.syncFromHidden();
                    if (window.orderHeaderTrackedFieldIds) {
                        TrackerUnsaved.wireFields(window.orderHeaderTrackedFieldIds);
                    }
                    if (TrackerUnsaved.isDirty()) {
                        TrackerUnsaved.markDirty();
                        TrackerUnsaved.restoreGuard();
                    }
                    window.orderHeaderAllowNavigate = false;
                    wireSaveAndReturnNavigation();
                });

                return true;
            }

            function ensureSaveReturnAsyncHandlers() {
                if (!initSaveReturnAsyncHandlers()) {
                    window.setTimeout(ensureSaveReturnAsyncHandlers, 50);
                }
            }

            function start() {
                window.orderHeaderAllowNavigate = false;
                wireSaveAndReturnNavigation();
                ensureSaveReturnAsyncHandlers();
            }

            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', start);
            } else {
                start();
            }
        })();
    </script>
</asp:Content>
