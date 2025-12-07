using TrackerDotNet.Classes.Poco;
namespace TrackerDotNet.Classes.Sql
{
    public class PaymentTermsRepository : RepositoryBase<PaymentTerm>
    {
        protected override string TableName => "PaymentTermsTbl";
        protected override string KeyColumn => "PaymentTermID";

        protected override string CoreColumns => "PaymentTermID, PaymentTermDesc, PaymentDays, DayOfMonth, UseDays, Enabled, Notes";
        protected override string LookupColumns => "PaymentTermID, PaymentTermDesc";
    }
}
