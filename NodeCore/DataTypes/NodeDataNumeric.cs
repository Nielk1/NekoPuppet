using FunctionalNetworkModel;
using System;

namespace NekoPuppet.Plugins.Nodes.Core.Data
{
    public class NodeDataNumeric : INodeData
    {
        private Int16 valInt16;
        private Byte valByte;
        private Double valDouble;

        private TypeTrack Track = TypeTrack.Empty;

        private enum TypeTrack
        {
            Empty,
            Byte,
            Int16,
            Double
        }

        private NodeDataNumeric(Int16 val)
        {
            this.Track = TypeTrack.Int16;
            this.valInt16 = val;
        }

        private NodeDataNumeric(Byte val)
        {
            this.Track = TypeTrack.Byte;
            this.valByte = val;
        }

        private NodeDataNumeric(Double val)
        {
            this.Track = TypeTrack.Double;
            this.valDouble = val;
        }

        public NodeDataNumeric()
        {
            this.Track = TypeTrack.Empty;
        }

        public double GetDouble()
        {
            switch (Track)
            {
                case TypeTrack.Empty:
                    return 0d;
                case TypeTrack.Byte:
                    return (double)valByte;
                case TypeTrack.Int16:
                    return (double)valInt16;
                case TypeTrack.Double:
                    return valDouble;
            }
            return 0d;
        }

        public override string ToString()
        {
            switch (Track)
            {
                case TypeTrack.Empty:
                    return "[Empty:0]";
                case TypeTrack.Byte:
                    return string.Format("[Byte:{0:X2}]", valByte);
                case TypeTrack.Int16:
                    return string.Format("[Int16:{0}]", valInt16);
                case TypeTrack.Double:
                    return string.Format("[Double:{0: 0.0000000000;-0.0000000000}]", valDouble); ;
            }
            return "[?]";
        }

        public static NodeDataNumeric FromInt16(Int16 val)
        {
            return new NodeDataNumeric(val);
        }

        public static NodeDataNumeric FromByte(Byte val)
        {
            return new NodeDataNumeric(val);
        }

        public static NodeDataNumeric FromDouble(Double val)
        {
            return new NodeDataNumeric(val);
        }

        public static NodeDataNumeric MakeZero()
        {
            return new NodeDataNumeric();
        }
    }
}
