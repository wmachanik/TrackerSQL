<%@ Page Title="System Data" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SystemData.aspx.cs" Inherits="TrackerDotNet.Tools.SystemData" %>

<asp:Content ID="cntSystemDataHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="cntSystemDataBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h1>System Data</h1>
    Test
    <asp:ObjectDataSource ID="odsSystemData" runat="server"
        TypeName="TrackerDotNet.Controls.SysDataTbl"
        SelectMethod="GetAll"
        UpdateMethod="Update"
        DataObjectTypeName="TrackerDotNet.Controls.SysDataTbl"></asp:ObjectDataSource>

    <asp:ObjectDataSource ID="odsItemTypes" runat="server"
        TypeName="TrackerDotNet.Controls.ItemTypeTbl"
        SelectMethod="GetAll"></asp:ObjectDataSource>
    <div class="responsive-layout-container">
        <asp:DetailsView ID="dvSystemData" runat="server"
            AutoGenerateRows="False"
            DataSourceID="odsSystemData"
            CssClass="TblFlex"
            OnItemUpdated="dvSystemData_ItemUpdated"
            OnDataBound="dvSystemData_DataBound">
            <Fields>
                <asp:BoundField DataField="ID" HeaderText="ID" ReadOnly="true" />

                <asp:CheckBoxField DataField="DoReoccuringOrders"
                    HeaderText="Do Reoccuring Orders"
                    SortExpression="DoReoccuringOrders" />

                <asp:BoundField DataField="LastReoccurringDate"
                    HeaderText="Last Reoccurring Date"
                    SortExpression="LastReoccurringDate"
                    DataFormatString="{0:d}"
                    ApplyFormatInEditMode="true" />

                <asp:BoundField DataField="DateLastPrepDateCalcd"
                    HeaderText="Date Last Prep Date Calculated"
                    SortExpression="DateLastPrepDateCalcd"
                    DataFormatString="{0:d}"
                    ApplyFormatInEditMode="true" />

                <asp:BoundField DataField="MinReminderDate"
                    HeaderText="Min Reminder Date"
                    SortExpression="MinReminderDate"
                    DataFormatString="{0:d}"
                    ApplyFormatInEditMode="true" />

                <asp:BoundField DataField="GroupItemTypeID"
                    HeaderText="Group Item Type #"
                    SortExpression="GroupItemTypeID" />

                <asp:BoundField DataField="InternalCustomerIds"
                    HeaderText="Internal Customer IDs"
                    SortExpression="InternalCustomerIds" />

                <asp:CommandField ShowEditButton="True" ButtonType="Image"
                    EditImageUrl="~/images/imgButtons/EditItem.gif"
                    UpdateImageUrl="~/images/imgButtons/UpdateItem.gif"
                    CancelImageUrl="~/images/imgButtons/CancelItem.gif"
                    ItemStyle-HorizontalAlign="Center"
                    ItemStyle-CssClass="command-field-padding" />
            </Fields>
        </asp:DetailsView>
    </div>

    <asp:Label ID="lblMessage" runat="server" ForeColor="Green" Visible="false"></asp:Label>
</asp:Content>
