<%@ Page Title="Contact Away Detail" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="True" CodeBehind="ContactsAwayDetail.aspx.cs" Inherits="TrackerSQL.Pages.ContactsAwayDetail" %>

<asp:Content ID="cntContactsAwayDetailHdr" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        // When start date changes, auto-set end date to same value (if end date is empty or before start)
        function onStartDateChange(startDateInput) {
            var endDateInput = document.getElementById('<%= tbxAwayEndDate.ClientID %>');
            if (endDateInput) {
                var startVal = startDateInput.value;
                var endVal = endDateInput.value;
                // Set end date to start date if end is empty or end is before start
                if (!endVal || endVal < startVal) {
                    endDateInput.value = startVal;
                }
            }
        }
    </script>
</asp:Content>
<asp:Content ID="cntContactsAwayDetailBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Contact Away Period</h1>
    <asp:ScriptManager runat="server" ID="scrmContactsAwayDetail" />
    <asp:UpdateProgress ID="udtpContactsAwayDetail" runat="server">
        <ProgressTemplate>
            &nbsp;&nbsp;

            <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="upnlContactsAwayDetail" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Panel ID="pnlAwayDetail" runat="server">
                <table class="Tbl">
                    <tr>
                        <td>Contact</td>
                        <td>
                            <ajaxToolkit:ComboBox ID="cboCustomer" runat="server"
                                DataTextField="CompanyName"
                                DataValueField="ContactID"
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
                            <asp:TextBox ID="tbxAwayStartDate" runat="server" TextMode="Date" Width="150px" 
                                onchange="onStartDateChange(this);" />
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
    <%-- Removed ObjectDataSources - using manual binding in code-behind with SQL Server repositories --%>
</asp:Content>



