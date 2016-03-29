/*using FunctionalNetworkModel;
using NekoParaCharacterTest.Utilities;
using NekoPuppet.Plugins.Nodes.Core.Data;
using Newtonsoft.Json.Linq;
using NodeCore.DataTypes;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Utils;

namespace NekoPuppet.Plugins.Nodes.Core.Control
{
    class ExpandStructDataNodeFactory : IControlNodeFactoryPlugin
    {
        public const string TYPESTRING = "Control.ExpandStructData"; // Type for note display and save/load
        public const string NODEMENU = "Control"; // Menu, / delimited
        public const string NAME = "ExpandStructData"; // Name in menu

        public string TypeString { get { return TYPESTRING; } }
        public string NodeMenu { get { return NODEMENU; } }
        public string Name { get { return NAME; } }

        public ExpandStructDataNodeFactory()
        {

        }

        public NodeViewModel CreateNode()
        {
            return new ExpandStructDataNode();
        }

        public NodeViewModel CreateNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut)
        {
            return new ExpandStructDataNode(data, executeIn, executeOut, dataIn, dataOut);
        }
    }

    class ExpandStructDataNode : NodeViewModel
    {
        class NodeData : PropertyDialogCollection
        {
            [Browsable(true)]
            [DisplayName("Name")]
            public string NodeName { get; set; }
        }

        PropertyDialog dlgEdit;

        ConnectorViewModel conIn;
        ConnectorViewModel[] conOut;

        class Cache
        {
            public NodeDataStruct value;
        }

        private ConditionalWeakTable<object, Cache> DataCache = new ConditionalWeakTable<object, Cache>();

        public override string Type { get { return ExpandStructDataNodeFactory.TYPESTRING; } }

        private string[] _Keys;
        private NodeDataStruct LastValue;

        //public override string Note { get { return string.Format("Value: {0}", Value); } }

        public ExpandStructDataNode() : base(ExpandStructDataNodeFactory.TYPESTRING)
        {
            // Prepare Connections
            conIn = new ConnectorViewModel("Input", typeof(NodeDataStruct));
            //conOut = new ConnectorViewModel("Value", typeof(INodeData));
            conOut = new ConnectorViewModel[0];

            this.InputConnectors.Add(conIn);
            //this.OutputConnectors.Add(conOut);

            // State Values
            //Value = null;

            // Create Dialog
            dlgEdit = new PropertyDialog();
        }

        public ExpandStructDataNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut) : base(ExpandStructDataNodeFactory.TYPESTRING)
        {
            JArray keyArr = (JArray)data["keys"];
            _Keys = keyArr.ToList().Select(dr => (string)dr).ToArray();

            // Prepare Connections
            conIn = new ConnectorViewModel("Input", typeof(NodeDataStruct), dataIn[0]);
            if (_Keys.Length == dataOut.Length)
            {
                conOut = new ConnectorViewModel[dataOut.Length];
                for (int x = 0; x < dataOut.Length; x++)
                {
                    conOut[x] = new ConnectorViewModel(_Keys[x], typeof(INodeData), dataOut[x]);
                    this.OutputConnectors.Add(conOut[x]);
                }
            }
            else
            {
                conOut = new ConnectorViewModel[0];
            }

            // Set Name
            Name = (string)data["name"];

            // Create Dialog
            dlgEdit = new PropertyDialog();
        }

        // this should be run when a new input is added or removed, for now, this is done when triggering edit mode
        private void UpdateOutputs()
        {
            if (conIn.IsConnected)
            {
                GetValue(null, null);
            }
            else
            {
                LastValue = null;
            }

            if (LastValue != null)
            {
                _Keys = LastValue.Keys;
                if(conOut.Length > _Keys.Length)
                {
                    this.OutputConnectors.RemoveRange(conOut);
                    conOut = new ConnectorViewModel[_Keys.Length];
                    for (int x = 0; x < _Keys.Length; x++)
                    {
                        conOut[x] = new ConnectorViewModel(_Keys[x], typeof(INodeData));
                        this.OutputConnectors.Add(conOut[x]);
                    }
                }
                else
                {
                    ConnectorViewModel[] tmp = conOut;
                    conOut = new ConnectorViewModel[_Keys.Length];
                    for (int x = 0; x < _Keys.Length; x++)
                    {
                        if(tmp.Length > x)
                        {
                            conOut[x] = new ConnectorViewModel(_Keys[x], typeof(INodeData));
                            tmp[x].AttachedConnections.ToList().ForEach(con =>
                            {
                                conOut[x].AttachedConnections.Add(con);
                            });
                            this.OutputConnectors.Add(conOut[x]);
                            this.OutputConnectors.Remove(tmp[x]);
                        }
                        else {
                            conOut[x] = new ConnectorViewModel(_Keys[x], typeof(INodeData));
                            this.OutputConnectors.Add(conOut[x]);
                        }
                    }
                }
            }
            else
            {
                conOut = new ConnectorViewModel[0];
            }
        }

        public override JObject Serialize()
        {
            JObject tmp = new JObject();
            tmp["name"] = Name;
            JArray keyArr = new JArray();
            _Keys.ToList().ForEach(dr => keyArr.Add(dr));
            tmp["keys"] = keyArr;
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
                UpdateOutputs();
            }
            OnPropertyChanged("Note");
        }

        public override void Start() { }

        public override void Stop() { }

        public override void Execute(object context) { }

        public override void Dispose() { }

        public override object GetValue(ConnectorViewModel connector, object context)
        {
            if (conIn.IsConnected)
            {
                Cache cache;
                NodeDataStruct retVal;
                if (context == null || !DataCache.TryGetValue(context, out cache))
                {
                    object newContext = new object(); // new exuction node, new execution context
                    retVal = conIn.AttachedConnections.Select(connection =>
                    {
                        try
                        {
                            object tmp = connection.SourceConnector.ParentNode.GetValue(connection.SourceConnector, context);
                            if (typeof(NodeDataStruct).IsAssignableFrom(tmp.GetType())) return (NodeDataStruct)tmp;
                            return null;
                        }
                        catch
                        {
                            return null;
                        }
                    }).Where(val => val != null).FirstOrDefault();
                    LastValue = retVal;
                    if (context != null) DataCache.Add(context, new Cache() { value = retVal });
                }
                else
                {
                    retVal = cache.value;
                }
                return retVal;
            }
            return null;
        }
    }
}
*/