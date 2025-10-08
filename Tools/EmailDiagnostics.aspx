<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" CodeBehind="EmailDiagnostics.aspx.cs" Inherits="TrackerDotNet.Tools.EmailDiagnostics" %>

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
    <asp:ScriptManager ID="scrmngEmailTest" runat="server" EnablePageMethods="true" />
    <asp:Label ID="lblGlobalStatus" runat="server" ForeColor="Blue" />

    <div class="container">
        <h2 class="test-h2">Email Configuration Tester</h2>
        
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
                                    <asp:Button ID="btnTestCombos" runat="server" Text="Test All Combos" OnClientClick="startComboTest(); return false;" />
                                    <asp:Label ID="lblProgress" runat="server" />
                                    <asp:PlaceHolder ID="phComboResults" runat="server" />
                                    <asp:Label ID="lblNoResults" runat="server" ForeColor="Gray" />
                                </fieldset>

                                <fieldset>
                                    <asp:HiddenField ID="hfProgressKey" runat="server" />
                                    <div id="progressStatus">Progress: 0%</div>
                                    <div id="comboResults"></div>
                                </fieldset>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </ContentTemplate>
            </ajax:TabPanel>

        </ajax:TabContainer>
    </div>

    <div>
    </div>
    <div id="spinner" style="display: none;">⏳ Testing...</div>
    <script type="text/javascript">
        function showSpinner() {
            document.getElementById("spinner").style.display = "block";
        }
    </script>
    <script type="text/javascript">
        function startComboTest() {
            var key = generateKey();
            document.getElementById('<%= hfProgressKey.ClientID %>').value = key;

            // Disable the button
            var btn = document.getElementById('<%= btnTestCombos.ClientID %>');
            btn.disabled = true;
            btn.style.opacity = "0.6"; // Optional visual cue
            btn.style.cursor = "not-allowed";

            var smtpHost = document.getElementById('<%= txtHost.ClientID %>').value;
            var smtpUser = document.getElementById('<%= txtUser.ClientID %>').value;
            var smtpPass = document.getElementById('<%= txtPass.ClientID %>').value;
            var fromAddress = document.getElementById('<%= txtFrom.ClientID %>').value;
            var toAddress = document.getElementById('<%= txtTo.ClientID %>').value;

            PageMethods.StartComboTest(key, smtpHost, smtpUser, smtpPass, fromAddress, toAddress);
            pollProgress(key, btn); //  pass the button reference
        }
        function applyCombo(port, option) {
            document.getElementById('<%= txtPort.ClientID %>').value = port;
            document.getElementById('<%= ddlSocketOption.ClientID %>').value = option;
            document.getElementById("progressStatus").innerText = "✅ Applied: Port " + port + ", Option " + option;
        }

        function pollProgress(key, btnRef) {
            PageMethods.GetComboProgress(key, function (result) {
                document.getElementById("progressStatus").innerText = "Progress: " + result.percent + "%";
                document.getElementById("comboResults").innerHTML = result.html;
                console.log("Polling progress for key:", key);

                if (!result.completed) {
                    setTimeout(function () { pollProgress(key, btnRef); }, 1000);
                } else {
                    btnRef.disabled = false;
                    btnRef.style.opacity = "1";
                    btnRef.style.cursor = "pointer";
                }
            });
        }

        function generateKey() {
            return 'combo_' + new Date().getTime();
        }
    </script>
    <%--</form>--%>
</asp:Content>
