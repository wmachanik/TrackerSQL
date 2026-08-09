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
                        <p class="page-tone-subtitle">Import data from XML</p>
                    </div>
                </div>

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
                            <asp:Button ID="GoButton" runat="server" Text="Execute"
                                CssClass="filter-panel-btn" OnClick="GoButton_Click"
                                CausesValidation="false"
                                OnClientClick="return confirm('Execute all enabled commands in this XML against SQL Server?');"
                                ToolTip="Run commands from the selected XML file" />
                            <asp:Button ID="btnBack" runat="server" Text="Back"
                                CssClass="filter-panel-btn" OnClick="btnBack_Click"
                                CausesValidation="false" ToolTip="Return to System Tools" />
                        </td>
                    </tr>
                </table>

                <asp:Panel ID="pnlFileBrowser" runat="server" CssClass="file-browser">
                    <strong>Available XML files in App_Data</strong><br />
                    <asp:Literal ID="ltrlFileList" runat="server" />
                </asp:Panel>

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
            <asp:PostBackTrigger ControlID="btnBack" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
