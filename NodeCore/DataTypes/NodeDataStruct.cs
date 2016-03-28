/*using FunctionalNetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeCore.DataTypes
{
    abstract class NodeDataStruct : INodeData
    {
        private string[] keys;

        public string[] Keys { get { return keys; } }
        public abstract IDictionary<string, INodeData> Values { get; }

        private NodeDataStruct() { }

        public NodeDataStruct(string[] keys)
        {
            this.keys = keys;
        }
    }
}
*/