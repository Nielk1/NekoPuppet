using FunctionalNetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeCore.DataTypes
{
    class NodeDataBoolean : INodeData, INodeDataBoolean
    {
        private bool val;

        private NodeDataBoolean(Boolean val)
        {
            this.val = val;
        }

        public bool GetBool()
        {
            return val;
        }

        public static NodeDataBoolean FromBoolean(Boolean val)
        {
            return new NodeDataBoolean(val);
        }

        public static NodeDataBoolean MakeTrue()
        {
            return new NodeDataBoolean(true);
        }

        public static NodeDataBoolean MakeFalse()
        {
            return new NodeDataBoolean(false);
        }
    }
}
