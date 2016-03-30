using FunctionalNetworkModel;
using NekoParaCharacterTest.Utilities;
using NekoPuppet.Plugins.Nodes.Core.Data;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace NekoPuppet.Plugins.Nodes.Core.Control
{
    class ValueBufferNodeFactory : IControlNodeFactoryPlugin
    {
        public const string TYPESTRING = "Control.ValueBuffer"; // Type for note display and save/load
        public const string NODEMENU = "Control"; // Menu, / delimited
        public const string NAME = "ValueBuffer"; // Name in menu

        public string TypeString { get { return TYPESTRING; } }
        public string NodeMenu { get { return NODEMENU; } }
        public string Name { get { return NAME; } }

        public ValueBufferNodeFactory()
        {

        }

        public NodeViewModel CreateNode()
        {
            return new ValueBufferNode();
        }

        public NodeViewModel CreateNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut)
        {
            return new ValueBufferNode(data, executeIn, executeOut, dataIn, dataOut);
        }
    }

    class ValueBufferNode : NodeViewModel
    {
        class NodeData : PropertyDialogCollection
        {
            [Browsable(true)]
            [DisplayName("Name")]
            public string NodeName { get; set; }
        }

        PropertyDialog dlgEdit;

        ExecutionConnectorViewModel conExecute;
        ConnectorViewModel conIn;
        ConnectorViewModel conOut;

        public override string Type { get { return ValueBufferNodeFactory.TYPESTRING; } }

        private INodeData Value;

        public override string Note { get { return string.Format("Value: {0}", Value); } }

        public ValueBufferNode() : base(ValueBufferNodeFactory.TYPESTRING)
        {
            // Prepare Connections
            conExecute = new ExecutionConnectorViewModel();
            conIn = new ConnectorViewModel("Set", typeof(INodeData));
            conOut = new ConnectorViewModel("Value", typeof(INodeData));

            this.InputExecutionConnectors.Add(conExecute);
            this.InputConnectors.Add(conIn);
            this.OutputConnectors.Add(conOut);

            // State Values
            Value = null;

            // Create Dialog
            dlgEdit = new PropertyDialog();
        }

        public ValueBufferNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut) : base(ValueBufferNodeFactory.TYPESTRING)
        {
            // Prepare Connections
            conExecute = new ExecutionConnectorViewModel(executeIn[0]);
            conIn = new ConnectorViewModel("Set", typeof(INodeData), dataIn[0]);
            conOut = new ConnectorViewModel("Value", typeof(INodeData), dataOut[0]);

            this.InputExecutionConnectors.Add(conExecute);
            this.InputConnectors.Add(conIn);
            this.OutputConnectors.Add(conOut);

            // Set Name
            Name = (string)data["name"];

            // State Values
            //Value = (Double)data["value"]; // no way to serilize generic values yet

            // Create Dialog
            dlgEdit = new PropertyDialog();
        }

        public override JObject Serialize()
        {
            JObject tmp = new JObject();
            tmp["name"] = Name;
            //tmp["value"] = Value; // no way to serilize generic values yet
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

        public override void Stop() { }

        public override void Execute(object context) {
            if (conIn.IsConnected)
            {
                object newContext = new object(); // new exuction node, new execution context
                Value = conIn.AttachedConnections.Select(connection =>
                {
                    try
                    {
                        object tmp = connection.SourceConnector.ParentNode.GetValue(connection.SourceConnector, context);
                        if (typeof(INodeData).IsAssignableFrom(tmp.GetType())) return (INodeData)tmp;
                        return null;
                    }
                    catch
                    {
                        return null;
                    }
                }).Where(val => val != null).FirstOrDefault();
            }
            else
            {
                Value = null;
            }
            OnPropertyChanged("Note");
        }

        public override void Dispose() { }

        public override object GetValue(ConnectorViewModel connector, object context)
        {
            return Value;
        }
    }
}
