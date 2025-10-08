<%@ Page Title="ReoccuringOrder Details" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="ReoccuringOrderDetails.aspx.cs" Inherits="TrackerDotNet.Pages.ReoccuringOrderDetails" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>

<asp:Content ID="cntReoccuringOrderDetailsHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntReoccuringOrderDetailsBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h2 class="InputFrm">Reoccuring Order Details</h2>
    <asp:Label ID="lblReoccuringOrderID" Visible="false" runat="server" />
    <asp:ScriptManager ID="smReoccuringOrderDetails" runat="server">
    </asp:ScriptManager>
    <asp:UpdateProgress ID="uprgReoccuringOrderDetails" runat="server" AssociatedUpdatePanelID="upnlReoccuringOrderDetails">
        <ProgressTemplate>
            <img src="../images/animi/BlueArrowsUpdate.gif" alt="updating" width="16" height="16" />updating.....
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="upnlReoccuringOrderDetails" runat="server">
        <ContentTemplate>
            <table class="TblMudZebra" cellpadding="0" cellspacing="4" style="font-size: large">
                <tr>
                    <td>Company Name</td>
                    <td colspan="3">
                        <asp:DropDownList ID="ddlCompanyName" runat="server" DataSourceID="sdsCompanys" DataTextField="CompanyName"
                            DataValueField="CustomerID" AppendDataBoundItems="true">
                            <asp:ListItem Value="0" Text="--- Select Contact Name ---" />
                        </asp:DropDownList>
                        <span class="rightColumn small">
                            <asp:Label ID="ReoccuringOrderIDLabel" runat="server" /></span>
                    </td>
                </tr>
                <tr>
                    <td>Value</td>
                    <td>
                        <asp:TextBox ID="ValueTextBox" runat="server" Text='<%# Bind("ReoccuranceValue") %>' Width="2em" /></td>
                    <td>Reoccurance Type</td>
                    <td>
                        <asp:DropDownList ID="ddlReoccuranceType" runat="server"
                            AppendDataBoundItems="True" DataSourceID="odsReoccuranceTypes"
                            DataTextField="Type" DataValueField="ID">
                            <asp:ListItem Value="0" Text="--- Select ---" />
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td>Item Type</td>
                    <td>
                        <asp:DropDownList ID="ddlItemType" runat="server" AppendDataBoundItems="True"
                            DataSourceID="odsItems" DataTextField="ItemDesc" DataValueField="ItemTypeID">
                            <asp:ListItem Text="--- Select ---" Value="0" />
                        </asp:DropDownList>
                    </td>
                    <td>Quantity Required</td>
                    <td>
                        <asp:TextBox ID="QuantityTextBox" runat="server" Text='<%# Bind("QtyRequired") %>' /></td>
                </tr>
                <tr>
                    <td>Until Date</td>
                    <td>
                        <asp:TextBox ID="UntilDateTextBox" runat="server" Text='<%# Bind("RequireUntilDate") %>' />
                        <ajaxToolkit:CalendarExtender ID="UntilDateTextBox_CalendarExtender" runat="server"
                            Enabled="True" TargetControlID="UntilDateTextBox" Format="dd/MM/yyyy" />
                    </td>
                    <td>Last Date  </td>
                    <td>
                        <asp:TextBox ID="LastDateTextBox" runat="server" Text='<%# Bind("DateLastDone") %>' />
                        <ajaxToolkit:CalendarExtender ID="LastDateTextBox_CalendarExtender" runat="server"
                            Enabled="True" TargetControlID="LastDateTextBox" Format="dd/MM/yyyy" />
                </tr>
                <tr>
                    <td>Packaging Type</td>
                    <td>
                        <asp:DropDownList ID="ddlPackagingTypes" runat="server" AppendDataBoundItems="True"
                            DataSourceID="odsPackagingTypes" DataTextField="Description" DataValueField="PackagingID">
                            <asp:ListItem Text="none" Value="0" />
                        </asp:DropDownList>
                    </td>
                    <td>Enabled</td>
                    <td>
                        <asp:CheckBox ID="EnabledCheckBox" runat="server" Text="enabled" TextAlign="Right" Checked='<%# Bind("enabled") %>' /><br />
                        <br />
                    </td>
                </tr>
                <tr>
                    <td>Notes</td>
                    <td colspan="2">
                        <asp:TextBox ID="NotesTextBox" runat="server" Text='<%# Bind("Notes") %>' TextMode="MultiLine"
                            Height="4em" Width="93%" /></td>
                    <td style="vertical-align: middle">NextDate:
                        <asp:Label ID="NextDateLabel" runat="server" Text='<%# Bind("NextDateRequired") %>' /></td>
                    <tr>
                        <td colspan="6" class="rowOddC">
                            <asp:Button ID="btnUpdate" Text="Update" runat="server" OnClick="btnUpdate_Click" />
                            &nbsp;&nbsp;&nbsp;
            <asp:Button ID="btnUpdateAndReturn" Text="Update & Return" runat="server" OnClick="btnUpdateAndReturn_Click" />
                            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            <asp:Button ID="btnInsert" Text="Insert" runat="server" Enabled="false" OnClick="btnInsert_Click" />
                            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            <asp:Button ID="btnDelete" runat="server" Text="Delete" OnClick="btnDelete_Click"
                OnClientClick="return confirm('Are you sure you want to delete this occurance?');" />
                            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            <asp:Button ID="btnCancel" Text="Cancel" runat="server" OnClick="btnCancel_Click" />
                        </td>
            </table>
            <div class="status-message">
                <asp:Literal ID="ltrlStatus" Text="" runat="server" /></div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <br />
    <asp:SqlDataSource ID="sdsCompanys" runat="server"
        ConnectionString="<%$ ConnectionStrings:Tracker08ConnectionString %>"
        ProviderName="<%$ ConnectionStrings:Tracker08ConnectionString.ProviderName %>"
        SelectCommand="SELECT [CompanyName], [CustomerID] FROM [CustomersTbl] ORDER BY [enabled], [CompanyName]"></asp:SqlDataSource>
    <asp:ObjectDataSource ID="odsItems" runat="server" TypeName="TrackerDotNet.Controls.ItemTypeTbl"
        SortParameterName="SortBy" SelectMethod="GetAll"
        OldValuesParameterFormatString="original_{0}">
        <SelectParameters>
            <asp:Parameter DefaultValue="[ItemEnabled], [SortOrder], ItemDesc" Name="SortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsReoccuranceTypes" runat="server" TypeName="TrackerDotNet.Controls.ReoccuranceTypeTbl"
        SelectMethod="GetAll" OldValuesParameterFormatString="original_{0}"></asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsPackagingTypes" runat="server" TypeName="TrackerDotNet.Controls.PackagingTbl"
        SortParameterName="SortBy" SelectMethod="GetAll"
        OldValuesParameterFormatString="original_{0}">
        <SelectParameters>
            <asp:Parameter DefaultValue="Description" Name="SortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>
</asp:Content>
