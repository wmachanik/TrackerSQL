<%@ Page Title="Move Delivery Date" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MoveDeliveryDate.aspx.cs" Inherits="TrackerDotNet.Tools.MoveDeliveryDate" %>
<asp:Content ID="cntMoveDeliveryDateHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntMoveDeliveryDateBdy" ContentPlaceHolderID="MainContent" runat="server">
  <h1></h1>


  <asp:ScriptManager ID="smgrMoveDeliveryDate"  runat="server" />
  <asp:UpdateProgress ID="UpdateProgress1" runat="server">
    <ProgressTemplate>&nbsp;&nbsp;
      <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
    </ProgressTemplate>
  </asp:UpdateProgress>
  <asp:UpdatePanel ID="upnlMoveDeliveryDate" runat="server" ChildrenAsTriggers="true"  >
    <ContentTemplate>
      <table class="TblCoffee">
        <tbody>
          <tr>
            <td>Old Delivery Date</td>
            <td>
              <asp:DropDownList ID="OldDeliveryDateDDL" runat="server" DataTextFormatString="{0:d}" 
                DataSourceID="odsCityDeliveryDates" DataTextField="Date" DataValueField="Date"></asp:DropDownList>
              <asp:ObjectDataSource ID="odsCityDeliveryDates" runat="server" 
                SelectMethod="GetAllDeliveryDates" 
                TypeName="TrackerDotNet.Controls.NextRoastDateByCityTbl">
              </asp:ObjectDataSource>
            </td>
          </tr>
          <tr>
            <td>New Delivery Date</td>
            <td>
              <asp:TextBox ID="NewDeliveryDateTextBox" runat="server"  Text="" />
              <ajaxToolkit:CalendarExtender ID="NewDeliveryDateTextBox_CalendarExtender" runat="server" CssClass="small" 
                      Enabled="True" TargetControlID="NewDeliveryDateTextBox" ClearTime="true" >
              </ajaxToolkit:CalendarExtender>
            </td>
          </tr>
          <tr>
            <td colspan="2" align="center">
              <asp:Button ID="btnMove" Text="Move" runat="server" 
                onclick="btnMove_Click" />&nbsp;&nbsp;&nbsp;&nbsp;
              <asp:Button ID="btnCancel" Text="Cancel" runat="server" />
            </td>
          </tr>
        </tbody>
      </table>
      <br />
      <asp:Literal ID="StatusLiteral" Text="" runat="server" />
      <asp:GridView ID="gvPrepData" runat="server" AllowPaging="True" CssClass="AutoWidthFrm" 
          AutoGenerateColumns="False" DataSourceID="sdsCityPrepDates" AllowSorting="true" >
          <Columns>
            <asp:BoundField DataField="City" HeaderText="City" SortExpression="City" />
            <asp:BoundField DataField="PreperationDate" DataFormatString="{0:d}" 
              HeaderText="PreperationDate" SortExpression="PreperationDate" />
            <asp:BoundField DataField="DeliveryDate" DataFormatString="{0:d}" 
              HeaderText="DeliveryDate" SortExpression="DeliveryDate" />
            <asp:BoundField DataField="NextPreperationDate" DataFormatString="{0:d}" 
              HeaderText="NextPreperationDate" SortExpression="NextPreperationDate" />
            <asp:BoundField DataField="NextDeliveryDate" DataFormatString="{0:d}" 
              HeaderText="NextDeliveryDate" SortExpression="NextDeliveryDate" />
          </Columns>
        </asp:GridView>
        <asp:SqlDataSource ID="sdsCityPrepDates" runat="server" 
          ConnectionString="<%$ ConnectionStrings:Tracker08ConnectionString %>" 
          ProviderName="<%$ ConnectionStrings:Tracker08ConnectionString.ProviderName %>" 
          SelectCommand="SELECT CityTbl.City, NextRoastDateByCityTbl.PreperationDate, NextRoastDateByCityTbl.DeliveryDate, NextRoastDateByCityTbl.NextPreperationDate, NextRoastDateByCityTbl.NextDeliveryDate FROM (NextRoastDateByCityTbl LEFT OUTER JOIN CityTbl ON NextRoastDateByCityTbl.CityID = CityTbl.ID) ORDER BY NextRoastDateByCityTbl.DeliveryDate, CityTbl.City">
        </asp:SqlDataSource>
    </ContentTemplate>
    <Triggers>
      <asp:AsyncPostBackTrigger ControlID="btnMove" EventName="Click" />
    </Triggers>
  </asp:UpdatePanel>
</asp:Content>
