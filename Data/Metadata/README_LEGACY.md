# LEGACY — do not use

SQL, reports, and scripts under `Data/Metadata/` in **TrackerSQL** are **stale copies**.

**Source of truth:** the **TrackerMigration** project (schema, CreateTables, Migrate_*, Alter_*, Verify_*).

Do not run scripts from this folder against production/dev databases. Do not treat filenames here as the current column/table names. Prefer the live SQL Server database and TrackerMigration for schema.

These files may remain on disk for historical reference only; they are **excluded from `TrackerSQL.csproj`**.
