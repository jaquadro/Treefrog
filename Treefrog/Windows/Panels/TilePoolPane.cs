using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Treefrog.Framework.Model;
using Treefrog.Presentation;
using Treefrog.Presentation.Commands;
using Treefrog.Presentation.Controllers;
using Treefrog.Presentation.Tools;
using Treefrog.Render.Layers;
using Treefrog.Windows.Controllers;

namespace Treefrog.Windows
{
    public partial class TilePoolPane : UserControl
    {
        private class LocalPointerEventResponder : ChainedPointerEventResponder
        {
            private TilePoolPane _form;

            public LocalPointerEventResponder (TilePoolPane form, IPointerResponder parentResponder)
                : base(parentResponder)
            {
                _form = form;
            }

            public override void HandleStartPointerSequence (PointerEventInfo info)
            {
                base.HandleStartPointerSequence(info);

                if (info.Type == PointerEventType.Secondary && _form._tilePool.SelectedTile != null) {
                    Point position = _form._pointerController.UntranslatePosition(new Point((int)info.X, (int)info.Y));

                    _form._tileContextMenu.Show(_form._layerControl, position);
                }
            }
        }

        private TilePoolListPresenter _controller;
        private TilePoolPresenter _tilePool;
        private ControlPointerEventController _pointerController;
        private GroupLayer _root;

        private UICommandController _commandController;

        private ContextMenuStrip _tileContextMenu;

        public TilePoolPane ()
        {
            InitializeComponent();

            ResetComponent();

            // Load form elements

            _buttonRemove.Image = Properties.Resources.Minus;
            _buttonAdd.Image = Properties.Resources.Plus;
            _buttonProperties.Image = Properties.Resources.Tags;

            ToolStripMenuItem tilePropertiesItem = new ToolStripMenuItem("Tile Properties") {
                Image = Properties.Resources.Tags,
            };

            _tileContextMenu = new ContextMenuStrip();
            _tileContextMenu.Items.AddRange(new ToolStripItem[] {
                tilePropertiesItem,
            });

            _commandController = new UICommandController();
            _commandController.MapButtons(new Dictionary<CommandKey, ToolStripButton>() {
                { CommandKey.TilePoolDelete, _buttonRemove },
                { CommandKey.TilePoolProperties, _buttonProperties },
            });
            _commandController.MapMenuItems(new Dictionary<CommandKey, ToolStripMenuItem>() {
                { CommandKey.TilePoolImport, importNewToolStripMenuItem },
                { CommandKey.TilePoolImportMerge, importMergeToolStripMenuItem },
                { CommandKey.TileProperties, tilePropertiesItem },
            });

            _pointerController = new ControlPointerEventController(_layerControl, _layerControl);

            // Setup control

            _poolComboBox.ComboBox.DisplayMember = "Name";

            _layerControl.BackColor = System.Drawing.Color.SlateGray;
            _layerControl.WidthSynced = true;
            _layerControl.CanvasAlignment = CanvasAlignment.UpperLeft;

            // Wire events

            _poolComboBox.SelectedIndexChanged += SelectTilePoolHandler;
        }

        public void BindController (TilePoolListPresenter controller)
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

            RebuildPoolList();
        }

        private void BindTilePoolManager (ITilePoolManager manager)
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

                _root = new GroupLayer(tilePool.RootLayer);

                _layerControl.RootLayer = _root;
                //_pointerController.Responder = tilePool.PointerEventResponder;
                _pointerController.Responder = new LocalPointerEventResponder(this, tilePool.PointerEventResponder);
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
                _controller.ActionSelectTilePool(((TilePool)_poolComboBox.SelectedItem).Uid);
        }

        private void SyncTilePoolManagerHandler (object sender, EventArgs e)
        {
            BindTilePoolManager(_controller.TilePoolManager);
        }

        private void SyncTilePoolListHandler (object sender, EventArgs e)
        {
            RebuildPoolList();
        }

        private void RebuildPoolList ()
        {
            _poolComboBox.Items.Clear();
            _poolComboBox.Text = "";

            if (_controller == null)
                return;

            foreach (TilePoolPresenter pool in _controller.TilePoolList) {
                _poolComboBox.Items.Add(pool.TilePool);

                if (pool == _controller.SelectedTilePool) {
                    _poolComboBox.SelectedItem = pool.TilePool;
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
