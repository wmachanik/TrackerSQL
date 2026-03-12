using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Classes.Poco;
using TrackerSQL.Classes;

namespace TrackerDotNet.Classes.Sql
{
    public class ItemPackagingsRepository : RepositoryBase<ItemPackaging>
    {
        protected override string TableName => "ItemPackagingsTbl";
        protected override string KeyColumn => "ItemPackagingID";
        protected override string CoreColumns => "ItemPackagingID, ItemPackagingDesc, AdditionalNotes, Symbol, Colour, BGColour";
        protected override string LookupColumns => "ItemPackagingID, ItemPackagingDesc";
    }
}
