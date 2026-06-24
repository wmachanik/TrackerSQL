# Legacy to Modern Naming Convention Reference

**Created:** 2026-05-15  
**Source:** `Data/Metadata/TableMigrationReport-14-May-26.csv`  
**Purpose:** Quick lookup for table/column name changes

---

## Table Name Mappings (Access ? SQL Server)

### Customer/Client ? Contact

| Legacy (Access) | Modern (SQL Server) | Notes |
|---|---|---|
| `CustomersTbl` | `ContactsTbl` | Main customer table |
| `CustomersAccInfoTbl` | `ContactsAccInfoTbl` | Accounting info |
| `CustomerTypeTbl` | `ContactTypesTbl` | Customer types |
| `CustomersAwayTbl` | `ContactsAwayPeriodTbl` | Away periods |
| `CustomerTrackedServiceItemsTbl` | `ContactTrackedServiceItemsTbl` | Tracked items |
| `ClientUsageTbl` | `ContactsItemsPredictedTbl` | Usage predictions |
| `ClientUsageLinesTbl` | `ContactsUsageTbl` | Usage summary |
| `ClientAwayPeriodTbl` | `ContactsAwayPeriodTbl` | Away periods |

### City ? Area

| Legacy (Access) | Modern (SQL Server) | Notes |
|---|---|---|
| `CityTbl` | `AreasTbl` | Delivery areas |
| `CityPrepDaysTbl` | `AreaPrepDaysTbl` | Prep days per area |
| `NextRoastDateByCityTbl` | `NextPreperationDateByAreasTbl` | Next prep dates |

### Item/Coffee ? Item

| Legacy (Access) | Modern (SQL Server) | Notes |
|---|---|---|
| `ItemTypeTbl` | `ItemsTbl` | Coffee/product items |
| `ItemUsageTbl` | `ContactsItemUsageTbl` | Item usage tracking |
| `PackagingTbl` | `ItemPackagingsTbl` | Packaging types |
| `PrepTypesTbl` | `ItemPrepTypesTbl` | Preparation types |
| `ServiceTypesTbl` | `ItemServiceTypesTbl` | Service types |

### Machine ? Equipment

| Legacy (Access) | Modern (SQL Server) | Notes |
|---|---|---|
| `MachineConditionsTbl` | `EquipConditionsTbl` | Equipment conditions |
| `EquipTypeTbl` | `EquipTypesTbl` | Equipment types |

### Reoccur ? Recurr (Spelling Fix)

| Legacy (Access) | Modern (SQL Server) | Notes |
|---|---|---|
| `ReoccuringOrderTbl` | `RecurringOrdersTbl` | Normalized to header table |
| | `RecurringOrderItemsTbl` | Normalized to line table |
| `ReoccuranceTypeTbl` | `RecurranceTypesTbl` | Recurrence types |

### People/Staff

| Legacy (Access) | Modern (SQL Server) | Notes |
|---|---|---|
| `PersonsTbl` | `PeopleTbl` | Staff/delivery people |

### Other Tables

| Legacy (Access) | Modern (SQL Server) | Notes |
|---|---|---|
| `HolidayClosureTbl` | `HolidayClosuresTbl` | Holiday closures |
| `InvoiceTypeTbl` | `InvoiceTypesTbl` | Invoice types |
| `OrdersTbl` | `OrdersTbl` + `OrderLinesTbl` | Normalized |
| `RepairsTbl` | `RepairsTbl` | No change |
| `SysDataTbl` | `SysDataTbl` | No change |

---

## Column Name Mappings (Common Changes)

### ID Columns

| Legacy | Modern | Pattern |
|---|---|---|
| `CustomerID` | `ContactID` | Customer ? Contact |
| `CityID` | `AreaID` | City ? Area |
| `ItemTypeID` | `ItemID` | ItemType ? Item |
| `MachineID` | `EquipmentID` | Machine ? Equipment |

### Date Columns

| Legacy | Modern | Notes |
|---|---|---|
| `RoastDate` | `PrepDate` | Roasting ? Preparation |
| `NextRoastDate` | `NextPreperationDate` | Roasting ? Preparation |
| `PrepDate` | `PrepDate` | Already migrated |

### Other Columns

| Legacy | Modern | Notes |
|---|---|---|
| `MachineSN` | `EquipmentSN` | Serial number |
| `Abreviation` | `Abbreviation` | Spelling fix |
| `PrepDate` | `PrepDate` | Coffee roasting ? prep |

---

## POCO Class Naming

### Pattern Rules

1. **Remove `Tbl` suffix:**
   - `CustomersTbl` ? `Customers` ? `Contact`

2. **Apply rename mappings:**
   - `Customers` ? `Contact`
   - `City` ? `Area`
   - `ItemType` ? `Item`

