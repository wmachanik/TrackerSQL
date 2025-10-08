<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RepairDetailOld.aspx.cs" Inherits="TrackerDotNet.Pages.RepairDetailOld" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
      <asp:DetailsView ID="dvRepairDetail" runat="server" CssClass="TblWhite small" 
        CellPadding="0" DataKeyNames="RepairID" 
        AutoGenerateRows="False" DataSourceID="odsRepairDetail">
        <Fields>
          <asp:TemplateField HeaderText="RepairID" Visible="false">
            <EditItemTemplate>
              <asp:Label ID="lblRepairID" runat="server" Text='<%# Eval("RepairID") %>'></asp:Label>
            </EditItemTemplate>
            <ItemTemplate>
              <asp:Label ID="lblRepairID" runat="server" Text='<%# Bind("RepairID") %>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="CustomerID">
            <EditItemTemplate>
              <asp:DropDownList ID="ddlCompany" runat="server" DataSourceID="odsCompanys" 
                DataTextField="CompanyName" DataValueField="CustomerID" 
                SelectedValue='<%# Bind("CustomerID") %>'>
              </asp:DropDownList>
            </EditItemTemplate>
            <ItemTemplate>
              <asp:DropDownList ID="ddlCompany" runat="server" DataSourceID="odsCompanys" 
                DataTextField="CompanyName" DataValueField="CustomerID" 
                SelectedValue='<%# Bind("CustomerID") %>'>
              </asp:DropDownList>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="ContactName">
            <EditItemTemplate>
              <asp:TextBox ID="tbxContactName" runat="server" Text='<%# Bind("ContactName") %>'></asp:TextBox>
            </EditItemTemplate>
            <ItemTemplate>
              <asp:Label ID="lblContactName" runat="server" Text='<%# Bind("ContactName") %>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="ContactEmail">
            <EditItemTemplate>
              <asp:TextBox ID="tbxContactEmail" runat="server" Text='<%# Bind("ContactEmail") %>'></asp:TextBox>
            </EditItemTemplate>
            <ItemTemplate>
              <asp:Label ID="lblContactEmail" runat="server" Text='<%# Bind("ContactEmail") %>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="JobCardNumber" SortExpression="JobCardNumber">
            <EditItemTemplate>
              <asp:TextBox ID="tbxJobCardNumber" runat="server" Text='<%# Bind("JobCardNumber") %>' />
            </EditItemTemplate>
            <ItemTemplate>
              <asp:Label ID="lblJobCardNumber" runat="server" Text='<%# Bind("JobCardNumber") %>' />
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="DateLogged" Visible="False">
            <EditItemTemplate>
              <asp:TextBox ID="tbxDateLogged" runat="server" Text='<%# Bind("DateLogged") %>'></asp:TextBox>
            </EditItemTemplate>
            <ItemTemplate>
              <asp:Label ID="lblDateLogged" runat="server" Text='<%# Bind("DateLogged") %>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="LastStatusChange" Visible="False">
            <EditItemTemplate>
              <asp:TextBox ID="tbxLastStatusChange" runat="server" 
                Text='<%# Bind("LastStatusChange") %>'></asp:TextBox>
            </EditItemTemplate>
            <ItemTemplate>
              <asp:Label ID="lblLastStatusChange" runat="server" Text='<%# Bind("LastStatusChange") %>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="MachineTypeID">
            <EditItemTemplate>
              <asp:DropDownList ID="ddlEquipTypes" runat="server" AppendDataBoundItems="True" 
                DataSourceID="odsEquipTypes" DataTextField="EquipTypeName" DataValueField="EquipTypeId"  >
                <asp:ListItem Text="none" Value="0" />
              </asp:DropDownList>
            </EditItemTemplate>
            <ItemTemplate>
              <asp:DropDownList ID="ddlEquipTypes" runat="server" AppendDataBoundItems="True" 
                DataSourceID="odsEquipTypes" DataTextField="EquipTypeName" DataValueField="EquipTypeId"  >
                <asp:ListItem Text="none" Value="0" />
              </asp:DropDownList>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:BoundField DataField="MachineSerialNumber" 
            HeaderText="MachineSerialNumber" />
          <asp:TemplateField HeaderText="SwopOutMachineID">
            <EditItemTemplate>
              <asp:DropDownList ID="ddlSwopOutMachine" runat="server" DataSourceID="odsCompanyDemos" 
                DataTextField="CompanyName" DataValueField="CustomerID" AppendDataBoundItems="true">
                <asp:ListItem Text="none" Value="0" />
              </asp:DropDownList>
            </EditItemTemplate>
            <ItemTemplate>
              <asp:DropDownList ID="ddlSwopOutMachine" runat="server" DataSourceID="odsCompanyDemos" 
                DataTextField="CompanyName" DataValueField="CustomerID" AppendDataBoundItems="true">
                <asp:ListItem Text="none" Value="0" />
              </asp:DropDownList>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="MachineConditionID">
            <EditItemTemplate>
              <asp:TextBox ID="TextBox4" runat="server" 
                Text='<%# Bind("MachineConditionID") %>'></asp:TextBox>
            </EditItemTemplate>
            <ItemTemplate>
              <asp:Label ID="Label4" runat="server" Text='<%# Bind("MachineConditionID") %>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="FrotherTaken">
            <EditItemTemplate>
              <asp:CheckBox ID="cbxTakenFrother" runat="server" Checked='<%# Bind("TakenFrother") %>'
                Text="Frother" TextAlign="Left" />&nbsp;&nbsp;&nbsp;&nbsp;
            </EditItemTemplate>
            <ItemTemplate>
              <asp:CheckBox ID="cbxTakenFrother" runat="server" Checked='<%# Bind("TakenFrother") %>'
                Text="Frother" TextAlign="Left" />&nbsp;&nbsp;&nbsp;&nbsp;
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="BeanLidTaken">
            <EditItemTemplate>
              <asp:CheckBox ID="cbxTakenBeanLid" runat="server" Checked='<%# Bind("TakenBeanLid") %>' 
                Text="BeanLid" TextAlign="Left" />&nbsp;&nbsp;&nbsp;&nbsp;
            </EditItemTemplate>
            <ItemTemplate>
              <asp:CheckBox ID="cbxTakenBeanLid" runat="server" Checked='<%# Bind("TakenBeanLid") %>'
                Text="BeanLid" TextAlign="Left" />
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="WaterLidTaken">
            <EditItemTemplate>
              <asp:CheckBox ID="cbxWaterLidTaken" runat="server" Checked='<%# Bind("TakenWaterLid") %>'
                Text="WaterLid" TextAlign="Left" />
            </EditItemTemplate>
            <ItemTemplate>
              <asp:CheckBox ID="cbxWaterLidTaken" runat="server" Checked='<%# Bind("TakenWaterLid") %>'
                Text="WaterLid" TextAlign="Left" />
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="BrokenFrother">
            <EditItemTemplate>
              <asp:CheckBox ID="CheckBox4" runat="server" 
                Checked='<%# Bind("BrokenFrother") %>' />
            </EditItemTemplate>
            <ItemTemplate>
              <asp:CheckBox ID="CheckBox4" runat="server" 
                Checked='<%# Bind("BrokenFrother") %>' Enabled="false" />
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="BrokenBeanLid">
            <EditItemTemplate>
              <asp:CheckBox ID="CheckBox5" runat="server" 
                Checked='<%# Bind("BrokenBeanLid") %>' />
            </EditItemTemplate>
            <ItemTemplate>
              <asp:CheckBox ID="CheckBox5" runat="server" 
                Checked='<%# Bind("BrokenBeanLid") %>' Enabled="false" />
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="BrokenWaterLid">
            <EditItemTemplate>
              <asp:CheckBox ID="CheckBox6" runat="server" 
                Checked='<%# Bind("BrokenWaterLid") %>' />
            </EditItemTemplate>
            <ItemTemplate>
              <asp:CheckBox ID="CheckBox6" runat="server" 
                Checked='<%# Bind("BrokenWaterLid") %>' Enabled="false" />
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="RepairFaultID">
            <EditItemTemplate>
              <asp:TextBox ID="TextBox5" runat="server" Text='<%# Bind("RepairFaultID") %>'></asp:TextBox>
            </EditItemTemplate>
            <ItemTemplate>
              <asp:DropDownList ID="ddlRepairFault" runat="server" AppendDataBoundItems="True" 
                DataSourceID="odsRepairFaults" DataTextField="RepairFaultDesc" DataValueField="RepairFaultID" 
                SelectedValue='<%# Bind("RepairFaultID") %>'>
                <asp:ListItem Text="none" Value="0" />
              </asp:DropDownList>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="RepairFaultDesc">
            <EditItemTemplate>
              <asp:TextBox ID="tbxRepairFaultDesc" runat="server" Text='<%# Bind("RepairFaultDesc") %>' 
                TextMode="MultiLine" Width="98%" />
            </EditItemTemplate>
            <ItemTemplate>
              <asp:Label ID="lblRepairFaultDesc" runat="server" Text='<%# Bind("RepairFaultDesc") %>' />
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="RepairStatusID">
            <EditItemTemplate>
              <asp:DropDownList ID="ddlRepairStatuses" runat="server" AppendDataBoundItems="True" 
                DataSourceID="odsRepairStatuses" DataTextField="RepairStatusDesc" DataValueField="RepairStatusID" 
                SelectedValue='<%# Bind("RepairStatusID") %>' >
                <asp:ListItem Text="none" Value="0" />
              </asp:DropDownList>
            </EditItemTemplate>
            <ItemTemplate>
              <asp:DropDownList ID="ddlRepairStatuses" runat="server" AppendDataBoundItems="True" 
                DataSourceID="odsRepairStatuses" DataTextField="RepairStatusDesc" DataValueField="RepairStatusID" 
                SelectedValue='<%# Bind("RepairStatusID") %>' >
                <asp:ListItem Text="none" Value="0" />
              </asp:DropDownList>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="RelatedOrderID" Visible="False">
            <EditItemTemplate>
              <asp:TextBox ID="TextBox7" runat="server" Text='<%# Bind("RelatedOrderID") %>'></asp:TextBox>
            </EditItemTemplate>
            <ItemTemplate>
              <asp:Label ID="Label6" runat="server" Text='<%# Bind("RelatedOrderID") %>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Notes">
            <EditItemTemplate>
              <asp:TextBox ID="tbxNotes" runat="server" Text='<%# Bind("Notes") %>' TextMode="MultiLine"
                Width="98%" />
            </EditItemTemplate>
            <ItemTemplate>
              <asp:Label ID="lblNotes" runat="server" Text='<%# Bind("Notes") %>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:CommandField ShowEditButton="True" />
        </Fields>
        <EmptyDataTemplate>
          <h2>New Item for Repair</h2>
          <br />
          <table class="TblDetailZebra">
            <tr>
              <td>Customer</td>
              <td>
              <asp:DropDownList ID="ddlNewCompany" runat="server" DataSourceID="odsCompanys" 
                DataTextField="CompanyName" DataValueField="CustomerID" AppendDataBoundItems="true"  >
                  <asp:ListItem Selected="True" Value="0">none</asp:ListItem>
                </asp:DropDownList>
              </td>
            </tr>
            <tr>
              <td colspan="2">&nbsp;</td>
            </tr>
            <tr>
              <td class="rowC" colspan="2" valign="middle" align="center">
                <asp:Button ID="btnInsert" Text="Insert" runat="server" OnClick="btnInsert_Click" />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <asp:Button ID="btnCancel" Text="Cancel" runat="server" />
              </td>
            </tr>
        </EmptyDataTemplate>
      </asp:DetailsView>
      <asp:Label ID="lblThisRepairID" runat="server" Visible="true" />
	  
      <asp:ObjectDataSource ID="odsRepairDetail" runat="server" SelectMethod="GetRepairById" 
        TypeName="TrackerDotNet.Controls.RepairsTbl" UpdateMethod="UpdateRepair" 
        OnUpdating="RepairUpdating" OnUpdated="RowUpdated" OldValuesParameterFormatString="original_{0}" 
        DataObjectTypeName="TrackerDotNet.Controls.RepairsTbl" 
        InsertMethod="InsertRepair" DeleteMethod="DeleteRepair">
        <DeleteParameters>
          <asp:Parameter Name="RepairID" Type="Int32" />
        </DeleteParameters>
        <SelectParameters>
          <asp:ControlParameter ControlID="lblThisRepairID" DefaultValue="0" Name="pRepairID" 
            PropertyName="Text" Type="Int32" />
        </SelectParameters>
        <UpdateParameters>
          <asp:Parameter Name="RepairItem" Type="Object" />
          <asp:Parameter Name="orig_RepairID" Type="Int32" />
        </UpdateParameters>
      </asp:ObjectDataSource>    
      <asp:ObjectDataSource ID="odsCompanys" runat="server" TypeName="TrackerDotNet.Controls.CompanyNames"
        SelectMethod="GetAll" OldValuesParameterFormatString="original_{0}">
      </asp:ObjectDataSource>
      <asp:ObjectDataSource ID="odsCompanyDemos" runat="server" TypeName="TrackerDotNet.Controls.CompanyNames"
        SelectMethod="GetAllDemo"
        OldValuesParameterFormatString="original_{0}">
      </asp:ObjectDataSource>
      <asp:ObjectDataSource ID="odsEquipTypes" runat="server" TypeName="TrackerDotNet.Controls.EquipTypeTbl"
          SortParameterName="SortBy" SelectMethod="GetAll"
          OldValuesParameterFormatString="original_{0}">
          <SelectParameters>
            <asp:Parameter DefaultValue="EquipTypeName" Name="SortBy" Type="String" />
          </SelectParameters>
      </asp:ObjectDataSource>

      <asp:ObjectDataSource ID="odsRepairFaults" runat="server" SelectMethod="GetAll" 
        SortParameterName="SortBy" TypeName="TrackerDotNet.Controls.RepairsTbl">
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

    </div>
  </form>
</body>
</html>
