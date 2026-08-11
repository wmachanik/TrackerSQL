# Migration Playbook (Junior-Friendly)
TrackerSQL (Access / OleDb / Jet 32-bit) → TrackerSQL (SQL Server / SqlClient)
Target Framework: .NET Framework 4.8 (unchanged)
Version: 1.0
Owner / Reviewer: (fill in)
Date Started: (fill in)

---

## 1. Overview (What We Are Doing)
We are creating a brand‑new project named `TrackerSQL` cloned from the existing `TrackerSQL`.  
The old project remains available for maintenance (critical bug fixes only).  
Inside the new project we will:
1. Replace the Access database with SQL Server / SQL Express.
2. Migrate schema and data safely in repeatable steps.
3. Refactor database calls to remove Access-specific SQL and OleDb usage.
4. Keep the UI working during incremental migration (pages continue to function).
5. Switch production to the new project only after full test sign‑off.

---

## 2. Roles & Responsibilities (Example)
- Lead Developer: Approves schema changes, reviews migrations.
- Junior Developer: Executes steps in this playbook, raises blockers early.
- Tester / QA: Runs smoke & regression checklists.
- Ops / Deployment: Handles IIS / SQL environment creation.
(Adjust for your team.)

---

## 3. Prerequisites (Install / Verify First)
- SQL Server Express or full SQL Server instance installed (with SSMS).
- Git installed and authenticated to repo.
- Visual Studio 2022 with:
  - .NET Framework 4.8 developer pack
  - Web development workload
- Powershell 5+ (standard on Windows 10/11).
- Backup copy of the current Access `.mdb` / `.accdb` file.

---

## 4. High-Level Phases (With Junior-Friendly Goal Statements)
| Phase | Name | Goal (Simple Language) |
|-------|------|------------------------|
| P0 | Preparation | Make a safe copy & freeze scope. |
| P1 | Project Clone | Create new `TrackerSQL` project so we stop touching old code. |
| P2 | Schema Extraction | List every table/column in Access (truth source). |
| P3 | SQL Schema Creation | Build matching tables in SQL Server (with improvements). |
| P4 | Data Migration (First Pass) | Move existing data across and check counts. |
| P5 | Data Access Abstraction | Introduce clean interface so code stops caring about Access vs SQL. |
| P6 | Query Refactor | Replace Access-specific syntax with T‑SQL. |
| P7 | Page-by-Page Migration | Update each page’s data sources in a controlled order. |
| P8 | Testing & Fixes | Confirm everything still works and is fast enough. |
| P9 | Cutover Prep | Final verification + migration rehearsal. |
| P10 | Production Cutover | Launch new version; keep rollback ready. |
| P11 | Post-Cutover Cleanup | Remove leftover unused code & finalize docs. |

---

## 5. Phase Details (Each With “Why”, “Inputs”, “Steps”, “Done When”)

### P0: Preparation
Why: Avoid mid-migration surprises.
Inputs: Current stable branch.
Steps:
1. Announce code freeze date for new features in old project (only critical fixes after).
2. List known pain points (e.g., slow pages, fragile queries).
3. Confirm Access DB path, ensure latest copy is checked in (if allowed) or securely stored.
Done When: Team agrees on freeze + has backup + this document stored in `/Docs`.

### P1: Project Clone
Why: Work without breaking existing production.
Inputs: Repository root.
Steps:
1. Create maintenance branch: