<%@ Page Title="System Preferences" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="SystemPreferences.aspx.cs" Inherits="TrackerSQL.Tools.SystemPreferences" %>

<asp:Content ID="cntSysPrefsHdr" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        (function () {
            function byId(id) { return document.getElementById(id); }

            window.trackerToggleSecret = function (inputId, btn) {
                var input = byId(inputId);
                if (!input || !btn) return;
                var showText = btn.getAttribute('data-show') || 'Show';
                var hideText = btn.getAttribute('data-hide') || 'Hide';
                if (input.type === 'password') {
                    input.type = 'text';
                    btn.value = hideText;
                    btn.setAttribute('aria-pressed', 'true');
                } else {
                    input.type = 'password';
                    btn.value = showText;
                    btn.setAttribute('aria-pressed', 'false');
                }
            };

            window.trackerFillAdminFromStore = function (storeId, adminId, force) {
                var store = byId(storeId);
                var admin = byId(adminId);
                if (!store || !admin) return;
                var base = (store.value || '').trim().replace(/\/+$/, '');
                if (!base) return;
                var suggested = base + '/wp-admin';
                var current = (admin.value || '').trim();
                var autoFlag = admin.getAttribute('data-auto-admin') === '1';
                if (force || !current || autoFlag || current === admin.getAttribute('data-last-auto')) {
                    admin.value = suggested;
                    admin.setAttribute('data-auto-admin', '1');
                    admin.setAttribute('data-last-auto', suggested);
                }
            };

            window.trackerMarkAdminManual = function (adminId) {
                var admin = byId(adminId);
                if (!admin) return;
                admin.setAttribute('data-auto-admin', '0');
            };
        })();
    </script>
</asp:Content>

