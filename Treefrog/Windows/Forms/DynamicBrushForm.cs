using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Treefrog.Presentation;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Layers;
using Treefrog.Model;
using Treefrog.Windows.Controls;
using Treefrog.Presentation.Tools;
using Treefrog.Presentation.Controllers;
using Treefrog.Framework;
using Treefrog.Framework.Model.Support;

namespace Treefrog.Windows.Forms
{
    public partial class DynamicBrushForm : Form
    {
        private class LocalPointerEventResponder : PointerEventResponder
        {
            private DynamicBrushForm _form;

            public LocalPointerEventResponder (DynamicBrushForm form)
            {
                _form = form;
            }

            public override void HandlePointerPosition (PointerEventInfo info)
            {
                TileCoord location = TileLocation(info);
                if (!TileInRange(location))
                    return;

                if (_form._tileController.SelectedTile != null) {
                    _form._layer.ClearTile(location.X, location.Y);
                    _form._layer.AddTile(location.X, location.Y, _form._tileController.SelectedTile);
                }
            }

            private TileCoord TileLocation (PointerEventInfo info)
            {
                return new TileCoord((int)Math.Floor(info.X / _form._layer.TileWidth), (int)Math.Floor(info.Y / _form._layer.TileHeight));
            }

            private bool TileInRange (TileCoord location)
            {
                if (location.X < 0 || location.X >= _form._layer.TilesWide)
                    return false;
                if (location.Y < 0 || location.Y >= _form._layer.TilesHigh)
                    return false;

                return true;
            }
        }

        private ITilePoolListPresenter _tileController;
        private ITileBrushManagerPresenter _brushController;

        private DynamicBrush _brush;
        private PointerEventController _pointerController;
        private LocalPointerEventResponder _pointerResponder;

        private MultiTileGridLayer _layer;
        private MultiTileControlLayer _clayer;

        public DynamicBrushForm (DynamicBrush brush)
        {
            InitializeComponent();
            InitializePrototypeList();

            _layerControl.ControlInitialized += LayerControlInitialized;

            _pointerController = new PointerEventController(_layerControl);
            _layerControl.MouseClick += _pointerController.TargetMouseClick;

            _pointerResponder = new LocalPointerEventResponder(this);
            _pointerController.Responder = _pointerResponder;

            InitializeBrush(brush);

            _prototypeList.SelectedIndexChanged += HandlePrototypeChanged;
            _tileSizeList.SelectedIndexChanged += HandleTileSizeChanged;
        }

        private void LayerControlInitialized (object sender, EventArgs e)
        {
            TilePoolTextureService poolService = new TilePoolTextureService(_tileController.TilePoolManager, _layerControl.GraphicsDeviceService);
            _layerControl.Services.AddService<TilePoolTextureService>(poolService);
        }

        public void BindTileController (ITilePoolListPresenter controller)
        {
            _tileController = controller;
            _tilePanel.BindController(controller);

            InitializeTileSizeList();
        }

        private void InitializeBrush (DynamicBrush brush)
        {
            _layer = new MultiTileGridLayer("Default", brush.TileWidth, brush.TileHeight, brush.BrushClass.TemplateWidth, brush.BrushClass.TemplateHeight);
            for (int i = 0; i < brush.BrushClass.TileCount; i++) {
                LocatedTile tile = brush.BrushClass.GetLocatedTile(i);
                if (tile.Tile != null)
                    _layer.AddTile(tile.X, tile.Y, tile.Tile);
            }

            if (_clayer != null)
                _layerControl.RemoveLayer(_clayer);

            _clayer = new MultiTileControlLayer(_layerControl, _layer);
            _clayer.ShouldDrawContent = LayerCondition.Always;
            _clayer.ShouldDrawGrid = LayerCondition.Always;
            _clayer.ShouldRespondToInput = LayerCondition.Always;

            _nameField.Text = brush.Name;

            _brush = brush;

            SelectCurrentPrototype();
            SelectCurrentTileSize();
        }

        private void InitializePrototypeList ()
        {
            _prototypeList.Items.Clear();
            _prototypeList.Items.AddRange(new string[] {
                "Basic",
                "Extended",
            });

            SelectCurrentPrototype();
        }

        private void SelectCurrentPrototype ()
        {
            if (_brush == null)
                return;

            for (int i = 0; i < _prototypeList.Items.Count; i++) {
                string item = _prototypeList.Items[i] as string;
                if (item == _brush.BrushClass.ClassName) {
                    _prototypeList.SelectedIndex = i;
                    break;
                }
            }
        }

        private class TileSize
        {
            public int Height;
            public int Width;

            public TileSize (int width, int height)
            {
                Width = width;
                Height = height;
            }

            public override string ToString ()
            {
                return Width + " x " + Height;
            }
        }

        private List<TileSize> _availableSizes = new List<TileSize>();

        private void InitializeTileSizeList ()
        {
            _tileSizeList.Items.Clear();
            _availableSizes.Clear();

            if (_tileController != null) {
                foreach (TilePool pool in _tileController.TilePoolList) {
                    if (_availableSizes.Exists(sz => { return sz.Width == pool.TileWidth && sz.Height == pool.TileHeight; }))
                        continue;

                    TileSize size = new TileSize(pool.TileWidth, pool.TileHeight);
                    _availableSizes.Add(size);
                    _tileSizeList.Items.Add(size);
                }
            }

            SelectCurrentTileSize();
        }

        private void SelectCurrentTileSize ()
        {
            if (_brush == null)
                return;

            for (int i = 0; i < _tileSizeList.Items.Count; i++) {
                TileSize item = _tileSizeList.Items[i] as TileSize;
                if (item.Width == _brush.BrushClass.TileWidth && item.Height == _brush.BrushClass.TileHeight) {
                    _tileSizeList.SelectedIndex = i;
                    break;
                }
            }
        }

        private void HandlePrototypeChanged (object sender, EventArgs e)
        {
            string prototype = _prototypeList.SelectedItem as string;
            if (prototype == null)
                return;

            if (_brush == null || _brush.BrushClass.ClassName != prototype)
                InitializeNewBrush();
        }

        private void HandleTileSizeChanged (object sender, EventArgs e)
        {
            TileSize size = _tileSizeList.SelectedItem as TileSize;
            if (size == null)
                return;

            if (_brush == null || _brush.TileWidth != size.Width || _brush.TileHeight != size.Height)
                InitializeNewBrush();
        }

        private void InitializeNewBrush ()
        {
            string prototype = _prototypeList.SelectedItem as string;
            TileSize size = _tileSizeList.SelectedItem as TileSize;

            if (prototype == null || size == null)
                return;

            DynamicBrushClass brushClass = null;
            switch (prototype) {
                case "Basic":
                    brushClass = new BasicDynamicBrushClass(size.Width, size.Height);
                    break;
                case "Extended":
                    brushClass = new ExtendedDynamicBrushClass(size.Width, size.Height);
                    break;
                default:
                    return;
            }

            InitializeBrush(new DynamicBrush(_brush.Name, size.Width, size.Height, brushClass));
        }
    }
}
