<%@ Page Title="System Data" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SystemData.aspx.cs" Inherits="TrackerSQL.Tools.SystemData" %>

<asp:Content ID="cntSystemDataHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="cntSystemDataBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h1>System Data</h1>
    
    <asp:ScriptManager ID="smSystemData" runat="server" EnablePartialRendering="true" />
    
    <asp:UpdateProgress ID="updtPrgSystemData" runat="server" AssociatedUpdatePanelID="upnlSystemData">
        <ProgressTemplate>
            <div style="text-align: left; padding: 10px;">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:ObjectDataSource ID="odsSysData" runat="server"
        TypeName="TrackerSQL.Tools.SystemData"
        SelectMethod="GetSystemDataForBinding"
        UpdateMethod="UpdateSystemData"
        DataObjectTypeName="TrackerSQL.Models.SysData">
    </asp:ObjectDataSource>

    <asp:ObjectDataSource ID="odsItemServiceTypes" runat="server"
        TypeName="TrackerSQL.Repositories.ItemServiceTypesRepository"
        SelectMethod="GetAll">
    </asp:ObjectDataSource>
    
    <asp:UpdatePanel ID="upnlSystemData" runat="server" ChildrenAsTriggers="true" UpdateMode="Always">
        <ContentTemplate>
            <div class="responsive-layout-container">
                <asp:DetailsView ID="dvSystemData" runat="server"
                    AutoGenerateRows="False"
                    CssClass="TblFlex table-auto-width"
                    DataSourceID="odsSysData"
                    OnItemUpdated="dvSystemData_ItemUpdated"
                    OnDataBound="dvSystemData_DataBound"
                    OnModeChanging="dvSystemData_ModeChanging"
                    DefaultMode="ReadOnly">
                    <Fields>
                        <asp:BoundField DataField="ID" HeaderText="ID" ReadOnly="true" />

                        <asp:CheckBoxField DataField="DoRecurringOrders"
                            HeaderText="Do Recurring Orders"
                            SortExpression="DoRecurringOrders" />

                        <asp:TemplateField HeaderText="Last Reoccurring Date" SortExpression="LastRecurringDate">
                            <ItemTemplate>
                                <%# Eval("LastRecurringDate", "{0:d}") ?? "(not set)" %>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="txtLastRecurringDate" runat="server" 
                                    Text='<%# Bind("LastRecurringDate", "{0:MM/dd/yyyy}") %>'
                                    Width="120px" />

                                <asp:ImageButton ID="imgCalendarRecurringDate" runat="server" ImageUrl="~/images/imgButtons/CalendarBtn.png"
                                    AlternateText="Select date" Style="vertical-align: middle;" />

                                <ajaxToolkit:calendarextender ID="calLastRecurringDate" runat="server"
                                    TargetControlID="txtLastRecurringDate" PopupButtonID="imgCalendarRecurringDate"
                                    Format="MM/dd/yyyy" CssClass="calendar-popup" />
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Date Last Prep Date Calculated" SortExpression="DateLastPrepDateCalcd">
                            <ItemTemplate>
                                <%# Eval("DateLastPrepDateCalcd", "{0:d}") ?? "(not set)" %>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="txtDateLastPrepDateCalcd" runat="server" 
                                    Text='<%# Bind("DateLastPrepDateCalcd", "{0:MM/dd/yyyy}") %>'
                                    Width="120px" />

                                <asp:ImageButton ID="imgCalendarPrepDate" runat="server" ImageUrl="~/images/imgButtons/CalendarBtn.png"
                                    AlternateText="Select date" Style="vertical-align: middle;" />

                                <ajaxtoolkit:calendarextender id="calDateLastPrepDateCalcd" runat="server"
                                    targetcontrolid="txtDateLastPrepDateCalcd" popupbuttonid="imgCalendarPrepDate"
                                    format="MM/dd/yyyy" cssclass="calendar-popup" />
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Min Reminder Date" SortExpression="MinReminderDate">
                            <ItemTemplate>
                                <%# Eval("MinReminderDate", "{0:d}") ?? "(not set)" %>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="txtMinReminderDate" runat="server" 
                                    Text='<%# Bind("MinReminderDate", "{0:MM/dd/yyyy}") %>'
                                    Width="120px" />

                                <asp:ImageButton ID="imgMinReminderDate" runat="server" ImageUrl="~/images/imgButtons/CalendarBtn.png"
                                    AlternateText="Select date" Style="vertical-align: middle;" />

                                <ajaxtoolkit:calendarextender id="calMinReminderDate" runat="server"
                                    targetcontrolid="txtMinReminderDate" popupbuttonid="imgMinReminderDate"
                                    format="MM/dd/yyyy"
                                    cssclass="calendar-popup" />
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Group Service Type (used for group items)" SortExpression="GroupItemServiceTypeID">
                            <ItemTemplate>
                                <%# GetItemServiceTypeName((int?)Eval("GroupItemServiceTypeID")) %>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:DropDownList ID="ddlGroupItemServiceTypeID" runat="server"
                                    DataSourceID="odsItemServiceTypes"
                                    DataTextField="ServiceTypeName"
                                    DataValueField="ItemServiceTypeID"
                                    SelectedValue='<%# Bind("GroupItemServiceTypeID") %>'
                                    AppendDataBoundItems="true">
                                    <asp:ListItem Value="" Text="(none)" />
                                </asp:DropDownList>
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField DataField="InternalContactIDs"
                            HeaderText="Internal Contact IDs"
                            SortExpression="InternalContactIDs"
                            NullDisplayText="(none)" />

                        <asp:CommandField ShowEditButton="True" ButtonType="Image"
                            EditImageUrl="~/images/imgButtons/EditItem.gif"
                            UpdateImageUrl="~/images/imgButtons/UpdateItem.gif"
                            CancelImageUrl="~/images/imgButtons/CancelItem.gif"
                            ItemStyle-HorizontalAlign="Center"
                            ItemStyle-CssClass="command-field-padding" />
                    </Fields>
                </asp:DetailsView>
            </div>

            <asp:Label ID="lblMessage" runat="server" ForeColor="Green" Visible="false"></asp:Label>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
