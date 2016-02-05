using System;
using System.Threading;
using NekoParaCharacterTest.Utilities;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using FunctionalNetworkModel;
using System.Windows.Forms;

namespace NekoPuppet.Plugins.Nodes.Core.Trigger
{
    class IntervalTriggerNodeFactory : IControlNodeFactoryPlugin
    {
        public const string TYPESTRING = "Trigger.Interval"; // Type for note display and save/load
        public const string NODEMENU = "Trigger"; // Menu, / delimited
        public const string NAME = "Interval"; // Name in menu

        public string TypeString { get { return TYPESTRING; } }
        public string NodeMenu { get { return NODEMENU; } }
        public string Name { get { return NAME; } }

        public IntervalTriggerNodeFactory()
        {

        }

        public NodeViewModel CreateNode()
        {
            return new IntervalTriggerNode();
        }

        public NodeViewModel CreateNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut)
        {
            return new IntervalTriggerNode(data, executeIn, executeOut, dataIn, dataOut);
        }
    }

    class IntervalTriggerNode : NodeViewModel
    {
        class NodeData : PropertyDialogCollection
        {
            [Browsable(true)]                         // this property should be visible
            //[ReadOnly(true)]                          // but just read only
            //[Description("sample hint1")]             // sample hint1
            //[Category("Category1")]                   // Category that I want
            [DisplayName("Name")]       // I want to say more, than just DisplayInt
            public string NodeName { get; set; }

            private int period;
            [Browsable(true)]
            public int Period
            {
                get { return period; }
                set
                {
                    period = System.Math.Max(System.Math.Min(value, 60 * 1000), 10);
                }
            }
        }

        PropertyDialog dlgEdit;

        ExecutionConnectorViewModel conExecute;

        //public override NodeExecutionType ExecutionType { get { return NodeExecutionType.Self; } }
        private bool ThreadRunning;
        private Thread worker;
        private int period;
        //private Semaphore diposeInterlock;

        public override string Type { get { return IntervalTriggerNodeFactory.TYPESTRING; } }

        public override string Note
        {
            get
            {
                return string.Format("Period: {0} ms", period);
            }
        }

        public IntervalTriggerNode() : base(IntervalTriggerNodeFactory.TYPESTRING)
        {
            // Prepare Connections
            conExecute = new ExecutionConnectorViewModel();
            this.OutputExecutionConnectors.Add(conExecute);

            //diposeInterlock = new Semaphore(0, 1);

            // State Value
            this.period = 100;
            worker = new Thread(DoWork);

            // Create Dialog
            dlgEdit = new PropertyDialog();
        }

        public IntervalTriggerNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut)
        {
            // Prepare Connections
            conExecute = new ExecutionConnectorViewModel(executeOut[0]);
            this.OutputExecutionConnectors.Add(conExecute);

            //diposeInterlock = new Semaphore(0, 1);

            // Set Name
            Name = (string)data["name"];

            // State Value
            this.period = (int)data["period"];
            worker = new Thread(DoWork);

            // Create Dialog
            dlgEdit = new PropertyDialog();
        }

        ~IntervalTriggerNode()
        {
            Dispose();
        }

        public override JObject Serialize()
        {
            JObject tmp = new JObject();
            tmp["name"] = Name;
            tmp["period"] = period;
            return tmp;
        }

        public override void Edit()
        {
            dlgEdit.Value = new NodeData()
            {
                NodeName = Name,
                Period = period
            };
            if (dlgEdit.ShowDialog() == DialogResult.OK)
            {
                Name = ((NodeData)(dlgEdit.Value)).NodeName;
                period = ((NodeData)(dlgEdit.Value)).Period;

                OnPropertyChanged("Note");
            }
        }

        public override void Start()
        {
            if (ThreadRunning) return;
            ThreadRunning = true;

            worker.Start();
        }

        public override void Execute() { }

        public override void Dispose()
        {
            Stop();
        }

        public override object GetValue(ConnectorViewModel connector)
        {
            return null;
        }


        private void DoWork()
        {
            while (ThreadRunning)
            {
                int intervalTmp = period;
                Thread.Sleep(intervalTmp);
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
            //diposeInterlock.Release();
        }

        private void Stop()
        {
            if (ThreadRunning)
            {
                ThreadRunning = false;
                //diposeInterlock.WaitOne();
            }
        }
    }
}