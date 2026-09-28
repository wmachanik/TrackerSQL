<%@ Page Title="Driver app" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="DriverApp.aspx.cs" Inherits="TrackerSQL.Tools.DriverApp" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server" />
<asp:Content ID="Body" ContentPlaceHolderID="MainContent" runat="server">
    <div class="simpleForm page-tone-panel page-tone-sysdata driver-app-page">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/icons8-delivery-16.png" alt="" />
            <div>
                <h1 class="page-tone-title">Quaffee Driver app</h1>
                <p class="page-tone-subtitle">Deliveries, proof of delivery and repairs on the driver's phone or tablet, also with no signal.
                    Drivers sign in with their normal Tracker login, which must be linked to a person in Lookups → People.</p>
            </div>
        </div>

        <asp:Panel ID="pnlSettings" runat="server" Visible="false" CssClass="complex-form-section driver-app-settings">
            <h3>Settings</h3>
            <asp:Literal ID="litMessage" runat="server" />
            <p>
                <asp:CheckBox ID="chkRunDone" runat="server" Text="Complete the order in Tracker (run Done) when the driver sends a delivery" />
                <span class="sys-prefs-help">Runs the same steps as the Done button on the delivery sheet. Deliveries with items still to follow
                    are always left for the office. When off, the office completes orders from the delivery sheet.</span>
            </p>
            <p>
                <asp:CheckBox ID="chkSendConfirmation" runat="server" Text="Email the client a delivery confirmation" />
                <span class="sys-prefs-help">Says who received it, when, and what was delivered or is still to follow, with the signature attached.
                    Copied to the orders address. It replaces the "delivered" email Done would send, so the client gets one email.</span>
            </p>
            <p>
                <asp:CheckBox ID="chkNotesToOffice" runat="server" Text="Drivers may email their delivery note to the office" />
                <span class="sys-prefs-help">For example when the client asks for a change to their order.</span>
            </p>
            <p>
                <label for="<%= txtOfficeEmail.ClientID %>">Office email for driver notes</label><br />
                <asp:TextBox ID="txtOfficeEmail" runat="server" MaxLength="200" TextMode="Email" Columns="40" />
                <span class="sys-prefs-help">Leave empty to use the orders address (<asp:Literal ID="litOrdersEmail" runat="server" />).</span>
            </p>
            <div class="button-row">
                <asp:Button ID="btnSaveSettings" runat="server" Text="Save settings" CssClass="filter-panel-btn" OnClick="btnSaveSettings_Click" />
                <span class="small"><asp:Literal ID="litSettingsInfo" runat="server" /></span>
            </div>
        </asp:Panel>

        <div class="complex-form-section">
            <h3>Android phone or tablet</h3>
            <asp:Panel ID="pnlApk" runat="server">
                <p>
                    <a class="driver-app-download" href="<%= ApkUrl %>" download="QuaffeeDriver.apk">Download the app</a>
                    <span class="small"><asp:Literal ID="litApkInfo" runat="server" /></span>
                </p>
                <ol>
                    <li>Open this page on the phone or tablet (address: <code><%= PageUrl %></code>) and tap <strong>Download the app</strong>.</li>
                    <li>Open the downloaded file. Android asks to allow the browser to install apps: tap <strong>Settings</strong>, turn on
                        <strong>Allow from this source</strong>, go back and tap <strong>Install</strong>.</li>
                    <li>If Play Protect warns that it doesn't recognise the app, tap <strong>Install anyway</strong> (it is not from the Play Store).</li>
                    <li>Open <strong>Quaffee Driver</strong> and sign in with the Tracker login.</li>
                </ol>
                <p class="small">To update, download and install again over the old one; deliveries waiting to send are kept.</p>
            </asp:Panel>
            <asp:Panel ID="pnlNoApk" runat="server" Visible="false" CssClass="status-message status-info">
                The Android app has not been built yet. In the TrackerDriver project run <code>npm run apk</code>;
                it saves the app as <code>Downloads\QuaffeeDriver.apk</code> in this site.
            </asp:Panel>
        </div>

        <div class="complex-form-section">
            <h3>Any phone, including iPhone (web app)</h3>
            <p>Open <a href="<%= WebAppUrl %>"><%= WebAppUrl %></a> on the phone, then:</p>
            <ul>
                <li><strong>Android (Chrome):</strong> menu ⋮ → <strong>Install app</strong> (or Add to Home screen).</li>
                <li><strong>iPhone (Safari):</strong> Share → <strong>Add to Home Screen</strong>.</li>
            </ul>
            <p class="small">It opens full screen with its own icon and keeps working with no signal once it has been opened once.
                Phones only offer to install it from an https:// address.</p>
        </div>
    </div>
</asp:Content>
