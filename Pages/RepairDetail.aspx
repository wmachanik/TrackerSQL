<%@ Page Title="Repair Detail" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="True" CodeBehind="RepairDetail.aspx.cs" Inherits="TrackerDotNet.Pages.RepairDetail" %>

<asp:Content ID="cntRepairDetailHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntRepairDetailBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Repair Detail</h1>
    <asp:ScriptManager runat="server" ID="scrmRepairDetail" />
    <asp:UpdateProgress ID="udtpRepairDetail" runat="server">
        <ProgressTemplate>
            &nbsp;&nbsp;
      <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="upnlRepairDetail" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Panel ID="pnlNewRepair" Visible="false" runat="server">
                <h2>New Item for Repair</h2>
                <br />
                <table class="TblCoffee">
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
                        <td colspan="2">&nbsp;</td>
                    </tr>
                    <tr>
                        <td class="rowC" colspan="2" valign="middle" align="center">
                            <asp:Button ID="btnInsert" Text="Insert" runat="server" OnClick="btnInsert_Click" />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <asp:Button ID="btnCancelInsert" Text="Cancel" runat="server" OnClick="btnCancel_Click" />
                        </td>
                    </tr>
            </asp:Panel>
            <asp:Panel ID="pnlRepairDetail" runat="server">
                <table cellpadding="0" cellspacing="0" runat="server" class="TblCoffee">
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
                                DataTextField="ConditionDesc" DataValueField="MachineConditionID">
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
                            <asp:TextBox ID="tbxRepairFaultDesc" runat="server" Text='<%# Bind("RepairFaultDesc") %>'
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
                        <td>ID:<asp:Label ID="lblRepairID" runat="server" />&nbsp;&nbsp;OID:
                            <asp:Label ID="lblRelatedOrderID" runat="server" /></td>
                        <td>Logged:<asp:Label ID="lblDateLogged" runat="server" />&nbsp;&nbsp;
              Chng:<asp:Label ID="lblLastChanged" runat="server" /></td>
                    </tr>
                    <tr>
                        <td colspan="2" class="rowC" style="text-align: center">
                            <asp:Button ID="btnUpdateAndReturn" Text="Update & Return" runat="server" OnClick="btnUpdateAndReturn_Click" AccessKey="U" ToolTip="update and return (AltShftU)" />
                            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <asp:Button ID="btnDelete" Text="Delete" runat="server" OnClick="btnDelete_Click" />
                            &nbsp;&nbsp;&nbsp;
               <asp:Button ID="btnCancel" Text="Cancel" runat="server" OnClick="btnCancel_Click" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <div class="status-message"><asp:Literal ID="ltrlStatus" runat="server" /></div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <asp:ObjectDataSource ID="odsCompanys" runat="server" TypeName="TrackerDotNet.Controls.CompanyNames"
        SelectMethod="GetAll" OldValuesParameterFormatString="original_{0}"></asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsCompanyDemos" runat="server" TypeName="TrackerDotNet.Controls.CompanyNames"
        SelectMethod="GetAllDemo"
        OldValuesParameterFormatString="original_{0}"></asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsEquipTypes" runat="server" TypeName="TrackerDotNet.Controls.EquipTypeTbl"
        SortParameterName="SortBy" SelectMethod="GetAll"
        OldValuesParameterFormatString="original_{0}">
        <SelectParameters>
            <asp:Parameter DefaultValue="EquipTypeName" Name="SortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>

    <asp:ObjectDataSource ID="odsRepairFaults" runat="server" SelectMethod="GetAll"
        SortParameterName="SortBy" TypeName="TrackerDotNet.Controls.RepairFaultsTbl">
        <SelectParameters>
            <asp:Parameter DefaultValue="SortOrder" Name="SortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsRepairStatuses" runat="server" SortParameterName="SortBy"
        SelectMethod="GetAll" TypeName="TrackerDotNet.Controls.RepairStatusesTbl">
        <SelectParameters>
            <asp:Parameter DefaultValue="SortOrder" Name="SortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsMachineConditions" runat="server" SortParameterName="SortBy"
        SelectMethod="GetAll" TypeName="TrackerDotNet.Controls.MachineConditionsTbl">
        <SelectParameters>
            <asp:SessionParameter Name="SortBy" SessionField="SortOrder" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>


</asp:Content>

