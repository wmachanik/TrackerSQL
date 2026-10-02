<%@ Page Title="Delivery Sheet" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="DeliverySheet.aspx.cs" Inherits="TrackerSQL.Pages.DeliverySheet" %>

<asp:Content ID="cntDeliveryHdr" title="Delivery Sheet" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntDeliveryBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smDelivery" runat="server" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="uprgDelivery" runat="server"
        AssociatedUpdatePanelID="upnlDeliveryItems" DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info page-tone-progress">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="upnlDeliveryItems" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="tbCalendarDate" EventName="TextChanged" />
            <asp:AsyncPostBackTrigger ControlID="ddlActivePrepDates" EventName="SelectedIndexChanged" />
            <asp:AsyncPostBackTrigger ControlID="btnGo" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnFind" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnFindDefault" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="ddlDeliveryBy" EventName="SelectedIndexChanged" />
            <asp:PostBackTrigger ControlID="btnPrint" />
            <asp:PostBackTrigger ControlID="btnRefresh" />
            <asp:PostBackTrigger ControlID="btnBack" />
        </Triggers>
        <ContentTemplate>
            <asp:Panel ID="pnlDeliveryShell" runat="server" CssClass="simpleForm page-tone-panel page-tone-delivery">
                <asp:Panel ID="pnlDeliveryDate" runat="server">
                    <div class="page-tone-header tool-card-header">
                        <img class="tool-card-icon" src="../images/imgButtons/icons8-delivery-16.png" alt="" />
                        <div>
                            <h1 class="page-tone-title">Delivery Sheet</h1>
                            <p class="page-tone-subtitle">Manage delivery schedules</p>
                        </div>
                    </div>

                    <div class="page-tone-toolbar filter-toolbar">
                        <div class="filter-section search-controls">
                            <asp:Label runat="server" Text="Delivery Date:" AssociatedControlID="ddlActivePrepDates" CssClass="small" />
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
                            <span class="image-button" title="Pick a delivery date">
                                <asp:ImageButton ID="btnCalendar" runat="server" ImageUrl="~/images/imgButtons/CalendarBtn.png"
                                    AlternateText="Calendar" ToolTip="Pick a delivery date"
                                    CausesValidation="false" OnClientClick="return false;" />
                                <asp:TextBox ID="tbCalendarDate" runat="server"
                                    Style="width: 0; height: 0; border: none; padding: 0; margin: 0; opacity: 0; position: absolute; left: 0; top: 100%;"
                                    AutoPostBack="true" OnTextChanged="tbCalendarDate_TextChanged" />
                                <ajaxToolkit:CalendarExtender
                                    ID="calExtender"
                                    runat="server"
                                    TargetControlID="tbCalendarDate"
                                    PopupButtonID="btnCalendar"
                                    PopupPosition="BottomLeft"
                                    Format="yyyy-MM-dd" />
                            </span>
                            <asp:Button ID="btnRefresh" CssClass="filter-panel-btn" Text="Refresh" AccessKey="R"
                                ToolTip="refresh lists (AltShftR)" runat="server" OnClick="btnRefresh_Click" />
                            <asp:Label ID="lblDeliveryBy" runat="server" Text="By:" Visible="false" />
                            <asp:DropDownList ID="ddlDeliveryBy" runat="server" AutoPostBack="true" Visible="false"
                                OnSelectedIndexChanged="ddlDeliveryBy_SelectedIndexChanged" />
                        </div>
                        <div class="filter-section admin-controls">
                            <asp:Panel ID="pnlFindContact" runat="server" DefaultButton="btnFindDefault" CssClass="delivery-find-panel">
                                <asp:Label runat="server" Text="To:" AssociatedControlID="tbxFindClient" CssClass="small" />
                                <asp:TextBox ID="tbxFindClient" runat="server" />
                                <asp:Button ID="btnFindDefault" runat="server"
                                    Text="Find"
                                    OnClick="btnFindDefault_Click"
                                    CausesValidation="false"
                                    TabIndex="-1"
                                    CssClass="delivery-find-default-btn" />
                                <span class="toolbar-icon-row">
                                    <span class="image-button" title="Find contact on open delivery days">
                                        <asp:ImageButton ID="btnFind" runat="server"
                                            ImageUrl="~/images/imgButtons/Find.gif"
                                            AlternateText="Find"
                                            ToolTip="Find company or ZZName on open delivery days"
                                            OnClick="btnFind_Click"
                                            CausesValidation="false" />
                                    </span>
                                </span>
                            </asp:Panel>
                            <span class="toolbar-icon-row">
                                <span class="image-button hideWhenPrinting" title="Print sheet (Alt+Shift+P)">
                                    <asp:ImageButton ID="btnPrint" runat="server"
                                        ImageUrl="~/images/imgButtons/Print.gif"
                                        AlternateText="Print"
                                        ToolTip="Print sheet (Alt+Shift+P)"
                                        OnClick="btnPrint_Click"
                                        AccessKey="P"
                                        CausesValidation="false" />
                                </span>
                                <asp:HyperLink ID="hlAddDeliveryItem" runat="server"
                                    ImageUrl="~/images/imgButtons/Add-Order.png"
                                    ToolTip="Add order"
                                    NavigateUrl="~/Pages/OrderDetail.aspx?NewOrder=true" />
                                <span class="image-button" title="Return to home">
                                    <asp:ImageButton ID="btnBack" runat="server"
                                        ImageUrl="~/images/imgButtons/Back.gif"
                                        AlternateText="Back"
                                        ToolTip="Return to home"
                                        OnClick="btnBack_Click"
                                        CausesValidation="false" />
                                </span>
                            </span>
                        </div>
                    </div>
                </asp:Panel>

                <div class="dlv-route-info small">
                    <asp:Literal ID="ltrlRouteInfo" runat="server" EnableViewState="false" />
                </div>
                <div id="dlvOrderBar" class="dlv-order-bar" hidden="hidden">
                    <span id="dlvOrderText"></span>
                    <button type="button" class="filter-panel-btn" onclick="dlvSaveOrder();">Save order</button>
                    <button type="button" class="filter-panel-btn" onclick="dlvCancelOrder();">Cancel</button>
                </div>
                <asp:HiddenField ID="hdnRouteOrder" runat="server" />
                <asp:Button ID="btnSaveRouteOrder" runat="server" Text="Save order" Style="display: none"
                    OnClick="btnSaveRouteOrder_Click" CausesValidation="false" UseSubmitBehavior="false" />

                <asp:Table ID="tblDeliveries" runat="server" EnableViewState="false" CssClass="TblZebra" Width="100%" CellPadding="0">
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

                <div class="status-message" id="pnlStatus" runat="server" style="margin-top: 12px;" visible="false">
                    <asp:Literal ID="ltrlStatus" runat="server" />
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>

    <script type="text/javascript">
        // Drag-to-order: rows carry data-person / data-contact; only a driver's own open stops move among each other.
        (function () {
            var dragRow = null, changed = null, start = null;

            function rowOf(el) { return el && el.closest ? el.closest('tr[data-person]') : null; }
            function table() { return document.getElementById('<%= tblDeliveries.ClientID %>'); }
            function rowsOf(person) {
                var t = table();
                return t ? Array.prototype.slice.call(t.querySelectorAll('tr[data-person="' + person + '"]')) : [];
            }
            function contactsOf(person) {
                var seen = [];
                rowsOf(person).forEach(function (r) {
                    var c = parseInt(r.getAttribute('data-contact'), 10);
                    if (seen.indexOf(c) < 0) seen.push(c);
                });
                return seen;
            }
            function bar(show, text) {
                var b = document.getElementById('dlvOrderBar');
                if (!b) return;
                b.hidden = !show;
                document.getElementById('dlvOrderText').textContent = text || '';
            }
            function post(request) {
                document.getElementById('<%= hdnRouteOrder.ClientID %>').value = JSON.stringify(request);
                document.getElementById('<%= btnSaveRouteOrder.ClientID %>').click();
            }

            document.addEventListener('mousedown', function (e) {
                var handle = e.target.closest ? e.target.closest('.dlv-handle') : null;
                var tr = rowOf(handle);
                if (tr) tr.setAttribute('draggable', 'true');
            });
            document.addEventListener('mouseup', function (e) {
                var tr = rowOf(e.target);
                if (tr && !dragRow) tr.removeAttribute('draggable');
            });
            document.addEventListener('dragstart', function (e) {
                var tr = rowOf(e.target);
                if (!tr || tr.getAttribute('draggable') !== 'true') return;
                var person = tr.getAttribute('data-person');
                if (changed && changed !== person) {
                    e.preventDefault();
                    alert('Save or cancel the order you changed first.');
                    return;
                }
                if (!changed) {
                    var rows = rowsOf(person);
                    start = { person: person, rows: rows, after: rows[rows.length - 1].nextSibling, contacts: contactsOf(person).join(',') };
                }
                dragRow = tr;
                tr.classList.add('dlv-dragging');
                e.dataTransfer.effectAllowed = 'move';
                try { e.dataTransfer.setData('text/plain', tr.getAttribute('data-order')); } catch (x) { }
            });
            document.addEventListener('dragover', function (e) {
                if (!dragRow) return;
                var tr = rowOf(e.target);
                if (!tr || tr.getAttribute('data-person') !== dragRow.getAttribute('data-person')) return;
                e.preventDefault();
                e.dataTransfer.dropEffect = 'move';
                if (tr === dragRow) return;
                var box = tr.getBoundingClientRect();
                tr.parentNode.insertBefore(dragRow, e.clientY > box.top + box.height / 2 ? tr.nextSibling : tr);
            });
            document.addEventListener('drop', function (e) { if (dragRow) e.preventDefault(); });
            document.addEventListener('dragend', function () {
                if (!dragRow) return;
                var person = dragRow.getAttribute('data-person');
                var name = dragRow.getAttribute('data-driver') || 'this driver';
                dragRow.classList.remove('dlv-dragging');
                dragRow.removeAttribute('draggable');
                dragRow = null;
                if (start && contactsOf(person).join(',') !== start.contacts) {
                    changed = person;
                    bar(true, 'You changed the order of ' + name + "'s stops.");
                } else {
                    changed = null;
                    bar(false);
                }
            });

            window.dlvSaveOrder = function () {
                if (!changed) return;
                var t = table();
                var plans = {};
                try { plans = JSON.parse(t.getAttribute('data-driver-plans') || '{}'); } catch (x) { }
                if (plans[changed] && !confirm(plans[changed] + "\n\nThe driver's own order normally comes first. Only change it when you need to, "
                    + 'for example when a client asked to be delivered first.\n\nSave your order anyway?')) return;
                post({ day: t.getAttribute('data-day'), person: parseInt(changed, 10), contacts: contactsOf(changed) });
            };
            window.dlvCancelOrder = function () {
                if (start) start.rows.forEach(function (r) { start.after ? start.after.parentNode.insertBefore(r, start.after) : r.parentNode.appendChild(r); });
                changed = null;
                start = null;
                bar(false);
            };
            window.dlvSuggested = function (person) {
                var t = table();
                if (!t || !confirm('Go back to the suggested order for this driver (learned and GPS route)?')) return false;
                post({ day: t.getAttribute('data-day'), person: person, reset: true });
                return false;
            };

            if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () { changed = null; start = null; });
            }
        })();
    </script>
</asp:Content>
