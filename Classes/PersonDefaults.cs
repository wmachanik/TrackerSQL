using TrackerSQL.Repositories;

namespace TrackerSQL.Classes
{
    /// <summary>System people defaults (PeopleTbl) — lazy ensure on first use.</summary>
    public static class PersonDefaults
    {
        public static int GetDefaultSalesAgentId()
        {
            return new PersonsRepository().GetOrEnsureDefaultSalesAgentId();
        }
    }
}
