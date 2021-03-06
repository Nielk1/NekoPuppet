﻿using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using FunctionalNetworkModel;
using NekoPuppet.Plugins.Nodes.Core.Data;
using NekoParaCharacterTest.Utilities;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;

namespace NekoPuppet.Plugins.Nodes.Core.Emote
{
    class EmoteSetVariableNodeFactory : IControlNodeFactoryPlugin
    {
        public const string TYPESTRING = "Emote.SetVariable"; // Type for note display and save/load
        public const string NODEMENU = "Emote"; // Menu, / delimited
        public const string NAME = "SetVariable"; // Name in menu

        public string TypeString { get { return TYPESTRING; } }
        public string NodeMenu { get { return NODEMENU; } }
        public string Name { get { return NAME; } }

        public EmoteSetVariableNodeFactory()
        {

        }

        public NodeViewModel CreateNode()
        {
            return new EmoteSetVariableNode();
        }

        public NodeViewModel CreateNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut)
        {
            return new EmoteSetVariableNode(data, executeIn, executeOut, dataIn, dataOut);
        }
    }

    class EmoteSetVariableNode : NodeViewModel
    {
        class NodeData : PropertyDialogCollection
        {
            public class EmoteVarNameConverter : StringConverter
            {
                public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
                {
                    //true means show a combobox
                    return true;
                }
                public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
                {
                    //true will limit to list. false will show the list, but allow free-form entry
                    return false;
                }

                public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
                {
                    List<string> tmp = getEmoteVarNames();
                    if (tmp == null) return new StandardValuesCollection(new string[] { });
                    return new StandardValuesCollection(tmp.ToArray());
                }

                private List<string> getEmoteVarNames()
                {
                    try
                    {
                        CharacterContext context = CharacterContext.GetCharacterContext();
                        uint countVars = context.GetCharacter(0).CountVariables();
                        return Enumerable.Range(0, (int)countVars).Select(idx => context.GetCharacter(0).GetVariableLabelAt((uint)idx)).ToList();
                    }
                    catch { }
                    return null;
                }
            }

            [Browsable(true)]
            [DisplayName("Name")]
            [Category("Node")]
            public string NodeName { get; set; }

            [Browsable(true)]
            [TypeConverter(typeof(EmoteVarNameConverter))]
            [DisplayName("Variable Name")]
            [Category("Emote")]
            [Description("Emote Player variable name.")]
            public string VarName
            {
                get;
                set;
            }

            private float _value;
            [Browsable(true)]
            [Category("Emote")]
            [Description("Value applied if no input is present.")]
            public float Value
            {
                get { return _value; }
                set
                {
                    _value = System.Math.Max(System.Math.Min(value, 100.0f), -100.0f); // test limit
                }
            }

            private float frameCount;
            [Browsable(true)]
            [Category("Emote")]
            [Description("FrameCount applied if no input is present.")]
            public float FrameCount
            {
                get { return frameCount; }
                set
                {
                    frameCount = System.Math.Max(System.Math.Min(value, 100f), 0f); // test limit
                }
            }

            private float easing;
            [Browsable(true)]
            [Category("Emote")]
            [Description("Easing applied if no input is present.\r\nGiven as a ratio from 0.0 to 1.0")]
            public float Easing
            {
                get { return easing; }
                set
                {
                    easing = System.Math.Max(System.Math.Min(value, 1.0f), 0f);
                }
            }
        }

        PropertyDialog dlgEdit;

        ExecutionConnectorViewModel conExecute;
        private ConnectorViewModel inputValue;
        private ConnectorViewModel inputFrameCount;
        private ConnectorViewModel inputEasing;

        //public override NodeExecutionType ExecutionType { get { return NodeExecutionType.Triggered; } }

        private string varName;
        private float value;
        private float frameCount;
        private float easing;

        CharacterContext charContext;

        public override string Type { get { return EmoteSetVariableNodeFactory.TYPESTRING; } }

        public override string Note
        {
            get
            {
                return string.Format("Name: {0}\r\n", varName) +
                       "------DEFAULTS------\r\n" +
                       string.Format("Value: {0}\r\n", value) +
                       string.Format("FrameCount: {0}\r\n", frameCount) +
                       string.Format("Easing: {0}\r\n", easing);
            }
        }

        // Fresh constructor
        public EmoteSetVariableNode() : base(EmoteSetVariableNodeFactory.TYPESTRING)
        {
            // Prepare Connections
            conExecute = new ExecutionConnectorViewModel();
            inputValue = new ConnectorViewModel("Value", typeof(NodeDataNumeric));
            inputFrameCount = new ConnectorViewModel("FrameCount", typeof(NodeDataNumeric));
            inputEasing = new ConnectorViewModel("Easing", typeof(NodeDataNumeric));

            this.InputExecutionConnectors.Add(conExecute);
            this.InputConnectors.Add(inputValue);
            this.InputConnectors.Add(inputFrameCount);
            this.InputConnectors.Add(inputEasing);

            // State Values
            value = 0f;
            frameCount = 0.1f;
            easing = 0.9f;
            varName = string.Empty;

            // Create Dialog
            dlgEdit = new PropertyDialog();

            // Get Character Context
            charContext = CharacterContext.GetCharacterContext();
        }

        // Load Data Constructor
        public EmoteSetVariableNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut)
        {
            // Prepare Connections
            conExecute = new ExecutionConnectorViewModel(executeIn[0]);
            inputValue = new ConnectorViewModel("Value", typeof(NodeDataNumeric), dataIn[0]);
            if (dataIn.Length > 1) // allow loading older node graph saves
            {
                inputFrameCount = new ConnectorViewModel("FrameCount", typeof(NodeDataNumeric), dataIn[1]);
                inputEasing = new ConnectorViewModel("Easing", typeof(NodeDataNumeric), dataIn[2]);
            }
            else
            {
                inputFrameCount = new ConnectorViewModel("FrameCount", typeof(NodeDataNumeric));
                inputEasing = new ConnectorViewModel("Easing", typeof(NodeDataNumeric));
            }

            this.InputExecutionConnectors.Add(conExecute);
            this.InputConnectors.Add(inputValue);
            this.InputConnectors.Add(inputFrameCount);
            this.InputConnectors.Add(inputEasing);

            // Set Name
            Name = (string)data["name"];

            // State Values
            value = (float)data["value"];
            frameCount = (float)data["frameCount"];
            easing = (float)data["easing"];
            varName = (string)data["varName"];

            // Create Dialog
            dlgEdit = new PropertyDialog();

            // Get Character Context
            charContext = CharacterContext.GetCharacterContext();
        }

        // Save data
        public override JObject Serialize()
        {
            JObject tmp = new JObject();
            tmp["name"] = Name;
            tmp["value"] = value;
            tmp["frameCount"] = frameCount;
            tmp["easing"] = easing;
            tmp["varName"] = varName;
            return tmp;
        }

        // Edit dialog
        public override void Edit() {
            dlgEdit.Value = new NodeData()
            {
                NodeName = Name,
                VarName = varName,
                Value = value,
                FrameCount = frameCount,
                Easing = easing,
            };
            if (dlgEdit.ShowDialog() == DialogResult.OK)
            {
                Name = ((NodeData)(dlgEdit.Value)).NodeName;
                varName = ((NodeData)(dlgEdit.Value)).VarName;
                value = ((NodeData)(dlgEdit.Value)).Value;
                frameCount = ((NodeData)(dlgEdit.Value)).FrameCount;
                easing = ((NodeData)(dlgEdit.Value)).Easing;
            }
            OnPropertyChanged("Note");
        }

        // Self execution
        public override void Start() { } // not self executing

        public override void Stop() { }

        // Triggered execution
        public override void Execute(object context)
        {
            float l_value = value;
            if (inputValue.IsConnected)
            {
                object newContext = new object(); // new exuction node, new execution context
                NodeDataNumeric InputVal = inputValue.AttachedConnections.Select(connection =>
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
                if (InputVal != null) l_value = InputVal.GetSingle();
            }

            charContext.GetCharacter(0).SetVariable(varName, l_value, frameCount, easing);
        }

        // called when something tries to ask this node for a value
        public override object GetValue(ConnectorViewModel connector, object context)
        {
            return null;
        }

        public override void Dispose() { }
    }
}
