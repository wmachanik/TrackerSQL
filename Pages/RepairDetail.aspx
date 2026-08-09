<%@ Page Title="Repair Detail" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="True"
    CodeBehind="RepairDetail.aspx.cs" Inherits="TrackerSQL.Pages.RepairDetail" MaintainScrollPositionOnPostback="true" %>

<asp:Content ID="cntRepairDetailHdr" title="Repair Detail" ContentPlaceHolderID="HeadContent" runat="server">
    <%-- Keep HeadContent free of <%= %> — ScriptManager cannot modify <head> when it contains code blocks. --%>
    <script type="text/javascript">
        // Lock the clicked save/delete button, swap its text and show the progress strip.
        // Full postbacks (Delete, Save & Return) never fire UpdateProgress, hence this.
        function beginRepairDetailSave(button, savingText, confirmMessage) {
            if (typeof repairDetailAllowNavigate === "function" && !repairDetailAllowNavigate()) {
                return false;
            }
            if (confirmMessage && !window.confirm(confirmMessage)) {
                return false;
            }
            if (!button || button.getAttribute("data-saving") === "true") {
                return false;
            }

            button.setAttribute("data-saving", "true");
            button.setAttribute("aria-disabled", "true");
            button.style.pointerEvents = "none";
            button.value = savingText || "Saving...";

            var row = button.parentNode;
            if (row) {
                var siblings = row.querySelectorAll("input[type='submit'], input[type='button'], button");
                for (var i = 0; i < siblings.length; i++) {
                    if (siblings[i] !== button) {
                        siblings[i].disabled = true;
                        siblings[i].style.opacity = "0.5";
                        siblings[i].style.pointerEvents = "none";
                    }
                }
            }

            var savingStatus = document.getElementById("repairDetailSaving");
            if (savingStatus) {
                var label = savingStatus.querySelector("span");
                if (label) {
                    label.innerHTML = "&nbsp;" + (savingText || "Saving") + ", please wait...";
                }
                savingStatus.style.display = "flex";
            }

            return true;
        }
    </script>
</asp:Content>

