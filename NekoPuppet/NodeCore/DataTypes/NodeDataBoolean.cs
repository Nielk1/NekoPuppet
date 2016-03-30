using FunctionalNetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeCore.DataTypes
{
    class NodeDataBoolean : INodeData, INodeDataBoolean, IComparableNodeData
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

        public override string ToString()
        {
            return string.Format("[Boolean:{0}]", val);
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

        public int CompareTo(object obj)
        {
            if (typeof(INodeDataBoolean).IsAssignableFrom(obj.GetType()))
            {
                INodeDataBoolean objTmp = (INodeDataBoolean)obj;
                if (objTmp.GetBool() == this.GetBool()) return 0;
                if (objTmp.GetBool()) return -1;
                if (this.GetBool()) return 1;
            }
            throw new ArgumentException("Could not compare values");
        }

        public override bool Equals(object other)
        {
            if (typeof(INodeDataBoolean).IsAssignableFrom(other.GetType()))
            {
                INodeDataBoolean objTmp = (INodeDataBoolean)other;
                return objTmp.GetBool() == this.GetBool();
            }
            return false;
        }

        public object Clone()
        {
            return NodeDataBoolean.FromBoolean(this.val);
        }
    }
}
