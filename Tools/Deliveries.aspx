<%@ Page Title="Deliveries & Areas" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Deliveries.aspx.cs" Inherits="TrackerSQL.Tools.Deliveries" %>

<asp:Content ID="cntDeliveriesHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="cntDeliveriesBdy" ContentPlaceHolderID="MainContent" runat="server">
    <div class="simpleForm page-tone-panel page-tone-delivery">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/icons8-delivery-16.png" alt="" />
            <div>
                <h1 class="page-tone-title">Deliveries &amp; Areas</h1>
                <p class="page-tone-subtitle">Everything for getting orders to customers: the daily sheet, the driver app, customer GPS points and the delivery set-up</p>
            </div>
        </div>

        <div class="complex-form-section">
            <h3>Every day</h3>
            <div class="dashboard-links">
                <div class="dashboard-card home-tone-delivery">
                    <div class="tool-card-header"><h4><a href="../Pages/DeliverySheet.aspx">Delivery Sheet</a></h4></div>
                    <p>The day's deliveries per driver, in route order</p>
                </div>
                <div class="dashboard-card home-tone-delivery">
                    <div class="tool-card-header"><h4><a href="DeliveryOrder.aspx">Delivery order</a></h4></div>
                    <p>The order drivers really delivered in; the route learns from it</p>
                </div>
                <div class="dashboard-card home-tone-delivery">
                    <div class="tool-card-header"><h4><a href="DriverApp.aspx">Driver App</a></h4></div>
                    <p>Install the app, and how Tracker handles deliveries sent from it<br /><a href="../driver/">Open the web app</a></p>
                </div>
                <div class="dashboard-card home-tone-delivery">
                    <div class="tool-card-header"><h4><a href="MoveDeliveryDate.aspx">Move delivery date</a></h4></div>
                    <p>Move a day's deliveries to another day</p>
                </div>
            </div>
        </div>

        <div class="complex-form-section">
            <h3>Customer GPS points</h3>
            <div class="dashboard-links">
                <div class="dashboard-card home-tone-delivery">
                    <div class="tool-card-header"><h4><a href="CustomerLocations.aspx">Customer Locations</a></h4></div>
                    <p>Look up GPS points in bulk and check the rough ones (administrators)</p>
                </div>
                <div class="dashboard-card home-tone-delivery">
                    <div class="tool-card-header"><h4><a href="SystemPreferences.aspx?section=gps">GPS &amp; driver settings</a></h4></div>
                    <p>Lookup service and key, map, depot, tracking and arrival times (administrators)</p>
                </div>
            </div>
        </div>

        <div class="complex-form-section">
            <h3>Set-up</h3>
            <div class="dashboard-links">
                <div class="dashboard-card home-tone-lookups">
                    <div class="tool-card-header"><h4><a href="../Pages/Lookups.aspx?tab=Areas">Areas</a></h4></div>
                    <p>Delivery areas, their codes and days</p>
                </div>
                <div class="dashboard-card home-tone-lookups">
                    <div class="tool-card-header"><h4><a href="../Pages/Lookups.aspx?tab=Delivery%20Days">Delivery Days</a></h4></div>
                    <p>Which areas are delivered on which day, and in what order</p>
                </div>
                <div class="dashboard-card home-tone-lookups">
                    <div class="tool-card-header"><h4><a href="../Pages/Lookups.aspx?tab=People">People</a></h4></div>
                    <p>Drivers and their Delivery Type: Does deliveries, Courier (waybill), Collect (shop) or None</p>
                </div>
                <div class="dashboard-card home-tone-lookups">
                    <div class="tool-card-header"><h4><a href="../Pages/Lookups.aspx?tab=Couriers">Couriers</a></h4></div>
                    <p>Courier companies asked for with the waybill</p>
                </div>
                <div class="dashboard-card home-tone-lookups">
                    <div class="tool-card-header"><h4><a href="../Pages/Lookups.aspx?tab=Driver%20Options">Driver Options</a></h4></div>
                    <p>The "how it was delivered" choices in the driver app</p>
                </div>
                <div class="dashboard-card home-tone-lookups">
                    <div class="tool-card-header"><h4><a href="../Pages/Lookups.aspx?tab=Packaging">Packaging</a></h4></div>
                    <p>Packaging and its colours (also shown in the driver app)</p>
                </div>
                <div class="dashboard-card home-tone-lookups">
                    <div class="tool-card-header"><h4><a href="HolidayClosures.aspx">Holiday closures</a></h4></div>
                    <p>Days with no deliveries</p>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
