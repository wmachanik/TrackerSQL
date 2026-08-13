<%@ Page Title="Contact Away Detail" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="True"
    CodeBehind="ContactsAwayDetail.aspx.cs" Inherits="TrackerSQL.Pages.ContactsAwayDetail"
    MaintainScrollPositionOnPostback="true" %>

<asp:Content ID="cntContactsAwayDetailHdr" title="Contact Away Detail" ContentPlaceHolderID="HeadContent" runat="server">
    <%-- Keep HeadContent free of <%= %> — ScriptManager cannot modify <head> when it contains code blocks. --%>
</asp:Content>

<asp:Content ID="cntContactsAwayDetailBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager runat="server" ID="scrmContactsAwayDetail" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="udtpContactsAwayDetail" runat="server"
        AssociatedUpdatePanelID="upnlContactsAwayDetail" DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info page-tone-progress">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="upnlContactsAwayDetail" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnUpdate" EventName="Click" />
            <asp:PostBackTrigger ControlID="btnUpdateAndReturn" />
            <asp:PostBackTrigger ControlID="btnDelete" />
            <asp:PostBackTrigger ControlID="btnCancel" />
        </Triggers>
        <ContentTemplate>
            <asp:Panel ID="pnlAwayDetail" runat="server" CssClass="simpleForm page-tone-panel page-tone-contacts">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/CalendarClock.gif" alt="" />
                    <div>
                        <h1 class="page-tone-title">Contact Away Period</h1>
                        <p class="page-tone-subtitle">Add or edit an away period for a contact</p>
                    </div>
                </div>

                <table class="TblCoffee detail-form-table" cellpadding="0" cellspacing="0">
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
                        <td colspan="2" class="rowC" style="text-align: center; padding-top: 12px;">
                            <div class="button-row">
                                <asp:Button ID="btnUpdate" Text="Save" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnUpdate_Click" ToolTip="Save and stay on this page" />
                                <asp:Button ID="btnUpdateAndReturn" Text="Save &amp; Return" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnUpdateAndReturn_Click" ToolTip="Save and return to Contacts Away" />
                                <asp:Button ID="btnDelete" Text="Delete" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnDelete_Click" Visible="false"
                                    OnClientClick="return confirm('Are you sure you want to delete this away period?');"
                                    ToolTip="Delete this away period" />
                                <span class="image-button" title="Return without saving">
                                    <asp:ImageButton ID="btnCancel" runat="server"
                                        ImageUrl="~/images/imgButtons/Back.gif"
                                        AlternateText="Back"
                                        ToolTip="Return without saving"
                                        OnClick="btnCancel_Click"
                                        CausesValidation="false" />
                                </span>
                            </div>
                        </td>
                    </tr>
                </table>

                <div class="status-message" id="pnlStatus" runat="server" style="margin-top: 12px;">
                    <asp:Literal ID="ltrlStatus" runat="server" />
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>

    <script type="text/javascript">
        function onStartDateChange(startDateInput) {
            var endDateInput = document.getElementById('<%= tbxAwayEndDate.ClientID %>');
            if (endDateInput) {
                var startVal = startDateInput.value;
                var endVal = endDateInput.value;
                if (!endVal || endVal < startVal) {
                    endDateInput.value = startVal;
                }
            }
        }
    </script>
</asp:Content>
