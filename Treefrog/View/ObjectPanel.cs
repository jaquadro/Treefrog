using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Treefrog.View.Controls.WinEx;

namespace Treefrog.View
{
    public partial class ObjectPanel : UserControl
    {
        public ObjectPanel ()
        {
            InitializeComponent();

            ResetComponent();

            // Load form elements

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            _addObjectButton.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.game--plus.png"));
            _removeObjectButton.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.game--minus.png"));
            _addCatButton.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.box--plus.png"));
            _removeCatButton.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.box--minus.png"));

            _viewButton.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.icons-large.png"));
            _viewItemLarge.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.icons-large.png"));
            _viewItemMedium.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.icons-medium.png"));
            _viewItemSmall.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.icons-small.png"));

            ComboBoxEx box = new ComboBoxEx();
            box.FlatStyle = FlatStyle.Popup;

            ToolStripControlHost host = new ToolStripControlHost(box);
            toolStrip1.Items.Remove(_categoryBox);
            toolStrip1.Items.Add(host);

        }

        private void ResetComponent ()
        {
            _removeObjectButton.Enabled = false;
            _removeCatButton.Enabled = false;
        }


    }
}
