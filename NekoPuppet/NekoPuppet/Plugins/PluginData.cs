using NekoPuppet;
using System.Collections.Generic;

namespace NekoPuppet.Plugins
{
    public struct NodeDescription
    {
        public string Name;
        public string NodeMenu;
        public IControlNodeFactoryPlugin Factory;
        public string TypeString;
    }

    internal interface IPluginDescription
    {
        List<NodeDescription> NodeDescriptions { get; }
    }
}