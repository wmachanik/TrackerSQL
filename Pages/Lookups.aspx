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
    <asp:Label ID="lblStatus" runat="server" ForeColor="Red" />
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
                            <asp:GridView ID="gvItems" runat="server" AllowPaging="True"
                                PageSize="20" CssClass="results-table sticky-first-column" Font-Size="Small" AllowSorting="True" AutoGenerateColumns="False"
                                OnRowCommand="gvItems_RowCommand" ShowFooter="True" CellPadding="0"
                                PagerStyle-CssClass="aspNetPager"
                                DataKeyNames="ItemID"
                                OnPageIndexChanging="gvItems_PageIndexChanging"
                                OnSorting="gvItems_Sorting"
                                OnRowEditing="gvItems_RowEditing"
                                OnRowCancelingEdit="gvItems_RowCancelingEdit"
                                OnRowUpdating="gvItems_RowUpdating">
                                <Columns>
                                    <asp:TemplateField ShowHeader="False" HeaderStyle-CssClass="col-tight" ItemStyle-CssClass="col-tight" FooterStyle-CssClass="col-tight">
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
                                    <asp:BoundField DataField="ItemID" HeaderText="ItemID" InsertVisible="True"
                                        ReadOnly="True" SortExpression="ItemID" Visible="False" />
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
                                    <asp:TemplateField HeaderText="SKU" SortExpression="SKUDesc" HeaderStyle-CssClass="col-tight" ItemStyle-CssClass="col-tight" FooterStyle-CssClass="col-tight">
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
                                    <asp:TemplateField HeaderText="Enbld" SortExpression="ItemEnabled" HeaderStyle-CssClass="col-tight" ItemStyle-CssClass="col-tight" FooterStyle-CssClass="col-tight">
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
                                    <asp:TemplateField HeaderText="Type" SortExpression="ItemServiceTypeID">
                                        <EditItemTemplate>
                                            <asp:DropDownList ID="ddlServiceType" runat="server"
                                                AppendDataBoundItems="true" DataTextField="ServiceType" DataValueField="ServiceTypeId"
                                                SelectedValue='<%# Bind("ItemServiceTypeID") %>' Width="10em">
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:DropDownList ID="ddlServiceType" runat="server"
                                                AppendDataBoundItems="true" DataTextField="ServiceType" DataValueField="ServiceTypeId"
                                                SelectedValue='<%# Bind("ItemServiceTypeID") %>' Width="10em">
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlServiceType" runat="server"
                                                AppendDataBoundItems="true" DataTextField="ServiceType" DataValueField="ServiceTypeId"
                                                Enabled="False" Width="10em">
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Replcment" SortExpression="ReplacementItemID">
                                        <EditItemTemplate>
                                            <asp:DropDownList ID="ddlReplacement" runat="server" AppendDataBoundItems="True"
                                                DataTextField="ItemDesc" DataValueField="ItemID"
                                                SelectedValue='<%# Bind("ReplacementItemID") %>'>
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:DropDownList ID="ddlReplacement" runat="server" AppendDataBoundItems="True"
                                                DataTextField="ItemDesc" DataValueField="ItemID"
                                                SelectedValue='<%# Bind("ReplacementItemID") %>'>
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlReplacement" runat="server" AppendDataBoundItems="True"
                                                DataTextField="ItemDesc" DataValueField="ItemID">
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Abrv" SortExpression="ItemShortName" HeaderStyle-CssClass="col-tight" ItemStyle-CssClass="col-tight" FooterStyle-CssClass="col-tight">
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
                                    <asp:TemplateField HeaderText="Qty" SortExpression="UnitsPerQty" HeaderStyle-CssClass="col-tight" ItemStyle-CssClass="col-tight" FooterStyle-CssClass="col-tight">
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
                                    <asp:TemplateField HeaderText="UoM" SortExpression="ItemUnitID" HeaderStyle-CssClass="col-tight" ItemStyle-CssClass="col-tight" FooterStyle-CssClass="col-tight">
                                        <EditItemTemplate>
                                            <asp:DropDownList ID="ddlUnits" runat="server" AppendDataBoundItems="True"
                                                DataTextField="UnitOfMeasure" Width="6em"
                                                DataValueField="ItemUnitID" SelectedValue='<%# Bind("ItemUnitID") %>'>
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:DropDownList ID="ddlUnits" runat="server" AppendDataBoundItems="True"
                                                DataTextField="UnitOfMeasure" Width="6em"
                                                DataValueField="ItemUnitID" SelectedValue='<%# Bind("ItemUnitID") %>'>
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlUnits" runat="server" AppendDataBoundItems="True"
                                                DataTextField="UnitOfMeasure" Width="6em"
                                                DataValueField="ItemUnitID">
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="S/O" SortExpression="SortOrder" HeaderStyle-CssClass="col-tight" ItemStyle-CssClass="col-tight" FooterStyle-CssClass="col-tight">
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
                                </Columns>
                                <EditRowStyle BackColor="#24BF61" />
                                <EmptyDataTemplate>
                                    <asp:DetailsView ID="dvItemIns" runat="server" AutoGenerateRows="False" BackColor="White"
                                        BorderColor="#DEDFDE" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" DataKeyNames="ItemID"
                                        OnItemInserted="dvItems_ItemInserted" ForeColor="Black"
                                        PagerStyle-CssClass="aspNetPager"
                                        GridLines="Vertical" Width="220px">
                                        <AlternatingRowStyle BackColor="White" />
                                        <EditRowStyle BackColor="#CE5D5A" Font-Bold="True" ForeColor="White" />
                                        <Fields>
                                            <asp:BoundField DataField="ItemDesc" HeaderText="ItemDesc" SortExpression="ItemDesc" />
                                            <asp:BoundField DataField="ItemsCharacteritics" HeaderText="ItemsCharacteritics"
                                                SortExpression="ItemsCharacteritics" />
                                            <asp:BoundField DataField="ItemDetail" HeaderText="ItemDetail" SortExpression="ItemDetail" />
                                            <asp:TemplateField HeaderText="ServiceType" SortExpression="ItemServiceTypeID">
                                                <EditItemTemplate>
                                                    <asp:DropDownList ID="ddlEditServiceType" runat="server"
                                                        DataTextField="ServiceType" DataValueField="ServiceTypeId" SelectedValue='<%# Bind("ItemServiceTypeID") %>'>
                                                    </asp:DropDownList>
                                                </EditItemTemplate>
                                                <InsertItemTemplate>
                                                    <asp:DropDownList ID="ddlInsServiceType" runat="server"
                                                        DataTextField="ServiceType" DataValueField="ServiceTypeId" SelectedValue='<%# Bind("ItemServiceTypeID") %>'>
                                                    </asp:DropDownList>
                                                </InsertItemTemplate>
                                                <ItemTemplate>
                                                    <asp:DropDownList ID="ddlServiceType" runat="server"
                                                        DataTextField="ServiceType" DataValueField="ServiceTypeId" SelectedValue='<%# Bind("ItemServiceTypeID") %>'>
                                                    </asp:DropDownList>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Replacement">
                                                <EditItemTemplate>
                                                    <asp:DropDownList ID="ddlEditReplacement" runat="server" AppendDataBoundItems="True"
                                                        DataTextField="ItemDesc" DataValueField="ItemID"
                                                        SelectedValue='<%# Bind("ReplacementItemID") %>'>
                                                        <asp:ListItem Value="0" Text="n/a" />
                                                    </asp:DropDownList>
                                                </EditItemTemplate>
                                                <InsertItemTemplate>
                                                    <asp:DropDownList ID="ddlInsReplacement" runat="server" AppendDataBoundItems="True"
                                                        DataTextField="ItemDesc" DataValueField="ItemID"
                                                        SelectedValue='<%# Bind("ReplacementItemID") %>'>
                                                        <asp:ListItem Value="0" Text="n/a" />
                                                    </asp:DropDownList>
                                                </InsertItemTemplate>
                                                <ItemTemplate>
                                                    <asp:DropDownList ID="ddlReplacement" runat="server" AppendDataBoundItems="True"
                                                        DataTextField="ItemDesc" DataValueField="ItemID"
                                                        SelectedValue='<%# Bind("ReplacementItemID") %>'>
                                                        <asp:ListItem Value="0" Text="n/a" />
                                                    </asp:DropDownList>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:DropDownList ID="ddlInsReplacement" runat="server" AppendDataBoundItems="True"
                                                        DataTextField="ItemDesc" DataValueField="ItemID"
                                                        SelectedValue='<%# Bind("ReplacementItemID") %>'>
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
                                OnPageIndexChanging="gvPeople_PageIndexChanging"
                                OnSorting="gvPeople_Sorting"
                                CssClass="results-table" ShowFooter="True">
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
                                    <asp:TemplateField HeaderText="Person" SortExpression="PersonName">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="tbxPersonName" runat="server" Width="8em" Text='<%# Bind("PersonName") %>' />
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="tbxPersonName" runat="server" Width="8em" Text="" />
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblPersonName" runat="server" Text='<%# Bind("PersonName") %>' />
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
                                            <asp:DropDownList ID="ddlDayOfWeek" runat="server">
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
                                                DataTextField="SecurityUsername" DataValueField="SecurityUsername">
                                                <asp:ListItem Value="" Text="n/a" />
                                            </asp:DropDownList>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:DropDownList ID="ddlSecurityNames" runat="server" AppendDataBoundItems="True"
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
        <ajaxToolkit:TabPanel ID="tabpnlEquipment" runat="server" HeaderText="Equipment Types">
            <HeaderTemplate>
                Equipment Types
            </HeaderTemplate>
            <ContentTemplate>
                <asp:UpdatePanel ID="upnlEquipment" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div class="results-container">
                            <asp:GridView ID="gvEquipment" runat="server" AllowPaging="True" EmptyDataText="No equipment found"
                                AllowSorting="True" AutoGenerateColumns="False" BackColor="White" ShowFooter="True"
                                BorderColor="#E7E7FF" BorderStyle="None" BorderWidth="1px" CellPadding="3" DataKeyNames="EquipTypeID"
                                OnRowCommand="gvEquipment_RowCommand" PageSize="20"
                                OnPageIndexChanging="gvEquipment_PageIndexChanging"
                                OnSorting="gvEquipment_Sorting"
                                CssClass="results-table"
                                OnSelectedIndexChanged="gvEquipment_SelectedIndexChanged"
                                OnRowEditing="gvEquipment_RowEditing"
                                OnRowCancelingEdit="gvEquipment_RowCancelingEdit"
                                OnRowUpdating="gvEquipment_RowUpdating">
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
                                            <asp:HiddenField ID="EquipTypeIdLabel" runat="server" Value='<%# Eval("EquipTypeID") %>' />
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="EquipTypeNameTextBox" runat="server" Text="" />
                                            <asp:HiddenField ID="EquipTypeIdLabel" runat="server" Value='<%# Eval("EquipTypeID") %>' />
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="EquipTypeNameLabel" runat="server" Text='<%# Bind("EquipTypeName") %>'></asp:Label>
                                            <asp:HiddenField ID="EquipTypeIdLabel" runat="server" Value='<%# Eval("EquipTypeID") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="EquipTypeDesc" SortExpression="EquipTypeDescription">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="EquipTypeDescTextBox" runat="server" Text='<%# Bind("EquipTypeDescription") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="EquipTypeDescTextBox" runat="server" Text="" />
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="EquipTypeDescLabel" runat="server" Text='<%# Bind("EquipTypeDescription") %>'></asp:Label>
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
        <ajaxToolkit:TabPanel ID="tabpnlAreas" runat="server" HeaderText="Areas">
            <HeaderTemplate>
                Areas
            </HeaderTemplate>
            <ContentTemplate>
                <asp:UpdatePanel ID="upnlAreas" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="responsive-layout-container" style="display: flex; gap: 20px; width: 90%; height: 600px; box-sizing: border-box;">
                            <div class="layout-main-panel" style="flex: 1 1 0; min-width: 0; display: flex; flex-direction: column;">
                                <div class="results-container scrollable-table-container" style="flex: 1; overflow: auto;max-height: 580px;">
                                    <asp:GridView ID="gvAreas" runat="server" AllowPaging="True" PageSize="20" AllowSorting="True"
                                        AutoGenerateColumns="False" BackColor="White" BorderColor="#DEDFDE" BorderStyle="None" CssClass="results-table no-sticky-last"
                                        Style="width: 90%;"
                                        BorderWidth="1px" CellPadding="4" ForeColor="Black" DataKeyNames="ID"
                                        ShowFooter="True" OnRowCommand="gvAreas_OnRowCommand" OnSelectedIndexChanged="gvAreas_OnSelectedIndexChanged"
                                        OnPageIndexChanging="gvAreas_PageIndexChanging"
                                        OnSorting="gvAreas_Sorting"
                                        OnRowEditing="gvAreas_RowEditing"
                                        OnRowCancelingEdit="gvAreas_RowCancelingEdit"
                                        OnRowUpdating="gvAreas_RowUpdating">
                                        <AlternatingRowStyle BackColor="White" />
                                        <Columns>
                                            <asp:CommandField ShowSelectButton="True" SelectImageUrl="~/images/imgButtons/SelectItem.gif" ButtonType="Image" />
                                            <asp:TemplateField HeaderText="Area Name" SortExpression="AreaName" ItemStyle-CssClass="wrap"  >
                                                <EditItemTemplate>
                                                    <asp:TextBox ID="tbxAreaName" runat="server" Text='<%# Bind("AreaName") %>'></asp:TextBox>
                                                </EditItemTemplate>
                                                <ItemTemplate>
                                                    <asp:Label ID="lblAreaName" runat="server" Text='<%# Bind("AreaName") %>'></asp:Label>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:TextBox ID="tbxAreaName" runat="server" Text=""></asp:TextBox>
                                                </FooterTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="ID" Visible="false">
                                                <EditItemTemplate>
                                                    <asp:Label ID="lblAreaID" runat="server" Text='<%# Bind("ID") %>' />
                                                </EditItemTemplate>
                                                <ItemTemplate>
                                                    <asp:Label ID="lblAreaID" runat="server" Text='<%# Bind("ID") %>' />
                                                </ItemTemplate>
                                                <FooterTemplate></FooterTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField ShowHeader="False">
                                                <EditItemTemplate>
                                                    <asp:ImageButton ID="btnAreaUpdate" runat="server" CausesValidation="True" CommandName="Update"
                                                        AlternateText="Update" ImageUrl="~/images/imgButtons/UpdateItem.gif" />
                                                    &nbsp;<asp:ImageButton ID="btnAreaCancel" runat="server" CausesValidation="False"
                                                        CommandName="Cancel" AlternateText="Cancel" ImageUrl="~/images/imgButtons/CancelItem.gif" />
                                                </EditItemTemplate>
                                                <ItemTemplate>
                                                    <asp:ImageButton ID="btnAreaEdit" runat="server" CausesValidation="False" CommandName="Edit"
                                                        AlternateText="Edit" ImageUrl="~/images/imgButtons/EditItem.gif" />
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:ImageButton ID="btnAreaInsert" runat="server" CommandName="AddArea" AlternateText="Add"
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
                            <div class="layout-detail-panel" style="flex: 1 1 0; min-width: 0; display: flex; flex-direction: column;">
                                <h4 style="margin-top: 0; margin-bottom: 10px;">Delivery Days for Selected Area</h4>
                                <div class="layout-panel-top scrollable-table-container" style="flex: 1; overflow: auto; max-height: 560px;">
                                    <asp:GridView ID="gvAreaDays" runat="server" AutoGenerateColumns="False"
                                        CssClass="results-table" Visible="false" ShowFooter="true" DataKeyNames="AreaPrepDaysID"
                                        OnRowEditing="gvAreaDays_RowEditing"
                                        OnRowCancelingEdit="gvAreaDays_RowCancelingEdit"
                                        OnRowUpdating="gvAreaDays_OnRowUpdating"
                                        OnRowDeleting="gvAreaDays_RowDeleting"
                                        OnRowCommand="gvAreaDays_RowCommand">
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
                                            <asp:Button ID="btnAddAreaDay" runat="server" Text="Add Prep Day" OnClick="btnAddAreaDay_Click" />
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
                                                    <asp:DropDownList ID="ddlPreperationDoW" runat="server">
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
                                                    <asp:HiddenField ID="AreaPrepDaysIDHidden" runat="server" Value='<%# Bind("AreaPrepDaysID") %>' />
                                                </EditItemTemplate>
                                                <ItemTemplate>
                                                    &nbsp;+&nbsp;<asp:Label ID="lblDeliveryDay" runat="server" Width="2em" Text='<%# Bind("DeliveryDelayDays") %>' />
                                                    =&nbsp;<asp:Label ID="AreaNameLabel" runat="server" Text='<%# GetDeliveryDay(Eval("PrepDayOfWeekID").ToString(),Eval("DeliveryDelayDays").ToString()) %>' />
                                                    <asp:HiddenField ID="AreaPrepDaysIDHidden" runat="server" Value='<%# Bind("AreaPrepDaysID") %>' />
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:TextBox ID="tbxDeliveryDelay" runat="server" Width="2em" Text='1'></asp:TextBox>
                                                    <asp:HiddenField ID="AreaPrepDaysIDHidden" runat="server" Value='<%# Bind("AreaPrepDaysID") %>' />
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
                                                    <asp:ImageButton ID="btnAreaDaysUpdate" runat="server" CausesValidation="False" CommandName="Update"
                                                        AlternateText="go" ImageUrl="~/images/imgButtons/UpdateItem.gif" />
                                                    &nbsp;
                                                    <asp:ImageButton ID="btnAreaDaysCancel" runat="server" CausesValidation="False" CommandName="Cancel"
                                                        AlternateText="no" ImageUrl="~/images/imgButtons/CancelItem.gif" />
                                                </EditItemTemplate>
                                                <ItemTemplate>
                                                    <asp:ImageButton ID="btnAreaDaysEdit" runat="server" CausesValidation="False" CommandName="Edit"
                                                        Text="Edit" ImageUrl="~/images/imgButtons/EditItem.gif" />&nbsp;
                                                    <asp:ImageButton ID="btnAreaDaysDelete" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete"
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
                                ShowFooter="true" AllowPaging="True" PageSize="20"
                                AllowSorting="True"
                                OnPageIndexChanging="gvPackaging_PageIndexChanging"
                                OnSorting="gvPackaging_Sorting"
                                DataKeyNames="ItemPackagingID"
                                OnRowEditing="gvPackaging_RowEditing"
                                OnRowCancelingEdit="gvPackaging_RowCancelingEdit"
                                OnRowUpdating="gvPackaging_RowUpdating">
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
                                    <asp:TemplateField HeaderText="Description" SortExpression="ItemPackagingDesc">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBoxDescription" runat="server" Text='<%# Bind("ItemPackagingDesc") %>' />
                                            <asp:HiddenField ID="hdnPackagingID" runat="server" Value='<%# Bind("ItemPackagingID") %>' />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="LabelDescription" runat="server" Text='<%# Bind("ItemPackagingDesc") %>' />
                                            <asp:HiddenField ID="hdnPackagingID" runat="server" Value='<%# Bind("ItemPackagingID") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="TextBoxDescription" runat="server" Text="" />
                                            <asp:HiddenField ID="hdnPackagingID" runat="server" Value='<%# Bind("ItemPackagingID") %>' />
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
                            <asp:GridView ID="gvInvoiceTypes" runat="server" AllowSorting="True" DataKeyNames="InvoiceTypeID"
                                CssClass="results-table" AutoGenerateColumns="False" ShowFooter="true"
                                OnRowCommand="gvInvoiceTypes_RowCommand"
                                OnPageIndexChanging="gvInvoiceTypes_PageIndexChanging"
                                OnSorting="gvInvoiceTypes_Sorting"
                                OnRowEditing="gvInvoiceTypes_RowEditing"
                                OnRowCancelingEdit="gvInvoiceTypes_RowCancelingEdit"
                                OnRowUpdating="gvInvoiceTypes_RowUpdating">
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
                            <asp:GridView ID="gvPaymentTerms" runat="server" AllowSorting="True" DataKeyNames="PaymentTermID"
                                CssClass="results-table" AutoGenerateColumns="False" ShowFooter="true"
                                OnRowCommand="gvPaymentTerms_RowCommand"
                                OnPageIndexChanging="gvPaymentTerms_PageIndexChanging"
                                OnSorting="gvPaymentTerms_Sorting"
                                OnRowEditing="gvPaymentTerms_RowEditing"
                                OnRowCancelingEdit="gvPaymentTerms_RowCancelingEdit"
                                OnRowUpdating="gvPaymentTerms_RowUpdating">
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
                            <asp:GridView ID="gvPriceLevels" runat="server" AllowSorting="True" DataKeyNames="PriceLevelID"
                                CssClass="results-table" AutoGenerateColumns="False" ShowFooter="true"
                                OnRowCommand="gvPriceLevels_RowCommand"
                                OnPageIndexChanging="gvPriceLevels_PageIndexChanging"
                                OnSorting="gvPriceLevels_Sorting"
                                OnRowEditing="gvPriceLevels_RowEditing"
                                OnRowCancelingEdit="gvPriceLevels_RowCancelingEdit"
                                OnRowUpdating="gvPriceLevels_RowUpdating">
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
                                AutoGenerateColumns="False" AllowPaging="True" AllowSorting="True"
                                ShowFooter="True" PageSize="20"
                                OnPageIndexChanging="gvRepairStatuses_PageIndexChanging"
                                OnSorting="gvRepairStatuses_Sorting"
                                OnRowCommand="gvRepairStatuses_RowCommand"
                                OnRowEditing="gvRepairStatuses_RowEditing"
                                OnRowCancelingEdit="gvRepairStatuses_RowCancelingEdit"
                                OnRowUpdating="gvRepairStatuses_RowUpdating"
                                OnRowDeleting="gvRepairStatuses_RowDeleting">
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
                                    <asp:CheckBoxField DataField="EmailContact" HeaderText="Email Contact" />
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

                    </ContentTemplate>
                </asp:UpdatePanel>
            </ContentTemplate>
        </ajaxToolkit:TabPanel>
    </ajaxToolkit:TabContainer>
    <%-- REMOVED: sdsItems SqlDataSource - VIOLATES HARD_PROJECT_RULES.md Rule #2 - Use ItemsRepository in code-behind --%>
    <%-- REMOVED: odsAllItems ObjectDataSource - VIOLATES HARD_PROJECT_RULES.md Rule #2 - Legacy ItemTypeTbl, use ItemsRepository --%>
    
    <asp:SqlDataSource ID="sdsUserNames" runat="server"
        ConnectionString="<%$ ConnectionStrings:ApplicationServices %>"
        SelectCommand="SELECT [UserName] AS SecurityUsername FROM [vw_aspnet_Users]" />
    
</asp:Content>
