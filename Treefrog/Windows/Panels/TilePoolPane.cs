using System;
using System.Drawing;
using System.Windows.Forms;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Amphibian.Drawing;
using Treefrog.Model;
using Treefrog.Presentation;
using Treefrog.Presentation.Layers;
using Treefrog.Windows.Controls;

namespace Treefrog.Windows
{
    using XColor = Microsoft.Xna.Framework.Color;
    using XRectangle = Microsoft.Xna.Framework.Rectangle;
    using Treefrog.Presentation.Commands;
    using Treefrog.Windows.Controllers;
    using System.Collections.Generic;
using Treefrog.Windows.Layers;
    using Treefrog.Presentation.Controllers;
    

    public partial class TilePoolPane : UserControl
    {
        private ITilePoolListPresenter _controller;
        private TilePoolPresenter _tilePool;
        private ControlPointerEventController _pointerController;
        private GroupLayer _root;

        private UICommandController _commandController;

        //private TileSetControlLayer _tileLayer;
        //private LayerGraphicsControl _layerControl;

        //private TileCoord _selectedTileCoord;

        //private Amphibian.Drawing.Brush _selectBrush;

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

                //_layerControl.Services.RemoveService<TilePoolTextureService>();
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

                /*if (_layerControl.Initialized) {
                    TilePoolTextureService poolService = new TilePoolTextureService(_controller.TilePoolManager, _layerControl.GraphicsDeviceService);
                    _layerControl.Services.AddService<TilePoolTextureService>(poolService);
                }
                else {
                    _layerControl.ControlInitialized += TileControlInitializedHandler;
                }*/
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

        /*private void TileControlInitializedHandler (object sender, EventArgs e)
        {
            _layerControl.ControlInitialized -= TileControlInitializedHandler;

            TilePoolTextureService poolService = new TilePoolTextureService(_controller.TilePoolManager, _layerControl.GraphicsDeviceService);
            _layerControl.Services.AddService<TilePoolTextureService>(poolService);
        }*/

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

        /*private void TileControlMouseDownHandler (object sender, TileMouseEventArgs e)
        {
            if (_controller != null) {
                _controller.ActionSelectTile(e.Tile);
            }
        }*/

        private void SyncTilePoolManagerHandler (object sender, EventArgs e)
        {
            BindTilePoolManager(_controller.TilePoolManager);

            /*if (_controller != null) {
                TilePoolTextureService poolService = _layerControl.Services.GetService<TilePoolTextureService>();
                if (poolService != null) {
                    poolService.Dispose();
                    _layerControl.Services.RemoveService<TilePoolTextureService>();
                }

                if (_layerControl.GraphicsDeviceService != null) {
                    poolService = new TilePoolTextureService(_controller.TilePoolManager, _layerControl.GraphicsDeviceService);
                    _layerControl.Services.AddService<TilePoolTextureService>(poolService);
                }
            }*/
        }

        private void SyncTilePoolListHandler (object sender, EventArgs e)
        {
            _poolComboBox.Items.Clear();
            _poolComboBox.Text = "";

            //_tileLayer.Layer = null;

            foreach (TilePoolPresenter pool in _controller.TilePoolList) {
                _poolComboBox.Items.Add(pool.TilePool.Name);

                if (pool == _controller.SelectedTilePool) {
                    _poolComboBox.SelectedItem = pool.TilePool.Name;

                    //_tileLayer.Layer = new TileSetLayer(pool.Name, pool);
                }
            }
        }

        private void SyncTilePoolControlHandler (object sender, EventArgs e)
        {
            //Tile selected = _controller.SelectedTile;
            //if (selected != null) {
            //    _selectedTileCoord = _tileLayer.TileToCoord(selected);
            //}
        }

        private void TileLayerResized (object sender, EventArgs e)
        {
            //if (_controller != null && _tileLayer.Layer != null) {
            //    Tile selected = _controller.SelectedTile;
            //    if (selected != null) {
            //        _selectedTileCoord = _tileLayer.TileToCoord(selected);
            //    }
            //}
        }

        #endregion

        /*private void DrawSelectedTileIndicators (object sender, DrawLayerEventArgs e)
        {
            if (_controller == null) {
                return;
            }

            Tile selectedTile = _controller.SelectedTile;
            if (_layerControl != null && _tileLayer != null && selectedTile != null) {
                if (_selectBrush == null) {
                    _selectBrush = _layerControl.CreateSolidColorBrush(new XColor(0.1f, 0.5f, 1f, 0.75f));
                }

                int x = _selectedTileCoord.X;
                int y = _selectedTileCoord.Y;

                Draw2D.FillRectangle(e.SpriteBatch, new XRectangle(
                    (int)(_selectedTileCoord.X * selectedTile.Width * _layerControl.Zoom),
                    (int)(_selectedTileCoord.Y * selectedTile.Height * _layerControl.Zoom),
                    (int)(selectedTile.Width * _layerControl.Zoom),
                    (int)(selectedTile.Height * _layerControl.Zoom)),
                    _selectBrush);
            }
        }*/

        private void ResetComponent ()
        {
            _poolComboBox.Items.Clear();
            _poolComboBox.Text = "";

            //if (_tileLayer != null) {
            //    _tileLayer.Layer = null;
            //}
        }
    }
}
