<%@ Page Title="WooCommerce Mapping" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="WooCommerceMapping.aspx.cs" Inherits="TrackerSQL.Tools.WooCommerceMapping" %>

<asp:Content ID="cntWooMapHdr" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        window.wooMapDirty = false;

        window.wooMapSetDisabled = function (btn, disabled) {
            if (!btn) return;
            if (disabled) {
                btn.setAttribute('disabled', 'disabled');
                btn.disabled = true;
            } else {
                btn.removeAttribute('disabled');
                btn.disabled = false;
            }
        };

        window.wooMapHasVisibleCatDirty = function () {
            var grid = document.getElementById('<%= gvCategories.ClientID %>');
            if (!grid) return false;
            var checks = grid.querySelectorAll('input[type="checkbox"][data-original]');
            for (var i = 0; i < checks.length; i++) {
                var original = checks[i].getAttribute('data-original') === '1';
                if ((!!checks[i].checked) !== original) return true;
            }
            return false;
        };

        window.wooMapPendingCount = function () {
            var el = document.getElementById('<%= hdnPendingCount.ClientID %>');
            if (!el) return 0;
            var n = parseInt(el.value || '0', 10);
            return isNaN(n) ? 0 : n;
        };

        window.wooMapRefreshDirty = function () {
            var catDirty = window.wooMapHasVisibleCatDirty() || window.wooMapPendingCount() > 0;
            var mode = document.getElementById('<%= ddlCatMode.ClientID %>');
            var modeDirty = !!(mode && mode.getAttribute('data-original') != null
                && mode.value !== mode.getAttribute('data-original'));
            window.wooMapDirty = catDirty || modeDirty;

            var modeBtn = document.getElementById('<%= btnSaveCatMode.ClientID %>');
            if (modeBtn && mode)
                window.wooMapSetDisabled(modeBtn, !modeDirty);

            var saveIncludes = document.getElementById('<%= btnSaveCatIncludes.ClientID %>');
            window.wooMapSetDisabled(saveIncludes, !catDirty);
        };

        window.wooMapConfirmLeave = function () {
            window.wooMapRefreshDirty();
            if (!window.wooMapDirty) return true;
            var msgEl = document.getElementById('<%= hdnUnsavedLeave.ClientID %>');
            var msg = (msgEl && msgEl.value) ? msgEl.value : 'You have unsaved category changes. Leave anyway?';
            return window.confirm(msg);
        };

        window.wooMapConfirmPullCats = function () {
            if (!window.wooMapConfirmLeave()) return false;
            var msgEl = document.getElementById('<%= hdnPullCatsConfirm.ClientID %>');
            var msg = (msgEl && msgEl.value) ? msgEl.value
                : 'Pull will refresh category names/parents from Woo. Your Include ticks on existing categories are kept. New categories get defaults. Continue?';
            return window.confirm(msg);
        };

        window.addEventListener('beforeunload', function (e) {
            window.wooMapRefreshDirty();
            if (!window.wooMapDirty) return;
            e.preventDefault();
            e.returnValue = '';
        });

        document.addEventListener('change', function (e) {
            var t = e.target;
            if (!t || t.type !== 'checkbox') return;
            if (t.getAttribute('data-original') == null) return;
            var grid = document.getElementById('<%= gvCategories.ClientID %>');
            if (!grid || !grid.contains(t)) return;
            window.wooMapRefreshDirty();
        }, false);

        if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                window.wooMapRefreshDirty();
            });
        }
    </script>
</asp:Content>

