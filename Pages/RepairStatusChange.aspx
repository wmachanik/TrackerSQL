<%@ Page Title="Repair Status Change" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RepairStatusChange.aspx.cs" Inherits="TrackerSQL.Pages.RepairStatusChange" %>

<asp:Content ID="cntRepairStatusChangeHdr" title="Repair Status Change" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        function beginRepairStatusSave(button) {
            if (button.getAttribute("data-saving") === "true") {
                return false;
            }

            button.setAttribute("data-saving", "true");
            button.setAttribute("aria-disabled", "true");
            button.style.pointerEvents = "none";
            button.value = "Saving...";

            var savingStatus = document.getElementById("repairStatusSaving");
            if (savingStatus) {
                savingStatus.style.display = "flex";
            }

            return true;
        }
    </script>
</asp:Content>
<asp:Content ID="cntRepairStatusChangeBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Repair Status Change</h2>
    <asp:ScriptManager runat="server" ID="scrmRepairStaus" />
    <asp:UpdatePanel ID="upnlRepairStaus" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <div class="responsive-layout-container">
                <div class="layout-main-panel">

                    <table id="tblRepairStatus" cellpadding="0" cellspacing="0" runat="server" class="TblCoffee">
                        <tr>
                            <td>Company</td>
                            <td>
                                <asp:Literal ID="ltrlComapny" runat="server" />
                            </td>
                        </tr>
                        <tr>
                            <td>Machine</td>
                            <td>
                                <asp:Literal ID="ltrlMachine" runat="server" />
                            </td>
                        </tr>
                        <tr>
                            <td>Machine S/N</td>
                            <td>
                                <asp:Literal ID="ltrlMachineSerialNumber" runat="server" /></td>
                        </tr>
                        <tr>
                            <td>Repair Status</td>
                            <td>
                                <asp:DropDownList ID="ddlRepairStatuses" runat="server" TabIndex="1"
                                    AppendDataBoundItems="True" Font-Size="Medium"
                                    DataSourceID="odsRepairStatuses" DataTextField="RepairStatusDesc"
                                    DataValueField="RepairStatusID">
                                    <asp:ListItem Text="--Select--" Value="0" />
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td>ID:</td>
                            <td>
                                <asp:Label ID="lblRepairID" runat="server" /></td>
                        </tr>
                        <tr>
                            <td colspan="2" class="horizMiddle">
                                <asp:Button ID="btnUpdateAndReturn" TabIndex="2" Text="Update & Return" runat="server"
                                    OnClick="btnUpdateAndReturn_Click" OnClientClick="return beginRepairStatusSave(this);"
                                    AccessKey="U" ToolTip="update and return (AltShftU)" />
                                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            <asp:Button ID="btnCancel" Text="Cancel" TabIndex="3" runat="server" OnClick="btnCancel_Click" />
                            </td>
                        </tr>
                    </table>
                    <div id="repairStatusSaving" class="status-message status-info page-tone-progress"
                        style="display: none;" role="status" aria-live="polite">
                        <img src="../images/animi/QuaffeeProgress.gif" alt="" />
                        <span>&nbsp;Saving repair status, please wait...</span>
                    </div>
                    <div class="status-message"><asp:Literal ID="ltrlStatus" runat="server" /></div>
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="btnUpdateAndReturn" />
            <asp:PostBackTrigger ControlID="btnCancel" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:UpdateProgress ID="udtpRepairStaus" runat="server" AssociatedUpdatePanelID="upnlRepairStaus">
        <ProgressTemplate>
            &nbsp;&nbsp;
      <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:ObjectDataSource ID="odsRepairStatuses" runat="server" SortParameterName="sortBy"
        SelectMethod="GetRepairStatuses" TypeName="TrackerSQL.Managers.RepairLookupDataSource">
        <SelectParameters>
            <asp:Parameter DefaultValue="SortOrder" Name="sortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>

</asp:Content>
