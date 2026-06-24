namespace TrackerSQL.Models
{
    /// <summary>
    /// Interface for entities that can be displayed in lookup dropdowns
    /// with formatted text (e.g., "_Disabled Item")
    /// </summary>
    public interface ILookupEntity
    {
        /// <summary>
        /// Gets the primary key ID of the entity
        /// </summary>
        int GetId();

        /// <summary>
        /// Gets the display text for the entity
        /// </summary>
        string GetDisplayText();

        /// <summary>
        /// Gets whether this entity is enabled/active
        /// Returns true if entity is enabled, false if disabled
        /// Returns null if entity doesn't have an enabled state
        /// </summary>
        bool? IsEnabled();
    }

    /// <summary>
    /// Helper class for formatting lookup display text
    /// </summary>
    public static class LookupFormatter
    {
        private const string DISABLED_PREFIX = "_";

        /// <summary>
        /// Formats display text for lookup dropdowns.
        /// Adds "_" prefix to disabled items for consistent sorting.
        /// </summary>
        /// <param name="text">The display text</param>
        /// <param name="isEnabled">Whether the item is enabled (null = no enable/disable concept)</param>
        /// <returns>Formatted display text</returns>
        public static string FormatLookupText(string text, bool? isEnabled)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // If item is disabled, prefix with "_" to sort disabled items to bottom
            if (isEnabled.HasValue && !isEnabled.Value)
                return DISABLED_PREFIX + text;

            return text;
        }

        /// <summary>
        /// Gets the display text from a lookup entity with proper formatting
        /// </summary>
        public static string GetFormattedDisplayText(ILookupEntity entity)
        {
            if (entity == null)
                return string.Empty;

            return FormatLookupText(entity.GetDisplayText(), entity.IsEnabled());
        }
    }
}
