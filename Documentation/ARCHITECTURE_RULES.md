# ??? Architecture Rules & Standards

**Project:** TrackerSQL  
**Target Framework:** .NET Framework 4.8  
**Last Updated:** 2025-03-26  

---

## ?? Table of Contents

1. [Data Access Rules](#data-access-rules)
2. [Repository Pattern Standards](#repository-pattern-standards)
3. [UI Layer Standards](#ui-layer-standards)
4. [Code Organization](#code-organization)
5. [Migration Guidelines](#migration-guidelines)

---

## ?? Data Access Rules

### Rule #1: Repository Pattern Mandatory

**All data access MUST use the Repository pattern.**

? **PROHIBITED:**
- `SqlDataSource` controls
- `ObjectDataSource` pointing to legacy Table Classes (e.g., `*Tbl.cs` in `Controls/`)
- Direct database access in code-behind
- Inline SQL in ASPX markup
- Access database connections

? **REQUIRED:**
- Repository classes inheriting from `RepositoryBase<T>`
- POCO models in `Classes/Poco/`
- Repositories in `Classes/Sql/`
- SQL Server connections only (`TrackerDataSQL`)

---

### Why This Rule?

**Problems with SqlDataSource/ObjectDataSource:**

1. **Violates Separation of Concerns** - Data access logic embedded in UI
2. **No Type Safety** - Magic strings, runtime errors
3. **Hard to Test** - Cannot mock or unit test
4. **Not Reusable** - Same data access code duplicated across pages
5. **Maintenance Nightmare** - SQL in markup, no version control
6. **Security Risks** - Easier to make SQL injection mistakes

**Benefits of Repository Pattern:**

1. ? **Clean Separation** - Data access isolated from UI
2. ? **Type Safe** - Compile-time checking, IntelliSense
3. ? **Testable** - Easy to mock and unit test
4. ? **Reusable** - One repository used by multiple pages
5. ? **Maintainable** - All data access in one place
6. ? **Consistent** - Standard CRUD operations across all tables

---

## ??? Repository Pattern Standards

### Standard Repository Structure

```csharp
// Location: Classes/Sql/[EntityName]Repository.cs
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerDotNet.Classes.Poco;

namespace TrackerDotNet.Classes.Sql
{
    public class [EntityName]Repository : RepositoryBase<[EntityName]>
    {
        // 1. Override protected properties
        protected override string TableName => "[TableName]";
        protected override string KeyColumn => "[PrimaryKeyColumn]";
        protected override string CoreColumns => "[Column1], [Column2], ...";
        protected override string LookupColumns => "[KeyColumn], [NameColumn]";

        // 2. Override GetAll if SQL aliases needed
        public override List<[EntityName]> GetAll(string sortBy)
        {
            var list = new List<[EntityName]>();
            string sql = "SELECT [Column1] AS [Property1], ... FROM [TableName]";
            
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                // Map POCO property names to DB column names if different
                string orderBy = sortBy.Equals("Property1", System.StringComparison.OrdinalIgnoreCase) 
                    ? "Column1" 
                    : sortBy;
                sql += " ORDER BY " + orderBy;
            }
            
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr.Read()) list.Add(DbMapper.Map<[EntityName]>(rdr));
            }
            return list;
        }

        // 3. Implement Insert
        public void Insert([EntityName] entity)
        {
            const string sql = "INSERT INTO [TableName] ([Col1], [Col2], ...) VALUES (@Val1, @Val2, ...)";
            
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@Val1", DataValue = entity.Property1, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Val2", DataValue = entity.Property2 ?? default, DataDbType = DbType.Int32 },
                // ... more parameters
            };
            
            ExecNonQuery(sql, parameters);
        }

        // 4. Implement Update
        public void Update([EntityName] entity)
        {
            const string sql = "UPDATE [TableName] SET [Col1] = @Val1, [Col2] = @Val2 WHERE [KeyColumn] = @ID";
            
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@Val1", DataValue = entity.Property1, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Val2", DataValue = entity.Property2, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ID", DataValue = entity.ID, DataDbType = DbType.Int32 }
            };
            
            ExecNonQuery(sql, parameters);
        }

        // 5. Implement Delete
        public void Delete(int id)
        {
            const string sql = "DELETE FROM [TableName] WHERE [KeyColumn] = @ID";
            
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ID", DataValue = id, DataDbType = DbType.Int32 }
            };
            
            ExecNonQuery(sql, parameters);
        }

        // 6. Add custom methods as needed
        public List<[EntityName]> Search(string searchTerm, string sortBy)
        {
            // Custom search implementation
        }
    }
}
```

---

### POCO Model Standards

```csharp
// Location: Classes/Poco/[EntityName].cs
namespace TrackerDotNet.Classes.Poco
{
    public class [EntityName]
    {
        // 1. Properties match DB columns (with possible aliases)
        public int [EntityName]ID { get; set; }
        public string [Property1] { get; set; }
        
        // 2. Use nullable types for optional DB columns
        public int? [OptionalProperty] { get; set; }
        public bool? [OptionalFlag] { get; set; }
        
        // 3. Use meaningful names (not necessarily DB column names)
        // Example: "PersonName" instead of "Person" if clearer
    }
}
```

---

## ??? UI Layer Standards

### Code-Behind Pattern

```csharp
// Pages/SomePage.aspx.cs
protected void Page_Load(object sender, EventArgs e)
{
    if (!IsPostBack)
    {
        BindGrid();
    }
}

private void BindGrid()
{
    var repo = new [EntityName]Repository();
    var data = repo.GetAll("[SortColumn]");
    gv[EntityName].DataSource = data;
    gv[EntityName].DataBind();
}

protected void gv[EntityName]_RowCommand(object sender, GridViewCommandEventArgs e)
{
    var repo = new [EntityName]Repository();
    
    if (e.CommandName.Equals("AddItem") || e.CommandName.Equals("Insert"))
    {
        // Extract values from controls
        var entity = new [EntityName]
        {
            Property1 = tbxProperty1.Text,
            Property2 = Convert.ToInt32(tbxProperty2.Text)
        };
        
        repo.Insert(entity);
        BindGrid();
    }
    else if (e.CommandName.Equals("Update"))
    {
        // Update logic
        repo.Update(entity);
        BindGrid();
    }
    else if (e.CommandName.Equals("Delete"))
    {
        // Delete logic
        int id = Convert.ToInt32(e.CommandArgument);
        repo.Delete(id);
        BindGrid();
    }
}
```

---

### ASPX Markup Pattern

```aspx
<!-- ? DO NOT USE -->
<asp:SqlDataSource ID="sdsEntity" runat="server" ... />
<asp:ObjectDataSource ID="odsEntity" TypeName="TrackerSQL.Controls.EntityTbl" ... />

<!-- ? USE INSTEAD -->
<asp:GridView ID="gvEntity" runat="server" 
    AutoGenerateColumns="False" 
    DataKeyNames="EntityID"
    OnRowCommand="gvEntity_RowCommand"
    ShowFooter="True">
    <!-- Column definitions -->
</asp:GridView>

<!-- Bind data in code-behind, NOT with DataSourceID -->
```

---

## ?? Code Organization

### Directory Structure

```
TrackerSQL/
??? Classes/
?   ??? Poco/              ? POCO models (e.g., Person.cs, Item.cs)
?   ??? Sql/               ? Repositories (e.g., PersonsRepository.cs)
??? Controls/              ? Legacy Table Classes (to be deleted)
?   ??? *Tbl.cs            ? DEPRECATED - Do not use
??? Pages/
?   ??? *.aspx.cs          ? Code-behind uses Repositories
??? Documentation/
    ??? ARCHITECTURE_RULES.md  ?? This file
```

---

### File Naming Conventions

| Type | Location | Pattern | Example |
|------|----------|---------|---------|
| **POCO** | `Classes/Poco/` | `[Entity].cs` | `Person.cs`, `Item.cs` |
| **Repository** | `Classes/Sql/` | `[Entity]Repository.cs` | `PersonsRepository.cs` |
| **Legacy (DELETE)** | `Controls/` | `*Tbl.cs` | `PersonsTbl.cs` ? |

---

## ?? Migration Guidelines

### Migration Process for Existing Pages

When refactoring a page that uses `SqlDataSource` or legacy `ObjectDataSource`:

#### Step 1: Identify Data Sources

```aspx
<!-- Find these in ASPX -->
<asp:SqlDataSource ID="sdsEntity" ... />
<asp:ObjectDataSource ID="odsEntity" TypeName="TrackerSQL.Controls.EntityTbl" ... />
```

#### Step 2: Check/Create Repository

```csharp
// Check if exists: Classes/Sql/EntityRepository.cs
// If not, create following standard structure above
```

#### Step 3: Update Code-Behind

**Before:**
```csharp
// GridView bound to DataSource in ASPX
// No code-behind needed (bad!)
```

**After:**
```csharp
protected void Page_Load(object sender, EventArgs e)
{
    if (!IsPostBack) BindGrid();
}

private void BindGrid()
{
    var repo = new EntityRepository();
    gvEntity.DataSource = repo.GetAll("SortColumn");
    gvEntity.DataBind();
}

protected void gvEntity_RowCommand(object sender, GridViewCommandEventArgs e)
{
    // Handle Insert/Update/Delete
}
```

#### Step 4: Update ASPX

**Before:**
```aspx
<asp:GridView ID="gvEntity" runat="server" DataSourceID="sdsEntity" ... />
<asp:SqlDataSource ID="sdsEntity" ... />
```

**After:**
```aspx
<asp:GridView ID="gvEntity" runat="server" 
    OnRowCommand="gvEntity_RowCommand" ... />
<!-- Remove DataSourceID -->
<!-- Delete SqlDataSource completely -->
```

#### Step 5: Test

- [ ] Page loads without errors
- [ ] Grid displays data
- [ ] Add/Edit/Update/Delete work
- [ ] Sorting/Paging work (if applicable)

#### Step 6: Clean Up

- [ ] Delete SqlDataSource/ObjectDataSource from ASPX
- [ ] Mark legacy `*Tbl.cs` for deletion (after full solution search)
- [ ] Update documentation

---

## ?? Exceptions

**There are NO exceptions to the Repository Pattern rule.**

Even for:
- ? "Simple" pages
- ? "Prototype" features
- ? "Legacy" pages that "work fine"
- ? "Quick fixes"

**All data access uses Repositories. No exceptions.**

---

## ? Checklist for New Development

When creating a new data-driven page:

- [ ] POCO model exists in `Classes/Poco/`
- [ ] Repository exists in `Classes/Sql/`
- [ ] Repository implements Insert/Update/Delete
- [ ] Code-behind binds grid in `Page_Load`
- [ ] Code-behind handles RowCommand events
- [ ] **NO** SqlDataSource in ASPX
- [ ] **NO** ObjectDataSource pointing to `Controls/*Tbl.cs`
- [ ] Connection string is `TrackerDataSQL` (SQL Server)
- [ ] Build successful
- [ ] Page tested (CRUD operations work)

---

## ?? Current Migration Status

### Lookups.aspx Status

| Tab | Status | Data Source Type |
|-----|--------|------------------|
| Equipment | ? Complete | Repository |
| InvoiceTypes | ? Complete | Repository |
| PaymentTerms | ? Complete | Repository |
| PriceLevels | ? Complete | Repository |
| People | ? Complete | Repository |
| Packaging | ? Complete | Repository |
| RepairStatuses | ? Complete | Repository |
| **Items** | ?? **IN PROGRESS** | ~~SqlDataSource~~ ? Repository |
| **Cities** | ?? **IN PROGRESS** | ~~SqlDataSource~~ ? Repository |

**Target:** 100% Repository pattern compliance

---

## ?? References

### Related Documents

- [PROJECT_OVERVIEW.md](PROJECT_OVERVIEW.md) - Overall project architecture
- [MIGRATION_GUIDE.md](MIGRATION_GUIDE.md) - Step-by-step migration instructions
- [CODE_STRUCTURE.md](CODE_STRUCTURE.md) - Code organization details
- [MIGRATION_TODO.md](WorkInProgress/MIGRATION_TODO.md) - Migration task list

### Pattern Resources

- **Repository Pattern:** Martin Fowler - https://martinfowler.com/eaaCatalog/repository.html
- **POCO:** Plain Old CLR Objects
- **Separation of Concerns:** https://en.wikipedia.org/wiki/Separation_of_concerns

---

## ?? Version History

| Date | Version | Changes |
|------|---------|---------|
| 2025-03-26 | 1.0 | Initial creation - Repository Pattern mandatory |

---

**? This is a living document. Update as patterns evolve.**

**? Never compromise on the Repository Pattern rule.**
