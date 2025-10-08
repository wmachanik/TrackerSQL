<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetPasswordHelper.aspx.cs" 
    Inherits="TrackerDotNet.Administration.ResetPasswordHelper" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Password Reset Helper</title>
    <style type="text/css">
        body { font-family: Arial; margin: 20px; }
        .container { width: 400px; margin: 0 auto; }
        .field { margin-bottom: 10px; }
        .success { color: green; }
        .error { color: red; }
    </style>
</head>
<body>
    <form id="frmResetPassword" runat="server">
    <div class="container">
        <h2>Password Reset Helper</h2>
        <div class="field">
            <asp:Label ID="lblUsername" runat="server" Text="Username:" AssociatedControlID="txtUsername"></asp:Label>
            <asp:TextBox ID="txtUsername" runat="server"></asp:TextBox>
        </div>
        <div class="field">
            <asp:Label ID="lblNewPassword" runat="server" Text="New Password:" AssociatedControlID="txtNewPassword"></asp:Label>
            <asp:TextBox ID="txtNewPassword" runat="server" TextMode="Password"></asp:TextBox>
        </div>
        <div class="field">
            <asp:Button ID="btnReset" runat="server" Text="Reset Password" OnClick="btnReset_Click" />
        </div>
        <div class="field">
            <asp:Label ID="lblMessage" runat="server" CssClass="success"></asp:Label>
            <asp:Label ID="lblError" runat="server" CssClass="error"></asp:Label>
        </div>
    </div>
    </form>
</body>
</html>