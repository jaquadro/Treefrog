using System;
using System.Windows.Forms;
using Treefrog.Presentation;
using Treefrog.Presentation.Layers;

namespace Treefrog.View
{
    public partial class LevelPanel : UserControl
    {
        #region Fields

        private ILevelPresenter _controller;

        #endregion

        #region Constructors

        public LevelPanel ()
        {
            InitializeComponent();
        }

        #endregion

        public void BindController (ILevelPresenter controller)
        {
            _controller = controller;

            viewportControl1.ContentPanel.Controls.Clear();

            if (_controller != null && _controller.LayerControl != null) {
                _controller.LayerControl.Dock = DockStyle.Fill;

                viewportControl1.Control = _controller.LayerControl;
            }
        }

        private void ResetComponent ()
        {
            viewportControl1.ContentPanel.Controls.Clear();
        }

        #region Properties

        #endregion

        #region Event Handlers

        private void LayerControlTileMouseMove (object sender, TileMouseEventArgs e)
        {

        }

        #endregion
    }
}
