# TrackerSQL Code Structure and Organization

## Purpose

This document explains the organization and structure of the TrackerSQL codebase. Use this to understand where different types of code live and how they interact.

---

## Table of Contents

1. [Solution Overview](#solution-overview)
2. [Project Structure](#project-structure)
3. [Namespace Organization](#namespace-organization)
4. [Key Patterns and Conventions](#key-patterns-and-conventions)
5. [Data Access Architecture](#data-access-architecture)
6. [Web Pages Structure](#web-pages-structure)
7. [Email System](#email-system)
8. [Migration Tools](#migration-tools)

---

## Solution Overview

The TrackerSQL solution contains **two projects**:

### 1. TrackerSQL (Main Web Application)

**Type:** ASP.NET Web Forms Application  
**Framework:** .NET Framework 4.8  
**Platform:** Any CPU  
**Project File:** `TrackerSQL.csproj`

**Purpose:**  
Production web application for managing coffee roasting business operations.

**Key Technologies:**
- ASP.NET Web Forms 4.8
- AjaxControlToolkit
- OleDb (migrating to SqlClient)
- Bootstrap (for styling)
- jQuery

### 2. MigrationRunner (Database Migration Utility)

**Type:** Console Application  
**Framework:** .NET Framework 4.8  
**Language:** C# 7.3 (explicitly set)  
**Platform:** x64  
**Project File:** `Migrations\MigrationRunner\MigrationRunner.csproj`

**Purpose:**  
Utility to migrate database from Access to SQL Server.

**Key Technologies:**
- .NET Core style SDK project (but targeting .NET Framework 4.8)
- OleDb (for reading Access)
- File I/O for generating SQL scripts
- JSON serialization (Newtonsoft.Json)

---

## Project Structure

### TrackerSQL Web Application

```
TrackerSQL\
?
??? Properties\
?   ??? AssemblyInfo.cs          # Assembly metadata
?
??? Pages\                        # ASP.NET Web Forms (.aspx + .aspx.cs)
?   ??? Default.aspx              # Homepage
?   ??? Customers\                # Customer management pages
?   ??? Orders\                   # Order management pages
?   ??? Reports\                  # Reporting pages
?   ??? Admin\                    # Administrative pages
?
??? Controls\                     # Data access classes (table classes)
?   ??? ContactsTbl.cs            # Contact (customer) data access
?   ??? ItemsTbl.cs               # Item data access
?   ??? OrdersTbl.cs              # Order data access
?   ??? SentRemindersLogTbl.cs    # Reminder tracking
?   ??? [~45 more *Tbl.cs files]  # One per database table
?
??? Classes\                      # Business logic and utilities
?   ??? TrackerDb.cs              # Database abstraction layer
?   ??? SystemConstants.cs        # Application constants
?   ??? TimeZoneUtils.cs          # Time zone handling
?   ??? AppLogger.cs              # Logging utility
?   ??? [other utilities]
?
??? DataSets\                     # Data models and business objects
?   ??? CustomersCls.cs           # Customer business object
?   ??? [other models]
?
??? Emails\                       # Email generation and sending
?   ??? EmailHelper.cs            # Email utilities
?   ??? CheckupEmailGenerator.cs  # Generates checkup emails
?   ??? Templates\                # Email templates
?
??? Tools\                        # Utility web pages
?   ??? [various utility pages]
?
??? Migrations\                   # Database migration resources
?   ??? MigrationRunner\          # Migration console app
?   ??? Data\                     # Migration metadata
?   ??? Scripts\                  # SQL helper scripts
?   ??? README files
?
??? Documentation\                # AI-focused documentation
?   ??? PROJECT_OVERVIEW.md       # Project overview
?   ??? AI_CONTEXT.md             # AI assistant guide
?   ??? MIGRATION_GUIDE.md        # Migration procedures
?   ??? TABLE_SCHEMA_REFERENCE.md # Complete schema reference
?   ??? CODE_STRUCTURE.md         # This file
?
??? Docs\                         # Original documentation
?   ??? MigrationPlaybook_TrackerDotNet_to_TrackerSQL.md
?
??? Data\                         # Metadata and configuration
?   ??? Metadata\                 # Migration metadata files
?
??? Styles\                       # CSS files
??? Scripts\                      # JavaScript files
??? Images\                       # Image assets
?
??? Web.config                    # Web application configuration
??? Global.asax                   # Application events
??? TrackerSQL.csproj             # Project file
```

### MigrationRunner Structure

```
Migrations\MigrationRunner\
?
??? Program.cs                    # Entry point, interactive menu
??? AccessSchemaReader.cs         # Reads Access database schema
??? DdlScriptGenerator.cs         # Generates CREATE TABLE scripts
??? DmlScriptGenerator.cs         # Generates INSERT scripts
??? FkScriptGenerator.cs          # Generates foreign key scripts
??? MetadataManager.cs            # Manages metadata files
??? Models\                       # Data models for migration
?   ??? TableDefinition.cs
?   ??? ColumnDefinition.cs
?   ??? [other models]
??? Metadata\                     # Configuration files
?   ??? BulkRenameRules.json
?   ??? [other configs]
??? MigrationRunner.csproj        # Project file
```

---

## Namespace Organization

### TrackerSQL Web Application Namespaces

#### `TrackerSQL.Controls`

**Purpose:** Data access layer (table classes)

**Pattern:** One class per database table

**Example:**
```csharp
namespace TrackerSQL.Controls
{
    public class ContactsTbl
    {
        // Properties matching database columns
        public int ContactID { get; set; }
        public string ContactName { get; set; }
        
        // Methods for CRUD operations
        public List<ContactsTbl> GetAll(string sortBy);
        public string InsertContact(ContactsTbl contact);
        public string UpdateContact(ContactsTbl contact);
    }
}
```

**Files:** `~45 *Tbl.cs files`

#### `TrackerSQL.Classes`

**Purpose:** Business logic, utilities, and helper classes

**Key Classes:**
- `TrackerDb` - Database connection and command execution
- `SystemConstants` - Application-wide constants
- `TimeZoneUtils` - Time zone handling utilities
- `AppLogger` - Logging functionality
- `DateCalculator` - Date calculation utilities

**Example:**
```csharp
namespace TrackerSQL.Classes
{
    public class TimeZoneUtils
    {
        public static DateTime Now()
        {
            // Returns current time in application time zone
        }
    }
}
```

#### `TrackerSQL.DataSets`

**Purpose:** Data models and business objects

**Pattern:** Business-focused data structures

**Example:**
```csharp
namespace TrackerSQL.DataSets
{
    public class CustomersCls
    {
        // Business properties
        // Validation methods
        // Business rules
    }
}
```

#### `TrackerSQL.Pages` (Code-behind)

**Purpose:** Web Forms code-behind files

**Pattern:** Partial classes for .aspx pages

**Example:**
```csharp
namespace TrackerSQL.Pages
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Page initialization
        }
    }
}
```

#### `TrackerSQL.Emails`

**Purpose:** Email generation and sending

**Key Classes:**
- Email template generation
- SMTP sending
- Email formatting utilities

### MigrationRunner Namespaces

#### `MigrationRunner`

**Purpose:** All migration tool code

**Key Classes:**
- `Program` - Entry point
- `AccessSchemaReader` - Schema extraction
- `DdlScriptGenerator` - DDL generation
- `DmlScriptGenerator` - DML generation
- `FkScriptGenerator` - FK generation

---

## Key Patterns and Conventions

### 1. Table Class Pattern

**Location:** `Controls\*Tbl.cs`

**Standard Structure:**
```csharp
using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;

namespace TrackerSQL.Controls
{
    public class [TableName]Tbl
    {
        // Constants for SQL queries
        private const string CONST_SQL_SELECT = "SELECT ...";
        private const string CONST_SQL_INSERT = "INSERT ...";
        private const string CONST_SQL_UPDATE = "UPDATE ...";
        
        // Private fields (backing fields for properties)
        private int _PrimaryKeyID;
        private string _SomeField;
        private DateTime _DateField;
        
        // Constructor (initialize with defaults)
        public [TableName]Tbl()
        {
            this._PrimaryKeyID = 0;
            this._SomeField = string.Empty;
            this._DateField = TimeZoneUtils.Now().Date;
        }
        
        // Properties (public accessors)
        public int PrimaryKeyID
        {
            get => this._PrimaryKeyID;
            set => this._PrimaryKeyID = value;
        }
        
        // Methods (CRUD operations)
        public List<[TableName]Tbl> GetAll(string SortBy) { }
        public [TableName]Tbl GetByID(int id) { }
        public string Insert[TableName]([TableName]Tbl item) { }
        public string Update[TableName]([TableName]Tbl item) { }
        public string Delete[TableName](int id) { }
    }
}
```

**Key Conventions:**
- Private fields prefixed with underscore
- Properties use get/set accessors
- Constructor initializes all fields
- Use `TimeZoneUtils.Now()` for dates
- Methods return error string (empty = success)

### 2. Database Access Pattern

**Using TrackerDb Class:**

```csharp
public List<ContactsTbl> GetActiveContacts()
{
    List<ContactsTbl> contacts = new List<ContactsTbl>();
    TrackerDb trackerDb = new TrackerDb();
    
    // Build SQL with parameters
    string sql = "SELECT * FROM ContactsTbl WHERE IsActive = ?";
    trackerDb.AddWhereParams((object)true, DbType.Boolean);
    
    // Execute query
    IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(sql);
    
    // Process results
    if (dataReader != null)
    {
        while (dataReader.Read())
        {
            contacts.Add(new ContactsTbl()
            {
                ContactID = Convert.ToInt32(dataReader["ContactID"]),
                ContactName = dataReader["ContactName"].ToString()
            });
        }
        dataReader.Close();
    }
    
    // Always close connection
    trackerDb.Close();
    
    return contacts;
}
```

**Important:**
- Always use parameterized queries
- Always close connection
- Always check for DBNull
- Always use proper DbType

### 3. Date Handling Pattern

**Always use TimeZoneUtils:**

```csharp
// ? WRONG
DateTime now = DateTime.Now;
DateTime today = DateTime.Today;

// ? CORRECT
DateTime now = TimeZoneUtils.Now();
DateTime today = TimeZoneUtils.Now().Date;
```

**Date Comparisons:**

```csharp
// When comparing dates, strip time component
DateTime targetDate = someDate.Date;

// When querying database
trackerDb.AddWhereParams((object)targetDate.Date, DbType.Date);
```

### 4. Null Handling Pattern

**Reading from Database:**

```csharp
// Value types
int id = dataReader["ContactID"] == DBNull.Value 
    ? 0 
    : Convert.ToInt32(dataReader["ContactID"]);

// Strings
string name = dataReader["ContactName"] == DBNull.Value 
    ? string.Empty 
    : dataReader["ContactName"].ToString();

// Dates
DateTime date = dataReader["PrepDate"] == DBNull.Value 
    ? TimeZoneUtils.Now().Date 
    : Convert.ToDateTime(dataReader["PrepDate"]).Date;

// Booleans
bool isActive = dataReader["IsActive"] != DBNull.Value 
    && Convert.ToBoolean(dataReader["IsActive"]);
```

### 5. Logging Pattern

**Using AppLogger:**

```csharp
AppLogger.WriteLog(
    SystemConstants.LogTypes.Email,
    $"ContactsTbl: Processed {count} contacts for {date:yyyy-MM-dd}"
);
```

**Log Types:**
- `SystemConstants.LogTypes.Email` - Email-related operations
- `SystemConstants.LogTypes.SendCheckup` - Checkup email operations
- `SystemConstants.LogTypes.General` - General operations
- `SystemConstants.LogTypes.Error` - Errors and exceptions

**Format:**
```csharp
$"[ClassName]: [Action] [Details with data]"
```

### 6. Error Handling Pattern

**Standard Try-Catch:**

```csharp
public int ProcessRecords(DateTime targetDate)
{
    int processedCount = 0;
    
    try
    {
        // Perform operation
        TrackerDb db = new TrackerDb();
        // ... operation code ...
        db.Close();
        
        // Check for errors
        if (!string.IsNullOrEmpty(errorResult))
        {
            AppLogger.WriteLog(
                SystemConstants.LogTypes.Error,
                $"MyClass: Operation failed: {errorResult}"
            );
            throw new Exception($"Operation failed: {errorResult}");
        }
        
        // Log success
        AppLogger.WriteLog(
            SystemConstants.LogTypes.General,
            $"MyClass: Successfully processed {processedCount} records"
        );
    }
    catch (Exception ex)
    {
        AppLogger.WriteLog(
            SystemConstants.LogTypes.Error,
            $"MyClass: Exception in operation: {ex.Message}"
        );
        throw new Exception($"Operation failed: {ex.Message}", ex);
    }
    
    return processedCount;
}
```

---

## Data Access Architecture

### TrackerDb Class (Database Abstraction)

**Location:** `Classes\TrackerDb.cs`

**Purpose:** Abstract database connections and command execution

**Key Methods:**

```csharp
public class TrackerDb
{
    // Connection management
    public void OpenConnection();
    public void Close();
    public bool TestConnection();
    
    // Parameter management
    public void AddParams(object value, DbType type);
    public void AddWhereParams(object value, DbType type);
    public void ClearParams();
    
    // Query execution
    public IDataReader ExecuteSQLGetDataReader(string sql);
    public string ExecuteNonQuerySQL(string sql);
    public object ExecuteScalar(string sql);
    
    // Transaction support
    public void BeginTransaction();
    public void CommitTransaction();
    public void RollbackTransaction();
}
```

**Usage Example:**

```csharp
TrackerDb db = new TrackerDb();

// For SELECT queries
db.AddWhereParams((object)contactId, DbType.Int32);
IDataReader reader = db.ExecuteSQLGetDataReader("SELECT * FROM ContactsTbl WHERE ContactID = ?");

// For INSERT/UPDATE/DELETE
db.AddParams((object)name, DbType.String);
db.AddParams((object)date, DbType.Date);
string result = db.ExecuteNonQuerySQL("INSERT INTO ContactsTbl (ContactName, PrepDate) VALUES (?, ?)");

// Always close
db.Close();
```

### Parameter Handling

**Two Types of Parameters:**

1. **Regular Parameters** (`AddParams`):
   - Used for INSERT/UPDATE values
   - Used for non-WHERE parameters

2. **Where Parameters** (`AddWhereParams`):
   - Used for WHERE clause values
   - Used for query filtering

**Example:**
```csharp
// UPDATE ContactsTbl SET ContactName = ? WHERE ContactID = ?
db.AddParams((object)newName, DbType.String);      // SET clause
db.AddWhereParams((object)contactId, DbType.Int32); // WHERE clause
string result = db.ExecuteNonQuerySQL(sql);
```

---

## Web Pages Structure

### ASP.NET Web Forms Organization

**Pattern:** Each page consists of two files:
1. `.aspx` - Markup (HTML + server controls)
2. `.aspx.cs` - Code-behind (C# logic)

**Example: Customer List Page**

**CustomerList.aspx:**
```aspx
<%@ Page Language="C#" AutoEventWireup="true" 
    CodeBehind="CustomerList.aspx.cs" 
    Inherits="TrackerSQL.Pages.CustomerList" %>

<asp:GridView ID="gvCustomers" runat="server" 
    AutoGenerateColumns="false"
    OnRowCommand="gvCustomers_RowCommand">
    <Columns>
        <asp:BoundField DataField="ContactID" HeaderText="ID" />
        <asp:BoundField DataField="ContactName" HeaderText="Name" />
        <asp:ButtonField CommandName="Edit" Text="Edit" />
    </Columns>
</asp:GridView>
```

**CustomerList.aspx.cs:**
```csharp
namespace TrackerSQL.Pages
{
    public partial class CustomerList : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGrid();
            }
        }
        
        private void BindGrid()
        {
            ContactsTbl contacts = new ContactsTbl();
            gvCustomers.DataSource = contacts.GetAll("ContactName");
            gvCustomers.DataBind();
        }
        
        protected void gvCustomers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Edit")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                // Handle edit
            }
        }
    }
}
```

### Common Page Patterns

**1. Data Binding:**
```csharp
protected void Page_Load(object sender, EventArgs e)
{
    if (!IsPostBack)
    {
        BindData();
    }
}

private void BindData()
{
    // Get data
    // Bind to controls
}
```

**2. Form Submission:**
```csharp
protected void btnSave_Click(object sender, EventArgs e)
{
    // Validate input
    if (!ValidateForm())
        return;
        
    // Save data
    SaveData();
    
    // Redirect or show message
}
```

**3. GridView Operations:**
```csharp
protected void gvData_RowEditing(object sender, GridViewEditEventArgs e)
{
    gvData.EditIndex = e.NewEditIndex;
    BindGrid();
}

protected void gvData_RowUpdating(object sender, GridViewUpdateEventArgs e)
{
    // Update logic
}

protected void gvData_RowDeleting(object sender, GridViewDeleteEventArgs e)
{
    // Delete logic
}
```

---

## Email System

### Email Architecture

**Location:** `Emails\` folder

**Key Components:**

1. **EmailHelper.cs** - Email sending utilities
2. **CheckupEmailGenerator.cs** - Generates checkup reminder emails
3. **Templates\\** - Email HTML templates

### Checkup Email Flow

```
1. Scheduled Task / Manual Trigger
   ?
2. CheckupEmailGenerator.GenerateCheckupEmails()
   ?
3. Query ContactsTbl for contacts needing checkup
   ?
4. For each contact:
   - Get predicted usage (ContactsItemsPredictedTbl)
   - Get recent orders (OrdersTbl, OrderLinesTbl)
   - Generate email HTML
   - Send via EmailHelper
   - Log to SentRemindersLogTbl
   ?
5. Return summary report
```

### Email Generation Pattern

```csharp
public class CheckupEmailGenerator
{
    public void SendCheckupEmails(DateTime targetDate)
    {
        // Get contacts needing checkup
        ContactsTbl contacts = new ContactsTbl();
        List<ContactsTbl> toEmail = contacts.GetContactsNeedingCheckup(targetDate);
        
        foreach (ContactsTbl contact in toEmail)
        {
            // Generate email content
            string emailBody = GenerateEmailBody(contact);
            
            // Send email
            EmailHelper.SendEmail(
                to: contact.EmailAddress,
                subject: "Checkup Reminder",
                body: emailBody,
                isHtml: true
            );
            
            // Log sent reminder
            SentRemindersLogTbl log = new SentRemindersLogTbl();
            log.CustomerID = contact.ContactID;
            log.DateSentReminder = TimeZoneUtils.Now();
            log.ReminderSent = true;
            log.InsertLogItem(log);
            
            AppLogger.WriteLog(
                SystemConstants.LogTypes.SendCheckup,
                $"Sent checkup to {contact.ContactName}"
            );
        }
    }
}
```

---

## Migration Tools

### MigrationRunner Architecture

**Purpose:** Automate database schema extraction and migration script generation

**Main Flow:**

```
1. User runs MigrationRunner.exe
   ?
2. Interactive menu displayed
   ?
3. User selects operation:
   - 1: Export Access Schema
   - 2: Apply Bulk Renames
   - 3: Generate DDL Scripts
   - 4: Generate DML Scripts
   - 5: Generate FK Scripts
   - $: Full Pipeline (all steps)
   ?
4. Operation executes
   ?
5. Results written to files/console
```

### Key Classes

**AccessSchemaReader.cs:**
- Connects to Access database
- Reads table and column definitions
- Exports to JSON metadata

**DdlScriptGenerator.cs:**
- Reads metadata
- Generates CREATE TABLE statements
- Outputs to `CreateTables_LATEST.sql`

**DmlScriptGenerator.cs:**
- Connects to Access database
- Generates INSERT statements for all tables
- Handles data type conversions
- Outputs to `DataMigration_LATEST.sql`

**FkScriptGenerator.cs:**
- Reads foreign key definitions
- Generates ALTER TABLE ADD CONSTRAINT
- Outputs to `AddForeignKeys_LATEST.sql`

### Configuration Files

**BulkRenameRules.json:**
```json
{
  "TableRenames": {
    "CustomersTbl": "ContactsTbl",
    "AreaTbl": "AreasTbl"
  },
  "ColumnRenames": {
    "CustomerID": "ContactID",
    "PrepDate": "PrepDate"
  }
}
```

**PlanColumns.csv:**
```csv
TableName,ColumnName,DataType,IsNullable,IsPrimaryKey
ContactsTbl,ContactID,INT,NO,YES
ContactsTbl,ContactName,NVARCHAR(255),NO,NO
```

---

## File Naming Conventions

### Table Classes
**Pattern:** `[TableName]Tbl.cs`
- Example: `ContactsTbl.cs`, `ItemsTbl.cs`, `OrdersTbl.cs`

### Web Pages
**Pattern:** `[FeatureName].aspx` + `[FeatureName].aspx.cs`
- Example: `CustomerList.aspx` + `CustomerList.aspx.cs`

### Utilities
**Pattern:** `[Purpose]Helper.cs` or `[Purpose]Utils.cs`
- Example: `EmailHelper.cs`, `TimeZoneUtils.cs`

### Constants
**Pattern:** `SystemConstants.cs` (single file)

### Configuration
**Pattern:** `Web.config` (main), `[Purpose].config` (supplemental)

---

## Dependencies and Libraries

### Main Web Application

**NuGet Packages:**
- `AjaxControlToolkit` - AJAX controls for Web Forms
- `BouncyCastle.Cryptography` - Cryptography utilities
- `MailKit` - Email sending (SMTP)
- `Newtonsoft.Json` - JSON serialization

**Framework References:**
- `System.Web` - ASP.NET Web Forms
- `System.Data` - Database access
- `System.Data.OleDb` - Access database (being phased out)
- `System.Data.SqlClient` - SQL Server (migration target)

### MigrationRunner

**NuGet Packages:**
- `Newtonsoft.Json` - JSON configuration files

**Framework References:**
- `System.Data.OleDb` - Access database reading
- `System.IO` - File operations

---

## Configuration Files

### Web.config (Main Application)

**Key Sections:**

```xml
<configuration>
  <!-- Connection Strings -->
  <connectionStrings>
    <add name="TrackerDB" 
         connectionString="..." 
         providerName="System.Data.OleDb"/>
  </connectionStrings>
  
  <!-- App Settings -->
  <appSettings>
    <add key="CompanyName" value="..."/>
    <add key="EmailServer" value="..."/>
  </appSettings>
  
  <!-- System Web -->
  <system.web>
    <compilation targetFramework="4.8" debug="true"/>
    <httpRuntime targetFramework="4.8"/>
  </system.web>
</configuration>
```

### MigrationRunner.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net48</TargetFramework>
    <LangVersion>7.3</LangVersion>
    <Nullable>disable</Nullable>
    <PlatformTarget>x64</PlatformTarget>
  </PropertyGroup>
</Project>
```

---

## Build and Deployment

### Building the Solution

**Visual Studio:**
```
Build > Build Solution (Ctrl+Shift+B)
```

**Command Line:**
```cmd
cd C:\SRC\ASP.net\TrackerSQL
msbuild TrackerSQL.sln /p:Configuration=Release
```

### Running MigrationRunner

```cmd
cd Migrations\MigrationRunner
dotnet build
dotnet run
```

### Deploying Web Application

1. Build in Release mode
2. Publish to folder or IIS
3. Update Web.config connection string
4. Configure IIS application pool (.NET 4.x)

---

## Testing Locations

### Unit Tests
*Currently: No formal unit test project*

**Recommendation:** Add a test project in future

### Manual Testing
- Use test pages in `Tools\` folder
- Test individual pages in browser
- Use SQL queries to verify data

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-03-26 | Initial code structure documentation |

---

**For More Information:**
- See `AI_CONTEXT.md` for coding patterns
- See `MIGRATION_GUIDE.md` for migration procedures
- See `TABLE_SCHEMA_REFERENCE.md` for database schema
