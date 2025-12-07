using System;

namespace MigrationRunner.UI
{
    // Shared RunRange options/state to allow asking once and reusing values across commands
    internal sealed class RunRangeOptions
    {
        public bool SuppressPrompts { get; set; }
        public string TargetConnectionString { get; set; }
        public string AccessConnectionString { get; set; }
        public bool? FullCompare { get; set; }
        public int? SamplesPerTable { get; set; }
        public int? PerTableTimeoutSeconds { get; set; }
        public bool? PersistOrphans { get; set; }
        public bool? SuppressIdentityOnCreate { get; set; }
        public bool? DropExistingOnCreate { get; set; }
    }

    internal static class RunRangeState
    {
        public static RunRangeOptions Current { get; set; }
    }
}
