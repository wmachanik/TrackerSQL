using System;
using System.IO;
using System.Web;
using System.Web.Hosting;
using Newtonsoft.Json.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// The Android app offered on this site: Downloads/QuaffeeDriver.apk plus Downloads/QuaffeeDriver.json
    /// ({ "version": "0.5.1", "versionCode": 6 }) written next to it by TrackerDriver "npm run apk".
    /// The app compares the version with its own and offers Update.
    /// </summary>
    public static class DriverAppReleaseManager
    {
        public const string ApkVirtualPath = "~/Downloads/QuaffeeDriver.apk";
        public const string InfoVirtualPath = "~/Downloads/QuaffeeDriver.json";

        private static readonly object Sync = new object();
        private static DateTime _infoStamp;
        private static MobileAppRelease _cached;

        /// <summary>Null when the APK or its version file is missing or unreadable.</summary>
        public static MobileAppRelease Latest()
        {
            return ReadRelease(requireApkNotOlderThanJson: false);
        }

        /// <summary>
        /// Like <see cref="Latest"/> but null when QuaffeeDriver.json was updated without replacing the APK
        /// (avoids offering an APK download whose file is still an older build).
        /// </summary>
        public static MobileAppRelease ApkForUpdateOffer()
        {
            return ReadRelease(requireApkNotOlderThanJson: true);
        }

        /// <summary>The web app deployed under /driver/ (driver/version.json from npm run build:site).</summary>
        public static MobileWebRelease WebLatest()
        {
            try
            {
                string path = HostingEnvironment.MapPath(WebVersionVirtualPath);
                if (path == null || !File.Exists(path))
                    return null;

                JObject info = JObject.Parse(File.ReadAllText(path));
                string version = ((string)info["version"] ?? string.Empty).Trim();
                if (version.Length == 0 || version.Length > 20)
                    return null;
                return new MobileWebRelease { Version = version };
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Driver web version file could not be read: " + ex.Message);
                return null;
            }
        }

        public const string WebVersionVirtualPath = "~/driver/version.json";

        private static MobileAppRelease ReadRelease(bool requireApkNotOlderThanJson)
        {
            try
            {
                string apkPath = HostingEnvironment.MapPath(ApkVirtualPath);
                string infoPath = HostingEnvironment.MapPath(InfoVirtualPath);
                if (apkPath == null || infoPath == null || !File.Exists(apkPath) || !File.Exists(infoPath))
                    return null;

                DateTime jsonStamp = File.GetLastWriteTimeUtc(infoPath);
                if (requireApkNotOlderThanJson)
                {
                    DateTime apkStamp = File.GetLastWriteTimeUtc(apkPath);
                    if (jsonStamp > apkStamp.AddSeconds(30))
                        return null;
                }

                DateTime stamp = jsonStamp;
                lock (Sync)
                {
                    if (!requireApkNotOlderThanJson && _cached != null && stamp == _infoStamp)
                        return _cached;

                    JObject info = JObject.Parse(File.ReadAllText(infoPath));
                    string version = ((string)info["version"] ?? string.Empty).Trim();
                    if (version.Length == 0 || version.Length > 20)
                        return null;
                    var release = new MobileAppRelease
                    {
                        Version = version,
                        VersionCode = (int?)info["versionCode"] ?? 0,
                        Url = VirtualPathUtility.ToAbsolute(ApkVirtualPath) + "?v=" + HttpUtility.UrlEncode(version)
                    };
                    if (!requireApkNotOlderThanJson)
                    {
                        _cached = release;
                        _infoStamp = stamp;
                    }
                    return release;
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Driver app version file could not be read: " + ex.Message);
                return null;
            }
        }
    }
}
