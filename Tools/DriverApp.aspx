<%@ Page Title="Driver App" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="DriverApp.aspx.cs" Inherits="TrackerSQL.Tools.DriverApp" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server" />
<asp:Content ID="Body" ContentPlaceHolderID="MainContent" runat="server">
    <div class="simpleForm page-tone-panel page-tone-sysdata driver-app-page">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/icons8-delivery-16.png" alt="" />
            <div>
                <h1 class="page-tone-title">Quaffee Driver App</h1>
                <p class="page-tone-subtitle">Deliveries, proof of delivery and repairs on the driver's phone or tablet, also with no signal.
                    Drivers sign in with their normal Tracker login, which must be linked to a person in Lookups → People.</p>
            </div>
        </div>

        <asp:Panel ID="pnlSettings" runat="server" Visible="false" CssClass="complex-form-section driver-app-settings">
            <h3>Settings</h3>
            <asp:Literal ID="litMessage" runat="server" />

            <h4 class="gps-settings-heading">Completing Deliveries</h4>
            <table class="detail-form-table sys-prefs-form-table driver-app-form">
                <tr>
                    <td class="sys-prefs-label">Order Done</td>
                    <td class="sys-prefs-field">
                        <asp:CheckBox ID="chkRunDone" runat="server" Text="Complete the order in Tracker (run Done) when the driver sends a delivery" />
                        <div class="driver-app-help">Runs the same steps as the Done button on the delivery sheet, in the background once the
                            delivery is saved. Deliveries with items still to follow are always left for the office. When off, the office
                            completes orders from the delivery sheet.</div>
                    </td>
                </tr>
                <tr>
                    <td class="sys-prefs-label">Proof of delivery</td>
                    <td class="sys-prefs-field">
                        <asp:CheckBox ID="chkSendConfirmation" runat="server" Text="Email the client the proof of delivery" />
                        <div class="driver-app-help">A PDF with the order, what was delivered or is still to follow, who received it, when,
                            whether they accepted the terms, and the signature; copied to the orders address. It replaces the "delivered" email
                            Done would send, so the client gets one email. The office can also email it later from Contact Details → Proof of Delivery.</div>
                    </td>
                </tr>
                <tr>
                    <td class="sys-prefs-label">How it was delivered</td>
                    <td class="sys-prefs-field">
                        <a href="../Pages/Lookups.aspx?tab=Delivery%20Outcomes">Lookups → Delivery Outcomes</a>
                        <div class="driver-app-help">The choices the driver picks from (e.g. "Handed to the contact", "Collected", "Nobody there"):
                            add to, reword, reorder or switch off. Changes reach the phones the next time they have signal.</div>
                    </td>
                </tr>
            </table>

            <h4 class="gps-settings-heading">Signature and Terms</h4>
            <table class="detail-form-table sys-prefs-form-table driver-app-form">
                <tr>
                    <td class="sys-prefs-label"><label for="<%= txtTerms.ClientID %>">Terms accepted on signing</label></td>
                    <td class="sys-prefs-field">
                        <asp:TextBox ID="txtTerms" runat="server" TextMode="MultiLine" Rows="4" MaxLength="1000" CssClass="sys-prefs-input driver-app-terms" />
                        <div class="driver-app-help">Shown with a tick box above the signature on the app; a signature needs the box ticked.
                            The wording they accepted is kept with the proof and printed on the proof of delivery.</div>
                    </td>
                </tr>
                <tr>
                    <td class="sys-prefs-label"><label for="<%= txtTermsUrl.ClientID %>">Full terms link</label></td>
                    <td class="sys-prefs-field">
                        <asp:TextBox ID="txtTermsUrl" runat="server" MaxLength="300" TextMode="Url" CssClass="sys-prefs-input" />
                        <div class="driver-app-help">Opens from the app ("Read the terms") and is printed on the proof of delivery.</div>
                    </td>
                </tr>
            </table>

            <h4 class="gps-settings-heading">Notes and Calls</h4>
            <table class="detail-form-table sys-prefs-form-table driver-app-form">
                <tr>
                    <td class="sys-prefs-label">Notes to the office</td>
                    <td class="sys-prefs-field">
                        <asp:CheckBox ID="chkNotesToOffice" runat="server" Text="Drivers may email their delivery note to the office" />
                        <div class="driver-app-help">For example when the client asks for a change to their order.</div>
                    </td>
                </tr>
                <tr>
                    <td class="sys-prefs-label"><label for="<%= txtOfficeEmail.ClientID %>">Office email</label></td>
                    <td class="sys-prefs-field">
                        <asp:TextBox ID="txtOfficeEmail" runat="server" MaxLength="200" TextMode="Email" CssClass="sys-prefs-input driver-app-short" />
                        <div class="driver-app-help">Where driver notes go. Leave empty to use the orders address (<asp:Literal ID="litOrdersEmail" runat="server" />).</div>
                    </td>
                </tr>
                <tr>
                    <td class="sys-prefs-label"><label for="<%= txtLandlines.ClientID %>">Landline numbers start with</label></td>
                    <td class="sys-prefs-field">
                        <asp:TextBox ID="txtLandlines" runat="server" MaxLength="200" CssClass="sys-prefs-input driver-app-short" />
                        <div class="driver-app-help">Separate with commas, e.g. <code>021, 011, 087</code>. The app offers only Call (no WhatsApp)
                            for these numbers. Written the local way: <code>021</code> also covers +27 21 and 27 21. Empty = every number may use WhatsApp.</div>
                    </td>
                </tr>
            </table>

            <div class="button-row driver-app-save">
                <asp:Button ID="btnSaveSettings" runat="server" Text="Save settings" CssClass="filter-panel-btn" OnClick="btnSaveSettings_Click" />
                <span class="sys-prefs-saved-hint"><asp:Literal ID="litSettingsInfo" runat="server" /></span>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlRelated" runat="server" Visible="false" CssClass="complex-form-section driver-app-related">
            <h3>Deliveries From the App</h3>
            <ul>
                <li><a href="DeliveriesAwaitingDone.aspx">Deliveries Waiting for Order Done</a>
                    <asp:Literal ID="litAwaitingCount" runat="server" />
                    <span class="driver-app-help">Deliveries the driver sent whose order is still not done: being completed, left for the
                        office or failed, with <strong>Complete Now</strong>. They are also flagged in red on the Delivery Sheet.</span></li>
                <li><a href="DeliveryOrder.aspx">Delivery Order</a>
                    <span class="driver-app-help">Each delivery records which stop it was that day, and the suggested route learns from it.</span></li>
            </ul>
        </asp:Panel>

        <div class="complex-form-section driver-app-install">
            <h3>Install the App on a Phone or Tablet</h3>
            <div class="driver-app-install-grid">
                <div class="driver-app-install-card">
                    <h4>Android</h4>
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
                        <p class="small">Updates: from version 0.5.1 the app shows <strong>Update</strong> (on the delivery list and under More) when a newer
                            version is on this page; tapping it downloads the new version, then open the download and tap <strong>Install</strong>.
                            Older versions: download and install again over the old one. Deliveries waiting to send are kept.</p>
                    </asp:Panel>
                    <asp:Panel ID="pnlNoApk" runat="server" Visible="false" CssClass="status-message status-info">
                        The Android app has not been built yet. In the TrackerDriver project run <code>npm run apk</code>;
                        it saves the app as <code>Downloads\QuaffeeDriver.apk</code> in this site.
                    </asp:Panel>
                </div>
                <div class="driver-app-install-card">
                    <h4>Any Phone, Including iPhone (Web App)</h4>
                    <p>Open <a href="<%= WebAppUrl %>"><%= WebAppUrl %></a> on the phone, then:</p>
                    <ul>
                        <li><strong>Android (Chrome):</strong> menu ⋮ → <strong>Install app</strong> (or Add to Home screen).</li>
                        <li><strong>iPhone (Safari):</strong> Share → <strong>Add to Home Screen</strong>.</li>
                    </ul>
                    <p class="small">It opens full screen with its own icon and keeps working with no signal once it has been opened once.
                        Phones only offer to install it from an https:// address.</p>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
