// Decompiled with JetBrains decompiler
// Type: TrackerSQL.DataSets.CustomersDataSet
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using TrackerSQL.Classes;

//- only form later versions #nullable disable
namespace TrackerSQL.DataSets
{
    [DesignerCategory("code")]
    [ToolboxItem(true)]
    [XmlRoot("CustomersDataSet")]
    [HelpKeyword("vs.data.DataSet")]
    [XmlSchemaProvider("GetTypedDataSetSchema")]
    [Serializable]
    public class CustomersDataSet : DataSet
    {
        private CustomersDataSet.CustomersDataTable tableCustomers;
        private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public CustomersDataSet()
        {
            this.BeginInit();
            this.InitClass();
            CollectionChangeEventHandler changeEventHandler = new CollectionChangeEventHandler(this.SchemaChanged);
            base.Tables.CollectionChanged += changeEventHandler;
            base.Relations.CollectionChanged += changeEventHandler;
            this.EndInit();
        }

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        protected CustomersDataSet(SerializationInfo info, StreamingContext context)
          : base(info, context, false)
        {
            if (this.IsBinarySerialized(info, context))
            {
                this.InitVars(false);
                CollectionChangeEventHandler changeEventHandler = new CollectionChangeEventHandler(this.SchemaChanged);
                this.Tables.CollectionChanged += changeEventHandler;
                this.Relations.CollectionChanged += changeEventHandler;
            }
            else
            {
                string s = (string)info.GetValue("XmlSchema", typeof(string));
                if (this.DetermineSchemaSerializationMode(info, context) == SchemaSerializationMode.IncludeSchema)
                {
                    DataSet dataSet = new DataSet();
                    dataSet.ReadXmlSchema((XmlReader)new XmlTextReader((TextReader)new StringReader(s)));
                    if (dataSet.Tables[nameof(Customers)] != null)
                        base.Tables.Add((DataTable)new CustomersDataSet.CustomersDataTable(dataSet.Tables[nameof(Customers)]));
                    this.DataSetName = dataSet.DataSetName;
                    this.Prefix = dataSet.Prefix;
                    this.Namespace = dataSet.Namespace;
                    this.Locale = dataSet.Locale;
                    this.CaseSensitive = dataSet.CaseSensitive;
                    this.EnforceConstraints = dataSet.EnforceConstraints;
                    this.Merge(dataSet, false, MissingSchemaAction.Add);
                    this.InitVars();
                }
                else
                    this.ReadXmlSchema((XmlReader)new XmlTextReader((TextReader)new StringReader(s)));
                this.GetSerializationData(info, context);
                CollectionChangeEventHandler changeEventHandler = new CollectionChangeEventHandler(this.SchemaChanged);
                base.Tables.CollectionChanged += changeEventHandler;
                this.Relations.CollectionChanged += changeEventHandler;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public CustomersDataSet.CustomersDataTable Customers => this.tableCustomers;

        [DebuggerNonUserCode]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public override SchemaSerializationMode SchemaSerializationMode
        {
            get => this._schemaSerializationMode;
            set => this._schemaSerializationMode = value;
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        [DebuggerNonUserCode]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new DataTableCollection Tables => base.Tables;

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DebuggerNonUserCode]
        public new DataRelationCollection Relations => base.Relations;

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        [DebuggerNonUserCode]
        protected override void InitializeDerivedDataSet()
        {
            this.BeginInit();
            this.InitClass();
            this.EndInit();
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        [DebuggerNonUserCode]
        public override DataSet Clone()
        {
            CustomersDataSet customersDataSet = (CustomersDataSet)base.Clone();
            customersDataSet.InitVars();
            customersDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
            return (DataSet)customersDataSet;
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        [DebuggerNonUserCode]
        protected override bool ShouldSerializeTables() => false;

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        protected override bool ShouldSerializeRelations() => false;

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        protected override void ReadXmlSerializable(XmlReader reader)
        {
            if (this.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
            {
                this.Reset();
                DataSet dataSet = new DataSet();
                int num = (int)dataSet.ReadXml(reader);
                if (dataSet.Tables["Customers"] != null)
                    base.Tables.Add((DataTable)new CustomersDataSet.CustomersDataTable(dataSet.Tables["Customers"]));
                this.DataSetName = dataSet.DataSetName;
                this.Prefix = dataSet.Prefix;
                this.Namespace = dataSet.Namespace;
                this.Locale = dataSet.Locale;
                this.CaseSensitive = dataSet.CaseSensitive;
                this.EnforceConstraints = dataSet.EnforceConstraints;
                this.Merge(dataSet, false, MissingSchemaAction.Add);
                this.InitVars();
            }
            else
            {
                int num = (int)this.ReadXml(reader);
                this.InitVars();
            }
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        [DebuggerNonUserCode]
        protected override XmlSchema GetSchemaSerializable()
        {
            MemoryStream memoryStream = new MemoryStream();
            this.WriteXmlSchema((XmlWriter)new XmlTextWriter((Stream)memoryStream, (Encoding)null));
            memoryStream.Position = 0;
            return XmlSchema.Read((XmlReader)new XmlTextReader((Stream)memoryStream), (ValidationEventHandler)null);
        }

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        internal void InitVars() => this.InitVars(true);

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        internal void InitVars(bool initTable)
        {
            this.tableCustomers = (CustomersDataSet.CustomersDataTable)base.Tables["Customers"];
            if (!initTable || this.tableCustomers == null)
                return;
            this.tableCustomers.InitVars();
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        [DebuggerNonUserCode]
        private void InitClass()
        {
            this.DataSetName = nameof(CustomersDataSet);
            this.Prefix = "";
            this.Namespace = "http://tempuri.org/CustomersDataSet.xsd";
            this.EnforceConstraints = true;
            this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
            this.tableCustomers = new CustomersDataSet.CustomersDataTable();
            base.Tables.Add((DataTable)this.tableCustomers);
        }

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        private bool ShouldSerializeCustomers() => false;

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        private void SchemaChanged(object sender, CollectionChangeEventArgs e)
        {
            if (e.Action != CollectionChangeAction.Remove)
                return;
            this.InitVars();
        }

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
        {
            CustomersDataSet customersDataSet = new CustomersDataSet();
            XmlSchemaComplexType typedDataSetSchema = new XmlSchemaComplexType();
            XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
            xmlSchemaSequence.Items.Add((XmlSchemaObject)new XmlSchemaAny()
            {
                Namespace = customersDataSet.Namespace
            });
            typedDataSetSchema.Particle = (XmlSchemaParticle)xmlSchemaSequence;
            XmlSchema schemaSerializable = customersDataSet.GetSchemaSerializable();
            if (xs.Contains(schemaSerializable.TargetNamespace))
            {
                MemoryStream memoryStream1 = new MemoryStream();
                MemoryStream memoryStream2 = new MemoryStream();
                try
                {
                    schemaSerializable.Write((Stream)memoryStream1);
                    IEnumerator enumerator = xs.Schemas(schemaSerializable.TargetNamespace).GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        XmlSchema current = (XmlSchema)enumerator.Current;
                        memoryStream2.SetLength(0L);
                        current.Write((Stream)memoryStream2);
                        if (memoryStream1.Length == memoryStream2.Length)
                        {
                            memoryStream1.Position = 0;
                            memoryStream2.Position = 0;
                            //do
                            //    ;
                            while (memoryStream1.Position != memoryStream1.Length && memoryStream1.ReadByte() == memoryStream2.ReadByte());
                            if (memoryStream1.Position == memoryStream1.Length)
                                return typedDataSetSchema;
                        }
                    }
                }
                finally
                {
                    memoryStream1?.Close();
                    memoryStream2?.Close();
                }
            }
            xs.Add(schemaSerializable);
            return typedDataSetSchema;
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public delegate void CustomersRowChangeEventHandler(
          object sender,
          CustomersDataSet.CustomersRowChangeEvent e);

        [XmlSchemaProvider("GetTypedTableSchema")]
        [Serializable]
        public class CustomersDataTable : TypedTableBase<CustomersDataSet.CustomersRow>
        {
            private DataColumn columnCompanyName;
            private DataColumn columnCustomerID;
            private DataColumn columnContactTitle;
            private DataColumn columnContactFirstName;
            private DataColumn columnContactLastName;
            private DataColumn columnContactAltFirstName;
            private DataColumn columnContactAltLastName;
            private DataColumn columnDepartment;
            private DataColumn columnBillingAddress;
            private DataColumn columnStateOrProvince;
            private DataColumn columnPostalCode;
            private DataColumn columnPhoneNumber;
            private DataColumn columnExtension;
            private DataColumn columnFaxNumber;
            private DataColumn columnCellNumber;
            private DataColumn columnEmailAddress;
            private DataColumn columnAltEmailAddress;
            private DataColumn columnCustomerType;
            private DataColumn columnEquipTypeName;
            private DataColumn columnCoffeePreference;
            private DataColumn columnCity;
            private DataColumn columnItemDesc;
            private DataColumn columnPriPrefQty;
            private DataColumn columnAbbreviation;
            private DataColumn columnMachineSN;
            private DataColumn columnUsesFilter;
            private DataColumn columnautofulfill;
            private DataColumn columnenabled;
            private DataColumn columnPredictionDisabled;
            private DataColumn columnAlwaysSendChkUp;
            private DataColumn columnNormallyResponds;
            private DataColumn columnNotes;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public CustomersDataTable()
            {
                this.TableName = "Customers";
                this.BeginInit();
                this.InitClass();
                this.EndInit();
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            internal CustomersDataTable(DataTable table)
            {
                this.TableName = table.TableName;
                if (table.CaseSensitive != table.DataSet.CaseSensitive)
                    this.CaseSensitive = table.CaseSensitive;
                if (table.Locale.ToString() != table.DataSet.Locale.ToString())
                    this.Locale = table.Locale;
                if (table.Namespace != table.DataSet.Namespace)
                    this.Namespace = table.Namespace;
                this.Prefix = table.Prefix;
                this.MinimumCapacity = table.MinimumCapacity;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            protected CustomersDataTable(SerializationInfo info, StreamingContext context)
              : base(info, context)
            {
                this.InitVars();
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn CompanyNameColumn => this.columnCompanyName;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn CustomerIDColumn => this.columnCustomerID;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn ContactTitleColumn => this.columnContactTitle;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn ContactFirstNameColumn => this.columnContactFirstName;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn ContactLastNameColumn => this.columnContactLastName;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn ContactAltFirstNameColumn => this.columnContactAltFirstName;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn ContactAltLastNameColumn => this.columnContactAltLastName;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn DepartmentColumn => this.columnDepartment;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn BillingAddressColumn => this.columnBillingAddress;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn StateOrProvinceColumn => this.columnStateOrProvince;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn PostalCodeColumn => this.columnPostalCode;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn PhoneNumberColumn => this.columnPhoneNumber;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn ExtensionColumn => this.columnExtension;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn FaxNumberColumn => this.columnFaxNumber;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn CellNumberColumn => this.columnCellNumber;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn EmailAddressColumn => this.columnEmailAddress;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn AltEmailAddressColumn => this.columnAltEmailAddress;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn CustomerTypeColumn => this.columnCustomerType;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn EquipTypeNameColumn => this.columnEquipTypeName;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn CoffeePreferenceColumn => this.columnCoffeePreference;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn CityColumn => this.columnCity;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn ItemDescColumn => this.columnItemDesc;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn PriPrefQtyColumn => this.columnPriPrefQty;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn AbbreviationColumn => this.columnAbbreviation;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn MachineSNColumn => this.columnMachineSN;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn UsesFilterColumn => this.columnUsesFilter;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn autofulfillColumn => this.columnautofulfill;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn enabledColumn => this.columnenabled;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn PredictionDisabledColumn => this.columnPredictionDisabled;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn AlwaysSendChkUpColumn => this.columnAlwaysSendChkUp;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn NormallyRespondsColumn => this.columnNormallyResponds;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn NotesColumn => this.columnNotes;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [Browsable(false)]
            [DebuggerNonUserCode]
            public int Count => this.Rows.Count;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public CustomersDataSet.CustomersRow this[int index]
            {
                get => (CustomersDataSet.CustomersRow)this.Rows[index];
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event CustomersDataSet.CustomersRowChangeEventHandler CustomersRowChanging;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event CustomersDataSet.CustomersRowChangeEventHandler CustomersRowChanged;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event CustomersDataSet.CustomersRowChangeEventHandler CustomersRowDeleting;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event CustomersDataSet.CustomersRowChangeEventHandler CustomersRowDeleted;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void AddCustomersRow(CustomersDataSet.CustomersRow row) => this.Rows.Add((DataRow)row);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public CustomersDataSet.CustomersRow AddCustomersRow(
              string CompanyName,
              string ContactTitle,
              string ContactFirstName,
              string ContactLastName,
              string ContactAltFirstName,
              string ContactAltLastName,
              string Department,
              string BillingAddress,
              string StateOrProvince,
              string PostalCode,
              string PhoneNumber,
              string Extension,
              string FaxNumber,
              string CellNumber,
              string EmailAddress,
              string AltEmailAddress,
              string CustomerType,
              string EquipTypeName,
              int CoffeePreference,
              string City,
              string ItemDesc,
              float PriPrefQty,
              string Abbreviation,
              string MachineSN,
              bool UsesFilter,
              bool autofulfill,
              bool enabled,
              bool PredictionDisabled,
              bool AlwaysSendChkUp,
              bool NormallyResponds,
              string Notes)
            {
                CustomersDataSet.CustomersRow row = (CustomersDataSet.CustomersRow)this.NewRow();
                object[] objArray = new object[32 /*0x20*/]
                {
        (object) CompanyName,
        null,
        (object) ContactTitle,
        (object) ContactFirstName,
        (object) ContactLastName,
        (object) ContactAltFirstName,
        (object) ContactAltLastName,
        (object) Department,
        (object) BillingAddress,
        (object) StateOrProvince,
        (object) PostalCode,
        (object) PhoneNumber,
        (object) Extension,
        (object) FaxNumber,
        (object) CellNumber,
        (object) EmailAddress,
        (object) AltEmailAddress,
        (object) CustomerType,
        (object) EquipTypeName,
        (object) CoffeePreference,
        (object) City,
        (object) ItemDesc,
        (object) PriPrefQty,
        (object) Abbreviation,
        (object) MachineSN,
        (object) UsesFilter,
        (object) autofulfill,
        (object) enabled,
        (object) PredictionDisabled,
        (object) AlwaysSendChkUp,
        (object) NormallyResponds,
        (object) Notes
                };
                row.ItemArray = objArray;
                this.Rows.Add((DataRow)row);
                return row;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public override DataTable Clone()
            {
                CustomersDataSet.CustomersDataTable customersDataTable = (CustomersDataSet.CustomersDataTable)base.Clone();
                customersDataTable.InitVars();
                return (DataTable)customersDataTable;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            protected override DataTable CreateInstance()
            {
                return (DataTable)new CustomersDataSet.CustomersDataTable();
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            internal void InitVars()
            {
                this.columnCompanyName = this.Columns["CompanyName"];
                this.columnCustomerID = this.Columns["CustomerID"];
                this.columnContactTitle = this.Columns["ContactTitle"];
                this.columnContactFirstName = this.Columns["ContactFirstName"];
                this.columnContactLastName = this.Columns["ContactLastName"];
                this.columnContactAltFirstName = this.Columns["ContactAltFirstName"];
                this.columnContactAltLastName = this.Columns["ContactAltLastName"];
                this.columnDepartment = this.Columns["Department"];
                this.columnBillingAddress = this.Columns["BillingAddress"];
                this.columnStateOrProvince = this.Columns["StateOrProvince"];
                this.columnPostalCode = this.Columns["PostalCode"];
                this.columnPhoneNumber = this.Columns["PhoneNumber"];
                this.columnExtension = this.Columns["Extension"];
                this.columnFaxNumber = this.Columns["FaxNumber"];
                this.columnCellNumber = this.Columns["CellNumber"];
                this.columnEmailAddress = this.Columns["EmailAddress"];
                this.columnAltEmailAddress = this.Columns["AltEmailAddress"];
                this.columnCustomerType = this.Columns["CustomerType"];
                this.columnEquipTypeName = this.Columns["EquipTypeName"];
                this.columnCoffeePreference = this.Columns["CoffeePreference"];
                this.columnCity = this.Columns["City"];
                this.columnItemDesc = this.Columns["ItemDesc"];
                this.columnPriPrefQty = this.Columns["PriPrefQty"];
                this.columnAbbreviation = this.Columns["Abbreviation"];
                this.columnMachineSN = this.Columns["MachineSN"];
                this.columnUsesFilter = this.Columns["UsesFilter"];
                this.columnautofulfill = this.Columns["autofulfill"];
                this.columnenabled = this.Columns["enabled"];
                this.columnPredictionDisabled = this.Columns["PredictionDisabled"];
                this.columnAlwaysSendChkUp = this.Columns["AlwaysSendChkUp"];
                this.columnNormallyResponds = this.Columns["NormallyResponds"];
                this.columnNotes = this.Columns["Notes"];
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            private void InitClass()
            {
                this.columnCompanyName = new DataColumn("CompanyName", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnCompanyName);
                this.columnCustomerID = new DataColumn("CustomerID", typeof(int), (string)null, MappingType.Element);
                this.Columns.Add(this.columnCustomerID);
                this.columnContactTitle = new DataColumn("ContactTitle", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnContactTitle);
                this.columnContactFirstName = new DataColumn("ContactFirstName", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnContactFirstName);
                this.columnContactLastName = new DataColumn("ContactLastName", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnContactLastName);
                this.columnContactAltFirstName = new DataColumn("ContactAltFirstName", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnContactAltFirstName);
                this.columnContactAltLastName = new DataColumn("ContactAltLastName", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnContactAltLastName);
                this.columnDepartment = new DataColumn("Department", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnDepartment);
                this.columnBillingAddress = new DataColumn("BillingAddress", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnBillingAddress);
                this.columnStateOrProvince = new DataColumn("StateOrProvince", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnStateOrProvince);
                this.columnPostalCode = new DataColumn("PostalCode", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnPostalCode);
                this.columnPhoneNumber = new DataColumn("PhoneNumber", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnPhoneNumber);
                this.columnExtension = new DataColumn("Extension", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnExtension);
                this.columnFaxNumber = new DataColumn("FaxNumber", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnFaxNumber);
                this.columnCellNumber = new DataColumn("CellNumber", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnCellNumber);
                this.columnEmailAddress = new DataColumn("EmailAddress", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnEmailAddress);
                this.columnAltEmailAddress = new DataColumn("AltEmailAddress", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnAltEmailAddress);
                this.columnCustomerType = new DataColumn("CustomerType", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnCustomerType);
                this.columnEquipTypeName = new DataColumn("EquipTypeName", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnEquipTypeName);
                this.columnCoffeePreference = new DataColumn("CoffeePreference", typeof(int), (string)null, MappingType.Element);
                this.Columns.Add(this.columnCoffeePreference);
                this.columnCity = new DataColumn("City", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnCity);
                this.columnItemDesc = new DataColumn("ItemDesc", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnItemDesc);
                this.columnPriPrefQty = new DataColumn("PriPrefQty", typeof(float), (string)null, MappingType.Element);
                this.Columns.Add(this.columnPriPrefQty);
                this.columnAbbreviation = new DataColumn("Abbreviation", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnAbbreviation);
                this.columnMachineSN = new DataColumn("MachineSN", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnMachineSN);
                this.columnUsesFilter = new DataColumn("UsesFilter", typeof(bool), (string)null, MappingType.Element);
                this.Columns.Add(this.columnUsesFilter);
                this.columnautofulfill = new DataColumn("autofulfill", typeof(bool), (string)null, MappingType.Element);
                this.Columns.Add(this.columnautofulfill);
                this.columnenabled = new DataColumn("enabled", typeof(bool), (string)null, MappingType.Element);
                this.Columns.Add(this.columnenabled);
                this.columnPredictionDisabled = new DataColumn("PredictionDisabled", typeof(bool), (string)null, MappingType.Element);
                this.Columns.Add(this.columnPredictionDisabled);
                this.columnAlwaysSendChkUp = new DataColumn("AlwaysSendChkUp", typeof(bool), (string)null, MappingType.Element);
                this.Columns.Add(this.columnAlwaysSendChkUp);
                this.columnNormallyResponds = new DataColumn("NormallyResponds", typeof(bool), (string)null, MappingType.Element);
                this.Columns.Add(this.columnNormallyResponds);
                this.columnNotes = new DataColumn("Notes", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnNotes);
                this.columnCompanyName.MaxLength = 50;
                this.columnCustomerID.AutoIncrement = true;
                this.columnCustomerID.AutoIncrementSeed = SystemConstants.DatabaseConstants.InvalidID;
                this.columnCustomerID.AutoIncrementStep = SystemConstants.DatabaseConstants.InvalidID;
                this.columnContactTitle.MaxLength = 50;
                this.columnContactFirstName.MaxLength = 30;
                this.columnContactLastName.MaxLength = 50;
                this.columnContactAltFirstName.MaxLength = 50;
                this.columnContactAltLastName.MaxLength = 50;
                this.columnDepartment.MaxLength = 50;
                this.columnBillingAddress.MaxLength = (int)byte.MaxValue;
                this.columnStateOrProvince.MaxLength = 20;
                this.columnPostalCode.MaxLength = 20;
                this.columnPhoneNumber.MaxLength = 30;
                this.columnExtension.MaxLength = 30;
                this.columnFaxNumber.MaxLength = 30;
                this.columnCellNumber.MaxLength = 50;
                this.columnEmailAddress.MaxLength = 50;
                this.columnAltEmailAddress.MaxLength = (int)byte.MaxValue;
                this.columnCustomerType.MaxLength = 30;
                this.columnEquipTypeName.MaxLength = 50;
                this.columnCity.MaxLength = (int)byte.MaxValue;
                this.columnItemDesc.MaxLength = 50;
                this.columnAbbreviation.MaxLength = 5;
                this.columnMachineSN.MaxLength = 50;
                this.columnNotes.MaxLength = 536870910;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public CustomersDataSet.CustomersRow NewCustomersRow()
            {
                return (CustomersDataSet.CustomersRow)this.NewRow();
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
            {
                return (DataRow)new CustomersDataSet.CustomersRow(builder);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected override Type GetRowType() => typeof(CustomersDataSet.CustomersRow);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            protected override void OnRowChanged(DataRowChangeEventArgs e)
            {
                base.OnRowChanged(e);
                if (this.CustomersRowChanged == null)
                    return;
                this.CustomersRowChanged((object)this, new CustomersDataSet.CustomersRowChangeEvent((CustomersDataSet.CustomersRow)e.Row, e.Action));
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected override void OnRowChanging(DataRowChangeEventArgs e)
            {
                base.OnRowChanging(e);
                if (this.CustomersRowChanging == null)
                    return;
                this.CustomersRowChanging((object)this, new CustomersDataSet.CustomersRowChangeEvent((CustomersDataSet.CustomersRow)e.Row, e.Action));
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected override void OnRowDeleted(DataRowChangeEventArgs e)
            {
                base.OnRowDeleted(e);
                if (this.CustomersRowDeleted == null)
                    return;
                this.CustomersRowDeleted((object)this, new CustomersDataSet.CustomersRowChangeEvent((CustomersDataSet.CustomersRow)e.Row, e.Action));
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            protected override void OnRowDeleting(DataRowChangeEventArgs e)
            {
                base.OnRowDeleting(e);
                if (this.CustomersRowDeleting == null)
                    return;
                this.CustomersRowDeleting((object)this, new CustomersDataSet.CustomersRowChangeEvent((CustomersDataSet.CustomersRow)e.Row, e.Action));
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void RemoveCustomersRow(CustomersDataSet.CustomersRow row)
            {
                this.Rows.Remove((DataRow)row);
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
            {
                XmlSchemaComplexType typedTableSchema = new XmlSchemaComplexType();
                XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
                CustomersDataSet customersDataSet = new CustomersDataSet();
                XmlSchemaAny xmlSchemaAny1 = new XmlSchemaAny();
                xmlSchemaAny1.Namespace = "http://www.w3.org/2001/XMLSchema";
                xmlSchemaAny1.MinOccurs = 0M;
                xmlSchemaAny1.MaxOccurs = Decimal.MaxValue;
                xmlSchemaAny1.ProcessContents = XmlSchemaContentProcessing.Lax;
                xmlSchemaSequence.Items.Add((XmlSchemaObject)xmlSchemaAny1);
                XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
                xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
                xmlSchemaAny2.MinOccurs = 1M;
                xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
                xmlSchemaSequence.Items.Add((XmlSchemaObject)xmlSchemaAny2);
                typedTableSchema.Attributes.Add((XmlSchemaObject)new XmlSchemaAttribute()
                {
                    Name = "namespace",
                    FixedValue = customersDataSet.Namespace
                });
                typedTableSchema.Attributes.Add((XmlSchemaObject)new XmlSchemaAttribute()
                {
                    Name = "tableTypeName",
                    FixedValue = nameof(CustomersDataTable)
                });
                typedTableSchema.Particle = (XmlSchemaParticle)xmlSchemaSequence;
                XmlSchema schemaSerializable = customersDataSet.GetSchemaSerializable();
                if (xs.Contains(schemaSerializable.TargetNamespace))
                {
                    MemoryStream memoryStream1 = new MemoryStream();
                    MemoryStream memoryStream2 = new MemoryStream();
                    try
                    {
                        schemaSerializable.Write((Stream)memoryStream1);
                        IEnumerator enumerator = xs.Schemas(schemaSerializable.TargetNamespace).GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            XmlSchema current = (XmlSchema)enumerator.Current;
                            memoryStream2.SetLength(0L);
                            current.Write((Stream)memoryStream2);
                            if (memoryStream1.Length == memoryStream2.Length)
                            {
                                memoryStream1.Position = 0;
                                memoryStream2.Position = 0;
                                
                                while (memoryStream1.Position != memoryStream1.Length && memoryStream1.ReadByte() == memoryStream2.ReadByte());
                                if (memoryStream1.Position == memoryStream1.Length)
                                    return typedTableSchema;
                            }
                        }
                    }
                    finally
                    {
                        memoryStream1?.Close();
                        memoryStream2?.Close();
                    }
                }
                xs.Add(schemaSerializable);
                return typedTableSchema;
            }
        }

        public class CustomersRow : DataRow
        {
            private CustomersDataSet.CustomersDataTable tableCustomers;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            internal CustomersRow(DataRowBuilder rb)
              : base(rb)
            {
                this.tableCustomers = (CustomersDataSet.CustomersDataTable)this.Table;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public string CompanyName
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.CompanyNameColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'CompanyName' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.CompanyNameColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public long CustomerID
            {
                get
                {
                    try
                    {
                        return (int)this[this.tableCustomers.CustomerIDColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'CustomerID' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.CustomerIDColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public string ContactTitle
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.ContactTitleColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'ContactTitle' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.ContactTitleColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string ContactFirstName
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.ContactFirstNameColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'ContactFirstName' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.ContactFirstNameColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string ContactLastName
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.ContactLastNameColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'ContactLastName' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.ContactLastNameColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public string ContactAltFirstName
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.ContactAltFirstNameColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'ContactAltFirstName' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.ContactAltFirstNameColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string ContactAltLastName
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.ContactAltLastNameColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'ContactAltLastName' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.ContactAltLastNameColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string Department
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.DepartmentColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'Department' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.DepartmentColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public string BillingAddress
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.BillingAddressColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'BillingAddress' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.BillingAddressColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string StateOrProvince
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.StateOrProvinceColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'StateOrProvince' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.StateOrProvinceColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string PostalCode
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.PostalCodeColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'PostalCode' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.PostalCodeColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public string PhoneNumber
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.PhoneNumberColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'PhoneNumber' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.PhoneNumberColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string Extension
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.ExtensionColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'Extension' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.ExtensionColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public string FaxNumber
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.FaxNumberColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'FaxNumber' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.FaxNumberColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string CellNumber
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.CellNumberColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'CellNumber' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.CellNumberColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public string EmailAddress
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.EmailAddressColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'EmailAddress' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.EmailAddressColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public string AltEmailAddress
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.AltEmailAddressColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'AltEmailAddress' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.AltEmailAddressColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string CustomerType
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.CustomerTypeColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'CustomerType' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.CustomerTypeColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string EquipTypeName
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.EquipTypeNameColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'EquipTypeName' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.EquipTypeNameColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public int CoffeePreference
            {
                get
                {
                    try
                    {
                        return (int)this[this.tableCustomers.CoffeePreferenceColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'CoffeePreference' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.CoffeePreferenceColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string City
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.CityColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'City' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.CityColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public string ItemDesc
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.ItemDescColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'ItemDesc' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.ItemDescColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public float PriPrefQty
            {
                get
                {
                    try
                    {
                        return (float)this[this.tableCustomers.PriPrefQtyColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'PriPrefQty' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.PriPrefQtyColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string Abbreviation
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.AbbreviationColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'Abbreviation' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.AbbreviationColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public string MachineSN
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.MachineSNColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'MachineSN' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.MachineSNColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool UsesFilter
            {
                get
                {
                    try
                    {
                        return (bool)this[this.tableCustomers.UsesFilterColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'UsesFilter' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.UsesFilterColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool autofulfill
            {
                get
                {
                    try
                    {
                        return (bool)this[this.tableCustomers.autofulfillColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'autofulfill' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.autofulfillColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool enabled
            {
                get
                {
                    try
                    {
                        return (bool)this[this.tableCustomers.enabledColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'enabled' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.enabledColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool PredictionDisabled
            {
                get
                {
                    try
                    {
                        return (bool)this[this.tableCustomers.PredictionDisabledColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'PredictionDisabled' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.PredictionDisabledColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool AlwaysSendChkUp
            {
                get
                {
                    try
                    {
                        return (bool)this[this.tableCustomers.AlwaysSendChkUpColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'AlwaysSendChkUp' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.AlwaysSendChkUpColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool NormallyResponds
            {
                get
                {
                    try
                    {
                        return (bool)this[this.tableCustomers.NormallyRespondsColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'NormallyResponds' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.NormallyRespondsColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string Notes
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomers.NotesColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'Notes' in table 'Customers' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomers.NotesColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsCompanyNameNull() => this.IsNull(this.tableCustomers.CompanyNameColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetCompanyNameNull()
            {
                this[this.tableCustomers.CompanyNameColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsCustomerIDNull() => this.IsNull(this.tableCustomers.CustomerIDColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetCustomerIDNull() => this[this.tableCustomers.CustomerIDColumn] = Convert.DBNull;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsContactTitleNull() => this.IsNull(this.tableCustomers.ContactTitleColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetContactTitleNull()
            {
                this[this.tableCustomers.ContactTitleColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsContactFirstNameNull() => this.IsNull(this.tableCustomers.ContactFirstNameColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetContactFirstNameNull()
            {
                this[this.tableCustomers.ContactFirstNameColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsContactLastNameNull() => this.IsNull(this.tableCustomers.ContactLastNameColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetContactLastNameNull()
            {
                this[this.tableCustomers.ContactLastNameColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsContactAltFirstNameNull()
            {
                return this.IsNull(this.tableCustomers.ContactAltFirstNameColumn);
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetContactAltFirstNameNull()
            {
                this[this.tableCustomers.ContactAltFirstNameColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsContactAltLastNameNull()
            {
                return this.IsNull(this.tableCustomers.ContactAltLastNameColumn);
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetContactAltLastNameNull()
            {
                this[this.tableCustomers.ContactAltLastNameColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsDepartmentNull() => this.IsNull(this.tableCustomers.DepartmentColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetDepartmentNull() => this[this.tableCustomers.DepartmentColumn] = Convert.DBNull;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsBillingAddressNull() => this.IsNull(this.tableCustomers.BillingAddressColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetBillingAddressNull()
            {
                this[this.tableCustomers.BillingAddressColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsStateOrProvinceNull() => this.IsNull(this.tableCustomers.StateOrProvinceColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetStateOrProvinceNull()
            {
                this[this.tableCustomers.StateOrProvinceColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsPostalCodeNull() => this.IsNull(this.tableCustomers.PostalCodeColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetPostalCodeNull() => this[this.tableCustomers.PostalCodeColumn] = Convert.DBNull;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsPhoneNumberNull() => this.IsNull(this.tableCustomers.PhoneNumberColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetPhoneNumberNull()
            {
                this[this.tableCustomers.PhoneNumberColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsExtensionNull() => this.IsNull(this.tableCustomers.ExtensionColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetExtensionNull() => this[this.tableCustomers.ExtensionColumn] = Convert.DBNull;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsFaxNumberNull() => this.IsNull(this.tableCustomers.FaxNumberColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetFaxNumberNull() => this[this.tableCustomers.FaxNumberColumn] = Convert.DBNull;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsCellNumberNull() => this.IsNull(this.tableCustomers.CellNumberColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetCellNumberNull() => this[this.tableCustomers.CellNumberColumn] = Convert.DBNull;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsEmailAddressNull() => this.IsNull(this.tableCustomers.EmailAddressColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetEmailAddressNull()
            {
                this[this.tableCustomers.EmailAddressColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsAltEmailAddressNull() => this.IsNull(this.tableCustomers.AltEmailAddressColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetAltEmailAddressNull()
            {
                this[this.tableCustomers.AltEmailAddressColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsCustomerTypeNull() => this.IsNull(this.tableCustomers.CustomerTypeColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetCustomerTypeNull()
            {
                this[this.tableCustomers.CustomerTypeColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsEquipTypeNameNull() => this.IsNull(this.tableCustomers.EquipTypeNameColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetEquipTypeNameNull()
            {
                this[this.tableCustomers.EquipTypeNameColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsCoffeePreferenceNull() => this.IsNull(this.tableCustomers.CoffeePreferenceColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetCoffeePreferenceNull()
            {
                this[this.tableCustomers.CoffeePreferenceColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsCityNull() => this.IsNull(this.tableCustomers.CityColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetCityNull() => this[this.tableCustomers.CityColumn] = Convert.DBNull;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsItemDescNull() => this.IsNull(this.tableCustomers.ItemDescColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetItemDescNull() => this[this.tableCustomers.ItemDescColumn] = Convert.DBNull;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsPriPrefQtyNull() => this.IsNull(this.tableCustomers.PriPrefQtyColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetPriPrefQtyNull() => this[this.tableCustomers.PriPrefQtyColumn] = Convert.DBNull;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsAbbreviationNull() => this.IsNull(this.tableCustomers.AbbreviationColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetAbbreviationNull()
            {
                this[this.tableCustomers.AbbreviationColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsMachineSNNull() => this.IsNull(this.tableCustomers.MachineSNColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetMachineSNNull() => this[this.tableCustomers.MachineSNColumn] = Convert.DBNull;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsUsesFilterNull() => this.IsNull(this.tableCustomers.UsesFilterColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetUsesFilterNull() => this[this.tableCustomers.UsesFilterColumn] = Convert.DBNull;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsautofulfillNull() => this.IsNull(this.tableCustomers.autofulfillColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetautofulfillNull()
            {
                this[this.tableCustomers.autofulfillColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsenabledNull() => this.IsNull(this.tableCustomers.enabledColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetenabledNull() => this[this.tableCustomers.enabledColumn] = Convert.DBNull;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsPredictionDisabledNull()
            {
                return this.IsNull(this.tableCustomers.PredictionDisabledColumn);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetPredictionDisabledNull()
            {
                this[this.tableCustomers.PredictionDisabledColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsAlwaysSendChkUpNull() => this.IsNull(this.tableCustomers.AlwaysSendChkUpColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetAlwaysSendChkUpNull()
            {
                this[this.tableCustomers.AlwaysSendChkUpColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsNormallyRespondsNull() => this.IsNull(this.tableCustomers.NormallyRespondsColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetNormallyRespondsNull()
            {
                this[this.tableCustomers.NormallyRespondsColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsNotesNull() => this.IsNull(this.tableCustomers.NotesColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetNotesNull() => this[this.tableCustomers.NotesColumn] = Convert.DBNull;
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public class CustomersRowChangeEvent : EventArgs
        {
            private CustomersDataSet.CustomersRow eventRow;
            private DataRowAction eventAction;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public CustomersRowChangeEvent(CustomersDataSet.CustomersRow row, DataRowAction action)
            {
                this.eventRow = row;
                this.eventAction = action;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public CustomersDataSet.CustomersRow Row => this.eventRow;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataRowAction Action => this.eventAction;
        }
    }
}