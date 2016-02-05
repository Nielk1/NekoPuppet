using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NekoParaCharacterTest.Utilities
{
    public partial class PropertyDialog : Form
    {
        public PropertyDialog()
        {
            InitializeComponent();
        }

        public object Value
        {
            get { return propertyGrid1.SelectedObject; }
            set
            {
                propertyGrid1.SelectedObject = value;
            }
        }
    }

    public class PropertyDialogCollection
    {

    }
}
