// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.DataSets.TrackerDataSet
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

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

//- only form later versions #nullable disable
namespace TrackerDotNet.DataSets
{
    [DesignerCategory("code")]
    [XmlSchemaProvider("GetTypedDataSetSchema")]
    [ToolboxItem(true)]
    [XmlRoot("TrackerDataSet")]
    [HelpKeyword("vs.data.DataSet")]
    [Serializable]
    public class TrackerDataSet : DataSet
    {
        private TrackerDataSet.OrderTblDataTable tableOrdersTbl;
        private TrackerDataSet.CustomersTblDataTable tableCustomersTbl;
        private TrackerDataSet.ItemTypeTblDataTable tableItemTypeTbl;
        private TrackerDataSet.CityTblDataTable tableCityTbl;
        private DataRelation relationPrimaryItemPrefernce;
        private DataRelation relationSecondaryItemPreference;
        private DataRelation relationCustomerCityRelation;
        private DataRelation relationOrdersToCustomerRelation;
        private DataRelation relationOrderItemToItemIDRelation;
        private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        [DebuggerNonUserCode]
        public TrackerDataSet()
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
        protected TrackerDataSet(SerializationInfo info, StreamingContext context)
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
                    if (dataSet.Tables[nameof(OrdersTbl)] != null)
                        base.Tables.Add((DataTable)new TrackerDataSet.OrderTblDataTable(dataSet.Tables[nameof(OrdersTbl)]));
                    if (dataSet.Tables[nameof(CustomersTbl)] != null)
                        base.Tables.Add((DataTable)new TrackerDataSet.CustomersTblDataTable(dataSet.Tables[nameof(CustomersTbl)]));
                    if (dataSet.Tables[nameof(ItemTypeTbl)] != null)
                        base.Tables.Add((DataTable)new TrackerDataSet.ItemTypeTblDataTable(dataSet.Tables[nameof(ItemTypeTbl)]));
                    if (dataSet.Tables[nameof(CityTbl)] != null)
                        base.Tables.Add((DataTable)new TrackerDataSet.CityTblDataTable(dataSet.Tables[nameof(CityTbl)]));
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
        public TrackerDataSet.OrderTblDataTable OrdersTbl => this.tableOrdersTbl;

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TrackerDataSet.CustomersTblDataTable CustomersTbl => this.tableCustomersTbl;

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TrackerDataSet.ItemTypeTblDataTable ItemTypeTbl => this.tableItemTypeTbl;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Browsable(false)]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        [DebuggerNonUserCode]
        public TrackerDataSet.CityTblDataTable CityTbl => this.tableCityTbl;

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DebuggerNonUserCode]
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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
            TrackerDataSet trackerDataSet = (TrackerDataSet)base.Clone();
            trackerDataSet.InitVars();
            trackerDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
            return (DataSet)trackerDataSet;
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
                if (dataSet.Tables["OrdersTbl"] != null)
                    base.Tables.Add((DataTable)new TrackerDataSet.OrderTblDataTable(dataSet.Tables["OrdersTbl"]));
                if (dataSet.Tables["CustomersTbl"] != null)
                    base.Tables.Add((DataTable)new TrackerDataSet.CustomersTblDataTable(dataSet.Tables["CustomersTbl"]));
                if (dataSet.Tables["ItemTypeTbl"] != null)
                    base.Tables.Add((DataTable)new TrackerDataSet.ItemTypeTblDataTable(dataSet.Tables["ItemTypeTbl"]));
                if (dataSet.Tables["CityTbl"] != null)
                    base.Tables.Add((DataTable)new TrackerDataSet.CityTblDataTable(dataSet.Tables["CityTbl"]));
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
            this.tableOrdersTbl = (TrackerDataSet.OrderTblDataTable)base.Tables["OrdersTbl"];
            if (initTable && this.tableOrdersTbl != null)
                this.tableOrdersTbl.InitVars();
            this.tableCustomersTbl = (TrackerDataSet.CustomersTblDataTable)base.Tables["CustomersTbl"];
            if (initTable && this.tableCustomersTbl != null)
                this.tableCustomersTbl.InitVars();
            this.tableItemTypeTbl = (TrackerDataSet.ItemTypeTblDataTable)base.Tables["ItemTypeTbl"];
            if (initTable && this.tableItemTypeTbl != null)
                this.tableItemTypeTbl.InitVars();
            this.tableCityTbl = (TrackerDataSet.CityTblDataTable)base.Tables["CityTbl"];
            if (initTable && this.tableCityTbl != null)
                this.tableCityTbl.InitVars();
            this.relationPrimaryItemPrefernce = this.Relations["PrimaryItemPrefernce"];
            this.relationSecondaryItemPreference = this.Relations["SecondaryItemPreference"];
            this.relationCustomerCityRelation = this.Relations["CustomerCityRelation"];
            this.relationOrdersToCustomerRelation = this.Relations["OrdersToCustomerRelation"];
            this.relationOrderItemToItemIDRelation = this.Relations["OrderItemToItemIDRelation"];
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        [DebuggerNonUserCode]
        private void InitClass()
        {
            this.DataSetName = nameof(TrackerDataSet);
            this.Prefix = "";
            this.Namespace = "http://tempuri.org/TrackerDataSet.xsd";
            this.EnforceConstraints = true;
            this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
            this.tableOrdersTbl = new TrackerDataSet.OrderTblDataTable();
            base.Tables.Add((DataTable)this.tableOrdersTbl);
            this.tableCustomersTbl = new TrackerDataSet.CustomersTblDataTable();
            base.Tables.Add((DataTable)this.tableCustomersTbl);
            this.tableItemTypeTbl = new TrackerDataSet.ItemTypeTblDataTable();
            base.Tables.Add((DataTable)this.tableItemTypeTbl);
            this.tableCityTbl = new TrackerDataSet.CityTblDataTable();
            base.Tables.Add((DataTable)this.tableCityTbl);
            this.relationPrimaryItemPrefernce = new DataRelation("PrimaryItemPrefernce", new DataColumn[1]
            {
      this.tableItemTypeTbl.ItemTypeIDColumn
            }, new DataColumn[1]
            {
      this.tableCustomersTbl.CoffeePreferenceColumn
            }, false);
            this.Relations.Add(this.relationPrimaryItemPrefernce);
            this.relationSecondaryItemPreference = new DataRelation("SecondaryItemPreference", new DataColumn[1]
            {
      this.tableItemTypeTbl.ItemTypeIDColumn
            }, new DataColumn[1]
            {
      this.tableCustomersTbl.SecondaryPreferenceColumn
            }, false);
            this.Relations.Add(this.relationSecondaryItemPreference);
            this.relationCustomerCityRelation = new DataRelation("CustomerCityRelation", new DataColumn[1]
            {
      this.tableCityTbl.IDColumn
            }, new DataColumn[1]
            {
      this.tableCustomersTbl.CityColumn
            }, false);
            this.Relations.Add(this.relationCustomerCityRelation);
            this.relationOrdersToCustomerRelation = new DataRelation("OrdersToCustomerRelation", new DataColumn[1]
            {
      this.tableCustomersTbl.CustomerIDColumn
            }, new DataColumn[1]
            {
      this.tableOrdersTbl.CustomerIDColumn
            }, false);
            this.Relations.Add(this.relationOrdersToCustomerRelation);
            this.relationOrderItemToItemIDRelation = new DataRelation("OrderItemToItemIDRelation", new DataColumn[1]
            {
      this.tableItemTypeTbl.ItemTypeIDColumn
            }, new DataColumn[1]
            {
      this.tableOrdersTbl.ItemTypeIDColumn
            }, false);
            this.Relations.Add(this.relationOrderItemToItemIDRelation);
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        [DebuggerNonUserCode]
        private bool ShouldSerializeOrdersTbl() => false;

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        [DebuggerNonUserCode]
        private bool ShouldSerializeCustomersTbl() => false;

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        private bool ShouldSerializeItemTypeTbl() => false;

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        [DebuggerNonUserCode]
        private bool ShouldSerializeCityTbl() => false;

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        private void SchemaChanged(object sender, CollectionChangeEventArgs e)
        {
            if (e.Action != CollectionChangeAction.Remove)
                return;
            this.InitVars();
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        [DebuggerNonUserCode]
        public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
        {
            TrackerDataSet trackerDataSet = new TrackerDataSet();
            XmlSchemaComplexType typedDataSetSchema = new XmlSchemaComplexType();
            XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
            xmlSchemaSequence.Items.Add((XmlSchemaObject)new XmlSchemaAny()
            {
                Namespace = trackerDataSet.Namespace
            });
            typedDataSetSchema.Particle = (XmlSchemaParticle)xmlSchemaSequence;
            XmlSchema schemaSerializable = trackerDataSet.GetSchemaSerializable();
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
        public delegate void OrdersTblRowChangeEventHandler(
          object sender,
          TrackerDataSet.OrdersTblRowChangeEvent e);

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public delegate void CustomersTblRowChangeEventHandler(
          object sender,
          TrackerDataSet.CustomersTblRowChangeEvent e);

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public delegate void ItemTypeTblRowChangeEventHandler(
          object sender,
          TrackerDataSet.ItemTypeTblRowChangeEvent e);

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public delegate void CityTblRowChangeEventHandler(
          object sender,
          TrackerDataSet.CityTblRowChangeEvent e);

        [XmlSchemaProvider("GetTypedTableSchema")]
        [Serializable]
        public class OrderTblDataTable : TypedTableBase<TrackerDataSet.OrdersTblRow>
        {
            private DataColumn columnOrderID;
            private DataColumn columnCustomerID;
            private DataColumn columnOrderDate;
            private DataColumn columnRoastDate;
            private DataColumn columnItemTypeID;
            private DataColumn columnQuantityOrdered;
            private DataColumn columnRequiredByDate;
            private DataColumn columnToBeDeliveredBy;
            private DataColumn columnConfirmed;
            private DataColumn columnDone;
            private DataColumn columnNotes;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public OrderTblDataTable()
            {
                this.TableName = "OrdersTbl";
                this.BeginInit();
                this.InitClass();
                this.EndInit();
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            internal OrderTblDataTable(DataTable table)
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
            protected OrderTblDataTable(SerializationInfo info, StreamingContext context)
              : base(info, context)
            {
                this.InitVars();
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn OrderIDColumn => this.columnOrderID;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn CustomerIDColumn => this.columnCustomerID;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn OrderDateColumn => this.columnOrderDate;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn RoastDateColumn => this.columnRoastDate;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn ItemTypeIDColumn => this.columnItemTypeID;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn QuantityOrderedColumn => this.columnQuantityOrdered;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn RequiredByDateColumn => this.columnRequiredByDate;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn ToBeDeliveredByColumn => this.columnToBeDeliveredBy;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn ConfirmedColumn => this.columnConfirmed;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn DoneColumn => this.columnDone;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn NotesColumn => this.columnNotes;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [Browsable(false)]
            public int Count => this.Rows.Count;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public TrackerDataSet.OrdersTblRow this[int index]
            {
                get => (TrackerDataSet.OrdersTblRow)this.Rows[index];
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event TrackerDataSet.OrdersTblRowChangeEventHandler OrdersTblRowChanging;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event TrackerDataSet.OrdersTblRowChangeEventHandler OrdersTblRowChanged;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event TrackerDataSet.OrdersTblRowChangeEventHandler OrdersTblRowDeleting;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event TrackerDataSet.OrdersTblRowChangeEventHandler OrdersTblRowDeleted;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void AddOrdersTblRow(TrackerDataSet.OrdersTblRow row) => this.Rows.Add((DataRow)row);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public TrackerDataSet.OrdersTblRow AddOrdersTblRow(
              TrackerDataSet.CustomersTblRow parentCustomersTblRowByOrdersToCustomerRelation,
              DateTime OrderDate,
              DateTime RoastDate,
              TrackerDataSet.ItemTypeTblRow parentItemTypeTblRowByOrderItemToItemIDRelation,
              float QuantityOrdered,
              DateTime RequiredByDate,
              int ToBeDeliveredBy,
              bool Confirmed,
              bool Done,
              string Notes)
            {
                TrackerDataSet.OrdersTblRow row = (TrackerDataSet.OrdersTblRow)this.NewRow();
                object[] objArray = new object[11]
                {
        null,
        null,
        (object) OrderDate,
        (object) RoastDate,
        null,
        (object) QuantityOrdered,
        (object) RequiredByDate,
        (object) ToBeDeliveredBy,
        (object) Confirmed,
        (object) Done,
        (object) Notes
                };
                if (parentCustomersTblRowByOrdersToCustomerRelation != null)
                    objArray[1] = parentCustomersTblRowByOrdersToCustomerRelation[0];
                if (parentItemTypeTblRowByOrderItemToItemIDRelation != null)
                    objArray[4] = parentItemTypeTblRowByOrderItemToItemIDRelation[0];
                row.ItemArray = objArray;
                this.Rows.Add((DataRow)row);
                return row;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public TrackerDataSet.OrdersTblRow FindByOrderID(int OrderID)
            {
                return (TrackerDataSet.OrdersTblRow)this.Rows.Find(new object[1]
                {
        (object) OrderID
                });
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public override DataTable Clone()
            {
                TrackerDataSet.OrderTblDataTable orderTblDataTable = (TrackerDataSet.OrderTblDataTable)base.Clone();
                orderTblDataTable.InitVars();
                return (DataTable)orderTblDataTable;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            protected override DataTable CreateInstance()
            {
                return (DataTable)new TrackerDataSet.OrderTblDataTable();
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            internal void InitVars()
            {
                this.columnOrderID = this.Columns["OrderID"];
                this.columnCustomerID = this.Columns["CustomerID"];
                this.columnOrderDate = this.Columns["OrderDate"];
                this.columnRoastDate = this.Columns["RoastDate"];
                this.columnItemTypeID = this.Columns["ItemTypeID"];
                this.columnQuantityOrdered = this.Columns["QuantityOrdered"];
                this.columnRequiredByDate = this.Columns["RequiredByDate"];
                this.columnToBeDeliveredBy = this.Columns["ToBeDeliveredBy"];
                this.columnConfirmed = this.Columns["Confirmed"];
                this.columnDone = this.Columns["Done"];
                this.columnNotes = this.Columns["Notes"];
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            private void InitClass()
            {
                this.columnOrderID = new DataColumn("OrderID", typeof(int), (string)null, MappingType.Element);
                this.Columns.Add(this.columnOrderID);
                this.columnCustomerID = new DataColumn("CustomerID", typeof(int), (string)null, MappingType.Element);
                this.Columns.Add(this.columnCustomerID);
                this.columnOrderDate = new DataColumn("OrderDate", typeof(DateTime), (string)null, MappingType.Element);
                this.Columns.Add(this.columnOrderDate);
                this.columnRoastDate = new DataColumn("RoastDate", typeof(DateTime), (string)null, MappingType.Element);
                this.Columns.Add(this.columnRoastDate);
                this.columnItemTypeID = new DataColumn("ItemTypeID", typeof(int), (string)null, MappingType.Element);
                this.Columns.Add(this.columnItemTypeID);
                this.columnQuantityOrdered = new DataColumn("QuantityOrdered", typeof(float), (string)null, MappingType.Element);
                this.Columns.Add(this.columnQuantityOrdered);
                this.columnRequiredByDate = new DataColumn("RequiredByDate", typeof(DateTime), (string)null, MappingType.Element);
                this.Columns.Add(this.columnRequiredByDate);
                this.columnToBeDeliveredBy = new DataColumn("ToBeDeliveredBy", typeof(int), (string)null, MappingType.Element);
                this.Columns.Add(this.columnToBeDeliveredBy);
                this.columnConfirmed = new DataColumn("Confirmed", typeof(bool), (string)null, MappingType.Element);
                this.Columns.Add(this.columnConfirmed);
                this.columnDone = new DataColumn("Done", typeof(bool), (string)null, MappingType.Element);
                this.Columns.Add(this.columnDone);
                this.columnNotes = new DataColumn("Notes", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnNotes);
                this.Constraints.Add((Constraint)new UniqueConstraint("Constraint1", new DataColumn[1]
                {
        this.columnOrderID
                }, true));
                this.columnOrderID.AutoIncrement = true;
                this.columnOrderID.AutoIncrementSeed = -1L;
                this.columnOrderID.AutoIncrementStep = -1L;
                this.columnOrderID.AllowDBNull = false;
                this.columnOrderID.Unique = true;
                this.columnNotes.MaxLength = (int)byte.MaxValue;
                this.ExtendedProperties.Add((object)"Generator_TableClassName", (object)nameof(OrderTblDataTable));
                this.ExtendedProperties.Add((object)"Generator_UserTableName", (object)"OrdersTbl");
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public TrackerDataSet.OrdersTblRow NewOrdersTblRow()
            {
                return (TrackerDataSet.OrdersTblRow)this.NewRow();
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
            {
                return (DataRow)new TrackerDataSet.OrdersTblRow(builder);
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            protected override Type GetRowType() => typeof(TrackerDataSet.OrdersTblRow);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected override void OnRowChanged(DataRowChangeEventArgs e)
            {
                base.OnRowChanged(e);
                if (this.OrdersTblRowChanged == null)
                    return;
                this.OrdersTblRowChanged((object)this, new TrackerDataSet.OrdersTblRowChangeEvent((TrackerDataSet.OrdersTblRow)e.Row, e.Action));
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected override void OnRowChanging(DataRowChangeEventArgs e)
            {
                base.OnRowChanging(e);
                if (this.OrdersTblRowChanging == null)
                    return;
                this.OrdersTblRowChanging((object)this, new TrackerDataSet.OrdersTblRowChangeEvent((TrackerDataSet.OrdersTblRow)e.Row, e.Action));
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected override void OnRowDeleted(DataRowChangeEventArgs e)
            {
                base.OnRowDeleted(e);
                if (this.OrdersTblRowDeleted == null)
                    return;
                this.OrdersTblRowDeleted((object)this, new TrackerDataSet.OrdersTblRowChangeEvent((TrackerDataSet.OrdersTblRow)e.Row, e.Action));
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            protected override void OnRowDeleting(DataRowChangeEventArgs e)
            {
                base.OnRowDeleting(e);
                if (this.OrdersTblRowDeleting == null)
                    return;
                this.OrdersTblRowDeleting((object)this, new TrackerDataSet.OrdersTblRowChangeEvent((TrackerDataSet.OrdersTblRow)e.Row, e.Action));
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void RemoveOrdersTblRow(TrackerDataSet.OrdersTblRow row)
            {
                this.Rows.Remove((DataRow)row);
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
            {
                XmlSchemaComplexType typedTableSchema = new XmlSchemaComplexType();
                XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
                TrackerDataSet trackerDataSet = new TrackerDataSet();
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
                    FixedValue = trackerDataSet.Namespace
                });
                typedTableSchema.Attributes.Add((XmlSchemaObject)new XmlSchemaAttribute()
                {
                    Name = "tableTypeName",
                    FixedValue = nameof(OrderTblDataTable)
                });
                typedTableSchema.Particle = (XmlSchemaParticle)xmlSchemaSequence;
                XmlSchema schemaSerializable = trackerDataSet.GetSchemaSerializable();
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

        [XmlSchemaProvider("GetTypedTableSchema")]
        [Serializable]
        public class CustomersTblDataTable : TypedTableBase<TrackerDataSet.CustomersTblRow>
        {
            private DataColumn columnCustomerID;
            private DataColumn columnCompanyName;
            private DataColumn columnContactTitle;
            private DataColumn columnContactFirstName;
            private DataColumn columnContactLastName;
            private DataColumn columnContactAltFirstName;
            private DataColumn columnContactAltLastName;
            private DataColumn columnDepartment;
            private DataColumn columnBillingAddress;
            private DataColumn columnCity;
            private DataColumn columnStateOrProvince;
            private DataColumn columnPostalCode;
            private DataColumn _columnCountry_Region;
            private DataColumn columnPhoneNumber;
            private DataColumn columnExtension;
            private DataColumn columnFaxNumber;
            private DataColumn columnCellNumber;
            private DataColumn columnEmailAddress;
            private DataColumn columnAltEmailAddress;
            private DataColumn columnContractNo;
            private DataColumn columnCustomerType;
            private DataColumn columnEquipType;
            private DataColumn columnCoffeePreference;
            private DataColumn columnPriPrefQty;
            private DataColumn columnSecondaryPreference;
            private DataColumn columnSecPrefQty;
            private DataColumn columnTypicallySecToo;
            private DataColumn columnPreferedAgent;
            private DataColumn columnMachineSN;
            private DataColumn columnUsesFilter;
            private DataColumn columnautofulfill;
            private DataColumn columnenabled;
            private DataColumn columnPredictionDisabled;
            private DataColumn columnAlwaysSendChkUp;
            private DataColumn columnNormallyResponds;
            private DataColumn columnNotes;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public CustomersTblDataTable()
            {
                this.TableName = "CustomersTbl";
                this.BeginInit();
                this.InitClass();
                this.EndInit();
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            internal CustomersTblDataTable(DataTable table)
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

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected CustomersTblDataTable(SerializationInfo info, StreamingContext context)
              : base(info, context)
            {
                this.InitVars();
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn CustomerIDColumn => this.columnCustomerID;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn CompanyNameColumn => this.columnCompanyName;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn ContactTitleColumn => this.columnContactTitle;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn ContactFirstNameColumn => this.columnContactFirstName;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn ContactLastNameColumn => this.columnContactLastName;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn ContactAltFirstNameColumn => this.columnContactAltFirstName;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn ContactAltLastNameColumn => this.columnContactAltLastName;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn DepartmentColumn => this.columnDepartment;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn BillingAddressColumn => this.columnBillingAddress;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn CityColumn => this.columnCity;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn StateOrProvinceColumn => this.columnStateOrProvince;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn PostalCodeColumn => this.columnPostalCode;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn _Country_RegionColumn => this._columnCountry_Region;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn PhoneNumberColumn => this.columnPhoneNumber;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn ExtensionColumn => this.columnExtension;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn FaxNumberColumn => this.columnFaxNumber;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn CellNumberColumn => this.columnCellNumber;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn EmailAddressColumn => this.columnEmailAddress;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn AltEmailAddressColumn => this.columnAltEmailAddress;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn ContractNoColumn => this.columnContractNo;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn CustomerTypeColumn => this.columnCustomerType;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn EquipTypeColumn => this.columnEquipType;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn CoffeePreferenceColumn => this.columnCoffeePreference;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn PriPrefQtyColumn => this.columnPriPrefQty;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn SecondaryPreferenceColumn => this.columnSecondaryPreference;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn SecPrefQtyColumn => this.columnSecPrefQty;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn TypicallySecTooColumn => this.columnTypicallySecToo;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn PreferedAgentColumn => this.columnPreferedAgent;

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

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn AlwaysSendChkUpColumn => this.columnAlwaysSendChkUp;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn NormallyRespondsColumn => this.columnNormallyResponds;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn NotesColumn => this.columnNotes;

            [DebuggerNonUserCode]
            [Browsable(false)]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public int Count => this.Rows.Count;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public TrackerDataSet.CustomersTblRow this[int index]
            {
                get => (TrackerDataSet.CustomersTblRow)this.Rows[index];
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event TrackerDataSet.CustomersTblRowChangeEventHandler CustomersTblRowChanging;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event TrackerDataSet.CustomersTblRowChangeEventHandler CustomersTblRowChanged;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event TrackerDataSet.CustomersTblRowChangeEventHandler CustomersTblRowDeleting;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event TrackerDataSet.CustomersTblRowChangeEventHandler CustomersTblRowDeleted;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void AddCustomersTblRow(TrackerDataSet.CustomersTblRow row)
            {
                this.Rows.Add((DataRow)row);
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public TrackerDataSet.CustomersTblRow AddCustomersTblRow(
              string CompanyName,
              string ContactTitle,
              string ContactFirstName,
              string ContactLastName,
              string ContactAltFirstName,
              string ContactAltLastName,
              string Department,
              string BillingAddress,
              TrackerDataSet.CityTblRow parentCityTblRowByCustomerCityRelation,
              string StateOrProvince,
              string PostalCode,
              string _Country_Region,
              string PhoneNumber,
              string Extension,
              string FaxNumber,
              string CellNumber,
              string EmailAddress,
              string AltEmailAddress,
              string ContractNo,
              string CustomerType,
              int EquipType,
              TrackerDataSet.ItemTypeTblRow parentItemTypeTblRowByPrimaryItemPrefernce,
              float PriPrefQty,
              TrackerDataSet.ItemTypeTblRow parentItemTypeTblRowBySecondaryItemPreference,
              float SecPrefQty,
              bool TypicallySecToo,
              int PreferedAgent,
              string MachineSN,
              bool UsesFilter,
              bool autofulfill,
              bool enabled,
              bool PredictionDisabled,
              bool AlwaysSendChkUp,
              bool NormallyResponds,
              string Notes)
            {
                TrackerDataSet.CustomersTblRow row = (TrackerDataSet.CustomersTblRow)this.NewRow();
                object[] objArray = new object[36]
                {
        null,
        (object) CompanyName,
        (object) ContactTitle,
        (object) ContactFirstName,
        (object) ContactLastName,
        (object) ContactAltFirstName,
        (object) ContactAltLastName,
        (object) Department,
        (object) BillingAddress,
        null,
        (object) StateOrProvince,
        (object) PostalCode,
        (object) _Country_Region,
        (object) PhoneNumber,
        (object) Extension,
        (object) FaxNumber,
        (object) CellNumber,
        (object) EmailAddress,
        (object) AltEmailAddress,
        (object) ContractNo,
        (object) CustomerType,
        (object) EquipType,
        null,
        (object) PriPrefQty,
        null,
        (object) SecPrefQty,
        (object) TypicallySecToo,
        (object) PreferedAgent,
        (object) MachineSN,
        (object) UsesFilter,
        (object) autofulfill,
        (object) enabled,
        (object) PredictionDisabled,
        (object) AlwaysSendChkUp,
        (object) NormallyResponds,
        (object) Notes
                };
                if (parentCityTblRowByCustomerCityRelation != null)
                    objArray[9] = parentCityTblRowByCustomerCityRelation[0];
                if (parentItemTypeTblRowByPrimaryItemPrefernce != null)
                    objArray[22] = parentItemTypeTblRowByPrimaryItemPrefernce[0];
                if (parentItemTypeTblRowBySecondaryItemPreference != null)
                    objArray[24] = parentItemTypeTblRowBySecondaryItemPreference[0];
                row.ItemArray = objArray;
                this.Rows.Add((DataRow)row);
                return row;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public TrackerDataSet.CustomersTblRow FindByCustomerID(int CustomerID)
            {
                return (TrackerDataSet.CustomersTblRow)this.Rows.Find(new object[1]
                {
        (object) CustomerID
                });
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public override DataTable Clone()
            {
                TrackerDataSet.CustomersTblDataTable customersTblDataTable = (TrackerDataSet.CustomersTblDataTable)base.Clone();
                customersTblDataTable.InitVars();
                return (DataTable)customersTblDataTable;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            protected override DataTable CreateInstance()
            {
                return (DataTable)new TrackerDataSet.CustomersTblDataTable();
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            internal void InitVars()
            {
                this.columnCustomerID = this.Columns["CustomerID"];
                this.columnCompanyName = this.Columns["CompanyName"];
                this.columnContactTitle = this.Columns["ContactTitle"];
                this.columnContactFirstName = this.Columns["ContactFirstName"];
                this.columnContactLastName = this.Columns["ContactLastName"];
                this.columnContactAltFirstName = this.Columns["ContactAltFirstName"];
                this.columnContactAltLastName = this.Columns["ContactAltLastName"];
                this.columnDepartment = this.Columns["Department"];
                this.columnBillingAddress = this.Columns["BillingAddress"];
                this.columnCity = this.Columns["City"];
                this.columnStateOrProvince = this.Columns["StateOrProvince"];
                this.columnPostalCode = this.Columns["PostalCode"];
                this._columnCountry_Region = this.Columns["Country/Region"];
                this.columnPhoneNumber = this.Columns["PhoneNumber"];
                this.columnExtension = this.Columns["Extension"];
                this.columnFaxNumber = this.Columns["FaxNumber"];
                this.columnCellNumber = this.Columns["CellNumber"];
                this.columnEmailAddress = this.Columns["EmailAddress"];
                this.columnAltEmailAddress = this.Columns["AltEmailAddress"];
                this.columnContractNo = this.Columns["ContractNo"];
                this.columnCustomerType = this.Columns["CustomerType"];
                this.columnEquipType = this.Columns["EquipType"];
                this.columnCoffeePreference = this.Columns["CoffeePreference"];
                this.columnPriPrefQty = this.Columns["PriPrefQty"];
                this.columnSecondaryPreference = this.Columns["SecondaryPreference"];
                this.columnSecPrefQty = this.Columns["SecPrefQty"];
                this.columnTypicallySecToo = this.Columns["TypicallySecToo"];
                this.columnPreferedAgent = this.Columns["PreferedAgent"];
                this.columnMachineSN = this.Columns["MachineSN"];
                this.columnUsesFilter = this.Columns["UsesFilter"];
                this.columnautofulfill = this.Columns["autofulfill"];
                this.columnenabled = this.Columns["enabled"];
                this.columnPredictionDisabled = this.Columns["PredictionDisabled"];
                this.columnAlwaysSendChkUp = this.Columns["AlwaysSendChkUp"];
                this.columnNormallyResponds = this.Columns["NormallyResponds"];
                this.columnNotes = this.Columns["Notes"];
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            private void InitClass()
            {
                this.columnCustomerID = new DataColumn("CustomerID", typeof(int), (string)null, MappingType.Element);
                this.Columns.Add(this.columnCustomerID);
                this.columnCompanyName = new DataColumn("CompanyName", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnCompanyName);
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
                this.columnCity = new DataColumn("City", typeof(int), (string)null, MappingType.Element);
                this.Columns.Add(this.columnCity);
                this.columnStateOrProvince = new DataColumn("StateOrProvince", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnStateOrProvince);
                this.columnPostalCode = new DataColumn("PostalCode", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnPostalCode);
                this._columnCountry_Region = new DataColumn("Country/Region", typeof(string), (string)null, MappingType.Element);
                this._columnCountry_Region.ExtendedProperties.Add((object)"Generator_ColumnVarNameInTable", (object)"_columnCountry_Region");
                this._columnCountry_Region.ExtendedProperties.Add((object)"Generator_UserColumnName", (object)"Country/Region");
                this.Columns.Add(this._columnCountry_Region);
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
                this.columnContractNo = new DataColumn("ContractNo", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnContractNo);
                this.columnCustomerType = new DataColumn("CustomerType", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnCustomerType);
                this.columnEquipType = new DataColumn("EquipType", typeof(int), (string)null, MappingType.Element);
                this.Columns.Add(this.columnEquipType);
                this.columnCoffeePreference = new DataColumn("CoffeePreference", typeof(int), (string)null, MappingType.Element);
                this.Columns.Add(this.columnCoffeePreference);
                this.columnPriPrefQty = new DataColumn("PriPrefQty", typeof(float), (string)null, MappingType.Element);
                this.Columns.Add(this.columnPriPrefQty);
                this.columnSecondaryPreference = new DataColumn("SecondaryPreference", typeof(int), (string)null, MappingType.Element);
                this.Columns.Add(this.columnSecondaryPreference);
                this.columnSecPrefQty = new DataColumn("SecPrefQty", typeof(float), (string)null, MappingType.Element);
                this.Columns.Add(this.columnSecPrefQty);
                this.columnTypicallySecToo = new DataColumn("TypicallySecToo", typeof(bool), (string)null, MappingType.Element);
                this.Columns.Add(this.columnTypicallySecToo);
                this.columnPreferedAgent = new DataColumn("PreferedAgent", typeof(int), (string)null, MappingType.Element);
                this.Columns.Add(this.columnPreferedAgent);
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
                this.Constraints.Add((Constraint)new UniqueConstraint("Constraint1", new DataColumn[1]
                {
        this.columnCustomerID
                }, true));
                this.columnCustomerID.AutoIncrement = true;
                this.columnCustomerID.AutoIncrementSeed = -1L;
                this.columnCustomerID.AutoIncrementStep = -1L;
                this.columnCustomerID.AllowDBNull = false;
                this.columnCustomerID.Unique = true;
                this.columnCompanyName.MaxLength = 50;
                this.columnContactTitle.MaxLength = 50;
                this.columnContactFirstName.MaxLength = 30;
                this.columnContactLastName.MaxLength = 50;
                this.columnContactAltFirstName.MaxLength = 50;
                this.columnContactAltLastName.MaxLength = 50;
                this.columnDepartment.MaxLength = 50;
                this.columnBillingAddress.MaxLength = (int)byte.MaxValue;
                this.columnStateOrProvince.MaxLength = 20;
                this.columnPostalCode.MaxLength = 20;
                this._columnCountry_Region.MaxLength = 50;
                this.columnPhoneNumber.MaxLength = 30;
                this.columnExtension.MaxLength = 30;
                this.columnFaxNumber.MaxLength = 30;
                this.columnCellNumber.MaxLength = 50;
                this.columnEmailAddress.MaxLength = 50;
                this.columnAltEmailAddress.MaxLength = (int)byte.MaxValue;
                this.columnContractNo.MaxLength = 50;
                this.columnCustomerType.MaxLength = 30;
                this.columnMachineSN.MaxLength = 50;
                this.columnNotes.MaxLength = 536870910;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public TrackerDataSet.CustomersTblRow NewCustomersTblRow()
            {
                return (TrackerDataSet.CustomersTblRow)this.NewRow();
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
            {
                return (DataRow)new TrackerDataSet.CustomersTblRow(builder);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected override Type GetRowType() => typeof(TrackerDataSet.CustomersTblRow);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            protected override void OnRowChanged(DataRowChangeEventArgs e)
            {
                base.OnRowChanged(e);
                if (this.CustomersTblRowChanged == null)
                    return;
                this.CustomersTblRowChanged((object)this, new TrackerDataSet.CustomersTblRowChangeEvent((TrackerDataSet.CustomersTblRow)e.Row, e.Action));
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected override void OnRowChanging(DataRowChangeEventArgs e)
            {
                base.OnRowChanging(e);
                if (this.CustomersTblRowChanging == null)
                    return;
                this.CustomersTblRowChanging((object)this, new TrackerDataSet.CustomersTblRowChangeEvent((TrackerDataSet.CustomersTblRow)e.Row, e.Action));
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            protected override void OnRowDeleted(DataRowChangeEventArgs e)
            {
                base.OnRowDeleted(e);
                if (this.CustomersTblRowDeleted == null)
                    return;
                this.CustomersTblRowDeleted((object)this, new TrackerDataSet.CustomersTblRowChangeEvent((TrackerDataSet.CustomersTblRow)e.Row, e.Action));
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected override void OnRowDeleting(DataRowChangeEventArgs e)
            {
                base.OnRowDeleting(e);
                if (this.CustomersTblRowDeleting == null)
                    return;
                this.CustomersTblRowDeleting((object)this, new TrackerDataSet.CustomersTblRowChangeEvent((TrackerDataSet.CustomersTblRow)e.Row, e.Action));
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void RemoveCustomersTblRow(TrackerDataSet.CustomersTblRow row)
            {
                this.Rows.Remove((DataRow)row);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
            {
                XmlSchemaComplexType typedTableSchema = new XmlSchemaComplexType();
                XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
                TrackerDataSet trackerDataSet = new TrackerDataSet();
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
                    FixedValue = trackerDataSet.Namespace
                });
                typedTableSchema.Attributes.Add((XmlSchemaObject)new XmlSchemaAttribute()
                {
                    Name = "tableTypeName",
                    FixedValue = nameof(CustomersTblDataTable)
                });
                typedTableSchema.Particle = (XmlSchemaParticle)xmlSchemaSequence;
                XmlSchema schemaSerializable = trackerDataSet.GetSchemaSerializable();
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

        [XmlSchemaProvider("GetTypedTableSchema")]
        [Serializable]
        public class ItemTypeTblDataTable : TypedTableBase<TrackerDataSet.ItemTypeTblRow>
        {
            private DataColumn columnItemTypeID;
            private DataColumn columnItemDesc;
            private DataColumn columnItemEnabled;
            private DataColumn columnItemsCharacteritics;
            private DataColumn columnItemDetail;
            private DataColumn columnServiceTypeID;
            private DataColumn columnReplacementID;
            private DataColumn columnSortOrder;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public ItemTypeTblDataTable()
            {
                this.TableName = "ItemTypeTbl";
                this.BeginInit();
                this.InitClass();
                this.EndInit();
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            internal ItemTypeTblDataTable(DataTable table)
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

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected ItemTypeTblDataTable(SerializationInfo info, StreamingContext context)
              : base(info, context)
            {
                this.InitVars();
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn ItemTypeIDColumn => this.columnItemTypeID;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn ItemDescColumn => this.columnItemDesc;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn ItemEnabledColumn => this.columnItemEnabled;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn ItemsCharacteriticsColumn => this.columnItemsCharacteritics;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn ItemDetailColumn => this.columnItemDetail;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn ServiceTypeIDColumn => this.columnServiceTypeID;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn ReplacementIDColumn => this.columnReplacementID;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn SortOrderColumn => this.columnSortOrder;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            [Browsable(false)]
            public int Count => this.Rows.Count;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public TrackerDataSet.ItemTypeTblRow this[int index]
            {
                get => (TrackerDataSet.ItemTypeTblRow)this.Rows[index];
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event TrackerDataSet.ItemTypeTblRowChangeEventHandler ItemTypeTblRowChanging;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event TrackerDataSet.ItemTypeTblRowChangeEventHandler ItemTypeTblRowChanged;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event TrackerDataSet.ItemTypeTblRowChangeEventHandler ItemTypeTblRowDeleting;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event TrackerDataSet.ItemTypeTblRowChangeEventHandler ItemTypeTblRowDeleted;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void AddItemTypeTblRow(TrackerDataSet.ItemTypeTblRow row)
            {
                this.Rows.Add((DataRow)row);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public TrackerDataSet.ItemTypeTblRow AddItemTypeTblRow(
              string ItemDesc,
              bool ItemEnabled,
              string ItemsCharacteritics,
              string ItemDetail,
              int ServiceTypeID,
              int ReplacementID,
              int SortOrder)
            {
                TrackerDataSet.ItemTypeTblRow row = (TrackerDataSet.ItemTypeTblRow)this.NewRow();
                object[] objArray = new object[8]
                {
        null,
        (object) ItemDesc,
        (object) ItemEnabled,
        (object) ItemsCharacteritics,
        (object) ItemDetail,
        (object) ServiceTypeID,
        (object) ReplacementID,
        (object) SortOrder
                };
                row.ItemArray = objArray;
                this.Rows.Add((DataRow)row);
                return row;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public TrackerDataSet.ItemTypeTblRow FindByItemTypeID(int ItemTypeID)
            {
                return (TrackerDataSet.ItemTypeTblRow)this.Rows.Find(new object[1]
                {
        (object) ItemTypeID
                });
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public override DataTable Clone()
            {
                TrackerDataSet.ItemTypeTblDataTable typeTblDataTable = (TrackerDataSet.ItemTypeTblDataTable)base.Clone();
                typeTblDataTable.InitVars();
                return (DataTable)typeTblDataTable;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected override DataTable CreateInstance()
            {
                return (DataTable)new TrackerDataSet.ItemTypeTblDataTable();
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            internal void InitVars()
            {
                this.columnItemTypeID = this.Columns["ItemTypeID"];
                this.columnItemDesc = this.Columns["ItemDesc"];
                this.columnItemEnabled = this.Columns["ItemEnabled"];
                this.columnItemsCharacteritics = this.Columns["ItemsCharacteritics"];
                this.columnItemDetail = this.Columns["ItemDetail"];
                this.columnServiceTypeID = this.Columns["ServiceTypeID"];
                this.columnReplacementID = this.Columns["ReplacementID"];
                this.columnSortOrder = this.Columns["SortOrder"];
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            private void InitClass()
            {
                this.columnItemTypeID = new DataColumn("ItemTypeID", typeof(int), (string)null, MappingType.Element);
                this.Columns.Add(this.columnItemTypeID);
                this.columnItemDesc = new DataColumn("ItemDesc", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnItemDesc);
                this.columnItemEnabled = new DataColumn("ItemEnabled", typeof(bool), (string)null, MappingType.Element);
                this.Columns.Add(this.columnItemEnabled);
                this.columnItemsCharacteritics = new DataColumn("ItemsCharacteritics", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnItemsCharacteritics);
                this.columnItemDetail = new DataColumn("ItemDetail", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnItemDetail);
                this.columnServiceTypeID = new DataColumn("ServiceTypeID", typeof(int), (string)null, MappingType.Element);
                this.Columns.Add(this.columnServiceTypeID);
                this.columnReplacementID = new DataColumn("ReplacementID", typeof(int), (string)null, MappingType.Element);
                this.Columns.Add(this.columnReplacementID);
                this.columnSortOrder = new DataColumn("SortOrder", typeof(int), (string)null, MappingType.Element);
                this.Columns.Add(this.columnSortOrder);
                this.Constraints.Add((Constraint)new UniqueConstraint("Constraint1", new DataColumn[1]
                {
        this.columnItemTypeID
                }, true));
                this.columnItemTypeID.AutoIncrement = true;
                this.columnItemTypeID.AutoIncrementSeed = -1L;
                this.columnItemTypeID.AutoIncrementStep = -1L;
                this.columnItemTypeID.AllowDBNull = false;
                this.columnItemTypeID.Unique = true;
                this.columnItemDesc.MaxLength = 50;
                this.columnItemsCharacteritics.MaxLength = 50;
                this.columnItemDetail.MaxLength = 536870910;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public TrackerDataSet.ItemTypeTblRow NewItemTypeTblRow()
            {
                return (TrackerDataSet.ItemTypeTblRow)this.NewRow();
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
            {
                return (DataRow)new TrackerDataSet.ItemTypeTblRow(builder);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected override Type GetRowType() => typeof(TrackerDataSet.ItemTypeTblRow);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected override void OnRowChanged(DataRowChangeEventArgs e)
            {
                base.OnRowChanged(e);
                if (this.ItemTypeTblRowChanged == null)
                    return;
                this.ItemTypeTblRowChanged((object)this, new TrackerDataSet.ItemTypeTblRowChangeEvent((TrackerDataSet.ItemTypeTblRow)e.Row, e.Action));
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected override void OnRowChanging(DataRowChangeEventArgs e)
            {
                base.OnRowChanging(e);
                if (this.ItemTypeTblRowChanging == null)
                    return;
                this.ItemTypeTblRowChanging((object)this, new TrackerDataSet.ItemTypeTblRowChangeEvent((TrackerDataSet.ItemTypeTblRow)e.Row, e.Action));
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected override void OnRowDeleted(DataRowChangeEventArgs e)
            {
                base.OnRowDeleted(e);
                if (this.ItemTypeTblRowDeleted == null)
                    return;
                this.ItemTypeTblRowDeleted((object)this, new TrackerDataSet.ItemTypeTblRowChangeEvent((TrackerDataSet.ItemTypeTblRow)e.Row, e.Action));
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            protected override void OnRowDeleting(DataRowChangeEventArgs e)
            {
                base.OnRowDeleting(e);
                if (this.ItemTypeTblRowDeleting == null)
                    return;
                this.ItemTypeTblRowDeleting((object)this, new TrackerDataSet.ItemTypeTblRowChangeEvent((TrackerDataSet.ItemTypeTblRow)e.Row, e.Action));
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void RemoveItemTypeTblRow(TrackerDataSet.ItemTypeTblRow row)
            {
                this.Rows.Remove((DataRow)row);
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
            {
                XmlSchemaComplexType typedTableSchema = new XmlSchemaComplexType();
                XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
                TrackerDataSet trackerDataSet = new TrackerDataSet();
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
                    FixedValue = trackerDataSet.Namespace
                });
                typedTableSchema.Attributes.Add((XmlSchemaObject)new XmlSchemaAttribute()
                {
                    Name = "tableTypeName",
                    FixedValue = nameof(ItemTypeTblDataTable)
                });
                typedTableSchema.Particle = (XmlSchemaParticle)xmlSchemaSequence;
                XmlSchema schemaSerializable = trackerDataSet.GetSchemaSerializable();
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

        [XmlSchemaProvider("GetTypedTableSchema")]
        [Serializable]
        public class CityTblDataTable : TypedTableBase<TrackerDataSet.CityTblRow>
        {
            private DataColumn columnID;
            private DataColumn columnCity;
            private DataColumn columnRoastingDay;
            private DataColumn columnDeliveryDelay;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public CityTblDataTable()
            {
                this.TableName = "CityTbl";
                this.BeginInit();
                this.InitClass();
                this.EndInit();
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            internal CityTblDataTable(DataTable table)
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
            protected CityTblDataTable(SerializationInfo info, StreamingContext context)
              : base(info, context)
            {
                this.InitVars();
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn IDColumn => this.columnID;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn CityColumn => this.columnCity;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataColumn RoastingDayColumn => this.columnRoastingDay;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn DeliveryDelayColumn => this.columnDeliveryDelay;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [Browsable(false)]
            [DebuggerNonUserCode]
            public int Count => this.Rows.Count;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public TrackerDataSet.CityTblRow this[int index]
            {
                get => (TrackerDataSet.CityTblRow)this.Rows[index];
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event TrackerDataSet.CityTblRowChangeEventHandler CityTblRowChanging;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event TrackerDataSet.CityTblRowChangeEventHandler CityTblRowChanged;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event TrackerDataSet.CityTblRowChangeEventHandler CityTblRowDeleting;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event TrackerDataSet.CityTblRowChangeEventHandler CityTblRowDeleted;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void AddCityTblRow(TrackerDataSet.CityTblRow row) => this.Rows.Add((DataRow)row);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public TrackerDataSet.CityTblRow AddCityTblRow(string City, int RoastingDay, int DeliveryDelay)
            {
                TrackerDataSet.CityTblRow row = (TrackerDataSet.CityTblRow)this.NewRow();
                object[] objArray = new object[4]
                {
        null,
        (object) City,
        (object) RoastingDay,
        (object) DeliveryDelay
                };
                row.ItemArray = objArray;
                this.Rows.Add((DataRow)row);
                return row;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public TrackerDataSet.CityTblRow FindByID(int ID)
            {
                return (TrackerDataSet.CityTblRow)this.Rows.Find(new object[1]
                {
        (object) ID
                });
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public override DataTable Clone()
            {
                TrackerDataSet.CityTblDataTable cityTblDataTable = (TrackerDataSet.CityTblDataTable)base.Clone();
                cityTblDataTable.InitVars();
                return (DataTable)cityTblDataTable;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected override DataTable CreateInstance()
            {
                return (DataTable)new TrackerDataSet.CityTblDataTable();
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            internal void InitVars()
            {
                this.columnID = this.Columns["ID"];
                this.columnCity = this.Columns["City"];
                this.columnRoastingDay = this.Columns["RoastingDay"];
                this.columnDeliveryDelay = this.Columns["DeliveryDelay"];
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            private void InitClass()
            {
                this.columnID = new DataColumn("ID", typeof(int), (string)null, MappingType.Element);
                this.Columns.Add(this.columnID);
                this.columnCity = new DataColumn("City", typeof(string), (string)null, MappingType.Element);
                this.Columns.Add(this.columnCity);
                this.columnRoastingDay = new DataColumn("RoastingDay", typeof(int), (string)null, MappingType.Element);
                this.Columns.Add(this.columnRoastingDay);
                this.columnDeliveryDelay = new DataColumn("DeliveryDelay", typeof(int), (string)null, MappingType.Element);
                this.Columns.Add(this.columnDeliveryDelay);
                this.Constraints.Add((Constraint)new UniqueConstraint("Constraint1", new DataColumn[1]
                {
        this.columnID
                }, true));
                this.columnID.AutoIncrement = true;
                this.columnID.AutoIncrementSeed = -1L;
                this.columnID.AutoIncrementStep = -1L;
                this.columnID.AllowDBNull = false;
                this.columnID.Unique = true;
                this.columnCity.MaxLength = (int)byte.MaxValue;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public TrackerDataSet.CityTblRow NewCityTblRow() => (TrackerDataSet.CityTblRow)this.NewRow();

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
            {
                return (DataRow)new TrackerDataSet.CityTblRow(builder);
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            protected override Type GetRowType() => typeof(TrackerDataSet.CityTblRow);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected override void OnRowChanged(DataRowChangeEventArgs e)
            {
                base.OnRowChanged(e);
                if (this.CityTblRowChanged == null)
                    return;
                this.CityTblRowChanged((object)this, new TrackerDataSet.CityTblRowChangeEvent((TrackerDataSet.CityTblRow)e.Row, e.Action));
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            protected override void OnRowChanging(DataRowChangeEventArgs e)
            {
                base.OnRowChanging(e);
                if (this.CityTblRowChanging == null)
                    return;
                this.CityTblRowChanging((object)this, new TrackerDataSet.CityTblRowChangeEvent((TrackerDataSet.CityTblRow)e.Row, e.Action));
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            protected override void OnRowDeleted(DataRowChangeEventArgs e)
            {
                base.OnRowDeleted(e);
                if (this.CityTblRowDeleted == null)
                    return;
                this.CityTblRowDeleted((object)this, new TrackerDataSet.CityTblRowChangeEvent((TrackerDataSet.CityTblRow)e.Row, e.Action));
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected override void OnRowDeleting(DataRowChangeEventArgs e)
            {
                base.OnRowDeleting(e);
                if (this.CityTblRowDeleting == null)
                    return;
                this.CityTblRowDeleting((object)this, new TrackerDataSet.CityTblRowChangeEvent((TrackerDataSet.CityTblRow)e.Row, e.Action));
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void RemoveCityTblRow(TrackerDataSet.CityTblRow row) => this.Rows.Remove((DataRow)row);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
            {
                XmlSchemaComplexType typedTableSchema = new XmlSchemaComplexType();
                XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
                TrackerDataSet trackerDataSet = new TrackerDataSet();
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
                    FixedValue = trackerDataSet.Namespace
                });
                typedTableSchema.Attributes.Add((XmlSchemaObject)new XmlSchemaAttribute()
                {
                    Name = "tableTypeName",
                    FixedValue = nameof(CityTblDataTable)
                });
                typedTableSchema.Particle = (XmlSchemaParticle)xmlSchemaSequence;
                XmlSchema schemaSerializable = trackerDataSet.GetSchemaSerializable();
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

        public class OrdersTblRow : DataRow
        {
            private TrackerDataSet.OrderTblDataTable tableOrdersTbl;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            internal OrdersTblRow(DataRowBuilder rb)
              : base(rb)
            {
                this.tableOrdersTbl = (TrackerDataSet.OrderTblDataTable)this.Table;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public int OrderID
            {
                get => (int)this[this.tableOrdersTbl.OrderIDColumn];
                set => this[this.tableOrdersTbl.OrderIDColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public long CustomerID
            {
                get
                {
                    try
                    {
                        return (int)this[this.tableOrdersTbl.CustomerIDColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'CustomerID' in table 'OrdersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableOrdersTbl.CustomerIDColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DateTime OrderDate
            {
                get
                {
                    try
                    {
                        return (DateTime)this[this.tableOrdersTbl.OrderDateColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'OrderDate' in table 'OrdersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableOrdersTbl.OrderDateColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DateTime RoastDate
            {
                get
                {
                    try
                    {
                        return (DateTime)this[this.tableOrdersTbl.RoastDateColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'RoastDate' in table 'OrdersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableOrdersTbl.RoastDateColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public int ItemTypeID
            {
                get
                {
                    try
                    {
                        return (int)this[this.tableOrdersTbl.ItemTypeIDColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'ItemTypeID' in table 'OrdersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableOrdersTbl.ItemTypeIDColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public float QuantityOrdered
            {
                get
                {
                    try
                    {
                        return (float)this[this.tableOrdersTbl.QuantityOrderedColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'QuantityOrdered' in table 'OrdersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableOrdersTbl.QuantityOrderedColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DateTime RequiredByDate
            {
                get
                {
                    try
                    {
                        return (DateTime)this[this.tableOrdersTbl.RequiredByDateColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'RequiredByDate' in table 'OrdersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableOrdersTbl.RequiredByDateColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public int ToBeDeliveredBy
            {
                get
                {
                    try
                    {
                        return (int)this[this.tableOrdersTbl.ToBeDeliveredByColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'ToBeDeliveredBy' in table 'OrdersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableOrdersTbl.ToBeDeliveredByColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool Confirmed
            {
                get
                {
                    try
                    {
                        return (bool)this[this.tableOrdersTbl.ConfirmedColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'Confirmed' in table 'OrdersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableOrdersTbl.ConfirmedColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool Done
            {
                get
                {
                    try
                    {
                        return (bool)this[this.tableOrdersTbl.DoneColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'Done' in table 'OrdersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableOrdersTbl.DoneColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string Notes
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableOrdersTbl.NotesColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'Notes' in table 'OrdersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableOrdersTbl.NotesColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public TrackerDataSet.CustomersTblRow CustomersTblRow
            {
                get
                {
                    return (TrackerDataSet.CustomersTblRow)this.GetParentRow(this.Table.ParentRelations["OrdersToCustomerRelation"]);
                }
                set
                {
                    this.SetParentRow((DataRow)value, this.Table.ParentRelations["OrdersToCustomerRelation"]);
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public TrackerDataSet.ItemTypeTblRow ItemTypeTblRow
            {
                get
                {
                    return (TrackerDataSet.ItemTypeTblRow)this.GetParentRow(this.Table.ParentRelations["OrderItemToItemIDRelation"]);
                }
                set
                {
                    this.SetParentRow((DataRow)value, this.Table.ParentRelations["OrderItemToItemIDRelation"]);
                }
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsCustomerIDNull() => this.IsNull(this.tableOrdersTbl.CustomerIDColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetCustomerIDNull() => this[this.tableOrdersTbl.CustomerIDColumn] = Convert.DBNull;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsOrderDateNull() => this.IsNull(this.tableOrdersTbl.OrderDateColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetOrderDateNull() => this[this.tableOrdersTbl.OrderDateColumn] = Convert.DBNull;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsRoastDateNull() => this.IsNull(this.tableOrdersTbl.RoastDateColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetRoastDateNull() => this[this.tableOrdersTbl.RoastDateColumn] = Convert.DBNull;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsItemTypeIDNull() => this.IsNull(this.tableOrdersTbl.ItemTypeIDColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetItemTypeIDNull() => this[this.tableOrdersTbl.ItemTypeIDColumn] = Convert.DBNull;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsQuantityOrderedNull() => this.IsNull(this.tableOrdersTbl.QuantityOrderedColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetQuantityOrderedNull()
            {
                this[this.tableOrdersTbl.QuantityOrderedColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsRequiredByDateNull() => this.IsNull(this.tableOrdersTbl.RequiredByDateColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetRequiredByDateNull()
            {
                this[this.tableOrdersTbl.RequiredByDateColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsToBeDeliveredByNull() => this.IsNull(this.tableOrdersTbl.ToBeDeliveredByColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetToBeDeliveredByNull()
            {
                this[this.tableOrdersTbl.ToBeDeliveredByColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsConfirmedNull() => this.IsNull(this.tableOrdersTbl.ConfirmedColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetConfirmedNull() => this[this.tableOrdersTbl.ConfirmedColumn] = Convert.DBNull;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsDoneNull() => this.IsNull(this.tableOrdersTbl.DoneColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetDoneNull() => this[this.tableOrdersTbl.DoneColumn] = Convert.DBNull;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsNotesNull() => this.IsNull(this.tableOrdersTbl.NotesColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetNotesNull() => this[this.tableOrdersTbl.NotesColumn] = Convert.DBNull;
        }

        public class CustomersTblRow : DataRow
        {
            private TrackerDataSet.CustomersTblDataTable tableCustomersTbl;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            internal CustomersTblRow(DataRowBuilder rb)
              : base(rb)
            {
                this.tableCustomersTbl = (TrackerDataSet.CustomersTblDataTable)this.Table;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public long CustomerID
            {
                get => (int)this[this.tableCustomersTbl.CustomerIDColumn];
                set => this[this.tableCustomersTbl.CustomerIDColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public string CompanyName
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomersTbl.CompanyNameColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'CompanyName' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.CompanyNameColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string ContactTitle
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomersTbl.ContactTitleColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'ContactTitle' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.ContactTitleColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public string ContactFirstName
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomersTbl.ContactFirstNameColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'ContactFirstName' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.ContactFirstNameColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public string ContactLastName
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomersTbl.ContactLastNameColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'ContactLastName' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.ContactLastNameColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string ContactAltFirstName
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomersTbl.ContactAltFirstNameColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'ContactAltFirstName' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.ContactAltFirstNameColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public string ContactAltLastName
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomersTbl.ContactAltLastNameColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'ContactAltLastName' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.ContactAltLastNameColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string Department
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomersTbl.DepartmentColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'Department' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.DepartmentColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public string BillingAddress
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomersTbl.BillingAddressColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'BillingAddress' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.BillingAddressColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public int City
            {
                get
                {
                    try
                    {
                        return (int)this[this.tableCustomersTbl.CityColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'City' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.CityColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public string StateOrProvince
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomersTbl.StateOrProvinceColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'StateOrProvince' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.StateOrProvinceColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string PostalCode
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomersTbl.PostalCodeColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'PostalCode' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.PostalCodeColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string _Country_Region
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomersTbl._Country_RegionColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'Country/Region' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl._Country_RegionColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string PhoneNumber
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomersTbl.PhoneNumberColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'PhoneNumber' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.PhoneNumberColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string Extension
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomersTbl.ExtensionColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'Extension' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.ExtensionColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public string FaxNumber
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomersTbl.FaxNumberColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'FaxNumber' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.FaxNumberColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public string CellNumber
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomersTbl.CellNumberColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'CellNumber' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.CellNumberColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string EmailAddress
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomersTbl.EmailAddressColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'EmailAddress' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.EmailAddressColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public string AltEmailAddress
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomersTbl.AltEmailAddressColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'AltEmailAddress' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.AltEmailAddressColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public string ContractNo
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomersTbl.ContractNoColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'ContractNo' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.ContractNoColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string CustomerType
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomersTbl.CustomerTypeColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'CustomerType' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.CustomerTypeColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public int EquipType
            {
                get
                {
                    try
                    {
                        return (int)this[this.tableCustomersTbl.EquipTypeColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'EquipType' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.EquipTypeColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public int CoffeePreference
            {
                get
                {
                    try
                    {
                        return (int)this[this.tableCustomersTbl.CoffeePreferenceColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'CoffeePreference' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.CoffeePreferenceColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public float PriPrefQty
            {
                get
                {
                    try
                    {
                        return (float)this[this.tableCustomersTbl.PriPrefQtyColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'PriPrefQty' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.PriPrefQtyColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public int SecondaryPreference
            {
                get
                {
                    try
                    {
                        return (int)this[this.tableCustomersTbl.SecondaryPreferenceColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'SecondaryPreference' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.SecondaryPreferenceColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public float SecPrefQty
            {
                get
                {
                    try
                    {
                        return (float)this[this.tableCustomersTbl.SecPrefQtyColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'SecPrefQty' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.SecPrefQtyColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool TypicallySecToo
            {
                get
                {
                    try
                    {
                        return (bool)this[this.tableCustomersTbl.TypicallySecTooColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'TypicallySecToo' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.TypicallySecTooColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public int PreferedAgent
            {
                get
                {
                    try
                    {
                        return (int)this[this.tableCustomersTbl.PreferedAgentColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'PreferedAgent' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.PreferedAgentColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string MachineSN
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomersTbl.MachineSNColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'MachineSN' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.MachineSNColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool UsesFilter
            {
                get
                {
                    try
                    {
                        return (bool)this[this.tableCustomersTbl.UsesFilterColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'UsesFilter' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.UsesFilterColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool autofulfill
            {
                get
                {
                    try
                    {
                        return (bool)this[this.tableCustomersTbl.autofulfillColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'autofulfill' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.autofulfillColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool enabled
            {
                get
                {
                    try
                    {
                        return (bool)this[this.tableCustomersTbl.enabledColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'enabled' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.enabledColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool PredictionDisabled
            {
                get
                {
                    try
                    {
                        return (bool)this[this.tableCustomersTbl.PredictionDisabledColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'PredictionDisabled' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.PredictionDisabledColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool AlwaysSendChkUp
            {
                get
                {
                    try
                    {
                        return (bool)this[this.tableCustomersTbl.AlwaysSendChkUpColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'AlwaysSendChkUp' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.AlwaysSendChkUpColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool NormallyResponds
            {
                get
                {
                    try
                    {
                        return (bool)this[this.tableCustomersTbl.NormallyRespondsColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'NormallyResponds' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.NormallyRespondsColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string Notes
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCustomersTbl.NotesColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'Notes' in table 'CustomersTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCustomersTbl.NotesColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public TrackerDataSet.ItemTypeTblRow ItemTypeTblRowByPrimaryItemPrefernce
            {
                get
                {
                    return (TrackerDataSet.ItemTypeTblRow)this.GetParentRow(this.Table.ParentRelations["PrimaryItemPrefernce"]);
                }
                set => this.SetParentRow((DataRow)value, this.Table.ParentRelations["PrimaryItemPrefernce"]);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public TrackerDataSet.ItemTypeTblRow ItemTypeTblRowBySecondaryItemPreference
            {
                get
                {
                    return (TrackerDataSet.ItemTypeTblRow)this.GetParentRow(this.Table.ParentRelations["SecondaryItemPreference"]);
                }
                set
                {
                    this.SetParentRow((DataRow)value, this.Table.ParentRelations["SecondaryItemPreference"]);
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public TrackerDataSet.CityTblRow CityTblRow
            {
                get
                {
                    return (TrackerDataSet.CityTblRow)this.GetParentRow(this.Table.ParentRelations["CustomerCityRelation"]);
                }
                set => this.SetParentRow((DataRow)value, this.Table.ParentRelations["CustomerCityRelation"]);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsCompanyNameNull() => this.IsNull(this.tableCustomersTbl.CompanyNameColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetCompanyNameNull()
            {
                this[this.tableCustomersTbl.CompanyNameColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsContactTitleNull() => this.IsNull(this.tableCustomersTbl.ContactTitleColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetContactTitleNull()
            {
                this[this.tableCustomersTbl.ContactTitleColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsContactFirstNameNull()
            {
                return this.IsNull(this.tableCustomersTbl.ContactFirstNameColumn);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetContactFirstNameNull()
            {
                this[this.tableCustomersTbl.ContactFirstNameColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsContactLastNameNull()
            {
                return this.IsNull(this.tableCustomersTbl.ContactLastNameColumn);
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetContactLastNameNull()
            {
                this[this.tableCustomersTbl.ContactLastNameColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsContactAltFirstNameNull()
            {
                return this.IsNull(this.tableCustomersTbl.ContactAltFirstNameColumn);
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetContactAltFirstNameNull()
            {
                this[this.tableCustomersTbl.ContactAltFirstNameColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsContactAltLastNameNull()
            {
                return this.IsNull(this.tableCustomersTbl.ContactAltLastNameColumn);
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetContactAltLastNameNull()
            {
                this[this.tableCustomersTbl.ContactAltLastNameColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsDepartmentNull() => this.IsNull(this.tableCustomersTbl.DepartmentColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetDepartmentNull()
            {
                this[this.tableCustomersTbl.DepartmentColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsBillingAddressNull() => this.IsNull(this.tableCustomersTbl.BillingAddressColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetBillingAddressNull()
            {
                this[this.tableCustomersTbl.BillingAddressColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsCityNull() => this.IsNull(this.tableCustomersTbl.CityColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetCityNull() => this[this.tableCustomersTbl.CityColumn] = Convert.DBNull;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsStateOrProvinceNull()
            {
                return this.IsNull(this.tableCustomersTbl.StateOrProvinceColumn);
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetStateOrProvinceNull()
            {
                this[this.tableCustomersTbl.StateOrProvinceColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsPostalCodeNull() => this.IsNull(this.tableCustomersTbl.PostalCodeColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetPostalCodeNull()
            {
                this[this.tableCustomersTbl.PostalCodeColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool Is_Country_RegionNull()
            {
                return this.IsNull(this.tableCustomersTbl._Country_RegionColumn);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void Set_Country_RegionNull()
            {
                this[this.tableCustomersTbl._Country_RegionColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsPhoneNumberNull() => this.IsNull(this.tableCustomersTbl.PhoneNumberColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetPhoneNumberNull()
            {
                this[this.tableCustomersTbl.PhoneNumberColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsExtensionNull() => this.IsNull(this.tableCustomersTbl.ExtensionColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetExtensionNull() => this[this.tableCustomersTbl.ExtensionColumn] = Convert.DBNull;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsFaxNumberNull() => this.IsNull(this.tableCustomersTbl.FaxNumberColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetFaxNumberNull() => this[this.tableCustomersTbl.FaxNumberColumn] = Convert.DBNull;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsCellNumberNull() => this.IsNull(this.tableCustomersTbl.CellNumberColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetCellNumberNull()
            {
                this[this.tableCustomersTbl.CellNumberColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsEmailAddressNull() => this.IsNull(this.tableCustomersTbl.EmailAddressColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetEmailAddressNull()
            {
                this[this.tableCustomersTbl.EmailAddressColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsAltEmailAddressNull()
            {
                return this.IsNull(this.tableCustomersTbl.AltEmailAddressColumn);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetAltEmailAddressNull()
            {
                this[this.tableCustomersTbl.AltEmailAddressColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsContractNoNull() => this.IsNull(this.tableCustomersTbl.ContractNoColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetContractNoNull()
            {
                this[this.tableCustomersTbl.ContractNoColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsCustomerTypeNull() => this.IsNull(this.tableCustomersTbl.CustomerTypeColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetCustomerTypeNull()
            {
                this[this.tableCustomersTbl.CustomerTypeColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsEquipTypeNull() => this.IsNull(this.tableCustomersTbl.EquipTypeColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetEquipTypeNull() => this[this.tableCustomersTbl.EquipTypeColumn] = Convert.DBNull;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsCoffeePreferenceNull()
            {
                return this.IsNull(this.tableCustomersTbl.CoffeePreferenceColumn);
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetCoffeePreferenceNull()
            {
                this[this.tableCustomersTbl.CoffeePreferenceColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsPriPrefQtyNull() => this.IsNull(this.tableCustomersTbl.PriPrefQtyColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetPriPrefQtyNull()
            {
                this[this.tableCustomersTbl.PriPrefQtyColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsSecondaryPreferenceNull()
            {
                return this.IsNull(this.tableCustomersTbl.SecondaryPreferenceColumn);
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetSecondaryPreferenceNull()
            {
                this[this.tableCustomersTbl.SecondaryPreferenceColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsSecPrefQtyNull() => this.IsNull(this.tableCustomersTbl.SecPrefQtyColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetSecPrefQtyNull()
            {
                this[this.tableCustomersTbl.SecPrefQtyColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsTypicallySecTooNull()
            {
                return this.IsNull(this.tableCustomersTbl.TypicallySecTooColumn);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetTypicallySecTooNull()
            {
                this[this.tableCustomersTbl.TypicallySecTooColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsPreferedAgentNull() => this.IsNull(this.tableCustomersTbl.PreferedAgentColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetPreferedAgentNull()
            {
                this[this.tableCustomersTbl.PreferedAgentColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsMachineSNNull() => this.IsNull(this.tableCustomersTbl.MachineSNColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetMachineSNNull() => this[this.tableCustomersTbl.MachineSNColumn] = Convert.DBNull;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsUsesFilterNull() => this.IsNull(this.tableCustomersTbl.UsesFilterColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetUsesFilterNull()
            {
                this[this.tableCustomersTbl.UsesFilterColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsautofulfillNull() => this.IsNull(this.tableCustomersTbl.autofulfillColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetautofulfillNull()
            {
                this[this.tableCustomersTbl.autofulfillColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsenabledNull() => this.IsNull(this.tableCustomersTbl.enabledColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetenabledNull() => this[this.tableCustomersTbl.enabledColumn] = Convert.DBNull;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsPredictionDisabledNull()
            {
                return this.IsNull(this.tableCustomersTbl.PredictionDisabledColumn);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetPredictionDisabledNull()
            {
                this[this.tableCustomersTbl.PredictionDisabledColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsAlwaysSendChkUpNull()
            {
                return this.IsNull(this.tableCustomersTbl.AlwaysSendChkUpColumn);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetAlwaysSendChkUpNull()
            {
                this[this.tableCustomersTbl.AlwaysSendChkUpColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsNormallyRespondsNull()
            {
                return this.IsNull(this.tableCustomersTbl.NormallyRespondsColumn);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetNormallyRespondsNull()
            {
                this[this.tableCustomersTbl.NormallyRespondsColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsNotesNull() => this.IsNull(this.tableCustomersTbl.NotesColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetNotesNull() => this[this.tableCustomersTbl.NotesColumn] = Convert.DBNull;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public TrackerDataSet.OrdersTblRow[] GetOrdersTblRows()
            {
                return this.Table.ChildRelations["OrdersToCustomerRelation"] == null ? new TrackerDataSet.OrdersTblRow[0] : (TrackerDataSet.OrdersTblRow[])this.GetChildRows(this.Table.ChildRelations["OrdersToCustomerRelation"]);
            }
        }

        public class ItemTypeTblRow : DataRow
        {
            private TrackerDataSet.ItemTypeTblDataTable tableItemTypeTbl;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            internal ItemTypeTblRow(DataRowBuilder rb)
              : base(rb)
            {
                this.tableItemTypeTbl = (TrackerDataSet.ItemTypeTblDataTable)this.Table;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public int ItemTypeID
            {
                get => (int)this[this.tableItemTypeTbl.ItemTypeIDColumn];
                set => this[this.tableItemTypeTbl.ItemTypeIDColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string ItemDesc
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableItemTypeTbl.ItemDescColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'ItemDesc' in table 'ItemTypeTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableItemTypeTbl.ItemDescColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool ItemEnabled
            {
                get
                {
                    try
                    {
                        return (bool)this[this.tableItemTypeTbl.ItemEnabledColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'ItemEnabled' in table 'ItemTypeTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableItemTypeTbl.ItemEnabledColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string ItemsCharacteritics
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableItemTypeTbl.ItemsCharacteriticsColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'ItemsCharacteritics' in table 'ItemTypeTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableItemTypeTbl.ItemsCharacteriticsColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string ItemDetail
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableItemTypeTbl.ItemDetailColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'ItemDetail' in table 'ItemTypeTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableItemTypeTbl.ItemDetailColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public int ServiceTypeID
            {
                get
                {
                    try
                    {
                        return (int)this[this.tableItemTypeTbl.ServiceTypeIDColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'ServiceTypeID' in table 'ItemTypeTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableItemTypeTbl.ServiceTypeIDColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public int ReplacementID
            {
                get
                {
                    try
                    {
                        return (int)this[this.tableItemTypeTbl.ReplacementIDColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'ReplacementID' in table 'ItemTypeTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableItemTypeTbl.ReplacementIDColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public int SortOrder
            {
                get
                {
                    try
                    {
                        return (int)this[this.tableItemTypeTbl.SortOrderColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'SortOrder' in table 'ItemTypeTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableItemTypeTbl.SortOrderColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsItemDescNull() => this.IsNull(this.tableItemTypeTbl.ItemDescColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetItemDescNull() => this[this.tableItemTypeTbl.ItemDescColumn] = Convert.DBNull;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsItemEnabledNull() => this.IsNull(this.tableItemTypeTbl.ItemEnabledColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetItemEnabledNull()
            {
                this[this.tableItemTypeTbl.ItemEnabledColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsItemsCharacteriticsNull()
            {
                return this.IsNull(this.tableItemTypeTbl.ItemsCharacteriticsColumn);
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetItemsCharacteriticsNull()
            {
                this[this.tableItemTypeTbl.ItemsCharacteriticsColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsItemDetailNull() => this.IsNull(this.tableItemTypeTbl.ItemDetailColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetItemDetailNull()
            {
                this[this.tableItemTypeTbl.ItemDetailColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsServiceTypeIDNull() => this.IsNull(this.tableItemTypeTbl.ServiceTypeIDColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetServiceTypeIDNull()
            {
                this[this.tableItemTypeTbl.ServiceTypeIDColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsReplacementIDNull() => this.IsNull(this.tableItemTypeTbl.ReplacementIDColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetReplacementIDNull()
            {
                this[this.tableItemTypeTbl.ReplacementIDColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsSortOrderNull() => this.IsNull(this.tableItemTypeTbl.SortOrderColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetSortOrderNull() => this[this.tableItemTypeTbl.SortOrderColumn] = Convert.DBNull;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public TrackerDataSet.CustomersTblRow[] GetCustomersTblRowsByPrimaryItemPrefernce()
            {
                return this.Table.ChildRelations["PrimaryItemPrefernce"] == null ? new TrackerDataSet.CustomersTblRow[0] : (TrackerDataSet.CustomersTblRow[])this.GetChildRows(this.Table.ChildRelations["PrimaryItemPrefernce"]);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public TrackerDataSet.CustomersTblRow[] GetCustomersTblRowsBySecondaryItemPreference()
            {
                return this.Table.ChildRelations["SecondaryItemPreference"] == null ? new TrackerDataSet.CustomersTblRow[0] : (TrackerDataSet.CustomersTblRow[])this.GetChildRows(this.Table.ChildRelations["SecondaryItemPreference"]);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public TrackerDataSet.OrdersTblRow[] GetOrdersTblRows()
            {
                return this.Table.ChildRelations["OrderItemToItemIDRelation"] == null ? new TrackerDataSet.OrdersTblRow[0] : (TrackerDataSet.OrdersTblRow[])this.GetChildRows(this.Table.ChildRelations["OrderItemToItemIDRelation"]);
            }
        }

        public class CityTblRow : DataRow
        {
            private TrackerDataSet.CityTblDataTable tableCityTbl;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            internal CityTblRow(DataRowBuilder rb)
              : base(rb)
            {
                this.tableCityTbl = (TrackerDataSet.CityTblDataTable)this.Table;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public int ID
            {
                get => (int)this[this.tableCityTbl.IDColumn];
                set => this[this.tableCityTbl.IDColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public string City
            {
                get
                {
                    try
                    {
                        return (string)this[this.tableCityTbl.CityColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'City' in table 'CityTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCityTbl.CityColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public int RoastingDay
            {
                get
                {
                    try
                    {
                        return (int)this[this.tableCityTbl.RoastingDayColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'RoastingDay' in table 'CityTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCityTbl.RoastingDayColumn] = (object)value;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public int DeliveryDelay
            {
                get
                {
                    try
                    {
                        return (int)this[this.tableCityTbl.DeliveryDelayColumn];
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new StrongTypingException("The value for column 'DeliveryDelay' in table 'CityTbl' is DBNull.", (Exception)ex);
                    }
                }
                set => this[this.tableCityTbl.DeliveryDelayColumn] = (object)value;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsCityNull() => this.IsNull(this.tableCityTbl.CityColumn);

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public void SetCityNull() => this[this.tableCityTbl.CityColumn] = Convert.DBNull;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public bool IsRoastingDayNull() => this.IsNull(this.tableCityTbl.RoastingDayColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetRoastingDayNull() => this[this.tableCityTbl.RoastingDayColumn] = Convert.DBNull;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public bool IsDeliveryDelayNull() => this.IsNull(this.tableCityTbl.DeliveryDelayColumn);

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void SetDeliveryDelayNull()
            {
                this[this.tableCityTbl.DeliveryDelayColumn] = Convert.DBNull;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public TrackerDataSet.CustomersTblRow[] GetCustomersTblRows()
            {
                return this.Table.ChildRelations["CustomerCityRelation"] == null ? new TrackerDataSet.CustomersTblRow[0] : (TrackerDataSet.CustomersTblRow[])this.GetChildRows(this.Table.ChildRelations["CustomerCityRelation"]);
            }
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public class OrdersTblRowChangeEvent : EventArgs
        {
            private TrackerDataSet.OrdersTblRow eventRow;
            private DataRowAction eventAction;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public OrdersTblRowChangeEvent(TrackerDataSet.OrdersTblRow row, DataRowAction action)
            {
                this.eventRow = row;
                this.eventAction = action;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public TrackerDataSet.OrdersTblRow Row => this.eventRow;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataRowAction Action => this.eventAction;
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public class CustomersTblRowChangeEvent : EventArgs
        {
            private TrackerDataSet.CustomersTblRow eventRow;
            private DataRowAction eventAction;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public CustomersTblRowChangeEvent(TrackerDataSet.CustomersTblRow row, DataRowAction action)
            {
                this.eventRow = row;
                this.eventAction = action;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public TrackerDataSet.CustomersTblRow Row => this.eventRow;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataRowAction Action => this.eventAction;
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public class ItemTypeTblRowChangeEvent : EventArgs
        {
            private TrackerDataSet.ItemTypeTblRow eventRow;
            private DataRowAction eventAction;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public ItemTypeTblRowChangeEvent(TrackerDataSet.ItemTypeTblRow row, DataRowAction action)
            {
                this.eventRow = row;
                this.eventAction = action;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public TrackerDataSet.ItemTypeTblRow Row => this.eventRow;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public DataRowAction Action => this.eventAction;
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public class CityTblRowChangeEvent : EventArgs
        {
            private TrackerDataSet.CityTblRow eventRow;
            private DataRowAction eventAction;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            [DebuggerNonUserCode]
            public CityTblRowChangeEvent(TrackerDataSet.CityTblRow row, DataRowAction action)
            {
                this.eventRow = row;
                this.eventAction = action;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public TrackerDataSet.CityTblRow Row => this.eventRow;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataRowAction Action => this.eventAction;
        }
    }
}