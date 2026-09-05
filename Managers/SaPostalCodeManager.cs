using System;

using System.Collections.Generic;

using System.Globalization;

using System.IO;

using System.Linq;

using System.Text;

using System.Web;

using TrackerSQL.Classes;

using TrackerSQL.Models;

using TrackerSQL.Repositories;



namespace TrackerSQL.Managers

{

    public class SaPostalCodeManager

    {

        public const string DefaultCsvFileName = "geo-south-africa-postal_EN.csv";



        private readonly SaPostalCodeRepository _repo = new SaPostalCodeRepository();

        private readonly WooAreaDeliveryDefaultRepository _areaRepo = new WooAreaDeliveryDefaultRepository();

        private readonly AreasRepository _areasRepo = new AreasRepository();



        public int GetReferenceRowCount()

        {

            if (!_repo.TableExists())

                return 0;

            return _repo.GetRowCount();

        }



        public class ImportResult

        {

            public bool Succeeded { get; set; }

            public int RowsImported { get; set; }

            public int RowsSkipped { get; set; }

            public string Message { get; set; }

        }



        public ImportResult ImportFromDefaultCsv(string updatedBy)

        {

            string path = ResolveCsvPath(DefaultCsvFileName);

            return ImportFromCsv(path, updatedBy);

        }



        public ImportResult ImportFromCsv(string csvPath, string updatedBy)

        {

            var result = new ImportResult();

            if (!File.Exists(csvPath))

            {

                result.Message = "CSV not found: " + csvPath;

                return result;

            }



            new PostalSchemaInstaller().EnsureSchema();

            new WooCommerceSettingsManager().EnsureSchema();

            var rows = ParseCsv(csvPath, out int skipped);

            _repo.TruncateAll();

            int n = _repo.BulkInsert(rows);

            result.Succeeded = true;

            result.RowsImported = n;

            result.RowsSkipped = skipped;

            result.Message = "Imported " + n + " postal rows (" + skipped + " skipped).";

            AppLogger.WriteLog("system", result.Message, updatedBy);

            return result;

        }



        public List<SaPostalCode> Search(string query, int maxRows = 200)

        {

            if (!_repo.TableExists())

                return new List<SaPostalCode>();

            return _repo.Search(query, maxRows);

        }



        /// <summary>Build Woo-style range text from place-name filter(s).</summary>
        public string BuildRangeTextForPlaces(IEnumerable<string> placeNames, out int codeCount)
        {
            int bridged;
            return BuildRangeTextForPlaces(placeNames, out codeCount, out bridged);
        }

        public string BuildRangeTextForPlaces(IEnumerable<string> placeNames, out int codeCount, out int bridgedMissing)
        {
            var codes = _repo.GetDistinctCodesForPlaceNames(placeNames);
            codeCount = codes.Count;
            return PostalRangeFormatter.CollapseCodesToRangeText(codes, out bridgedMissing);
        }



        public string BuildRangeTextForArea(int areaId, string placeFilter, out int codeCount, out string matchSummary)

        {

            var names = ResolvePlaceNames(areaId, placeFilter);

            matchSummary = names.Count == 0

                ? "(no place names to match)"

                : string.Join(", ", names);

            return BuildRangeTextForPlaces(names, out codeCount);

        }



        public void ApplyRangesToArea(int areaId, string rangeText, int? personId, string updatedBy)

        {

            if (areaId <= 0)

                throw new ArgumentOutOfRangeException(nameof(areaId));

            _areaRepo.Upsert(areaId, personId, rangeText, updatedBy);

            AppLogger.WriteLog("system", "Applied postal ranges to area #" + areaId, updatedBy);

        }



        public List<WooAreaDeliveryDefault> GetAreasWithSettings()

        {

            return new WooCommerceAreaMappingManager().GetAreaDeliveryDefaults();

        }



        private List<string> ResolvePlaceNames(int areaId, string placeFilter)

