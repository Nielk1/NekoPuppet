using FunctionalNetworkModel;
using Mentor.Utilities;
using NekoParaCharacterTest.Utilities;
using NekoPuppet.Plugins.Nodes.Core.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace NekoPuppet.Plugins.Nodes.Core.Variables
{
    static class VariableCache
    {
        private static WeakValueDictionary<string, INodeData> Variables = new WeakValueDictionary<string, INodeData>();

        internal static void SetVariable(string varName, INodeData value)
        {
            lock(Variables)
            {
                Variables[varName] = value;
            }
        }

        internal static object GetValue(string varName)
        {
            lock (Variables)
            {
                INodeData tmp;
                Variables.TryGetValue(varName, out tmp);
                return tmp;
            }
        }
    }

    class SetVariableNodeFactory : IControlNodeFactoryPlugin
    {
        public const string TYPESTRING = "Variables.Set"; // Type for note display and save/load
        public const string NODEMENU = "Variables"; // Menu, / delimited
        public const string NAME = "Set"; // Name in menu

        public string TypeString { get { return TYPESTRING; } }
        public string NodeMenu { get { return NODEMENU; } }
        public string Name { get { return NAME; } }

        public SetVariableNodeFactory()
        {

        }

        public NodeViewModel CreateNode()
        {
            return new SetVariableNode();
        }

        public NodeViewModel CreateNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut)
        {
            return new SetVariableNode(data, executeIn, executeOut, dataIn, dataOut);
        }
    }

    class SetVariableNode : NodeViewModel
    {
        class NodeData : PropertyDialogCollection
        {
            [Browsable(true)]
            [DisplayName("Name")]
            public string NodeName { get; set; }

            [Browsable(true)]
            public string VarName { get; set; }
        }

        PropertyDialog dlgEdit;

        ExecutionConnectorViewModel conExecute;
        ConnectorViewModel conIn;

        public override string Type { get { return SetVariableNodeFactory.TYPESTRING; } }

        private String VarName;
        private INodeData Value;

        public override string Note { get { return string.Format("{0}", VarName); } }

        public SetVariableNode() : base(SetVariableNodeFactory.TYPESTRING)
        {
            // Prepare Connections
            conExecute = new ExecutionConnectorViewModel();
            conIn = new ConnectorViewModel("Set", typeof(INodeData));

            this.InputExecutionConnectors.Add(conExecute);
            this.InputConnectors.Add(conIn);

            // State Values
            VarName = null;
            Value = null;

            // Create Dialog
            dlgEdit = new PropertyDialog();
        }

        public SetVariableNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut) : base(SetVariableNodeFactory.TYPESTRING)
        {
            // Prepare Connections
            conExecute = new ExecutionConnectorViewModel(executeIn[0]);
            conIn = new ConnectorViewModel("Set", typeof(INodeData), dataIn[0]);

            this.InputExecutionConnectors.Add(conExecute);
            this.InputConnectors.Add(conIn);

            // Set Name
            Name = (string)data["name"];

            // State Values
            VarName = (string)data["varname"];

            // Create Dialog
            dlgEdit = new PropertyDialog();
        }

        public override JObject Serialize()
        {
            JObject tmp = new JObject();
            tmp["name"] = Name;
            tmp["varname"] = VarName;
            return tmp;
        }

        public override void Edit()
        {
            dlgEdit.Value = new NodeData()
            {
                NodeName = Name,
                VarName = VarName
            };
            if (dlgEdit.ShowDialog() == DialogResult.OK)
            {
                Name = ((NodeData)(dlgEdit.Value)).NodeName;
                VarName = ((NodeData)(dlgEdit.Value)).VarName;
            }
            OnPropertyChanged("Note");
        }

        public override void Start() { }

        public override void Stop() { }

        public override void Execute(object context)
        {
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
            if (Value != null) Value = (INodeData)Value.Clone();
            if (VarName != null && VarName.Length > 0)
            {
                VariableCache.SetVariable(VarName, Value);
            }
            OnPropertyChanged("Note");
        }

        public override void Dispose() { }

        public override object GetValue(ConnectorViewModel connector, object context) { return null; }
    }

    class GetVariableNodeFactory : IControlNodeFactoryPlugin
    {
        public const string TYPESTRING = "Variables.Get"; // Type for note display and save/load
        public const string NODEMENU = "Variables"; // Menu, / delimited
        public const string NAME = "Get"; // Name in menu

        public string TypeString { get { return TYPESTRING; } }
        public string NodeMenu { get { return NODEMENU; } }
        public string Name { get { return NAME; } }

        public GetVariableNodeFactory()
        {

        }

        public NodeViewModel CreateNode()
        {
            return new GetVariableNode();
        }

        public NodeViewModel CreateNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut)
        {
            return new GetVariableNode(data, executeIn, executeOut, dataIn, dataOut);
        }
    }

    class GetVariableNode : NodeViewModel
    {
        class NodeData : PropertyDialogCollection
        {
            [Browsable(true)]
            [DisplayName("Name")]
            public string NodeName { get; set; }

            [Browsable(true)]
            public string VarName { get; set; }
        }

        PropertyDialog dlgEdit;

        ConnectorViewModel conOut;

        public override string Type { get { return GetVariableNodeFactory.TYPESTRING; } }

        private String VarName;

        public override string Note { get { return string.Format("{0}", VarName); } }

        public GetVariableNode() : base(GetVariableNodeFactory.TYPESTRING)
        {
            // Prepare Connections
            conOut = new ConnectorViewModel("Value", typeof(INodeData));

            this.OutputConnectors.Add(conOut);

            // State Values
            VarName = null;

            // Create Dialog
            dlgEdit = new PropertyDialog();
        }

        public GetVariableNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut) : base(GetVariableNodeFactory.TYPESTRING)
        {
            // Prepare Connections
            conOut = new ConnectorViewModel("Value", typeof(NodeDataNumeric), dataOut[0]);

            this.OutputConnectors.Add(conOut);

            // Set Name
            Name = (string)data["name"];

            // State Values
            VarName = (string)data["varname"];

            // Create Dialog
            dlgEdit = new PropertyDialog();
        }

        public override JObject Serialize()
        {
            JObject tmp = new JObject();
            tmp["name"] = Name;
            tmp["varname"] = VarName;
            return tmp;
        }

        public override void Edit()
        {
            dlgEdit.Value = new NodeData()
            {
                NodeName = Name,
                VarName = VarName
            };
            if (dlgEdit.ShowDialog() == DialogResult.OK)
            {
                Name = ((NodeData)(dlgEdit.Value)).NodeName;
                VarName = ((NodeData)(dlgEdit.Value)).VarName;
            }
            OnPropertyChanged("Note");
        }

        public override void Start() { }

        public override void Stop() { }

        public override void Execute(object context) { }

        public override void Dispose() { }

        public override object GetValue(ConnectorViewModel connector, object context)
        {
            if (VarName != null && VarName.Length > 0)
            {
                return VariableCache.GetValue(VarName);
            }
            return null;
        }
    }
}
