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
    class DataRoutingNodeFactory : IControlNodeFactoryPlugin
    {
        public const string TYPESTRING = "Routing.Data"; // Type for note display and save/load
        public const string NODEMENU = "Routing"; // Menu, / delimited
        public const string NAME = "Data"; // Name in menu

        public string TypeString { get { return TYPESTRING; } }
        public string NodeMenu { get { return NODEMENU; } }
        public string Name { get { return NAME; } }

        public DataRoutingNodeFactory()
        {

        }

        public NodeViewModel CreateNode()
        {
            return new DataRoutingNode();
        }

        public NodeViewModel CreateNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut)
        {
            return new DataRoutingNode(data, executeIn, executeOut, dataIn, dataOut);
        }
    }

    class DataRoutingNode : NodeViewModel
    {
        ConnectorViewModel conOut;
        ConnectorViewModel conInput;

        public override string Type { get { return DataRoutingNodeFactory.TYPESTRING; } }

        //public override string Note { get { } }

        public DataRoutingNode() : base(DataRoutingNodeFactory.TYPESTRING)
        {
            // Prepare Connections
            conOut = new ConnectorViewModel("In", typeof(NodeDataNumeric));
            conInput = new ConnectorViewModel("Out", typeof(NodeDataNumeric));

            this.OutputConnectors.Add(conOut);
            this.InputConnectors.Add(conInput);
        }

        public DataRoutingNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut) : base(DataRoutingNodeFactory.TYPESTRING)
        {
            // Prepare Connections
            conOut = new ConnectorViewModel("In", typeof(NodeDataNumeric), dataOut[0]);
            conInput = new ConnectorViewModel("Out", typeof(NodeDataNumeric), dataIn[0]);

            this.OutputConnectors.Add(conOut);
            this.InputConnectors.Add(conInput);

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

        public override void Execute(object context) { }

        public override void Dispose() { }

        public override object GetValue(ConnectorViewModel connector, object context)
        {
            if (conInput.IsConnected)
            {
                return conInput.AttachedConnections.Select(connection =>
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
            return null;
        }
    }
}
