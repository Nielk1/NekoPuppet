using FunctionalNetworkModel;
using NekoParaCharacterTest.Utilities;
using NekoPuppet.Plugins.Nodes.Core.Data;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace NekoPuppet.Plugins.Nodes.Core.DeadZone
{
    class ScaledRadialNodeFactory : IControlNodeFactoryPlugin
    {
        public const string TYPESTRING = "DeadZone.ScaledRadial"; // Type for note display and save/load
        public const string NODEMENU = "DeadZone"; // Menu, / delimited
        public const string NAME = "ScaledRadial"; // Name in menu

        public string TypeString { get { return TYPESTRING; } }
        public string NodeMenu { get { return NODEMENU; } }
        public string Name { get { return NAME; } }

        public ScaledRadialNodeFactory()
        {

        }

        public NodeViewModel CreateNode()
        {
            return new ScaledRadialNode();
        }

        public NodeViewModel CreateNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut)
        {
            return new ScaledRadialNode(data, executeIn, executeOut, dataIn, dataOut);
        }
    }

    class ScaledRadialNode : NodeViewModel
    {
        class NodeData : PropertyDialogCollection
        {
            [Browsable(true)]
            [DisplayName("Name")]
            public string NodeName { get; set; }

            [Browsable(true)]
            public double Min { get; set; }

            [Browsable(true)]
            public double Max { get; set; }
        }

        class Cache
        {
            internal double X;
            internal double Y;
        }

        PropertyDialog dlgEdit;

        double Min;
        double Max;

        ConnectorViewModel conOutX;
        ConnectorViewModel conOutY;
        ConnectorViewModel conInX;
        ConnectorViewModel conInY;
        ConnectorViewModel conInMax;
        ConnectorViewModel conInMin;

        // non executing nodes can take advantage of a weak data cache
        // triggered nodes can simply store the data when triggered until triggered again
        private ConditionalWeakTable<object, Cache> DataCache = new ConditionalWeakTable<object, Cache>();

        public override string Type { get { return ScaledRadialNodeFactory.TYPESTRING; } }

        public override string Note
        {
            get
            {
                return string.Format("Default Min: {0}\r\n", Min) +
                       string.Format("Default Max: {0}", Max);
            }
        }

        public ScaledRadialNode() : base(ScaledRadialNodeFactory.TYPESTRING)
        {
            // Prepare Connections
            conOutX = new ConnectorViewModel("X", typeof(NodeDataNumeric));
            conOutY = new ConnectorViewModel("Y", typeof(NodeDataNumeric));
            conInX = new ConnectorViewModel("X", typeof(NodeDataNumeric));
            conInY = new ConnectorViewModel("Y", typeof(NodeDataNumeric));
            conInMin = new ConnectorViewModel("Min", typeof(NodeDataNumeric));
            conInMax = new ConnectorViewModel("Max", typeof(NodeDataNumeric));

            this.OutputConnectors.Add(conOutX);
            this.OutputConnectors.Add(conOutY);
            this.InputConnectors.Add(conInX);
            this.InputConnectors.Add(conInY);
            this.InputConnectors.Add(conInMin);
            this.InputConnectors.Add(conInMax);

            // State Values
            Min = 0;
            Max = 1;

            // Create Dialog
            dlgEdit = new PropertyDialog();
        }

        public ScaledRadialNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut) : base(ScaledRadialNodeFactory.TYPESTRING)
        {
            // Prepare Connections
            conOutX = new ConnectorViewModel("X", typeof(NodeDataNumeric), dataOut[0]);
            conOutY = new ConnectorViewModel("Y", typeof(NodeDataNumeric), dataOut[1]);
            conInX = new ConnectorViewModel("X", typeof(NodeDataNumeric), dataIn[0]);
            conInY = new ConnectorViewModel("Y", typeof(NodeDataNumeric), dataIn[1]);
            conInMin = new ConnectorViewModel("Min", typeof(NodeDataNumeric), dataIn[2]);
            conInMax = new ConnectorViewModel("Max", typeof(NodeDataNumeric), dataIn[3]);

            this.OutputConnectors.Add(conOutX);
            this.OutputConnectors.Add(conOutY);
            this.InputConnectors.Add(conInX);
            this.InputConnectors.Add(conInY);
            this.InputConnectors.Add(conInMin);
            this.InputConnectors.Add(conInMax);

            // Set Name
            Name = (string)data["name"];

            // State Values
            Min = (int)data["min"];
            Max = (int)data["max"];

            // Create Dialog
            dlgEdit = new PropertyDialog();
        }

        public override JObject Serialize()
        {
            JObject tmp = new JObject();
            tmp["name"] = Name;
            tmp["min"] = Min;
            tmp["max"] = Max;
            return tmp;
        }

        public override void Edit()
        {
            dlgEdit.Value = new NodeData()
            {
                NodeName = Name,
                Min = Min,
                Max = Max,
            };
            if (dlgEdit.ShowDialog() == DialogResult.OK)
            {
                Name = ((NodeData)(dlgEdit.Value)).NodeName;
                Min = ((NodeData)(dlgEdit.Value)).Min;
                Max = ((NodeData)(dlgEdit.Value)).Max;
            }
            OnPropertyChanged("Note");
        }

        public override void Start() { }

        public override void Stop() { }

        public override void Execute(object context) { }

        public override void Dispose() { }

        // called when something tries to ask this node for a value
        public override object GetValue(ConnectorViewModel connector, object context)
        {
            if (conOutX == connector || conOutY == connector)
            {
                Cache cache;
                double outX;
                double outY;

                // if the cache context is not set or there is no data cached for that context
                // ask attached nodes for values so we can calculate our output
                if (context == null || !DataCache.TryGetValue(context, out cache))
                {
                    NodeDataNumeric MinNodeVal = conInMin.AttachedConnections.Select(connection =>
                    {
                        try
                        {
                            object tmp = connection.SourceConnector.ParentNode.GetValue(connection.SourceConnector, context);
                            if (typeof(NodeDataNumeric).IsAssignableFrom(tmp.GetType())) return (NodeDataNumeric)tmp;
                            return null;
                        }
                        catch
                        {
                            return null;
                        }
                    }).Where(val => val != null).FirstOrDefault();

                    NodeDataNumeric MaxNodeVal = conInMax.AttachedConnections.Select(connection =>
                    {
                        try
                        {
                            object tmp = connection.SourceConnector.ParentNode.GetValue(connection.SourceConnector, context);
                            if (typeof(NodeDataNumeric).IsAssignableFrom(tmp.GetType())) return (NodeDataNumeric)tmp;
                            return null;
                        }
                        catch
                        {
                            return null;
                        }
                    }).Where(val => val != null).FirstOrDefault();

                    NodeDataNumeric XNodeVal = conInX.AttachedConnections.Select(connection =>
                    {
                        try
                        {
                            object tmp = connection.SourceConnector.ParentNode.GetValue(connection.SourceConnector, context);
                            if (typeof(NodeDataNumeric).IsAssignableFrom(tmp.GetType())) return (NodeDataNumeric)tmp;
                            return null;
                        }
                        catch
                        {
                            return null;
                        }
                    }).Where(val => val != null).FirstOrDefault();

                    NodeDataNumeric YNodeVal = conInY.AttachedConnections.Select(connection =>
                    {
                        try
                        {
                            object tmp = connection.SourceConnector.ParentNode.GetValue(connection.SourceConnector, context);
                            if (typeof(NodeDataNumeric).IsAssignableFrom(tmp.GetType())) return (NodeDataNumeric)tmp;
                            return null;
                        }
                        catch
                        {
                            return null;
                        }
                    }).Where(val => val != null).FirstOrDefault();

                    double Min = this.Min;
                    double Max = this.Max;
                    double X = 0;
                    double Y = 0;

                    try { if (MinNodeVal != null) Min = MinNodeVal.GetDouble(); } catch { }
                    try { if (MaxNodeVal != null) Max = MaxNodeVal.GetDouble(); } catch { }
                    try { if (XNodeVal != null) X = XNodeVal.GetDouble(); } catch { }
                    try { if (YNodeVal != null) Y = YNodeVal.GetDouble(); } catch { }

                    double MaxMag = Max - Min;

                    double inputMagnitude = System.Math.Sqrt(X * X + Y * Y);

                    if (inputMagnitude < Min)
                    {
                        DataCache.Add(context, new Cache() { X = 0d, Y = 0d });
                        //Console.WriteLine("Storing data for {0,8:X8}", RuntimeHelpers.GetHashCode(context));
                        return NodeDataNumeric.FromDouble(0d);
                    }
                    if (inputMagnitude > Max) inputMagnitude = Max;

                    double newMagnitude = ((inputMagnitude - Min) / MaxMag);

                    outX = System.Math.Min(System.Math.Max(X / inputMagnitude, -1d), 1d) * newMagnitude;
                    outY = System.Math.Min(System.Math.Max(Y / inputMagnitude, -1d), 1d) * newMagnitude;

                    DataCache.Add(context, new Cache() { X = outX, Y = outY });

                    //Console.WriteLine("Storing data for {0,8:X8}", RuntimeHelpers.GetHashCode(context));
                }
                else // we already have the output for this context cached, re-output it
                {
                    outX = cache.X;
                    outY = cache.Y;
                    //Console.WriteLine("Loading data for {0,8:X8}", RuntimeHelpers.GetHashCode(context));
                }

                if (conOutX == connector)
                {
                    return NodeDataNumeric.FromDouble(outX);
                }
                if (conOutY == connector)
                {
                    return NodeDataNumeric.FromDouble(outY);
                }
            }
            return null;
        }
    }
}
