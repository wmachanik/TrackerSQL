using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using MigrationRunner.UI;

namespace MigrationRunner
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                // Update banner to include new R option for cleanup, X for DROP ALL, and & for verification
                Console.WriteLine("== Migration Runner (menu v2: options 1..12, A..D, M/MS/N/!, O, R, X, &, Z, $) ==");
                var baseDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');

                var configPath = ResolveConfigPath(baseDir, args);
                if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
                {
                    Console.Error.WriteLine("Missing MigrationConfig.json. Place it next to the exe, in a parent folder, or pass --config <fullPath>.");
                    return 2;
                }

                // Use a stable, writable root: <repo>\Data, else %LOCALAPPDATA%, else bin
                var migrationsDir = ResolveWritableRoot(baseDir);
                if (string.IsNullOrWhiteSpace(migrationsDir))
                {
                    Console.Error.WriteLine("Could not find a writable working folder for Metadata output.");
                    return 2;
                }
                Console.WriteLine("Working folder: " + migrationsDir);

                // Copy existing metadata into Data on first run
                BootstrapMetadata(migrationsDir, baseDir);               

                var configJson = File.ReadAllText(configPath);
                
                var config = JsonConvert.DeserializeObject<MigrationConfig>(configJson);
                if (config == null)
                    throw new InvalidOperationException("Invalid config JSON at: " + configPath);


                var commands = new List<IMenuCommand>
                {
                    new ExportAccessSchemaCommand(migrationsDir, config),
                    new ReviewEditPlanCommand(migrationsDir),
                    new ExportPlanSummaryCsvCommand(migrationsDir),
                    new ExportPlanCsvInteractiveCommand(migrationsDir),
                    new ImportPlanCsvInteractiveCommand(migrationsDir),
                    new BulkRenameDryRunCommand(migrationsDir),
                    new BulkRenameApplyCommand(migrationsDir),
                    new FullPlanReviewCommand(migrationsDir),
                    new BeforeAfterCsvCommand(migrationsDir),
                    new HumanReviewCsvCommand(migrationsDir),
                    new HumanReviewCsvImportCommand(migrationsDir),
                    new PreflightValidateCommand(migrationsDir),

                    new GenerateCreateTablesScriptCommand(migrationsDir),
                    new GenerateForeignKeysScriptCommand(migrationsDir),
                    new GenerateDataMigrationScriptCommand(migrationsDir),
                    new StageAccessToSqlCommand(migrationsDir, config),
                    new ApplyDataMigrationScriptCommand(migrationsDir, config),

                    // Add the custom normalize command (!)
                    new CustomNormalizeCommand(migrationsDir, config),

                    new ApplyDdlScriptsCommand(migrationsDir, config),
                    new OpenSqlFolderCommand(migrationsDir),
                    
                    // Add cleanup command before pipeline commands
                    new CleanupTablesCommand(migrationsDir, config),
                    
                    // Add DROP ALL TABLES command (DANGER!)
                    new DropAllTablesCommand(migrationsDir, config),
                    
                    // Add comprehensive post-migration verification as Option O
                    new PostMigrationVerificationCommand(migrationsDir, config),

                    new ExitCommand(),
                };

                // Add Z command to run range A..! sequentially
                commands.Add(new MigrationRunner.UI.RunRangeCommand(commands, config));  // TODO: Define this class

                // Add pipeline command after RunRange so it appears below Z in the menu
                commands.Add(new MigrationRunner.UI.CreateLoadThenApplyFksCommand(migrationsDir, config));

                var menu = new MenuController("Select an option:", commands);
                return menu.RunLoop();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("ERROR: " + ex);
                return 1;
            }
        }

        private static string ResolveConfigPath(string baseDir, string[] args)
        {
            // 1) --config <path> or --config=path
            for (int i = 0; i < (args?.Length ?? 0); i++)
            {
                var a = args[i] ?? "";
                if (a.Equals("--config", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    var p = args[i + 1];
                    if (!string.IsNullOrWhiteSpace(p)) return Path.GetFullPath(p);
                }
                else if (a.StartsWith("--config=", StringComparison.OrdinalIgnoreCase))
                {
                    var p = a.Substring("--config=".Length).Trim();
                    if (!string.IsNullOrWhiteSpace(p)) return Path.GetFullPath(p);
                }
            }

            // 2) exe folder
            var here = Path.Combine(baseDir, "MigrationConfig.json");
            if (File.Exists(here)) return here;

            // 3) walk up to 7 parents; probe both direct and sibling "Migrations\\MigrationConfig.json"
            var dir = new DirectoryInfo(baseDir);
            for (int up = 0; up < 7 && dir != null; up++, dir = dir.Parent)
            {
                var direct = Path.Combine(dir.FullName, "MigrationConfig.json");
                if (File.Exists(direct)) return direct;

                var siblingMigrations = Path.Combine(dir.FullName, "Migrations", "MigrationConfig.json");
                if (File.Exists(siblingMigrations)) return siblingMigrations;
            }

            return null;
        }

        // Working root preference:
        //  1) <repo>\Data (if repo root detected and writable)
        //  2) %LOCALAPPDATA%\TrackerSQL\MigrationRunner
        //  3) baseDir (bin) if writable
        private static string ResolveWritableRoot(string baseDir)
        {
            string repoRoot = FindRepoRoot(baseDir);

            // 1) <repo>\Data
            if (!string.IsNullOrWhiteSpace(repoRoot))
            {
                var dataRoot = Path.Combine(repoRoot, "Data");
                if (EnsureWritable(Path.Combine(dataRoot, "Metadata")))
                    return dataRoot;
            }

            // 2) LocalAppData
            var lad = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!string.IsNullOrWhiteSpace(lad))
            {
                var appRoot = Path.Combine(lad, "TrackerSQL", "MigrationRunner");
                if (EnsureWritable(Path.Combine(appRoot, "Metadata")))
                    return appRoot;
            }

            // 3) bin folder
            if (EnsureWritable(Path.Combine(baseDir, "Metadata")))
                return baseDir;

            return null;
        }

        private static string FindRepoRoot(string baseDir)
        {
            var dir = new DirectoryInfo(baseDir);
            for (int up = 0; up < 8 && dir != null; up++, dir = dir.Parent)
            {
                var git = Path.Combine(dir.FullName, ".git");
                if (Directory.Exists(git)) return dir.FullName;
            }
            return null;
        }

        private static bool EnsureWritable(string folder)
        {
            try
            {
                Directory.CreateDirectory(folder);
                var probe = Path.Combine(folder, ".write.test");
                File.WriteAllText(probe, "ok");
                File.Delete(probe);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Copy existing metadata (if found) into the new working Data folder
        private static void BootstrapMetadata(string workingRoot, string baseDir)
        {
            var targetAccess = Path.Combine(workingRoot, "Metadata", "AccessSchema");
            var targetEdits = Path.Combine(workingRoot, "Metadata", "PlanEdits");

            if (Directory.Exists(targetAccess)) return; // already set up

            var repoRoot = FindRepoRoot(baseDir);

            // Known old locations to pull from
            var candidates = new List<string>
            {
                Path.Combine(baseDir, "Metadata"),                        // bin\...\Metadata
                Path.Combine(baseDir, "Migrations", "Metadata"),          // bin\...\Migrations\Metadata
            };

            // Also check repo-level legacy locations
            if (!string.IsNullOrWhiteSpace(repoRoot))
            {
                candidates.Add(Path.Combine(repoRoot, "Migrations", "Metadata"));
                candidates.Add(Path.Combine(repoRoot, "Metadata"));
            }

            foreach (var cand in candidates)
            {
                try
                {
                    var srcAccess = Path.Combine(cand, "AccessSchema");
                    var srcEdits  = Path.Combine(cand, "PlanEdits");
                    if (Directory.Exists(srcAccess) && !Directory.Exists(targetAccess))
                    {
                        CopyDirectory(srcAccess, targetAccess);
                        Console.WriteLine("Copied AccessSchema from: " + srcAccess);
                    }
                    if (Directory.Exists(srcEdits) && !Directory.Exists(targetEdits))
                    {
                        CopyDirectory(srcEdits, targetEdits);
                        Console.WriteLine("Copied PlanEdits from: " + srcEdits);
                    }
                    if (Directory.Exists(targetAccess)) break;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Metadata bootstrap skipped for '" + cand + "': " + ex.Message);
                }
            }
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);
            foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.TopDirectoryOnly))
            {
                var name = Path.GetFileName(file);
                File.Copy(file, Path.Combine(destDir, name), overwrite: true);
            }
            foreach (var dir in Directory.GetDirectories(sourceDir, "*", SearchOption.TopDirectoryOnly))
            {
                var name = Path.GetFileName(dir);
                CopyDirectory(dir, Path.Combine(destDir, name));
            }
        }
    }
}