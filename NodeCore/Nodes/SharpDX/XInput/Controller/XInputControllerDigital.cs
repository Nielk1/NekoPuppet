using System;
using SharpDX.XInput;
using NekoParaCharacterTest.Utilities;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using FunctionalNetworkModel;
using NekoPuppet.Plugins.Nodes.Core.Data;
using NodeCore.DataTypes;

namespace NekoPuppet.Plugins.Nodes.Core.XInput
{
    class XInputControllerDigitalNodeFactory : IControlNodeFactoryPlugin
    {
        public const string TYPESTRING = "SharpDX.XInput.Controller.Digital"; // Type for note display and save/load
        public const string NODEMENU = "SharpDX/XInput/Controller"; // Menu, / delimited
        public const string NAME = "Digital"; // Name in menu

        public string TypeString { get { return TYPESTRING; } }
        public string NodeMenu { get { return NODEMENU; } }
        public string Name { get { return NAME; } }

        public XInputControllerDigitalNodeFactory()
        {

        }

        public NodeViewModel CreateNode()
        {
            return new XInputControllerDigitalNode();
        }

        public NodeViewModel CreateNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut)
        {
            return new XInputControllerDigitalNode(data, executeIn, executeOut, dataIn, dataOut);
        }
    }

    class XInputControllerDigitalNode : NodeViewModel
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

        ConnectorViewModel conDPadUp;
        ConnectorViewModel conDPadDown;
        ConnectorViewModel conDPadLeft;
        ConnectorViewModel conDPadRight;
        ConnectorViewModel conStart;
        ConnectorViewModel conBack;
        ConnectorViewModel conLeftThumb;
        ConnectorViewModel conRightThumb;
        ConnectorViewModel conLeftShoulder;
        ConnectorViewModel conRightShoulder;
        ConnectorViewModel conA;
        ConnectorViewModel conB;
        ConnectorViewModel conX;
        ConnectorViewModel conY;

        private UserIndex currentUser;
        private Controller controller;
        private State? cState;

        public override string Type { get { return XInputControllerDigitalNodeFactory.TYPESTRING; } }

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
                        "DPad: " + (cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp) ? "\u2191" : " ") +
                        (cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown) ? "\u2193" : " ") +
                        (cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) ? "\u2190" : " ") +
                        (cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) ? "\u2192" : " ") + "\r\n" +
                        "S/B: " + (cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Start) ? "Start" : "     ") + " " +
                        (cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Back) ? "Back" : "    ") + "\r\n" +
                        "Thumb: " + (cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftThumb) ? "Left" : "    ") + " " +
                        (cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightThumb) ? "Right" : "     ") + "\r\n" +
                        "Shoulder: " + (cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder) ? "Left" : "    ") + " " +
                        (cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder) ? "Right" : "     ") + "\r\n" +
                        "Face: " + (cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A) ? "A" : " ") +
                        (cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B) ? "B" : " ") +
                        (cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.X) ? "X" : " ") +
                        (cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Y) ? "Y" : " ") + "\r\n" +
                        "----------";
                }
                else
                {
                    retVal +=
                        "----------\r\n" +
                        "DPad: \r\n" +
                        "S/B: \r\n" +
                        "Thumb: \r\n" +
                        "Shoulder: \r\n" +
                        "Face: \r\n" +
                        "----------";
                }
                return retVal;
            }
        }

        public XInputControllerDigitalNode() : base(XInputControllerDigitalNodeFactory.TYPESTRING)
        {
            // Prepare Execution Connection
            conExecuted = new ExecutionConnectorViewModel();
            conExecute = new ExecutionConnectorViewModel();

            this.InputExecutionConnectors.Add(conExecuted);
            this.OutputExecutionConnectors.Add(conExecute);

            // Prepare Connections
            conDPadUp = new ConnectorViewModel("DPadUp", typeof(NodeDataBoolean));
            conDPadDown = new ConnectorViewModel("DPadDown", typeof(NodeDataBoolean));
            conDPadLeft = new ConnectorViewModel("DPadLeft", typeof(NodeDataBoolean));
            conDPadRight = new ConnectorViewModel("DPadRight", typeof(NodeDataBoolean));
            conStart = new ConnectorViewModel("Start", typeof(NodeDataBoolean));
            conBack = new ConnectorViewModel("Back", typeof(NodeDataBoolean));
            conLeftThumb = new ConnectorViewModel("LeftThumb", typeof(NodeDataBoolean));
            conRightThumb = new ConnectorViewModel("RightThumb", typeof(NodeDataBoolean));
            conLeftShoulder = new ConnectorViewModel("LeftShoulder", typeof(NodeDataBoolean));
            conRightShoulder = new ConnectorViewModel("RightShoulder", typeof(NodeDataBoolean));
            conA = new ConnectorViewModel("A", typeof(NodeDataBoolean));
            conB = new ConnectorViewModel("B", typeof(NodeDataBoolean));
            conX = new ConnectorViewModel("X", typeof(NodeDataBoolean));
            conY = new ConnectorViewModel("Y", typeof(NodeDataBoolean));

            this.OutputConnectors.Add(conDPadUp);
            this.OutputConnectors.Add(conDPadDown);
            this.OutputConnectors.Add(conDPadLeft);
            this.OutputConnectors.Add(conDPadRight);
            this.OutputConnectors.Add(conStart);
            this.OutputConnectors.Add(conBack);
            this.OutputConnectors.Add(conLeftThumb);
            this.OutputConnectors.Add(conRightThumb);
            this.OutputConnectors.Add(conLeftShoulder);
            this.OutputConnectors.Add(conRightShoulder);
            this.OutputConnectors.Add(conA);
            this.OutputConnectors.Add(conB);
            this.OutputConnectors.Add(conX);
            this.OutputConnectors.Add(conY);

            // State Values
            currentUser = UserIndex.One;
            controller = new SharpDX.XInput.Controller(currentUser);

            // Create Dialog
            dlgEdit = new PropertyDialog();
        }

        public XInputControllerDigitalNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut) : base(XInputControllerDigitalNodeFactory.TYPESTRING)
        {
            // Prepare Execution Connection
            conExecuted = new ExecutionConnectorViewModel(executeIn[0]);
            conExecute = new ExecutionConnectorViewModel(executeOut[0]);

            this.InputExecutionConnectors.Add(conExecuted);
            this.OutputExecutionConnectors.Add(conExecute);

            // Prepare Connections
            conDPadUp = new ConnectorViewModel("DPadUp", typeof(NodeDataBoolean), dataOut[0]);
            conDPadDown = new ConnectorViewModel("DPadDown", typeof(NodeDataBoolean), dataOut[1]);
            conDPadLeft = new ConnectorViewModel("DPadLeft", typeof(NodeDataBoolean), dataOut[2]);
            conDPadRight = new ConnectorViewModel("DPadRight", typeof(NodeDataBoolean), dataOut[3]);
            conStart = new ConnectorViewModel("Start", typeof(NodeDataBoolean), dataOut[4]);
            conBack = new ConnectorViewModel("Back", typeof(NodeDataBoolean), dataOut[5]);
            conLeftThumb = new ConnectorViewModel("LeftThumb", typeof(NodeDataBoolean), dataOut[6]);
            conRightThumb = new ConnectorViewModel("RightThumb", typeof(NodeDataBoolean), dataOut[7]);
            conLeftShoulder = new ConnectorViewModel("LeftShoulder", typeof(NodeDataBoolean), dataOut[8]);
            conRightShoulder = new ConnectorViewModel("RightShoulder", typeof(NodeDataBoolean), dataOut[9]);
            conA = new ConnectorViewModel("A", typeof(NodeDataBoolean), dataOut[10]);
            conB = new ConnectorViewModel("B", typeof(NodeDataBoolean), dataOut[11]);
            conX = new ConnectorViewModel("X", typeof(NodeDataBoolean), dataOut[12]);
            conY = new ConnectorViewModel("Y", typeof(NodeDataBoolean), dataOut[13]);

            this.OutputConnectors.Add(conDPadUp);
            this.OutputConnectors.Add(conDPadDown);
            this.OutputConnectors.Add(conDPadLeft);
            this.OutputConnectors.Add(conDPadRight);
            this.OutputConnectors.Add(conStart);
            this.OutputConnectors.Add(conBack);
            this.OutputConnectors.Add(conLeftThumb);
            this.OutputConnectors.Add(conRightThumb);
            this.OutputConnectors.Add(conLeftShoulder);
            this.OutputConnectors.Add(conRightShoulder);
            this.OutputConnectors.Add(conA);
            this.OutputConnectors.Add(conB);
            this.OutputConnectors.Add(conX);
            this.OutputConnectors.Add(conY);

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
        public override void Stop() { }

        public override void Execute(object context)
        {
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
                    con.DestConnector.ParentNode.Execute(new object());
                }
            }
        }

        public override void Dispose() { }

        public override object GetValue(ConnectorViewModel connector, object context)
        {
            if (controller.IsConnected && cState.HasValue)
            {
                if (conDPadUp == connector)
                    return NodeDataBoolean.FromBoolean(cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp));
                if (conDPadDown == connector)
                    return NodeDataBoolean.FromBoolean(cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown));
                if (conDPadLeft == connector)
                    return NodeDataBoolean.FromBoolean(cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft));
                if (conDPadRight == connector)
                    return NodeDataBoolean.FromBoolean(cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight));
                if (conStart == connector)
                    return NodeDataBoolean.FromBoolean(cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Start));
                if (conBack == connector)
                    return NodeDataBoolean.FromBoolean(cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Back));
                if (conLeftThumb == connector)
                    return NodeDataBoolean.FromBoolean(cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftThumb));
                if (conRightThumb == connector)
                    return NodeDataBoolean.FromBoolean(cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightThumb));
                if (conLeftShoulder == connector)
                    return NodeDataBoolean.FromBoolean(cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder));
                if (conRightShoulder == connector)
                    return NodeDataBoolean.FromBoolean(cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder));
                if (conA == connector)
                    return NodeDataBoolean.FromBoolean(cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A));
                if (conB == connector)
                    return NodeDataBoolean.FromBoolean(cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B));
                if (conX == connector)
                    return NodeDataBoolean.FromBoolean(cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.X));
                if (conY == connector)
                    return NodeDataBoolean.FromBoolean(cState.Value.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Y));
            }
            return null;
        }
    }
}