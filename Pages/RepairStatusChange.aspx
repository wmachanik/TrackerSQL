<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RepairStatusChange.aspx.cs" Inherits="TrackerDotNet.Pages.RepairStatusChange" %>

<asp:Content ID="cntRepairStatusChangeHdr" ContentPlaceHolderID="HeadContent" runat="server">
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
                                <asp:Button ID="btnUpdateAndReturn" TabIndex="2" Text="Update & Return" runat="server" OnClick="btnUpdateAndReturn_Click" AccessKey="U" ToolTip="update and return (AltShftU)" />
                                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            <asp:Button ID="btnCancel" Text="Cancel" TabIndex="3" runat="server" OnClick="btnCancel_Click" />
                            </td>
                        </tr>
                    </table>
                     <div class="status-message"><asp:Literal ID="ltrlStatus" runat="server" /></div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:UpdateProgress ID="udtpRepairStaus" runat="server" AssociatedUpdatePanelID="upnlRepairStaus">
        <ProgressTemplate>
            &nbsp;&nbsp;
      <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:ObjectDataSource ID="odsEquipTypes" runat="server" TypeName="TrackerDotNet.Controls.EquipTypeTbl"
        SortParameterName="SortBy" SelectMethod="GetAll"
        OldValuesParameterFormatString="original_{0}">
        <SelectParameters>
            <asp:Parameter DefaultValue="EquipTypeName" Name="SortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsRepairStatuses" runat="server" SortParameterName="SortBy"
        SelectMethod="GetAll" TypeName="TrackerDotNet.Controls.RepairStatusesTbl">
        <SelectParameters>
            <asp:Parameter DefaultValue="SortOrder" Name="SortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>

</asp:Content>
