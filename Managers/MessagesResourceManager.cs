using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Web;
using System.Xml.Linq;
using TrackerDotNet.Classes;

namespace TrackerDotNet.Managers
{
    /// <summary>
    /// Unified message manager:
    /// 1. Base values from ~/Resources/Messages.resx if present; else from compiled global resource "Messages".
    /// 2. Text overrides (only changed values) stored in ~/App_Data/MessagesOverrides.xml.
    /// </summary>
    public class MessagesResourceManager
    {
        private static readonly object _lock = new object();

        private const string PhysicalResxRelative = "~/Resources/Messages.resx";
        private const string OverridesRelative = "~/App_Data/MessagesOverrides.xml";
        private const string GlobalResourceBase = "Messages"; // App_GlobalResources/Messages.resx compiled name

        #region Model
        public class MessageRecord
        {
            public string Key;
            public string BaseValue;
            public string OverrideValue;
            public string EffectiveValue => OverrideValue ?? BaseValue;
            public bool IsOverridden => OverrideValue != null;
        }
        #endregion

        private string MapPath(string rel)
        {
            var ctx = HttpContext.Current;
            if (ctx == null) throw new InvalidOperationException("No HttpContext.");
            return ctx.Server.MapPath(rel);
        }

        public Dictionary<string, MessageRecord> LoadAll()
        {
            var baseEntries = LoadBaseMessages();
            var overrides = LoadOverrides();

            // Merge
            foreach (var ov in overrides)
            {
                if (baseEntries.ContainsKey(ov.Key))
                {
                    baseEntries[ov.Key].OverrideValue = ov.Value;
                }
                else
                {
                    // Ignore unknown keys to keep system clean (could allow custom if needed)
                    AppLogger.WriteLog(SystemConstants.LogTypes.System,
                        "MessagesResourceManager: Override key '" + ov.Key + "' ignored (not in base).");
                }
            }

            return baseEntries
                .OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(k => k.Key, v => v.Value, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns effective key->value map (what UI expects).
        /// </summary>
        public Dictionary<string, string> LoadEffective()
        {
            return LoadAll().ToDictionary(k => k.Key, v => v.Value.EffectiveValue, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Update (override) an existing key's value. Rejects unknown keys.
        /// </summary>
        public void Update(string key, string newValue)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be empty.");

            lock (_lock)
            {
                var all = LoadAll();
                if (!all.ContainsKey(key))
                    throw new KeyNotFoundException("Key '" + key + "' not found in base messages.");

                var rec = all[key];
                string baseVal = rec.BaseValue ?? string.Empty;
                string newVal = newValue ?? string.Empty;

                if (string.Equals(baseVal, newVal, StringComparison.Ordinal))
                {
                    // Remove override if it exists (revert to base)
                    rec.OverrideValue = null;
                }
                else
                {
                    rec.OverrideValue = newVal;
                }

                // Persist only keys whose override differs from base
                var toPersist = all.Values
                    .Where(r => r.OverrideValue != null && !string.Equals(r.BaseValue, r.OverrideValue, StringComparison.Ordinal))
                    .OrderBy(r => r.Key, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(r => r.Key, r => r.OverrideValue, StringComparer.OrdinalIgnoreCase);

                SaveOverrides(toPersist);
            }
        }

        #region Base Loaders

        private Dictionary<string, MessageRecord> LoadBaseMessages()
        {
            // Prefer physical file if present
            var dict = LoadPhysicalResx();
            if (dict.Count > 0)
                return dict;

            // Else try compiled global resources
            return LoadCompiledGlobalMessages();
        }

        private Dictionary<string, MessageRecord> LoadPhysicalResx()
        {
            var result = new Dictionary<string, MessageRecord>(StringComparer.OrdinalIgnoreCase);
            string path = MapPath(PhysicalResxRelative);
            if (!File.Exists(path))
                return result;

            try
            {
                var doc = XDocument.Load(path);
                var root = doc.Root;
                if (root == null) return result;
                foreach (var data in root.Elements("data"))
                {
                    var nameAttr = data.Attribute("name");
                    if (nameAttr == null) continue;
                    var valEl = data.Element("value");
                    string key = nameAttr.Value;
                    string val = valEl != null ? valEl.Value : string.Empty;
                    if (!result.ContainsKey(key))
                    {
                        result[key] = new MessageRecord
                        {
                            Key = key,
                            BaseValue = val
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "MessagesResourceManager: Error reading physical resx: " + ex.Message);
            }
            return result;
        }

        private Dictionary<string, MessageRecord> LoadCompiledGlobalMessages()
        {
            var result = new Dictionary<string, MessageRecord>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var rm = ResolveGlobalResourceManager(GlobalResourceBase);
                if (rm == null) return result;

                var rs = rm.GetResourceSet(System.Globalization.CultureInfo.CurrentUICulture, true, true);
                if (rs == null) return result;

                foreach (System.Collections.DictionaryEntry entry in rs)
                {
                    string key = entry.Key.ToString();
                    string val = entry.Value == null ? string.Empty : entry.Value.ToString();
                    if (!result.ContainsKey(key))
                    {
                        result[key] = new MessageRecord
                        {
                            Key = key,
                            BaseValue = val
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "MessagesResourceManager: Error loading compiled resources: " + ex.Message);
            }
            return result;
        }

        private ResourceManager ResolveGlobalResourceManager(string baseName)
        {
            // Existing heuristic: look for strongly typed resource wrapper classes
            var candidates = new[]
            {
                "Resources." + baseName,
                baseName
            };
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var cand in candidates)
                {
                    var t = asm.GetType(cand, false, true);
                    if (t == null) continue;
                    var prop = t.GetProperty("ResourceManager",
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (prop != null)
                    {
                        var rm = prop.GetValue(null, null) as ResourceManager;
                        if (rm != null) return rm;
                    }
                }
            }

            // Fallback: scan manifest resource names for an embedded *.Messages.resources
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                string[] names;
                try
                {
                    names = asm.GetManifestResourceNames();
                }
                catch
                {
                    continue; // skip dynamic / restricted assemblies
                }

                foreach (var resName in names)
                {
                    // Match ...Messages.resources (case-insensitive)
                    if (!resName.EndsWith("." + baseName + ".resources", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Derive the base name (strip the .resources suffix)
                    var baseNameCandidate = resName.Substring(0, resName.Length - ".resources".Length);

                    try
                    {
                        var rm = new ResourceManager(baseNameCandidate, asm);
                        // Quick sanity check: does it return a ResourceSet?
                        var rs = rm.GetResourceSet(CultureInfo.InvariantCulture, true, false);
                        if (rs != null)
                        {
                            AppLogger.WriteLog(SystemConstants.LogTypes.System,
                                "MessagesResourceManager: Resolved compiled resource via manifest '" + resName + "'");
                            return rm;
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.System,
                            "MessagesResourceManager: Manifest probe failed for '" + resName + "': " + ex.Message);
                    }
                }
            }

            AppLogger.WriteLog(SystemConstants.LogTypes.System,
                "MessagesResourceManager: No compiled global resource manager found for '" + baseName + "'");
            return null;
        }
        #endregion

        #region Overrides

        private Dictionary<string, string> LoadOverrides()
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string path = MapPath(OverridesRelative);
            if (!File.Exists(path)) return dict;

            try
            {
                var doc = XDocument.Load(path);
                var root = doc.Root;
                if (root == null) return dict;
                foreach (var msg in root.Elements("msg"))
                {
                    var keyAttr = msg.Attribute("key");
                    if (keyAttr == null) continue;
                    dict[keyAttr.Value] = msg.Value ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "MessagesResourceManager: Override load failed: " + ex.Message);
            }
            return dict;
        }

        private void SaveOverrides(Dictionary<string, string> overrides)
        {
            string path = MapPath(OverridesRelative);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");

            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("overrides",
                    overrides
                        .OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase)
                        .Select(kv => new XElement("msg",
                            new XAttribute("key", kv.Key),
                            kv.Value ?? string.Empty))
                )
            );

            var tmp = path + ".tmp";
            doc.Save(tmp);
            File.Copy(tmp, path, true);
            File.Delete(tmp);

            AppLogger.WriteLog(SystemConstants.LogTypes.System,
                "MessagesResourceManager: Saved " + overrides.Count + " overrides to " + path);
        }

        #endregion
    }
}