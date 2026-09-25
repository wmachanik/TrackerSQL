<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DisableClient.aspx.cs" Inherits="TrackerSQL.DisableClient" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Coffee Checkup Reminder Settings - Quaffee</title>
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

        .option-block {
            text-align: left;
            margin: 16px 0;
            padding: 12px 14px;
            background: #fff;
            border: 1px solid #d9e0c8;
            border-radius: 6px;
        }

        .option-block label {
            font-weight: bold;
            color: #2c5530;
        }

        .option-help {
            margin: 6px 0 0 24px;
            font-size: 14px;
            color: #555;
            line-height: 1.4;
        }

        .options-help {
            margin-top: 14px;
            font-size: 14px;
            color: #666;
            text-align: left;
        }
    </style>
</head>
<body>
    <form id="frmDisable" runat="server">
        <div class="container">
            <div class="header">
                <img src="images/logo/QuaffeeLogoSmall.jpg" alt="Quaffee Logo" style="height: 100px; width: auto;" />
                <h1 style="color: #2c5530; margin-top: 15px;">
                    <asp:Literal ID="ltrlPageHeading" runat="server" />
                </h1>
            </div>

            <div id="confirmationSection" runat="server" class="confirmation-section">
                <div class="content">
                    <h2><asp:Literal ID="ltrlConfirmationHeader" runat="server" /></h2>
                    <p style="font-size: 18px; margin: 20px 0;">
                        <asp:Literal ID="ltrlConfirmationMessage" runat="server" />
                    </p>
                    <div class="company-name">
                        <asp:Label ID="CompanyNameLabel" Text="Loading..." runat="server" />
                    </div>
                    <p style="font-size: 16px; color: #666; margin: 25px 0;">
                        <asp:Literal ID="ltrlWarningMessage" runat="server" />
                    </p>
                    <div id="disableOptions" style="text-align: left;">
                        <div class="option-block">
                            <asp:RadioButton ID="rbRemindersOnly" runat="server" GroupName="DisableChoice"
                                Checked="true" />
                            <asp:Label ID="lblRemindersOnly" runat="server" AssociatedControlID="rbRemindersOnly" />
                            <p class="option-help"><asp:Literal ID="ltrlRemindersHelp" runat="server" /></p>
                        </div>
                        <div class="option-block">
                            <asp:RadioButton ID="rbDisableAll" runat="server" GroupName="DisableChoice" />
                            <asp:Label ID="lblDisableAll" runat="server" AssociatedControlID="rbDisableAll" />
                            <p class="option-help"><asp:Literal ID="ltrlAllHelp" runat="server" /></p>
                        </div>
                        <p class="options-help"><asp:Literal ID="ltrlOptionsHelp" runat="server" /></p>
                    </div>
                </div>

                <div class="buttons">
                    <asp:Button ID="btnConfirmDisable" runat="server"
                        CssClass="btn btn-danger"
                        OnClick="btnConfirmDisable_Click"
                        OnClientClick="return confirm('Are you sure you want to update your reminder preferences?');" />

                    <a href="https://quaffee.co.za" class="btn btn-secondary">
                        <asp:Literal ID="ltrlCancelText" runat="server" />
                    </a>
                </div>

                <div style="margin-top: 20px; font-size: 14px; color: #666;">
                    <p>
                        <asp:Literal ID="ltrlHelpMessage" runat="server" />
                        <asp:Literal ID="ltrlContactEmail" runat="server" />
                        or call us to update your preferences.
                    </p>
                </div>
            </div>

            <div id="successSection" runat="server" class="success-message">
                <div class="content">
                    <h2 style="color: #28a745;"><asp:Literal ID="ltrlSuccessHeader" runat="server" /></h2>
                    <p style="font-size: 18px; margin: 20px 0;">
                        <asp:Literal ID="ltrlSuccessMessage" runat="server" />
                    </p>
                    <div class="company-name">
                        <asp:Label ID="CompanyNameSuccessLabel" runat="server" />
                    </div>
                    <p style="font-size: 16px; color: #666; margin: 25px 0;">
                        <asp:Literal ID="ltrlSuccessDetails" runat="server" />
                    </p>
                </div>

                <div style="margin-top: 20px; font-size: 14px; color: #666;">
                    <p>
                        <asp:Literal ID="ltrlReenableMessage" runat="server" />
                    </p>
                </div>
            </div>

            <div id="errorSection" runat="server" style="display: none;">
                <div class="content">
                    <h2 style="color: #c82333;"><asp:Literal ID="ltrlErrorHeader" runat="server" /></h2>
                    <p style="font-size: 18px; margin: 20px 0;">
                        <asp:Literal ID="ltrlErrorMessage" runat="server" />
                    </p>
                </div>
                <div class="buttons">
                    <a href="https://quaffee.co.za" class="btn btn-secondary">
                        <asp:Literal ID="ltrlReturnHomeText" runat="server" />
                    </a>
                </div>
            </div>

            <div class="footer">
               <p>Visit our website: <a href="https://quaffee.co.za" style="color: #2c5530; font-weight: bold;">quaffee.co.za</a></p>
                <p style="font-size: 12px; color: #999;">
                    This page allows you to manage your coffee checkup reminder preferences.
                </p>
            </div>
        </div>
    </form>
</body>
</html>
