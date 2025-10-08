<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DisableClient.aspx.cs" Inherits="TrackerDotNet.DisableClient" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Disable Coffee Checkup Reminders - Quaffee</title>
    <style type="text/css">
        body {
            font-family: Calibri, Arial, sans-serif;
            margin: 0;
            padding: 20px;
            background-color: #f5f5f5;
        }

        .container {
            max-width: 600px;
            margin: 0 auto;
            background-color: white;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }

        .header {
            text-align: center;
            margin-bottom: 30px;
        }

        .content {
            background-color: #F9FFDF;
            padding: 30px;
            border-radius: 8px;
            text-align: center;
            margin: 20px 0;
        }

        .company-name {
            font-size: x-large;
            font-weight: bold;
            color: #2c5530;
            margin: 15px 0;
        }

        .buttons {
            text-align: center;
            margin: 30px 0;
        }

        .btn {
            padding: 12px 30px;
            margin: 0 10px;
            border: none;
            border-radius: 5px;
            font-size: 16px;
            cursor: pointer;
            text-decoration: none;
            display: inline-block;
        }

        .btn-danger {
            background-color: #dc3545;
            color: white;
        }

            .btn-danger:hover {
                background-color: #c82333;
            }

        .btn-secondary {
            background-color: #6c757d;
            color: white;
        }

            .btn-secondary:hover {
                background-color: #545b62;
            }

        .footer {
            text-align: center;
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #eee;
        }

        .success-message {
            display: none;
        }

        .confirmation-section {
        }
    </style>
</head>
<body>
    <form id="frmDisable" runat="server">
        <div class="container">
            <div class="header">
                <img src="images/logo/QuaffeeLogoSmall.jpg" alt="Quaffee Logo" />
                <h1 style="color: #2c5530; margin-top: 15px;">Coffee Checkup Reminder Settings</h1>
            </div>

            <!-- Confirmation Section (shown initially) -->
            <div id="confirmationSection" runat="server" class="confirmation-section">
                <div class="content">
                    <h2>📧 Disable Coffee Checkup Reminders?</h2>
                    <p style="font-size: 18px; margin: 20px 0;">
                        You are about to disable coffee checkup reminders for:
                   
                    </p>
                    <div class="company-name">
                        <asp:Label ID="CompanyNameLabel" Text="Loading..." runat="server" />
                    </div>
                    <p style="font-size: 16px; color: #666; margin: 25px 0;">
                        ⚠️ <strong>This will stop all future coffee checkup reminder emails.</strong><br />
                        You can always re-enable reminders by contacting us directly.
                   
                    </p>
                </div>

                <div class="buttons">
                    <asp:Button ID="btnConfirmDisable" runat="server"
                        Text="Yes, Disable Reminders"
                        CssClass="btn btn-danger"
                        OnClick="btnConfirmDisable_Click"
                        OnClientClick="return confirm('Are you sure you want to disable coffee checkup reminders? This action will stop all future reminder emails.');" />

                    <a href="http://www.quaffee.co.za" class="btn btn-secondary">Cancel & Keep Reminders
                    </a>
                </div>

                <div style="margin-top: 20px; font-size: 14px; color: #666;">
                    <p>
                        💡 <strong>Need help instead?</strong><br />
                        Contact us at
                        <asp:Literal ID="ltrlContactEmail" runat="server" />
                        or call us to update your preferences.  
                    </p>
                </div>
            </div>

            <!-- Success Section (shown after confirmation) -->
            <div id="successSection" runat="server" class="success-message">
                <div class="content">
                    <h2 style="color: #28a745;">✅ Reminders Disabled Successfully</h2>
                    <p style="font-size: 18px; margin: 20px 0;">
                        Coffee checkup reminders have been disabled for:
                   
                    </p>
                    <div class="company-name">
                        <asp:Label ID="CompanyNameSuccessLabel" runat="server" />
                    </div>
                    <p style="font-size: 16px; color: #666; margin: 25px 0;">
                        Our administrator has been notified of this change.<br />
                        You will no longer receive automated coffee checkup reminder emails.
                   
                    </p>
                </div>

                <div style="margin-top: 20px; font-size: 14px; color: #666;">
                    <p>
                        <strong>Need to re-enable reminders?</strong><br />
                        Contact us at
                        <asp:Literal ID="ltrlContactEmailSuccess" runat="server" />
                        and we'll be happy to help.
                    </p>
                </div>
            </div>

            <div class="footer">
                <p>Visit our website: <a href="http://www.quaffee.co.za" style="color: #2c5530; font-weight: bold;">www.quaffee.co.za</a></p>
                <p style="font-size: 12px; color: #999;">
                    This page allows you to manage your coffee checkup reminder preferences.
               
                </p>
            </div>
        </div>
    </form>
</body>
</html>