<asp:Content ID="cntSysPrefsBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smSysPrefs" runat="server" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="updtPrgSysPrefs" runat="server" AssociatedUpdatePanelID="upnlSysPrefs"
        DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info page-tone-progress">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:ObjectDataSource ID="odsSysData" runat="server"
        TypeName="TrackerSQL.Tools.SystemPreferences"
        SelectMethod="GetSystemDataForBinding"
        UpdateMethod="UpdateSystemData"
        DataObjectTypeName="TrackerSQL.Models.SysData">
    </asp:ObjectDataSource>

    <asp:ObjectDataSource ID="odsItemServiceTypes" runat="server"
        TypeName="TrackerSQL.Repositories.ItemServiceTypesRepository"
        SelectMethod="GetAll">
    </asp:ObjectDataSource>

    <asp:UpdatePanel ID="upnlSysPrefs" runat="server" ChildrenAsTriggers="true" UpdateMode="Always">
        <ContentTemplate>
            <asp:Panel ID="pnlAccessDenied" runat="server" Visible="false" CssClass="simpleForm page-tone-panel page-tone-sysdata">
                <asp:Label ID="lblAccessDenied" runat="server" CssClass="status-message status-error" />
            </asp:Panel>

            <asp:Panel ID="pnlSysPrefs" runat="server" CssClass="simpleForm page-tone-panel page-tone-sysdata">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/Toolbox.png" alt="" />
                    <div>
                        <h1 class="page-tone-title"><asp:Literal ID="litPageTitle" runat="server" /></h1>
                        <p class="page-tone-subtitle"><asp:Literal ID="litPageSubtitle" runat="server" /></p>
                    </div>
                </div>

                <div class="sys-prefs-tabs-wrap">
                    <span class="sys-prefs-tabs-label"><asp:Literal ID="litSectionsHeading" runat="server" /></span>
                    <nav class="sys-prefs-tabs" aria-label="Preferences sections">
                        <asp:LinkButton ID="btnNavGeneral" runat="server" CssClass="sys-prefs-tab" OnClick="btnNavGeneral_Click" CausesValidation="false" />
                        <asp:LinkButton ID="btnNavWoo" runat="server" CssClass="sys-prefs-tab" OnClick="btnNavWoo_Click" CausesValidation="false" />
                    </nav>
                </div>

                <div class="sys-prefs-content">
                        <asp:MultiView ID="mvSections" runat="server" ActiveViewIndex="0">
                            <asp:View ID="viewGeneral" runat="server">
                                <div class="responsive-layout-container">
                                    <asp:DetailsView ID="dvSystemData" runat="server"
                                        AutoGenerateRows="False"
                                        CssClass="TblFlex table-auto-width"
                                        DataSourceID="odsSysData"
                                        OnItemUpdated="dvSystemData_ItemUpdated"
                                        OnDataBound="dvSystemData_DataBound"
                                        OnModeChanging="dvSystemData_ModeChanging"
                                        DefaultMode="ReadOnly">
                                        <Fields>
                                            <asp:BoundField DataField="ID" HeaderText="ID" ReadOnly="true" />
                                            <asp:CheckBoxField DataField="DoRecurringOrders" HeaderText="Do Recurring Orders" />
                                            <asp:TemplateField HeaderText="Last Recurring Date" SortExpression="LastRecurringDate">
                                                <ItemTemplate>
                                                    <%# Eval("LastRecurringDate", "{0:d}") ?? "(not set)" %>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:TextBox ID="txtLastRecurringDate" runat="server"
                                                        Text='<%# Bind("LastRecurringDate", "{0:MM/dd/yyyy}") %>' Width="120px" />
                                                    <asp:ImageButton ID="imgCalendarRecurringDate" runat="server" ImageUrl="~/images/imgButtons/CalendarBtn.png"
                                                        AlternateText="Select date" Style="vertical-align: middle;" />
                                                    <ajaxToolkit:CalendarExtender ID="calLastRecurringDate" runat="server"
                                                        TargetControlID="txtLastRecurringDate" PopupButtonID="imgCalendarRecurringDate"
                                                        Format="MM/dd/yyyy" CssClass="calendar-popup" />
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Date Last Prep Date Calculated" SortExpression="DateLastPrepDateCalcd">
                                                <ItemTemplate>
                                                    <%# Eval("DateLastPrepDateCalcd", "{0:d}") ?? "(not set)" %>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:TextBox ID="txtDateLastPrepDateCalcd" runat="server"
                                                        Text='<%# Bind("DateLastPrepDateCalcd", "{0:MM/dd/yyyy}") %>' Width="120px" />
                                                    <asp:ImageButton ID="imgCalendarPrepDate" runat="server" ImageUrl="~/images/imgButtons/CalendarBtn.png"
                                                        AlternateText="Select date" Style="vertical-align: middle;" />
                                                    <ajaxToolkit:CalendarExtender ID="calDateLastPrepDateCalcd" runat="server"
                                                        TargetControlID="txtDateLastPrepDateCalcd" PopupButtonID="imgCalendarPrepDate"
                                                        Format="MM/dd/yyyy" CssClass="calendar-popup" />
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Min Reminder Date" SortExpression="MinReminderDate">
                                                <ItemTemplate>
                                                    <%# Eval("MinReminderDate", "{0:d}") ?? "(not set)" %>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:TextBox ID="txtMinReminderDate" runat="server"
                                                        Text='<%# Bind("MinReminderDate", "{0:MM/dd/yyyy}") %>' Width="120px" />
                                                    <asp:ImageButton ID="imgMinReminderDate" runat="server" ImageUrl="~/images/imgButtons/CalendarBtn.png"
                                                        AlternateText="Select date" Style="vertical-align: middle;" />
                                                    <ajaxToolkit:CalendarExtender ID="calMinReminderDate" runat="server"
                                                        TargetControlID="txtMinReminderDate" PopupButtonID="imgMinReminderDate"
                                                        Format="MM/dd/yyyy" CssClass="calendar-popup" />
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Group Service Type (used for group items)" SortExpression="GroupReferenceItemID">
                                                <ItemTemplate>
                                                    <%# GetItemServiceTypeName((int?)Eval("GroupReferenceItemID")) %>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:DropDownList ID="ddlGroupReferenceItemID" runat="server"
                                                        DataSourceID="odsItemServiceTypes"
                                                        DataTextField="ItemServiceTypeName"
                                                        DataValueField="ItemServiceTypeID"
                                                        SelectedValue='<%# Bind("GroupReferenceItemID") %>'
                                                        AppendDataBoundItems="true">
                                                        <asp:ListItem Value="" Text="(none)" />
                                                    </asp:DropDownList>
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="InternalContactIDs" HeaderText="Internal Contact IDs" NullDisplayText="(none)" />
                                            <asp:CommandField ShowEditButton="True" ButtonType="Image"
                                                EditImageUrl="~/images/imgButtons/EditItem.gif"
                                                UpdateImageUrl="~/images/imgButtons/UpdateItem.gif"
                                                CancelImageUrl="~/images/imgButtons/CancelItem.gif"
                                                ItemStyle-HorizontalAlign="Center"
                                                ItemStyle-CssClass="command-field-padding" />
                                        </Fields>
                                    </asp:DetailsView>
                                </div>
                            </asp:View>

                            <asp:View ID="viewWoo" runat="server">
                                <h2 class="sys-prefs-section-title"><asp:Literal ID="litWooSectionTitle" runat="server" /></h2>
                                <asp:Panel ID="pnlWooHome" runat="server">
                                    <p><asp:Literal ID="litWooStatus" runat="server" /></p>
                                    <p class="sys-prefs-phase-note"><asp:Literal ID="litPhase2Note" runat="server" /></p>
                                    <div class="button-row">
                                        <asp:Button ID="btnStartWizard" runat="server" CssClass="filter-panel-btn" OnClick="btnStartWizard_Click" CausesValidation="false" />
                                        <asp:HyperLink ID="hlWooMapping" runat="server" NavigateUrl="~/Tools/WooCommerceMapping.aspx"
                                            CssClass="filter-panel-btn sys-prefs-mapping-link" />
                                    </div>
                                    <asp:Panel ID="pnlWooSettings" runat="server" Visible="false" CssClass="sys-prefs-woo-settings">
                                        <table class="detail-form-table sys-prefs-form-table">
                                            <tr>
                                                <td class="sys-prefs-label"><asp:Literal ID="litLblStoreUrl" runat="server" /></td>
                                                <td class="sys-prefs-field"><asp:TextBox ID="txtStoreUrl" runat="server" CssClass="sys-prefs-input" /></td>
                                            </tr>
                                            <tr>
                                                <td class="sys-prefs-label"><asp:Literal ID="litLblAdminUrl" runat="server" /></td>
                                                <td class="sys-prefs-field"><asp:TextBox ID="txtAdminUrl" runat="server" CssClass="sys-prefs-input" /></td>
                                            </tr>
                                            <tr>
                                                <td class="sys-prefs-label"><asp:Literal ID="litLblKey" runat="server" /></td>
                                                <td class="sys-prefs-field">
                                                    <div class="sys-prefs-secret-row">
                                                        <asp:TextBox ID="txtConsumerKey" runat="server" CssClass="sys-prefs-input" TextMode="Password" autocomplete="off" />
                                                        <asp:Button ID="btnToggleKeyHome" runat="server" CssClass="filter-panel-btn sys-prefs-toggle-secret"
                                                            UseSubmitBehavior="false" CausesValidation="false" />
                                                    </div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="sys-prefs-label"><asp:Literal ID="litLblSecret" runat="server" /></td>
                                                <td class="sys-prefs-field">
                                                    <div class="sys-prefs-secret-row">
                                                        <asp:TextBox ID="txtConsumerSecret" runat="server" CssClass="sys-prefs-input" TextMode="Password" autocomplete="off" />
                                                        <asp:Button ID="btnToggleSecretHome" runat="server" CssClass="filter-panel-btn sys-prefs-toggle-secret"
                                                            UseSubmitBehavior="false" CausesValidation="false" />
                                                    </div>
                                                </td>
                                            </tr>
                                        </table>
                                        <p class="sys-prefs-saved-hint"><asp:Literal ID="litSavedKeyHint" runat="server" /></p>
                                        <div class="button-row">
                                            <asp:Button ID="btnSaveWooCreds" runat="server" CssClass="filter-panel-btn" OnClick="btnSaveWooCreds_Click" />
                                            <asp:Button ID="btnTestWoo" runat="server" CssClass="filter-panel-btn" OnClick="btnTestWoo_Click" CausesValidation="false" />
                                        </div>
                                        <asp:Panel ID="pnlEnableNext" runat="server" Visible="false" CssClass="sys-prefs-enable-next">
                                            <p class="sys-prefs-enable-hint"><asp:Literal ID="litEnableNextHint" runat="server" /></p>
                                            <p>
                                                <asp:CheckBox ID="chkOpenMappingAfterEnable" runat="server" Checked="true" />
                                                <asp:Literal ID="litOpenMappingAfterEnable" runat="server" />
                                            </p>
                                            <asp:Button ID="btnEnableWoo" runat="server" CssClass="filter-panel-btn sys-prefs-enable-btn"
                                                OnClick="btnEnableWoo_Click" CausesValidation="false" />
                                        </asp:Panel>
                                        <asp:Panel ID="pnlDisableNext" runat="server" Visible="false" CssClass="sys-prefs-disable-next">
                                            <p class="sys-prefs-disable-hint"><asp:Literal ID="litDisableHint" runat="server" /></p>
                                            <div class="button-row">
                                                <asp:Button ID="btnDisableWoo" runat="server" CssClass="filter-panel-btn"
                                                    OnClick="btnDisableWoo_Click" CausesValidation="false" />
                                                <asp:Button ID="btnRerunWizard" runat="server" CssClass="filter-panel-btn sys-prefs-secondary-btn"
                                                    OnClick="btnStartWizard_Click" CausesValidation="false" />
                                            </div>
                                        </asp:Panel>
                                    </asp:Panel>
                                </asp:Panel>

                                <asp:Panel ID="pnlWizard" runat="server" Visible="false" CssClass="sys-prefs-wizard">
                                    <div class="sys-prefs-progress" aria-label="Setup progress">
                                        <div class="sys-prefs-progress-label"><asp:Literal ID="litProgressLabel" runat="server" /></div>
                                        <ol class="sys-prefs-progress-steps">
                                            <li runat="server" id="liProg1" class="sys-prefs-progress-step"><asp:Literal ID="litProg1" runat="server" /></li>
                                            <li runat="server" id="liProg2" class="sys-prefs-progress-step"><asp:Literal ID="litProg2" runat="server" /></li>
                                            <li runat="server" id="liProg3" class="sys-prefs-progress-step"><asp:Literal ID="litProg3" runat="server" /></li>
                                            <li runat="server" id="liProg4" class="sys-prefs-progress-step"><asp:Literal ID="litProg4" runat="server" /></li>
                                            <li runat="server" id="liProg5" class="sys-prefs-progress-step"><asp:Literal ID="litProg5" runat="server" /></li>
                                            <li runat="server" id="liProg6" class="sys-prefs-progress-step"><asp:Literal ID="litProg6" runat="server" /></li>
                                        </ol>
                                    </div>
                                    <asp:MultiView ID="mvWizard" runat="server" ActiveViewIndex="0">
                                        <asp:View ID="wizPrep" runat="server">
                                            <h3><asp:Literal ID="litWizPrepTitle" runat="server" /></h3>
                                            <div class="sys-prefs-help"><asp:Literal ID="litWizPrepBody" runat="server" Mode="PassThrough" /></div>
                                        </asp:View>
                                        <asp:View ID="wizSchema" runat="server">
                                            <h3><asp:Literal ID="litWizSchemaTitle" runat="server" /></h3>
                                            <p><asp:Literal ID="litWizSchemaBody" runat="server" Mode="PassThrough" /></p>
                                            <asp:Button ID="btnEnsureSchema" runat="server" CssClass="filter-panel-btn" OnClick="btnEnsureSchema_Click" CausesValidation="false" />
                                        </asp:View>
                                        <asp:View ID="wizCreds" runat="server">
                                            <h3><asp:Literal ID="litWizCredsTitle" runat="server" /></h3>
                                            <table class="detail-form-table sys-prefs-form-table">
                                                <tr>
                                                    <td class="sys-prefs-label"><asp:Literal ID="litWizLblStore" runat="server" /></td>
                                                    <td class="sys-prefs-field"><asp:TextBox ID="txtWizStoreUrl" runat="server" CssClass="sys-prefs-input" /></td>
                                                </tr>
                                                <tr>
                                                    <td class="sys-prefs-label"><asp:Literal ID="litWizLblAdmin" runat="server" /></td>
                                                    <td class="sys-prefs-field"><asp:TextBox ID="txtWizAdminUrl" runat="server" CssClass="sys-prefs-input" /></td>
                                                </tr>
                                                <tr>
                                                    <td class="sys-prefs-label"><asp:Literal ID="litWizLblKey" runat="server" /></td>
                                                    <td class="sys-prefs-field">
                                                        <div class="sys-prefs-secret-row">
                                                            <asp:TextBox ID="txtWizKey" runat="server" CssClass="sys-prefs-input" TextMode="Password" autocomplete="off" />
                                                            <asp:Button ID="btnToggleKeyWiz" runat="server" CssClass="filter-panel-btn sys-prefs-toggle-secret"
                                                                UseSubmitBehavior="false" CausesValidation="false" />
                                                        </div>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="sys-prefs-label"><asp:Literal ID="litWizLblSecret" runat="server" /></td>
                                                    <td class="sys-prefs-field">
                                                        <div class="sys-prefs-secret-row">
                                                            <asp:TextBox ID="txtWizSecret" runat="server" CssClass="sys-prefs-input" TextMode="Password" autocomplete="off" />
                                                            <asp:Button ID="btnToggleSecretWiz" runat="server" CssClass="filter-panel-btn sys-prefs-toggle-secret"
                                                                UseSubmitBehavior="false" CausesValidation="false" />
                                                        </div>
                                                    </td>
                                                </tr>
                                            </table>
                                            <asp:Button ID="btnWizSaveCreds" runat="server" CssClass="filter-panel-btn" OnClick="btnWizSaveCreds_Click" />
                                        </asp:View>
                                        <asp:View ID="wizTest" runat="server">
                                            <h3><asp:Literal ID="litWizTestTitle" runat="server" /></h3>
                                            <p><asp:Literal ID="litWizTestBody" runat="server" /></p>
                                            <asp:Button ID="btnWizTest" runat="server" CssClass="filter-panel-btn" OnClick="btnWizTest_Click" CausesValidation="false" />
                                        </asp:View>
                                        <asp:View ID="wizOptions" runat="server">
                                            <h3><asp:Literal ID="litWizOptionsTitle" runat="server" /></h3>
                                            <table class="detail-form-table">
                                                <tr>
                                                    <td><asp:Literal ID="litWizLblCat" runat="server" /></td>
                                                    <td>
                                                        <asp:DropDownList ID="ddlCategoryMode" runat="server">
                                                            <asp:ListItem Value="All" Text="All" />
                                                        </asp:DropDownList>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td><asp:Literal ID="litWizLblDispatch" runat="server" /></td>
                                                    <td><asp:TextBox ID="txtDispatchIds" runat="server" Text="5,7" Width="200px" /></td>
                                                </tr>
                                                <tr>
                                                    <td colspan="2">
                                                        <asp:CheckBox ID="chkTrackingRequired" runat="server" Checked="true" />
                                                        <asp:Literal ID="litWizLblTracking" runat="server" />
                                                    </td>
                                                </tr>
                                            </table>
                                            <asp:Button ID="btnWizSaveOptions" runat="server" CssClass="filter-panel-btn" OnClick="btnWizSaveOptions_Click" />
                                        </asp:View>
                                        <asp:View ID="wizFinish" runat="server">
                                            <h3><asp:Literal ID="litWizFinishTitle" runat="server" /></h3>
                                            <p><asp:Literal ID="litWizFinishBody" runat="server" /></p>
                                            <p>
                                                <asp:CheckBox ID="chkOpenMappingAfterFinish" runat="server" Checked="true" />
                                                <asp:Literal ID="litOpenMappingAfterFinish" runat="server" />
                                            </p>
                                            <asp:Button ID="btnWizFinish" runat="server" CssClass="filter-panel-btn" OnClick="btnWizFinish_Click" CausesValidation="false" />
                                        </asp:View>
                                    </asp:MultiView>
                                    <div class="button-row" style="margin-top: 12px;">
                                        <asp:Button ID="btnWizBack" runat="server" CssClass="filter-panel-btn" OnClick="btnWizBack_Click" CausesValidation="false" />
                                        <asp:Button ID="btnWizNext" runat="server" CssClass="filter-panel-btn" OnClick="btnWizNext_Click" CausesValidation="false" />
                                        <asp:Button ID="btnWizCancel" runat="server" CssClass="filter-panel-btn" OnClick="btnWizCancel_Click" CausesValidation="false" />
                                    </div>
                                </asp:Panel>
                            </asp:View>
                        </asp:MultiView>
                </div>

                <div class="page-tone-footer">
                    <asp:Label ID="lblMessage" runat="server" CssClass="status-message" Visible="false"></asp:Label>
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
