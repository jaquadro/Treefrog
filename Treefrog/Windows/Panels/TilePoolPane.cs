using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Treefrog.Framework.Model;
using Treefrog.Presentation;
using Treefrog.Presentation.Commands;
using Treefrog.Presentation.Controllers;
using Treefrog.Windows.Controllers;
using Treefrog.Windows.Layers;

namespace Treefrog.Windows
{
    public partial class TilePoolPane : UserControl
    {
        private ITilePoolListPresenter _controller;
        private TilePoolPresenter _tilePool;
        private ControlPointerEventController _pointerController;
        private GroupLayer _root;

        private UICommandController _commandController;

        public TilePoolPane ()
        {
            InitializeComponent();

            ResetComponent();

            // Load form elements

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            _buttonRemove.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons.minus16.png"));
            _buttonAdd.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons.plus16.png"));
            _buttonProperties.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.tags.png"));

            _commandController = new UICommandController();
            _commandController.MapButtons(new Dictionary<CommandKey, ToolStripButton>() {
                { CommandKey.TilePoolDelete, _buttonRemove },
            });
            _commandController.MapMenuItems(new Dictionary<CommandKey, ToolStripMenuItem>() {
                { CommandKey.TilePoolImport, importNewToolStripMenuItem },
            });

            _pointerController = new ControlPointerEventController(_layerControl, _layerControl);

            // Setup control

            _layerControl.BackColor = System.Drawing.Color.SlateGray;
            _layerControl.WidthSynced = true;
            _layerControl.CanvasAlignment = CanvasAlignment.UpperLeft;

            // Wire events

            _poolComboBox.SelectedIndexChanged += SelectTilePoolHandler;
        }

        public void BindController (ITilePoolListPresenter controller)
        {
            if (_controller == controller) {
                return;
            }

            if (_controller != null) {
                _controller.SyncTilePoolManager -= SyncTilePoolManagerHandler;
                _controller.SyncTilePoolList -= SyncTilePoolListHandler;
                _controller.SyncTilePoolControl -= SyncTilePoolControlHandler;
                _controller.SelectedTilePoolChanged -= SelectedTilePoolChangedHandler;
            }

            _controller = controller;

            if (_controller != null) {
                _controller.SyncTilePoolManager += SyncTilePoolManagerHandler;
                _controller.SyncTilePoolList += SyncTilePoolListHandler;
                _controller.SyncTilePoolControl += SyncTilePoolControlHandler;
                _controller.SelectedTilePoolChanged += SelectedTilePoolChangedHandler;

                _commandController.BindCommandManager(_controller.CommandManager);

                BindTilePoolManager(_controller.TilePoolManager);
                BindTilePool(_controller.SelectedTilePool);
            }
            else {
                _commandController.BindCommandManager(null);

                BindTilePoolManager(null);
                BindTilePool(null);

                ResetComponent();
            }
        }

        private void BindTilePoolManager (TilePoolManager manager)
        {
            if (manager != null) {
                _layerControl.TextureCache.SourcePool = manager.TexturePool;
            }
            else {
                _layerControl.TextureCache.SourcePool = null;
            }
        }

        private void BindTilePool (TilePoolPresenter tilePool)
        {
            if (_tilePool != null) {
                _tilePool.LevelGeometry = null;
            }
            if (_layerControl.RootLayer != null) {
                _layerControl.RootLayer.Dispose();
                _layerControl.RootLayer = null;
            }

            _tilePool = tilePool;
            if (_tilePool != null) {
                _tilePool.LevelGeometry = _layerControl.LevelGeometry;

                _root = new GroupLayer() {
                    IsRendered = true,
                    Model = tilePool.RootLayer,
                };

                _layerControl.RootLayer = _root;
                _pointerController.Responder = tilePool.PointerEventResponder;
            }
            else {
                _root = null;
                _pointerController.Responder = null;
            }
        }

        private void SelectedTilePoolChangedHandler (object sender, EventArgs e)
        {
            BindTilePool(_controller.SelectedTilePool);
        }

        #region Event Dispatchers

        protected override void OnSizeChanged (EventArgs e)
        {
            base.OnSizeChanged(e);

            toolStrip1.CanOverflow = false;

            int width = toolStrip1.Width - _buttonAdd.Width - _buttonRemove.Width - _buttonProperties.Width - toolStripSeparator1.Width - toolStrip1.Padding.Horizontal - _buttonAdd.Margin.Horizontal - _buttonRemove.Margin.Horizontal - _buttonProperties.Margin.Horizontal - toolStripSeparator1.Margin.Horizontal - _poolComboBox.Margin.Horizontal - 1;
            _poolComboBox.Size = new Size(width, _poolComboBox.Height);
        }

        #endregion

        #region Event Handlers

        private void ShowPropertiesClickedHandler (object sender, EventArgs e)
        {
            //if (_controller != null)
            //    _controller.ActionShowTilePoolProperties();
        }

        private void SelectTilePoolHandler (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.ActionSelectTilePool((string)_poolComboBox.SelectedItem);
        }

        private void SyncTilePoolManagerHandler (object sender, EventArgs e)
        {
            BindTilePoolManager(_controller.TilePoolManager);
        }

        private void SyncTilePoolListHandler (object sender, EventArgs e)
        {
            _poolComboBox.Items.Clear();
            _poolComboBox.Text = "";

            foreach (TilePoolPresenter pool in _controller.TilePoolList) {
                _poolComboBox.Items.Add(pool.TilePool.Name);

                if (pool == _controller.SelectedTilePool) {
                    _poolComboBox.SelectedItem = pool.TilePool.Name;
                }
            }
        }

        private void SyncTilePoolControlHandler (object sender, EventArgs e)
        { }

        private void TileLayerResized (object sender, EventArgs e)
        { }

        #endregion

        private void ResetComponent ()
        {
            _poolComboBox.Items.Clear();
            _poolComboBox.Text = "";
        }
    }
}
