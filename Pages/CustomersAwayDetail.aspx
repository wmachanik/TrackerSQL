<%@ Page Title="Customer Away Detail" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="True" CodeBehind="CustomersAwayDetail.aspx.cs" Inherits="TrackerSQL.Pages.CustomersAwayDetail" %>

<asp:Content ID="cntCustomersAwayDetailHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntCustomersAwayDetailBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Customer Away Period</h1>
    <asp:ScriptManager runat="server" ID="scrmCustomersAwayDetail" />
    <asp:UpdateProgress ID="udtpCustomersAwayDetail" runat="server">
        <ProgressTemplate>
            &nbsp;&nbsp;
           
            <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="upnlCustomersAwayDetail" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Panel ID="pnlAwayDetail" runat="server">
                <table class="Tbl">
                    <tr>
                        <td>Customer</td>
                        <td>
                            <ajaxToolkit:ComboBox ID="cboCustomer" runat="server"
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
                        <td>Away Start</td>
                        <td>
                            <asp:TextBox ID="tbxAwayStartDate" runat="server" TextMode="Date" Width="150px" />
                        </td>
                    </tr>
                    <tr>
                        <td>Away End</td>
                        <td>
                            <asp:TextBox ID="tbxAwayEndDate" runat="server" TextMode="Date" Width="150px" />
                        </td>
                    </tr>
                    <tr>
                        <td>Reason</td>
                        <td>
                            <asp:DropDownList ID="ddlReason" runat="server"
                                DataSourceID="odsAwayReasons"
                                DataTextField="ReasonDesc"
                                DataValueField="AwayReasonID"
                                AppendDataBoundItems="true" Width="200px">
                                <asp:ListItem Text="--Select--" Value="0" />
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2" class="rowC" style="text-align: center; padding: 6px 8px">
                            <asp:Button ID="btnInsert" Text="Save" runat="server" OnClick="btnInsert_Click" />
                            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                           
                            <asp:Button ID="btnDelete" Text="Delete" runat="server" OnClick="btnDelete_Click" Visible="false"
                                OnClientClick="return confirm('Are you sure you want to delete this away period?');" />
                            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                           
                            <asp:Button ID="btnCancel" Text="Cancel" runat="server" OnClick="btnCancel_Click" />
                        </td>
                    </tr>
                </table>
                <div class="status-message"><asp:Literal ID="ltrlStatus" runat="server" /></div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:ObjectDataSource ID="odsCompanys" runat="server"
        SelectMethod="GetAllCustomerNames"
        TypeName="TrackerSQL.Controls.CustomersTbl" />
    <asp:ObjectDataSource ID="odsAwayReasons" runat="server"
        TypeName="TrackerSQL.Controls.CustomersAwayTbl"
        SelectMethod="GetAllAwayReasons" />
</asp:Content>
