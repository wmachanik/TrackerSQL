<%@ Page Title="Move Delivery Date" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="MoveDeliveryDate.aspx.cs" Inherits="TrackerSQL.Tools.MoveDeliveryDate" %>

<asp:Content ID="cntMoveDeliveryDateHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntMoveDeliveryDateBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smgrMoveDeliveryDate" runat="server" />

    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="upnlMoveDeliveryDate"
        DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info page-tone-progress">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="upnlMoveDeliveryDate" runat="server" ChildrenAsTriggers="true">
        <ContentTemplate>
            <asp:Panel ID="pnlMoveDeliveryDate" runat="server" CssClass="simpleForm page-tone-panel page-tone-move">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/MoveOnADay.gif" alt="" />
                    <div>
                        <h1 class="page-tone-title">Move Delivery Date</h1>
                        <p class="page-tone-subtitle">Shift delivery schedule</p>
                    </div>
                </div>

                <table class="TblCoffee">
                    <tbody>
                        <tr>
                            <td>Old Delivery Date</td>
                            <td>
                                <asp:DropDownList ID="OldDeliveryDateDDL" runat="server" DataTextFormatString="{0:d}"
                                    DataSourceID="odsAreaDeliveryDates" DataTextField="Date" DataValueField="Date"></asp:DropDownList>
                                <asp:ObjectDataSource ID="odsAreaDeliveryDates" runat="server"
                                    SelectMethod="GetAllDeliveryDates"
                                    TypeName="TrackerSQL.Managers.NextPrepDateDataSource">
                                </asp:ObjectDataSource>
                            </td>
                        </tr>
                        <tr>
                            <td>New Delivery Date</td>
                            <td>
                                <asp:TextBox ID="NewDeliveryDateTextBox" runat="server" Text="" />
                                <ajaxToolkit:CalendarExtender ID="NewDeliveryDateTextBox_CalendarExtender" runat="server" CssClass="small"
                                    Enabled="True" TargetControlID="NewDeliveryDateTextBox" ClearTime="true">
                                </ajaxToolkit:CalendarExtender>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2" align="center">
                                <asp:Button ID="btnMove" Text="Move" runat="server"
                                    OnClick="btnMove_Click" />&nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:Button ID="btnCancel" Text="Cancel" runat="server" />
                            </td>
                        </tr>
                    </tbody>
                </table>

                <asp:GridView ID="gvPrepData" runat="server" AllowPaging="True" CssClass="AutoWidthFrm results-table"
                    AutoGenerateColumns="False" AllowSorting="true" style="margin-top: 12px;">
                    <Columns>
                        <asp:BoundField DataField="Area" HeaderText="Area" SortExpression="Area" />
                        <asp:BoundField DataField="PreparationDate" DataFormatString="{0:d}"
                            HeaderText="Prep Date" SortExpression="PreparationDate" />
                        <asp:BoundField DataField="DeliveryDate" DataFormatString="{0:d}"
                            HeaderText="Delivery Date" SortExpression="DeliveryDate" />
                        <asp:BoundField DataField="NextPreparationDate" DataFormatString="{0:d}"
                            HeaderText="Next Prep Date" SortExpression="NextPreparationDate" />
                        <asp:BoundField DataField="NextDeliveryDate" DataFormatString="{0:d}"
                            HeaderText="Next Delivery Date" SortExpression="NextDeliveryDate" />
                    </Columns>
                </asp:GridView>

                <div class="page-tone-footer">
                    <div class="status-message" id="pnlStatus" runat="server">
                        <asp:Literal ID="StatusLiteral" Text="" runat="server" />
                    </div>
                </div>
            </asp:Panel>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnMove" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
