//------------------------------------------------------------------------------
// TrackerSQL v3.x — ManualSqlHistoryStore
// Persists Manual SQL runs (App_Data/manual-sql-history.json) for XMLtoSQL UI.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Newtonsoft.Json;
using TrackerSQL.Classes;

namespace TrackerSQL.Managers
{
    public sealed class ManualSqlHistoryEntry
    {
        public string Id { get; set; }
        public DateTime RanAt { get; set; }
        public string UserName { get; set; }
        public string Type { get; set; }
        public string Sql { get; set; }
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public int? RowCount { get; set; }
        /// <summary>Optional SELECT snapshot (capped) so history can re-show results without re-running.</summary>
        public ManualSqlResultSnapshot ResultSnapshot { get; set; }

        [JsonIgnore]
        public string RanAtDisplay => RanAt.ToString("yyyy-MM-dd HH:mm:ss");

        [JsonIgnore]
        public string SqlPreview
        {
            get
            {
                string s = (Sql ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Trim();
                while (s.Contains("  "))
                    s = s.Replace("  ", " ");
                return s.Length <= 100 ? s : s.Substring(0, 97) + "...";
            }
        }

        [JsonIgnore]
        public string OkDisplay => Succeeded ? "Yes" : "No";
    }

    public sealed class ManualSqlResultSnapshot
    {
        public List<string> Columns { get; set; } = new List<string>();
        public List<List<string>> Rows { get; set; } = new List<List<string>>();
        public bool Truncated { get; set; }
    }

    public static class ManualSqlHistoryStore
    {
        private const string RelativePath = "~/App_Data/manual-sql-history.json";
        private const int MaxEntries = 75;
        private const int MaxSelectRows = 200;
        private const int MaxCellChars = 200;
        private static readonly object FileLock = new object();

        public static List<ManualSqlHistoryEntry> LoadAll()
        {
            lock (FileLock)
            {
                string path = MapPath();
                if (!File.Exists(path))
                    return new List<ManualSqlHistoryEntry>();

                try
                {
                    string json = File.ReadAllText(path);
                    var list = JsonConvert.DeserializeObject<List<ManualSqlHistoryEntry>>(json);
                    return list ?? new List<ManualSqlHistoryEntry>();
                }
                catch (Exception ex)
                {
                    AppLogger.WriteLog("xmltosql", "ManualSqlHistory LoadAll: " + ex.Message);
                    return new List<ManualSqlHistoryEntry>();
                }
            }
        }

        public static ManualSqlHistoryEntry GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            return LoadAll().FirstOrDefault(e =>
                string.Equals(e.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public static ManualSqlHistoryEntry Add(
            string type,
            string sql,
            bool succeeded,
            string message,
            DataTable selectResult,
            string userName)
        {
            var entry = new ManualSqlHistoryEntry
            {
                Id = Guid.NewGuid().ToString("N"),
                RanAt = TimeZoneUtils.Now(),
                UserName = string.IsNullOrWhiteSpace(userName) ? "(unknown)" : userName.Trim(),
                Type = type ?? "unknown",
                Sql = sql ?? string.Empty,
                Succeeded = succeeded,
                Message = message ?? string.Empty,
                RowCount = selectResult?.Rows.Count,
                ResultSnapshot = BuildSnapshot(selectResult)
            };

            lock (FileLock)
            {
                var list = LoadAllUnlocked();
                list.Insert(0, entry);
                if (list.Count > MaxEntries)
                    list = list.Take(MaxEntries).ToList();
                SaveUnlocked(list);
            }

            AppLogger.WriteLog("xmltosql",
                "Manual SQL history saved id=" + entry.Id
                + " type=" + entry.Type
                + " ok=" + entry.Succeeded
                + " user=" + entry.UserName);

            return entry;
        }

        public static DataTable SnapshotToDataTable(ManualSqlResultSnapshot snap)
        {
            var table = new DataTable();
            if (snap == null || snap.Columns == null || snap.Columns.Count == 0)
                return table;

            foreach (string col in snap.Columns)
            {
                string name = string.IsNullOrWhiteSpace(col) ? "Column" : col;
                string unique = name;
                int n = 1;
                while (table.Columns.Contains(unique))
                    unique = name + "_" + (n++);
                table.Columns.Add(unique, typeof(string));
            }

            if (snap.Rows == null)
                return table;

            foreach (List<string> row in snap.Rows)
            {
                DataRow dr = table.NewRow();
                for (int i = 0; i < table.Columns.Count; i++)
                    dr[i] = (row != null && i < row.Count) ? (row[i] ?? string.Empty) : string.Empty;
                table.Rows.Add(dr);
            }

            return table;
        }

        private static ManualSqlResultSnapshot BuildSnapshot(DataTable table)
        {
            if (table == null || table.Columns.Count == 0)
                return null;

            var snap = new ManualSqlResultSnapshot();
            foreach (DataColumn col in table.Columns)
                snap.Columns.Add(col.ColumnName ?? string.Empty);

            int take = Math.Min(table.Rows.Count, MaxSelectRows);
            snap.Truncated = table.Rows.Count > MaxSelectRows;
            for (int r = 0; r < take; r++)
            {
                var cells = new List<string>(table.Columns.Count);
                for (int c = 0; c < table.Columns.Count; c++)
                {
                    object val = table.Rows[r][c];
                    string text = val == null || val == DBNull.Value ? string.Empty : Convert.ToString(val);
                    if (text != null && text.Length > MaxCellChars)
                        text = text.Substring(0, MaxCellChars - 3) + "...";
                    cells.Add(text ?? string.Empty);
                }
                snap.Rows.Add(cells);
            }

            return snap;
        }

        private static List<ManualSqlHistoryEntry> LoadAllUnlocked()
        {
            string path = MapPath();
            if (!File.Exists(path))
                return new List<ManualSqlHistoryEntry>();

            try
            {
                string json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<List<ManualSqlHistoryEntry>>(json)
                    ?? new List<ManualSqlHistoryEntry>();
            }
            catch
            {
                return new List<ManualSqlHistoryEntry>();
            }
        }

        private static void SaveUnlocked(List<ManualSqlHistoryEntry> list)
        {
            string path = MapPath();
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(path, JsonConvert.SerializeObject(list, Formatting.Indented));
        }

        private static string MapPath()
        {
            if (HttpContext.Current != null)
                return HttpContext.Current.Server.MapPath(RelativePath);
            return HostingEnvironment.MapPath(RelativePath);
        }
    }
}
