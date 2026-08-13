<%@ Page Title="Database Backup" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="DatabaseBackup.aspx.cs" Inherits="TrackerSQL.Tools.DatabaseBackup"
    MaintainScrollPositionOnPostback="true" %>

<asp:Content ID="cntDatabaseBackupHdr" title="Database Backup"  ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        function beginDatabaseBackup(button, savingText) {
            if (!button || button.getAttribute("data-saving") === "true") {
                return false;
            }
            if (!window.confirm("Create a new SQL Server backup of the live database now?")) {
                return false;
            }
            button.setAttribute("data-saving", "true");
            button.setAttribute("aria-disabled", "true");
            button.style.pointerEvents = "none";
            button.value = savingText || "Backing up...";
            var savingStatus = document.getElementById("databaseBackupSaving");
            if (savingStatus) {
                savingStatus.style.display = "flex";
            }
            return true;
        }
    </script>
</asp:Content>

<asp:Content ID="cntDatabaseBackupBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smDatabaseBackup" runat="server" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="uprgDatabaseBackup" runat="server"
        AssociatedUpdatePanelID="upnlDatabaseBackup" DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info page-tone-progress">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="upnlDatabaseBackup" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <Triggers>
            <asp:PostBackTrigger ControlID="btnBackupNow" />
            <asp:PostBackTrigger ControlID="btnDownloadSelected" />
            <asp:PostBackTrigger ControlID="btnBack" />
        </Triggers>
        <ContentTemplate>
            <asp:Panel ID="pnlDatabaseBackup" runat="server" CssClass="simpleForm page-tone-panel page-tone-tools">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/icons8-data-backup-30.png" alt="" />
                    <div>
                        <h1 class="page-tone-title">Database Backup</h1>
                        <p class="page-tone-subtitle">Folder from Web.config <code>DatabaseBackupFolder</code> (default App_Data\Backup)</p>
                    </div>
                </div>

                <div class="page-tone-toolbar button-row">
                    <asp:Button ID="btnBackupNow" runat="server" Text="Backup Now" CssClass="filter-panel-btn"
                        OnClick="btnBackupNow_Click"
                        OnClientClick="return beginDatabaseBackup(this, 'Backing up...');"
                        ToolTip="Create a timestamped .bak file in App_Data\Backup" />
                    <asp:Button ID="btnDownloadSelected" runat="server" Text="Download Selected" CssClass="filter-panel-btn"
                        OnClick="btnDownloadSelected_Click"
                        ToolTip="Download the ticked backup (select exactly one)" />
                    <asp:Button ID="btnDeleteSelected" runat="server" Text="Delete Selected" CssClass="filter-panel-btn"
                        OnClick="btnDeleteSelected_Click"
                        OnClientClick="return confirm('Delete the selected backup file(s)? This cannot be undone.');"
                        ToolTip="Delete the ticked backup files from App_Data\Backup" />
                    <asp:Button ID="btnRefresh" runat="server" Text="Refresh" CssClass="filter-panel-btn"
                        OnClick="btnRefresh_Click" CausesValidation="false"
                        ToolTip="Reload the backup list" />
                    <span class="image-button" title="Return to System Tools">
                        <asp:ImageButton ID="btnBack" runat="server"
                            ImageUrl="~/images/imgButtons/Back.gif"
                            AlternateText="Back"
                            ToolTip="Return to System Tools"
                            OnClick="btnBack_Click"
                            CausesValidation="false" />
                    </span>
                </div>

                <div id="databaseBackupSaving" class="status-message status-info page-tone-progress"
                    style="display: none; margin-top: 12px;" role="status" aria-live="polite">
                    <img src="../images/animi/QuaffeeProgress.gif" alt="" />
                    <span>&nbsp;Backing up database, please wait...</span>
                </div>

                <div class="results-container" style="margin-top: 12px;">
                    <asp:GridView ID="gvBackups" runat="server" AutoGenerateColumns="False"
                        CssClass="results-table results-table-fit" EmptyDataText="No backups found yet."
                        DataKeyNames="FileName"
                        OnRowCommand="gvBackups_RowCommand"
                        OnRowDataBound="gvBackups_RowDataBound">
                        <Columns>
                            <asp:TemplateField HeaderText="Select" HeaderStyle-CssClass="col-tight" ItemStyle-CssClass="col-tight">
                                <ItemTemplate>
                                    <asp:CheckBox ID="chkSelect" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="FileName" HeaderText="Backup file" />
                            <asp:BoundField DataField="CreatedDisplay" HeaderText="Created" />
                            <asp:BoundField DataField="SizeDisplay" HeaderText="Size" ItemStyle-HorizontalAlign="Right"
                                HeaderStyle-HorizontalAlign="Right" />
                            <asp:TemplateField HeaderText="" HeaderStyle-CssClass="col-tight" ItemStyle-CssClass="col-tight">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnDownloadRow" runat="server" Text="Download"
                                        CommandName="DownloadBackup"
                                        CommandArgument='<%# Eval("FileName") %>'
                                        CausesValidation="false"
                                        ToolTip="Download this backup file" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>

                <div class="page-tone-footer">
                    <div class="status-message" id="pnlStatus" runat="server" style="margin-top: 12px;">
                        <asp:Literal ID="ltrlStatus" runat="server" />
                    </div>
                    <p class="page-tone-subtitle" style="margin-top: 8px;">
                        Folder:
                        <asp:Literal ID="ltrlBackupFolder" runat="server" />
                    </p>
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
