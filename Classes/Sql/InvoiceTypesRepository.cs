using TrackerDotNet.Classes.Poco;
namespace TrackerDotNet.Classes.Sql
{
    public class InvoiceTypesRepository : RepositoryBase<InvoiceType>
    {
        protected override string TableName => "InvoiceTypesTbl";
        protected override string KeyColumn => "InvoiceTypeID";

        // Minimal/core columns for targeted fetches
        protected override string CoreColumns => "InvoiceTypeID, InvoiceTypeDesc, Enabled";
        // Lookup (id + display text)
        protected override string LookupColumns => "InvoiceTypeID, InvoiceTypeDesc";
    }
}
