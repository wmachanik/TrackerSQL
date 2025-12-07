<%@ Page Title="Lookup Tables" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Lookups.aspx.cs" Inherits="TrackerSQL.Pages.Lookups" MaintainScrollPositionOnPostback="true" %>

<asp:Content ID="cntLookupHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntLookupBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="scmLookup" runat="server">
    </asp:ScriptManager>
    <asp:UpdateProgress ID="uprgLookup" runat="server">
        <ProgressTemplate>
            Please Wait&nbsp;<img src="../images/animi/QuaffeeProgress.gif" alt="Please Wait..." />&nbsp;...
        </ProgressTemplate>
    </asp:UpdateProgress>
    <h2>Tables...</h2>
    <asp:Label ID="lblStatus" runat="server" />
    <ajaxToolkit:TabContainer ID="tabcLookup" runat="server" ActiveTabIndex="5" CssClass="MyTabStyle" ScrollBars="None" UseVerticalStripPlacement="false">
        <ajaxToolkit:TabPanel runat="server" HeaderText="Items" ID="tabpnlItems">
            <HeaderTemplate>
                Items
            </HeaderTemplate>
            <ContentTemplate>
                <asp:UpdatePanel ID="upnlItems" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div class="filter-toolbar">
                            <div class="filter-section search-controls">
                                <div class="filter-control">
                                    <label for="<%=tbxItemSearch.ClientID%>">Search:</label>
                                    <asp:TextBox ID="tbxItemSearch" runat="server" OnTextChanged="tbxItemSearch_TextChanged" />
                                </div>
                                <asp:Button ID="btnGon" Text="Go" runat="server" ToolTip="search for this item" OnClick="btnGo_Click" />
                                <asp:Button ID="btnReset" Text="Reset" runat="server" OnClick="btnReset_Click" />
                            </div>
                        </div>
                        <div class="results-container scrollable-table-container">
                            <asp:GridView ID="gvItems" runat="server" DataSourceID="sdsItems" AllowPaging="True"
                                PageSize="20" CssClass="results-table" Font-Size="Small" AllowSorting="True" AutoGenerateColumns="False"
                                OnRowCommand="gvItems_RowCommand" ShowFooter="True" CellPadding="0"
                                PagerStyle-CssClass="aspNetPager"
                                DataKeyNames="ItemTypeID">
                                <Columns>
                                    <asp:BoundField DataField="ItemTypeID" HeaderText="ItemTypeID" InsertVisible="True"
                                        ReadOnly="True" SortExpression="ItemTypeID" Visible="False" />
                                    <asp:TemplateField HeaderText="Item" SortExpression="ItemDesc">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="tbxEItem" runat="server" Text='<%# Bind("ItemDesc") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="tbxItem" runat="server" Text="" Width="10em" />
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblItem" runat="server" Text='<%# Bind("ItemDesc") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="SKU" SortExpression="SKUDesc">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="tbxESKU" runat="server" Width="4.5em" Text='<%# Bind("SKU") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="tbxSKU" runat="server" Width="4.5em" Text="" />
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblSKU" runat="server" Text='<%# Bind("SKU") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Enbld" SortExpression="ItemEnabled">
                                        <EditItemTemplate>
                                            <asp:CheckBox ID="cbxItemEnabled" runat="server" Checked='<%# Bind("ItemEnabled") %>' />
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:CheckBox ID="cbxItemEnabled" runat="server" Checked="true" />
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:CheckBox ID="cbxItemEnabled" runat="server" Checked='<%# Bind("ItemEnabled") %>'
                                                Enabled="false" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Characteritics" SortExpression="ItemsCharacteritics">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="tbxItemCharacteristics" runat="server" Width="8em" Text='<%# Bind("ItemsCharacteritics") %>' />
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="tbxItemCharacteristics" runat="server" Width="8em" Text="" />
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblItemCharacteristics" runat="server" Text='<%# Bind("ItemsCharacteritics") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Detail" SortExpression="ItemDetail">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="tbxItemDetail" runat="server" Width="8em" Text='<%# Bind("ItemDetail") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="tbxItemDetail" runat="server" Width="8em" Text="" />
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblItemDetail" runat="server" Text='<%# Bind("ItemDetail") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Type" SortExpression="ServiceTypeId">
                                        <EditItemTemplate>
                                            <asp:DropDownList ID="ddlServiceType" runat="server" DataSourceID="sdsServiceTypes"
                                                AppendDataBoundItems="true" DataTextField="ServiceType" DataValueField="ServiceTypeId"
                                                SelectedValue='<%# Bind("ServiceTypeId") %>' Width="10em">
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:DropDownList ID="ddlServiceType" runat="server" DataSourceID="sdsServiceTypes"
                                                AppendDataBoundItems="true" DataTextField="ServiceType" DataValueField="ServiceTypeId"
                                                SelectedValue='<%# Bind("ServiceTypeId") %>' Width="10em">
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlServiceType" runat="server" DataSourceID="sdsServiceTypes"
                                                AppendDataBoundItems="true" DataTextField="ServiceType" DataValueField="ServiceTypeId"
                                                Enabled="False" SelectedValue='<%# Bind("ServiceTypeId") %>' Width="10em">
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Replcment" SortExpression="Replacement">
                                        <EditItemTemplate>
                                            <asp:DropDownList ID="ddlReplacement" runat="server" AppendDataBoundItems="True"
                                                DataSourceID="odsAllItems" DataTextField="ItemDesc" DataValueField="ItemTypeID"
                                                SelectedValue='<%# Bind("Replacement") %>'>
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:DropDownList ID="ddlReplacement" runat="server" AppendDataBoundItems="True"
                                                DataSourceID="odsAllItems" DataTextField="ItemDesc" DataValueField="ItemTypeID"
                                                SelectedValue='<%# Bind("Replacement") %>'>
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlReplacement" runat="server" AppendDataBoundItems="True"
                                                DataSourceID="odsAllItems" DataTextField="ItemDesc" DataValueField="ItemTypeID"
                                                SelectedValue='<%# Bind("Replacement") %>'>
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Abrv" SortExpression="ItemShortName">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="tbxItemShortName" runat="server" Width="4em" Text='<%# Bind("ItemShortName") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="tbxItemShortName" runat="server" Text="" Width="4em" />
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblItemShortName" runat="server" Text='<%# Bind("ItemShortName") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Qty" SortExpression="UnitsPerQty">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="tbxUnitsPerQtyr" runat="server" Width="1.1em" Text='<%# Bind("UnitsPerQty") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="tbxUnitsPerQty" runat="server" Text='1' Width="1.1em" />
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblUnitsPerQty" runat="server" Width="1.1em" Text='<%# Bind("UnitsPerQty") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="UoM" SortExpression="UoMID">
                                        <EditItemTemplate>
                                            <asp:DropDownList ID="ddlUnits" runat="server" AppendDataBoundItems="True"
                                                DataSourceID="odsItemUnits" DataTextField="UnitOfMeasure"
                                                DataValueField="ItemUnitID" SelectedValue='<%# Bind("UoMID") %>'>
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:DropDownList ID="ddlUnits" runat="server" AppendDataBoundItems="True"
                                                DataSourceID="odsItemUnits" DataTextField="UnitOfMeasure"
                                                DataValueField="ItemUnitID" SelectedValue='<%# Bind("UoMID") %>'>
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlUnits" runat="server" AppendDataBoundItems="True"
                                                DataSourceID="odsItemUnits" DataTextField="UnitOfMeasure"
                                                DataValueField="ItemUnitID" SelectedValue='<%# Bind("UoMID") %>'>
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="S/O" SortExpression="SortOrder">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="tbxSortOrder" runat="server" Width="1.1em" Text='<%# Bind("SortOrder") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="tbxSortOrder" runat="server" Text='1' Width="1.1em" />
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblSortOrder" runat="server" Width="1.1em" Text='<%# Bind("SortOrder") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ShowHeader="False">
                                        <EditItemTemplate>
                                            <asp:ImageButton ID="btnUpdate" runat="server" CausesValidation="False" CommandName="Update"
                                                AlternateText="go" ImageUrl="~/images/imgButtons/UpdateItem.gif" />
                                            <asp:ImageButton ID="btnCancel" runat="server" CausesValidation="False" CommandName="Cancel"
                                                AlternateText="no" ImageUrl="~/images/imgButtons/CancelItem.gif" />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:ImageButton ID="btnEdit" runat="server" CausesValidation="False" CommandName="Edit"
                                                AlternateText="Edit" ImageUrl="~/images/imgButtons/EditItem.gif" />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:ImageButton ID="btnAdd" runat="server" CausesValidation="False" CommandName="AddItem"
                                                ImageUrl="~/images/imgButtons/AddItem.gif" AlternateText="Add" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <EditRowStyle BackColor="#24BF61" />
                                <EmptyDataTemplate>
                                    <asp:DetailsView ID="dvItemIns" runat="server" AutoGenerateRows="False" BackColor="White"
                                        BorderColor="#DEDFDE" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" DataKeyNames="ItemTypeID"
                                        DataSourceID="sdsItems" OnItemInserted="dvItems_ItemInserted" ForeColor="Black"
                                        PagerStyle-CssClass="aspNetPager"
                                        GridLines="Vertical" Width="220px">
                                        <AlternatingRowStyle BackColor="White" />
                                        <EditRowStyle BackColor="#CE5D5A" Font-Bold="True" ForeColor="White" />
                                        <Fields>
                                            <asp:BoundField DataField="ItemDesc" HeaderText="ItemDesc" SortExpression="ItemDesc" />
                                            <asp:BoundField DataField="ItemsCharacteritics" HeaderText="ItemsCharacteritics"
                                                SortExpression="ItemsCharacteritics" />
                                            <asp:BoundField DataField="ItemDetail" HeaderText="ItemDetail" SortExpression="ItemDetail" />
                                            <asp:TemplateField HeaderText="ServiceType" SortExpression="ServiceTypeId">
                                                <EditItemTemplate>
                                                    <asp:DropDownList ID="ddlEditServiceType" runat="server" DataSourceID="sdsServiceTypes"
                                                        DataTextField="ServiceType" DataValueField="ServiceTypeId" SelectedValue='<%# Bind("ServiceTypeId") %>'>
                                                    </asp:DropDownList>
                                                </EditItemTemplate>
                                                <InsertItemTemplate>
                                                    <asp:DropDownList ID="ddlInsServiceType" runat="server" DataSourceID="sdsServiceTypes"
                                                        DataTextField="ServiceType" DataValueField="ServiceTypeId" SelectedValue='<%# Bind("ServiceTypeId") %>'>
                                                    </asp:DropDownList>
                                                </InsertItemTemplate>
                                                <ItemTemplate>
                                                    <asp:DropDownList ID="ddlServiceType" runat="server" DataSourceID="sdsServiceTypes"
                                                        DataTextField="ServiceType" DataValueField="ServiceTypeId" SelectedValue='<%# Bind("ServiceTypeId") %>'>
                                                    </asp:DropDownList>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Replacement">
                                                <EditItemTemplate>
                                                    <asp:DropDownList ID="ddlEditReplacement" runat="server" AppendDataBoundItems="True"
                                                        DataSourceID="odsAllItems" DataTextField="ItemDesc" DataValueField="ItemTypeID"
                                                        SelectedValue='<%# Bind("Replacement") %>'>
                                                        <asp:ListItem Value="0" Text="n/a" />
                                                    </asp:DropDownList>
                                                </EditItemTemplate>
                                                <InsertItemTemplate>
                                                    <asp:DropDownList ID="ddlInsReplacement" runat="server" AppendDataBoundItems="True"
                                                        DataSourceID="odsAllItems" DataTextField="ItemDesc" DataValueField="ItemTypeID"
                                                        SelectedValue='<%# Bind("Replacement") %>'>
                                                        <asp:ListItem Value="0" Text="n/a" />
                                                    </asp:DropDownList>
                                                </InsertItemTemplate>
                                                <ItemTemplate>
                                                    <asp:DropDownList ID="ddlReplacement" runat="server" AppendDataBoundItems="True"
                                                        DataSourceID="odsAllItems" DataTextField="ItemDesc" DataValueField="ItemTypeID"
                                                        SelectedValue='<%# Bind("Replacement") %>'>
                                                        <asp:ListItem Value="0" Text="n/a" />
                                                    </asp:DropDownList>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:DropDownList ID="ddlInsReplacement" runat="server" AppendDataBoundItems="True"
                                                        DataSourceID="odsAllItems" DataTextField="ItemDesc" DataValueField="ItemTypeID"
                                                        SelectedValue='<%# Bind("Replacement") %>'>
                                                        <asp:ListItem Value="0" Text="n/a" />
                                                    </asp:DropDownList>
                                                </FooterTemplate>
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="ItemShortName" HeaderText="ItemShortName" SortExpression="ItemShortName" />
                                            <asp:BoundField DataField="SortOrder" HeaderText="SortOrder" SortExpression="SortOrder" />
                                            <asp:CheckBoxField DataField="ItemEnabled" HeaderText="ItemEnabled" SortExpression="ItemEnabled" />
                                            <asp:CommandField ShowEditButton="True" ShowInsertButton="True" ButtonType="Image"
                                                EditImageUrl="~/images/imgButtons/EditItem.gif" UpdateImageUrl="~/images/imgButtons/UpdateItem.gif"
                                                CancelImageUrl="~/images/imgButtons/CancelItem.gif" InsertImageUrl="~/images/imgButtons/AddItem.gif" />
                                        </Fields>
                                        <FooterStyle BackColor="#CCCC99" BorderStyle="Dashed" BorderColor="Cornsilk" />
                                        <HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="White" />
                                        <PagerStyle BackColor="#F7F7DE" ForeColor="Black" HorizontalAlign="Right" />
                                        <RowStyle BackColor="#F7F7DE" />
                                    </asp:DetailsView>
                                </EmptyDataTemplate>
                                <FooterStyle BackColor="#50D17C" Font-Bold="True" ForeColor="White" BorderStyle="Dashed" BorderColor="Cornsilk" />
                                <HeaderStyle BackColor="#D0D17C" Font-Bold="True" ForeColor="Black" />
                                <PagerSettings FirstPageImageUrl="~/images/imgButtons/FirstPage.gif" LastPageImageUrl="~/images/imgButtons/LastPage.gif"
                                    Mode="NumericFirstLast" NextPageImageUrl="~/images/imgButtons/NextPage.gif"
                                    PreviousPageImageUrl="~/images/imgButtons/PrevPage.gif" />

                            </asp:GridView>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <asp:ObjectDataSource ID="odsItemUnits" runat="server" SelectMethod="GetAll"
                    TypeName="TrackerSQL.Controls.ItemUnitsTbl"
                    OldValuesParameterFormatString="original_{0}">
                    <SelectParameters>
                        <asp:Parameter DefaultValue="UnitOfMeasure" Name="SortBy" Type="String" />
                    </SelectParameters>
                </asp:ObjectDataSource>
            </ContentTemplate>
        </ajaxToolkit:TabPanel>
        <ajaxToolkit:TabPanel ID="tabpnlPeople" runat="server" HeaderText="People">
            <HeaderTemplate>
                People
            </HeaderTemplate>
            <ContentTemplate>
                <asp:UpdatePanel ID="upnlPeople" runat="server" UpdateMode="Conditional">
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="gvPeople" />
                    </Triggers>
                    <ContentTemplate>
                        <div class="results-container">
                            <asp:GridView ID="gvPeople" runat="server" AllowPaging="True" AllowSorting="True"
                                AutoGenerateColumns="False" CellPadding="1" PageSize="20" DataKeyNames="PersonID"
                                OnRowCommand="gvPeople_RowCommand" OnRowUpdating="gvPeople_RowUpdating"
                                OnRowEditing="gvPeople_RowEditing" OnRowDataBound="gvPeople_RowDataBound"
                                DataSourceID="odsPeople" CssClass="results-table" ShowFooter="True">
                                <Columns>
                                    <asp:TemplateField ShowHeader="False">
                                        <EditItemTemplate>
                                            <asp:ImageButton ID="btnUpdate" runat="server" CausesValidation="False" CommandName="Update"
                                                AlternateText="go" ImageUrl="~/images/imgButtons/UpdateItem.gif" />
                                            <asp:ImageButton ID="btnCancel" runat="server" CausesValidation="False" CommandName="Cancel"
                                                AlternateText="no" ImageUrl="~/images/imgButtons/CancelItem.gif" />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:ImageButton ID="btnEdit" runat="server" CausesValidation="False"
                                                CommandName="Edit" CommandArgument='<%# Container.DataItemIndex %>'
                                                AlternateText="Edit" ImageUrl="~/images/imgButtons/EditItem.gif" />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:ImageButton ID="btnAdd" runat="server" CausesValidation="False" CommandName="AddItem"
                                                ImageUrl="~/images/imgButtons/AddItem.gif" AlternateText="Add" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="PersonID" HeaderText="PersonID" InsertVisible="False"
                                        ReadOnly="True" SortExpression="PersonID" />
                                    <asp:TemplateField HeaderText="Person" SortExpression="Person">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="tbxPerson" runat="server" Width="8em" Text='<%# Bind("Person") %>' />
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="tbxPerson" runat="server" Width="8em" Text="" />
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblPerson" runat="server" Text='<%# Bind("Person") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Abbreviation" SortExpression="Abbreviation">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="tbxAbbreviation" runat="server" Width="8em" Text='<%# Bind("Abbreviation") %>' />
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="tbxAbbreviation" runat="server" Width="8em" Text="" />
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblAbbreviation" runat="server" Text='<%# Bind("Abbreviation") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Enbld" SortExpression="Enabled">
                                        <EditItemTemplate>
                                            <asp:CheckBox ID="cbxEnabled" runat="server" Checked='<%# Bind("Enabled") %>' />
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:CheckBox ID="cbxEnabled" runat="server" Checked="true" />
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:CheckBox ID="cbxEnabled" runat="server" Checked='<%# Bind("Enabled") %>'
                                                Enabled="false" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Normal Delivery DoW" SortExpression="NormalDeliveryDoW">
                                        <EditItemTemplate>
                                            <asp:DropDownList ID="ddlDayOfWeek" runat="server" SelectedValue='<%# Bind("NormalDeliveryDoW") %>'>
                                                <asp:ListItem Value="0" Text="0 - Any Day" />
                                                <asp:ListItem Value="1" Text="1 - Sunday" />
                                                <asp:ListItem Value="2" Text="2 - Monday" />
                                                <asp:ListItem Value="3" Text="3 - Tuesday" />
                                                <asp:ListItem Value="4" Text="4 - Wednesday" />
                                                <asp:ListItem Value="5" Text="5 - Thursday" />
                                                <asp:ListItem Value="6" Text="6 - Friday" />
                                                <asp:ListItem Value="7" Text="7 - Saturday" />
                                            </asp:DropDownList>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:DropDownList ID="ddlDayOfWeek" runat="server">
                                                <asp:ListItem Value="0" Text="0 - Any Day" Selected="True" />
                                                <asp:ListItem Value="1" Text="1 - Sunday" />
                                                <asp:ListItem Value="2" Text="2 - Monday" />
                                                <asp:ListItem Value="3" Text="3 - Tuesday" />
                                                <asp:ListItem Value="4" Text="4 - Wednesday" />
                                                <asp:ListItem Value="5" Text="5 - Thursday" />
                                                <asp:ListItem Value="6" Text="6 - Friday" />
                                                <asp:ListItem Value="7" Text="7 - Saturday" />
                                            </asp:DropDownList>
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlDayOfWeek" runat="server"
                                                SelectedValue='<%# Bind("NormalDeliveryDoW") %>'>
                                                <asp:ListItem Value="0" Text="0 - Any Day" />
                                                <asp:ListItem Value="1" Text="1 - Sunday" />
                                                <asp:ListItem Value="2" Text="2 - Monday" />
                                                <asp:ListItem Value="3" Text="3 - Tuesday" />
                                                <asp:ListItem Value="4" Text="4 - Wednesday" />
                                                <asp:ListItem Value="5" Text="5 - Thursday" />
                                                <asp:ListItem Value="6" Text="6 - Friday" />
                                                <asp:ListItem Value="7" Text="7 - Saturday" />
                                            </asp:DropDownList>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Username" SortExpression="SecurityUsername">
                                        <EditItemTemplate>
                                            <asp:DropDownList ID="ddlSecurityNames" runat="server" AppendDataBoundItems="True"
                                                DataSourceID="sdsUserNames" DataTextField="SecurityUsername" DataValueField="SecurityUsername">
                                                <asp:ListItem Value="" Text="n/a" />
                                            </asp:DropDownList>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:DropDownList ID="ddlSecurityNames" runat="server" DataSourceID="sdsUserNames" AppendDataBoundItems="True"
                                                DataTextField="SecurityUsername" DataValueField="SecurityUsername">
                                                <asp:ListItem Value="" Text="n/a" />
                                            </asp:DropDownList>
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblSecurityName" runat="server" Text='<%# Eval("SecurityUsername") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <EditRowStyle BackColor="#7C6F57" />
                            </asp:GridView>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </ContentTemplate>
        </ajaxToolkit:TabPanel>
        <ajaxToolkit:TabPanel ID="tabpnlEquipment" runat="server" HeaderText="Equipment">
            <ContentTemplate>
                <asp:UpdatePanel ID="upnlEquipment" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div class="results-container">
                            <asp:GridView ID="gvEquipment" runat="server" AllowPaging="True" EmptyDataText="No equipment found"
                                AllowSorting="True" AutoGenerateColumns="False" BackColor="White" ShowFooter="True"
                                BorderColor="#E7E7FF" BorderStyle="None" BorderWidth="1px" CellPadding="3" DataKeyNames="EquipTypeId"
                                OnRowCommand="gvEquipment_RowCommand" DataSourceID="odsEquipTypes" PageSize="20"
                                CssClass="results-table"
                                OnSelectedIndexChanged="gvEquipment_SelectedIndexChanged">
                                <AlternatingRowStyle BackColor="#F7F7F7" />
                                <Columns>
                                    <asp:TemplateField ShowHeader="False">
                                        <EditItemTemplate>
                                            <asp:ImageButton ID="btnUpdate" runat="server" CausesValidation="False" CommandName="Update"
                                                AlternateText="go" ImageUrl="~/images/imgButtons/UpdateItem.gif" />
                                            &nbsp;
                                            <asp:ImageButton ID="btnCancel" runat="server" CausesValidation="False" CommandName="Cancel"
                                                AlternateText="no" ImageUrl="~/images/imgButtons/CancelItem.gif" />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:ImageButton ID="btnEdit" runat="server" CausesValidation="False" CommandName="Edit"
                                                AlternateText="Edit" ImageUrl="~/images/imgButtons/EditItem.gif" />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:ImageButton ID="btnAdd" runat="server" CausesValidation="False" CommandName="Insert"
                                                ImageUrl="~/images/imgButtons/AddItem.gif" AlternateText="Add" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="EquipTypeName" SortExpression="EquipTypeName">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="EquipTypeNameTextBox" runat="server" Text='<%# Bind("EquipTypeName") %>'></asp:TextBox>
                                            <asp:HiddenField ID="EquipTypeIdLabel" runat="server" Value='<%# Eval("EquipTypeId") %>' />
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="EquipTypeNameTextBox" runat="server" Text="" />
                                            <asp:HiddenField ID="EquipTypeIdLabel" runat="server" Value='<%# Eval("EquipTypeId") %>' />
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="EquipTypeNameLabel" runat="server" Text='<%# Bind("EquipTypeName") %>'></asp:Label>
                                            <asp:HiddenField ID="EquipTypeIdLabel" runat="server" Value='<%# Eval("EquipTypeId") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="EquipTypeDesc" SortExpression="EquipTypeDesc">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="EquipTypeDescTextBox" runat="server" Text='<%# Bind("EquipTypeDesc") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="EquipTypeDescTextBox" runat="server" Text="" />
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="EquipTypeDescLabel" runat="server" Text='<%# Bind("EquipTypeDesc") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <FooterStyle BackColor="#B5C7DE" ForeColor="#4A3C8C" BorderStyle="Dashed" BorderColor="Cornsilk" />
                                <PagerStyle BackColor="#E7E7FF" ForeColor="#4A3C8C" HorizontalAlign="Right" />
                                <SelectedRowStyle BackColor="#738A9C" Font-Bold="True" ForeColor="#F7F7F7" />
                                <SortedAscendingCellStyle BackColor="#F4F4FD" />
                                <SortedAscendingHeaderStyle BackColor="#5A4C9D" ForeColor="AliceBlue" />
                                <SortedDescendingCellStyle BackColor="#D8D8F0" />
                                <SortedDescendingHeaderStyle BackColor="#3E3277" />
                            </asp:GridView>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </ContentTemplate>
        </ajaxToolkit:TabPanel>
        <ajaxToolkit:TabPanel ID="tabpnlCities" runat="server" HeaderText="Cities">
            <ContentTemplate>
                <asp:UpdatePanel ID="upnlCities" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="responsive-layout-container">
                            <div class="layout-main-panel">
                                <div class="results-container">
                                    <asp:GridView ID="gvCities" runat="server" AllowPaging="True" PageSize="20" AllowSorting="True"
                                        AutoGenerateColumns="False" BackColor="White" BorderColor="#DEDFDE" BorderStyle="None" CssClass="results-table"
                                        BorderWidth="1px" CellPadding="4" DataSourceID="sdsCities" ForeColor="Black" DataKeyNames="ID"
                                        ShowFooter="True" OnRowCommand="gvCities_OnRowCommand" OnSelectedIndexChanged="gvCities_OnSelectedIndexChanged">
                                        <AlternatingRowStyle BackColor="White" />
                                        <Columns>
                                            <asp:CommandField ShowSelectButton="True" SelectImageUrl="~/images/imgButtons/SelectItem.gif" ButtonType="Image" />
                                            <asp:TemplateField HeaderText="City" SortExpression="City">
                                                <EditItemTemplate>
                                                    <asp:TextBox ID="tbxCity" runat="server" Text='<%# Bind("City") %>'></asp:TextBox>
                                                </EditItemTemplate>
                                                <ItemTemplate>
                                                    <asp:Label ID="lblCity" runat="server" Text='<%# Bind("City") %>'></asp:Label>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:TextBox ID="tbxCity" runat="server" Text=""></asp:TextBox>
                                                </FooterTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="ID" Visible="false">
                                                <EditItemTemplate>
                                                    <asp:Label ID="lblCityID" runat="server" Text='<%# Bind("ID") %>' />
                                                </EditItemTemplate>
                                                <ItemTemplate>
                                                    <asp:Label ID="lblCityID" runat="server" Text='<%# Bind("ID") %>' />
                                                </ItemTemplate>
                                                <FooterTemplate></FooterTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField ShowHeader="False">
                                                <EditItemTemplate>
                                                    <asp:ImageButton ID="btnCityUpdate" runat="server" CausesValidation="True" CommandName="Update"
                                                        AlternateText="Update" ImageUrl="~/images/imgButtons/UpdateItem.gif" />
                                                    &nbsp;<asp:ImageButton ID="btnCityCancel" runat="server" CausesValidation="False"
                                                        CommandName="Cancel" AlternateText="Cancel" ImageUrl="~/images/imgButtons/CancelItem.gif" />
                                                </EditItemTemplate>
                                                <ItemTemplate>
                                                    <asp:ImageButton ID="btnCityEdit" runat="server" CausesValidation="False" CommandName="Edit"
                                                        AlternateText="Edit" ImageUrl="~/images/imgButtons/EditItem.gif" />
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:ImageButton ID="btnCityInsert" runat="server" CommandName="AddCity" AlternateText="Ins"
                                                        ImageUrl="~/images/imgButtons/AddItem.gif" />
                                                </FooterTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                        <FooterStyle BackColor="#CCCC99" BorderStyle="Dashed" BorderColor="Cornsilk" />
                                        <HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="Brown" />
                                        <PagerStyle BackColor="#F7F7DE" ForeColor="Black" HorizontalAlign="Right" />
                                        <RowStyle BackColor="#F7F7DE" />
                                        <SelectedRowStyle BackColor="#63cb66" Font-Bold="True" ForeColor="#4e664d" />
                                        <SortedAscendingCellStyle BackColor="#FBFBF2" />
                                        <SortedAscendingHeaderStyle BackColor="#848384" />
                                        <SortedDescendingCellStyle BackColor="#EAEAD3" />
                                        <SortedDescendingHeaderStyle BackColor="#575357" />
                                    </asp:GridView>
                                </div>
                            </div>
                            <div class="layout-detail-panel">
                                <div class="layout-panel-top scrollable-table-container">
                                    <asp:GridView ID="gvCityDays" runat="server" AutoGenerateColumns="False"
                                        CssClass="results-table" DataSourceID="odsCityDays" Visible="false" ShowFooter="true"
                                        OnRowUpdating="gvCityDays_OnRowUpdating" OnRowCommand="gvCityDays_RowCommand">
                                        <EmptyDataTemplate>
                                            <asp:DropDownList ID="ddlPreperationDoW" runat="server">
                                                <asp:ListItem Value="1">Sunday</asp:ListItem>
                                                <asp:ListItem Value="2">Monday</asp:ListItem>
                                                <asp:ListItem Selected="True" Value="3">Tuesday</asp:ListItem>
                                                <asp:ListItem Value="4">Wednesday</asp:ListItem>
                                                <asp:ListItem Value="5">Thursday</asp:ListItem>
                                                <asp:ListItem Value="6">Friday</asp:ListItem>
                                                <asp:ListItem Value="7">Saturday</asp:ListItem>
                                            </asp:DropDownList>&nbsp;&nbsp;
                                            <asp:TextBox ID="tbxDeliveryDelay" runat="server" Text="1" />&nbsp;&nbsp;
                                            <asp:TextBox ID="tbxDeliveryOrder" runat="server" Text="20" />&nbsp;&nbsp;&nbsp;
                                            <asp:Button ID="btnAddCity" runat="server" Text="Add Prep Day" OnClick="btnAddCity_Click" />
                                        </EmptyDataTemplate>
                                        <Columns>
                                            <asp:TemplateField HeaderText="Prep Day">
                                                <EditItemTemplate>
                                                    <asp:DropDownList ID="ddlPreperationDoW" runat="server" SelectedValue='<%# Bind("PrepDayOfWeekID") %>'>
                                                        <asp:ListItem Value="0" Text="--select a day--" />
                                                        <asp:ListItem Value="1">Sunday</asp:ListItem>
                                                        <asp:ListItem Value="2">Monday</asp:ListItem>
                                                        <asp:ListItem Selected="True" Value="3">Tuesday</asp:ListItem>
                                                        <asp:ListItem Value="4">Wednesday</asp:ListItem>
                                                        <asp:ListItem Value="5">Thursday</asp:ListItem>
                                                        <asp:ListItem Value="6">Friday</asp:ListItem>
                                                        <asp:ListItem Value="7">Saturday</asp:ListItem>
                                                    </asp:DropDownList>
                                                </EditItemTemplate>
                                                <ItemTemplate>
                                                    <asp:DropDownList ID="ddlPreperationDoW" runat="server" SelectedValue='<%# Bind("PrepDayOfWeekID") %>'>
                                                        <asp:ListItem Value="0" Text="--select a day--" />
                                                        <asp:ListItem Value="1">Sunday</asp:ListItem>
                                                        <asp:ListItem Value="2">Monday</asp:ListItem>
                                                        <asp:ListItem Selected="True" Value="3">Tuesday</asp:ListItem>
                                                        <asp:ListItem Value="4">Wednesday</asp:ListItem>
                                                        <asp:ListItem Value="5">Thursday</asp:ListItem>
                                                        <asp:ListItem Value="6">Friday</asp:ListItem>
                                                        <asp:ListItem Value="7">Saturday</asp:ListItem>
                                                    </asp:DropDownList>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:DropDownList ID="ddlPreperationDoW" runat="server" SelectedValue='<%# Bind("PrepDayOfWeekID") %>'>
                                                        <asp:ListItem Value="0" Text="--select a day--" />
                                                        <asp:ListItem Value="1">Sunday</asp:ListItem>
                                                        <asp:ListItem Value="2">Monday</asp:ListItem>
                                                        <asp:ListItem Selected="True" Value="3">Tuesday</asp:ListItem>
                                                        <asp:ListItem Value="4">Wednesday</asp:ListItem>
                                                        <asp:ListItem Value="5">Thursday</asp:ListItem>
                                                        <asp:ListItem Value="6">Friday</asp:ListItem>
                                                        <asp:ListItem Value="7">Saturday</asp:ListItem>
                                                    </asp:DropDownList>
                                                </FooterTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Dlvry Delay" SortExpression="DeliveryDelayDays">
                                                <EditItemTemplate>
                                                    <asp:TextBox ID="tbxDeliveryDelay" runat="server" Width="2em" Text='<%# Bind("DeliveryDelayDays") %>'></asp:TextBox>
                                                    <asp:HiddenField ID="CityPrepDaysIDHidden" runat="server" Value='<%# Bind("CityPrepDaysID") %>' />
                                                </EditItemTemplate>
                                                <ItemTemplate>
                                                    &nbsp;+&nbsp;<asp:Label ID="lblDeliveryDay" runat="server" Width="2em" Text='<%# Bind("DeliveryDelayDays") %>' />
                                                    =&nbsp;<asp:Label ID="CityNameLabel" runat="server" Text='<%# GetDeliveryDay(Eval("PrepDayOfWeekID").ToString(),Eval("DeliveryDelayDays").ToString()) %>' />
                                                    <asp:HiddenField ID="CityPrepDaysIDHidden" runat="server" Value='<%# Bind("CityPrepDaysID") %>' />
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:TextBox ID="tbxDeliveryDelay" runat="server" Width="2em" Text='1'></asp:TextBox>
                                                    <asp:HiddenField ID="CityPrepDaysIDHidden" runat="server" Value='<%# Bind("CityPrepDaysID") %>' />
                                                </FooterTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Dlvry Order" SortExpression="DeliveryOrder">
                                                <EditItemTemplate>
                                                    <asp:TextBox ID="tbxDeliveryOrder" runat="server" Width="2em" Text='<%# Bind("DeliveryOrder") %>'></asp:TextBox>
                                                </EditItemTemplate>
                                                <ItemTemplate>
                                                    <asp:Label ID="lblDeliveryOrder" runat="server" Text='<%# Bind("DeliveryOrder") %>'></asp:Label>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:TextBox ID="tbxDeliveryOrder" runat="server" Width="2em" Text='30'></asp:TextBox>
                                                </FooterTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField ShowHeader="False">
                                                <EditItemTemplate>
                                                    <asp:ImageButton ID="btnCityDaysUpdate" runat="server" CausesValidation="False" CommandName="Update"
                                                        AlternateText="go" ImageUrl="~/images/imgButtons/UpdateItem.gif" />
                                                    &nbsp;
                                                    <asp:ImageButton ID="btnCityDaysCancel" runat="server" CausesValidation="False" CommandName="Cancel"
                                                        AlternateText="no" ImageUrl="~/images/imgButtons/CancelItem.gif" />
                                                </EditItemTemplate>
                                                <ItemTemplate>
                                                    <asp:ImageButton ID="btnCityDaysEdit" runat="server" CausesValidation="False" CommandName="Edit"
                                                        Text="Edit" ImageUrl="~/images/imgButtons/EditItem.gif" />&nbsp;
                                                    <asp:ImageButton ID="btnCityDaysDelete" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete"
                                                        ImageUrl="~/images/imgButtons/Trashcan.gif" />
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:ImageButton ID="btnAdd" runat="server" CausesValidation="False" CommandName="AddItem"
                                                        ImageUrl="~/images/imgButtons/AddItem.gif" AlternateText="Add" />
                                                </FooterTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                        <FooterStyle BackColor="#90e010" BorderStyle="Dashed" BorderColor="Cornsilk" />
                                    </asp:GridView>
                                </div>
                            </div>
                        </div>
                        <asp:ObjectDataSource ID="odsCityDays" runat="server"
                            DataObjectTypeName="TrackerSQL.Controls.CityPrepDaysTbl"
                            InsertMethod="InsertCityPrepDay" SelectMethod="GetAllByCityId"
                            TypeName="TrackerSQL.Controls.CityPrepDaysTbl"
                            UpdateMethod="UpdateCityPrepDay"
                            OldValuesParameterFormatString="original_{0}">
                            <SelectParameters>
                                <asp:ControlParameter ControlID="gvCities" Name="pCityID"
                                    PropertyName="SelectedValue" Type="Int32" />
                            </SelectParameters>
                        </asp:ObjectDataSource>

                    </ContentTemplate>
                </asp:UpdatePanel>
            </ContentTemplate>
        </ajaxToolkit:TabPanel>
        <ajaxToolkit:TabPanel ID="tabpnlPackaging" runat="server" HeaderText="Packaging">
            <ContentTemplate>
                <asp:UpdatePanel ID="upnlPackaging" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div class="responsive-layout-container scrollable-table-container">
                            <asp:GridView ID="gvPackaging" runat="server" AutoGenerateColumns="False" CssClass="TblWhite"
                                OnRowCommand="gvPackaging_RowCommand" OnRowDataBound="gvPackaging_RowDataBound"
                                ShowFooter="true" DataSourceID="odsPackaging" AllowPaging="True" PageSize="20"
                                AllowSorting="True">
                                <FooterStyle BorderStyle="Dashed" BorderColor="Cornsilk" />
                                <Columns>
                                    <asp:TemplateField ShowHeader="False">
                                        <EditItemTemplate>
                                            <asp:ImageButton ID="btnUpdate" runat="server" CausesValidation="False" CommandName="Update"
                                                AlternateText="go" ImageUrl="~/images/imgButtons/UpdateItem.gif" />
                                            &nbsp;
                                        <asp:ImageButton ID="btnCancel" runat="server" CausesValidation="False" CommandName="Cancel"
                                            AlternateText="no" ImageUrl="~/images/imgButtons/CancelItem.gif" />
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:ImageButton ID="btnAdd" runat="server" CausesValidation="False" CommandName="AddItem"
                                                ImageUrl="~/images/imgButtons/AddItem.gif" AlternateText="Add" />
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:ImageButton ID="btnEdit" runat="server" CausesValidation="False" CommandName="Edit"
                                                Text="Edit" ImageUrl="~/images/imgButtons/EditItem.gif" />&nbsp; 
                                        <asp:ImageButton ID="btnDelete" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete"
                                            ImageUrl="~/images/imgButtons/Trashcan.gif" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Description" SortExpression="Description">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBoxDescription" runat="server" Text='<%# Bind("Description") %>' />
                                            <asp:HiddenField ID="hdnPackagingID" runat="server" Value='<%# Bind("PackagingID") %>' />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="LabelDescription" runat="server" Text='<%# Bind("Description") %>' />
                                            <asp:HiddenField ID="hdnPackagingID" runat="server" Value='<%# Bind("PackagingID") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="TextBoxDescription" runat="server" Text="" />
                                            <asp:HiddenField ID="hdnPackagingID" runat="server" Value='<%# Bind("PackagingID") %>' />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="AdditionalNotes" SortExpression="AdditionalNotes">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBoxAdditionalNotes" runat="server" Text='<%# Bind("AdditionalNotes") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="LabelAdditionalNotes" runat="server" Text='<%# Bind("AdditionalNotes") %>'></asp:Label>
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="TextBoxAdditionalNotes" runat="server" Text="" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="BGColour" SortExpression="BGColour">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBoxBGColour" runat="server" Text='<%# Bind("BGColour") %>' />
                                            <asp:ImageButton ID="ImangeButtonBGColour" runat="server" ImageUrl="~/images/imgButtons/Picture.gif" />
                                            <ajaxToolkit:ColorPickerExtender ID="ColorPickerExtBGColour" runat="server" TargetControlID="TextBoxBGColour"
                                                PopupButtonID="ImangeButtonBGColour" PopupPosition="TopRight" OnClientColorSelectionChanged="ColorPickerExtBGColour_OnClientColorSelectionChanged" />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="LabelBGColour" runat="server" Text='<%# Bind("BGColour") %>'></asp:Label>
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="TextBoxBGColour" runat="server" Text='' />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Colour" SortExpression="Colour">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBoxColour" runat="server" Text='<%# Bind("Colour") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="LabelColour" runat="server" Text='<%# Bind("Colour") %>'></asp:Label>
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="TextBoxColour" runat="server" Text="" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Symbol" SortExpression="Symbol">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBoxSymbol" runat="server" Text='<%# Bind("Symbol") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="LabelSymbol" runat="server" Text='<%# Bind("Symbol") %>'></asp:Label>
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="TextBoxSymbol" runat="server" Text="" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </ContentTemplate>
        </ajaxToolkit:TabPanel>
        <ajaxToolkit:TabPanel ID="tabInvoiceTypes" runat="server" HeaderText="InvoiceTypes">
            <ContentTemplate>
                <asp:UpdateProgress runat="server" ID="gvInvoiceTypesUpdateProgress" AssociatedUpdatePanelID="gvInvoiceTypesUpdatePanel">
                    <ProgressTemplate>
                        Please Wait&nbsp;<img src="../images/animi/QuaffeeProgress.gif" alt="Please Wait..." />&nbsp;...
                    </ProgressTemplate>
                </asp:UpdateProgress>
                <asp:UpdatePanel ID="gvInvoiceTypesUpdatePanel" runat="server">
                    <ContentTemplate>
                        <div class="responsive-layout-container scrollable-table-container">
                            <asp:GridView ID="gvInvoiceTypes" runat="server" AllowSorting="True" DataSourceID="odsInvoiceTypes" DataKeyNames="InvoiceTypeID"
                                CssClass="results-table" AutoGenerateColumns="False" ShowFooter="true"
                                OnRowCommand="gvInvoiceTypes_RowCommand">
                                <Columns>
                                    <asp:TemplateField ShowHeader="False">
                                        <EditItemTemplate>
                                            <asp:ImageButton ID="btnInvUpdate" runat="server" CausesValidation="False" CommandName="Update"
                                                AlternateText="go" ImageUrl="~/images/imgButtons/UpdateItem.gif" />&nbsp;
                                        <asp:ImageButton ID="btnInvCancel" runat="server" CausesValidation="False" CommandName="Cancel"
                                            AlternateText="no" ImageUrl="~/images/imgButtons/CancelItem.gif" />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:ImageButton ID="btnInvEdit" runat="server" CausesValidation="False" CommandName="Edit"
                                                Text="Edit" ImageUrl="~/images/imgButtons/EditItem.gif" />&nbsp;
                                        <asp:ImageButton ID="btnInvDelete" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete"
                                            ImageUrl="~/images/imgButtons/Trashcan.gif" />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:ImageButton ID="btnInvAdd" runat="server" CausesValidation="False" CommandName="AddItem"
                                                ImageUrl="~/images/imgButtons/AddItem.gif" AlternateText="Add" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ConvertEmptyStringToNull="False" HeaderText="Invoice Type" SortExpression="InvoiceTypeDesc">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="InvoiceTypeDescTextBox" runat="server" Text='<%# Bind("InvoiceTypeDesc") %>' Width="15em" />
                                            <asp:HiddenField ID="InvoiceTypeIDHidden" runat="server" Value='<%# Bind("InvoiceTypeID") %>' />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="InvoiceTypeDescLabel" runat="server" Text='<%# Bind("InvoiceTypeDesc") %>' />
                                            <asp:HiddenField ID="InvoiceTypeIDHidden" runat="server" Value='<%# Bind("InvoiceTypeID") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="InvoiceTypeDescTextBox" runat="server" Width="15em" />
                                            <asp:HiddenField ID="InvoiceTypeIDHidden" runat="server" Value="0" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ConvertEmptyStringToNull="False" HeaderText="Enabled" SortExpression="Enabled">
                                        <EditItemTemplate>
                                            <asp:CheckBox ID="EnabledCheckBox" runat="server" Text="Yes" Checked='<%# Bind("Enabled") %>' />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:CheckBox ID="EnabledCheckBox" runat="server" Text="Yes" Checked='<%# Bind("Enabled") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:CheckBox ID="EnabledCheckBox" runat="server" Text="Yes" Checked="true" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ConvertEmptyStringToNull="False" HeaderText="Notes" SortExpression="Notes">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="NotesTextBox" runat="server" Text='<%# Bind("Notes") %>' Width="30em" />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="NotesLabel" runat="server" Text='<%# Bind("Notes") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="NotesTextBox" runat="server" Width="30em" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <EmptyDataTemplate>
                                    Invoice Type:&nbsp;<asp:TextBox ID="InvoiceTypeDescTextBox" runat="server" Width="15em" />&nbsp;&nbsp;
                                Enabled: &nbsp;<asp:CheckBox ID="EnabledCheckBox" runat="server" Text="Yes" Checked="true" />&nbsp;&nbsp;
                                Notes:&nbsp;<asp:TextBox ID="NotesTextBox" runat="server" Text="" Width="30em" />&nbsp;&nbsp;&nbsp;
                                <asp:Button ID="InsertButton" runat="server" CausesValidation="False" CommandName="Insert" Text="Insert" />
                                </EmptyDataTemplate>
                            </asp:GridView>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </ContentTemplate>
        </ajaxToolkit:TabPanel>
        <ajaxToolkit:TabPanel ID="tabPaymentTerms" runat="server" HeaderText="PaymentTerms">
            <ContentTemplate>
                <asp:UpdateProgress runat="server" ID="PaymentTermsUpdateProgress" AssociatedUpdatePanelID="gvPaymentTermsUpdatePanel">
                    <ProgressTemplate>please wait...</ProgressTemplate>
                </asp:UpdateProgress>
                <asp:UpdatePanel ID="gvPaymentTermsUpdatePanel" runat="server" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="responsive-layout-container scrollable-table-container">
                            <asp:GridView ID="gvPaymentTerms" runat="server" AllowSorting="True" DataSourceID="odsPaymentTerms" DataKeyNames="PaymentTermID"
                                CssClass="results-table" AutoGenerateColumns="False" ShowFooter="true"
                                OnRowCommand="gvPaymentTerms_RowCommand">
                                <Columns>
                                    <asp:TemplateField ShowHeader="False">
                                        <EditItemTemplate>
                                            <asp:ImageButton ID="btnPayUpdate" runat="server" CausesValidation="False" CommandName="Update"
                                                AlternateText="go" ImageUrl="~/images/imgButtons/UpdateItem.gif" />
                                            &nbsp;
                                            <asp:ImageButton ID="btnPayCancel" runat="server" CausesValidation="False" CommandName="Cancel"
                                                AlternateText="no" ImageUrl="~/images/imgButtons/CancelItem.gif" />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:ImageButton ID="btnPayEdit" runat="server" CausesValidation="False" CommandName="Edit"
                                                Text="Edit" ImageUrl="~/images/imgButtons/EditItem.gif" />&nbsp;
                                            <asp:ImageButton ID="btnPayDelete" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete"
                                                ImageUrl="~/images/imgButtons/Trashcan.gif" />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:ImageButton ID="btnPayAdd" runat="server" CausesValidation="False" CommandName="AddItem"
                                                ImageUrl="~/images/imgButtons/AddItem.gif" AlternateText="Add" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ConvertEmptyStringToNull="False" HeaderText="Price Level" SortExpression="PaymentTermDesc">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="PaymentTermDescTextBox" runat="server" Text='<%# Bind("PaymentTermDesc") %>' Width="15em" />
                                            <asp:HiddenField ID="PaymentTermIDHidden" runat="server" Value='<%# Bind("PaymentTermID") %>' />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="PaymentTermDescLabel" runat="server" Text='<%# Bind("PaymentTermDesc") %>' />
                                            <asp:HiddenField ID="PaymentTermIDHidden" runat="server" Value='<%# Bind("PaymentTermID") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="PaymentTermDescTextBox" runat="server" Width="15em" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ConvertEmptyStringToNull="False" HeaderText="Payment Days" SortExpression="PaymentDays">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="PaymentDaysTextBox" runat="server" Text='<%# Bind("PaymentDays") %>' Width="5em" />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="PaymentDaysLabel" runat="server" Text='<%# Bind("PaymentDays") %>' CssClass="rowR" />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="PaymentDaysTextBox" runat="server" Width="5em" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ConvertEmptyStringToNull="False" HeaderText="Day Of Month" SortExpression="DayOfMonth">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="DayOfMonthTextBox" runat="server" Text='<%# Bind("DayOfMonth") %>' Width="5em" />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="DayOfMonthLabel" runat="server" Text='<%# Bind("DayOfMonth") %>' CssClass="rowR" />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="DayOfMonthTextBox" runat="server" Width="5em" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ConvertEmptyStringToNull="False" HeaderText="UseDays" SortExpression="UseDays">
                                        <EditItemTemplate>
                                            <asp:CheckBox ID="UseDaysCheckBox" runat="server" Text="Yes" Checked='<%# Bind("UseDays") %>' />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:CheckBox ID="UseDaysCheckBox" runat="server" Text="Yes" Checked='<%# Bind("UseDays") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:CheckBox ID="UseDaysCheckBox" runat="server" Text="Yes" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ConvertEmptyStringToNull="False" HeaderText="Enabled" SortExpression="Enabled">
                                        <EditItemTemplate>
                                            <asp:CheckBox ID="EnabledCheckBox" runat="server" Text="Yes" Checked='<%# Bind("Enabled") %>' />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:CheckBox ID="EnabledCheckBox" runat="server" Text="Yes" Checked='<%# Bind("Enabled") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:CheckBox ID="EnabledCheckBox" runat="server" Text="Yes" Checked="true" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ConvertEmptyStringToNull="False" HeaderText="Notes" SortExpression="Notes">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="NotesTextBox" runat="server" Text='<%# Bind("Notes") %>' Width="30em" />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="NotesLabel" runat="server" Text='<%# Bind("Notes") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="NotesTextBox" runat="server" Text='<%# Bind("Notes") %>' Width="30em" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <EmptyDataTemplate>
                                    &nbsp;&nbsp;Price Level:&nbsp;<asp:TextBox ID="PaymentTermDescTextBox" runat="server" Width="15em" />
                                    &nbsp;&nbsp;Payment Days:&nbsp;<asp:TextBox ID="PaymentDaysTextBox" runat="server" Width="5em" />
                                    &nbsp;&nbsp;Day of Month:&nbsp;<asp:TextBox ID="DayOfMonthTextBox" runat="server" Width="5em" />
                                    &nbsp;&nbsp;UseDays:&nbsp;<asp:CheckBox ID="UseDaysCheckBox" runat="server" Text="Yes" />
                                    &nbsp;&nbsp;Enabled:&nbsp;<asp:CheckBox ID="EnabledCheckBox" runat="server" Text="Yes" Checked="true" />
                                    &nbsp;&nbsp;Notes:&nbsp;<asp:TextBox ID="NotesTextBox" runat="server" Text="" Width="30em" />
                                    &nbsp;&nbsp;&nbsp;&nbsp;<asp:Button ID="InsertButton" runat="server" CausesValidation="False" CommandName="Insert" Text="Insert" />
                                </EmptyDataTemplate>
                            </asp:GridView>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </ContentTemplate>
        </ajaxToolkit:TabPanel>
        <ajaxToolkit:TabPanel ID="tabPriceLevels" runat="server" HeaderText="PriceLevels">
            <ContentTemplate>
                <asp:UpdateProgress runat="server" ID="PriceLevelUpdateProgress" AssociatedUpdatePanelID="gvPriceLevelsUpdatePanel">
                    <ProgressTemplate>please wait...</ProgressTemplate>
                </asp:UpdateProgress>
                <asp:UpdatePanel ID="gvPriceLevelsUpdatePanel" runat="server" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="responsive-layout-container scrollable-table-container">
                            <asp:GridView ID="gvPriceLevels" runat="server" AllowSorting="True" DataSourceID="odsPriceLevels" DataKeyNames="PriceLevelID"
                                CssClass="results-table" AutoGenerateColumns="False" ShowFooter="true"
                                OnRowCommand="gvPriceLevels_RowCommand">
                                <Columns>
                                    <asp:TemplateField ShowHeader="False">
                                        <EditItemTemplate>
                                            <asp:ImageButton ID="btnPLUpdate" runat="server" CausesValidation="False" CommandName="Update"
                                                AlternateText="go" ImageUrl="~/images/imgButtons/UpdateItem.gif" />
                                            &nbsp;
                                        <asp:ImageButton ID="btnPLCancel" runat="server" CausesValidation="False" CommandName="Cancel"
                                            AlternateText="no" ImageUrl="~/images/imgButtons/CancelItem.gif" />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:ImageButton ID="btnPLEdit" runat="server" CausesValidation="False" CommandName="Edit"
                                                Text="Edit" ImageUrl="~/images/imgButtons/EditItem.gif" />&nbsp;
                                        <asp:ImageButton ID="btnPLDelete" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete"
                                            ImageUrl="~/images/imgButtons/Trashcan.gif" />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:ImageButton ID="btnAdd" runat="server" CausesValidation="False" CommandName="AddItem"
                                                ImageUrl="~/images/imgButtons/AddItem.gif" AlternateText="Add" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ConvertEmptyStringToNull="False" HeaderText="Price Level" SortExpression="PriceLevelDesc">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="PriceLevelDescTextBox" runat="server" Text='<%# Bind("PriceLevelDesc") %>' Width="15em" />
                                            <asp:HiddenField ID="hdnPriceLevelID" runat="server" Value='<%# Bind("PriceLevelID") %>' />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="PriceLevelDescLabel" runat="server" Text='<%# Bind("PriceLevelDesc") %>' />
                                            <asp:HiddenField ID="hdnPriceLevelID" runat="server" Value='<%# Bind("PriceLevelID") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="PriceLevelDescTextBox" runat="server" Width="15em" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ConvertEmptyStringToNull="False" HeaderText="Pricing Factor" SortExpression="PricingFactor">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="PricingFactorTextBox" runat="server" Text='<%# Bind("PricingFactor", "{0:#.###}") %>' Width="5em" />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="PricingFactorLabel" runat="server" Text='<%# Bind("PricingFactor", "{0:#.###}") %>' CssClass="rowR" />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="PricingFactorTextBox" runat="server" Width="5em" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ConvertEmptyStringToNull="False" HeaderText="Enabled" SortExpression="Enabled">
                                        <EditItemTemplate>
                                            <asp:CheckBox ID="EnabledCheckBox" runat="server" Text="Yes" Checked='<%# Bind("Enabled") %>' />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:CheckBox ID="EnabledCheckBox" runat="server" Text="Yes" Checked='<%# Bind("Enabled") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:CheckBox ID="EnabledCheckBox" runat="server" Text="Yes" Checked="true" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ConvertEmptyStringToNull="False" HeaderText="Notes" SortExpression="Notes">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="NotesTextBox" runat="server" Text='<%# Bind("Notes") %>' Width="30em" />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="NotesLabel" runat="server" Text='<%# Bind("Notes") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="NotesTextBox" runat="server" Width="30em" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <EmptyDataTemplate>
                                    &nbsp;&nbsp;Price Level:&nbsp;<asp:TextBox ID="PriceLevelDescTextBox" runat="server" Width="15em" />
                                    &nbsp;&nbsp;Factor: &nbsp;<asp:TextBox ID="PricingFactorTextBox" runat="server" Width="5em" />
                                    &nbsp;&nbsp;Enabled:&nbsp;<asp:CheckBox ID="EnabledCheckBox" runat="server" Text="Yes" Checked="true" />
                                    &nbsp;&nbsp;Notes:&nbsp;<asp:TextBox ID="NotesTextBox" runat="server" Text="" Width="30em" />&nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:ImageButton ID="btnAdd" runat="server" CausesValidation="False" CommandName="Insert"
                                    ImageUrl="~/images/imgButtons/AddItem.gif" AlternateText="Add" />
                                </EmptyDataTemplate>
                            </asp:GridView>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </ContentTemplate>
        </ajaxToolkit:TabPanel>
        <ajaxToolkit:TabPanel ID="tabpnlRepairStatuses" runat="server" HeaderText="Repair Statuses">
            <ContentTemplate>
                <asp:UpdatePanel ID="upnlRepairStatuses" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div class="results-container">
                            <asp:GridView ID="gvRepairStatuses" runat="server" DataKeyNames="RepairStatusID"
                                DataSourceID="odsRepairStatuses" AutoGenerateColumns="False" AllowPaging="True" AllowSorting="True"
                                ShowFooter="True" PageSize="20">
                                <Columns>
                                    <asp:BoundField DataField="RepairStatusID" HeaderText="ID" ReadOnly="True" />
                                    <asp:TemplateField HeaderText="Status">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="tbxStatusDesc" runat="server" Text='<%# Bind("RepairStatusDesc") %>' />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblStatusDesc" runat="server" Text='<%# Bind("RepairStatusDesc") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="tbxStatusDescFooter" runat="server" Width="12em" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:CheckBoxField DataField="EmailClient" HeaderText="Email Client" />
                                    <asp:BoundField DataField="SortOrder" HeaderText="Sort Order" />
                                    <asp:TemplateField HeaderText="Status Note">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="tbxStatusNote" runat="server" Text='<%# Bind("StatusNote") %>' Width="30em" />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblStatusNote" runat="server" Text='<%# Bind("StatusNote") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="tbxStatusNoteFooter" runat="server" Width="30em" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ShowHeader="False">
                                        <EditItemTemplate>
                                            <asp:ImageButton ID="btnUpdate" runat="server" CommandName="Update"
                                                ImageUrl="~/images/imgButtons/UpdateItem.gif" AlternateText="Update" />
                                            <asp:ImageButton ID="btnCancel" runat="server" CommandName="Cancel"
                                                ImageUrl="~/images/imgButtons/CancelItem.gif" AlternateText="Cancel" />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:ImageButton ID="btnEdit" runat="server" CommandName="Edit"
                                                ImageUrl="~/images/imgButtons/EditItem.gif" AlternateText="Edit" />
                                            <asp:ImageButton ID="btnDelete" runat="server" CommandName="Delete"
                                                ImageUrl="~/images/imgButtons/Trashcan.gif" AlternateText="Delete" />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:ImageButton ID="btnAdd" runat="server" CommandName="Insert"
                                                ImageUrl="~/images/imgButtons/AddItem.gif" AlternateText="Add" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>

                            <asp:ObjectDataSource ID="odsRepairStatuses" runat="server"
                                TypeName="TrackerSQL.Controls.RepairStatusesTbl"
                                DataObjectTypeName="TrackerSQL.Controls.RepairStatusesTbl"
                                SelectMethod="GetAll" InsertMethod="Insert" UpdateMethod="Update" DeleteMethod="Delete"
                                SortParameterName="SortBy"
                                OnInserted="odsRepairStatuses_Inserted"
                                OnUpdated="odsRepairStatuses_Updated"
                                OnDeleted="odsRepairStatuses_Deleted">
                                <SelectParameters>
                                    <asp:Parameter Name="SortBy" Type="String" DefaultValue="SortOrder" />
                                </SelectParameters>
                                <DeleteParameters>
                                    <asp:Parameter Name="repairStatusID" Type="Int32" />
                                </DeleteParameters>
                            </asp:ObjectDataSource>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </ContentTemplate>
        </ajaxToolkit:TabPanel>
    </ajaxToolkit:TabContainer>
    <asp:SqlDataSource ID="sdsItems" runat="server" ConflictDetection="OverwriteChanges"
        ConnectionString="<%$ ConnectionStrings:Tracker08ConnectionString %>"
        OldValuesParameterFormatString="original_{0}" ProviderName="<%$ ConnectionStrings:Tracker08ConnectionString.ProviderName %>"
        SelectCommand="SELECT ItemTypeID, ItemDesc, SKU, ItemEnabled, ItemsCharacteritics, ItemDetail, ServiceTypeId, iif(IsNull(ReplacementID), 0, ReplacementID) AS Replacement, ItemShortName, SortOrder, UnitsPerQty, iif(IsNull(ItemUnitID), 0, ItemUnitID) AS UoMID FROM ItemTypeTbl WHERE (ItemDesc LIKE ?) ORDER BY SortOrder, ItemDesc"
        UpdateCommand="UPDATE ItemTypeTbl SET ItemDesc = ?, SKU = ?, ItemEnabled = ?, ItemsCharacteritics = ?, ItemDetail = ?, ServiceTypeId = ?, ReplacementID = ?, ItemShortName = ?, SortOrder = ?, UnitsPerQty = ?, ItemUnitID = ? WHERE (ItemTypeID = ?)"
        DeleteCommand="DELETE FROM [ItemTypeTbl] WHERE [ItemTypeID] = ? "
        InsertCommand="INSERT INTO ItemTypeTbl(ItemDesc, SKU, ItemEnabled, ItemsCharacteritics, ItemDetail, ServiceTypeId, ReplacementID, ItemShortName, SortOrder, UnitsPerQty, ItemUnitID) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)">
        <SelectParameters>
            <asp:SessionParameter DefaultValue="%" Name="?" SessionField="SearchItemContains"
                DbType="String" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="ItemDesc" Type="String" />
            <asp:Parameter Name="SKU" Type="String" />
            <asp:Parameter Name="ItemEnabled" Type="Boolean" />
            <asp:Parameter Name="ItemsCharacteritics" Type="String" />
            <asp:Parameter Name="ItemDetail" Type="String" />
            <asp:Parameter Name="ServiceTypeId" Type="Int32" />
            <asp:Parameter Name="Replacement" Type="Int32" />
            <asp:Parameter Name="ItemShortName" Type="String" />
            <asp:Parameter Name="SortOrder" Type="Int32" />
            <asp:Parameter Name="UnitsPerQty" Type="Single" />
            <asp:Parameter Name="UoMID" Type="Int32" />
            <asp:Parameter Name="original_ItemTypeID" Type="Int32" />
        </UpdateParameters>
        <DeleteParameters>
            <asp:Parameter Name="original_ItemTypeID" Type="Int32" />
        </DeleteParameters>
        <InsertParameters>
            <asp:Parameter Name="ItemDesc" Type="String" />
            <asp:Parameter Name="SKU" Type="String" />
            <asp:Parameter Name="ItemEnabled" Type="Boolean" />
            <asp:Parameter Name="ItemsCharacteritics" Type="String" />
            <asp:Parameter Name="ItemDetail" Type="String" />
            <asp:Parameter Name="ServiceTypeId" Type="Int32" />
            <asp:Parameter Name="ReplacementID" Type="Int32" />
            <asp:Parameter Name="ItemShortName" Type="String" />
            <asp:Parameter Name="SortOrder" Type="Int32" />
            <asp:Parameter Name="UnitsPerQty" Type="Single" />
            <asp:Parameter Name="UoMID" Type="Int32" />
        </InsertParameters>
    </asp:SqlDataSource>
    <asp:ObjectDataSource ID="odsAllItems" runat="server" SelectMethod="GetAll" TypeName="TrackerSQL.Controls.ItemTypeTbl"
        OldValuesParameterFormatString="original_{0}">
        <SelectParameters>
            <asp:Parameter DefaultValue="ItemDesc" Name="SortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsPeople" runat="server" TypeName="TrackerSQL.Controls.PersonsTbl"
        DataObjectTypeName="TrackerSQL.Controls.PersonsTbl" SelectMethod="GetAll" SortParameterName="SortBy"
        UpdateMethod="UpdatePerson" OldValuesParameterFormatString="original_{0}" DeleteMethod="DeletePerson"
        InsertMethod="InsertPerson">
        <SelectParameters>
            <asp:Parameter DefaultValue="&quot;Abbreviation&quot;" Name="SortBy" Type="String" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="pPerson" Type="Object" DbType="Object" />
            <asp:Parameter Name="pOrignal_PersonID" Type="Int32" />
        </UpdateParameters>
        <DeleteParameters>
            <asp:Parameter Name="pPersonID" Type="Int32" />
        </DeleteParameters>
    </asp:ObjectDataSource>
    <asp:SqlDataSource ID="sdsUserNames" runat="server"
        ConnectionString="<%$ ConnectionStrings:ApplicationServices %>"
        SelectCommand="SELECT [UserName] AS SecurityUsername FROM [vw_aspnet_Users]" />
    <asp:ObjectDataSource ID="odsEquipTypes" runat="server" TypeName="TrackerSQL.Controls.EquipTypeTbl"
        DataObjectTypeName="TrackerSQL.Controls.EquipTypeTbl" SelectMethod="GetAll" SortParameterName="SortBy"
        UpdateMethod="UpdateEquipItem" OldValuesParameterFormatString="original_{0}" InsertMethod="InsertEquipObj"
        OnInserting="odsEquipTypes_OnInserting">
        <SelectParameters>
            <asp:Parameter DefaultValue="EquipTypeName" Name="SortBy" Type="String" />
        </SelectParameters>
        <InsertParameters>
            <asp:Parameter DbType="Object" Name="objEquipType" Type="Object" />
        </InsertParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsInvoiceTypes" runat="server" TypeName="TrackerSQL.Controls.InvoiceTypeTbl"
        DataObjectTypeName="TrackerSQL.Controls.InvoiceTypeTbl" SelectMethod="GetAll" SortParameterName="SortBy"
        UpdateMethod="Update" OldValuesParameterFormatString="original_{0}" InsertMethod="Insert" DeleteMethod="Delete">
        <DeleteParameters>
            <asp:Parameter Name="pInvoiceTypeID" Type="Int32" />
        </DeleteParameters>
        <SelectParameters>
            <asp:Parameter DefaultValue="InvoiceTypeDesc" Name="SortBy" Type="String" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="pInvoiceTypeTbl" Type="Object" DbType="Object" />
            <asp:Parameter Name="pOrignal_InvoiceTypeID" Type="Int32" />
        </UpdateParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsPaymentTerms" runat="server" TypeName="TrackerSQL.Controls.PaymentTermsTbl"
        DataObjectTypeName="TrackerSQL.Controls.PaymentTermsTbl" SelectMethod="GetAll" SortParameterName="SortBy"
        UpdateMethod="Update" OldValuesParameterFormatString="original_{0}" InsertMethod="Insert" DeleteMethod="Delete">
        <DeleteParameters>
            <asp:Parameter Name="pPaymentTermID" Type="Int32" />
        </DeleteParameters>
        <SelectParameters>
            <asp:Parameter DefaultValue="PriceDesc" Name="SortBy" Type="String" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="pPaymentTermsTbl" Type="Object" DbType="Object" />
            <asp:Parameter Name="pOrignal_PaymentTermID" Type="Int32" />
        </UpdateParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsPriceLevels" runat="server" TypeName="TrackerSQL.Controls.PriceLevelsTbl"
        DataObjectTypeName="TrackerSQL.Controls.PriceLevelsTbl" SelectMethod="GetAll" SortParameterName="SortBy"
        UpdateMethod="Update" OldValuesParameterFormatString="original_{0}" InsertMethod="Insert" DeleteMethod="Delete">
        <DeleteParameters>
            <asp:Parameter Name="pPriceLevelID" Type="Int32" />
        </DeleteParameters>
        <SelectParameters>
            <asp:Parameter DefaultValue="PriceDesc" Name="SortBy" Type="String" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="pPriceLevelTbl" Type="Object" DbType="Object" />
            <asp:Parameter Name="pOrignal_PriceLevelID" Type="Int32" />
        </UpdateParameters>
    </asp:ObjectDataSource>
    <asp:SqlDataSource ID="sdsServiceTypes" runat="server" ConnectionString="<%$ ConnectionStrings:Tracker08ConnectionString %>"
        ProviderName="<%$ ConnectionStrings:Tracker08ConnectionString.ProviderName %>"
        SelectCommand="SELECT [ServiceTypeId], [ServiceType] FROM [ServiceTypesTbl]"></asp:SqlDataSource>
    <asp:SqlDataSource ID="sdsReplacementItems" runat="server" ConnectionString="<%$ ConnectionStrings:Tracker08ConnectionString %>"
        ProviderName="<%$ ConnectionStrings:Tracker08ConnectionString.ProviderName %>"
        SelectCommand="SELECT [ItemTypeID], [ItemDesc] FROM [ItemTypeTbl] ORDER BY [ItemDesc]"></asp:SqlDataSource>
    <asp:SqlDataSource ID="sdsCities" runat="server" OnSelecting="sdsCities_Selecting"
        ConflictDetection="CompareAllValues" ConnectionString="<%$ ConnectionStrings:Tracker08ConnectionString %>"
        DeleteCommand="DELETE FROM [CityTbl] WHERE [ID] = ?" InsertCommand="INSERT INTO CityTbl(City) VALUES (?)"
        OldValuesParameterFormatString="original_{0}" ProviderName="<%$ ConnectionStrings:Tracker08ConnectionString.ProviderName %>"
        SelectCommand="SELECT [ID], [City] FROM [CityTbl] ORDER BY [City]"
        UpdateCommand="UPDATE [CityTbl] SET [City] = ? WHERE [ID] = ?">
        <DeleteParameters>
            <asp:Parameter Name="original_ID" Type="Int32" />
            <asp:Parameter Name="original_City" Type="String" />
        </DeleteParameters>
        <InsertParameters>
            <asp:Parameter Name="City" Type="String" />
        </InsertParameters>
        <UpdateParameters>
            <asp:Parameter Name="City" Type="String" />
            <asp:Parameter Name="RoastingDay" Type="Int32" />
        </UpdateParameters>
    </asp:SqlDataSource>
    <asp:ObjectDataSource runat="server" SelectMethod="GetAll" TypeName="TrackerSQL.Controls.PackagingTbl"
        ID="odsPackaging" SortParameterName="SortBy" DataObjectTypeName="TrackerSQL.Controls.PackagingTbl"
        InsertMethod="InsertPackaging" OldValuesParameterFormatString="original_{0}" UpdateMethod="UpdatePackaging">
        <SelectParameters>
            <asp:Parameter Name="SortBy" Type="String" DefaultValue="Description" />
        </SelectParameters>
    </asp:ObjectDataSource>
</asp:Content>
