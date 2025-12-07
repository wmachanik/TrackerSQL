using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Globalization;

namespace MigrationRunner
{
    internal static class HumanReviewConstraintsImporter
    {
        public static int ImportConstraints(string migrationsDir, string csvPath, out string jsonOutPath)
        {
            jsonOutPath = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "PlanConstraints.json");
            try
            {
                if (string.IsNullOrWhiteSpace(csvPath))
                {
                    csvPath = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "TableMigrationReport.csv");
                }
                if (!File.Exists(csvPath))
                {
                    Console.Error.WriteLine("Human Review CSV not found: " + csvPath);
                    return 2;
                }

                var lines = File.ReadAllLines(csvPath);
                var constraints = new ConstraintsIndex();
                var map = new Dictionary<string, ConstraintTable>(StringComparer.OrdinalIgnoreCase);

                Func<string, bool> isDivider = s => (s ?? "").StartsWith("-------------------------------/-------------------------------", StringComparison.Ordinal);
                Func<List<string>, List<string>> norm = row =>
                {
                    var r = new List<string>(row ?? new List<string>());
                    while (r.Count < 64) r.Add(string.Empty); // pad generously
                    for (int k = 0; k < r.Count; k++) r[k] = (r[k] ?? "").Trim(); // strong trim
                    return r;
                };

