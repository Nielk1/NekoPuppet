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
    class MathNumericConstantNodeFactory : IControlNodeFactoryPlugin
    {
        public const string TYPESTRING = "Math.NumericConstant"; // Type for note display and save/load
        public const string NODEMENU = "Math"; // Menu, / delimited
        public const string NAME = "NumericConstant"; // Name in menu

        public string TypeString { get { return TYPESTRING; } }
        public string NodeMenu { get { return NODEMENU; } }
        public string Name { get { return NAME; } }

        public MathNumericConstantNodeFactory()
        {

        }

        public NodeViewModel CreateNode()
        {
            return new MathNumericConstantNode();
        }

        public NodeViewModel CreateNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut)
        {
            return new MathNumericConstantNode(data, executeIn, executeOut, dataIn, dataOut);
        }
    }

    class MathNumericConstantNode : NodeViewModel
    {
        class NodeData : PropertyDialogCollection
        {
            [Browsable(true)]
            [DisplayName("Name")]
            public string NodeName { get; set; }

            [Browsable(true)]
            public CastType CastToType { get; set; }

            [Browsable(true)]
            public Double Value { get; set; }
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

        PropertyDialog dlgEdit;

        ConnectorViewModel conOut;

        public override string Type { get { return MathNumericConstantNodeFactory.TYPESTRING; } }

        private CastType CastToType;
        private Double Value;

        public override string Note { get { return string.Format("Value: {0}", Value); } }

        public MathNumericConstantNode() : base(MathNumericConstantNodeFactory.TYPESTRING)
        {
            // Prepare Connections
            conOut = new ConnectorViewModel("Value", typeof(NodeDataNumeric));

            this.OutputConnectors.Add(conOut);

            // State Values
            CastToType = CastType.Double;
            Value = 0d;

            // Create Dialog
            dlgEdit = new PropertyDialog();
        }

        public MathNumericConstantNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut) : base(MathNumericConstantNodeFactory.TYPESTRING)
        {
            // Prepare Connections
            conOut = new ConnectorViewModel("Value", typeof(NodeDataNumeric), dataOut[0]);

            this.OutputConnectors.Add(conOut);

            // Set Name
            Name = (string)data["name"];

            // State Values
            if (!Enum.TryParse<CastType>((string)data["CastToType"], out CastToType)) CastToType = CastType.Double;
            Value = (Double)data["value"];

            // Create Dialog
            dlgEdit = new PropertyDialog();
        }

        public override JObject Serialize()
        {
            JObject tmp = new JObject();
            tmp["name"] = Name;
            tmp["value"] = Value;
            return tmp;
        }

        public override void Edit()
        {
            dlgEdit.Value = new NodeData()
            {
                NodeName = Name,
                CastToType = CastToType,
                Value = Value
            };
            if (dlgEdit.ShowDialog() == DialogResult.OK)
            {
                Name = ((NodeData)(dlgEdit.Value)).NodeName;
                CastToType = ((NodeData)(dlgEdit.Value)).CastToType;
                Value = ((NodeData)(dlgEdit.Value)).Value;
            }
            OnPropertyChanged("Note");
        }

        public override void Start() { }

        public override void Stop() { }

        public override void Execute(object context) { }

        public override void Dispose() { }

        public override object GetValue(ConnectorViewModel connector, object context)
        {
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
                    return NodeDataNumeric.FromDouble(Value);
                    //case CastType.Single:
                    //    return (Single)retVal;
                    //case CastType.Decimal:
                    //    return (Decimal)retVal;

                    //case CastType.IConvertable:
                    //    return enumeration.Sum(dr => dr);
            }
            return NodeDataNumeric.MakeZero();
        }
    }
}
