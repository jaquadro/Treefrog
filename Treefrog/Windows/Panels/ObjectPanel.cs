using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Treefrog.Presentation;

namespace Treefrog.Windows.Panels
{
    public partial class ObjectPanel : UserControl
    {
        private IObjectPoolCollectionPresenter _controller;

        public ObjectPanel ()
        {
            InitializeComponent();

            ResetComponent();

            // Load form elements

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            _buttonRemoveObject.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.game--minus.png"));
            _buttonAddObject.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.game--plus.png"));
        }

        private void ResetComponent ()
        {
            _buttonAddObject.Enabled = false;
            _buttonRemoveObject.Enabled = false;
        }

        public void BindController (IObjectPoolCollectionPresenter controller)
        {
            if (_controller == controller)
                return;

            if (_controller != null) {

            }

            _controller = controller;

            if (_controller != null) {

            }
            else {
                ResetComponent();
            }
        }
    }
}
