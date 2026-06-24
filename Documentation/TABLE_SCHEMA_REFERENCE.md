# TrackerSQL Table Schema Reference

## Document Purpose

This document provides a **complete reference** of all tables in the TrackerSQL database migration from Microsoft Access to SQL Server. Use this as the authoritative source for table and column name mappings.

**Master Data Source:** `Migrations\Data\TableMigrationReport-10-Mar-26.xlsx`

---

## Quick Reference: Table Name Mappings

| Access Table Name | SQL Server Table Name | Migration Type | Notes |
|-------------------|----------------------|----------------|-------|
| `CustomersTbl` | `ContactsTbl` | Rename | Main customer table |
| `ClientUsageTbl` | `ContactsItemsPredictedTbl` | Rename | Usage predictions |
| `ClientUsageLinesTbl` | `ContactsUsageTbl` | Rename | Actual usage tracking |
| `ItemUsageTbl` | `ContactsItemUsageTbl` | Rename | Item-level usage |
| `AreaTbl` | `AreasTbl` | Rename | Delivery areas |
| `ItemTypeTbl` | `ItemsTbl` | Rename | Coffee items/products |
| `PackagingTbl` | `ItemPackagingsTbl` | Rename | Packaging types |
| `PrepTypesTbl` | `ItemPrepTypesTbl` | Rename | Preparation methods |
| `ServiceTypesTbl` | `ItemServiceTypesTbl` | Rename | Service types |
| `MachineConditionsTbl` | `EquipConditionsTbl` | Rename | Equipment conditions |
| `PersonsTbl` | `PeopleTbl` | Rename | People |
| `CustomerTypeTbl` | `ContactTypesTbl` | Rename | Contact types |
| `OrdersTbl` | `OrdersTbl` + `OrderLinesTbl` | Normalization | Split into header/lines |
| `SysDataTbl` | `SysDataTbl` | Direct Copy | System configuration |
| `SentRemindersLogTbl` | `SentRemindersLogTbl` | Direct Copy | Email tracking |

*See detailed sections below for complete column-level mappings.*

---

## Complete Table Descriptions

### 1. ContactsTbl (formerly CustomersTbl)

**Purpose:** Master customer/client contact information

**Migration Type:** Rename (Customer ? Contact terminology)

**Relationships:**
- Referenced by: `OrdersTbl`, `ContactsUsageTbl`, `ContactsItemsPredictedTbl`, `ContactsItemUsageTbl`, `SentRemindersLogTbl`
- References: `AreasTbl` (via AreaID), `ContactTypesTbl` (via ContactTypeID), `PeopleTbl` (via PersonID)

#### Column Mappings

| Access Column | SQL Server Column | Type (Access) | Type (SQL Server) | Notes |
|---------------|-------------------|---------------|-------------------|-------|
| `CustomerID` | `ContactID` | AutoNumber | INT IDENTITY(1,1) | Primary key |
| `CustomerName` | `ContactName` | Text(255) | NVARCHAR(255) NOT NULL | Contact name |
| `ContactPerson` | `ContactPerson` | Text(100) | NVARCHAR(100) | Person to contact |
| `AreaID` | `AreaID` | Long Integer | INT | FK to AreasTbl |
| `Address1` | `Address1` | Text(255) | NVARCHAR(255) | Street address |
| `Address2` | `Address2` | Text(255) | NVARCHAR(255) | Additional address |
| `PhoneNumber` | `PhoneNumber` | Text(50) | NVARCHAR(50) | Contact phone |
| `EmailAddress` | `EmailAddress` | Text(255) | NVARCHAR(255) | Email |
| `NextPreperationDate` | `NextPreperationDate` | Date/Time | DATETIME2 | Next preparation date |
| `PrepDate` | `PrepDate` | Date/Time | DATETIME2 | Last preparation date |
| `IsActive` | `IsActive` | Yes/No | BIT NOT NULL DEFAULT 1 | Active status |
| `Notes` | `Notes` | Memo | NVARCHAR(MAX) | Additional notes |
| `CustTypeID` | `ContactTypeID` | Long Integer | INT | FK to ContactTypesTbl |
| `PersonID` | `PersonID` | Long Integer | INT | FK to PeopleTbl (delivery person) |
| `DeliveryDoW` | `DeliveryDoW` | Text(20) | NVARCHAR(20) | Day of week for delivery |
| `MachineSN` | `EquipmentSN` | Text(100) | NVARCHAR(100) | Equipment serial number |
| `NextDeliveryDate` | `NextDeliveryDate` | Date/Time | DATETIME2 | Calculated next delivery |
| `AutoFulFillBagsNotify` | `AutoFulFillBagsNotify` | Yes/No | BIT | Auto-fulfill notification flag |

