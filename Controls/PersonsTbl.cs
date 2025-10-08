// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.PersonsTbl
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web.Security;
using TrackerDotNet.Classes;

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class PersonsTbl
    {
        private const string CONST_SQL_SELECT = "SELECT PersonID, Person, Abreviation, Enabled, NormalDeliveryDoW, SecurityUsername FROM PersonsTbl";
        private const string CONST_SQL_INSERT = "INSERT INTO PersonsTbl (Person, Abreviation, Enabled, NormalDeliveryDoW, SecurityUsername) VALUES (?,?,?,?,?) ";
        private const string CONST_SQL_UPDATE = "UPDATE PersonsTbl SET Person = ?, Abreviation = ?, Enabled  = ?, NormalDeliveryDoW = ?, SecurityUsername = ? WHERE (PersonID = ?)";
        private const string CONST_SQL_INSERT_SECNAMENULL = "INSERT INTO PersonsTbl (Person, Abreviation, Enabled, NormalDeliveryDoW) VALUES (?,?,?,?) ";
        private const string CONST_SQL_UPDATE_SECNAMENULL = "UPDATE PersonsTbl SET Person = ?, Abreviation = ?, Enabled  = ?, NormalDeliveryDoW = ? WHERE (PersonID = ?)";
        private const string CONST_SQL_DELETE = "DELETE FROM PersonsTbl WHERE (PersonID = ?)";
        private const string CONST_SQL_GETNORMALDELIVERYDOW_BYID = "SELECT NormalDeliveryDoW FROM PersonsTbl WHERE (PersonID = ?)";
        private const string CONST_SQL_GETPERSONSIDBYABREV = "SELECT PersonID FROM PersonsTbl WHERE (Abreviation LIKE '?')";
        private const string CONST_SQL_GETPERSONSID_BYSECURITYNAME = "SELECT PersonID FROM PersonsTbl WHERE (SecurityUsername = ?)";
        private const string CONST_SQL_GETPERSONSNAME_BYID = "SELECT Person FROM PersonsTbl WHERE (PersonID  = ?)";
        private int _PersonID;
        private string _Person;
        private string _Abreviation;
        private bool _Enabled;
        private int _NormalDeliveryDoW;
        private string _SecurityUsername;

        public PersonsTbl()
        {
            this._PersonID = 0;
            this._Person = string.Empty;
            this._Abreviation = string.Empty;
            this._Enabled = false;
            this._NormalDeliveryDoW = 0;
            this._SecurityUsername = string.Empty;
        }

        public int PersonID
        {
            get => this._PersonID;
            set => this._PersonID = value;
        }

        public string Person
        {
            get => this._Person;
            set => this._Person = value;
        }

        public string Abreviation
        {
            get => this._Abreviation;
            set => this._Abreviation = value;
        }

        public bool Enabled
        {
            get => this._Enabled;
            set => this._Enabled = value;
        }

        public int NormalDeliveryDoW
        {
            get => this._NormalDeliveryDoW;
            set => this._NormalDeliveryDoW = value;
        }

        public string SecurityUsername
        {
            get => this._SecurityUsername;
            set => this._SecurityUsername = value;
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public List<PersonsTbl> GetAll(string SortBy)
        {
            List<PersonsTbl> all = new List<PersonsTbl>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = $"SELECT PersonID, Person, Abreviation, Enabled, NormalDeliveryDoW, SecurityUsername FROM PersonsTbl ORDER BY {(!string.IsNullOrEmpty(SortBy) ? SortBy : "Abreviation")}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new PersonsTbl()
                    {
                        PersonID = dataReader["PersonID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PersonID"]),
                        Person = dataReader["Person"] == DBNull.Value ? string.Empty : dataReader["Person"].ToString(),
                        Abreviation = dataReader["Abreviation"] == DBNull.Value ? string.Empty : dataReader["Abreviation"].ToString(),
                        Enabled = dataReader["Enabled"] != DBNull.Value && Convert.ToBoolean(dataReader["Enabled"]),
                        NormalDeliveryDoW = dataReader["NormalDeliveryDoW"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["NormalDeliveryDoW"]),
                        SecurityUsername = dataReader["SecurityUsername"] == DBNull.Value ? string.Empty : dataReader["SecurityUsername"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }

        [DataObjectMethod(DataObjectMethodType.Insert, true)]
        public string InsertPerson(PersonsTbl pPerson)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pPerson.Person, DbType.String);
            trackerDb.AddParams((object)pPerson.Abreviation);
            trackerDb.AddParams((object)pPerson.Enabled, DbType.Boolean);
            trackerDb.AddParams((object)pPerson.NormalDeliveryDoW, DbType.Int32);
            if (pPerson.SecurityUsername != null)
                trackerDb.AddParams((object)pPerson.SecurityUsername);
            string str = trackerDb.ExecuteNonQuerySQL("INSERT INTO PersonsTbl (Person, Abreviation, Enabled, NormalDeliveryDoW, SecurityUsername) VALUES (?,?,?,?,?) ");
            trackerDb.Close();
            return str;
        }

        [DataObjectMethod(DataObjectMethodType.Update, true)]
        public static string UpdatePerson(PersonsTbl pPerson) => PersonsTbl.UpdatePerson(pPerson, 0);

        [DataObjectMethod(DataObjectMethodType.Update, false)]
        public static string UpdatePerson(PersonsTbl pPerson, int pOrignal_PersonID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pPerson.Person);
            trackerDb.AddParams((object)pPerson.Abreviation);
            trackerDb.AddParams((object)pPerson.Enabled, DbType.Boolean);
            trackerDb.AddParams((object)pPerson.NormalDeliveryDoW, DbType.Int32);
            string strSQL;
            if (pPerson.SecurityUsername == null)
            {
                strSQL = "UPDATE PersonsTbl SET Person = ?, Abreviation = ?, Enabled  = ?, NormalDeliveryDoW = ? WHERE (PersonID = ?)";
            }
            else
            {
                trackerDb.AddParams((object)pPerson.SecurityUsername);
                strSQL = "UPDATE PersonsTbl SET Person = ?, Abreviation = ?, Enabled  = ?, NormalDeliveryDoW = ?, SecurityUsername = ? WHERE (PersonID = ?)";
            }
            if (pOrignal_PersonID > 0)
                trackerDb.AddWhereParams((object)pOrignal_PersonID, DbType.Int32);
            else
                trackerDb.AddWhereParams((object)pPerson.PersonID, DbType.Int32);
            string str = trackerDb.ExecuteNonQuerySQL(strSQL);
            trackerDb.Close();
            return str;
        }

        [DataObjectMethod(DataObjectMethodType.Delete, true)]
        public string DeletePerson(int pPersonID)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pPersonID, DbType.Int32);
            string str = trackerDb.ExecuteNonQuerySQL("DELETE FROM PersonsTbl WHERE (PersonID = ?)");
            trackerDb.Close();
            return str;
        }

        public bool IsNormalDeliveryDoW(int pPersonID, int pDoW)
        {
            bool flag = true;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pPersonID, DbType.Int32);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT NormalDeliveryDoW FROM PersonsTbl WHERE (PersonID = ?)");
            if (dataReader != null)
            {
                if (dataReader.Read())
                {
                    int int32 = dataReader["NormalDeliveryDoW"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["NormalDeliveryDoW"]);
                    if (int32 != 0)
                        flag = int32 == pDoW;
                }
                else
                    dataReader.Close();
            }
            trackerDb.Close();
            return flag;
        }

        public int PersonsIDoFSecurityUsers(string pPersonSecurityName)
        {
            int num = 0;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pPersonSecurityName, DbType.String);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT PersonID FROM PersonsTbl WHERE (SecurityUsername = ?)");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    num = dataReader["PersonID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PersonID"]);
                else
                    dataReader.Close();
            }
            trackerDb.Close();
            return num;
        }

        public int PersonsIDFromAbreviation(string pAbrv)
        {
            int num = 0;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pAbrv, DbType.String);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT PersonID FROM PersonsTbl WHERE (Abreviation LIKE '?')");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    num = dataReader["PersonID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PersonID"].ToString());
                else
                    dataReader.Close();
            }
            trackerDb.Close();
            return num;
        }

        public string PersonsNameFromID(int pPersonsID)
        {
            string str = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pPersonsID, DbType.Int32);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT Person FROM PersonsTbl WHERE (PersonID  = ?)");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    str = dataReader["Person"] == DBNull.Value ? string.Empty : dataReader["Person"].ToString();
                else
                    dataReader.Close();
            }
            trackerDb.Close();
            return str;
        }

        public List<string> SecurityUsersNotInPeopleTbl()
        {
            List<string> stringList = new List<string>();
            foreach (MembershipUser allUser in Membership.GetAllUsers())
            {
                if (this.PersonsIDoFSecurityUsers(allUser.UserName) == 0)
                    stringList.Add(allUser.UserName);
            }
            return stringList;
        }
    }
}