using System;

namespace TrackerSQL.Models
{
    /// <summary>
    /// Woo global product attribute (parent). When UseForVariants is set, options are pulled
    /// and mapped to Tracker order-line fields via ranked cascades:
    /// Item (SKU) comes from product/variation mapping; attributes supply Qty, Packaging, Notes.
    /// Rank 0 = does not set that field. Rank 1 = try first; 2 = if primary missing; etc.
    /// Tracker Packaging currently also carries grind/prep (legacy) — map Woo Prep → PackRank.
    /// </summary>
    public class WooAttributeParent
    {
        public const int RankOff = 0;
        public const int RankMax = 5;

        public int ParentID { get; set; }
        public long WooAttributeId { get; set; }
        public string AttributeName { get; set; }
        public string Slug { get; set; }
        public bool UseForVariants { get; set; }
        public int TermCount { get; set; }

        /// <summary>0 = off; 1 = primary qty source; 2 = secondary; …</summary>
        public int QtyRank { get; set; }

        /// <summary>0 = off; 1 = primary packaging source; 2 = secondary; …</summary>
        public int PackRank { get; set; }

        /// <summary>0 = off; 1..n = append to order notes (lower first).</summary>
        public int NoteRank { get; set; }

        /// <summary>Legacy column — ignored by resolve; kept for schema compatibility.</summary>
        public int ResolvePriority { get; set; } = 100;

        /// <summary>Legacy column — migrated into ranks on read if ranks are unset.</summary>
        public string ImportChannel { get; set; }

        public bool ContributesQty => NormalizeRank(QtyRank) > 0;
        public bool ContributesPack => NormalizeRank(PackRank) > 0;
        public bool ContributesNote => NormalizeRank(NoteRank) > 0;
        public bool ContributesLine => ContributesQty || ContributesPack;
        public bool ContributesAnything => ContributesLine || ContributesNote;

        /// <summary>Sort key for UI labels (qty, then pack, then note).</summary>
        public int DisplaySortRank
        {
            get
            {
                int q = NormalizeRank(QtyRank);
                int p = NormalizeRank(PackRank);
                int n = NormalizeRank(NoteRank);
                int best = int.MaxValue;
                if (q > 0) best = Math.Min(best, q);
                if (p > 0) best = Math.Min(best, p);
                if (n > 0) best = Math.Min(best, n);
                return best == int.MaxValue ? 100 : best;
            }
        }

        public static int NormalizeRank(int rank)
        {
            if (rank < 0) return RankOff;
            if (rank > RankMax) return RankMax;
            return rank;
        }

        public static int ParseRank(string selectedValue)
        {
            int n;
            if (string.IsNullOrWhiteSpace(selectedValue) || !int.TryParse(selectedValue.Trim(), out n))
                return RankOff;
            return NormalizeRank(n);
        }

        /// <summary>
        /// If new rank columns are still zero, infer from legacy ImportChannel once.
        /// </summary>
        public void ApplyLegacyChannelIfNeeded()
        {
            if (ContributesAnything)
                return;
            string c = (ImportChannel ?? string.Empty).Trim();
            if (c.Length == 0)
                return;
            if (string.Equals(c, "Notes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(c, "OrderNotes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(c, "ToNotes", StringComparison.OrdinalIgnoreCase))
            {
                NoteRank = 1;
                return;
            }
            if (string.Equals(c, "PackWins", StringComparison.OrdinalIgnoreCase)
                || c.IndexOf("pack wins", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                PackRank = 1;
                return;
            }
            if (string.Equals(c, "QtyPackFallback", StringComparison.OrdinalIgnoreCase)
                || c.IndexOf("pack if no", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                QtyRank = 1;
                PackRank = 2;
                return;
            }
            if (string.Equals(c, "Line", StringComparison.OrdinalIgnoreCase)
                || c.IndexOf("qty", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                QtyRank = 1;
                PackRank = 1;
            }
        }

        /// <summary>MapRole for pulled option rows based on this parent's ranks.</summary>
        public string DefaultMapRole()
        {
            bool q = ContributesQty;
            bool p = ContributesPack;
            bool n = ContributesNote;
            if (n && !q && !p)
                return WooAttributeMapRoles.NotesOnly;
            if (q && p)
                return WooAttributeMapRoles.Both;
            if (q)
                return WooAttributeMapRoles.QtyOnly;
            if (p)
                return WooAttributeMapRoles.PackagingOnly;
            return WooAttributeMapRoles.NotesOnly;
        }
    }
}
