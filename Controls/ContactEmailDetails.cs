//------------------------------------------------------------------------------
// TrackerSQL v3.x — ContactEmailDetails
// Data access / control type: ContactEmailDetails.
//------------------------------------------------------------------------------

using System;
using System.Data;
using TrackerSQL.Classes;

//- only form later versions #nullable disable
namespace TrackerSQL.Controls
{
    [Obsolete("DO NOT USE Comtrols use Models - MIGRATION IN PROGRESS", true)]
    public class ContactEmailDetails
    {
        private const string CONST_SQLGETCONTACTEMAILDETAILS = "SELECT ContactFirstName, ContactLastName, ContactAltFirstName, ContactAltLastName, EmailAddress, AltEmailAddress, CustomerID  FROM CustomersTbl WHERE (CustomerID = ?)";
        private string _FirstName;
        private string _LastName;
        private string _EmailAddress;
        private string _altFirstName;
        private string _altLastName;
        private string _altEmailAddress;

        public ContactEmailDetails()
        {
            this._FirstName = this._LastName = this._EmailAddress = "";
            this._altFirstName = this._altLastName = this._altEmailAddress = "";
        }

        public string FirstName
        {
            get => this._FirstName;
            set => this._FirstName = value;
        }

        public string LastName
        {
            get => this._LastName;
            set => this._LastName = value;
        }

        public string EmailAddress
        {
            get => this._EmailAddress;
            set => this._EmailAddress = value;
        }

        public string altFirstName
        {
            get => this._altFirstName;
            set => this._altFirstName = value;
        }

        public string altLastName
        {
            get => this._altLastName;
            set => this._altLastName = value;
        }

        public string altEmailAddress
        {
            get => this._altEmailAddress;
            set => this._altEmailAddress = value;
        }

        public ContactEmailDetails GetContactsEmailDetails(long pContactID)
        {
            ContactEmailDetails contactsEmailDetails = new ContactEmailDetails();
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pContactID, DbType.Int64, "@CustomerID");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT ContactFirstName, ContactLastName, ContactAltFirstName, ContactAltLastName, EmailAddress, AltEmailAddress, CustomerID  FROM CustomersTbl WHERE (CustomerID = ?)");
            if (dataReader != null)
            {
                if (dataReader.Read())
                {
                    contactsEmailDetails.FirstName = dataReader["ContactFirstName"] == DBNull.Value ? string.Empty : dataReader["ContactFirstName"].ToString();
                    contactsEmailDetails.LastName = dataReader["ContactLastName"] == DBNull.Value ? string.Empty : dataReader["ContactLastName"].ToString();
                    contactsEmailDetails.EmailAddress = dataReader["EmailAddress"] == DBNull.Value ? string.Empty : dataReader["EmailAddress"].ToString();
                    contactsEmailDetails.altFirstName = dataReader["ContactAltFirstName"] == DBNull.Value ? string.Empty : dataReader["ContactAltFirstName"].ToString();
                    contactsEmailDetails.altLastName = dataReader["ContactAltLastName"] == DBNull.Value ? string.Empty : dataReader["ContactAltLastName"].ToString();
                    contactsEmailDetails.altEmailAddress = dataReader["AltEmailAddress"] == DBNull.Value ? string.Empty : dataReader["AltEmailAddress"].ToString();
                }
                dataReader.Close();
            }
            trackerDb.Close();
            return contactsEmailDetails;
        }
    }
}