3. **Consider singular/plural:**
   - `CustomersTbl` ? `Contact` (singular POCO)
   - `AreasTbl` ? `Area` (singular POCO)
   - `ItemsTbl` ? `Item` (singular POCO)

4. **Repository naming:**
   - POCO: `Contact`
   - Repository: `ContactsRepository` (plural)

---

## Examples of Correct Mappings

### Example 1: CustomersTbl

```
Legacy Class:    Controls\CustomersTbl.cs
POCO:            Classes\Poco\Contact.cs
Repository:      Classes\Sql\ContactsRepository.cs
Table (SQL):     ContactsTbl
Primary Key:     ContactID
```

### Example 2: CityPrepDaysTbl

```
Legacy Class:    Controls\CityPrepDaysTbl.cs
POCO:            Classes\Poco\AreaPrepDays.cs
Repository:      Classes\Sql\AreaPrepDaysRepository.cs
Table (SQL):     AreaPrepDaysTbl
Primary Key:     AreaPrepDaysID
```

### Example 3: ItemTypeTbl

```
Legacy Class:    Controls\ItemTypeTbl.cs
POCO:            Classes\Poco\Item.cs
Repository:      Classes\Sql\ItemsRepository.cs
Table (SQL):     ItemsTbl
Primary Key:     ItemID
```

### Example 4: ReoccuringOrderTbl (Normalized)

```
Legacy Class:    Controls\ReoccuringOrderTbl.cs
POCO:            Classes\Poco\RecurringOrder.cs (header)
                 Classes\Poco\RecurringOrderItem.cs (line)
Repository:      Classes\Sql\RecurringOrdersRepository.cs
Table (SQL):     RecurringOrdersTbl (header)
                 RecurringOrderItemsTbl (lines)
Primary Key:     RecurringOrderID
                 RecurringOrderItemID
```

---

## Common Mistakes to Avoid

### ? Don't Do This

```csharp
// WRONG: Using old names
var customerTbl = new CustomersTbl();
var city = cityTbl.GetCityByID(cityId);
string sql = "SELECT * FROM CustomersTbl WHERE CityID = ?";
```

### ? Do This Instead

```csharp
// RIGHT: Using new names
var repo = new ContactsRepository();
var area = repo.GetAreaById(areaId);
string sql = "SELECT * FROM ContactsTbl WHERE AreaID = @AreaId";
```

---

## Verification Checklist

When migrating code, verify:

- [ ] Table names use modern SQL names (Contact*, Area*, Item*)
- [ ] Column names use modern names (ContactID, AreaID, ItemID)
- [ ] POCO classes use singular names (Contact, Area, Item)
- [ ] Repository classes use plural names (ContactsRepository, AreasRepository)
- [ ] No references to Customer*, City*, ItemType*, Machine*
- [ ] Date columns use Preperation (not Roast)
- [ ] Spelling corrections applied (Abbreviation, Recurring)

---

## Quick Reference Table

| Concept | Legacy Term | Modern Term |
|---|---|---|
| **Customers** | Customer*, Client* | Contact* |
| **Locations** | City* | Area* |
| **Products** | ItemType* | Item* |
| **Equipment** | Machine* | Equipment*, Equip* |
| **Coffee Process** | Roast*, Roasting* | Prep*, Preperation* |
| **Recurring** | Reoccur* | Recurr* |
| **Packaging** | Packaging* | ItemPackaging* |
| **Service** | ServiceType* | ItemServiceType* |
| **Staff** | Persons* | People* |

---

## Using This Reference

### When Analyzing Legacy Classes

```powershell
# Script automatically applies these mappings
.\DevTools\Scripts\Analyze-Legacy-Classes.ps1
```

### When Generating Repositories

```powershell
# Use the MODERN POCO name
.\DevTools\Scripts\Generate-Repository.ps1 -PocoName "Contact"  # NOT "Customer"
.\DevTools\Scripts\Generate-Repository.ps1 -PocoName "Area"     # NOT "City"
.\DevTools\Scripts\Generate-Repository.ps1 -PocoName "Item"     # NOT "ItemType"
```

### When Writing SQL

```csharp
// Use modern table names
string sql = @"
    SELECT c.ContactID, c.CompanyName, a.AreaName
    FROM ContactsTbl c
    INNER JOIN AreasTbl a ON c.AreaID = a.AreaID
    WHERE c.ContactID = @ContactId";
```

---

## Version History

| Version | Date | Changes |
|---|---|---|
| 1.0 | 2026-05-15 | Initial naming convention reference |

---

**For complete table/column mappings, see:**  
`Data/Metadata/TableMigrationReport-14-May-26.csv`
