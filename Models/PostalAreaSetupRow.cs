using System;
using System.Collections.Generic;

namespace TrackerSQL.Models
{
    [Serializable]
    public class PostalAreaSetupRow
    {
        public int AreaID { get; set; }
        public string AreaName { get; set; }
        public int? DefaultPreferredAgentID { get; set; }
        public string PostalRanges { get; set; }
        public string SuggestedRanges { get; set; }
        public int SuggestedCodeCount { get; set; }
        public string DispatchHint { get; set; }
        public string Status { get; set; }
        public bool FillUnmapped { get; set; }
        public string SuggestedMatchNote { get; set; }
    }

    [Serializable]
    public class PostalGapGroup
    {
        public int SuggestedAreaID { get; set; }
        public string SuggestedAreaName { get; set; }
        public int CodeCount { get; set; }
        public string PlaceSummary { get; set; }
        public string RangeText { get; set; }
        public string Reason { get; set; }
        public string MatchNote { get; set; }
    }

    [Serializable]
    public class PostalConflictGroup
    {
        public string AreaNames { get; set; }
        public int CodeCount { get; set; }
        public string PlaceSummary { get; set; }
        public string RangeText { get; set; }
        public string Kind { get; set; }
    }

    [Serializable]
    public class PostalGapAnalysis
    {
        public int ReferenceCodes { get; set; }
        public int MappedCodes { get; set; }
        public int UnmappedCodes { get; set; }
        public int OverlapCodes { get; set; }
        public int CatchAllLeftovers { get; set; }
        public string CatchAllAreaName { get; set; }
        public string Summary { get; set; }
        public List<PostalGapGroup> Groups { get; set; }
        public List<PostalConflictGroup> Conflicts { get; set; }
    }

    [Serializable]
    public class ContactPostalSuggestion
    {
        public int ContactID { get; set; }
        public string CompanyName { get; set; }
        public string BillingAddress { get; set; }
        public int? AreaID { get; set; }
        public string AreaName { get; set; }
        public string SuggestedPostalCode { get; set; }
        public string MatchPlace { get; set; }
        public string Reason { get; set; }
        public int Confidence { get; set; }
        public bool Selected { get; set; }
    }
}
