/*using NekoPuppet.Plugins.Nodes.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.XInput;
using FunctionalNetworkModel;

namespace NodeCore.DataTypes
{
    class NodeDataXInputButtons : NodeDataStruct
    {
        private GamepadButtonFlags buttons;

        public NodeDataXInputButtons(GamepadButtonFlags buttons) : base(new string[] { "DPadUp", "DPadDown", "DPadLeft", "DPadRight", "Start", "Back", "LeftThumb", "RightThumb", "LeftShoulder", "RightShoulder", "A", "B", "X", "Y" })
        {
            this.buttons = buttons;
        }

        public override IDictionary<string, INodeData> Values
        {
            get
            {
                return new Dictionary<string, INodeData>
                {
                    {"DPadUp", NodeDataBoolean.FromBoolean(buttons.HasFlag(GamepadButtonFlags.DPadUp)) },
                    {"DPadDown", NodeDataBoolean.FromBoolean(buttons.HasFlag(GamepadButtonFlags.DPadDown)) },
                    {"DPadLeft", NodeDataBoolean.FromBoolean(buttons.HasFlag(GamepadButtonFlags.DPadLeft)) },
                    {"DPadRight", NodeDataBoolean.FromBoolean(buttons.HasFlag(GamepadButtonFlags.DPadRight)) },
                    {"Start", NodeDataBoolean.FromBoolean(buttons.HasFlag(GamepadButtonFlags.Start)) },
                    {"Back", NodeDataBoolean.FromBoolean(buttons.HasFlag(GamepadButtonFlags.Back)) },
                    {"LeftThumb", NodeDataBoolean.FromBoolean(buttons.HasFlag(GamepadButtonFlags.LeftThumb)) },
                    {"RightThumb", NodeDataBoolean.FromBoolean(buttons.HasFlag(GamepadButtonFlags.RightThumb)) },
                    {"LeftShoulder", NodeDataBoolean.FromBoolean(buttons.HasFlag(GamepadButtonFlags.LeftShoulder)) },
                    {"RightShoulder", NodeDataBoolean.FromBoolean(buttons.HasFlag(GamepadButtonFlags.RightShoulder)) },
                    {"A", NodeDataBoolean.FromBoolean(buttons.HasFlag(GamepadButtonFlags.A)) },
                    {"B", NodeDataBoolean.FromBoolean(buttons.HasFlag(GamepadButtonFlags.B)) },
                    {"X", NodeDataBoolean.FromBoolean(buttons.HasFlag(GamepadButtonFlags.X)) },
                    {"Y", NodeDataBoolean.FromBoolean(buttons.HasFlag(GamepadButtonFlags.Y)) },
                };
            }
        }
    }
}
*/