using FunctionalNetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace NekoPuppet
{
    public interface IControlNodeFactoryPlugin
    {
        string TypeString { get; }
        string NodeMenu { get; }
        string Name { get; }

        NodeViewModel CreateNode();
        NodeViewModel CreateNode(JObject data, Guid[] executeIn, Guid[] executeOut, Guid[] dataIn, Guid[] dataOut);
    }
}
