namespace MigrationRunner
{
    class MigrationConfig
    {
        public string AccessConnectionString { get; set; }
        public string TargetConnectionString { get; set; }
        public string[] AccessTables { get; set; }
        public string[] AccessTableExcludes { get; set; }

        public MigrationConfig()
        {
            AccessTables = new string[0];
            AccessTableExcludes = new string[0];
        }
    }
}