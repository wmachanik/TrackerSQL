<%@ Page Title="Lookups" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Lookups.aspx.cs" Inherits="TrackerSQL.Pages.Lookups" MaintainScrollPositionOnPostback="true" %>

<asp:Content ID="cntLookupHdr" title="Lookups" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        // In-cell <input type=color> (over the icon). Anchors the OS picker to the cell,
        // unlike a temporary body input which Edge places at the top-left.
        function packagingColorEditor(fromEl) {
            var swatch = fromEl && fromEl.parentNode;
            return swatch ? swatch.parentNode : null;
        }

        function packagingSyncColorHit(hit) {
            try {
                var editor = packagingColorEditor(hit);
                var tb = editor ? editor.querySelector('input.packaging-color-hex') : null;
                var v = (tb && tb.value ? tb.value : '').replace(/^#+/, '').trim();
                if (/^[0-9A-Fa-f]{6}$/.test(v))
                    hit.value = '#' + v;
            } catch (ex) { }
        }

        function packagingApplyColorHit(hit) {
            try {
                if (!hit || !hit.value) return;
                var editor = packagingColorEditor(hit);
                var tb = editor ? editor.querySelector('input.packaging-color-hex') : null;
                if (tb) tb.value = hit.value.toUpperCase();
            } catch (ex) { }
        }
    </script>
</asp:Content>
<asp:Content ID="cntLookupBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="scmLookup" runat="server" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="uprgLookup" runat="server" DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info page-tone-progress">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:Panel ID="pnlLookups" runat="server" CssClass="simpleForm page-tone-panel page-tone-lookups">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/icons8-lookups-30.png" alt="" />
            <div>
                <h1 class="page-tone-title">Lookups</h1>
                <p class="page-tone-subtitle">Configure system lookup tables</p>
            </div>
        </div>

        <ajaxToolkit:TabContainer ID="tabcLookup" runat="server" ActiveTabIndex="0" CssClass="MyTabStyle" ScrollBars="None" UseVerticalStripPlacement="false">
        <ajaxToolkit:TabPanel runat="server" HeaderText="Items" ID="tabpnlItems">
            <HeaderTemplate>
                Items
            </HeaderTemplate>
            <ContentTemplate>
                <asp:UpdatePanel ID="upnlItems" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="gvItems" />
                        <asp:AsyncPostBackTrigger ControlID="btnGon" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="btnReset" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="tbxItemSearch" EventName="TextChanged" />
                    </Triggers>
                    <ContentTemplate>
                        <div class="filter-toolbar lookups-tab-toolbar">
                            <div class="filter-section search-controls">
                                <div class="filter-control">
                                    <asp:Label AssociatedControlID="tbxItemSearch" runat="server" Text="Search (name / SKU / ID):" />
                                    <asp:TextBox ID="tbxItemSearch" runat="server" AutoPostBack="true"
                                        OnTextChanged="tbxItemSearch_TextChanged"
                                        ToolTip="Search by item name, SKU, abbreviation, or Item ID (e.g. 8JuraDecT36 or 634)" />
                                </div>
                                <asp:Button ID="btnGon" Text="Go" runat="server" CssClass="filter-panel-btn"
                                    CausesValidation="false"
                                    ToolTip="search for this item" OnClick="btnGo_Click" />
                                <asp:Button ID="btnReset" Text="Reset" runat="server" CssClass="filter-panel-btn"
                                    CausesValidation="false"
                                    OnClick="btnReset_Click" />
                            </div>
                            <div class="filter-section admin-controls lookups-tab-icon-wrap">
                                <img class="lookups-tab-icon" src="../images/imgButtons/icons8-list-of-items.png"
                                    width="32" height="32" alt="" />
                            </div>
                        </div>
                        <div class="results-container scrollable-table-container">
                            <asp:GridView ID="gvItems" runat="server" AllowPaging="True"
                                PageSize="20" CssClass="results-table sticky-first-column" Font-Size="Small" AllowSorting="True" AutoGenerateColumns="False"
                                OnRowCommand="gvItems_RowCommand" ShowFooter="True" CellPadding="0"
                                PagerStyle-CssClass="pager-row"
                                DataKeyNames="ItemID"
                                OnPageIndexChanging="gvItems_PageIndexChanging"
                                OnSorting="gvItems_Sorting"
                                OnRowEditing="gvItems_RowEditing"
                                OnRowCancelingEdit="gvItems_RowCancelingEdit"
                                OnRowUpdating="gvItems_RowUpdating"
                                OnRowDataBound="gvItems_RowDataBound"
                                OnRowCreated="gvItems_RowCreated">
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
                                                AppendDataBoundItems="true" DataTextField="ItemServiceTypeName" DataValueField="ItemServiceTypeID"
                                                Width="10em">
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:DropDownList ID="ddlServiceType" runat="server"
                                                AppendDataBoundItems="true" DataTextField="ItemServiceTypeName" DataValueField="ItemServiceTypeID"
                                                Width="10em">
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlServiceType" runat="server"
                                                AppendDataBoundItems="true" DataTextField="ItemServiceTypeName" DataValueField="ItemServiceTypeID"
                                                Enabled="False" Width="10em">
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Replcment" SortExpression="ReplacementItemID">
                                        <EditItemTemplate>
                                            <asp:DropDownList ID="ddlReplacement" runat="server" AppendDataBoundItems="True"
                                                DataTextField="ItemDesc" DataValueField="ItemID">
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:DropDownList ID="ddlReplacement" runat="server" AppendDataBoundItems="True"
                                                DataTextField="ItemDesc" DataValueField="ItemID">
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlReplacement" runat="server" AppendDataBoundItems="True"
                                                DataTextField="ItemDesc" DataValueField="ItemID"
                                                Enabled="False">
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
                                                CssClass="lookups-item-uom" DataTextField="UnitOfMeasure" Width="6em"
                                                DataValueField="ItemUnitID">
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:DropDownList ID="ddlUnits" runat="server" AppendDataBoundItems="True"
                                                CssClass="lookups-item-uom" DataTextField="UnitOfMeasure" Width="6em"
                                                DataValueField="ItemUnitID">
                                                <asp:ListItem Value="0" Text="n/a" />
                                            </asp:DropDownList>
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblItemUnit" runat="server" Text="n/a" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="S/O" SortExpression="SortOrder" HeaderStyle-CssClass="col-tight" ItemStyle-CssClass="col-tight" FooterStyle-CssClass="col-tight">
                                        <EditItemTemplate>
                                            <asp:DropDownList ID="ddlSortOrder" runat="server" CssClass="lookups-item-so" Font-Size="Smaller" Width="11em" />
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:DropDownList ID="ddlSortOrder" runat="server" CssClass="lookups-item-so" Font-Size="Smaller" Width="11em" />
                                        </FooterTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblSortOrder" runat="server" />
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
                                <PagerTemplate>
                                    <asp:PlaceHolder ID="plhPager" runat="server" />
                                </PagerTemplate>

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
                        <div class="filter-toolbar lookups-tab-toolbar">
                            <div class="filter-section search-controls">
                                <div class="filter-control">
                                    <asp:Label AssociatedControlID="tbxPeopleSearch" runat="server" Text="Search:" />
                                    <asp:TextBox ID="tbxPeopleSearch" runat="server" OnTextChanged="tbxPeopleSearch_TextChanged" />
                                </div>
                                <asp:Button ID="btnPeopleGo" Text="Go" runat="server" CssClass="filter-panel-btn"
                                    ToolTip="Search people by name or abbreviation" OnClick="btnPeopleGo_Click" />
                                <asp:Button ID="btnPeopleReset" Text="Reset" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnPeopleReset_Click" />
                            </div>
                            <div class="filter-section admin-controls lookups-tab-icon-wrap">
                                <img class="lookups-tab-icon" src="../images/imgButtons/icons8-list-of-people.png"
                                    width="32" height="32" alt="" />
                            </div>
                        </div>
                        <div class="responsive-layout-container scrollable-table-container">
                            <asp:GridView ID="gvPeople" runat="server" AllowPaging="True" AllowSorting="True"
                                AutoGenerateColumns="False" PageSize="20" DataKeyNames="PersonID"
                                OnRowCommand="gvPeople_RowCommand" OnRowUpdating="gvPeople_RowUpdating"
                                OnRowEditing="gvPeople_RowEditing" OnRowCancelingEdit="gvPeople_RowCancelingEdit"
                                OnRowDataBound="gvPeople_RowDataBound"
                                OnPageIndexChanging="gvPeople_PageIndexChanging"
                                OnSorting="gvPeople_Sorting"
                                OnRowCreated="gvPeople_RowCreated"
                                CssClass="results-table" ShowFooter="True">
                                <Columns>
                                    <asp:TemplateField ShowHeader="False">
                                        <EditItemTemplate>
                                            <asp:ImageButton ID="btnUpdate" runat="server" CausesValidation="False" CommandName="Update"
                                                AlternateText="go" ImageUrl="~/images/imgButtons/UpdateItem.gif" />&nbsp;
                                            <asp:ImageButton ID="btnCancel" runat="server" CausesValidation="False" CommandName="Cancel"
                                                AlternateText="no" ImageUrl="~/images/imgButtons/CancelItem.gif" />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:ImageButton ID="btnEdit" runat="server" CausesValidation="False"
                                                CommandName="Edit" AlternateText="Edit"
                                                ImageUrl="~/images/imgButtons/EditItem.gif" />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:ImageButton ID="btnAdd" runat="server" CausesValidation="False" CommandName="AddItem"
                                                ImageUrl="~/images/imgButtons/AddItem.gif" AlternateText="Add" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ConvertEmptyStringToNull="False" HeaderText="Person" SortExpression="PersonName">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="tbxPersonName" runat="server" Width="15em" Text='<%# Bind("PersonName") %>' />
                                            <asp:HiddenField ID="hdnPersonID" runat="server" Value='<%# Bind("PersonID") %>' />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblPersonName" runat="server" Text='<%# Bind("PersonName") %>' />
                                            <asp:HiddenField ID="hdnPersonID" runat="server" Value='<%# Bind("PersonID") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="tbxPersonName" runat="server" Width="15em" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ConvertEmptyStringToNull="False" HeaderText="Abbreviation" SortExpression="Abbreviation">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="tbxAbbreviation" runat="server" Width="8em" Text='<%# Bind("Abbreviation") %>' />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblAbbreviation" runat="server" Text='<%# Bind("Abbreviation") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="tbxAbbreviation" runat="server" Width="8em" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ConvertEmptyStringToNull="False" HeaderText="Enabled" SortExpression="Enabled">
                                        <EditItemTemplate>
                                            <asp:CheckBox ID="cbxEnabled" runat="server" Text="Yes" Checked='<%# Bind("Enabled") %>' />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:CheckBox ID="cbxEnabled" runat="server" Text="Yes" Checked='<%# Bind("Enabled") %>' Enabled="false" />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:CheckBox ID="cbxEnabled" runat="server" Text="Yes" Checked="true" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Normal Delivery DoW" SortExpression="NormalDeliveryDoW">
                                        <EditItemTemplate>
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
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlDayOfWeek" runat="server" Enabled="false">
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
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Username" SortExpression="SecurityUsername">
                                        <EditItemTemplate>
                                            <asp:DropDownList ID="ddlSecurityNames" runat="server" AppendDataBoundItems="True"
                                                DataTextField="SecurityUsername" DataValueField="SecurityUsername">
                                                <asp:ListItem Value="" Text="n/a" />
                                            </asp:DropDownList>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblSecurityName" runat="server" Text='<%# Eval("SecurityUsername") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:DropDownList ID="ddlSecurityNames" runat="server" AppendDataBoundItems="True"
                                                DataTextField="SecurityUsername" DataValueField="SecurityUsername">
                                                <asp:ListItem Value="" Text="n/a" />
                                            </asp:DropDownList>
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <PagerStyle CssClass="pager-row" />
                                <PagerTemplate>
                                    <asp:PlaceHolder ID="plhPager" runat="server" />
                                </PagerTemplate>
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
                        <div class="filter-toolbar lookups-tab-toolbar">
                            <div class="filter-section search-controls">
                                <div class="filter-control">
                                    <asp:Label AssociatedControlID="tbxEquipSearch" runat="server" Text="Search:" />
                                    <asp:TextBox ID="tbxEquipSearch" runat="server" OnTextChanged="tbxEquipSearch_TextChanged" />
                                </div>
                                <asp:Button ID="btnEquipGo" Text="Go" runat="server" CssClass="filter-panel-btn"
                                    ToolTip="Search equipment types by name or description" OnClick="btnEquipGo_Click" />
                                <asp:Button ID="btnEquipReset" Text="Reset" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnEquipReset_Click" />
                            </div>
                            <div class="filter-section admin-controls lookups-tab-icon-wrap">
                                <img class="lookups-tab-icon" src="../images/imgButtons/icons8-list-of-equipment.png"
                                    width="32" height="32" alt="" />
                            </div>
                        </div>
                        <div class="responsive-layout-container scrollable-table-container">
                            <asp:GridView ID="gvEquipment" runat="server" AllowPaging="True" EmptyDataText="No equipment found"
                                AllowSorting="True" AutoGenerateColumns="False" ShowFooter="True"
                                DataKeyNames="EquipTypeID" PageSize="20"
                                OnRowCommand="gvEquipment_RowCommand"
                                OnPageIndexChanging="gvEquipment_PageIndexChanging"
                                OnSorting="gvEquipment_Sorting"
                                CssClass="results-table"
                                OnRowEditing="gvEquipment_RowEditing"
                                OnRowCancelingEdit="gvEquipment_RowCancelingEdit"
                                OnRowUpdating="gvEquipment_RowUpdating"
                                OnRowCreated="gvEquipment_RowCreated">
                                <Columns>
                                    <asp:TemplateField ShowHeader="False">
                                        <EditItemTemplate>
                                            <asp:ImageButton ID="btnUpdate" runat="server" CausesValidation="False" CommandName="Update"
                                                AlternateText="go" ImageUrl="~/images/imgButtons/UpdateItem.gif" />&nbsp;
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
                                    <asp:TemplateField ConvertEmptyStringToNull="False" HeaderText="Equipment Type" SortExpression="EquipTypeName">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="EquipTypeNameTextBox" runat="server" Text='<%# Bind("EquipTypeName") %>' Width="15em" />
                                            <asp:HiddenField ID="EquipTypeIdLabel" runat="server" Value='<%# Eval("EquipTypeID") %>' />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="EquipTypeNameLabel" runat="server" Text='<%# Bind("EquipTypeName") %>' />
                                            <asp:HiddenField ID="EquipTypeIdLabel" runat="server" Value='<%# Eval("EquipTypeID") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="EquipTypeNameTextBox" runat="server" Width="15em" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ConvertEmptyStringToNull="False" HeaderText="Description" SortExpression="EquipTypeDescription">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="EquipTypeDescTextBox" runat="server" Text='<%# Bind("EquipTypeDescription") %>' Width="30em" />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="EquipTypeDescLabel" runat="server" Text='<%# Bind("EquipTypeDescription") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="EquipTypeDescTextBox" runat="server" Width="30em" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <PagerStyle CssClass="pager-row" />
                                <PagerTemplate>
                                    <asp:PlaceHolder ID="plhPager" runat="server" />
                                </PagerTemplate>
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
                        <div class="filter-toolbar lookups-tab-toolbar">
                            <div class="filter-section search-controls">
                                <div class="filter-control">
                                    <asp:Label AssociatedControlID="tbxAreaSearch" runat="server" Text="Search:" />
                                    <asp:TextBox ID="tbxAreaSearch" runat="server" OnTextChanged="tbxAreaSearch_TextChanged" />
                                </div>
                                <asp:Button ID="btnAreaGo" Text="Go" runat="server" CssClass="filter-panel-btn"
                                    ToolTip="Search areas by name" OnClick="btnAreaGo_Click" />
                                <asp:Button ID="btnAreaReset" Text="Reset" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnAreaReset_Click" />
                            </div>
                            <div class="filter-section admin-controls lookups-tab-icon-wrap">
                                <img class="lookups-tab-icon" src="../images/imgButtons/icons8-list-of-areas.png"
                                    width="32" height="32" alt="" />
                            </div>
                        </div>
                        <div class="lookups-areas-layout">
                            <div class="lookups-areas-list">
                                <div class="lookups-areas-list-scroll">
                                    <asp:GridView ID="gvAreas" runat="server" AllowPaging="True" PageSize="15" AllowSorting="True"
                                        AutoGenerateColumns="False" CssClass="results-table lookups-areas-table no-sticky-last"
                                        DataKeyNames="AreaID" ShowFooter="True"
                                        OnRowCommand="gvAreas_OnRowCommand" OnSelectedIndexChanged="gvAreas_OnSelectedIndexChanged"
                                        OnPageIndexChanging="gvAreas_PageIndexChanging"
                                        OnSorting="gvAreas_Sorting"
                                        OnRowEditing="gvAreas_RowEditing"
                                        OnRowCancelingEdit="gvAreas_RowCancelingEdit"
                                        OnRowUpdating="gvAreas_RowUpdating"
                                        OnRowCreated="gvAreas_RowCreated">
                                        <Columns>
                                            <asp:CommandField ShowSelectButton="True"
                                                SelectImageUrl="~/images/imgButtons/SelectItem.gif" ButtonType="Image"
                                                HeaderStyle-CssClass="col-cmd" ItemStyle-CssClass="col-cmd"
                                                FooterStyle-CssClass="col-cmd" />
                                            <asp:TemplateField HeaderText="Area Name" SortExpression="AreaName"
                                                HeaderStyle-CssClass="col-area-name" ItemStyle-CssClass="col-area-name wrap"
                                                FooterStyle-CssClass="col-area-name">
                                                <EditItemTemplate>
                                                    <asp:TextBox ID="tbxAreaName" runat="server" Text='<%# Bind("AreaName") %>' Width="100%" />
                                                </EditItemTemplate>
                                                <ItemTemplate>
                                                    <asp:Label ID="lblAreaName" runat="server" Text='<%# Bind("AreaName") %>' />
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:TextBox ID="tbxAreaName" runat="server" Text="" Width="100%" />
                                                </FooterTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="ID" Visible="false">
                                                <EditItemTemplate>
                                                    <asp:Label ID="lblAreaID" runat="server" Text='<%# Bind("AreaID") %>' />
                                                </EditItemTemplate>
                                                <ItemTemplate>
                                                    <asp:Label ID="lblAreaID" runat="server" Text='<%# Bind("AreaID") %>' />
                                                </ItemTemplate>
                                                <FooterTemplate></FooterTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField ShowHeader="False"
                                                HeaderStyle-CssClass="col-cmd" ItemStyle-CssClass="col-cmd"
                                                FooterStyle-CssClass="col-cmd">
                                                <EditItemTemplate>
                                                    <asp:ImageButton ID="btnAreaUpdate" runat="server" CausesValidation="True" CommandName="Update"
                                                        AlternateText="Update" ImageUrl="~/images/imgButtons/UpdateItem.gif" />
                                                    <asp:ImageButton ID="btnAreaCancel" runat="server" CausesValidation="False"
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
                                        <SelectedRowStyle CssClass="SelectedRowStyle" />
                                        <PagerStyle CssClass="pager-row" />
                                        <PagerTemplate>
                                            <asp:PlaceHolder ID="plhPager" runat="server" />
                                        </PagerTemplate>
                                    </asp:GridView>
                                </div>
                            </div>
                            <div class="lookups-areas-days">
                                <h4 class="lookups-areas-days-title">Delivery Days for Selected Area</h4>
                                <div class="lookups-areas-days-scroll">
                                    <asp:GridView ID="gvAreaDays" runat="server" AutoGenerateColumns="False"
                                        CssClass="results-table in-panel-grid lookups-area-days-table no-sticky-last"
                                        Visible="false" ShowFooter="true" DataKeyNames="AreaPrepDaysID"
                                        OnRowEditing="gvAreaDays_RowEditing"
                                        OnRowCancelingEdit="gvAreaDays_RowCancelingEdit"
                                        OnRowUpdating="gvAreaDays_OnRowUpdating"
                                        OnRowDeleting="gvAreaDays_RowDeleting"
                                        OnRowCommand="gvAreaDays_RowCommand">
                                        <EmptyDataTemplate>
                                            <div class="lookups-area-days-empty">
                                                <asp:DropDownList ID="ddlPreperationDoW" runat="server">
                                                    <asp:ListItem Value="1">Sunday</asp:ListItem>
                                                    <asp:ListItem Value="2">Monday</asp:ListItem>
                                                    <asp:ListItem Selected="True" Value="3">Tuesday</asp:ListItem>
                                                    <asp:ListItem Value="4">Wednesday</asp:ListItem>
                                                    <asp:ListItem Value="5">Thursday</asp:ListItem>
                                                    <asp:ListItem Value="6">Friday</asp:ListItem>
                                                    <asp:ListItem Value="7">Saturday</asp:ListItem>
                                                </asp:DropDownList>
                                                <asp:TextBox ID="tbxDeliveryDelay" runat="server" Text="1" Width="3em"
                                                    ToolTip="Delivery delay (days)" />
                                                <asp:TextBox ID="tbxDeliveryOrder" runat="server" Text="20" Width="3em"
                                                    ToolTip="Delivery order" />
                                                <asp:Button ID="btnAddAreaDay" runat="server" Text="Add Prep Day"
                                                    CssClass="filter-panel-btn" OnClick="btnAddAreaDay_Click" />
                                            </div>
                                        </EmptyDataTemplate>
                                        <Columns>
                                            <asp:TemplateField HeaderText="Prep Day"
                                                HeaderStyle-CssClass="col-prep-day" ItemStyle-CssClass="col-prep-day"
                                                FooterStyle-CssClass="col-prep-day">
                                                <EditItemTemplate>
                                                    <asp:DropDownList ID="ddlPreperationDoW" runat="server"
                                                        SelectedValue='<%# Eval("PrepDayOfWeekID") == null ? "0" : Eval("PrepDayOfWeekID").ToString() %>'>
                                                        <asp:ListItem Value="0" Text="--select a day--" />
                                                        <asp:ListItem Value="1">Sunday</asp:ListItem>
                                                        <asp:ListItem Value="2">Monday</asp:ListItem>
                                                        <asp:ListItem Value="3">Tuesday</asp:ListItem>
                                                        <asp:ListItem Value="4">Wednesday</asp:ListItem>
                                                        <asp:ListItem Value="5">Thursday</asp:ListItem>
                                                        <asp:ListItem Value="6">Friday</asp:ListItem>
                                                        <asp:ListItem Value="7">Saturday</asp:ListItem>
                                                    </asp:DropDownList>
                                                </EditItemTemplate>
                                                <ItemTemplate>
                                                    <asp:Label ID="lblPrepDay" runat="server"
                                                        Text='<%# GetPrepDayName(Eval("PrepDayOfWeekID")) %>' />
                                                </ItemTemplate>
                                                <FooterTemplate>
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
                                                </FooterTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Dlvry Delay" SortExpression="DeliveryDelayDays"
                                                HeaderStyle-CssClass="col-tight" ItemStyle-CssClass="col-tight"
                                                FooterStyle-CssClass="col-tight">
                                                <EditItemTemplate>
                                                    <asp:TextBox ID="tbxDeliveryDelay" runat="server" Width="2em" Text='<%# Bind("DeliveryDelayDays") %>' />
                                                    <asp:HiddenField ID="AreaPrepDaysIDHidden" runat="server" Value='<%# Bind("AreaPrepDaysID") %>' />
                                                </EditItemTemplate>
                                                <ItemTemplate>
                                                    +&nbsp;<asp:Label ID="lblDeliveryDay" runat="server" Text='<%# Bind("DeliveryDelayDays") %>' />
                                                    =&nbsp;<asp:Label ID="AreaNameLabel" runat="server" Text='<%# GetDeliveryDay(Eval("PrepDayOfWeekID").ToString(),Eval("DeliveryDelayDays").ToString()) %>' />
                                                    <asp:HiddenField ID="AreaPrepDaysIDHidden" runat="server" Value='<%# Bind("AreaPrepDaysID") %>' />
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:TextBox ID="tbxDeliveryDelay" runat="server" Width="2em" Text="1" />
                                                    <asp:HiddenField ID="AreaPrepDaysIDHidden" runat="server" Value='<%# Bind("AreaPrepDaysID") %>' />
                                                </FooterTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Dlvry Order" SortExpression="DeliveryOrder"
                                                HeaderStyle-CssClass="col-tight" ItemStyle-CssClass="col-tight"
                                                FooterStyle-CssClass="col-tight">
                                                <EditItemTemplate>
                                                    <asp:TextBox ID="tbxDeliveryOrder" runat="server" Width="2em" Text='<%# Bind("DeliveryOrder") %>' />
                                                </EditItemTemplate>
                                                <ItemTemplate>
                                                    <asp:Label ID="lblDeliveryOrder" runat="server" Text='<%# Bind("DeliveryOrder") %>' />
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:TextBox ID="tbxDeliveryOrder" runat="server" Width="2em" Text="30" />
                                                </FooterTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField ShowHeader="False"
                                                HeaderStyle-CssClass="col-cmd" ItemStyle-CssClass="col-cmd"
                                                FooterStyle-CssClass="col-cmd">
                                                <EditItemTemplate>
                                                    <asp:ImageButton ID="btnAreaDaysUpdate" runat="server" CausesValidation="False" CommandName="Update"
                                                        AlternateText="go" ImageUrl="~/images/imgButtons/UpdateItem.gif" />
                                                    <asp:ImageButton ID="btnAreaDaysCancel" runat="server" CausesValidation="False" CommandName="Cancel"
                                                        AlternateText="no" ImageUrl="~/images/imgButtons/CancelItem.gif" />
                                                </EditItemTemplate>
                                                <ItemTemplate>
                                                    <asp:ImageButton ID="btnAreaDaysEdit" runat="server" CausesValidation="False" CommandName="Edit"
                                                        Text="Edit" ImageUrl="~/images/imgButtons/EditItem.gif" />
                                                    <asp:ImageButton ID="btnAreaDaysDelete" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete"
                                                        ImageUrl="~/images/imgButtons/Trashcan.gif" />
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:ImageButton ID="btnAdd" runat="server" CausesValidation="False" CommandName="AddItem"
                                                        ImageUrl="~/images/imgButtons/AddItem.gif" AlternateText="Add" />
                                                </FooterTemplate>
                                            </asp:TemplateField>
                                        </Columns>
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
                        <div class="filter-toolbar lookups-tab-toolbar">
                            <div class="filter-section search-controls">
                                <div class="filter-control">
                                    <asp:Label AssociatedControlID="tbxPackagingSearch" runat="server" Text="Search:" />
                                    <asp:TextBox ID="tbxPackagingSearch" runat="server" OnTextChanged="tbxPackagingSearch_TextChanged" />
                                </div>
                                <asp:Button ID="btnPackagingGo" Text="Go" runat="server" CssClass="filter-panel-btn"
                                    ToolTip="Search packaging by description" OnClick="btnPackagingGo_Click" />
                                <asp:Button ID="btnPackagingReset" Text="Reset" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnPackagingReset_Click" />
                            </div>
                            <div class="filter-section admin-controls lookups-tab-icon-wrap">
                                <img class="lookups-tab-icon" src="../images/imgButtons/icons8-list-of-packaging.png"
                                    width="32" height="32" alt="" />
                            </div>
                        </div>
                        <div class="responsive-layout-container scrollable-table-container">
                            <asp:GridView ID="gvPackaging" runat="server" AutoGenerateColumns="False" CssClass="results-table results-table-fit"
                                OnRowCommand="gvPackaging_RowCommand" OnRowDataBound="gvPackaging_RowDataBound"
                                ShowFooter="true" AllowPaging="True" PageSize="20"
                                AllowSorting="True"
                                OnPageIndexChanging="gvPackaging_PageIndexChanging"
                                OnSorting="gvPackaging_Sorting"
                                DataKeyNames="ItemPackagingID"
                                OnRowEditing="gvPackaging_RowEditing"
                                OnRowCancelingEdit="gvPackaging_RowCancelingEdit"
                                OnRowUpdating="gvPackaging_RowUpdating"
                                OnRowCreated="gvPackaging_RowCreated">
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
                                    <asp:TemplateField HeaderText="BGColour" SortExpression="BGColour"
                                        ItemStyle-CssClass="packaging-color-col" HeaderStyle-CssClass="packaging-color-col">
                                        <EditItemTemplate>
                                            <span class="packaging-color-editor">
                                                <asp:TextBox ID="TextBoxBGColour" runat="server" Text='<%# Bind("BGColour") %>'
                                                    CssClass="packaging-color-hex" ToolTip="Background colour (#RRGGBB)" />
                                                <span class="packaging-color-swatch" title="Pick background colour">
                                                    <img class="packaging-color-icon" width="16" height="16" alt=""
                                                        src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAsTAAALEwEAmpwYAAACQ0lEQVR4nHWS72sScRjA75X/ikuLXs5RE4JyK0gmTQtpECk1crqt+SI1vTwnU8LzPJ3NRWKsmh3RDyaeDiyqEcz1Y5u6LRKpVbaI7XTuXRdPnDQ5z3zg8/Lz+T5feBBEMHgPJg0oXETghCsfUGB7CZmMI5+QyYi5rq6DSLtB1aiIUGChqVM3/9CX5uC95QNsOIvw0+uFL3Y7rOh0kJbL2URnZ4iSSEQCmRJ5lL7IQ+1dWHcW4dP45wa/fL4GWx4PvOnvB1p7NEKhvIjj/J3J0JUkLFhXd/iyMMDxFTcyWVQJtFEerMvXtdFD4xfusRnXJrx0l+GVZXm7XYCTKzOjsB0zQebqMZYeOSJFHAPRQGz0dV1uYF1ubCKU98l7z0HK2E0g9oFoIWH/2BzgbfI/maMcGYSUUZ7nArXn/9YXsmBZqW7ixopQ5tiJDQNt6q4hJ/WzNZv/O9gDWy2YyRIzk51gdosOEMJsWCCFd1S5QGHMW2yRx8hS5cazXQiv3YYHSxNVYaD8zgQ0Ls4hvfo4obMtNr8cLDGcvB/guJ9tjhSS/ZDCxT6k5yIlPj346LfV/61F5gf4mzDrFsiEDrM0fkBav4VefTyoNWfqf+bLwkA9knVXluLHgfZ3kI1LVKspkXL4aURPvgXHY6ZtgFydgsuUDSykqvmU6xGUEmmwdFDrfsEaptfg2uwPcDypQDAXAe8iCeakC85Oj7B94SFSTaHNMn/OYLRU45zH1dh8ToOla6qwYa9v0pBThYcI5S2jRCj8BSWLZfvoLu3GAAAAAElFTkSuQmCC" />
                                                    <input type="color" class="packaging-color-hit" value="#ffffff"
                                                        onmousedown="packagingSyncColorHit(this);"
                                                        oninput="packagingApplyColorHit(this);"
                                                        onchange="packagingApplyColorHit(this);" />
                                                </span>
                                            </span>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="LabelBGColour" runat="server" Text='<%# Bind("BGColour") %>'></asp:Label>
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <span class="packaging-color-editor">
                                                <asp:TextBox ID="TextBoxBGColour" runat="server" Text=""
                                                    CssClass="packaging-color-hex" ToolTip="Background colour (#RRGGBB)" />
                                                <span class="packaging-color-swatch" title="Pick background colour">
                                                    <img class="packaging-color-icon" width="16" height="16" alt=""
                                                        src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAsTAAALEwEAmpwYAAACQ0lEQVR4nHWS72sScRjA75X/ikuLXs5RE4JyK0gmTQtpECk1crqt+SI1vTwnU8LzPJ3NRWKsmh3RDyaeDiyqEcz1Y5u6LRKpVbaI7XTuXRdPnDQ5z3zg8/Lz+T5feBBEMHgPJg0oXETghCsfUGB7CZmMI5+QyYi5rq6DSLtB1aiIUGChqVM3/9CX5uC95QNsOIvw0+uFL3Y7rOh0kJbL2URnZ4iSSEQCmRJ5lL7IQ+1dWHcW4dP45wa/fL4GWx4PvOnvB1p7NEKhvIjj/J3J0JUkLFhXd/iyMMDxFTcyWVQJtFEerMvXtdFD4xfusRnXJrx0l+GVZXm7XYCTKzOjsB0zQebqMZYeOSJFHAPRQGz0dV1uYF1ubCKU98l7z0HK2E0g9oFoIWH/2BzgbfI/maMcGYSUUZ7nArXn/9YXsmBZqW7ixopQ5tiJDQNt6q4hJ/WzNZv/O9gDWy2YyRIzk51gdosOEMJsWCCFd1S5QGHMW2yRx8hS5cazXQiv3YYHSxNVYaD8zgQ0Ls4hvfo4obMtNr8cLDGcvB/guJ9tjhSS/ZDCxT6k5yIlPj346LfV/61F5gf4mzDrFsiEDrM0fkBav4VefTyoNWfqf+bLwkA9knVXluLHgfZ3kI1LVKspkXL4aURPvgXHY6ZtgFydgsuUDSykqvmU6xGUEmmwdFDrfsEaptfg2uwPcDypQDAXAe8iCeakC85Oj7B94SFSTaHNMn/OYLRU45zH1dh8ToOla6qwYa9v0pBThYcI5S2jRCj8BSWLZfvoLu3GAAAAAElFTkSuQmCC" />
                                                    <input type="color" class="packaging-color-hit" value="#ffffff"
                                                        onmousedown="packagingSyncColorHit(this);"
                                                        oninput="packagingApplyColorHit(this);"
                                                        onchange="packagingApplyColorHit(this);" />
                                                </span>
                                            </span>
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Colour" SortExpression="Colour"
                                        ItemStyle-CssClass="packaging-color-col" HeaderStyle-CssClass="packaging-color-col">
                                        <EditItemTemplate>
                                            <span class="packaging-color-editor">
                                                <asp:TextBox ID="TextBoxColour" runat="server" Text='<%# Bind("Colour") %>'
                                                    CssClass="packaging-color-hex" ToolTip="Foreground colour (#RRGGBB)" />
                                                <span class="packaging-color-swatch" title="Pick foreground colour">
                                                    <img class="packaging-color-icon" width="16" height="16" alt=""
                                                        src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAsTAAALEwEAmpwYAAACQ0lEQVR4nHWS72sScRjA75X/ikuLXs5RE4JyK0gmTQtpECk1crqt+SI1vTwnU8LzPJ3NRWKsmh3RDyaeDiyqEcz1Y5u6LRKpVbaI7XTuXRdPnDQ5z3zg8/Lz+T5feBBEMHgPJg0oXETghCsfUGB7CZmMI5+QyYi5rq6DSLtB1aiIUGChqVM3/9CX5uC95QNsOIvw0+uFL3Y7rOh0kJbL2URnZ4iSSEQCmRJ5lL7IQ+1dWHcW4dP45wa/fL4GWx4PvOnvB1p7NEKhvIjj/J3J0JUkLFhXd/iyMMDxFTcyWVQJtFEerMvXtdFD4xfusRnXJrx0l+GVZXm7XYCTKzOjsB0zQebqMZYeOSJFHAPRQGz0dV1uYF1ubCKU98l7z0HK2E0g9oFoIWH/2BzgbfI/maMcGYSUUZ7nArXn/9YXsmBZqW7ixopQ5tiJDQNt6q4hJ/WzNZv/O9gDWy2YyRIzk51gdosOEMJsWCCFd1S5QGHMW2yRx8hS5cazXQiv3YYHSxNVYaD8zgQ0Ls4hvfo4obMtNr8cLDGcvB/guJ9tjhSS/ZDCxT6k5yIlPj346LfV/61F5gf4mzDrFsiEDrM0fkBav4VefTyoNWfqf+bLwkA9knVXluLHgfZ3kI1LVKspkXL4aURPvgXHY6ZtgFydgsuUDSykqvmU6xGUEmmwdFDrfsEaptfg2uwPcDypQDAXAe8iCeakC85Oj7B94SFSTaHNMn/OYLRU45zH1dh8ToOla6qwYa9v0pBThYcI5S2jRCj8BSWLZfvoLu3GAAAAAElFTkSuQmCC" />
                                                    <input type="color" class="packaging-color-hit" value="#000000"
                                                        onmousedown="packagingSyncColorHit(this);"
                                                        oninput="packagingApplyColorHit(this);"
                                                        onchange="packagingApplyColorHit(this);" />
                                                </span>
                                            </span>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="LabelColour" runat="server" Text='<%# Bind("Colour") %>'></asp:Label>
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <span class="packaging-color-editor">
                                                <asp:TextBox ID="TextBoxColour" runat="server" Text=""
                                                    CssClass="packaging-color-hex" ToolTip="Foreground colour (#RRGGBB)" />
                                                <span class="packaging-color-swatch" title="Pick foreground colour">
                                                    <img class="packaging-color-icon" width="16" height="16" alt=""
                                                        src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAsTAAALEwEAmpwYAAACQ0lEQVR4nHWS72sScRjA75X/ikuLXs5RE4JyK0gmTQtpECk1crqt+SI1vTwnU8LzPJ3NRWKsmh3RDyaeDiyqEcz1Y5u6LRKpVbaI7XTuXRdPnDQ5z3zg8/Lz+T5feBBEMHgPJg0oXETghCsfUGB7CZmMI5+QyYi5rq6DSLtB1aiIUGChqVM3/9CX5uC95QNsOIvw0+uFL3Y7rOh0kJbL2URnZ4iSSEQCmRJ5lL7IQ+1dWHcW4dP45wa/fL4GWx4PvOnvB1p7NEKhvIjj/J3J0JUkLFhXd/iyMMDxFTcyWVQJtFEerMvXtdFD4xfusRnXJrx0l+GVZXm7XYCTKzOjsB0zQebqMZYeOSJFHAPRQGz0dV1uYF1ubCKU98l7z0HK2E0g9oFoIWH/2BzgbfI/maMcGYSUUZ7nArXn/9YXsmBZqW7ixopQ5tiJDQNt6q4hJ/WzNZv/O9gDWy2YyRIzk51gdosOEMJsWCCFd1S5QGHMW2yRx8hS5cazXQiv3YYHSxNVYaD8zgQ0Ls4hvfo4obMtNr8cLDGcvB/guJ9tjhSS/ZDCxT6k5yIlPj346LfV/61F5gf4mzDrFsiEDrM0fkBav4VefTyoNWfqf+bLwkA9knVXluLHgfZ3kI1LVKspkXL4aURPvgXHY6ZtgFydgsuUDSykqvmU6xGUEmmwdFDrfsEaptfg2uwPcDypQDAXAe8iCeakC85Oj7B94SFSTaHNMn/OYLRU45zH1dh8ToOla6qwYa9v0pBThYcI5S2jRCj8BSWLZfvoLu3GAAAAAElFTkSuQmCC" />
                                                    <input type="color" class="packaging-color-hit" value="#000000"
                                                        onmousedown="packagingSyncColorHit(this);"
                                                        oninput="packagingApplyColorHit(this);"
                                                        onchange="packagingApplyColorHit(this);" />
                                                </span>
                                            </span>
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
                                <PagerStyle CssClass="pager-row" />
                                <PagerTemplate>
                                    <asp:PlaceHolder ID="plhPager" runat="server" />
                                </PagerTemplate>
                            </asp:GridView>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </ContentTemplate>
        </ajaxToolkit:TabPanel>
        <ajaxToolkit:TabPanel ID="tabpnlSortOrders" runat="server" HeaderText="Sort Orders">
            <ContentTemplate>
                <asp:UpdatePanel ID="upnlSortOrders" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="gvSortOrders" />
                    </Triggers>
                    <ContentTemplate>
                        <p class="page-tone-subtitle" style="margin:0.6rem 0;">
                            Major item categories. The number is stored on each item as Sort Order (S/O).
                        </p>
                        <div class="results-container scrollable-table-container">
                            <asp:GridView ID="gvSortOrders" runat="server" DataKeyNames="SortOrderID"
                                CssClass="results-table results-table-fit" AutoGenerateColumns="False" ShowFooter="true"
                                OnRowCommand="gvSortOrders_RowCommand"
                                OnRowEditing="gvSortOrders_RowEditing"
                                OnRowCancelingEdit="gvSortOrders_RowCancelingEdit"
                                OnRowUpdating="gvSortOrders_RowUpdating"
                                OnRowDeleting="gvSortOrders_RowDeleting"
                                OnRowDataBound="gvSortOrders_RowDataBound">
                                <Columns>
                                    <asp:TemplateField ShowHeader="False">
                                        <EditItemTemplate>
                                            <asp:ImageButton ID="btnSoUpdate" runat="server" CausesValidation="False" CommandName="Update"
                                                AlternateText="go" ImageUrl="~/images/imgButtons/UpdateItem.gif" />
                                            <asp:ImageButton ID="btnSoCancel" runat="server" CausesValidation="False" CommandName="Cancel"
                                                AlternateText="no" ImageUrl="~/images/imgButtons/CancelItem.gif" />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:ImageButton ID="btnSoEdit" runat="server" CausesValidation="False" CommandName="Edit"
                                                AlternateText="Edit" ImageUrl="~/images/imgButtons/EditItem.gif" />
                                            <asp:ImageButton ID="btnSoDelete" runat="server" CausesValidation="False" CommandName="Delete"
                                                AlternateText="Delete" ImageUrl="~/images/imgButtons/Trashcan.gif"
                                                OnClientClick="return confirm('Delete this sort order?');" />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:ImageButton ID="btnSoAdd" runat="server" CausesValidation="False" CommandName="AddItem"
                                                ImageUrl="~/images/imgButtons/AddItem.gif" AlternateText="Add" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Value" SortExpression="SortValue">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="tbxSortValue" runat="server" Text='<%# Bind("SortValue") %>' Width="4em" />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblSortValue" runat="server" Text='<%# Bind("SortValue") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="tbxSortValueFooter" runat="server" Width="4em" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Description" SortExpression="SortOrderDesc">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="tbxSortDesc" runat="server" Text='<%# Bind("SortOrderDesc") %>' Width="16em" />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblSortDesc" runat="server" Text='<%# Bind("SortOrderDesc") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="tbxSortDescFooter" runat="server" Width="16em" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Enabled" SortExpression="IsEnabled">
                                        <EditItemTemplate>
                                            <asp:CheckBox ID="cbxSortEnabled" runat="server" Checked='<%# Bind("IsEnabled") %>' />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:CheckBox ID="cbxSortEnabled" runat="server" Checked='<%# Bind("IsEnabled") %>' Enabled="false" />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:CheckBox ID="cbxSortEnabledFooter" runat="server" Checked="true" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </ContentTemplate>
        </ajaxToolkit:TabPanel>
        <ajaxToolkit:TabPanel ID="tabpnlRepairStatuses" runat="server" HeaderText="Repair Statuses">
            <ContentTemplate>
                <asp:UpdatePanel ID="upnlRepairStatuses" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="gvRepairStatuses" />
                        <asp:AsyncPostBackTrigger ControlID="btnRepairStatusGo" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="btnRepairStatusReset" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="tbxRepairStatusSearch" EventName="TextChanged" />
                    </Triggers>
                    <ContentTemplate>
                        <asp:Panel ID="pnlRepairStatusSearch" runat="server" DefaultButton="btnRepairStatusGo" CssClass="filter-toolbar lookups-tab-toolbar">
                            <div class="filter-section search-controls">
                                <div class="filter-control">
                                    <asp:Label AssociatedControlID="tbxRepairStatusSearch" runat="server" Text="Search:" />
                                    <asp:TextBox ID="tbxRepairStatusSearch" runat="server"
                                        AutoPostBack="true"
                                        OnTextChanged="tbxRepairStatusSearch_TextChanged" />
                                </div>
                                <asp:Button ID="btnRepairStatusGo" Text="Go" runat="server" CssClass="filter-panel-btn"
                                    CausesValidation="false"
                                    ToolTip="Search repair statuses" OnClick="btnRepairStatusGo_Click" />
                                <asp:Button ID="btnRepairStatusReset" Text="Reset" runat="server" CssClass="filter-panel-btn"
                                    CausesValidation="false"
                                    OnClick="btnRepairStatusReset_Click" />
                            </div>
                            <div class="filter-section admin-controls lookups-tab-icon-wrap">
                                <img class="lookups-tab-icon" src="../images/imgButtons/icons8-list-of-repair-statuses.png"
                                    width="32" height="32" alt="" />
                            </div>
                        </asp:Panel>
                        <div class="results-container scrollable-table-container">
                            <asp:GridView ID="gvRepairStatuses" runat="server" DataKeyNames="RepairStatusID"
                                CssClass="results-table results-table-fit"
                                AutoGenerateColumns="False" AllowPaging="True" AllowSorting="True"
                                ShowFooter="True" PageSize="20"
                                OnPageIndexChanging="gvRepairStatuses_PageIndexChanging"
                                OnSorting="gvRepairStatuses_Sorting"
                                OnRowCommand="gvRepairStatuses_RowCommand"
                                OnRowEditing="gvRepairStatuses_RowEditing"
                                OnRowCancelingEdit="gvRepairStatuses_RowCancelingEdit"
                                OnRowUpdating="gvRepairStatuses_RowUpdating"
                                OnRowDeleting="gvRepairStatuses_RowDeleting"
                                OnRowDataBound="gvRepairStatuses_RowDataBound"
                                OnRowCreated="gvRepairStatuses_RowCreated">
                                <Columns>
                                    <asp:TemplateField ShowHeader="False">
                                        <EditItemTemplate>
                                            <asp:ImageButton ID="btnUpdate" runat="server" CausesValidation="False" CommandName="Update"
                                                ImageUrl="~/images/imgButtons/UpdateItem.gif" AlternateText="Update" />
                                            <asp:ImageButton ID="btnCancel" runat="server" CausesValidation="False" CommandName="Cancel"
                                                ImageUrl="~/images/imgButtons/CancelItem.gif" AlternateText="Cancel" />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:ImageButton ID="btnEdit" runat="server" CausesValidation="False" CommandName="Edit"
                                                ImageUrl="~/images/imgButtons/EditItem.gif" AlternateText="Edit" />
                                            <asp:ImageButton ID="btnDelete" runat="server" CausesValidation="False" CommandName="Delete"
                                                ImageUrl="~/images/imgButtons/Trashcan.gif" AlternateText="Delete"
                                                OnClientClick="return confirm('Delete this repair status?');" />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:ImageButton ID="btnAdd" runat="server" CausesValidation="False" CommandName="Insert"
                                                ImageUrl="~/images/imgButtons/AddItem.gif" AlternateText="Add" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="RepairStatusID" HeaderText="ID" ReadOnly="True" />
                                    <asp:TemplateField HeaderText="Status" SortExpression="RepairStatusDesc">
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
                                    <asp:TemplateField HeaderText="Email Contact" SortExpression="EmailContact">
                                        <EditItemTemplate>
                                            <asp:CheckBox ID="cbxEmailContact" runat="server"
                                                Checked='<%# (Eval("EmailContact") as bool?) == true %>' />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:CheckBox ID="cbxEmailContactView" runat="server"
                                                Checked='<%# (Eval("EmailContact") as bool?) == true %>' Enabled="false" />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:CheckBox ID="cbxEmailContactFooter" runat="server" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Sort Order" SortExpression="SortOrder">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="tbxSortOrder" runat="server" Width="4em"
                                                Text='<%# Eval("SortOrder") %>' />
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblSortOrder" runat="server" Text='<%# Eval("SortOrder") %>' />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="tbxSortOrderFooter" runat="server" Width="4em" Text="0" />
                                        </FooterTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Status Note" SortExpression="StatusNote">
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
                                </Columns>
                                <PagerStyle CssClass="pager-row" />
                                <PagerTemplate>
                                    <asp:PlaceHolder ID="plhPager" runat="server" />
                                </PagerTemplate>
                            </asp:GridView>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </ContentTemplate>
        </ajaxToolkit:TabPanel>
        <ajaxToolkit:TabPanel ID="tabInvoiceTypes" runat="server" HeaderText="InvoiceTypes">
            <ContentTemplate>
                <asp:UpdateProgress runat="server" ID="gvInvoiceTypesUpdateProgress" AssociatedUpdatePanelID="gvInvoiceTypesUpdatePanel" DisplayAfter="0">
                    <ProgressTemplate>
                        <div class="status-message status-info page-tone-progress">
                            <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                            &nbsp;Please wait...
                        </div>
                    </ProgressTemplate>
                </asp:UpdateProgress>
                <asp:UpdatePanel ID="gvInvoiceTypesUpdatePanel" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="gvInvoiceTypes" />
                        <asp:AsyncPostBackTrigger ControlID="btnInvoiceTypeGo" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="btnInvoiceTypeReset" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="tbxInvoiceTypeSearch" EventName="TextChanged" />
                    </Triggers>
                    <ContentTemplate>
                        <div class="filter-toolbar lookups-tab-toolbar">
                            <div class="filter-section search-controls">
                                <div class="filter-control">
                                    <asp:Label AssociatedControlID="tbxInvoiceTypeSearch" runat="server" Text="Search:" />
                                    <asp:TextBox ID="tbxInvoiceTypeSearch" runat="server"
                                        CausesValidation="false"
                                        OnTextChanged="tbxInvoiceTypeSearch_TextChanged" />
                                </div>
                                <asp:Button ID="btnInvoiceTypeGo" Text="Go" runat="server" CssClass="filter-panel-btn"
                                    CausesValidation="false"
                                    ToolTip="Search invoice types" OnClick="btnInvoiceTypeGo_Click" />
                                <asp:Button ID="btnInvoiceTypeReset" Text="Reset" runat="server" CssClass="filter-panel-btn"
                                    CausesValidation="false"
                                    OnClick="btnInvoiceTypeReset_Click" />
                            </div>
                            <div class="filter-section admin-controls lookups-tab-icon-wrap">
                                <img class="lookups-tab-icon" src="../images/imgButtons/icons8-list-of-invoice-types.png"
                                    width="32" height="32" alt="" />
                            </div>
                        </div>
                        <div class="responsive-layout-container scrollable-table-container">
                            <asp:GridView ID="gvInvoiceTypes" runat="server" AllowSorting="True" DataKeyNames="InvoiceTypeID"
                                CssClass="results-table" AutoGenerateColumns="False" ShowFooter="true"
                                OnRowCommand="gvInvoiceTypes_RowCommand"
                                OnPageIndexChanging="gvInvoiceTypes_PageIndexChanging"
                                OnSorting="gvInvoiceTypes_Sorting"
                                OnRowEditing="gvInvoiceTypes_RowEditing"
                                OnRowCancelingEdit="gvInvoiceTypes_RowCancelingEdit"
                                OnRowUpdating="gvInvoiceTypes_RowUpdating"
                                OnRowDeleting="gvInvoiceTypes_RowDeleting"
                                OnRowDataBound="gvInvoiceTypes_RowDataBound">
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
                <asp:UpdateProgress runat="server" ID="PaymentTermsUpdateProgress" AssociatedUpdatePanelID="gvPaymentTermsUpdatePanel" DisplayAfter="0">
                    <ProgressTemplate>
                        <div class="status-message status-info page-tone-progress">
                            <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                            &nbsp;Please wait...
                        </div>
                    </ProgressTemplate>
                </asp:UpdateProgress>
                <asp:UpdatePanel ID="gvPaymentTermsUpdatePanel" runat="server" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="filter-toolbar lookups-tab-toolbar">
                            <div class="filter-section search-controls">
                                <div class="filter-control">
                                    <asp:Label AssociatedControlID="tbxPaymentTermSearch" runat="server" Text="Search:" />
                                    <asp:TextBox ID="tbxPaymentTermSearch" runat="server" OnTextChanged="tbxPaymentTermSearch_TextChanged" />
                                </div>
                                <asp:Button ID="btnPaymentTermGo" Text="Go" runat="server" CssClass="filter-panel-btn"
                                    ToolTip="Search payment terms" OnClick="btnPaymentTermGo_Click" />
                                <asp:Button ID="btnPaymentTermReset" Text="Reset" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnPaymentTermReset_Click" />
                            </div>
                            <div class="filter-section admin-controls lookups-tab-icon-wrap">
                                <img class="lookups-tab-icon" src="../images/imgButtons/icons8-list-of-payment-terms.png"
                                    width="32" height="32" alt="" />
                            </div>
                        </div>
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
                <asp:UpdateProgress runat="server" ID="PriceLevelUpdateProgress" AssociatedUpdatePanelID="gvPriceLevelsUpdatePanel" DisplayAfter="0">
                    <ProgressTemplate>
                        <div class="status-message status-info page-tone-progress">
                            <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                            &nbsp;Please wait...
                        </div>
                    </ProgressTemplate>
                </asp:UpdateProgress>
                <asp:UpdatePanel ID="gvPriceLevelsUpdatePanel" runat="server" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="filter-toolbar lookups-tab-toolbar">
                            <div class="filter-section search-controls">
                                <div class="filter-control">
                                    <asp:Label AssociatedControlID="tbxPriceLevelSearch" runat="server" Text="Search:" />
                                    <asp:TextBox ID="tbxPriceLevelSearch" runat="server" OnTextChanged="tbxPriceLevelSearch_TextChanged" />
                                </div>
                                <asp:Button ID="btnPriceLevelGo" Text="Go" runat="server" CssClass="filter-panel-btn"
                                    ToolTip="Search price levels" OnClick="btnPriceLevelGo_Click" />
                                <asp:Button ID="btnPriceLevelReset" Text="Reset" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnPriceLevelReset_Click" />
                            </div>
                            <div class="filter-section admin-controls lookups-tab-icon-wrap">
                                <img class="lookups-tab-icon" src="../images/imgButtons/icons8-list-of-price-levels.png"
                                    width="32" height="32" alt="" />
                            </div>
                        </div>
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

    </ajaxToolkit:TabContainer>

        <asp:UpdatePanel ID="upnlLookupStatus" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="status-message" style="margin-top: 12px;" id="pnlLookupStatus" runat="server" visible="false">
                    <asp:Label ID="lblStatus" runat="server" />
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>

    <%-- REMOVED: sdsItems SqlDataSource - VIOLATES HARD_PROJECT_RULES.md Rule #2 - Use ItemsRepository in code-behind --%>
    <%-- REMOVED: odsAllItems ObjectDataSource - VIOLATES HARD_PROJECT_RULES.md Rule #2 - Legacy ItemTypeTbl, use ItemsRepository --%>
    
    <asp:SqlDataSource ID="sdsUserNames" runat="server"
        ConnectionString="<%$ ConnectionStrings:TrackerDataSQL %>"
        SelectCommand="SELECT [UserName] AS SecurityUsername FROM [vw_aspnet_Users]" />
    
</asp:Content>
