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
    class XInputControllerNodeFactory : IControlNodeFactoryPlugin
    {
        public const string TYPESTRING = "SharpDX.XInput.Controller.Analog"; // Type for note display and save/load
        public const string NODEMENU = "SharpDX/XInput/Controller"; // Menu, / delimited
        public const string NAME = "Analog"; // Name in menu

        public string TypeString { get { return TYPESTRING; } }
        public string NodeMenu { get { return NODEMENU; } }
        public string Name { get { return NAME; } }

        public XInputControllerNodeFactory()
        {

        }

        public NodeViewModel CreateNode()
        {
            return new XInputControllerNode();
        }

        public NodeViewModel CreateNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut)
        {
            return new XInputControllerNode(data, executeIn, executeOut, dataIn, dataOut);
        }
    }

    class XInputControllerNode : NodeViewModel
    {
        class NodeData : PropertyDialogCollection
        {
            [Browsable(true)]                         // this property should be visible
            //[ReadOnly(true)]                          // but just read only
            //[Description("sample hint1")]             // sample hint1
            //[Category("Category1")]                   // Category that I want
            [DisplayName("Name")]       // I want to say more, than just DisplayInt
            public string NodeName { get; set; }

            [Browsable(true)]
            public UserIndex User { get; set; }
        }

        PropertyDialog dlgEdit;

        ExecutionConnectorViewModel conExecuted;
        ExecutionConnectorViewModel conExecute;

        ConnectorViewModel conLeftThumbX;
        ConnectorViewModel conLeftThumbY;
        ConnectorViewModel conLeftTrigger;
        ConnectorViewModel conRightThumbX;
        ConnectorViewModel conRightThumbY;
        ConnectorViewModel conRightTrigger;

        private UserIndex currentUser;
        private Controller controller;
        private State? cState;

        public override string Type { get { return XInputControllerNodeFactory.TYPESTRING; } }

        public override string Note
        {
            get
            {
                string retVal =
                    string.Format("Player: {0}\r\n", currentUser.ToString());

                if (controller != null && controller.IsConnected && cState.HasValue)
                {
                    retVal +=
                        "----------\r\n" +
                        string.Format("LeftThumbX: {0}\r\n", cState.Value.Gamepad.LeftThumbX.ToString()) +
                        string.Format("LeftThumbY: {0}\r\n", cState.Value.Gamepad.LeftThumbY.ToString()) +
                        string.Format("LeftTrigger: {0}\r\n", cState.Value.Gamepad.LeftTrigger.ToString()) +
                        string.Format("RightThumbX: {0}\r\n", cState.Value.Gamepad.RightThumbX.ToString()) +
                        string.Format("RightThumbY: {0}\r\n", cState.Value.Gamepad.RightThumbY.ToString()) +
                        string.Format("RightTrigger: {0}\r\n", cState.Value.Gamepad.RightTrigger.ToString()) +
                        "----------";
                }
                else
                {
                    retVal +=
                        "----------\r\n" +
                        "LeftThumbX: \r\n" +
                        "LeftThumbY: \r\n" +
                        "LeftTrigger: \r\n" +
                        "RightThumbX: \r\n" +
                        "RightThumbY: \r\n" +
                        "RightTrigger: \r\n" +
                        "----------";
                }
                return retVal;
            }
        }

        public XInputControllerNode() : base(XInputControllerNodeFactory.TYPESTRING)
        {
            // Prepare Execution Connection
            conExecuted = new ExecutionConnectorViewModel();
            conExecute = new ExecutionConnectorViewModel();

            this.InputExecutionConnectors.Add(conExecuted);
            this.OutputExecutionConnectors.Add(conExecute);

            // Prepare Connections
            conLeftThumbX   = new ConnectorViewModel("LeftThumbX", typeof(NodeDataNumeric));
            conLeftThumbY   = new ConnectorViewModel("LeftThumbY", typeof(NodeDataNumeric));
            conLeftTrigger  = new ConnectorViewModel("LeftTrigger", typeof(NodeDataNumeric));
            conRightThumbX  = new ConnectorViewModel("RightThumbX", typeof(NodeDataNumeric));
            conRightThumbY  = new ConnectorViewModel("RightThumbY", typeof(NodeDataNumeric));
            conRightTrigger = new ConnectorViewModel("RightTrigger", typeof(NodeDataNumeric));

            this.OutputConnectors.Add(conLeftThumbX);
            this.OutputConnectors.Add(conLeftThumbY);
            this.OutputConnectors.Add(conLeftTrigger);
            this.OutputConnectors.Add(conRightThumbX);
            this.OutputConnectors.Add(conRightThumbY);
            this.OutputConnectors.Add(conRightTrigger);

            // State Values
            currentUser = UserIndex.One;
            controller = new SharpDX.XInput.Controller(currentUser);

            // Create Dialog
            dlgEdit = new PropertyDialog();
        }

        public XInputControllerNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut) : base(XInputControllerNodeFactory.TYPESTRING)
        {
            // Prepare Execution Connection
            conExecuted = new ExecutionConnectorViewModel(executeIn[0]);
            conExecute = new ExecutionConnectorViewModel(executeOut[0]);

            this.InputExecutionConnectors.Add(conExecuted);
            this.OutputExecutionConnectors.Add(conExecute);

            // Prepare Connections
            conLeftThumbX = new ConnectorViewModel("LeftThumbX", typeof(NodeDataNumeric), dataOut[0]);
            conLeftThumbY = new ConnectorViewModel("LeftThumbY", typeof(NodeDataNumeric), dataOut[1]);
            conLeftTrigger = new ConnectorViewModel("LeftTrigger", typeof(NodeDataNumeric), dataOut[2]);
            conRightThumbX = new ConnectorViewModel("RightThumbX", typeof(NodeDataNumeric), dataOut[3]);
            conRightThumbY = new ConnectorViewModel("RightThumbY", typeof(NodeDataNumeric), dataOut[4]);
            conRightTrigger = new ConnectorViewModel("RightTrigger", typeof(NodeDataNumeric), dataOut[5]);

            this.OutputConnectors.Add(conLeftThumbX);
            this.OutputConnectors.Add(conLeftThumbY);
            this.OutputConnectors.Add(conLeftTrigger);
            this.OutputConnectors.Add(conRightThumbX);
            this.OutputConnectors.Add(conRightThumbY);
            this.OutputConnectors.Add(conRightTrigger);

            // Set Name
            Name = (string)data["name"];

            // State Values
            if (!Enum.TryParse<UserIndex>((string)data["currentUser"], out currentUser)) currentUser = UserIndex.One;
            controller = new SharpDX.XInput.Controller(currentUser);

            // Create Dialog
            dlgEdit = new PropertyDialog();
        }

        public override JObject Serialize()
        {
            JObject tmp = new JObject();
            tmp["name"] = Name;
            tmp["currentUser"] = currentUser.ToString();
            return tmp;
        }

        public override void Edit()
        {
            dlgEdit.Value = new NodeData()
            {
                NodeName = Name,
                User = currentUser
            };
            if (dlgEdit.ShowDialog() == DialogResult.OK)
            {
                Name = ((NodeData)(dlgEdit.Value)).NodeName;
                currentUser = ((NodeData)(dlgEdit.Value)).User;
                controller = new Controller(currentUser);
            }
            OnPropertyChanged("Note");
        }

        public override void Start() { }

        public override void Execute() {
            if (controller.IsConnected)
            {
                cState = controller.GetState();
            }
            else
            {
                cState = null;
            }

            OnPropertyChanged("Note");

            foreach (ExecutionConnectionViewModel con in this.AttachedExecutionConnections)
            {
                if (con.SourceConnector != null &&
                    con.SourceConnector.ParentNode == this &&
                    con.DestConnector != null &&
                    con.DestConnector.ParentNode != null)
                {
                    con.DestConnector.ParentNode.Execute();
                }
            }
        }

        public override void Dispose() { }

        public override object GetValue(ConnectorViewModel connector)
        {
            if (controller.IsConnected && cState.HasValue)
            {
                if (conLeftThumbX == connector)
                    return NodeDataNumeric.FromInt16(cState.Value.Gamepad.LeftThumbX);
                if (conLeftThumbY == connector)
                    return NodeDataNumeric.FromInt16(cState.Value.Gamepad.LeftThumbY);
                if (conLeftTrigger == connector)
                    return NodeDataNumeric.FromByte(cState.Value.Gamepad.LeftTrigger);
                if (conRightThumbX == connector)
                    return NodeDataNumeric.FromInt16(cState.Value.Gamepad.RightThumbX);
                if (conRightThumbY == connector)
                    return NodeDataNumeric.FromInt16(cState.Value.Gamepad.RightThumbY);
                if (conRightTrigger == connector)
                    return NodeDataNumeric.FromByte(cState.Value.Gamepad.RightTrigger);
            }
            return null;
        }
    }
}