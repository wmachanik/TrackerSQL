<%@ Page Title="Delivery Promise Rules" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="DeliveryPromiseRules.aspx.cs" Inherits="TrackerSQL.Tools.DeliveryPromiseRules"
    MaintainScrollPositionOnPostback="true" %>

<asp:Content ID="cntDeliveryPromiseHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="cntDeliveryPromiseBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smDeliveryPromise" runat="server" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="upgDeliveryPromise" runat="server" AssociatedUpdatePanelID="upnlDeliveryPromise"
        DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info page-tone-progress">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:Panel ID="pnlAccessDenied" runat="server" Visible="false" CssClass="status-message status-error">
        <asp:Label ID="lblAccessDenied" runat="server" />
    </asp:Panel>

    <asp:UpdatePanel ID="upnlDeliveryPromise" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <asp:Panel ID="pnlMain" runat="server" CssClass="simpleForm page-tone-panel page-tone-tools">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/Calendar.gif" alt="" />
                    <div>
                        <h1 class="page-tone-title">Delivery Promise Rules</h1>
                        <p class="page-tone-subtitle">Website-promised delivery / dispatch windows by area (customer SLA)</p>
                    </div>
                </div>

                <div class="page-tone-toolbar filter-toolbar">
                    <div class="filter-section search-controls">
                        <div class="filter-control">
                            <asp:Label ID="lblSearch" runat="server" Text="Search:" AssociatedControlID="txtSearch" CssClass="small" />
                            <asp:TextBox ID="txtSearch" runat="server" Width="220" ToolTip="Group, area, notes, kind…" />
                        </div>
                        <div class="filter-control">
                            <asp:Label ID="lblFilterGroup" runat="server" Text="Group:" AssociatedControlID="ddlFilterGroup" CssClass="small" />
                            <asp:DropDownList ID="ddlFilterGroup" runat="server" />
                        </div>
                        <div class="filter-control">
                            <asp:CheckBox ID="chkShowDisabled" runat="server" Text="Show disabled" />
                        </div>
                        <div class="filter-control">
                            <asp:Button ID="btnSearch" runat="server" Text="Go" CssClass="filter-panel-btn"
                                OnClick="btnSearch_Click" CausesValidation="false" />
                            <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="filter-panel-btn"
                                OnClick="btnReset_Click" CausesValidation="false" />
                        </div>
                    </div>
                    <div class="filter-section action-buttons">
                        <asp:Button ID="btnShowAddPanel" runat="server" Text="New rule" CssClass="filter-panel-btn"
                            OnClick="btnShowAddPanel_Click" CausesValidation="false" />
                    </div>
                </div>

                <p class="woo-map-section-note">
                    Order-window → promised day. DOW: Sun–Sat, or <strong>AnyWD</strong> (courier time-of-day).
                    Times are 24h; end is exclusive.
                </p>

                <asp:Panel ID="pnlAddInline" runat="server" CssClass="simpleForm delivery-promise-add" Visible="false">
                    <fieldset>
                        <legend>New rule</legend>
                        <div class="filter-section">
                            <div class="filter-control">
                                <asp:Label runat="server" Text="Group" AssociatedControlID="txtNewGroup" CssClass="small" />
                                <asp:TextBox ID="txtNewGroup" runat="server" Width="140" />
                            </div>
                            <div class="filter-control">
                                <asp:Label runat="server" Text="Area name" AssociatedControlID="txtNewAreaName" CssClass="small" />
                                <asp:TextBox ID="txtNewAreaName" runat="server" Width="140" />
                            </div>
                            <div class="filter-control">
                                <asp:Label runat="server" Text="Area" AssociatedControlID="ddlNewArea" CssClass="small" />
                                <asp:DropDownList ID="ddlNewArea" runat="server" />
                            </div>
                            <div class="filter-control">
                                <asp:Label runat="server" Text="Start" AssociatedControlID="ddlNewStartDow" CssClass="small" />
                                <asp:DropDownList ID="ddlNewStartDow" runat="server" />
                                <asp:TextBox ID="txtNewStartTime" runat="server" Width="48" Text="12:00" ToolTip="HH:mm" />
                            </div>
                            <div class="filter-control">
                                <asp:Label runat="server" Text="End" AssociatedControlID="ddlNewEndDow" CssClass="small" />
                                <asp:DropDownList ID="ddlNewEndDow" runat="server" />
                                <asp:TextBox ID="txtNewEndTime" runat="server" Width="48" Text="12:00" ToolTip="HH:mm" />
                            </div>
                            <div class="filter-control">
                                <asp:Label runat="server" Text="Kind" AssociatedControlID="ddlNewKind" CssClass="small" />
                                <asp:DropDownList ID="ddlNewKind" runat="server">
                                    <asp:ListItem Selected="True">Delivery</asp:ListItem>
                                    <asp:ListItem>Dispatch</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="filter-control">
                                <asp:Label runat="server" Text="Result" AssociatedControlID="ddlNewResultMode" CssClass="small" />
                                <asp:DropDownList ID="ddlNewResultMode" runat="server">
                                    <asp:ListItem Value="FixedDow" Text="Fixed day" Selected="True" />
                                    <asp:ListItem Value="SameWorkday" Text="Same workday" />
                                    <asp:ListItem Value="NextWorkday" Text="Next workday" />
                                </asp:DropDownList>
                                <asp:DropDownList ID="ddlNewResultDow" runat="server" ToolTip="Result day for Fixed day" />
                            </div>
                            <div class="filter-control">
                                <asp:Button ID="btnAddRule" runat="server" Text="Add" CssClass="filter-panel-btn"
                                    OnClick="btnAddRule_Click" CausesValidation="false" />
                                <asp:Button ID="btnCancelAdd" runat="server" Text="Cancel" CssClass="filter-panel-btn"
                                    OnClick="btnCancelAdd_Click" CausesValidation="false" />
                            </div>
                        </div>
                    </fieldset>
                </asp:Panel>

                <div class="results-container">
                    <asp:GridView ID="gvRules" runat="server" CssClass="results-table delivery-promise-grid"
                        AutoGenerateColumns="false" DataKeyNames="RuleID"
                        AllowPaging="true" PageSize="25" AllowSorting="true"
                        OnPageIndexChanging="gvRules_PageIndexChanging"
                        OnSorting="gvRules_Sorting"
                        OnRowEditing="gvRules_RowEditing"
                        OnRowCancelingEdit="gvRules_RowCancelingEdit"
                        OnRowUpdating="gvRules_RowUpdating"
                        OnRowDeleting="gvRules_RowDeleting"
                        OnRowDataBound="gvRules_RowDataBound"
                        EmptyDataText="No delivery promise rules found. Open Woo Mapping once to seed from XML.">
                        <Columns>
                            <asp:BoundField DataField="RuleID" HeaderText="ID" ReadOnly="true"
                                SortExpression="RuleID" ItemStyle-CssClass="col-tight" HeaderStyle-CssClass="col-tight" />

                            <asp:TemplateField HeaderText="Group" SortExpression="RuleGroup">
                                <ItemTemplate><%# Eval("RuleGroup") %></ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtRuleGroup" runat="server" Text='<%# Bind("RuleGroup") %>' Width="110" />
                                </EditItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Area" SortExpression="AreaMatchName">
                                <ItemTemplate>
                                    <span class="delivery-promise-area"><%# FormatAreaDisplay(Container.DataItem) %></span>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtAreaMatchName" runat="server" Text='<%# Bind("AreaMatchName") %>' Width="120" ToolTip="Match name" /><br />
                                    <asp:DropDownList ID="ddlArea" runat="server" />
                                </EditItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Order window" SortExpression="SortOrder">
                                <ItemTemplate>
                                    <span class="delivery-promise-window"><%# FormatWindowRange(Container.DataItem) %></span>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:DropDownList ID="ddlStartDow" runat="server" />
                                    <asp:TextBox ID="txtStartTime" runat="server" Width="48" ToolTip="HH:mm"
                                        Text='<%# FormatTime(Eval("WindowStartMinutes")) %>' />
                                    <span class="delivery-promise-sep">–</span>
                                    <asp:DropDownList ID="ddlEndDow" runat="server" />
                                    <asp:TextBox ID="txtEndTime" runat="server" Width="48" ToolTip="HH:mm"
                                        Text='<%# FormatTime(Eval("WindowEndMinutes")) %>' />
                                    <div class="delivery-promise-edit-meta">
                                        Sort
                                        <asp:TextBox ID="txtSortOrder" runat="server" Text='<%# Bind("SortOrder") %>' Width="40" />
                                    </div>
                                </EditItemTemplate>
                                <ItemStyle CssClass="wrap" />
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Promise" SortExpression="PromiseKind">
                                <ItemTemplate>
                                    <span class="delivery-promise-result"><%# FormatPromise(Container.DataItem) %></span>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:DropDownList ID="ddlPromiseKind" runat="server">
                                        <asp:ListItem>Delivery</asp:ListItem>
                                        <asp:ListItem>Dispatch</asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:DropDownList ID="ddlResultMode" runat="server">
                                        <asp:ListItem Value="FixedDow" Text="Fixed day" />
                                        <asp:ListItem Value="SameWorkday" Text="Same workday" />
                                        <asp:ListItem Value="NextWorkday" Text="Next workday" />
                                    </asp:DropDownList>
                                    <asp:DropDownList ID="ddlResultDow" runat="server" ToolTip="Result day" />
                                </EditItemTemplate>
                                <ItemStyle CssClass="wrap" />
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Flags">
                                <ItemTemplate>
                                    <%# FormatFlags(Eval("NoThursdayDispatch"), Eval("WedAfterNoonToFriday"), Eval("Enabled")) %>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:CheckBox ID="chkNoThu" runat="server" Text="No Thu" Checked='<%# Bind("NoThursdayDispatch") %>' /><br />
                                    <asp:CheckBox ID="chkWedFri" runat="server" Text="Wed→Fri" Checked='<%# Bind("WedAfterNoonToFriday") %>' /><br />
                                    <asp:CheckBox ID="chkEnabled" runat="server" Text="On" Checked='<%# Bind("Enabled") %>' />
                                </EditItemTemplate>
                                <ItemStyle CssClass="col-tight" />
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Notes" SortExpression="Notes">
                                <ItemTemplate>
                                    <div class="clamp-3 delivery-promise-notes"><%# Eval("Notes") %></div>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtNotes" runat="server" Text='<%# Bind("Notes") %>'
                                        TextMode="MultiLine" Rows="2" Width="160" />
                                </EditItemTemplate>
                                <ItemStyle CssClass="wrap" />
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="" HeaderStyle-CssClass="col-tight" ItemStyle-CssClass="col-tight col-cmd">
                                <ItemTemplate>
                                    <asp:ImageButton ID="btnEdit" runat="server" CommandName="Edit"
                                        ImageUrl="~/images/imgButtons/EditItem.gif" AlternateText="Edit" ToolTip="Edit" />
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:ImageButton ID="btnUpdate" runat="server" CommandName="Update" CausesValidation="false"
                                        ImageUrl="~/images/imgButtons/UpdateItem.gif" AlternateText="Save" ToolTip="Save" />
                                    &nbsp;
                                    <asp:ImageButton ID="btnCancel" runat="server" CommandName="Cancel"
                                        ImageUrl="~/images/imgButtons/CancelItem.gif" AlternateText="Cancel" ToolTip="Cancel" />
                                </EditItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="" HeaderStyle-CssClass="col-tight" ItemStyle-CssClass="col-tight">
                                <ItemTemplate>
                                    <asp:ImageButton ID="btnDelete" runat="server"
                                        ImageUrl="~/images/imgButtons/DelItem.gif"
                                        AlternateText="Delete" ToolTip="Delete rule"
                                        CommandName="Delete" CausesValidation="false"
                                        OnClientClick="return confirm('Delete this delivery promise rule?');" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>

                        <PagerStyle CssClass="aspNetPager" />
                        <HeaderStyle CssClass="TblWhiteHeader" />
                        <RowStyle CssClass="TblRow" />
                        <AlternatingRowStyle CssClass="TblRowAlt" />
                        <EditRowStyle BackColor="#FFF5D6" />
                    </asp:GridView>
                </div>

                <div class="page-tone-footer button-row">
                    <span class="image-button" title="Back to System Tools">
                        <asp:ImageButton ID="btnBack" runat="server"
                            ImageUrl="~/images/imgButtons/Back.gif"
                            AlternateText="Back"
                            PostBackUrl="~/Tools/SystemTools.aspx"
                            CausesValidation="false" />
                    </span>
                    <div class="status-message" id="pnlStatus" runat="server">
                        <asp:Literal ID="ltrlStatus" runat="server" />
                    </div>
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
