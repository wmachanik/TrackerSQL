<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ViewMyOrder.aspx.cs" Inherits="TrackerSQL.Pages.ViewMyOrder" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Order</title>
    <meta http-equiv="Cache-Control" content="no-store, no-cache, must-revalidate" />
    <meta http-equiv="Pragma" content="no-cache" />
    <meta http-equiv="Expires" content="-1" />
    <style>
        body { font-family: Arial, sans-serif; margin:16px; }
        h1 { font-size:20px; margin:0 0 4px 0; }
        .meta { color:#555; font-size:12px; margin:0 0 14px 0; }
        table { border-collapse:collapse; width:100%; max-width:900px; }
        th, td { border:1px solid #ccc; padding:6px 8px; font-size:13px; }
        th { background:#f3f3f3; text-align:left; }
        tfoot td { background:#fafafa; font-weight:bold; }
        .status { margin:14px 0; color:#b00; }
        .ok { margin:14px 0; color:#060; }
        .warn { background:#fff4d6; border:1px solid #f0c36d; padding:8px 10px; margin:12px 0; font-size:12px; }
        .change-box { margin-top:28px; max-width:900px; }
        textarea { width:100%; min-height:120px; resize:vertical; font-family: Consolas, monospace; }
        .btn { padding:6px 14px; cursor:pointer; font-size:13px; }
        .footer { margin-top:40px; font-size:11px; color:#777; }
        .empty { font-style:italic; color:#666; }
    </style>
</head>
<body>
    <form id="formViewMyOrder" runat="server">
        <h1 id="hdrTitle" runat="server">Order</h1>
        <div class="meta" id="hdrMeta" runat="server"></div>
        <asp:Literal ID="litHeaderNotes" runat="server" />
        <asp:PlaceHolder ID="phError" runat="server" Visible="false">
            <div id="errBox" runat="server" class="status"></div>
        </asp:PlaceHolder>

        <asp:PlaceHolder ID="phOrder" runat="server" Visible="false">
            <table style="width:auto">
                <thead>
                    <tr>
                        <th style="width:50%">Item</th>
                        <th style="width:15%">Packaging</th>
                        <th style="width:10%">Qty</th>
                        <th style="width:10%">UoM</th>
                    </tr>
                </thead>
                <tbody id="tbodyLines" runat="server"></tbody>
                <tfoot>
                    <tr>
                        <td colspan="2">Total Lines</td>
                        <td colspan="3"><asp:Literal ID="litTotalLines" runat="server" /></td>
                    </tr>
                </tfoot>
            </table>

            <div class="change-box">
                <h2 style="font-size:16px; margin:20px 0 8px 0;">Request a Change</h2>
                <asp:Literal ID="litChangeResult" runat="server" />
                <asp:TextBox ID="tbxChangeRequest" runat="server" TextMode="MultiLine" />
                <div style="margin-top:8px">
                    <asp:Button ID="btnSubmitChange" runat="server" Text="Submit Request" CssClass="btn"
                        OnClick="btnSubmitChange_Click" />
                </div>
            </div>
        </asp:PlaceHolder>

        <div class="footer">
            View generated at <%= DateTime.UtcNow.ToString("u") %> (UTC).
        </div>
    </form>
</body>
</html>