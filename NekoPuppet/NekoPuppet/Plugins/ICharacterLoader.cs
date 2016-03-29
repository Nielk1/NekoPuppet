using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoPuppet.Plugins
{
    public interface ICharacterLoader
    {
        List<ICharacterListViewItem> GetCharacters();
    }
}
