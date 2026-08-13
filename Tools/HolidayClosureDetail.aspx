<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="HolidayClosureDetail.aspx.cs" Inherits="TrackerSQL.Tools.HolidayClosureDetail"
    Title="Closure Detail" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="cntHolidayClosureDetailHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="cntHolidayClosureDetailBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smClosureDetail" runat="server" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="upgClosureDetail" runat="server" AssociatedUpdatePanelID="upnlDetail"
        DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info page-tone-progress">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="upnlDetail" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
        <ContentTemplate>
            <asp:HiddenField ID="hdnClosureId" runat="server" Value="" />

            <asp:Panel ID="pnlDetail" runat="server" CssClass="simpleForm page-tone-panel page-tone-holiday">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/Calendar.gif" alt="" />
                    <div>
                        <h1 class="page-tone-title">
                            <asp:Literal ID="litPageTitle" runat="server" Text="Closure Detail" />
                        </h1>
                        <p class="page-tone-subtitle">
                            <asp:Literal ID="litPanelTitle" runat="server" Text="Add or edit a holiday / closure" />
                        </p>
                    </div>
                </div>

                <table class="detail-form-table" style="margin-top: 12px;">
                    <tr>
                        <td>Start Date</td>
                        <td>
                            <asp:TextBox ID="txtDate" runat="server" Width="140px" />
                            <asp:ImageButton ID="imgCalendarDate" runat="server"
                                ImageUrl="~/images/imgButtons/CalendarBtn.png"
                                AlternateText="Select date" ToolTip="Select date"
                                CausesValidation="false"
                                OnClientClick="return false;"
                                style="vertical-align: middle; margin-left: 4px;" />
                            <ajaxToolkit:CalendarExtender ID="calDetailDate" runat="server"
                                TargetControlID="txtDate" PopupButtonID="imgCalendarDate"
                                Format="yyyy-MM-dd" FirstDayOfWeek="Monday"
                                CssClass="calendar-popup" />
                        </td>
                    </tr>
                    <tr>
                        <td>Days Closed</td>
                        <td>
                            <asp:TextBox ID="txtDays" runat="server" Width="60px" Text="1" />
                        </td>
                    </tr>
                    <tr>
                        <td>Strategy</td>
                        <td>
                            <asp:DropDownList ID="ddlStrategy" runat="server" Width="140px">
                                <asp:ListItem>Forward</asp:ListItem>
                                <asp:ListItem>Backward</asp:ListItem>
                                <asp:ListItem>Skip</asp:ListItem>
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td>Applies</td>
                        <td>
                            <asp:CheckBox ID="chkPrep" runat="server" Text="Prep" Checked="true" />
                            &nbsp;&nbsp;
                            <asp:CheckBox ID="chkDelivery" runat="server" Text="Delivery" Checked="true" />
                        </td>
                    </tr>
                    <tr>
                        <td>Description</td>
                        <td>
                            <asp:TextBox ID="txtDesc" runat="server" Width="100%" MaxLength="255" />
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2" class="button-row">
                            <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="filter-panel-btn"
                                OnClick="btnSave_Click" CausesValidation="false"
                                ToolTip="Save this closure and stay on the page" />
                            <asp:Button ID="btnSaveReturn" runat="server" Text="Save &amp; Return" CssClass="filter-panel-btn"
                                OnClick="btnSaveReturn_Click" CausesValidation="false"
                                ToolTip="Save this closure and return to the list" />
                            <asp:Button ID="btnDelete" runat="server" Text="Delete" CssClass="filter-panel-btn"
                                OnClick="btnDelete_Click" CausesValidation="false"
                                OnClientClick="return confirm('Delete this closure?');"
                                ToolTip="Delete this closure" />
                            <span class="image-button" title="Return to holiday closures list without saving">
                                <asp:ImageButton ID="btnBack" runat="server"
                                    ImageUrl="~/images/imgButtons/Back.gif"
                                    AlternateText="Back"
                                    ToolTip="Return to holiday closures list without saving"
                                    OnClick="btnBack_Click"
                                    CausesValidation="false" />
                            </span>
                        </td>
                    </tr>
                </table>

                <div class="page-tone-footer">
                    <div class="status-message" id="pnlStatus" runat="server">
                        <asp:Literal ID="ltrlStatus" runat="server" />
                    </div>
                </div>
            </asp:Panel>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnSave" EventName="Click" />
            <asp:PostBackTrigger ControlID="btnSaveReturn" />
            <asp:PostBackTrigger ControlID="btnDelete" />
            <asp:PostBackTrigger ControlID="btnBack" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
