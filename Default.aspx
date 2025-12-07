<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="TrackerSQL.Default" %>

<%--<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>--%>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>Welcome to Tracker.NET</h2>
    
    <!-- Dashboard Grid Layout -->
    <div class="responsive-layout-container">
        
        <!-- Contacts Section -->
        <div class="complex-form-section">
            <h3>Contact Management</h3>
            <div class="dashboard-links">
                <div class="dashboard-card">
                    <h4><a href="Pages/Contacts.aspx">Contacts</a></h4>
                    <p>Manage contact information and accounts</p>
                </div>
                <div class="dashboard-card">
                    <h4><a href="Pages/SendCoffeeCheckup.aspx">Send Checkup</a></h4>
                    <p>Send automated contact checkups</p>
                </div>
                <div class="dashboard-card">
                    <h4><a href="Pages/SentRemindersSheet.aspx">Reminder History</a></h4>
                    <p>View sent reminders and notifications</p>
                </div>
            </div>
        </div>

        <!-- Orders Section -->
        <div class="complex-form-section">
            <h3>Order Management</h3>
            <div class="dashboard-links">
                <div class="dashboard-card btn-add">
                    <h4><a href="Pages/OrderDetail.aspx?NewOrder=true">New Order</a></h4>
                    <p>Create a new customer order</p>
                </div>
                <div class="dashboard-card">
                    <h4><a href="Pages/DeliverySheet.aspx">View &amp; Edit Orders</a></h4>
                    <p>Manage existing orders and deliveries</p>
                </div>
                <div class="dashboard-card">
                    <h4><a href="Pages/ReoccuringOrders.aspx">Recurring Orders</a></h4>
                    <p>Set up and manage recurring orders</p>
                </div>
            </div>
        </div>

        <!-- Repairs Section -->
        <div class="complex-form-section">
            <h3>Repair Services</h3>
            <div class="dashboard-links">
                <div class="dashboard-card">
                    <h4><a href="Pages/Repairs.aspx">Repairs</a></h4>
                    <p>View and manage repair requests</p>
                </div>
                <div class="dashboard-card btn-add">
                    <h4><a href="Pages/RepairDetail.aspx">New Repair</a></h4>
                    <p>Create a new repair ticket</p>
                </div>
            </div>
        </div>

        <!-- Preparation Section -->
        <div class="complex-form-section">
            <h3>Preparation &amp; Logistics</h3>
            <div class="dashboard-links">
                <div class="dashboard-card">
                    <h4><a href="Pages/CoffeeRequired.aspx">Required Sheet</a></h4>
                    <p>View coffee preparation requirements</p>
                </div>
                <div class="dashboard-card">
                    <h4><a href="Pages/DeliverySheet.aspx">Delivery Sheet</a></h4>
                    <p>Manage delivery schedules</p>
                </div>
                <div class="dashboard-card">
                    <h4><a href="Pages/PreperationSummary.aspx">Weekly Summary</a></h4>
                    <p>View weekly preparation summary</p>
                </div>
            </div>
        </div>

        <!-- System Section -->
        <div class="complex-form-section">
            <h3>System Administration</h3>
            <div class="dashboard-links">
                <div class="dashboard-card">
                    <h4><a href="Pages/ItemGroups.aspx">Item Groups</a></h4>
                    <p>Manage product categories</p>
                </div>
                <div class="dashboard-card">
                    <h4><a href="Pages/Lookups.aspx">Lookups</a></h4>
                    <p>Configure system lookup tables</p>
                </div>
                <div class="dashboard-card">
                    <h4><a href="Tools/SystemTools.aspx">System Tools</a></h4>
                    <p>Access system utilities and tools</p>
                </div>
            </div>
        </div>

    </div>

    <!-- Stats Section -->
    <div class="results-container" style="text-align: center; margin-top: 20px; margin-bottom: 20px;">
        <h3 style="color: var(--primary-green); margin-bottom: 10px;">Total Coffee Served</h3>
        <div style="font-size: 2em; font-weight: bold; color: var(--secondary-brown);">
            <asp:Label ID="lblTotalCupCount" Text="" runat="server" />
        </div>
        <p style="color: var(--primary-brown); font-style: italic;">cups and counting...</p>
    </div>
    &nbsp;

    <div style="margin-top:40px; border-top:1px solid #ccc; padding-top:6px; font-size:12px; color:#555;">
        <asp:Literal ID="litCurrentDate" runat="server" />
    </div>

</asp:Content>