                int i = 0;
                while (i < lines.Length)
                {
                    var row = Split(lines[i]);
                    if (row.Count == 0) { i++; continue; }

                    // SIMPLE SECTION
                    if (StartsWithCell(row, "Table") && IndexOf(row, "Before") == 1 && IndexOf(row, "After") == 2)
                    {
                        i++;
                        if (i >= lines.Length) break;

                        var mapRow = norm(Split(lines[i]));
                        if (mapRow.Count < 4 || !string.IsNullOrEmpty(mapRow[0])) { i++; continue; }

                        var afterTable = (mapRow[2] ?? "").Trim();
                        var action = (mapRow[3] ?? "").Trim();

                        if (string.IsNullOrEmpty(afterTable) || action.Equals("Ignore", StringComparison.OrdinalIgnoreCase))
                        {
                            while (i < lines.Length && !isDivider(lines[i])) i++;
                            i++; // past divider
                            continue;
                        }

                        // Seek Rows:
                        bool rowsHeaderFound = false;
                        while (i < lines.Length)
                        {
                            i++;
                            if (i >= lines.Length) break;
                            var r2 = Split(lines[i]);
                            if (StartsWithCell(r2, "Rows:")) { rowsHeaderFound = true; row = r2; break; }
                            if (isDivider(lines[i])) break;
                        }
                        if (!rowsHeaderFound) { continue; }

                        // Simple header indices need a local header variable
                        var hdr = row;

                        int iAfterCol  = IndexOf(hdr, "After Col Name");
                        int iAfterKey  = IndexOf(hdr, "After Key");
                        int iAfterAuto = IndexOf(hdr, "After Auto");
                        int iAfterNN   = IndexOf(hdr, "After NotNull");
                        if (iAfterNN < 0) iAfterNN = IndexOf(hdr, "After Not Null");
                        int iAfterType = IndexOf(hdr, "After Type");

                        i++;
                        while (i < lines.Length && !isDivider(lines[i]))
                        {
                            var data = norm(Split(lines[i]));
                            var afterCol  = Get(data, iAfterCol);
                            var afterKey  = Get(data, iAfterKey);
                            var afterAuto = Get(data, iAfterAuto);
                            var afterNN   = Get(data, iAfterNN);
                            var afterType = Get(data, iAfterType);

                            if (!string.IsNullOrWhiteSpace(afterCol))
                            {
                                var ct = GetOrAdd(map, constraints, afterTable);

                                if (!string.IsNullOrWhiteSpace(afterType))
                                {
                                    if (ct.ColumnTypes == null) ct.ColumnTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                                    ct.ColumnTypes[afterCol] = afterType.Trim();
                                }

                                if (IsPk(afterKey)) AddUnique(ct.PrimaryKey, afterCol);
                                if (IsYes(afterAuto)) AddUnique(ct.IdentityColumns, afterCol);
                                var fk = ParseFk(afterCol, afterKey);
                                if (fk != null) AddFk(ct.ForeignKeys, fk);
                                if (IsYes(afterNN)) AddUnique(ct.NotNullColumns, afterCol);
                            }
                            i++;
                        }
                        i++; // skip divider
                        continue;
                    }

                    // NORMALIZE SECTION
                    if (StartsWithCell(row, "Table:"))
                    {
                        i++;
                        if (i >= lines.Length) break;
                        var meta = norm(Split(lines[i]));
                        if (meta.Count < 5 || !StartsWithCell(meta, "=====")) { i++; continue; }

                        var headerTbl = (meta[2] ?? "").Trim();
                        var lineTbl = (meta[3] ?? "").Trim();

                        bool rowsHeaderFound = false;
                        while (i < lines.Length)
                        {
                            i++;
                            if (i >= lines.Length) break;
                            var r2 = Split(lines[i]);
                            if (StartsWithCell(r2, "Rows:")) { rowsHeaderFound = true; row = r2; break; }
                            if (isDivider(lines[i])) break;
                        }
                        if (!rowsHeaderFound) { continue; }

                        // NORMALIZE header/lines indices
                        var hdr = row;
                        int iAhCol  = IndexOf(hdr, "After Header Col Name");
                        int iAhKey  = IndexOf(hdr, "After Header Key");
                        int iAhAuto = IndexOf(hdr, "After Header Auto");
                        int iAhNN   = IndexOf(hdr, "After Header NotNull");
                        if (iAhNN < 0) iAhNN = IndexOf(hdr, "After Header Not Null");
                        // NEW: header type
                        int iAhType = IndexOf(hdr, "After Header Type");

                        int iAlCol  = IndexOf(hdr, "After Lines Col Name");
                        int iAlKey  = IndexOf(hdr, "After Lines Key");
                        int iAlAuto = IndexOf(hdr, "After Lines Auto");
                        int iAlNN   = IndexOf(hdr, "After Lines NotNull");
                        if (iAlNN < 0) iAlNN = IndexOf(hdr, "After Lines Not Null");
                        // NEW: lines type
                        int iAlType = IndexOf(hdr, "After Lines Type");

                        // Resilience: if CSV reused Header NotNull label for Lines
                        if (iAlNN < 0)
                        {
                            var alt = IndexOf(hdr, "After Header NotNull", iAhNN + 1);
                            if (alt > iAhNN) iAlNN = alt;
                        }

                        i++;
                        while (i < lines.Length && !isDivider(lines[i]))
                        {
                            var data = norm(Split(lines[i]));

                            // Header side
                            var ahCol  = Get(data, iAhCol);
                            var ahKey  = Get(data, iAhKey);
                            var ahAuto = Get(data, iAhAuto);
                            var ahNN   = Get(data, iAhNN);
                            var ahType = Get(data, iAhType); // NEW
                            if (!string.IsNullOrWhiteSpace(ahCol) && !string.IsNullOrWhiteSpace(headerTbl))
                            {
                                var ctH = GetOrAdd(map, constraints, headerTbl);

                                // NEW: persist header type
                                if (!string.IsNullOrWhiteSpace(ahType))
                                {
                                    if (ctH.ColumnTypes == null) ctH.ColumnTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                                    ctH.ColumnTypes[ahCol] = ahType.Trim();
                                }

                                if (IsPk(ahKey)) AddUnique(ctH.PrimaryKey, ahCol);
                                if (IsYes(ahAuto)) AddUnique(ctH.IdentityColumns, ahCol);
                                var fkH = ParseFk(ahCol, ahKey);
                                if (fkH != null) AddFk(ctH.ForeignKeys, fkH);
                                if (IsYes(ahNN)) AddUnique(ctH.NotNullColumns, ahCol);
                            }

                            // Lines side
                            var alCol  = Get(data, iAlCol);
                            var alKey  = Get(data, iAlKey);
                            var alAuto = Get(data, iAlAuto);
                            var alNN   = Get(data, iAlNN);
                            var alType = Get(data, iAlType); // NEW
                            if (!string.IsNullOrWhiteSpace(alCol) && !string.IsNullOrWhiteSpace(lineTbl))
                            {
                                var ctL = GetOrAdd(map, constraints, lineTbl);

                                // NEW: persist lines type
                                if (!string.IsNullOrWhiteSpace(alType))
                                {
                                    if (ctL.ColumnTypes == null) ctL.ColumnTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                                    ctL.ColumnTypes[alCol] = alType.Trim();
                                }

                                if (IsPk(alKey)) AddUnique(ctL.PrimaryKey, alCol);
                                if (IsYes(alAuto)) AddUnique(ctL.IdentityColumns, alCol);
                                var fkL = ParseFk(alCol, alKey);
                                if (fkL != null)
                                {
                                    if (IsHeaderFk(alKey) && !string.IsNullOrWhiteSpace(headerTbl))
                                        fkL.RefTable = headerTbl;
                                    AddFk(ctL.ForeignKeys, fkL);
                                }
                                if (IsYes(alNN)) AddUnique(ctL.NotNullColumns, alCol);
                            }

                            i++;
                        }
                        i++; // skip divider
                        continue;
                    }

                    i++;
                }

