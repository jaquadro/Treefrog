using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Treefrog.Render.Layers;
using Treefrog.Presentation;
using Treefrog.Presentation.Controllers;

namespace Treefrog.Windows.Panels
{
    public partial class MinimapPanel : UserControl
    {
        private ControlPointerEventController _pointerController;
        private MinimapPresenter _controller;
        private GroupLayer _root;

        public MinimapPanel ()
        {
            InitializeComponent();

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

        public void BindController (MinimapPresenter controller)
        {
            if (_controller == controller)
                return;

            if (_controller != null) {
                _controller.CurrentLevelChanged -= CurrentLevelChanged;
                _controller.PointerEventResponderChanged -= PointerEventResponderChanged;
            }

            _controller = controller;

            if (_controller != null) {
                _controller.CurrentLevelChanged += CurrentLevelChanged;
                _controller.PointerEventResponderChanged += PointerEventResponderChanged;

                _pointerController.Responder = _controller.PointerEventResponder;
            }
            else {
                _pointerController.Responder = null;
            }

            BindCurrentLevel();
        }

        private void BindCurrentLevel ()
        {
            if (_controller.Level != null) {
                _root = new GroupLayer(_controller.RootLayer);

                _controller.LevelGeometry = _layerControl.LevelGeometry;

                _layerControl.RootLayer = _root;
                _layerControl.TextureCache.SourcePool = _controller.TexturePool;

                _layerControl.ReferenceOriginX = _controller.Level.OriginX;
                _layerControl.ReferenceOriginY = _controller.Level.OriginY;
                _layerControl.ReferenceWidth = _controller.Level.Width;
                _layerControl.ReferenceHeight = _controller.Level.Height;

                float zoomWidth = (float)_layerControl.Width / _layerControl.ReferenceWidth;
                float zoomHeight = (float)_layerControl.Height / _layerControl.ReferenceHeight;
                _layerControl.Zoom = Math.Min(zoomWidth, zoomHeight);
            }
            else {
                _root = null;
                _layerControl.RootLayer = null;
                _layerControl.TextureCache.SourcePool = null;
            }
        }

        private void PointerEventResponderChanged (object sender, EventArgs e)
        {
            _pointerController.Responder = _controller.PointerEventResponder;
        }

        private void CurrentLevelChanged (object sender, EventArgs e)
        {
            BindCurrentLevel();
        }

        private void LayerControlScrolled (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.UpdateAreaBox();
        }
    }
}