**Key Changes:**
- `CustomerID` ? `ContactID` (consistent terminology)
- `AreaID` ? `AreaID` (broader concept)
- `PrepDate`, `NextPreperationDate` ? `PrepDate`, `NextPreperationDate` (broader than just roasting)
- `MachineSN` ? `EquipmentSN` (professional terminology)
- `CustTypeID` ? `ContactTypeID` (consistent with Contact terminology)

---

### 2. ContactsItemsPredictedTbl (formerly ClientUsageTbl)

**Purpose:** Stores calculated/predicted usage metrics for contacts (one row per contact)

**Migration Type:** Rename (Client ? Contact, clarify "predicted")

**Relationships:**
- References: `ContactsTbl` (via ContactID)

#### Column Mappings

| Access Column | SQL Server Column | Type (Access) | Type (SQL Server) | Notes |
|---------------|-------------------|---------------|-------------------|-------|
| `CustomerID` | `ContactID` | Long Integer | INT NOT NULL | PK (NOT identity), FK to ContactsTbl |
| `NextPreperationDate` | `NextPreperationDate` | Date/Time | DATETIME2 | Predicted next prep date |
| `PredictedBagsNeeded` | `PredictedBagsNeeded` | Long Integer | INT | Calculated bag quantity |
| `LastCalculated` | `LastCalculated` | Date/Time | DATETIME2 | When prediction was made |
| `LastPrepDate` | `LastPrepDate` | Date/Time | DATETIME2 | Last actual prep date |
| `AverageDailyUsage` | `AverageDailyUsage` | Double | FLOAT | Daily usage rate |
| `Notes` | `Notes` | Memo | NVARCHAR(MAX) | Calculation notes |

**Key Changes:**
- Table name clarifies this is PREDICTED data (not actual usage)
- `CustomerID` ? `ContactID`
- All `Roast` references ? `Prep`

