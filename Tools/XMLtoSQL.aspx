<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="XMLtoSQL.aspx.cs" Inherits="TrackerSQL.Tools.XMLtoSQL"
    Title="XML to SQL" %>

<asp:Content ID="cntXMLtoSQLHdr" title="XML to SQL" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        function selectFile(filePath) {
            var tb = document.getElementById('<%= FileNameTextBox.ClientID %>');
            if (tb) tb.value = filePath;
        }
    </script>
</asp:Content>

<asp:Content ID="cntXMLtoSQLBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smXmlToSql" runat="server" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="upgXmlToSql" runat="server" AssociatedUpdatePanelID="upnlXmlToSql" DisplayAfter="0">
        <ProgressTemplate>
            <div class="status-message status-info page-tone-progress">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="upnlXmlToSql" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <asp:Panel ID="pnlMain" runat="server" CssClass="simpleForm page-tone-panel page-tone-xml">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/Table.png" alt="" />
                    <div>
                        <h1 class="page-tone-title">XML to SQL</h1>
                        <p class="page-tone-subtitle">Run XML migration packs or ad-hoc SQL against SQL Server</p>
                    </div>
                </div>

                <div class="sys-prefs-tabs-wrap" style="margin-bottom: 12px;">
                    <nav class="sys-prefs-tabs" aria-label="XML to SQL modes">
                        <asp:LinkButton ID="btnTabXml" runat="server" CssClass="sys-prefs-tab" Text="XML packs"
                            OnClick="btnTabXml_Click" CausesValidation="false" />
                        <asp:LinkButton ID="btnTabManual" runat="server" CssClass="sys-prefs-tab" Text="Manual SQL"
                            OnClick="btnTabManual_Click" CausesValidation="false" />
                    </nav>
                </div>

                <asp:MultiView ID="mvModes" runat="server" ActiveViewIndex="0">
                    <asp:View ID="viewXml" runat="server">
                        <table class="detail-form-table" style="width: 100%;">
                            <tr>
                                <td style="width: 4.5rem; white-space: nowrap;">File</td>
                                <td style="width: 100%;">
                                    <asp:TextBox ID="FileNameTextBox" runat="server"
                                        CssClass="xmltosql-filepath" Width="100%"
                                        style="width: 100%; box-sizing: border-box;" />
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2" class="button-row">
                                    <asp:Button ID="RefreshFilesButton" runat="server" Text="Refresh Files"
                                        CssClass="filter-panel-btn" OnClick="RefreshFilesButton_Click"
                                        CausesValidation="false" ToolTip="Reload XML files from App_Data" />
                                    <asp:Button ID="GoButton" runat="server" Text="Execute XML"
                                        CssClass="filter-panel-btn" OnClick="GoButton_Click"
                                        CausesValidation="false"
                                        OnClientClick="return confirm('Execute all enabled commands in this XML against SQL Server?');"
                                        ToolTip="Run commands from the selected XML file" />
                                    <span class="image-button" title="Return to System Tools">
                                        <asp:ImageButton ID="btnBack" runat="server"
                                            ImageUrl="~/images/imgButtons/Back.gif"
                                            AlternateText="Back"
                                            ToolTip="Return to System Tools"
                                            OnClick="btnBack_Click"
                                            CausesValidation="false" />
                                    </span>
                                </td>
                            </tr>
                        </table>

                        <asp:Panel ID="pnlFileBrowser" runat="server" CssClass="file-browser">
                            <strong>Available XML files in App_Data</strong><br />
                            <asp:Literal ID="ltrlFileList" runat="server" />
                        </asp:Panel>
                    </asp:View>

                    <asp:View ID="viewManual" runat="server">
                        <p class="page-tone-subtitle" style="margin: 0 0 8px 0;">
                            Run a single SELECT / INSERT / UPDATE / DELETE (or CREATE / ALTER / DROP / EXEC) statement.
                            Results appear in the grids below.
                        </p>
                        <table class="detail-form-table" style="width: 100%;">
                            <tr>
                                <td style="width: 4.5rem; white-space: nowrap; vertical-align: top; padding-top: 8px;">SQL</td>
                                <td style="width: 100%;">
                                    <asp:TextBox ID="tbxManualSql" runat="server" TextMode="MultiLine" Rows="10"
                                        CssClass="sys-prefs-input" Width="100%"
                                        style="width: 100%; box-sizing: border-box; font-family: Consolas, monospace;"
                                        SpellCheck="false"
                                        ToolTip="One SQL statement against the Tracker database" />
                                </td>
                            </tr>
                            <tr>
                                <td style="white-space: nowrap;">Type</td>
                                <td>
                                    <asp:DropDownList ID="ddlManualType" runat="server" CssClass="sys-prefs-input">
                                        <asp:ListItem Value="auto" Text="Auto-detect" Selected="True" />
                                        <asp:ListItem Value="select" Text="SELECT" />
                                        <asp:ListItem Value="insert" Text="INSERT" />
                                        <asp:ListItem Value="update" Text="UPDATE" />
                                        <asp:ListItem Value="delete" Text="DELETE" />
                                        <asp:ListItem Value="create" Text="CREATE" />
                                        <asp:ListItem Value="alter" Text="ALTER" />
                                        <asp:ListItem Value="drop" Text="DROP" />
                                        <asp:ListItem Value="exec" Text="EXEC" />
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2" class="button-row">
                                    <asp:Button ID="btnManualExecute" runat="server" Text="Execute SQL"
                                        CssClass="filter-panel-btn" OnClick="btnManualExecute_Click"
                                        CausesValidation="false"
                                        OnClientClick="return confirm('Run this SQL against SQL Server?');"
                                        ToolTip="Execute the SQL in the box" />
                                    <asp:Button ID="btnManualClear" runat="server" Text="Clear"
                                        CssClass="filter-panel-btn" OnClick="btnManualClear_Click"
                                        CausesValidation="false" ToolTip="Clear the SQL box and results" />
                                    <span class="image-button" title="Return to System Tools">
                                        <asp:ImageButton ID="btnBackManual" runat="server"
                                            ImageUrl="~/images/imgButtons/Back.gif"
                                            AlternateText="Back"
                                            ToolTip="Return to System Tools"
                                            OnClick="btnBack_Click"
                                            CausesValidation="false" />
                                    </span>
                                </td>
                            </tr>
                        </table>

                        <h3 class="woo-map-section-title" style="margin-top: 16px;">Previous Manual SQL</h3>
                        <p class="page-tone-subtitle" style="margin: 0 0 8px 0;">
                            Click <strong>Open</strong> to reload a past statement (and its saved result snapshot for SELECTs).
                        </p>
                        <asp:GridView ID="gvManualHistory" runat="server" CssClass="results-table"
                            AutoGenerateColumns="false" DataKeyNames="Id"
                            OnRowCommand="gvManualHistory_RowCommand"
                            EmptyDataText="No manual SQL has been run yet."
                            style="width: 100%;">
                            <Columns>
                                <asp:TemplateField HeaderText="" HeaderStyle-CssClass="col-cmd" ItemStyle-CssClass="col-cmd">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnOpenHistory" runat="server" Text="Open"
                                            CommandName="OpenHistory" CommandArgument='<%# Eval("Id") %>'
                                            CausesValidation="false" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="RanAtDisplay" HeaderText="When" />
                                <asp:BoundField DataField="UserName" HeaderText="User" />
                                <asp:BoundField DataField="Type" HeaderText="Type" />
                                <asp:BoundField DataField="SqlPreview" HeaderText="SQL" />
                                <asp:BoundField DataField="OkDisplay" HeaderText="OK" />
                                <asp:BoundField DataField="Message" HeaderText="Result" />
                            </Columns>
                        </asp:GridView>
                    </asp:View>
                </asp:MultiView>

                <asp:GridView ID="gvSQLResults" runat="server" CssClass="results-table"
                    AutoGenerateColumns="false" style="margin-top: 14px; width: 100%;">
                    <Columns>
                        <asp:BoundField DataField="Type" HeaderText="Type" />
                        <asp:BoundField DataField="SqlPreview" HeaderText="SQL" />
                        <asp:BoundField DataField="Succeeded" HeaderText="OK" />
                        <asp:BoundField DataField="Error" HeaderText="Error / Notes" />
                    </Columns>
                </asp:GridView>

                <asp:Panel ID="pnlSQLResults" runat="server" style="margin-top: 12px; width: 100%; overflow-x: auto;" />

                <div class="page-tone-footer">
                    <div class="status-message" id="pnlStatus" runat="server">
                        <asp:Literal ID="ltrlStatus" runat="server" />
                    </div>
                </div>
            </asp:Panel>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="GoButton" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="RefreshFilesButton" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnManualExecute" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnManualClear" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="gvManualHistory" EventName="RowCommand" />
            <asp:AsyncPostBackTrigger ControlID="btnTabXml" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnTabManual" EventName="Click" />
            <asp:PostBackTrigger ControlID="btnBack" />
            <asp:PostBackTrigger ControlID="btnBackManual" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