                // Summary
                int tblCount = constraints.Tables?.Count ?? 0;
                int pkCols = (constraints.Tables ?? new List<ConstraintTable>()).Sum(t => (t.PrimaryKey ?? new List<string>()).Count);
                int idCols = (constraints.Tables ?? new List<ConstraintTable>()).Sum(t => (t.IdentityColumns ?? new List<string>()).Count);
                int fkCount = (constraints.Tables ?? new List<ConstraintTable>()).Sum(t => (t.ForeignKeys ?? new List<ForeignKeyDef>()).Count);
                int nnCols = (constraints.Tables ?? new List<ConstraintTable>()).Sum(t => (t.NotNullColumns ?? new List<string>()).Count);
                Console.WriteLine($"Constraints summary: Tables={tblCount} PKCols={pkCols} IdentityCols={idCols} FKs={fkCount} NotNullCols={nnCols}");

                Directory.CreateDirectory(Path.GetDirectoryName(jsonOutPath) ?? migrationsDir);
                File.WriteAllText(jsonOutPath, JsonConvert.SerializeObject(constraints, Formatting.Indented));
                Console.WriteLine("Constraints extracted to: " + jsonOutPath);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("ImportConstraints failed: " + ex.Message);
                return 1;
            }
        }

        // ---- CSV helpers ----

        private static List<string> Split(string line)
        {
            var res = new List<string>();
            if (line == null) return res;
            var sb = new System.Text.StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                if (inQuotes)
                {
                    if (ch == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                        else { inQuotes = false; }
                    }
                    else sb.Append(ch);
                }
                else
                {
                    if (ch == '"') inQuotes = true;
                    else if (ch == ',') { res.Add(sb.ToString()); sb.Clear(); }
                    else sb.Append(ch);
                }
            }
            res.Add(sb.ToString());
            for (int k = 0; k < res.Count; k++) res[k] = (res[k] ?? "").Trim();
            return res;
        }

        private static int IndexOf(List<string> header, string name, int startIndex = 0)
        {
            if (header == null || name == null) return -1;
            for (int i = Math.Max(0, startIndex); i < header.Count; i++)
                if (string.Equals(header[i] ?? "", name, StringComparison.OrdinalIgnoreCase))
                    return i;

            // tolerant: collapse spaces to match "NotNull" vs "Not Null"
            string normName = RemoveSpaces(name);
            for (int i = Math.Max(0, startIndex); i < header.Count; i++)
                if (string.Equals(RemoveSpaces(header[i] ?? ""), normName, StringComparison.OrdinalIgnoreCase))
                    return i;

            return -1;
        }

        private static string RemoveSpaces(string s)
            => new string((s ?? "").Where(c => !char.IsWhiteSpace(c)).ToArray());

        private static bool StartsWithCell(List<string> row, string prefix)
            => row != null && row.Count > 0 && (row[0] ?? "").StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

        private static string Get(List<string> cols, int idx)
            => (idx >= 0 && idx < (cols?.Count ?? 0)) ? (cols[idx] ?? "").Trim() : "";

        // ---- constraints helpers ----

        private static ConstraintTable GetOrAdd(Dictionary<string, ConstraintTable> map, ConstraintsIndex root, string table)
        {
            if (string.IsNullOrWhiteSpace(table)) return null;
            if (!map.TryGetValue(table, out var ct))
            {
                ct = new ConstraintTable { Table = table };
                map[table] = ct;
                if (root.Tables == null) root.Tables = new List<ConstraintTable>();
                root.Tables.Add(ct);
            }
            return ct;
        }

        private static void AddUnique(List<string> list, string v)
        {
            if (list == null || string.IsNullOrWhiteSpace(v)) return;
            if (!list.Any(x => string.Equals(x ?? "", v, StringComparison.OrdinalIgnoreCase)))
                list.Add(v);
        }

        private static void AddFk(List<ForeignKeyDef> list, ForeignKeyDef fk)
        {
            if (list == null || fk == null) return;
            if (!list.Any(x =>
                    string.Equals(x.Column ?? "", fk.Column ?? "", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.RefTable ?? "", fk.RefTable ?? "", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.RefColumn ?? "", fk.RefColumn ?? "", StringComparison.OrdinalIgnoreCase)))
            {
                list.Add(fk);
            }
        }

        private static ForeignKeyDef ParseFk(string column, string afterKey)
        {
            var key = (afterKey ?? "").Trim();
            if (!key.StartsWith("FK", StringComparison.OrdinalIgnoreCase)) return null;

            // expect FK (RefTable[.RefColumn]) OR FK (Header)
            string inside = "";
            var open = key.IndexOf('(');
            var close = key.LastIndexOf(')');
            if (open >= 0 && close > open) inside = key.Substring(open + 1, close - open - 1).Trim();

            var fk = new ForeignKeyDef { Column = column };

            if (inside.Equals("Header", StringComparison.OrdinalIgnoreCase))
            {
                // RefTable will be set by caller for Header case
                return fk;
            }

            if (inside.Length == 0) return fk; // no detail; will try to infer

            // Split "Table.Col" or just "Table"
            var parts = inside.Split(new[] { '.' }, 2);
            fk.RefTable = (parts.Length > 0 ? parts[0] : "").Trim();
            fk.RefColumn = (parts.Length > 1 ? parts[1] : "").Trim();
            if (fk.RefColumn != null && fk.RefColumn.Length == 0) fk.RefColumn = null;

            return fk;
        }

        private static bool IsPk(string afterKey)
            => string.Equals((afterKey ?? "").Trim(), "PK", StringComparison.OrdinalIgnoreCase);

        private static bool IsYes(string v)
        {
            var s = (v ?? "").Trim();
            if (s.Length == 0) return false;
            if (s.StartsWith("y", true, CultureInfo.InvariantCulture)) return true;  // Yes/Y
            if (s.StartsWith("t", true, CultureInfo.InvariantCulture)) return true;  // True/T
            if (s == "1") return true;
            if (s.Equals("required", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private static bool IsHeaderFk(string afterKey)
            => (afterKey ?? "").IndexOf("FK (Header", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}