# ?? IMMEDIATE ACTION REQUIRED - Delete SqlDataSource Controls

**File:** `Pages/Lookups.aspx`  
**Action:** Manually delete the following SqlDataSource controls  
**Why:** They violate the mandatory Repository Pattern rule

---

## ? DELETE THESE CONTROLS NOW

### 1. Delete `sdsItems` (around line 1224-1265)

Search for:
```aspx
<asp:SqlDataSource ID="sdsItems" runat="server"
```

Delete the entire block from `<asp:SqlDataSource ID="sdsItems"` through the closing `</asp:SqlDataSource>` tag.

This includes all SelectParameters, UpdateParameters, DeleteParameters, and InsertParameters.

---

### 2. Delete `odsAllItems` (around line 1266-1272)

Search for:
```aspx
<asp:ObjectDataSource ID="odsAllItems" runat="server" SelectMethod="GetAll" TypeName="TrackerSQL.Controls.ItemTypeTbl"
```

Delete the entire block:
```aspx
<asp:ObjectDataSource ID="odsAllItems" runat="server" SelectMethod="GetAll" TypeName="TrackerSQL.Controls.ItemTypeTbl"
    OldValuesParameterFormatString="original_{0}">
    <SelectParameters>
        <asp:Parameter DefaultValue="ItemDesc" Name="SortBy" Type="String" />
    </SelectParameters>
</asp:ObjectDataSource>
```

---

### 3. Delete `sdsServiceTypes` (around line 1290-1292)

Search for:
```aspx
<asp:SqlDataSource ID="sdsServiceTypes" runat="server"
```

Delete:
```aspx
<asp:SqlDataSource ID="sdsServiceTypes" runat="server" ConnectionString="<%$ ConnectionStrings:TrackerDataSQL %>"
    ProviderName="<%$ ConnectionStrings:TrackerDataSQL.ProviderName %>"
    SelectCommand="SELECT [ServiceTypeId], [ServiceType] FROM [ServiceTypesTbl]"></asp:SqlDataSource>
```

---

### 4. Delete `sdsReplacementItems` (around line 1293-1295)

Search for:
```aspx
<asp:SqlDataSource ID="sdsReplacementItems" runat="server"
```

Delete:
```aspx
<asp:SqlDataSource ID="sdsReplacementItems" runat="server" ConnectionString="<%$ ConnectionStrings:TrackerDataSQL %>"
    ProviderName="<%$ ConnectionStrings:TrackerDataSQL.ProviderName %>"
    SelectCommand="SELECT [ItemID] AS ItemTypeID, [ItemDesc] FROM [ItemsTbl] ORDER BY [ItemDesc]"></asp:SqlDataSource>
```

---

### 5. Delete `sdsCities` (around line 1296-1312)

Search for:
```aspx
<asp:SqlDataSource ID="sdsCities" runat="server" OnSelecting="sdsCities_Selecting"
```

Delete the entire block including all parameters:
```aspx
<asp:SqlDataSource ID="sdsCities" runat="server" OnSelecting="sdsCities_Selecting"
    ConflictDetection="CompareAllValues" ConnectionString="<%$ ConnectionStrings:TrackerDataSQL %>"
    DeleteCommand="DELETE FROM [AreaTbl] WHERE [ID] = @ID" InsertCommand="INSERT INTO AreaTbl(Area) VALUES (@Area)"
    OldValuesParameterFormatString="original_{0}" ProviderName="<%$ ConnectionStrings:TrackerDataSQL.ProviderName %>"
    SelectCommand="SELECT [ID], [Area] FROM [AreaTbl] ORDER BY [Area]"
    UpdateCommand="UPDATE [AreaTbl] SET [Area] = @Area WHERE [ID] = @ID">
    <DeleteParameters>
        <asp:Parameter Name="ID" Type="Int32" />
    </DeleteParameters>
    <InsertParameters>
        <asp:Parameter Name="Area" Type="String" />
    </InsertParameters>
    <UpdateParameters>
        <asp:Parameter Name="Area" Type="String" />
        <asp:Parameter Name="ID" Type="Int32" />
    </UpdateParameters>
</asp:SqlDataSource>
```

---

## ? KEEP THESE (They use Repositories)

DO NOT delete these - they already use the Repository pattern:

- `odsPeople` ? PersonsRepository ?
- `odsEquipTypes` ? EquipTypesRepository ?
- `odsInvoiceTypes` ? InvoiceTypesRepository ?
- `odsPaymentTerms` ? PaymentTermsRepository ?
- `odsPriceLevels` ? PriceLevelsRepository ?
- `odsPackaging` ? ItemPackagingsRepository ?
- `odsRepairStatuses` ? RepairStatusesRepository ?
- `odsAreaDays` ? AreaPrepDaysTbl (will refactor in Phase 2)
- `sdsUserNames` ? ASP.NET membership (different database)

---

## ?? Quick Checklist

After deleting, verify:
- [ ] `sdsItems` - DELETED
- [ ] `odsAllItems` - DELETED
- [ ] `sdsServiceTypes` - DELETED
- [ ] `sdsReplacementItems` - DELETED
- [ ] `sdsCities` - DELETED
- [ ] Build still succeeds
- [ ] No other SqlDataSource controls remain (except `sdsUserNames`)

---

## Next Step

After deleting these controls, I'll update the code-behind (Lookups.aspx.cs) to bind the grids using Repositories.

**Ready to proceed?** Delete these 5 controls now, then let me know and I'll implement the code-behind! ??
