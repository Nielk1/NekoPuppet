using System;
using SharpDX.XInput;
using NekoParaCharacterTest.Utilities;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using FunctionalNetworkModel;
using NekoPuppet.Plugins.Nodes.Core.Data;

namespace NekoPuppet.Plugins.Nodes.Core.XInput
{
    class XInputConstantsNodeFactory : IControlNodeFactoryPlugin
    {
        public const string TYPESTRING = "SharpDX.XInput.Controller.Constants"; // Type for note display and save/load
        public const string NODEMENU = "SharpDX/XInput/Controller"; // Menu, / delimited
        public const string NAME = "Constants"; // Name in menu

        public string TypeString { get { return TYPESTRING; } }
        public string NodeMenu { get { return NODEMENU; } }
        public string Name { get { return NAME; } }

        public XInputConstantsNodeFactory()
        {

        }

        public NodeViewModel CreateNode()
        {
            return new XInputConstantsNode();
        }

        public NodeViewModel CreateNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut)
        {
            return new XInputConstantsNode(data, executeIn, executeOut, dataIn, dataOut);
        }
    }

    class XInputConstantsNode : NodeViewModel
    {
        class NodeData : PropertyDialogCollection
        {
            [Browsable(true)]                         // this property should be visible
            //[ReadOnly(true)]                          // but just read only
            //[Description("sample hint1")]             // sample hint1
            //[Category("Category1")]                   // Category that I want
            [DisplayName("Name")]       // I want to say more, than just DisplayInt
            public string NodeName { get; set; }
        }

        PropertyDialog dlgEdit;

        ConnectorViewModel conLeftThumbDeadZone;
        ConnectorViewModel conRightThumbDeadZone;
        ConnectorViewModel conTriggerThreshold;

        public override string Type { get { return XInputConstantsNodeFactory.TYPESTRING; } }

        //public override string Note { get { } }

        public XInputConstantsNode() : base(XInputConstantsNodeFactory.TYPESTRING)
        {
            // Prepare Connections
            conLeftThumbDeadZone = new ConnectorViewModel("LeftThumbDeadZone", typeof(NodeDataNumeric));
            conRightThumbDeadZone = new ConnectorViewModel("RightThumbDeadZone", typeof(NodeDataNumeric));
            conTriggerThreshold = new ConnectorViewModel("TriggerThreshold", typeof(NodeDataNumeric));

            this.OutputConnectors.Add(conLeftThumbDeadZone);
            this.OutputConnectors.Add(conRightThumbDeadZone);
            this.OutputConnectors.Add(conTriggerThreshold);

            // Create Dialog
            dlgEdit = new PropertyDialog();
        }

        public XInputConstantsNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut) : base(XInputControllerNodeFactory.TYPESTRING)
        {
            // Prepare Connections
            conLeftThumbDeadZone = new ConnectorViewModel("LeftThumbDeadZone", typeof(NodeDataNumeric), dataOut[0]);
            conRightThumbDeadZone = new ConnectorViewModel("RightThumbDeadZone", typeof(NodeDataNumeric), dataOut[1]);
            conTriggerThreshold = new ConnectorViewModel("TriggerThreshold", typeof(NodeDataNumeric), dataOut[2]);

            this.OutputConnectors.Add(conLeftThumbDeadZone);
            this.OutputConnectors.Add(conRightThumbDeadZone);
            this.OutputConnectors.Add(conTriggerThreshold);

            // Set Name
            Name = (string)data["name"];

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
                NodeName = Name
            };
            if (dlgEdit.ShowDialog() == DialogResult.OK)
            {
                Name = ((NodeData)(dlgEdit.Value)).NodeName;
            }
            OnPropertyChanged("Note");
        }

        public override void Start() { }

        public override void Execute() { }

        public override void Dispose() { }

        public override object GetValue(ConnectorViewModel connector)
        {
            if (conLeftThumbDeadZone == connector)
                return NodeDataNumeric.FromInt16(SharpDX.XInput.Gamepad.LeftThumbDeadZone);
            if (conRightThumbDeadZone == connector)
                return NodeDataNumeric.FromInt16(SharpDX.XInput.Gamepad.RightThumbDeadZone);
            if (conTriggerThreshold == connector)
                return NodeDataNumeric.FromByte(SharpDX.XInput.Gamepad.TriggerThreshold);
            return null;
        }
    }
}