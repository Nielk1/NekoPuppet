using FunctionalNetworkModel;
using NekoParaCharacterTest.Utilities;
using NekoPuppet.Plugins.Nodes.Core.Data;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace NekoPuppet.Plugins.Nodes.Core.Math
{
    class MathSumNodeFactory : IControlNodeFactoryPlugin
    {
        public const string TYPESTRING = "Math.Sum"; // Type for note display and save/load
        public const string NODEMENU = "Math"; // Menu, / delimited
        public const string NAME = "Sum"; // Name in menu

        public string TypeString { get { return TYPESTRING; } }
        public string NodeMenu { get { return NODEMENU; } }
        public string Name { get { return NAME; } }

        public MathSumNodeFactory()
        {

        }

        public NodeViewModel CreateNode()
        {
            return new MathSumNode();
        }

        public NodeViewModel CreateNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut)
        {
            return new MathSumNode(data, executeIn, executeOut, dataIn, dataOut);
        }
    }

    class MathSumNode : NodeViewModel
    {
        class NodeData : PropertyDialogCollection
        {
            [Browsable(true)]
            [DisplayName("Name")]
            public string NodeName { get; set; }

            [Browsable(true)]
            public CastType CastToType { get; set; }
        }

        enum CastType
        {
            //SByte,
            //Int16,
            //Int32,
            //Int64,
            //Byte,
            //UInt16,
            //UInt32,
            //UInt64,
            Double,
            //Single,
            //Decimal,
        }

        class Cache
        {
            public double value;
        }

        PropertyDialog dlgEdit;

        ConnectorViewModel conOut;
        ConnectorViewModel conInput;

        private ConditionalWeakTable<object, Cache> DataCache = new ConditionalWeakTable<object, Cache>();

        public override string Type { get { return MathSumNodeFactory.TYPESTRING; } }


        private CastType CastToType;

        //public override string Note { get { } }

        public MathSumNode() : base(MathSumNodeFactory.TYPESTRING)
        {
            // Prepare Connections
            conOut = new ConnectorViewModel("Sum", typeof(NodeDataNumeric));
            conInput = new ConnectorViewModel("Multi-Input", typeof(NodeDataNumeric));

            this.OutputConnectors.Add(conOut);
            this.InputConnectors.Add(conInput);

            // State Values
            CastToType = CastType.Double;

            // Create Dialog
            dlgEdit = new PropertyDialog();
        }

        public MathSumNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut) : base(MathSumNodeFactory.TYPESTRING)
        {
            // Prepare Connections
            conOut = new ConnectorViewModel("Sum", typeof(NodeDataNumeric), dataOut[0]);
            conInput = new ConnectorViewModel("Multi-Input", typeof(NodeDataNumeric), dataIn[0]);

            this.OutputConnectors.Add(conOut);
            this.InputConnectors.Add(conInput);

            // Set Name
            Name = (string)data["name"];

            // State Values
            if (!Enum.TryParse<CastType>((string)data["CastToType"], out CastToType)) CastToType = CastType.Double;

            // Create Dialog
            dlgEdit = new PropertyDialog();
        }

        public override JObject Serialize()
        {
            JObject tmp = new JObject();
            tmp["name"] = Name;
            return tmp;
        }

        public override void Edit()
        {
            dlgEdit.Value = new NodeData()
            {
                NodeName = Name,
                CastToType = CastToType
            };
            if (dlgEdit.ShowDialog() == DialogResult.OK)
            {
                Name = ((NodeData)(dlgEdit.Value)).NodeName;
                CastToType = ((NodeData)(dlgEdit.Value)).CastToType;
            }
            OnPropertyChanged("Note");
        }

        public override void Start() { }

        public override void Stop() { }

        public override void Execute(object context) { }

        public override void Dispose() { }

        public override object GetValue(ConnectorViewModel connector, object context)
        {
            if (conInput.IsConnected)
            {
                Cache cache;
                double retVal;
                if (context == null || !DataCache.TryGetValue(context, out cache))
                {
                    retVal = conInput.AttachedConnections.Sum(connection =>
                    {
                        object tmp = connection.SourceConnector.ParentNode.GetValue(connection.SourceConnector, context);
                        try { if (typeof(NodeDataNumeric).IsAssignableFrom(tmp.GetType())) return ((NodeDataNumeric)tmp).GetDouble(); } catch { }
                        return 0d;
                    });
                    DataCache.Add(context, new Cache() { value = retVal });
                }
                else
                {
                    retVal = cache.value;
                }

                switch (CastToType)
                {
                    //case CastType.SByte:
                    //    return (SByte)retVal;
                    //case CastType.Int16:
                    //    return (Int16)retVal;
                    //case CastType.Int32:
                    //    return (Int32)retVal;
                    //case CastType.Int64:
                    //    return (Int64)retVal;
                    //case CastType.Byte:
                    //    return (Byte)retVal;
                    //case CastType.UInt16:
                    //    return (UInt16)retVal;
                    //case CastType.UInt32:
                    //    return (UInt32)retVal;
                    //case CastType.UInt64:
                    //    return (UInt64)retVal;
                    case CastType.Double:
                        return NodeDataNumeric.FromDouble(retVal);
                        //case CastType.Single:
                        //    return (Single)retVal;
                        //case CastType.Decimal:
                        //    return (Decimal)retVal;

                        //case CastType.IConvertable:
                        //    return enumeration.Sum(dr => dr);
                }
                return NodeDataNumeric.MakeZero();
            }
            return NodeDataNumeric.MakeZero();
        }
    }
}
