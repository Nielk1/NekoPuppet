using FunctionalNetworkModel;
using NekoParaCharacterTest.Utilities;
using NekoPuppet.Plugins.Nodes.Core.Data;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace NekoPuppet.Plugins.Nodes.Core.Routing
{
    class ExecuteRoutingNodeFactory : IControlNodeFactoryPlugin
    {
        public const string TYPESTRING = "Routing.Execute"; // Type for note display and save/load
        public const string NODEMENU = "Routing"; // Menu, / delimited
        public const string NAME = "Execute"; // Name in menu

        public string TypeString { get { return TYPESTRING; } }
        public string NodeMenu { get { return NODEMENU; } }
        public string Name { get { return NAME; } }

        public ExecuteRoutingNodeFactory()
        {

        }

        public NodeViewModel CreateNode()
        {
            return new ExecuteRoutingNode();
        }

        public NodeViewModel CreateNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut)
        {
            return new ExecuteRoutingNode(data, executeIn, executeOut, dataIn, dataOut);
        }
    }

    class ExecuteRoutingNode : NodeViewModel
    {
        ExecutionConnectorViewModel conExecuted;
        ExecutionConnectorViewModel conExecute;

        public override string Type { get { return ExecuteRoutingNodeFactory.TYPESTRING; } }

        //public override string Note { get { } }

        public ExecuteRoutingNode() : base(ExecuteRoutingNodeFactory.TYPESTRING)
        {
            // Prepare Execution Connection
            conExecuted = new ExecutionConnectorViewModel();
            conExecute = new ExecutionConnectorViewModel();

            this.InputExecutionConnectors.Add(conExecuted);
            this.OutputExecutionConnectors.Add(conExecute);
        }

        public ExecuteRoutingNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut) : base(ExecuteRoutingNodeFactory.TYPESTRING)
        {
            // Prepare Execution Connection
            conExecuted = new ExecutionConnectorViewModel(executeIn[0]);
            conExecute = new ExecutionConnectorViewModel(executeOut[0]);

            this.InputExecutionConnectors.Add(conExecuted);
            this.OutputExecutionConnectors.Add(conExecute);

            // Set Name
            //Name = (string)data["name"];
        }

        public override JObject Serialize()
        {
            JObject tmp = new JObject();
            //tmp["name"] = Name;
            return tmp;
        }

        public override void Edit()
        {
            /*dlgEdit.Value = new NodeData()
            {
                NodeName = Name,
                CastToType = CastToType
            };
            if (dlgEdit.ShowDialog() == DialogResult.OK)
            {
                Name = ((NodeData)(dlgEdit.Value)).NodeName;
                CastToType = ((NodeData)(dlgEdit.Value)).CastToType;
            }
            OnPropertyChanged("Note");*/
        }

        public override void Start() { }

        public override void Stop() { }

        public override void Execute(object context) {
            foreach (ExecutionConnectionViewModel con in this.AttachedExecutionConnections)
            {
                if (con.SourceConnector != null &&
                    con.SourceConnector.ParentNode == this &&
                    con.DestConnector != null &&
                    con.DestConnector.ParentNode != null)
                {
                    con.DestConnector.ParentNode.Execute(context);
                }
            }
        }

        public override void Dispose() { }

        public override object GetValue(ConnectorViewModel connector, object context)
        {
            return null;
        }
    }
}
