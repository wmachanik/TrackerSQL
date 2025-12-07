using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace MigrationRunner
{
    internal static class PlanNamingFixer
    {
        public static int Apply(string migrationsDir, out string logPath)
        {
            logPath = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "Logs",
                "NamingFix_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");

            var sb = new StringBuilder();
            int fileChanges = 0, constraintChanges = 0, warnings = 0;

            try
            {
                var accessSchemaDir = Path.Combine(migrationsDir, "Metadata", "AccessSchema");
                var constraintsPath = Path.Combine(migrationsDir, "Metadata", "PlanEdits", "PlanConstraints.json");
                Directory.CreateDirectory(Path.GetDirectoryName(logPath) ?? migrationsDir);

                if (!Directory.Exists(accessSchemaDir))
                {
                    File.WriteAllText(logPath, "Access schema folder not found: " + accessSchemaDir);
                    return 2;
                }

                // 1) Fix AccessSchema plans
                var schemaFiles = Directory.GetFiles(accessSchemaDir, "*.schema.json")
                                           .Where(f => !string.Equals(Path.GetFileName(f), "index.json", StringComparison.OrdinalIgnoreCase))
                                           .ToArray();

                foreach (var f in schemaFiles)
                {
                    TableSchema s = null;
                    try { s = JsonConvert.DeserializeObject<TableSchema>(File.ReadAllText(f)); }
                    catch (Exception ex) { sb.AppendLine("WARN parse: " + f + " -> " + ex.Message); warnings++; continue; }
                    if (s == null) continue;

                    if (s.Plan == null)
                        s.Plan = new TablePlan { Classification = "Copy", TargetTable = s.SourceTable, ColumnActions = new List<ColumnPlan>() };
                    if (s.Plan.ColumnActions == null)
                        s.Plan.ColumnActions = new List<ColumnPlan>();

                    bool changed = false;

                    Func<string, string> normalizeIdSuffix = name =>
                    {
                        if (string.IsNullOrWhiteSpace(name)) return name;
                        // Only transform exact trailing "Id" to "ID" (case-sensitive to avoid re-touching already "ID")
                        if (name.EndsWith("Id", StringComparison.Ordinal))
                            return name.Substring(0, name.Length - 2) + "ID";
                        return name;
                    };

                    // Normalize: keep TargetTable aligned with HeaderTable
                    var isNormalize = string.Equals(s.Plan.Classification ?? "Copy", "Normalize", StringComparison.OrdinalIgnoreCase);
                    var n = s.Plan.Normalize;
                    if (isNormalize && n != null)
                    {
                        if (!string.IsNullOrWhiteSpace(n.HeaderTable) &&
                            !string.Equals(s.Plan.TargetTable ?? "", n.HeaderTable ?? "", StringComparison.Ordinal))
                        {
                            sb.AppendLine($"FIX [{s.SourceTable}] TargetTable: '{s.Plan.TargetTable ?? ""}' -> '{n.HeaderTable ?? ""}'");
                            s.Plan.TargetTable = n.HeaderTable;
                            changed = true;
                        }
                    }

                    // ColumnActions.Target ID casing
                    foreach (var a in s.Plan.ColumnActions)
                    {
                        var old = a.Target;
                        var tgt = string.IsNullOrWhiteSpace(a.Target) ? a.Source : a.Target;
                        var fixedTgt = normalizeIdSuffix(tgt);
                        if (!string.Equals(fixedTgt ?? "", tgt ?? "", StringComparison.Ordinal))
                        {
                            a.Target = fixedTgt;
                            sb.AppendLine($"FIX [{s.SourceTable}] Column Target '{tgt}' -> '{fixedTgt}' (src='{a.Source}')");
                            changed = true;
                        }
                    }

                    // Normalize key names and PK lists for Normalize plans
                    if (isNormalize && n != null)
                    {
                        // New keys
                        var nhOld = n.NewHeaderKeyName ?? "";
                        var nlOld = n.NewLineKeyName ?? "";
                        var nhNew = normalizeIdSuffix(nhOld);
                        var nlNew = normalizeIdSuffix(nlOld);
                        if (!string.Equals(nhNew, nhOld, StringComparison.Ordinal)) { n.NewHeaderKeyName = nhNew; sb.AppendLine($"FIX [{s.SourceTable}] NewHeaderKeyName '{nhOld}' -> '{nhNew}'"); changed = true; }
                        if (!string.Equals(nlNew, nlOld, StringComparison.Ordinal)) { n.NewLineKeyName = nlNew; sb.AppendLine($"FIX [{s.SourceTable}] NewLineKeyName '{nlOld}' -> '{nlNew}'"); changed = true; }

                        // PK lists
                        if (n.HeaderPrimaryKey != null && n.HeaderPrimaryKey.Count > 0)
                        {
                            for (int i = 0; i < n.HeaderPrimaryKey.Count; i++)
                            {
                                var old = n.HeaderPrimaryKey[i];
                                var nw = normalizeIdSuffix(old);
                                if (!string.Equals(nw ?? "", old ?? "", StringComparison.Ordinal))
                                {
                                    n.HeaderPrimaryKey[i] = nw;
                                    sb.AppendLine($"FIX [{s.SourceTable}] Header PK '{old}' -> '{nw}'");
                                    changed = true;
                                }
                            }
                        }
                        if (n.LinePrimaryKey != null && n.LinePrimaryKey.Count > 0)
                        {
                            for (int i = 0; i < n.LinePrimaryKey.Count; i++)
                            {
                                var old = n.LinePrimaryKey[i];
                                var nw = normalizeIdSuffix(old);
                                if (!string.Equals(nw ?? "", old ?? "", StringComparison.Ordinal))
                                {
                                    n.LinePrimaryKey[i] = nw;
                                    sb.AppendLine($"FIX [{s.SourceTable}] Line PK '{old}' -> '{nw}'");
                                    changed = true;
                                }
                            }
                        }

                        // Link FK
                        var linkOld = n.LineLinkKeyName ?? "";
                        var linkNew = normalizeIdSuffix(linkOld);
                        if (!string.Equals(linkNew, linkOld, StringComparison.Ordinal))
                        {
                            n.LineLinkKeyName = linkNew;
                            sb.AppendLine($"FIX [{s.SourceTable}] LineLinkKeyName '{linkOld}' -> '{linkNew}'");
                            changed = true;
                        }

                        // If still empty, try infer from header PK/new key present in line targets
                        if (string.IsNullOrWhiteSpace(n.LineLinkKeyName))
                        {
                            var headerKeyPref = !string.IsNullOrWhiteSpace(n.NewHeaderKeyName)
                                ? n.NewHeaderKeyName
                                : (n.HeaderPrimaryKey != null && n.HeaderPrimaryKey.Count > 0 ? n.HeaderPrimaryKey[0] : "");
                            if (!string.IsNullOrWhiteSpace(headerKeyPref))
                            {
                                var headerSet = new HashSet<string>(n.HeaderColumns ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
                                var lineSet = new HashSet<string>(n.LineColumns ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
                                var lineTargets = (s.Plan.ColumnActions ?? new List<ColumnPlan>())
                                    .Where(a => lineSet.Contains(a.Source ?? "", StringComparer.OrdinalIgnoreCase) &&
                                                !string.Equals(a.Action ?? "Copy", "Drop", StringComparison.OrdinalIgnoreCase))
                                    .Select(a => string.IsNullOrWhiteSpace(a.Target) ? a.Source : a.Target)
                                    .ToList();
                                if (lineTargets.Any(t => string.Equals(t ?? "", headerKeyPref ?? "", StringComparison.OrdinalIgnoreCase)))
                                {
                                    n.LineLinkKeyName = headerKeyPref;
                                    sb.AppendLine($"INF [{s.SourceTable}] Inferred LineLinkKeyName = '{n.LineLinkKeyName}'");
                                    changed = true;
                                }
                            }
                        }
                    }

                    if (changed)
                    {
                        try
                        {
                            File.WriteAllText(f, JsonConvert.SerializeObject(s, Formatting.Indented), Encoding.UTF8);
                            fileChanges++;
                        }
                        catch (Exception ex) { sb.AppendLine("ERROR save: " + f + " -> " + ex.Message); }
                    }
                }

                // 2) Fix constraints to keep them aligned (IDs casing etc.)
                if (File.Exists(constraintsPath))
                {
                    try
                    {
                        var idx = JsonConvert.DeserializeObject<ConstraintsIndex>(File.ReadAllText(constraintsPath)) ?? new ConstraintsIndex();
                        bool cChanged = false;

                        Func<string, string> normId = v =>
                        {
                            if (string.IsNullOrWhiteSpace(v)) return v;
                            if (v.EndsWith("Id", StringComparison.Ordinal)) return v.Substring(0, v.Length - 2) + "ID";
                            return v;
                        };

                        foreach (var t in idx.Tables ?? new List<ConstraintTable>())
                        {
                            // PK
                            for (int i = 0; i < (t.PrimaryKey ?? new List<string>()).Count; i++)
                            {
                                var old = t.PrimaryKey[i];
                                var nw = normId(old);
                                if (!string.Equals(nw ?? "", old ?? "", StringComparison.Ordinal))
                                {
                                    t.PrimaryKey[i] = nw;
                                    sb.AppendLine($"FIX [Constraints:{t.Table}] PK '{old}' -> '{nw}'");
                                    cChanged = true;
                                }
                            }
                            // Identity
                            for (int i = 0; i < (t.IdentityColumns ?? new List<string>()).Count; i++)
                            {
                                var old = t.IdentityColumns[i];
                                var nw = normId(old);
                                if (!string.Equals(nw ?? "", old ?? "", StringComparison.Ordinal))
                                {
                                    t.IdentityColumns[i] = nw;
                                    sb.AppendLine($"FIX [Constraints:{t.Table}] Identity '{old}' -> '{nw}'");
                                    cChanged = true;
                                }
                            }
                            // FKs
                            foreach (var fk in t.ForeignKeys ?? new List<ForeignKeyDef>())
                            {
                                var cold = fk.Column;
                                var cnew = normId(cold);
                                if (!string.Equals(cnew ?? "", cold ?? "", StringComparison.Ordinal))
                                {
                                    fk.Column = cnew;
                                    sb.AppendLine($"FIX [Constraints:{t.Table}] FK Column '{cold}' -> '{cnew}'");
                                    cChanged = true;
                                }
                                var rold = fk.RefColumn;
                                var rnew = normId(rold);
                                if (!string.Equals(rnew ?? "", rold ?? "", StringComparison.Ordinal))
                                {
                                    fk.RefColumn = rnew;
                                    sb.AppendLine($"FIX [Constraints:{t.Table}] FK RefColumn '{rold}' -> '{rnew}'");
                                    cChanged = true;
                                }
                            }
                        }

                        if (cChanged)
                        {
                            File.WriteAllText(constraintsPath, JsonConvert.SerializeObject(idx, Formatting.Indented), Encoding.UTF8);
                            constraintChanges++;
                        }
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine("WARN: Failed to normalize constraints: " + ex.Message);
                        warnings++;
                    }
                }
                else
                {
                    sb.AppendLine("INFO: Constraints file not found (skip): " + constraintsPath);
                }

                sb.Insert(0, $"Naming auto-fix finished. schemaChanges={fileChanges}, constraintChanges={constraintChanges}, warnings={warnings}{Environment.NewLine}");
                File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
                return warnings > 0 ? 1 : 0;
            }
            catch (Exception ex)
            {
                File.WriteAllText(logPath, "Naming auto-fix failed: " + ex.Message);
                return 1;
            }
        }
    }
}