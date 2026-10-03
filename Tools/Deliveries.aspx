<%@ Page Title="Deliveries & areas" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Deliveries.aspx.cs" Inherits="TrackerSQL.Tools.Deliveries" %>

<asp:Content ID="cntDeliveriesHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="cntDeliveriesBdy" ContentPlaceHolderID="MainContent" runat="server">
    <div class="simpleForm page-tone-panel page-tone-delivery">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/icons8-delivery-16.png" alt="" />
            <div>
                <h1 class="page-tone-title">Deliveries &amp; areas</h1>
                <p class="page-tone-subtitle">Everything for getting orders to customers: the daily sheet, the driver app, driver tracking, customer GPS points and the delivery settings</p>
            </div>
        </div>

        <div class="responsive-layout-container home-dashboard deliveries-hub">
            <div class="complex-form-section">
                <h3>Every day</h3>
                <div class="dashboard-links">
                    <div class="dashboard-card home-tone-dlv-sheet">
                        <div class="tool-card-header">
                            <img class="tool-card-icon" src="../images/imgButtons/icons8-deliver-sheet-16.png" alt="" />
                            <h4><a href="../Pages/DeliverySheet.aspx">Delivery sheet</a></h4>
                        </div>
                        <p>The day's deliveries per driver, in route order</p>
                    </div>
                    <div class="dashboard-card home-tone-dlv-tracking">
                        <div class="tool-card-header">
                            <img class="tool-card-icon" src="../images/imgButtons/Compass.gif" alt="" />
                            <h4><a href="DriverTracking.aspx">Driver tracking</a></h4>
                        </div>
                        <p>Map of where each driver is now, their stops left and when they should finish</p>
                    </div>
                    <div class="dashboard-card home-tone-dlv-order">
                        <div class="tool-card-header">
                            <img class="tool-card-icon" src="../images/imgButtons/icons8-order-completed-16.png" alt="" />
                            <h4><a href="DeliveryOrder.aspx">Delivery order</a></h4>
                        </div>
                        <p>Each driver's stops: what is done, what is next and when the rest are expected (live)</p>
                    </div>
                    <div class="dashboard-card home-tone-dlv-app">
                        <div class="tool-card-header">
                            <img class="tool-card-icon" src="../images/imgButtons/Mobile-phone.gif" alt="" />
                            <h4><a href="DriverApp.aspx">Driver app</a></h4>
                        </div>
                        <p>Install the app and its settings<br /><a href="../driver/">Open the web app</a></p>
                    </div>
                    <div class="dashboard-card home-tone-dlv-app">
                        <div class="tool-card-header">
                            <img class="tool-card-icon" src="../images/imgButtons/Red%20mark.gif" alt="" />
                            <h4><a href="DeliveriesAwaitingDone.aspx">Waiting for Order Done</a></h4>
                        </div>
                        <p>Deliveries the driver sent whose order is not done yet; Complete Now (administrators)</p>
                    </div>
                    <div class="dashboard-card home-tone-dlv-move">
                        <div class="tool-card-header">
                            <img class="tool-card-icon" src="../images/imgButtons/icons8-move-date.png" alt="" />
                            <h4><a href="MoveDeliveryDate.aspx">Move delivery date</a></h4>
                        </div>
                        <p>Move a day's deliveries to another day</p>
                    </div>
                </div>
            </div>

            <div class="complex-form-section">
                <h3>Customer GPS points</h3>
                <div class="dashboard-links">
                    <div class="dashboard-card home-tone-dlv-locations">
                        <div class="tool-card-header">
                            <img class="tool-card-icon" src="../images/imgButtons/Red pin.gif" alt="" />
                            <h4><a href="CustomerLocations.aspx">Customer locations</a></h4>
                        </div>
                        <p>Look up GPS points in bulk and check the rough ones (administrators)</p>
                    </div>
                    <div class="dashboard-card home-tone-dlv-gps">
                        <div class="tool-card-header">
                            <img class="tool-card-icon" src="../images/imgButtons/SystemSettingsWithGlobe.png" alt="" />
                            <h4><a href="SystemPreferences.aspx?section=gps">GPS and driver settings</a></h4>
                        </div>
                        <p>Lookup service and key, map, depot, tracking and arrival times (administrators)</p>
                    </div>
                </div>
            </div>

            <div class="complex-form-section">
                <h3>Settings</h3>
                <div class="dashboard-links">
                    <div class="dashboard-card home-tone-dlv-areas">
                        <div class="tool-card-header">
                            <img class="tool-card-icon" src="../images/imgButtons/icons8-list-of-areas.png" alt="" />
                            <h4><a href="../Pages/Lookups.aspx?tab=Areas">Areas</a></h4>
                        </div>
                        <p>Delivery areas, their codes and days</p>
                    </div>
                    <div class="dashboard-card home-tone-dlv-days">
                        <div class="tool-card-header">
                            <img class="tool-card-icon" src="../images/imgButtons/Calendar.gif" alt="" />
                            <h4><a href="../Pages/Lookups.aspx?tab=Delivery%20Days">Delivery days</a></h4>
                        </div>
                        <p>The order of the areas each day; used for stops that have no GPS point yet</p>
                    </div>
                    <div class="dashboard-card home-tone-dlv-people">
                        <div class="tool-card-header">
                            <img class="tool-card-icon" src="../images/imgButtons/icons8-list-of-people.png" alt="" />
                            <h4><a href="../Pages/Lookups.aspx?tab=People">People</a></h4>
                        </div>
                        <p>Drivers and their delivery type: does deliveries, courier (waybill), collect (shop) or none</p>
                    </div>
                    <div class="dashboard-card home-tone-dlv-couriers">
                        <div class="tool-card-header">
                            <img class="tool-card-icon" src="../images/imgButtons/icons8-truck-loading-16.png" alt="" />
                            <h4><a href="../Pages/Lookups.aspx?tab=Couriers">Couriers</a></h4>
                        </div>
                        <p>Courier companies, their tracking links and waybill formats</p>
                    </div>
                    <div class="dashboard-card home-tone-dlv-outcomes">
                        <div class="tool-card-header">
                            <img class="tool-card-icon" src="../images/imgButtons/icons8-completed-delivery-36.png" alt="" />
                            <h4><a href="../Pages/Lookups.aspx?tab=Delivery%20Outcomes">Delivery outcomes</a></h4>
                        </div>
                        <p>How it was delivered, or why not: the choices in the driver app</p>
                    </div>
                    <div class="dashboard-card home-tone-dlv-packaging">
                        <div class="tool-card-header">
                            <img class="tool-card-icon" src="../images/imgButtons/icons8-list-of-packaging.png" alt="" />
                            <h4><a href="../Pages/Lookups.aspx?tab=Packaging">Packaging</a></h4>
                        </div>
                        <p>Packaging and its colours (also shown in the driver app)</p>
                    </div>
                    <div class="dashboard-card home-tone-dlv-holiday">
                        <div class="tool-card-header">
                            <img class="tool-card-icon" src="../images/imgButtons/CalendarClock.gif" alt="" />
                            <h4><a href="HolidayClosures.aspx">Holiday closures</a></h4>
                        </div>
                        <p>Days with no deliveries</p>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
