using FunctionalNetworkModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace NekoPuppet
{
    public partial class FunctionGraphForm : Form
    {
        private Dictionary<string, IControlNodeFactoryPlugin> TypeToFactory;

        public FunctionGraphForm()
        {
            InitializeComponent();

            TypeToFactory = new Dictionary<string, IControlNodeFactoryPlugin>();

            nodeGraph1.NodeCreated += NodeGraph1_NodeCreated;
            nodeGraph1.NodeDeleted += NodeGraph1_NodeDeleted;

            saveFileDialog1.InitialDirectory = Path.Combine(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath), "nodes");
            openFileDialog1.InitialDirectory = Path.Combine(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath), "nodes");

            saveFileDialog1.FileName = string.Empty;
            openFileDialog1.FileName = string.Empty;

            string pluginsDir = Path.Combine(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath), "plugins");
            if (!Directory.Exists(pluginsDir)) Directory.CreateDirectory(pluginsDir);
            List<string> plugins = Directory.GetFiles(pluginsDir, "*.dll", SearchOption.TopDirectoryOnly).ToList();
            plugins.ForEach(dr =>
            {
                try
                {
                    IControlNodeFactoryPlugin[] pluginInstances = LoadAssembly(dr);
                    foreach (IControlNodeFactoryPlugin plugin in pluginInstances)
                    {
                        nodeGraph1.RegisterNodeType(plugin.NodeMenu, plugin.Name, plugin);
                        TypeToFactory.Add(plugin.TypeString, plugin);
                    }
                }
                catch { }
            });


            /*HardcodedPluginDescription desc = new HardcodedPluginDescription();
            desc.NodeDescriptions.ForEach(nodeDesc =>
            {
                nodeGraph1.RegisterNodeType(nodeDesc.NodeMenu, nodeDesc.Name, nodeDesc.Factory);
                TypeToFactory.Add(nodeDesc.TypeString, nodeDesc.Factory);
            });*/
        }

        public void NotifyNodesOfClose()
        {
            List<FunctionalNetworkModel.NodeViewModel> nodes = this.nodeGraph1.ViewModel.Network.Nodes.ToList();
            nodes.ForEach(node =>
            {
                //node = null; // will the GC be enough? NOPE
                node.Stop();
            });
        }

        private IControlNodeFactoryPlugin[] LoadAssembly(string assemblyPath)
        {
            string assembly = Path.GetFullPath(assemblyPath);
            Assembly ptrAssembly = Assembly.LoadFile(assembly);
            try
            {
                return ptrAssembly.GetTypes().AsEnumerable()
                    .Where(item => item.IsClass)
                    .Where(item => item.GetInterfaces().Contains(typeof(IControlNodeFactoryPlugin)))
                    .ToList()
                    .Select(item => (IControlNodeFactoryPlugin)Activator.CreateInstance(item))
                    .ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new IControlNodeFactoryPlugin[0];
            }
            /*foreach (Type item in ptrAssembly.GetTypes())
            {
                if (!item.IsClass) continue;
                if (item.GetInterfaces().Contains(typeof(IControlNodeFactoryPlugin)))
                {
                    return (IControlNodeFactoryPlugin)Activator.CreateInstance(item);
                }
            }
            throw new Exception("Invalid DLL, Interface not found!");*/
        }


        private void NodeGraph1_NodeDeleted(object sender, NodeGraph.NodeDeletedEventArgs e)
        {
            //e.Node.Stop();
            e.Node.Dispose();
        }

        private void NodeGraph1_NodeCreated(object sender, NodeGraph.NodeCreatedEventArgs e)
        {
            //if(e.Node.ExecutionType == NetworkModel.NodeExecutionType.Self)
            //{
            e.Node.Start();
            //}
        }



        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dictionary<Guid, Guid> Remap = new Dictionary<Guid, Guid>();

            Func<Guid, Guid> convertGuid = ((input) =>
            {
                if (Remap.ContainsKey(input)) return Remap[input];
                Remap[input] = Guid.NewGuid();
                return Remap[input];
            });

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Take a copy of the selected nodes list so we can delete nodes while iterating.
                var nodesCopy = this.nodeGraph1.ViewModel.Network.Nodes.ToArray();
                foreach (var node in nodesCopy)
                {
                    //if (node.IsSelected)
                    {
                        nodeGraph1.ViewModel.DeleteNode(node);
                        node.Dispose();
                    }
                }

                {
                    JObject obj = JObject.Parse(File.ReadAllText(openFileDialog1.FileName));

                    if (obj["nodes"] != null && obj["nodes"].Type == JTokenType.Array)
                    {
                        List<NodeViewModel> nodeList = new List<NodeViewModel>();

                        JArray arr = (JArray)obj["nodes"];
                        arr.ToList().ForEach(dr =>
                        {
                            if (dr["type"] != null && dr["type"].Type == JTokenType.String)
                            {
                                string type = (string)dr["type"];
                                if (TypeToFactory.ContainsKey(type))
                                {
                                    Guid[] _ei = dr["ei"] != null && dr["ei"].Type == JTokenType.Array ? ((JArray)dr["ei"]).Select(dx => convertGuid(new Guid((string)dx))).ToArray() : new Guid[0];
                                    Guid[] _eo = dr["eo"] != null && dr["eo"].Type == JTokenType.Array ? ((JArray)dr["eo"]).Select(dx => convertGuid(new Guid((string)dx))).ToArray() : new Guid[0];
                                    Guid[] _di = dr["di"] != null && dr["di"].Type == JTokenType.Array ? ((JArray)dr["di"]).Select(dx => convertGuid(new Guid((string)dx))).ToArray() : new Guid[0];
                                    Guid[] _do = dr["do"] != null && dr["do"].Type == JTokenType.Array ? ((JArray)dr["do"]).Select(dx => convertGuid(new Guid((string)dx))).ToArray() : new Guid[0];

                                    NodeViewModel model = TypeToFactory[type].CreateNode((JObject)dr["data"], _ei, _eo, _di, _do);
                                    if (model != null)
                                    {
                                        nodeList.Add(model);

                                        model.X = dr["x"] != null && dr["x"].Type == JTokenType.Float ? (float)dr["x"] : 0f;
                                        model.Y = dr["y"] != null && dr["y"].Type == JTokenType.Float ? (float)dr["y"] : 0f;
                                        NodeViewModel created = nodeGraph1.ViewModel.CreateNode(model, /*position,*/ false);
                                    }
                                }
                            }
                        });

                        if (obj["dcons"] != null && obj["dcons"].Type == JTokenType.Array)
                        {
                            ((JArray)obj["dcons"]).ToList().ForEach(dr =>
                            {
                                if (dr != null && dr.Type == JTokenType.Object)
                                {
                                    Guid conSource = convertGuid(new Guid((string)(((JObject)dr)["s"])));
                                    Guid conDest = convertGuid(new Guid((string)(((JObject)dr)["d"])));

                                    ConnectorViewModel conSource2 = nodeList.SelectMany(dx => dx.OutputConnectors).Where(dx => dx.Guid == conSource).FirstOrDefault();
                                    ConnectorViewModel conDest2 = nodeList.SelectMany(dx => dx.InputConnectors).Where(dx => dx.Guid == conDest).FirstOrDefault();

                                    if (conSource2 != null && conDest2 != null)
                                    {
                                        nodeGraph1.ViewModel.Network.Connections.Add(new ConnectionViewModel()
                                        {
                                            DestConnector = conDest2,
                                            SourceConnector = conSource2
                                        });
                                    }
                                }
                            });
                        }

                        if (obj["econs"] != null && obj["econs"].Type == JTokenType.Array)
                        {
                            ((JArray)obj["econs"]).ToList().ForEach(dr =>
                            {
                                if (dr != null && dr.Type == JTokenType.Object)
                                {
                                    Guid conSource = convertGuid(new Guid((string)(((JObject)dr)["s"])));
                                    Guid conDest = convertGuid(new Guid((string)(((JObject)dr)["d"])));

                                    ExecutionConnectorViewModel conSource2 = nodeList.SelectMany(dx => dx.OutputExecutionConnectors).Where(dx => dx.Guid == conSource).FirstOrDefault();
                                    ExecutionConnectorViewModel conDest2 = nodeList.SelectMany(dx => dx.InputExecutionConnectors).Where(dx => dx.Guid == conDest).FirstOrDefault();

                                    if (conSource2 != null && conDest2 != null)
                                    {
                                        nodeGraph1.ViewModel.Network.ExecutionConnections.Add(new ExecutionConnectionViewModel()
                                        {
                                            DestConnector = conDest2,
                                            SourceConnector = conSource2
                                        });
                                    }
                                }
                            });
                        }

                        nodeList.ForEach(dr => dr.Start());
                    }
                }
            }
        }

        private void appendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                JObject obj = JObject.Parse(File.ReadAllText(openFileDialog1.FileName));

                if (obj["nodes"] != null && obj["nodes"].Type == JTokenType.Array)
                {
                    List<NodeViewModel> nodeList = new List<NodeViewModel>();

                    JArray arr = (JArray)obj["nodes"];
                    arr.ToList().ForEach(dr =>
                    {
                        if (dr["type"] != null && dr["type"].Type == JTokenType.String)
                        {
                            string type = (string)dr["type"];
                            if (TypeToFactory.ContainsKey(type))
                            {
                                Guid[] _ei = dr["ei"] != null && dr["ei"].Type == JTokenType.Array ? ((JArray)dr["ei"]).Select(dx => new Guid((string)dx)).ToArray() : new Guid[0];
                                Guid[] _eo = dr["eo"] != null && dr["eo"].Type == JTokenType.Array ? ((JArray)dr["eo"]).Select(dx => new Guid((string)dx)).ToArray() : new Guid[0];
                                Guid[] _di = dr["di"] != null && dr["di"].Type == JTokenType.Array ? ((JArray)dr["di"]).Select(dx => new Guid((string)dx)).ToArray() : new Guid[0];
                                Guid[] _do = dr["do"] != null && dr["do"].Type == JTokenType.Array ? ((JArray)dr["do"]).Select(dx => new Guid((string)dx)).ToArray() : new Guid[0];

                                NodeViewModel model = TypeToFactory[type].CreateNode((JObject)dr["data"], _ei, _eo, _di, _do);
                                if (model != null)
                                {
                                    nodeList.Add(model);

                                    model.X = dr["x"] != null && dr["x"].Type == JTokenType.Float ? (float)dr["x"] : 0f;
                                    model.Y = dr["y"] != null && dr["y"].Type == JTokenType.Float ? (float)dr["y"] : 0f;
                                    NodeViewModel created = nodeGraph1.ViewModel.CreateNode(model, /*position,*/ false);
                                }
                            }
                        }
                    });

                    if (obj["dcons"] != null && obj["dcons"].Type == JTokenType.Array)
                    {
                        ((JArray)obj["dcons"]).ToList().ForEach(dr =>
                        {
                            if (dr != null && dr.Type == JTokenType.Object)
                            {
                                Guid conSource = new Guid((string)(((JObject)dr)["s"]));
                                Guid conDest = new Guid((string)(((JObject)dr)["d"]));

                                ConnectorViewModel conSource2 = nodeList.SelectMany(dx => dx.OutputConnectors).Where(dx => dx.Guid == conSource).FirstOrDefault();
                                ConnectorViewModel conDest2 = nodeList.SelectMany(dx => dx.InputConnectors).Where(dx => dx.Guid == conDest).FirstOrDefault();

                                if (conSource2 != null && conDest2 != null)
                                {
                                    nodeGraph1.ViewModel.Network.Connections.Add(new ConnectionViewModel()
                                    {
                                        DestConnector = conDest2,
                                        SourceConnector = conSource2
                                    });
                                }
                            }
                        });
                    }

                    if (obj["econs"] != null && obj["econs"].Type == JTokenType.Array)
                    {
                        ((JArray)obj["econs"]).ToList().ForEach(dr =>
                        {
                            if (dr != null && dr.Type == JTokenType.Object)
                            {
                                Guid conSource = new Guid((string)(((JObject)dr)["s"]));
                                Guid conDest = new Guid((string)(((JObject)dr)["d"]));

                                ExecutionConnectorViewModel conSource2 = nodeList.SelectMany(dx => dx.OutputExecutionConnectors).Where(dx => dx.Guid == conSource).FirstOrDefault();
                                ExecutionConnectorViewModel conDest2 = nodeList.SelectMany(dx => dx.InputExecutionConnectors).Where(dx => dx.Guid == conDest).FirstOrDefault();

                                if (conSource2 != null && conDest2 != null)
                                {
                                    nodeGraph1.ViewModel.Network.ExecutionConnections.Add(new ExecutionConnectionViewModel()
                                    {
                                        DestConnector = conDest2,
                                        SourceConnector = conSource2
                                    });
                                }
                            }
                        });
                    }

                    nodeList.ForEach(dr => dr.Start());
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                List<FunctionalNetworkModel.NodeViewModel> nodes = this.nodeGraph1.ViewModel.Network.Nodes.ToList();
                List<FunctionalNetworkModel.ConnectionViewModel> connections = nodes.SelectMany(dr => dr.AttachedConnections).Distinct().Where(dr => nodes.Contains(dr.DestConnector.ParentNode)).ToList();
                List<FunctionalNetworkModel.ExecutionConnectionViewModel> executionConnections = nodes.SelectMany(dr => dr.AttachedExecutionConnections).Distinct().Where(dr => nodes.Contains(dr.DestConnector.ParentNode)).ToList();

                JObject SaveData = new JObject();
                SaveData["nodes"] = JArray.FromObject(nodes
                    .Select(dr =>
                    {
                        JObject tmp = new JObject();
                        tmp["type"] = dr.Type;
                        tmp["data"] = dr.Serialize();
                        tmp["x"] = dr.X;
                        tmp["y"] = dr.Y;
                        if (dr.InputExecutionConnectors.Count > 0) tmp["ei"] = JArray.FromObject(dr.InputExecutionConnectors.Select(dx => dx.Guid));
                        if (dr.OutputExecutionConnectors.Count > 0) tmp["eo"] = JArray.FromObject(dr.OutputExecutionConnectors.Select(dx => dx.Guid));
                        if (dr.InputConnectors.Count > 0) tmp["di"] = JArray.FromObject(dr.InputConnectors.Select(dx => dx.Guid));
                        if (dr.OutputConnectors.Count > 0) tmp["do"] = JArray.FromObject(dr.OutputConnectors.Select(dx => dx.Guid));
                        return tmp;
                    })
                    .ToList());
                SaveData["dcons"] = JArray.FromObject(connections
                    .Select(dr =>
                    {
                        JObject tmp = new JObject();
                        tmp["s"] = dr.SourceConnector.Guid;
                        tmp["d"] = dr.DestConnector.Guid;
                        return tmp;
                    })
                    .Where(dr => dr != null)
                    .ToList());
                SaveData["econs"] = JArray.FromObject(executionConnections
                    .Select(dr =>
                    {
                        JObject tmp = new JObject();
                        tmp["s"] = dr.SourceConnector.Guid;
                        tmp["d"] = dr.DestConnector.Guid;
                        return tmp;
                    })
                    .Where(dr => dr != null)
                    .ToList());

                using (StreamWriter writer = File.CreateText(saveFileDialog1.FileName))
                {
                    writer.Write(SaveData.ToString());
                }
            }
        }

        private void saveSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                List<FunctionalNetworkModel.NodeViewModel> nodes = this.nodeGraph1.ViewModel.Network.Nodes.Where(dr => dr.IsSelected).ToList();
                List<FunctionalNetworkModel.ConnectionViewModel> connections = nodes.SelectMany(dr => dr.AttachedConnections).Distinct().Where(dr => nodes.Contains(dr.DestConnector.ParentNode)).ToList();
                List<FunctionalNetworkModel.ExecutionConnectionViewModel> executionConnections = nodes.SelectMany(dr => dr.AttachedExecutionConnections).Distinct().Where(dr => nodes.Contains(dr.DestConnector.ParentNode)).ToList();

                JObject SaveData = new JObject();
                SaveData["nodes"] = JArray.FromObject(nodes
                    .Select(dr =>
                    {
                        JObject tmp = new JObject();
                        tmp["type"] = dr.Type;
                        tmp["data"] = dr.Serialize();
                        tmp["x"] = dr.X;
                        tmp["y"] = dr.Y;
                        tmp["ei"] = JArray.FromObject(dr.InputExecutionConnectors.Select(dx => dx.Guid));
                        tmp["eo"] = JArray.FromObject(dr.OutputExecutionConnectors.Select(dx => dx.Guid));
                        tmp["di"] = JArray.FromObject(dr.InputConnectors.Select(dx => dx.Guid));
                        tmp["do"] = JArray.FromObject(dr.OutputConnectors.Select(dx => dx.Guid));
                        return tmp;
                    })
                    .ToList());
                SaveData["dcons"] = JArray.FromObject(connections
                    .Select(dr =>
                    {
                        JObject tmp = new JObject();
                        tmp["s"] = dr.SourceConnector.Guid;
                        tmp["d"] = dr.DestConnector.Guid;
                        return tmp;
                    })
                    .Where(dr => dr != null)
                    .ToList());
                SaveData["econs"] = JArray.FromObject(executionConnections
                    .Select(dr =>
                    {
                        JObject tmp = new JObject();
                        tmp["s"] = dr.SourceConnector.Guid;
                        tmp["d"] = dr.DestConnector.Guid;
                        return tmp;
                    })
                    .Where(dr => dr != null)
                    .ToList());

                using (StreamWriter writer = File.CreateText(saveFileDialog1.FileName))
                {
                    writer.Write(SaveData.ToString());
                }
            }
        }

        private void FunctionGraphForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.ApplicationExitCall)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void duplicateNodesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string dmpCache = string.Empty;
            {
                List<FunctionalNetworkModel.NodeViewModel> nodes = this.nodeGraph1.ViewModel.Network.Nodes.Where(dr => dr.IsSelected).ToList();
                List<FunctionalNetworkModel.ConnectionViewModel> connections = nodes.SelectMany(dr => dr.AttachedConnections).Distinct().Where(dr => nodes.Contains(dr.DestConnector.ParentNode)).ToList();
                List<FunctionalNetworkModel.ExecutionConnectionViewModel> executionConnections = nodes.SelectMany(dr => dr.AttachedExecutionConnections).Distinct().Where(dr => nodes.Contains(dr.DestConnector.ParentNode)).ToList();

                JObject SaveData = new JObject();
                SaveData["nodes"] = JArray.FromObject(nodes
                    .Select(dr =>
                    {
                        JObject tmp = new JObject();
                        tmp["type"] = dr.Type;
                        tmp["data"] = dr.Serialize();
                        tmp["x"] = dr.X;
                        tmp["y"] = dr.Y;
                        tmp["ei"] = JArray.FromObject(dr.InputExecutionConnectors.Select(dx => dx.Guid));
                        tmp["eo"] = JArray.FromObject(dr.OutputExecutionConnectors.Select(dx => dx.Guid));
                        tmp["di"] = JArray.FromObject(dr.InputConnectors.Select(dx => dx.Guid));
                        tmp["do"] = JArray.FromObject(dr.OutputConnectors.Select(dx => dx.Guid));
                        return tmp;
                    })
                    .ToList());
                SaveData["dcons"] = JArray.FromObject(connections
                    .Select(dr =>
                    {
                        JObject tmp = new JObject();
                        tmp["s"] = dr.SourceConnector.Guid;
                        tmp["d"] = dr.DestConnector.Guid;
                        return tmp;
                    })
                    .Where(dr => dr != null)
                    .ToList());
                SaveData["econs"] = JArray.FromObject(executionConnections
                    .Select(dr =>
                    {
                        JObject tmp = new JObject();
                        tmp["s"] = dr.SourceConnector.Guid;
                        tmp["d"] = dr.DestConnector.Guid;
                        return tmp;
                    })
                    .Where(dr => dr != null)
                    .ToList());

                dmpCache = SaveData.ToString();
            }
            {
                JObject obj = JObject.Parse(dmpCache);

                if (obj["nodes"] != null && obj["nodes"].Type == JTokenType.Array)
                {
                    List<NodeViewModel> nodeList = new List<NodeViewModel>();

                    JArray arr = (JArray)obj["nodes"];
                    arr.ToList().ForEach(dr =>
                    {
                        if (dr["type"] != null && dr["type"].Type == JTokenType.String)
                        {
                            string type = (string)dr["type"];
                            if (TypeToFactory.ContainsKey(type))
                            {
                                Guid[] _ei = dr["ei"] != null && dr["ei"].Type == JTokenType.Array ? ((JArray)dr["ei"]).Select(dx => new Guid((string)dx)).ToArray() : new Guid[0];
                                Guid[] _eo = dr["eo"] != null && dr["eo"].Type == JTokenType.Array ? ((JArray)dr["eo"]).Select(dx => new Guid((string)dx)).ToArray() : new Guid[0];
                                Guid[] _di = dr["di"] != null && dr["di"].Type == JTokenType.Array ? ((JArray)dr["di"]).Select(dx => new Guid((string)dx)).ToArray() : new Guid[0];
                                Guid[] _do = dr["do"] != null && dr["do"].Type == JTokenType.Array ? ((JArray)dr["do"]).Select(dx => new Guid((string)dx)).ToArray() : new Guid[0];

                                NodeViewModel model = TypeToFactory[type].CreateNode((JObject)dr["data"], _ei, _eo, _di, _do);
                                if (model != null)
                                {
                                    nodeList.Add(model);

                                    model.X = dr["x"] != null && dr["x"].Type == JTokenType.Float ? (float)dr["x"] : 0f;
                                    model.Y = dr["y"] != null && dr["y"].Type == JTokenType.Float ? (float)dr["y"] : 0f;
                                    NodeViewModel created = nodeGraph1.ViewModel.CreateNode(model, /*position,*/ false);
                                }
                            }
                        }
                    });

                    if (obj["dcons"] != null && obj["dcons"].Type == JTokenType.Array)
                    {
                        ((JArray)obj["dcons"]).ToList().ForEach(dr =>
                        {
                            if (dr != null && dr.Type == JTokenType.Object)
                            {
                                Guid conSource = new Guid((string)(((JObject)dr)["s"]));
                                Guid conDest = new Guid((string)(((JObject)dr)["d"]));

                                ConnectorViewModel conSource2 = nodeList.SelectMany(dx => dx.OutputConnectors).Where(dx => dx.Guid == conSource).FirstOrDefault();
                                ConnectorViewModel conDest2 = nodeList.SelectMany(dx => dx.InputConnectors).Where(dx => dx.Guid == conDest).FirstOrDefault();

                                if (conSource2 != null && conDest2 != null)
                                {
                                    nodeGraph1.ViewModel.Network.Connections.Add(new ConnectionViewModel()
                                    {
                                        DestConnector = conDest2,
                                        SourceConnector = conSource2
                                    });
                                }
                            }
                        });
                    }

                    if (obj["econs"] != null && obj["econs"].Type == JTokenType.Array)
                    {
                        ((JArray)obj["econs"]).ToList().ForEach(dr =>
                        {
                            if (dr != null && dr.Type == JTokenType.Object)
                            {
                                Guid conSource = new Guid((string)(((JObject)dr)["s"]));
                                Guid conDest = new Guid((string)(((JObject)dr)["d"]));

                                ExecutionConnectorViewModel conSource2 = nodeList.SelectMany(dx => dx.OutputExecutionConnectors).Where(dx => dx.Guid == conSource).FirstOrDefault();
                                ExecutionConnectorViewModel conDest2 = nodeList.SelectMany(dx => dx.InputExecutionConnectors).Where(dx => dx.Guid == conDest).FirstOrDefault();

                                if (conSource2 != null && conDest2 != null)
                                {
                                    nodeGraph1.ViewModel.Network.ExecutionConnections.Add(new ExecutionConnectionViewModel()
                                    {
                                        DestConnector = conDest2,
                                        SourceConnector = conSource2
                                    });
                                }
                            }
                        });
                    }

                    nodeList.ForEach(dr => dr.Start());
                }
            }
        }
    }
}
