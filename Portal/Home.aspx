<%@ Page Title="Home" Language="C#" MasterPageFile="~/Portal/Portal.Master"
    AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="TrackerSQL.Portal.Home" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server" />
<asp:Content ID="Body" ContentPlaceHolderID="MainContent" runat="server">
    <div class="portal-welcome">
        <h1>Welcome, <asp:Literal ID="litName" runat="server" /></h1>
        <p>
            View your details, orders, repairs and recurring orders.
            You can update some of your details directly; anything else can be sent to us as a change request.
        </p>
        <asp:Literal ID="litNextCoffee" runat="server" />
    </div>
    <div class="home-dashboard portal-dashboard">
        <div class="dashboard-links">
            <a class="dashboard-card-link" href="MyContact.aspx">
                <div class="dashboard-card home-tone-contacts">
                    <div class="tool-card-header">
                        <img class="tool-card-icon" src="../images/imgButtons/icons8-new-contact-48.png" alt="" />
                        <h4>My Details</h4>
                    </div>
                    <p>The contact details we have on file for you</p>
                </div>
            </a>
            <a class="dashboard-card-link" href="MyOrders.aspx">
                <div class="dashboard-card home-tone-orders">
                    <div class="tool-card-header">
                        <img class="tool-card-icon" src="../images/imgButtons/icons8-view-orders-16.png" alt="" />
                        <h4>My Orders</h4>
                    </div>
                    <p>Your order history and delivery status</p>
                </div>
            </a>
            <a class="dashboard-card-link" href="MyRepairs.aspx">
                <div class="dashboard-card home-tone-repairs">
                    <div class="tool-card-header">
                        <img class="tool-card-icon" src="../images/imgButtons/icons8-repair-tools-16.png" alt="" />
                        <h4>My Repairs</h4>
                    </div>
                    <p>Machine repairs and their progress</p>
                </div>
            </a>
            <a class="dashboard-card-link" href="MyRecurring.aspx">
                <div class="dashboard-card home-tone-recurring">
                    <div class="tool-card-header">
                        <img class="tool-card-icon" src="../images/imgButtons/icons8-order-16.png" alt="" />
                        <h4>My Recurring Orders</h4>
                    </div>
                    <p>Your standing orders and when they are next due</p>
                </div>
            </a>
            <a class="dashboard-card-link" href="ChangePassword.aspx">
                <div class="dashboard-card home-tone-users">
                    <div class="tool-card-header">
                        <img class="tool-card-icon" src="../images/imgButtons/Key.gif" alt="" />
                        <h4>Change Password</h4>
                    </div>
                    <p>Choose a new password for your My Quaffee login</p>
                </div>
            </a>
            <asp:LinkButton ID="lnkSignOutCard" runat="server" CssClass="dashboard-card-link"
                OnClick="lnkSignOutCard_Click" CausesValidation="false">
                <div class="dashboard-card home-tone-tools">
                    <div class="tool-card-header">
                        <img class="tool-card-icon" src="../images/imgButtons/Lock.gif" alt="" />
                        <h4>Sign Out</h4>
                    </div>
                    <p>Sign out of My Quaffee</p>
                </div>
            </asp:LinkButton>
        </div>
    </div>
</asp:Content>
