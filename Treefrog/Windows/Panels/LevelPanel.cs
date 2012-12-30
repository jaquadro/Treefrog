using System;
using System.Windows.Forms;
using Treefrog.Presentation;
using Treefrog.Presentation.Layers;
using Treefrog.Windows.Controls;
using Treefrog.Windows.Layers;

namespace Treefrog.Windows
{
    public partial class LevelPanel : UserControl
    {
        #region Fields

        //private ILevelPresenter _controller;
        private LevelPresenter2 _controller;
        private LayerGraphicsControl _layerControl;
        private GroupLayer _root;

        #endregion

        #region Constructors

        public LevelPanel ()
        {
            InitializeComponent();

            _layerControl = new LayerGraphicsControl();
            _layerControl.Dock = DockStyle.Fill;

            _viewportControl.Control = _layerControl;
        }

        #endregion

        public void BindController (LevelPresenter2 controller)
        {
            _controller = controller;
            _controller.LevelGeometry = _layerControl.LevelGeometry;

            _root = new GroupLayer() {
                IsRendered = true,
                Model = controller.RootLayer,
            };

            _layerControl.RootLayer = _root;
            _layerControl.TextureCache.SourcePool = controller.TexturePool;

            _layerControl.ReferenceOriginX = controller.Level.OriginX;
            _layerControl.ReferenceOriginY = controller.Level.OriginY;
            _layerControl.ReferenceWidth = controller.Level.Width;
            _layerControl.ReferenceHeight = controller.Level.Height;
        }

        private void ResetComponent ()
        {
        }
    }
}
