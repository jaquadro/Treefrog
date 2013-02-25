using System;
using System.Windows.Forms;
using Treefrog.Presentation;
using Treefrog.Presentation.Layers;
using Treefrog.Windows.Controls;
using Treefrog.Windows.Layers;
using Treefrog.Presentation.Controllers;
using System.Drawing;
using Treefrog.Windows.Controllers;

namespace Treefrog.Windows
{
    public partial class LevelPanel : UserControl
    {
        private ControlPointerEventController _pointerController;
        private LevelPresenter _controller;
        private LayerGraphicsControl _layerControl;
        private GroupLayer _root;

        public LevelPanel ()
        {
            InitializeComponent();

            _layerControl = new LayerGraphicsControl();
            _layerControl.Dock = DockStyle.Fill;

            _viewportControl.Control = _layerControl;

            _pointerController = new ControlPointerEventController(_layerControl, _layerControl);
        }

        protected override void Dispose (bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();

                _pointerController.Dispose();
                _layerControl.Dispose();
            }

            base.Dispose(disposing);
        }

        public void BindController (LevelPresenter controller)
        {
            if (_controller != null) {
                _controller.LevelGeometry = null;
                _controller.PointerEventResponderChanged -= PointerEventResponderChanged;
                _controller.ContextMenuActivated -= ContextMenuActivatedHandler;
            }

            _controller = controller;
            if (_controller != null) {
                _controller.LevelGeometry = _layerControl.LevelGeometry;
                _controller.PointerEventResponderChanged += PointerEventResponderChanged;
                _controller.ContextMenuActivated += ContextMenuActivatedHandler;

                _pointerController.Responder = _controller.PointerEventResponder;

                _root = new GroupLayer(controller.RootLayer);

                _layerControl.RootLayer = _root;
                _layerControl.TextureCache.SourcePool = _controller.TexturePool;

                _layerControl.ReferenceOriginX = _controller.Level.OriginX;
                _layerControl.ReferenceOriginY = _controller.Level.OriginY;
                _layerControl.ReferenceWidth = _controller.Level.Width;
                _layerControl.ReferenceHeight = _controller.Level.Height;
            }
            else {
                _root = null;
                _layerControl.RootLayer = null;
                _layerControl.TextureCache.SourcePool = null;

                _pointerController.Responder = null;
            }
        }

        private void ResetComponent ()
        {
        }

        private void PointerEventResponderChanged (object sender, EventArgs e)
        {
            _pointerController.Responder = _controller.PointerEventResponder;
        }

        UICommandController _contextController;

        private void ContextMenuActivatedHandler (object sender, ContextMenuEventArgs e)
        {
            ContextMenuStrip menu = CommandMenuBuilder.BuildContextMenu(e.Menu);

            if (_contextController != null)
                _contextController.Dispose();

            _contextController = new UICommandController();
            _contextController.BindCommandManager(_controller.CommandManager);
            _contextController.MapMenuItems(menu.Items);

            menu.Show(this, _pointerController.UntranslatePosition(new Point(e.Location.X, e.Location.Y)));
        }
    }
}