<asp:Content ID="cntWooMapBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smWooMap" runat="server" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="upgWooMap" runat="server" AssociatedUpdatePanelID="upnlWooMap" DisplayAfter="0">
        <ProgressTemplate>
            <div class="status-message status-info page-tone-progress">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="upnlWooMap" runat="server" UpdateMode="Conditional">
        <Triggers>
            <asp:PostBackTrigger ControlID="btnBack" />
        </Triggers>
        <ContentTemplate>
            <asp:Panel ID="pnlAccessDenied" runat="server" Visible="false" CssClass="simpleForm page-tone-panel page-tone-sysdata">
                <asp:Label ID="lblAccessDenied" runat="server" CssClass="status-message status-error" />
            </asp:Panel>

            <asp:Panel ID="pnlMain" runat="server" CssClass="simpleForm page-tone-panel page-tone-sysdata">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/Toolbox.png" alt="" />
                    <div>
                        <h1 class="page-tone-title"><asp:Literal ID="litTitle" runat="server" /></h1>
                        <p class="page-tone-subtitle"><asp:Literal ID="litSubtitle" runat="server" /></p>
                    </div>
                </div>

                <div class="sys-prefs-tabs-wrap">
                    <nav class="sys-prefs-tabs" aria-label="Mapping sections">
                        <asp:LinkButton ID="btnTabCat" runat="server" CssClass="sys-prefs-tab" OnClick="btnTabCat_Click" CausesValidation="false" OnClientClick="return wooMapConfirmLeave();" />
                        <asp:LinkButton ID="btnTabMap" runat="server" CssClass="sys-prefs-tab" OnClick="btnTabMap_Click" CausesValidation="false" OnClientClick="return wooMapConfirmLeave();" />
                        <asp:LinkButton ID="btnTabWild" runat="server" CssClass="sys-prefs-tab" OnClick="btnTabWild_Click" CausesValidation="false" OnClientClick="return wooMapConfirmLeave();" />
                        <asp:LinkButton ID="btnTabSync" runat="server" CssClass="sys-prefs-tab" OnClick="btnTabSync_Click" CausesValidation="false" OnClientClick="return wooMapConfirmLeave();" />
                    </nav>
                </div>

                <asp:MultiView ID="mvTabs" runat="server" ActiveViewIndex="0">
                    <asp:View ID="viewCat" runat="server">
                        <p><asp:Literal ID="litCatHelp" runat="server" /></p>
                        <div class="button-row woo-map-cat-toolbar">
                            <asp:Literal ID="litCatModeLbl" runat="server" />
                            <asp:DropDownList ID="ddlCatMode" runat="server" onchange="wooMapRefreshDirty();">
                                <asp:ListItem Value="All" Text="All categories" />
                                <asp:ListItem Value="IncludeList" Text="Included only" />
                            </asp:DropDownList>
                            <asp:Button ID="btnSaveCatMode" runat="server" CssClass="filter-panel-btn" OnClick="btnSaveCatMode_Click" />
                            <asp:Button ID="btnSaveCatIncludes" runat="server" CssClass="filter-panel-btn" OnClick="btnSaveCatIncludes_Click" />
                            <asp:Button ID="btnPullCats" runat="server" CssClass="filter-panel-btn" OnClick="btnPullCats_Click" CausesValidation="false" OnClientClick="return wooMapConfirmPullCats();" />
                        </div>
                        <asp:HiddenField ID="hdnUnsavedLeave" runat="server" />
                        <asp:HiddenField ID="hdnPullCatsConfirm" runat="server" />
                        <asp:HiddenField ID="hdnPendingCount" runat="server" Value="0" />
                        <asp:Literal ID="litCatPageInfo" runat="server" />
                        <asp:GridView ID="gvCategories" runat="server" CssClass="results-table woo-map-cat-grid" AutoGenerateColumns="false"
                            DataKeyNames="FilterID" AllowPaging="true" PageSize="25"
                            PagerSettings-Mode="NumericFirstLast" PagerSettings-Position="Bottom"
                            PagerStyle-CssClass="woo-map-pager"
                            OnPageIndexChanging="gvCategories_PageIndexChanging"
                            OnRowDataBound="gvCategories_RowDataBound">
                            <Columns>
                                <asp:BoundField DataField="WooCategoryId" HeaderText="Woo ID" />
                                <asp:TemplateField HeaderText="Category">
                                    <ItemTemplate>
                                        <span class='<%# "woo-cat-name woo-cat-depth-" + Convert.ToInt32(Eval("Depth")) %>'>
                                            <%# Server.HtmlEncode(Convert.ToString(Eval("CategoryName"))) %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Include">
                                    <ItemTemplate>
                                        <asp:CheckBox ID="chkInclude" runat="server"
                                            Checked='<%# Eval("IncludeInSync") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </asp:View>

                    <asp:View ID="viewMap" runat="server">
                        <p><asp:Literal ID="litMapHelp" runat="server" /></p>
                        <div class="button-row">
                            <asp:Button ID="btnPullProducts" runat="server" CssClass="filter-panel-btn" OnClick="btnPullProducts_Click" CausesValidation="false" />
                        </div>
                        <asp:GridView ID="gvPull" runat="server" CssClass="results-table" AutoGenerateColumns="false"
                            DataKeyNames="RowKey" OnRowCommand="gvPull_RowCommand" OnRowDataBound="gvPull_RowDataBound">
                            <Columns>
                                <asp:BoundField DataField="Sku" HeaderText="Woo SKU" />
                                <asp:BoundField DataField="Name" HeaderText="Woo name" />
                                <asp:BoundField DataField="CategoriesLabel" HeaderText="Categories" />
                                <asp:BoundField DataField="Status" HeaderText="Status" />
                                <asp:TemplateField HeaderText="Tracker item">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlItem" runat="server" CssClass="sys-prefs-input" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Qty factor">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtQty" runat="server" Text='<%# Eval("QtyFactor") %>' Width="60px" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:Button ID="btnSaveMap" runat="server" CssClass="filter-panel-btn" CommandName="SaveMap"
                                            CommandArgument='<%# Container.DataItemIndex %>' Text="Save map" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                        <h3 style="margin-top:1rem;"><asp:Literal ID="litExistingMaps" runat="server" /></h3>
                        <asp:GridView ID="gvMaps" runat="server" CssClass="results-table" AutoGenerateColumns="false"
                            DataKeyNames="MappingID" OnRowCommand="gvMaps_RowCommand">
                            <Columns>
                                <asp:BoundField DataField="MappingID" HeaderText="ID" />
                                <asp:BoundField DataField="ItemSku" HeaderText="Item SKU" />
                                <asp:BoundField DataField="ItemDesc" HeaderText="Item" />
                                <asp:BoundField DataField="SkuPattern" HeaderText="Woo SKU" />
                                <asp:BoundField DataField="WooProductId" HeaderText="Product" />
                                <asp:BoundField DataField="WooVariationId" HeaderText="Variation" />
                                <asp:BoundField DataField="QtyFactor" HeaderText="Qty" />
                                <asp:BoundField DataField="LastWooStatus" HeaderText="Last Woo" />
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:Button ID="btnDelMap" runat="server" CssClass="filter-panel-btn" CommandName="DelMap"
                                            CommandArgument='<%# Eval("MappingID") %>' Text="Delete"
                                            OnClientClick="return confirm('Delete this mapping?');" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </asp:View>

                    <asp:View ID="viewWild" runat="server">
                        <p><asp:Literal ID="litWildHelp" runat="server" /></p>
                        <table class="detail-form-table sys-prefs-form-table">
                            <tr>
                                <td class="sys-prefs-label"><asp:Literal ID="litWildPrefix" runat="server" /></td>
                                <td class="sys-prefs-field"><asp:TextBox ID="txtWildPrefix" runat="server" CssClass="sys-prefs-input" /></td>
                            </tr>
                            <tr>
                                <td class="sys-prefs-label"><asp:Literal ID="litWildSuffix" runat="server" /></td>
                                <td class="sys-prefs-field"><asp:TextBox ID="txtWildSuffix" runat="server" CssClass="sys-prefs-input" /></td>
                            </tr>
                            <tr>
                                <td class="sys-prefs-label"><asp:Literal ID="litWildQty" runat="server" /></td>
                                <td class="sys-prefs-field"><asp:TextBox ID="txtWildQty" runat="server" Text="0.25" Width="80px" /></td>
                            </tr>
                            <tr>
                                <td class="sys-prefs-label"><asp:Literal ID="litWildItem" runat="server" /></td>
                                <td class="sys-prefs-field"><asp:DropDownList ID="ddlWildItem" runat="server" CssClass="sys-prefs-input" /></td>
                            </tr>
                            <tr>
                                <td class="sys-prefs-label"><asp:Literal ID="litWildNotes" runat="server" /></td>
                                <td class="sys-prefs-field"><asp:TextBox ID="txtWildNotes" runat="server" CssClass="sys-prefs-input" /></td>
                            </tr>
                        </table>
                        <asp:HiddenField ID="hdnWildId" runat="server" Value="0" />
                        <div class="button-row">
                            <asp:Button ID="btnSaveWild" runat="server" CssClass="filter-panel-btn" OnClick="btnSaveWild_Click" />
                        </div>
                        <asp:GridView ID="gvWild" runat="server" CssClass="results-table" AutoGenerateColumns="false"
                            DataKeyNames="RuleID" OnRowCommand="gvWild_RowCommand">
                            <Columns>
                                <asp:BoundField DataField="SkuPrefixPattern" HeaderText="Prefix" />
                                <asp:BoundField DataField="SuffixToken" HeaderText="Suffix" />
                                <asp:BoundField DataField="QtyFactor" HeaderText="Qty" />
                                <asp:BoundField DataField="ItemDesc" HeaderText="Item" />
                                <asp:BoundField DataField="Notes" HeaderText="Notes" />
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:Button ID="btnDelWild" runat="server" CssClass="filter-panel-btn" CommandName="DelWild"
                                            CommandArgument='<%# Eval("RuleID") %>' Text="Delete"
                                            OnClientClick="return confirm('Delete this rule?');" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </asp:View>

                    <asp:View ID="viewSync" runat="server">
                        <p><asp:Literal ID="litSyncHelp" runat="server" /></p>
                        <div class="button-row">
                            <asp:Button ID="btnDryPush" runat="server" CssClass="filter-panel-btn" OnClick="btnDryPush_Click" CausesValidation="false" />
                            <asp:Button ID="btnPushEnabled" runat="server" CssClass="filter-panel-btn sys-prefs-enable-btn" OnClick="btnPushEnabled_Click" CausesValidation="false" />
                        </div>
                    </asp:View>
                </asp:MultiView>

                <div class="page-tone-footer button-row">
                    <span class="image-button" title="Return to System Preferences">
                        <asp:ImageButton ID="btnBack" runat="server"
                            ImageUrl="~/images/imgButtons/Back.gif"
                            AlternateText="Back"
                            ToolTip="Return to System Preferences"
                            OnClick="btnBack_Click"
                            OnClientClick="return wooMapConfirmLeave();"
                            CausesValidation="false" />
                    </span>
                    <asp:Label ID="lblMessage" runat="server" CssClass="status-message" Visible="false" />
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
