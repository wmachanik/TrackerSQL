<%@ Page Title="Messages Editor" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="MessagesEditor.aspx.cs"
    Inherits="TrackerSQL.Tools.MessagesEditor"
    ValidateRequest="false" %>

<asp:Content ID="HeadCnt" ContentPlaceHolderID="HeadContent" runat="server" />

<asp:Content ID="BodyCnt" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Messages Editor</h1>

    <asp:ScriptManager ID="smgrMessagesEditor" runat="server" />
    <asp:UpdateProgress ID="udpMessagesEditor" runat="server">
        <ProgressTemplate>
            &nbsp;&nbsp;<img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:Panel ID="pnlAccessDenied" runat="server" Visible="false" CssClass="error-panel">
        <asp:Literal ID="ltrlAccessDenied" runat="server"
            Text="You do not have permission to access this page." />
    </asp:Panel>

    <asp:UpdatePanel ID="upnlMessagesEditor" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
        <ContentTemplate>

            <asp:Panel ID="pnlEditor" runat="server" Visible="false">

                <!-- Search Panel -->
                <div class="filter-toolbar">
                    <div class="filter-section search-controls">
                        <div class="filter-control">
                            <label for="<%=tbxSearch.ClientID%>">Search:</label>
                            <asp:TextBox ID="tbxSearch" runat="server" Width="260" />
                        </div>
                        <asp:Button ID="btnSearch" runat="server" Text="Go" ToolTip="Search messages" OnClick="btnSearch_Click" />
                        <asp:Button ID="btnReset" runat="server" Text="Reset" ToolTip="Clear filter" OnClick="btnReset_Click" />
                    </div>
                </div>
                <div class="status-message">
                    <asp:Label ID="ResultsTitleLabel" runat="server" CssClass="status-info" Text="Messages" />
                    <br />
                    <asp:Literal ID="ltrlStatus" runat="server" />
                    <br />
                </div>

                <!-- Use standardized results container for controlled overflow -->
                <asp:Panel ID="pnlResultsSection" runat="server" CssClass="results-container results-panel" Visible="true">
                    <br />

                    <asp:GridView ID="gvMessages" runat="server"
                        AutoGenerateColumns="False"
                        DataKeyNames="Key"
                        CssClass="results-table"
                        AllowPaging="True"
                        PageSize="30"
                        AllowSorting="True"
                        OnPageIndexChanging="gvMessages_PageIndexChanging"
                        OnRowEditing="gvMessages_RowEditing"
                        OnRowCancelingEdit="gvMessages_RowCancelingEdit"
                        OnRowUpdating="gvMessages_RowUpdating"
                        OnRowDataBound="gvMessages_RowDataBound">

                        <Columns>
                            <asp:BoundField DataField="Key" HeaderText="Key" ReadOnly="True" SortExpression="Key">
                            </asp:BoundField>

                            <asp:TemplateField HeaderText="Value" SortExpression="Value">
                                <ItemTemplate>
                                    <div class="clamp-3">
                                        <asp:Label ID="lblValue" runat="server"
                                            Text='<%# HttpUtility.HtmlEncode(Eval("Value") as string) %>' />
                                    </div>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtEditValue" runat="server"
                                        Text='<%# Bind("Value") %>'
                                        TextMode="MultiLine" Rows="5" Width="100%"
                                        ValidateRequestMode="Disabled" />
                                </EditItemTemplate>
                                <ItemStyle CssClass="wrap" />
                            </asp:TemplateField>

                            <asp:TemplateField ShowHeader="False">
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
                        </Columns>

                        <PagerStyle CssClass="aspNetPager" />
                        <HeaderStyle CssClass="TblWhiteHeader" />
                        <RowStyle CssClass="TblRow" />
                        <AlternatingRowStyle CssClass="TblRowAlt" />
                        <EditRowStyle BackColor="#FFF5D6" />
                    </asp:GridView>
                </asp:Panel>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