<asp:Content ID="cntRepairDetailBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager runat="server" ID="scrmRepairDetail" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="udtpRepairDetail" runat="server"
        AssociatedUpdatePanelID="upnlRepairDetail" DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info page-tone-progress">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:HiddenField ID="hdnRepairDirty" runat="server" Value="0" />

    <asp:UpdatePanel ID="upnlRepairDetail" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <Triggers>
            <%-- Insert redirects; must be a full postback or UpdatePanel can fire create twice. --%>
            <asp:PostBackTrigger ControlID="btnInsert" />
            <asp:AsyncPostBackTrigger ControlID="btnUpdate" EventName="Click" />
            <asp:PostBackTrigger ControlID="btnUpdateAndReturn" />
            <asp:PostBackTrigger ControlID="btnDelete" />
            <asp:PostBackTrigger ControlID="btnCancel" />
            <asp:PostBackTrigger ControlID="btnCancelInsert" />
        </Triggers>
        <ContentTemplate>
            <asp:Panel ID="pnlRepairShell" runat="server" CssClass="simpleForm page-tone-panel page-tone-repairs">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/icons8-repair-tools-16.png" alt="" />
                    <div>
                        <h1 class="page-tone-title">Repair Detail</h1>
                        <p class="page-tone-subtitle">View and manage repair requests</p>
                    </div>
                </div>

                <asp:Panel ID="pnlNewRepair" Visible="false" runat="server">
                    <h2 style="margin: 12px 0 8px;">New Item for Repair</h2>
                    <table class="TblCoffee detail-form-table">
                        <tr>
                            <td>Customer</td>
                            <td>
                                <ajaxToolkit:ComboBox ID="cboNewCompany" runat="server"
                                    DataSourceID="odsCompanys"
                                    DataTextField="CompanyName"
                                    DataValueField="CustomerID"
                                    AutoCompleteMode="SuggestAppend"
                                    DropDownStyle="DropDown"
                                    AppendDataBoundItems="true"
                                    Width="250px">
                                    <asp:ListItem Selected="True" Value="0">none</asp:ListItem>
                                </ajaxToolkit:ComboBox>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2" class="rowC" style="text-align: center; padding-top: 12px;">
                                <div class="button-row">
                                    <asp:Button ID="btnInsert" Text="Insert" runat="server" CssClass="filter-panel-btn"
                                        OnClick="btnInsert_Click"
                                        OnClientClick="return beginRepairDetailSave(this, 'Creating...');" />
                                    <asp:Button ID="btnCancelInsert" Text="Back" runat="server" CssClass="filter-panel-btn"
                                        OnClick="btnCancel_Click" CausesValidation="false"
                                        OnClientClick="return repairDetailConfirmLeave();" />
                                </div>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>

                <asp:Panel ID="pnlRepairDetail" runat="server">
                    <table cellpadding="0" cellspacing="0" runat="server" class="TblCoffee detail-form-table">
                        <tr>
                            <td>Company</td>
                            <td>
                                <ajaxToolkit:ComboBox ID="cboCompany" runat="server"
                                    DataSourceID="odsCompanys"
                                    DataTextField="CompanyName"
                                    DataValueField="CustomerID"
                                    AutoCompleteMode="SuggestAppend"
                                    DropDownStyle="DropDown"
                                    AppendDataBoundItems="true"
                                    Width="250px">
                                    <asp:ListItem Selected="True" Value="0">none</asp:ListItem>
                                </ajaxToolkit:ComboBox>
                            </td>
                        </tr>
                        <tr>
                            <td>Contact</td>
                            <td>
                                <asp:TextBox ID="tbxContactName" runat="server" />
                            </td>
                        </tr>
                        <tr>
                            <td>Email</td>
                            <td>
                                <asp:TextBox ID="tbxContactEmail" runat="server" /></td>
                        </tr>
                        <tr>
                            <td>J/C num</td>
                            <td>
                                <asp:TextBox ID="tbxJobCardNumber" runat="server" /></td>
                        </tr>
                        <tr>
                            <td>Machine Type</td>
                            <td>
                                <asp:DropDownList ID="ddlEquipTypes" runat="server" AppendDataBoundItems="True"
                                    DataSourceID="odsEquipTypes" DataTextField="EquipTypeName"
                                    DataValueField="EquipTypeId">
                                    <asp:ListItem Selected="True" Value="0">unknown</asp:ListItem>
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td>Machine S/N</td>
                            <td>
                                <asp:TextBox ID="tbxMachineSerialNumber" runat="server" /></td>
                        </tr>
                        <tr>
                            <td>Swop Out?</td>
                            <td>
                                <asp:DropDownList ID="ddlSwopOutMachine" runat="server" DataSourceID="odsCompanyDemos"
                                    DataTextField="CompanyName" DataValueField="CustomerID"
                                    AppendDataBoundItems="True">
                                    <asp:ListItem Text="none" Value="0" />
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td>Machine Condition</td>
                            <td>
                                <asp:DropDownList ID="ddlMachineCondtion" runat="server"
                                    AppendDataBoundItems="True" DataSourceID="odsMachineConditions"
                                    DataTextField="ConditionDesc" DataValueField="EquipConditionID">
                                    <asp:ListItem Text="none" Value="0" />
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td>Taken</td>
                            <td>
                                <asp:CheckBox ID="cbxTakenFrother" runat="server" Text="Frother" TextAlign="Left" />&nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:CheckBox ID="cbxTakenBeanLid" runat="server" Text="Bean Lid" TextAlign="Left" />&nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:CheckBox ID="cbxTakenWaterLid" runat="server" Text="Water Lid" TextAlign="Left" />
                            </td>
                        </tr>
                        <tr>
                            <td>Broken</td>
                            <td>
                                <asp:CheckBox ID="cbxBrokenFrother" runat="server" Text="Frother" TextAlign="Left" />&nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:CheckBox ID="cbxBrokenBeanLid" runat="server" Text="Bean Lid" TextAlign="Left" />&nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:CheckBox ID="cbxBrokenWaterLid" runat="server" Text="Water Lid" TextAlign="Left" />
                            </td>
                        </tr>
                        <tr>
                            <td>Repair Fault</td>
                            <td>
                                <asp:DropDownList ID="ddlRepairFault" runat="server" AppendDataBoundItems="True"
                                    DataSourceID="odsRepairFaults" DataTextField="RepairFaultDesc" DataValueField="RepairFaultID">
                                    <asp:ListItem Text="--Select--" Value="0" />
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:TextBox ID="tbxRepairFaultDesc" runat="server"
                                    TextMode="MultiLine" Width="98%" />
                            </td>
                        </tr>
                        <tr>
                            <td>Repair Status</td>
                            <td>
                                <asp:DropDownList ID="ddlRepairStatuses" runat="server"
                                    AppendDataBoundItems="True"
                                    DataSourceID="odsRepairStatuses" DataTextField="RepairStatusDesc"
                                    DataValueField="RepairStatusID">
                                    <asp:ListItem Text="--Select--" Value="0" />
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <em>Notes:</em><br />
                                <asp:TextBox ID="tbxNotes" runat="server" TextMode="MultiLine" Width="98%" />
                            </td>
                        </tr>
                        <tr style="font-size: x-small">
                            <td>ID:<asp:Label ID="lblRepairID" runat="server" />&nbsp;&nbsp;OLID:
                                <asp:HiddenField ID="hdnRelatedOrderLineID" runat="server" Value="0" />
                                <a runat="server" id="lnkRelatedOrder" class="repair-related-order-link"
                                    target="_blank" rel="noopener"></a>
                            </td>
                            <td>Logged:<asp:Label ID="lblDateLogged" runat="server" />&nbsp;&nbsp;
                                Chng:<asp:Label ID="lblLastChanged" runat="server" /></td>
                        </tr>
                        <tr>
                            <td colspan="2" class="rowC" style="text-align: center; padding-top: 12px;">
                                <div class="button-row">
                                    <asp:Button ID="btnUpdate" Text="Save" runat="server" CssClass="filter-panel-btn"
                                        OnClick="btnUpdate_Click" AccessKey="S"
                                        OnClientClick="return beginRepairDetailSave(this, 'Saving...');"
                                        ToolTip="Save and stay on this page (Alt+Shift+S)" />
                                    <asp:Button ID="btnUpdateAndReturn" Text="Save &amp; Return" runat="server" CssClass="filter-panel-btn"
                                        OnClick="btnUpdateAndReturn_Click" AccessKey="U"
                                        OnClientClick="return beginRepairDetailSave(this, 'Saving...');"
                                        ToolTip="Save and return (Alt+Shift+U)" />
                                    <asp:Button ID="btnDelete" Text="Delete" runat="server" CssClass="filter-panel-btn"
                                        OnClick="btnDelete_Click"
                                        OnClientClick="return beginRepairDetailSave(this, 'Deleting...', 'Delete this repair?');"
                                        ToolTip="Delete this repair" />
                                    <asp:Button ID="btnCancel" Text="Back" runat="server" CssClass="filter-panel-btn"
                                        OnClick="btnCancel_Click" CausesValidation="false"
                                        OnClientClick="return repairDetailConfirmLeave();"
                                        ToolTip="Return without saving" />
                                </div>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>

                <div id="repairDetailSaving" class="status-message status-info page-tone-progress"
                    style="display: none; margin-top: 12px;" role="status" aria-live="polite">
                    <img src="../images/animi/QuaffeeProgress.gif" alt="" />
                    <span>&nbsp;Saving repair, please wait...</span>
                </div>

                <div class="status-message" id="pnlStatusMessage" runat="server" style="margin-top: 12px;">
                    <asp:Literal ID="ltrlStatus" runat="server" />
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>

    <asp:ObjectDataSource ID="odsCompanys" runat="server" TypeName="TrackerSQL.Managers.RepairLookupDataSource"
        SelectMethod="GetCompanyNames" OldValuesParameterFormatString="original_{0}"></asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsCompanyDemos" runat="server" TypeName="TrackerSQL.Managers.RepairLookupDataSource"
        SelectMethod="GetDemoCompanyNames"
        OldValuesParameterFormatString="original_{0}"></asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsEquipTypes" runat="server" TypeName="TrackerSQL.Managers.RepairLookupDataSource"
        SortParameterName="sortBy" SelectMethod="GetEquipTypes"
        OldValuesParameterFormatString="original_{0}">
        <SelectParameters>
            <asp:Parameter DefaultValue="EquipTypeName" Name="sortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>

    <asp:ObjectDataSource ID="odsRepairFaults" runat="server" SelectMethod="GetRepairFaults"
        SortParameterName="sortBy" TypeName="TrackerSQL.Managers.RepairLookupDataSource">
        <SelectParameters>
            <asp:Parameter DefaultValue="SortOrder" Name="sortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsRepairStatuses" runat="server" SortParameterName="sortBy"
        SelectMethod="GetRepairStatuses" TypeName="TrackerSQL.Managers.RepairLookupDataSource">
        <SelectParameters>
            <asp:Parameter DefaultValue="SortOrder" Name="sortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsMachineConditions" runat="server" SortParameterName="sortBy"
        SelectMethod="GetEquipConditions" TypeName="TrackerSQL.Managers.RepairLookupDataSource">
        <SelectParameters>
            <asp:Parameter DefaultValue="SortOrder" Name="sortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>

    <%-- Init in MainContent (not Head) so <%= ClientID %> does not block ScriptManager --%>
    <script type="text/javascript">
        TrackerUnsaved.init({
            dirtyFieldId: '<%= hdnRepairDirty.ClientID %>',
            rootId: '<%= pnlRepairShell.ClientID %>',
            leaveMessage: 'You have unsaved changes. Leave without saving?',
            aliases: {
                markDirty: 'repairDetailMarkDirty',
                clearDirty: 'repairDetailClearDirty',
                allowNavigate: 'repairDetailAllowNavigate',
                confirmLeave: 'repairDetailConfirmLeave',
                wireFields: 'repairDetailWireFields'
            }
        });
    </script>
</asp:Content>