        {

            var names = new List<string>();

            if (!string.IsNullOrWhiteSpace(placeFilter))

            {

                foreach (string part in placeFilter.Split(new[] { ';', ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))

                {

                    string t = part.Trim();

                    if (t.Length > 0)

                        names.Add(t);

                }

                return names;

            }



            var area = _areasRepo.GetById(areaId);

            if (area != null && !string.IsNullOrWhiteSpace(area.AreaName))

                names.Add(area.AreaName.Trim());

            return names;

        }



        public static List<SaPostalCode> ParseCsv(string csvPath, out int skipped)

        {

            skipped = 0;

            var list = new List<SaPostalCode>();

            foreach (string line in File.ReadAllLines(csvPath, Encoding.UTF8))

            {

                if (string.IsNullOrWhiteSpace(line))

                    continue;

                if (line.StartsWith("\"Postal Code\"", StringComparison.OrdinalIgnoreCase))

                    continue;



                if (!TryParseCsvLine(line, out SaPostalCode row))

                {

                    skipped++;

                    continue;

                }

                list.Add(row);

            }

            return list;

        }



        private static bool TryParseCsvLine(string line, out SaPostalCode row)

        {

            row = null;

            var fields = SplitCsvLine(line);

            if (fields.Count < 2)

                return false;



            string codeText = fields[0].Trim().Trim('"');

            if (!int.TryParse(codeText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int code) || code <= 0)

                return false;



            string place = fields[1].Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(place))

                return false;



            decimal? lat = null;

            decimal? lng = null;

            if (fields.Count > 4)

            {

                if (decimal.TryParse(fields[4].Trim().Trim('"'), NumberStyles.Float, CultureInfo.InvariantCulture, out decimal la))

                    lat = la;

                if (fields.Count > 5 && decimal.TryParse(fields[5].Trim().Trim('"'), NumberStyles.Float, CultureInfo.InvariantCulture, out decimal lo))

                    lng = lo;

            }



            row = new SaPostalCode

            {

                PostalCode = code,

                PlaceName = place,

                Province = fields.Count > 2 ? NullIfEmpty(fields[2]) : null,

                Municipality = fields.Count > 3 ? NullIfEmpty(fields[3]) : null,

                Latitude = lat,

                Longitude = lng

            };

            return true;

        }



        private static List<string> SplitCsvLine(string line)

        {

            var fields = new List<string>();

            var sb = new StringBuilder();

            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)

            {

                char c = line[i];

                if (c == '"')

                {

                    inQuotes = !inQuotes;

                    continue;

                }

                if (c == ',' && !inQuotes)

                {

                    fields.Add(sb.ToString());

                    sb.Clear();

                    continue;

                }

                sb.Append(c);

            }

            fields.Add(sb.ToString());

            return fields;

        }



        private static string NullIfEmpty(string s)

        {

            if (string.IsNullOrWhiteSpace(s))

                return null;

            return s.Trim().Trim('"');

        }



        private static string ResolveCsvPath(string fileName)

        {

            if (HttpContext.Current != null)

                return HttpContext.Current.Server.MapPath("~/Data/" + fileName);

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", fileName);

        }

    }



    /// <summary>Shared formatter: list of 4-digit codes → 7800...7806;7810.
    /// Consecutive codes form a range; a single missing code between two known codes
    /// is bridged (8145, 8147 → 8145...8147) because the SA table has holes.</summary>
    public static class PostalRangeFormatter
    {
        public static string CollapseCodesToRangeText(IEnumerable<int> codes)
        {
            int bridged;
            return CollapseCodesToRangeText(codes, out bridged);
        }

        public static string CollapseCodesToRangeText(IEnumerable<int> codes, out int bridgedMissing)
        {
            bridgedMissing = 0;
            if (codes == null)
                return string.Empty;

            var sorted = codes.Where(c => c > 0).Distinct().OrderBy(c => c).ToList();
            if (sorted.Count == 0)
                return string.Empty;

            var parts = new List<string>();
            int rangeStart = sorted[0];
            int rangeEnd = sorted[0];

            for (int i = 1; i < sorted.Count; i++)
            {
                int next = sorted[i];
                if (next == rangeEnd + 1)
                {
                    rangeEnd = next;
                    continue;
                }
                // One hole in the reference table — still one Woo range.
                if (next == rangeEnd + 2)
                {
                    bridgedMissing++;
                    rangeEnd = next;
                    continue;
                }

                parts.Add(FormatRange(rangeStart, rangeEnd));
                rangeStart = rangeEnd = next;
            }
            parts.Add(FormatRange(rangeStart, rangeEnd));
            return string.Join(";", parts);
        }

        public static string BridgedGapNote(int bridgedMissing)
        {
            if (bridgedMissing <= 0)
                return string.Empty;
            return "Joined with ... across " + bridgedMissing
                + " code(s) that are not in the SA table (normal — the list has holes).";
        }

        private static string FormatRange(int from, int to)
        {
            if (from == to)
                return from.ToString("0000", CultureInfo.InvariantCulture);
            return from.ToString("0000", CultureInfo.InvariantCulture)
                + "..."
                + to.ToString("0000", CultureInfo.InvariantCulture);
        }
    }

}

