using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using FunctionalNetworkModel;

namespace NekoPuppet.Plugins.Nodes.Core.Debug
{
    class ConsoleOutNodeFactory : IControlNodeFactoryPlugin
    {
        public const string TYPESTRING = "Debugging.Output.ConsoleOut"; // Type for note display and save/load
        public const string NODEMENU = "Debugging/Output"; // Menu, / delimited
        public const string NAME = "Console Out"; // Name in menu

        public string TypeString { get { return TYPESTRING; } }
        public string NodeMenu { get { return NODEMENU; } }
        public string Name { get { return NAME; } }

        public ConsoleOutNodeFactory()
        {

        }

        public NodeViewModel CreateNode()
        {
            return new ConsoleOutNode();
        }

        public NodeViewModel CreateNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut)
        {
            return new ConsoleOutNode(data, executeIn, executeOut, dataIn, dataOut);
        }
    }

    class ConsoleOutNode : NodeViewModel
    {
        //public override NodeExecutionType ExecutionType { get { return NodeExecutionType.Triggered; } }

        private ConnectorViewModel inputText;

        public override string Type { get { return ConsoleOutNodeFactory.TYPESTRING; } }

        public ConsoleOutNode() : base(ConsoleOutNodeFactory.TYPESTRING)
        {
            this.InputExecutionConnectors.Add(new ExecutionConnectorViewModel());
            inputText = new ConnectorViewModel("Text", typeof(INodeData));
            this.InputConnectors.Add(inputText);
        }

        public ConsoleOutNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut)
        {
            this.InputExecutionConnectors.Add(new ExecutionConnectorViewModel(executeIn[0]));
            inputText = new ConnectorViewModel("Text", typeof(INodeData), dataIn[0]);
            this.InputConnectors.Add(inputText);
        }

        public override JObject Serialize()
        {
            JObject tmp = new JObject();
            tmp["name"] = Name;
            return tmp;
        }

        public override void Edit() { }

        public override void Start() { }

        public override void Execute()
        {
            string output = this.Name;
            if (inputText.IsConnected)
            {
                output = string.Join("\t", inputText.AttachedConnections.Select(connection =>
                {
                    object tmp = connection.SourceConnector.ParentNode.GetValue(connection.SourceConnector);
                    try { return ((INodeData)tmp).ToString(); } catch { }
                    try { return tmp.ToString(); } catch { }
                    return null;
                }));
            }
            Console.WriteLine(output);
        }

        public override object GetValue(ConnectorViewModel connector)
        {
            return null;
        }

        public override void Dispose() { }
    }
}
