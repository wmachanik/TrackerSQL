<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" CodeBehind="EmailDiagnostics.aspx.cs" Inherits="TrackerSQL.Tools.EmailDiagnostics" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>

<asp:Content ID="cntEmailTestHdr" ContentPlaceHolderID="HeadContent" runat="server">
    <title>QonT Email Configuration Tester</title>
    <link rel="stylesheet" type="text/css" href="./EmailDiagnostics.css" />
    <style type="text/css">
        .auto-style1 {
            width: 32px;
            height: 32px;
        }
    </style>
</asp:Content>
<asp:Content ID="cntSendCoffeeCheckupBdy" ContentPlaceHolderID="MainContent" runat="server">
    <%--<form id="form1" runat="server">--%>
    <asp:ScriptManager ID="scrmngEmailTest" runat="server" AsyncPostBackTimeout="120000" />

    <asp:Panel ID="pnlEmailDiagnostics" runat="server" CssClass="simpleForm page-tone-panel page-tone-email">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/World.gif" alt="" />
            <div>
                <h1 class="page-tone-title">Email Diagnostics</h1>
                <p class="page-tone-subtitle">Test SMTP and email settings</p>
            </div>
        </div>

        <asp:Label ID="lblGlobalStatus" runat="server" ForeColor="Blue" />

        <div class="container">
        <ajax:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0" CssClass="ajax-tabs">

            <!-- 📨 Email Test Tab -->
            <ajax:TabPanel ID="TabEmail" runat="server" HeaderText="📨 Test Email">
                <ContentTemplate>
                    <asp:UpdateProgress ID="uprgEmailTest" runat="server" AssociatedUpdatePanelID="upGlobal">
                        <ProgressTemplate>
                            <div class="update-progress">
                                <img alt="progress" class="auto-style1" src="../images/animi/img_progress.gif" height="16" />Sending...
                            </div>
                        </ProgressTemplate>
                    </asp:UpdateProgress>
                    <asp:UpdatePanel ID="upGlobal" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <div class="test-css">
                                <fieldset>
                                    <legend>SMTP Settings</legend>

                                    <label for="<%= txtHost.ClientID %>">SMTP Host:</label>
                                    <asp:TextBox ID="txtHost" runat="server"
                                        title="Enter your SMTP server's hostname"
                                        placeholder="smtp.yourdomain.com" />

                                    <label for="<%= txtPort.ClientID %>">Port:</label>
                                    <asp:TextBox ID="txtPort" runat="server"
                                        title="Enter the SMTP port number"
                                        placeholder="465" />

                                    <label for="<%= txtUser.ClientID %>">Username:</label>
                                    <asp:TextBox ID="txtUser" runat="server"
                                        title="Enter your SMTP username"
                                        placeholder="user@example.com" />

                                    <div class="password-group">
                                        <label for="<%= txtPass.ClientID %>">Password:</label>
                                        <div class="input-with-toggle">
                                            <asp:TextBox ID="txtPass" runat="server" TextMode="Password"
                                                title="Enter your password"
                                                placeholder="••••••••" />

                                            <div class="checkbox-label">
                                                <asp:CheckBox ID="chkShowPwd" runat="server"
                                                    AutoPostBack="true"
                                                    OnCheckedChanged="chkShowPwd_CheckedChanged" />
                                                <label for="<%= chkShowPwd.ClientID %>">Show password</label>
                                            </div>
                                        </div>
                                    </div>

                                    <label for="<%= chkSSL.ClientID %>">Enable SSL:</label>
                                    <asp:CheckBox ID="chkSSL" runat="server"
                                        title="Check if SSL should be used when connecting" />

                                    <label for="<%= ddlSocketOption.ClientID %>">Secure Socket Option:</label>
                                    <asp:DropDownList ID="ddlSocketOption" runat="server"
                                        title="Select the appropriate secure socket method">
                                        <asp:ListItem Text="Auto" Value="Auto" />
                                        <asp:ListItem Text="None" Value="None" />
                                        <asp:ListItem Text="SSL on Connect" Value="SslOnConnect" />
                                        <asp:ListItem Text="StartTLS" Value="StartTls" />
                                    </asp:DropDownList>

                                    <label for="<%= txtTimeout.ClientID %>">Timeout (ms):</label>
                                    <asp:TextBox ID="txtTimeout" runat="server" Text="10000"
                                        title="Enter the connection timeout in milliseconds"
                                        placeholder="10000" />
                                </fieldset>

                                <fieldset>
                                    <legend>Email Details</legend>

                                    <label for="<%= txtFrom.ClientID %>">From Address:</label>
                                    <asp:TextBox ID="txtFrom" runat="server"
                                        title="Enter the sender's email address"
                                        placeholder="sender@domain.com" />

                                    <label for="<%= txtTo.ClientID %>">To Address:</label>
                                    <asp:TextBox ID="txtTo" runat="server"
                                        title="Enter the recipient's email address"
                                        placeholder="recipient@domain.com" />

                                    <label for="<%= txtSubject.ClientID %>">Subject:</label>
                                    <asp:TextBox ID="txtSubject" runat="server"
                                        title="Enter the email subject"
                                        placeholder="Test Subject" />

                                    <label for="<%= txtBody.ClientID %>">Body:</label>
                                    <asp:TextBox ID="txtBody" runat="server" TextMode="MultiLine" Rows="5"
                                        title="Enter the body of the email"
                                        placeholder="This is a test message..." />
                                </fieldset>

                                <div class="form-actions">
                                    <asp:Button ID="btnSend" runat="server" Text="Send Test Email" OnClick="btnSend_Click" />
                                    <asp:Button ID="btnSaveConfig" runat="server" Text="Save to Web.Config" OnClick="btnSaveConfig_Click" />
                                </div>

                                <div>
                                    <asp:Label ID="lblResult" runat="server" ForeColor="Red" />
                                </div>
                            </div>

                        </ContentTemplate>
                    </asp:UpdatePanel>
                </ContentTemplate>
            </ajax:TabPanel>

            <!-- 🧪 Diagnostics Tab -->
            <ajax:TabPanel ID="TabDiag" runat="server" HeaderText="🧪 Diagnostics">
                <ContentTemplate>
                    <asp:UpdateProgress ID="uprgEmailDiag" runat="server" AssociatedUpdatePanelID="upDiag">
                        <ProgressTemplate>
                            <img src="../images/animi/img_progress.gif" alt="Testing..." width="16" height="16" />Testing...
                        </ProgressTemplate>
                    </asp:UpdateProgress>
                    <asp:UpdatePanel ID="upDiag" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="btnDiagnostics" EventName="Click" />
                            <asp:AsyncPostBackTrigger ControlID="btnViewLog" EventName="Click" />
                            <asp:AsyncPostBackTrigger ControlID="btnTestCombos" EventName="Click" />
                            <asp:AsyncPostBackTrigger ControlID="tmrComboProgress" EventName="Tick" />
                        </Triggers>
                        <ContentTemplate>
                            <div class="test-css">

                                <fieldset>
                                    <legend>SMTP Diagnostics</legend>
                                    <asp:Button ID="btnDiagnostics" runat="server" Text="Run SMTP Diagnostics" OnClick="btnDiagnostics_Click" />
                                    <asp:Label ID="lblDiagnostics" runat="server" CssClass="diagnostics-output" />
                                </fieldset>

                                <fieldset>
                                    <legend>Diagnostics Log Viewer</legend>
                                    <asp:Button ID="btnViewLog" runat="server" Text="View Log File" OnClick="btnViewLog_Click" />
                                    <asp:Literal ID="litLogOutput" runat="server" Mode="PassThrough" />
                                </fieldset>

                                <fieldset>
                                    <legend>Log Utilities</legend>
                                    <asp:Button ID="btnClearCombosLog" runat="server" Text="Clear Combos Log" OnClick="btnClearCombosLog_Click" />
                                    <asp:Label ID="lblClearLogStatus" runat="server" />
                                    <asp:HyperLink ID="lnkDownloadLog" runat="server" Text="Download Log File" NavigateUrl="~/App_Data/smtp_combos.log" Target="_blank" />
                                </fieldset>

                                <fieldset>
                                    <legend>Test All SMTP Combos</legend>
                                    <asp:Button ID="btnTestCombos" runat="server" Text="Test All Combos" OnClick="btnTestCombos_Click" />
                                    <asp:Label ID="lblProgress" runat="server" CssClass="combo-progress-label" />
                                    <asp:Literal ID="litComboResults" runat="server" Mode="PassThrough" />
                                    <asp:Timer ID="tmrComboProgress" runat="server" Interval="1000" Enabled="false" OnTick="tmrComboProgress_Tick" />
                                </fieldset>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </ContentTemplate>
            </ajax:TabPanel>
            <!-- Single Email test -->
            <ajax:TabPanel ID="TabCcTest" runat="server" HeaderText="📎 CC Test">
                <ContentTemplate>
                    <asp:UpdatePanel ID="upCcTest" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <div class="test-css">
                                <fieldset>
                                    <legend>CC Delivery Test</legend>

                                    <label for="<%= txtFrom.ClientID %>">From Address:</label>
                                    <asp:TextBox ID="TextBox1" runat="server" title="Enter the sender's email address" placeholder="sender@domain.com" />

                                    <label for="<%= txtTo.ClientID %>">To Address:</label>
                                    <asp:TextBox ID="TextBox2" runat="server" title="Enter the recipient's email address" placeholder="recipient@domain.com" />

                                    <label for="<%= txtCc.ClientID %>">CC Address(es):</label>
                                    <asp:TextBox ID="txtCc" runat="server" title="Enter CC addresses separated by comma or semicolon" placeholder="admin@domain.com" />

                                    <label for="<%= txtSubject.ClientID %>">Subject:</label>
                                    <asp:TextBox ID="TextBox3" runat="server" title="Enter the email subject" placeholder="CC Test" />

                                    <label for="<%= txtBody.ClientID %>">Body:</label>
                                    <asp:TextBox ID="TextBox4" runat="server" TextMode="MultiLine" Rows="4" title="Enter the body of the email" placeholder="This is a CC test..." />

                                    <div class="form-actions" style="margin-top: 10px;">
                                        <asp:Button ID="btnSendCcTest" runat="server" Text="Send CC Test" OnClick="btnSendCcTest_Click" />
                                        <asp:Button ID="btnSendBothTest" runat="server" Text="Send Both Tests" OnClick="btnSendBothTest_Click" />
                                    </div>

                                    <div style="margin-top: 8px;">
                                        <asp:Label ID="lblCcResult" runat="server" ForeColor="Red" />
                                    </div>
                                </fieldset>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </ContentTemplate>
            </ajax:TabPanel>
        </ajax:TabContainer>
        </div>
    </asp:Panel>

    <script type="text/javascript">
        function applyCombo(port, option) {
            document.getElementById('<%= txtPort.ClientID %>').value = port;
            document.getElementById('<%= ddlSocketOption.ClientID %>').value = option;
        }
    </script>
    <%--</form>--%>
</asp:Content>