**Important:** `ContactID` is PRIMARY KEY but NOT IDENTITY (it's a foreign key to ContactsTbl)

---

### 3. ContactsUsageTbl (formerly ClientUsageLinesTbl)

**Purpose:** Tracks actual usage events/deliveries for contacts (multiple rows per contact)

**Migration Type:** Rename (Client ? Contact, UsageLines ? Usage)

**Relationships:**
- References: `ContactsTbl` (via ContactID), `PeopleTbl` (via PersonID)

#### Column Mappings

| Access Column | SQL Server Column | Type (Access) | Type (SQL Server) | Notes |
|---------------|-------------------|---------------|-------------------|-------|
| `ClientUsageLineNo` | `ContactUsageLineNo` | AutoNumber | INT IDENTITY(1,1) | Primary key |
| `CustomerID` | `ContactID` | Long Integer | INT NOT NULL | FK to ContactsTbl |
| `PrepDate` | `PrepDate` | Date/Time | DATETIME2 NOT NULL | Actual prep date |
| `BagsDelivered` | `BagsDelivered` | Long Integer | INT | Bags delivered |
| `PersonID` | `PersonID` | Long Integer | INT | FK to PeopleTbl |
| `Notes` | `Notes` | Memo | NVARCHAR(MAX) | Delivery notes |
| `CreatedDate` | `CreatedDate` | Date/Time | DATETIME2 DEFAULT GETDATE() | Record creation |

**Key Changes:**
- `ClientUsageLineNo` ? `ContactUsageLineNo` (consistent terminology)
- `CustomerID` ? `ContactID`
- `PrepDate` ? `PrepDate`

**Important:** This is the ACTUAL usage log (vs ContactsItemsPredictedTbl which is predictions)

---

### 4. ContactsItemUsageTbl (formerly ItemUsageTbl)

**Purpose:** Detailed item-level usage tracking (what items were used/delivered)

**Migration Type:** Rename (ItemUsage ? ContactsItemUsage for clarity)

**Relationships:**
- References: `ContactsTbl` (via ContactID), `ItemsTbl` (via ItemID), `ContactsUsageTbl` (via ContactUsageLineNo)

#### Column Mappings

| Access Column | SQL Server Column | Type (Access) | Type (SQL Server) | Notes |
|---------------|-------------------|---------------|-------------------|-------|
| `ItemUsageID` | `ContactItemUsageID` | AutoNumber | INT IDENTITY(1,1) | Primary key |
| `ContactUsageLineNo` | `ContactUsageLineNo` | Long Integer | INT NOT NULL | FK to ContactsUsageTbl |
| `CustomerID` | `ContactID` | Long Integer | INT NOT NULL | FK to ContactsTbl |
| `ItemTypeID` | `ItemID` | Long Integer | INT NOT NULL | FK to ItemsTbl |
| `Quantity` | `Quantity` | Long Integer | INT | Quantity used |
| `PrepDate` | `PrepDate` | Date/Time | DATETIME2 | Prep date for this item |
| `Notes` | `Notes` | Memo | NVARCHAR(MAX) | Item notes |

**Key Changes:**
- `ItemUsageID` ? `ContactItemUsageID` (clearer naming)
- `CustomerID` ? `ContactID`
- `ItemTypeID` ? `ItemID` (consistent with ItemsTbl rename)
- `PrepDate` ? `PrepDate`

---

### 5. AreasTbl (formerly AreaTbl)

**Purpose:** Delivery areas/locations

**Migration Type:** Rename (Area ? Area for broader concept)

**Relationships:**
- Referenced by: `ContactsTbl` (via AreaID)

#### Column Mappings

| Access Column | SQL Server Column | Type (Access) | Type (SQL Server) | Notes |
|---------------|-------------------|---------------|-------------------|-------|
| `AreaID` | `AreaID` | AutoNumber | INT IDENTITY(1,1) | Primary key |
| `AreaName` | `AreaName` | Text(100) | NVARCHAR(100) NOT NULL | Area name |
| `Province` | `Province` | Text(50) | NVARCHAR(50) | Province/state |
| `PostalCode` | `PostalCode` | Text(20) | NVARCHAR(20) | Postal/ZIP code |
| `DeliveryNotes` | `DeliveryNotes` | Memo | NVARCHAR(MAX) | Area-specific notes |
| `IsActive` | `IsActive` | Yes/No | BIT DEFAULT 1 | Active status |

**Key Changes:**
- `AreaID` ? `AreaID` (broader geographic concept)
- `AreaName` ? `AreaName`

**Reason:** "Area" is more flexible than "Area" (could be neighborhood, district, region, etc.)

---

### 6. ItemsTbl (formerly ItemTypeTbl)

**Purpose:** Coffee items, products, and SKUs

**Migration Type:** Rename (ItemType ? Item for simpliArea)

**Relationships:**
- Referenced by: `OrderLinesTbl`, `ContactsItemUsageTbl`, `ItemPackagingsTbl`
- References: `ItemPrepTypesTbl` (via ItemPrepID)

#### Column Mappings

| Access Column | SQL Server Column | Type (Access) | Type (SQL Server) | Notes |
|---------------|-------------------|---------------|-------------------|-------|
| `ItemTypeID` | `ItemID` | AutoNumber | INT IDENTITY(1,1) | Primary key |
| `ItemTypeName` | `ItemName` | Text(255) | NVARCHAR(255) NOT NULL | Item name |
| `ItemTypeDescription` | `ItemDescription` | Memo | NVARCHAR(MAX) | Detailed description |
| `PrepID` | `ItemPrepID` | Long Integer | INT | FK to ItemPrepTypesTbl |
| `PackagingID` | `ItemPackagingID` | Long Integer | INT | FK to ItemPackagingsTbl |
| `UnitPrice` | `UnitPrice` | Currency | DECIMAL(19,4) | Price per unit |
| `IsActive` | `IsActive` | Yes/No | BIT DEFAULT 1 | Active status |
| `ReorderLevel` | `ReorderLevel` | Long Integer | INT | Inventory reorder point |
| `QuantityOnHand` | `QuantityOnHand` | Long Integer | INT | Current inventory |
| `Notes` | `Notes` | Memo | NVARCHAR(MAX) | Item notes |

**Key Changes:**
- `ItemTypeID` ? `ItemID` (simpler naming)
- `ItemTypeName` ? `ItemName`
- `ItemTypeDescription` ? `ItemDescription`
- `PrepID` ? `ItemPrepID` (consistency)
- `PackagingID` ? `ItemPackagingID` (consistency)

---

### 7. ItemPackagingsTbl (formerly PackagingTbl)

**Purpose:** Packaging types and options

**Migration Type:** Rename (Packaging ? ItemPackaging for relationship clarity)

**Relationships:**
- Referenced by: `ItemsTbl` (via ItemPackagingID)

#### Column Mappings

| Access Column | SQL Server Column | Type (Access) | Type (SQL Server) | Notes |
|---------------|-------------------|---------------|-------------------|-------|
| `PackagingID` | `ItemPackagingID` | AutoNumber | INT IDENTITY(1,1) | Primary key |
| `Description` | `ItemPackagingDesc` | Text(255) | NVARCHAR(255) NOT NULL | Packaging description |
| `AdditionalNotes` | `AdditionalNotes` | Memo | NVARCHAR(MAX) | Notes |
| `Symbol` | `Symbol` | Text(10) | NVARCHAR(10) | Short code/symbol |
| `Colour` | `Colour` | Text(50) | NVARCHAR(50) | Display color |
| `BGColour` | `BGColour` | Text(50) | NVARCHAR(50) | Background color |

**Key Changes:**
- `PackagingID` ? `ItemPackagingID` (shows relationship to Items)
- `Description` ? `ItemPackagingDesc` (avoid generic name)

---

### 8. ItemPrepTypesTbl (formerly PrepTypesTbl)

**Purpose:** Coffee preparation/roasting methods

**Migration Type:** Rename (PrepTypes ? ItemPrepTypes for relationship clarity)

**Relationships:**
- Referenced by: `ItemsTbl` (via ItemPrepID)

#### Column Mappings

| Access Column | SQL Server Column | Type (Access) | Type (SQL Server) | Notes |
|---------------|-------------------|---------------|-------------------|-------|
| `PrepID` | `ItemPrepID` | AutoNumber | INT IDENTITY(1,1) | Primary key |
| `PrepType` | `ItemPrepType` | Text(100) | NVARCHAR(100) NOT NULL | Prep method name |
| `IdentifyingChar` | `IdentifyingChar` | Text(1) | NVARCHAR(1) | Short code |
| `PrepDescription` | `PrepDescription` | Memo | NVARCHAR(MAX) | Detailed description |

**Key Changes:**
- `PrepID` ? `ItemPrepID` (relationship to Items)
- `PrepType` ? `ItemPrepType`

---

### 9. ItemServiceTypesTbl (formerly ServiceTypesTbl)

**Purpose:** Service/support types for items

**Migration Type:** Rename (ServiceTypes ? ItemServiceTypes)

**Relationships:**
- Referenced by: Various tables tracking service events

#### Column Mappings

| Access Column | SQL Server Column | Type (Access) | Type (SQL Server) | Notes |
|---------------|-------------------|---------------|-------------------|-------|
| `ServiceTypeID` | `ItemServiceTypeID` | AutoNumber | INT IDENTITY(1,1) | Primary key |
| `ServiceTypeName` | `ItemServiceTypeName` | Text(100) | NVARCHAR(100) NOT NULL | Service type |
| `Description` | `ServiceDescription` | Memo | NVARCHAR(MAX) | Description |
| `IsActive` | `IsActive` | Yes/No | BIT DEFAULT 1 | Active status |

**Key Changes:**
- `ServiceTypeID` ? `ItemServiceTypeID`
- `ServiceTypeName` ? `ItemServiceTypeName`

---

### 10. EquipConditionsTbl (formerly MachineConditionsTbl)

**Purpose:** Equipment status and condition tracking

**Migration Type:** Rename (Machine ? Equipment/Equip terminology)

**Relationships:**
- References: `ContactsTbl` (via ContactID - equipment installation location)

#### Column Mappings

| Access Column | SQL Server Column | Type (Access) | Type (SQL Server) | Notes |
|---------------|-------------------|---------------|-------------------|-------|
| `MachineConditionID` | `EquipConditionID` | AutoNumber | INT IDENTITY(1,1) | Primary key |
| `MachineSN` | `EquipmentSN` | Text(100) | NVARCHAR(100) NOT NULL | Serial number |
| `CustomerID` | `ContactID` | Long Integer | INT | FK to ContactsTbl (location) |
| `Condition` | `Condition` | Text(50) | NVARCHAR(50) | Condition status |
| `InspectionDate` | `InspectionDate` | Date/Time | DATETIME2 | Last inspection |
| `NextServiceDate` | `NextServiceDate` | Date/Time | DATETIME2 | Scheduled service |
| `Notes` | `Notes` | Memo | NVARCHAR(MAX) | Condition notes |
| `IsActive` | `IsActive` | Yes/No | BIT DEFAULT 1 | Active equipment |

**Key Changes:**
- `MachineConditionID` ? `EquipConditionID` (professional terminology)
- `MachineSN` ? `EquipmentSN`
- `CustomerID` ? `ContactID`

---

### 11. PeopleTbl (formerly PersonsTbl)

**Purpose:** Staff members and delivery personnel

**Migration Type:** Rename (Persons ? People for better grammar)

**Relationships:**
- Referenced by: `ContactsTbl` (delivery person), `ContactsUsageTbl`, `OrdersTbl`

#### Column Mappings

| Access Column | SQL Server Column | Type (Access) | Type (SQL Server) | Notes |
|---------------|-------------------|---------------|-------------------|-------|
| `PersonID` | `PersonID` | AutoNumber | INT IDENTITY(1,1) | Primary key |
| `Person` | `Person` | Text(100) | NVARCHAR(100) NOT NULL | Person name |
| `Abreviation` | `Abbreviation` | Text(10) | NVARCHAR(10) | Short code (SPELLING FIXED) |
| `Enabled` | `Enabled` | Yes/No | BIT DEFAULT 1 | Active status |
| `NormalDeliveryDoW` | `NormalDeliveryDoW` | Text(20) | NVARCHAR(20) | Default delivery day |
| `SecurityUsername` | `SecurityUsername` | Text(50) | NVARCHAR(50) | Login username |
| `Email` | `Email` | Text(255) | NVARCHAR(255) | Email address |

**Key Changes:**
- Table name: `PersonsTbl` ? `PeopleTbl` (grammatically correct plural)
- `Abreviation` ? `Abbreviation` (spelling correction)

---

### 12. ContactTypesTbl (formerly CustomerTypeTbl)

**Purpose:** Categories/types of contacts

**Migration Type:** Rename (CustomerType ? ContactType)

**Relationships:**
- Referenced by: `ContactsTbl` (via ContactTypeID)

#### Column Mappings

| Access Column | SQL Server Column | Type (Access) | Type (SQL Server) | Notes |
|---------------|-------------------|---------------|-------------------|-------|
| `CustTypeID` | `ContactTypeID` | AutoNumber | INT IDENTITY(1,1) | Primary key |
| `CustTypeDesc` | `ContactTypeDesc` | Text(100) | NVARCHAR(100) NOT NULL | Type description |
| `Notes` | `Notes` | Memo | NVARCHAR(MAX) | Additional notes |

**Key Changes:**
- `CustTypeID` ? `ContactTypeID`
- `CustTypeDesc` ? `ContactTypeDesc` (kept "Desc" pattern)

---

### 13. OrdersTbl and OrderLinesTbl (Normalized from OrdersTbl)

**Purpose:** Order management (header and line items)

**Migration Type:** Normalization (single table split into header + lines)

#### OrdersTbl (Header)

| Access Column | SQL Server Column | Type (Access) | Type (SQL Server) | Notes |
|---------------|-------------------|---------------|-------------------|-------|
| `OrderID` | `OrderID` | AutoNumber | INT IDENTITY(1,1) | Primary key |
| `CustomerID` | `ContactID` | Long Integer | INT NOT NULL | FK to ContactsTbl |
| `OrderDate` | `OrderDate` | Date/Time | DATETIME2 NOT NULL | Order date |
| `RequiredDate` | `RequiredDate` | Date/Time | DATETIME2 | Requested delivery |
| `ShippedDate` | `ShippedDate` | Date/Time | DATETIME2 | Actual delivery |
| `PersonID` | `PersonID` | Long Integer | INT | FK to PeopleTbl (sales person) |
| `OrderStatus` | `OrderStatus` | Text(50) | NVARCHAR(50) | Status |
| `TotalAmount` | `TotalAmount` | Currency | DECIMAL(19,4) | Order total |
| `Notes` | `Notes` | Memo | NVARCHAR(MAX) | Order notes |

#### OrderLinesTbl (Line Items) - NEW TABLE

| Column Name | Type | Constraints | Notes |
|-------------|------|-------------|-------|
| `OrderLineID` | INT IDENTITY(1,1) | PRIMARY KEY | Line item ID |
| `OrderID` | INT | NOT NULL, FK to OrdersTbl | Parent order |
| `ItemID` | INT | NOT NULL, FK to ItemsTbl | Product ordered |
| `Quantity` | INT | NOT NULL | Quantity |
| `UnitPrice` | DECIMAL(19,4) | NOT NULL | Price per unit |
| `LineTotal` | DECIMAL(19,4) | NOT NULL | Quantity * UnitPrice |
| `Notes` | NVARCHAR(MAX) | | Line-specific notes |

**Key Changes:**
- `CustomerID` ? `ContactID`
- Order lines extracted to separate table (eliminates ItemTypeID1, ItemTypeID2, etc.)
- Supports unlimited line items per order
- Better relational structure

**Old Access Structure (DEPRECATED):**
```
OrdersTbl:
  - ItemTypeID1, Quantity1, Price1
  - ItemTypeID2, Quantity2, Price2
  - ItemTypeID3, Quantity3, Price3
  ... (limited slots, many NULLs)
```

**New SQL Server Structure:**
```
OrdersTbl (1) ----< OrderLinesTbl (many)
```

---

### 14. SysDataTbl

**Purpose:** System configuration and settings

**Migration Type:** Direct Copy (minimal changes)

**Relationships:** None (singleton configuration table)

#### Column Mappings

| Access Column | SQL Server Column | Type (Access) | Type (SQL Server) | Notes |
|---------------|-------------------|---------------|-------------------|-------|
| `SysDataID` | `SysDataID` | AutoNumber | INT IDENTITY(1,1) | Primary key |
| `CompanyName` | `CompanyName` | Text(255) | NVARCHAR(255) | Company name |
| `Address` | `Address` | Text(255) | NVARCHAR(255) | Company address |
| `Phone` | `Phone` | Text(50) | NVARCHAR(50) | Phone number |
| `Email` | `Email` | Text(255) | NVARCHAR(255) | Company email |
| `LastBackupDate` | `LastBackupDate` | Date/Time | DATETIME2 | Last DB backup |
| `MinReminderDate` | `MinReminderDate` | Date/Time | DATETIME2 | Earliest reminder date |
| `DefaultPrepDays` | `DefaultPrepDays` | Long Integer | INT | Default prep lead time |
| `EmailServerSettings` | `EmailServerSettings` | Memo | NVARCHAR(MAX) | SMTP config (JSON) |
| `ApplicationVersion` | `ApplicationVersion` | Text(50) | NVARCHAR(50) | Version number |

**Key Changes:**
- Minimal changes (mostly data type conversions)
- No renamed columns
- Configuration preserved as-is

**Usage:**
```csharp
SysDataTbl sysData = new SysDataTbl();
DateTime minDate = sysData.GetMinReminderDate();
string company = sysData.CompanyName;
```

---

### 15. SentRemindersLogTbl

**Purpose:** Tracks sent email reminders/checkups

**Migration Type:** Direct Copy

**Relationships:**
- References: `ContactsTbl` (via ContactID - though column was CustomerID in Access)

#### Column Mappings

| Access Column | SQL Server Column | Type (Access) | Type (SQL Server) | Notes |
|---------------|-------------------|---------------|-------------------|-------|
| `ReminderID` | `ReminderID` | AutoNumber | INT IDENTITY(1,1) | Primary key |
| `CustomerID` | `CustomerID` | Long Integer | INT NOT NULL | FK to ContactsTbl (NOTE: not renamed to ContactID yet) |
| `DateSentReminder` | `DateSentReminder` | Date/Time | DATETIME2 NOT NULL | When reminder sent |
| `NextPreperationDate` | `NextPreperationDate` | Date/Time | DATETIME2 | Predicted next prep |
| `ReminderSent` | `ReminderSent` | Yes/No | BIT NOT NULL DEFAULT 0 | Send status |
| `HadAutoFulfilItem` | `HadAutoFulfilItem` | Yes/No | BIT DEFAULT 0 | Auto-fulfill flag |
| `HadReoccurItems` | `HadReoccurItems` | Yes/No | BIT DEFAULT 0 | Recurring items flag |

**Key Changes:**
- `NextPreperationDate` ? `NextPreperationDate` (if it existed)
- ?? **NOTE:** `CustomerID` column NOT YET RENAMED to `ContactID` in this table

**Important:**
This table uses `CustomerID` column name even though it references `ContactsTbl.ContactID`. This should be updated in future migration phase for consistency.

**Methods Available:**
```csharp
GetAll(string sortBy)
GetAllByDate(DateTime dateSent, string sortBy)
GetLast20DatesReminderSent()
DeleteTodaysEntries(DateTime targetDate)
GetLastSuccessfulCheckupDate()
GetEntriesCountForDate(DateTime targetDate)
```

---

## Column Naming Patterns

### Primary Keys

**Pattern:** `[TableName without Tbl]ID`

Examples:
- `ContactsTbl.ContactID`
- `ItemsTbl.ItemID`
- `OrdersTbl.OrderID`
- `AreasTbl.AreaID`

**Type:** `INT IDENTITY(1,1)` (auto-increment)

**Exception:** `ContactsItemsPredictedTbl.ContactID` is PK but NOT IDENTITY (it's a FK)

### Foreign Keys

**Pattern:** Same name as referenced PK

Examples:
- `OrdersTbl.ContactID` references `ContactsTbl.ContactID`
- `ItemsTbl.ItemPrepID` references `ItemPrepTypesTbl.ItemPrepID`
- `ContactsTbl.AreaID` references `AreasTbl.AreaID`

**Type:** Matches referenced column type (usually `INT`)

### Date Columns

**Naming:**
- `PrepDate` - Actual preparation date
- `NextPreperationDate` - Scheduled/predicted next prep
- `OrderDate` - Date order placed
- `RequiredDate` - Date customer needs it
- `ShippedDate` - Date actually shipped
- `InspectionDate` - Date inspected
- `NextServiceDate` - Next scheduled service
- `DateSentReminder` - Date reminder email sent

**Type:** `DATETIME2` (preferred) or `DATETIME`

**C# Usage:** Always use `.Date` property when comparing dates without time:
```csharp
DateTime compareDate = myDate.Date;
trackerDb.AddWhereParams((object)compareDate, DbType.Date);
```

### Boolean Columns

**Naming Patterns:**
- `Is[Adjective]` - Status flags (e.g., `IsActive`, `IsDeleted`)
- `Has[Noun]` - Possession flags (e.g., `HadAutoFulfilItem`, `HadReoccurItems`)
- `[Adjective]` - Simple flags (e.g., `Enabled`, `ReminderSent`)

**Type:** `BIT`

**Defaults:** Usually `DEFAULT 0` or `DEFAULT 1` depending on meaning

**C# Reading:**
```csharp
bool isActive = dataReader["IsActive"] != DBNull.Value 
    && Convert.ToBoolean(dataReader["IsActive"]);
```

### Text Columns

**Types:**
- `NVARCHAR(50)` - Short codes, names
- `NVARCHAR(100)` - Medium names
- `NVARCHAR(255)` - Long names, descriptions
- `NVARCHAR(MAX)` - Long text, notes, memos

**Always use NVARCHAR (Unicode) never VARCHAR** to support international characters

### Numeric Columns

**Types:**
- `INT` - Most integer values
- `BIGINT` - Large counts
- `SMALLINT` - Small ranges
- `TINYINT` - Very small ranges (0-255)
- `DECIMAL(19,4)` - Money values
- `FLOAT` - Scientific calculations

---

## Data Type Quick Reference

### Access ? SQL Server Mappings

| Access Type | SQL Server Equivalent | Example Usage |
|-------------|----------------------|---------------|
| **AutoNumber** | `INT IDENTITY(1,1)` | Primary keys |
| **Text(50)** | `NVARCHAR(50)` | Short text |
| **Text(255)** | `NVARCHAR(255)` | Standard text |
| **Memo** | `NVARCHAR(MAX)` | Long text, notes |
| **Number (Long Integer)** | `INT` | Foreign keys, counts |
| **Number (Integer)** | `SMALLINT` | Small integers |
| **Number (Byte)** | `TINYINT` | 0-255 values |
| **Number (Double)** | `FLOAT` | Decimals |
| **Number (Decimal)** | `DECIMAL(p,s)` | Exact decimals |
| **Currency** | `DECIMAL(19,4)` | Money |
| **Date/Time** | `DATETIME2` or `DATETIME` | Dates |
| **Yes/No** | `BIT` | Booleans |
| **OLE Object** | `VARBINARY(MAX)` | Binary data |
| **Hyperlink** | `NVARCHAR(255)` | URLs |

---

## Foreign Key Relationships

### Major Relationship Chains

**Contact-Centric Relationships:**
```
ContactsTbl (1)
  ??? OrdersTbl (many)
  ?   ??? OrderLinesTbl (many)
  ??? ContactsUsageTbl (many)
  ?   ??? ContactsItemUsageTbl (many)
  ??? ContactsItemsPredictedTbl (1-to-1)
  ??? SentRemindersLogTbl (many)
```

**Item-Centric Relationships:**
```
ItemsTbl (1)
  ??? ItemPrepTypesTbl (many-to-1) - prep method
  ??? ItemPackagingsTbl (many-to-1) - packaging type
  ??? OrderLinesTbl (many) - ordered items
  ??? ContactsItemUsageTbl (many) - item usage
```

**Area/Location Relationships:**
```
AreasTbl (1)
  ??? ContactsTbl (many) - contacts in area
```

**People Relationships:**
```
PeopleTbl (1)
  ??? ContactsTbl (many) - assigned delivery person
  ??? OrdersTbl (many) - sales person
  ??? ContactsUsageTbl (many) - person who delivered
```

---

## Verification Queries

### Check Table Names
```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
```

### Check Column Names for Specific Table
```sql
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ContactsTbl'
ORDER BY ORDINAL_POSITION;
```

### Verify Foreign Keys
```sql
SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ReferencedColumn
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fkc 
    ON fk.object_id = fkc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) = 'ContactsTbl'
ORDER BY fk.name;
```

### Check Identity Columns
```sql
SELECT 
    OBJECT_NAME(object_id) AS TableName,
    name AS ColumnName,
    seed_value,
    increment_value,
    last_value
FROM sys.identity_columns
ORDER BY TableName;
```

---

## Common Queries by Table

### ContactsTbl
```sql
-- Get all active contacts
SELECT ContactID, ContactName, AreaID, NextPreperationDate
FROM ContactsTbl
WHERE IsActive = 1
ORDER BY ContactName;

-- Get contacts due for prep
SELECT ContactID, ContactName, NextPreperationDate
FROM ContactsTbl
WHERE IsActive = 1 
  AND NextPreperationDate <= GETDATE()
ORDER BY NextPreperationDate;

-- Get contacts by area
SELECT c.ContactID, c.ContactName, a.AreaName
FROM ContactsTbl c
INNER JOIN AreasTbl a ON c.AreaID = a.AreaID
WHERE c.IsActive = 1
ORDER BY a.AreaName, c.ContactName;
```

### ItemsTbl
```sql
-- Get all active items with pricing
SELECT ItemID, ItemName, UnitPrice, QuantityOnHand
FROM ItemsTbl
WHERE IsActive = 1
ORDER BY ItemName;

-- Get items needing reorder
SELECT ItemID, ItemName, QuantityOnHand, ReorderLevel
FROM ItemsTbl
WHERE IsActive = 1 
  AND QuantityOnHand <= ReorderLevel
ORDER BY QuantityOnHand;
```

### OrdersTbl with Lines
```sql
-- Get order with all line items
SELECT 
    o.OrderID,
    o.OrderDate,
    c.ContactName,
    i.ItemName,
    ol.Quantity,
    ol.UnitPrice,
    ol.LineTotal
FROM OrdersTbl o
INNER JOIN ContactsTbl c ON o.ContactID = c.ContactID
INNER JOIN OrderLinesTbl ol ON o.OrderID = ol.OrderID
INNER JOIN ItemsTbl i ON ol.ItemID = i.ItemID
WHERE o.OrderID = @OrderID
ORDER BY ol.OrderLineID;
```

### SentRemindersLogTbl
```sql
-- Get recent reminders
SELECT TOP 100
    r.ReminderID,
    r.DateSentReminder,
    c.ContactName,
    r.NextPreperationDate,
    r.ReminderSent
FROM SentRemindersLogTbl r
INNER JOIN ContactsTbl c ON r.CustomerID = c.ContactID
WHERE r.ReminderSent = 1
ORDER BY r.DateSentReminder DESC;

-- Get last 20 dates reminders were sent
SELECT DISTINCT TOP 20 DateSentReminder
FROM SentRemindersLogTbl
ORDER BY DateSentReminder DESC;
```

---

## Notes for AI Assistants

### When Writing Code

1. **Always check this document** for correct table and column names
2. **Use SQL Server names** in all new code (Contact*, not Customer*)
3. **Reference the Excel file** if this doc doesn't have details you need
4. **Preserve existing patterns** when modifying code

### When Uncertain About Names

**Check order:**
1. This document (`TABLE_SCHEMA_REFERENCE.md`)
2. Excel file (`Migrations\Data\TableMigrationReport-10-Mar-26.xlsx`)
3. Generated SQL scripts (`Data\Metadata\PlanEdits\Sql\CreateTables_LATEST.sql`)
4. Ask the user

### Common Mistakes to Avoid

? Using `CustomerID` in new code ? ? Use `ContactID`  
? Using `ItemTypeID` ? ? Use `ItemID`  
? Using `AreaID` ? ? Use `AreaID`  
? Using `PrepDate` ? ? Use `PrepDate`  
? Using `MachineSN` ? ? Use `EquipmentSN`  

**Exception:** `SentRemindersLogTbl` still uses `CustomerID` column (legacy naming, to be fixed)

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-03-26 | Initial comprehensive table schema reference |

---

**Primary Reference:** `Migrations\Data\TableMigrationReport-10-Mar-26.xlsx`  
**Last Schema Update:** March 26, 2025  
**Total Tables:** ~45  
**Migration Status:** Schema Complete, Code Migration In Progress
