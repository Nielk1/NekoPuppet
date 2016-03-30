using FunctionalNetworkModel;
using NekoParaCharacterTest.Utilities;
using NekoPuppet.Plugins.Nodes.Core.Data;
using Newtonsoft.Json.Linq;
using NodeCore.DataTypes;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace NekoPuppet.Plugins.Nodes.Core.Control
{
    class ValueChangedNodeFactory : IControlNodeFactoryPlugin
    {
        public const string TYPESTRING = "Control.ValueChanged"; // Type for note display and save/load
        public const string NODEMENU = "Control"; // Menu, / delimited
        public const string NAME = "ValueChanged"; // Name in menu

        public string TypeString { get { return TYPESTRING; } }
        public string NodeMenu { get { return NODEMENU; } }
        public string Name { get { return NAME; } }

        public ValueChangedNodeFactory()
        {

        }

        public NodeViewModel CreateNode()
        {
            return new ValueChangedNode();
        }

        public NodeViewModel CreateNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut)
        {
            return new ValueChangedNode(data, executeIn, executeOut, dataIn, dataOut);
        }
    }

    class ValueChangedNode : NodeViewModel
    {
        class NodeData : PropertyDialogCollection
        {
            [Browsable(true)]
            [DisplayName("Name")]
            public string NodeName { get; set; }
        }

        PropertyDialog dlgEdit;

        ExecutionConnectorViewModel conExecuteIn;
        ExecutionConnectorViewModel conExecuteOut;

        ConnectorViewModel conIn;
        ConnectorViewModel conOut;

        public override string Type { get { return ValueChangedNodeFactory.TYPESTRING; } }

        private INodeData OldValue;
        private INodeData Value;

        public override string Note { get {
                return string.Format("Current: {0}\r\nLast: {1}", Value, OldValue);
            } }

        public ValueChangedNode() : base(ValueChangedNodeFactory.TYPESTRING)
        {
            // Prepare Connections
            conExecuteIn = new ExecutionConnectorViewModel();
            conExecuteOut = new ExecutionConnectorViewModel();
            conIn = new ConnectorViewModel("Set", typeof(INodeData));
            conOut = new ConnectorViewModel("Value", typeof(INodeData));

            this.InputExecutionConnectors.Add(conExecuteIn);
            this.OutputExecutionConnectors.Add(conExecuteOut);
            this.InputConnectors.Add(conIn);
            this.OutputConnectors.Add(conOut);

            // State Values
            Value = null;
            OldValue = null;

            // Create Dialog
            dlgEdit = new PropertyDialog();
        }

        public ValueChangedNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut) : base(ValueChangedNodeFactory.TYPESTRING)
        {
            // Prepare Connections
            conExecuteIn = new ExecutionConnectorViewModel(executeIn[0]);
            conExecuteOut = new ExecutionConnectorViewModel(executeOut[0]);
            conIn = new ConnectorViewModel("Set", typeof(INodeData), dataIn[0]);
            conOut = new ConnectorViewModel("Value", typeof(INodeData), dataOut[0]);

            this.InputExecutionConnectors.Add(conExecuteIn);
            this.OutputExecutionConnectors.Add(conExecuteOut);
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

        public override void Execute(object context)
        {
            OldValue = Value;

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

            if (OldValue != null && Value != null && !OldValue.Equals(Value))
            {
                foreach (ExecutionConnectionViewModel con in this.AttachedExecutionConnections)
                {
                    if (con.SourceConnector != null &&
                        con.SourceConnector.ParentNode == this &&
                        con.DestConnector != null &&
                        con.DestConnector.ParentNode != null)
                    {
                        con.DestConnector.ParentNode.Execute(new object());
                    }
                }
            }
        }

        public override void Dispose() { }

        public override object GetValue(ConnectorViewModel connector, object context)
        {
            return Value;
        }
    }
}
