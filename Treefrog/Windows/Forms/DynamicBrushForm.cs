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
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Windows.Controllers;
using Treefrog.Presentation.Commands;
using System.Collections.ObjectModel;
using Treefrog.Presentation.Annotations;
using Treefrog.Windows.Layers;

namespace Treefrog.Windows.Forms
{
    public partial class DynamicBrushForm : Form
    {
        private class LocalPointerEventResponder : PointerEventResponder
        {
            private bool _erase;
            private DynamicBrushForm _form;

            public LocalPointerEventResponder (DynamicBrushForm form)
            {
                _form = form;
            }

            public override void HandlePointerPosition (PointerEventInfo info)
            {
                if (_form._tileController == null || _form._layer == null)
                    return;

                TileCoord location = TileLocation(info);
                if (!TileInRange(location))
                    return;

                if (_form._tileController.SelectedTile != null) {
                    if (_form._tileController.SelectedTile.Width != _form._layer.TileWidth ||
                        _form._tileController.SelectedTile.Height != _form._layer.TileHeight) {
                        MessageBox.Show("Selected tile not compatible with brush tile dimensions.", "Incompatible Tile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    _form._layer.ClearTile(location.X, location.Y);
                    if (!_erase)
                        _form._layer.AddTile(location.X, location.Y, _form._tileController.SelectedTile);
                }
            }

            public void SetDrawTool ()
            {
                _erase = false;
            }

            public void SetEraseTool ()
            {
                _erase = true;
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

        private class LocalLayerContext : ILayerContext
        {
            private DynamicBrushForm _form;
            private CommandHistory _history;
            private ObservableCollection<Annotation> _annots;

            public LocalLayerContext (DynamicBrushForm form)
            {
                _form = form;
                _history = new CommandHistory();
                _annots = new ObservableCollection<Annotation>();
            }

            public ILevelGeometry Geometry
            {
                get { return _form._layerControl.LevelGeometry; }
            }

            public Presentation.Commands.CommandHistory History
            {
                get { return _history; }
            }

            public System.Collections.ObjectModel.ObservableCollection<Presentation.Annotations.Annotation> Annotations
            {
                get { return _annots; }
            }

            public void SetPropertyProvider (IPropertyProvider provider)
            { }

            public void ActivatePropertyProvider (IPropertyProvider provider)
            { }

            public void ActivateContextMenu (CommandMenu menu, Treefrog.Framework.Imaging.Point location)
            { }
        }

        private class OverlayRenderCore : LocalRenderCore
        {
            private DynamicBrushForm _form;

            public OverlayRenderCore (DynamicBrushForm form)
            {
                _form = form;
            }

            public override void RenderContent (RenderLayer renderContext, SpriteBatch spriteBatch)
            {
                Dictionary<string, Texture2D> brushClassOverlays = _form._brushClassOverlays;
                DynamicTileBrush brush = _form._brush;

                if (!brushClassOverlays.ContainsKey(brush.BrushClass.ClassName)) {
                    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    brushClassOverlays.Add(brush.BrushClass.ClassName,
                        Texture2D.FromStream(spriteBatch.GraphicsDevice, assembly.GetManifestResourceStream("Treefrog.Icons.DynBrushOverlays." + brush.BrushClass.ClassName + ".png")));
                }

                Texture2D overlay = brushClassOverlays[brush.BrushClass.ClassName];

                int width = (int)(overlay.Width * renderContext.LevelGeometry.ZoomFactor * (brush.TileWidth / 16.0));
                int height = (int)(overlay.Height * renderContext.LevelGeometry.ZoomFactor * (brush.TileHeight / 16.0));

                Microsoft.Xna.Framework.Rectangle dstRect = new Microsoft.Xna.Framework.Rectangle(0, 0, width, height);
                spriteBatch.Draw(overlay, dstRect, new Microsoft.Xna.Framework.Color(1f, 1f, 1f, .5f));
            }
        }

        // TODO: Overlays need a separate registry if planning plugin support
        private Dictionary<string, Texture2D> _brushClassOverlays = new Dictionary<string, Texture2D>();

        private ITilePoolListPresenter _tileController;

        private DynamicTileBrush _brush;
        private PointerEventController _pointerController;
        private LocalPointerEventResponder _pointerResponder;
        private LocalLayerContext _layerContext;

        private GroupLayerPresenter _rootLayer;
        private MultiTileGridLayer _layer;

        private ValidationController _validateController;

        public DynamicBrushForm ()
        {
            InitializeForm();

            InitializeNewBrush();

            _prototypeList.SelectedIndexChanged += HandlePrototypeChanged;
            _tileSizeList.SelectedIndexChanged += HandleTileSizeChanged;

            _validateController.Validate();
        }

        public DynamicBrushForm (DynamicTileBrush brush)
        {
            InitializeForm();

            InitializeBrush(brush);

            _prototypeList.SelectedIndexChanged += HandlePrototypeChanged;
            _tileSizeList.SelectedIndexChanged += HandleTileSizeChanged;

            _prototypeList.Enabled = false;
            _tileSizeList.Enabled = false;

            _validateController.Validate();
        }

        private void InitializeForm ()
        {
            InitializeComponent();

            _validateController = new ValidationController() {
                OKButton = _buttonOk,
            };

            _validateController.RegisterControl(_nameField, ValidateName);

            _validateController.RegisterValidationFunc(ValidatePrototype);
            _validateController.RegisterValidationFunc(ValidateTileSize);

            _layerContext = new LocalLayerContext(this);

            InitializePrototypeList();
            InitializeLayers();
        }

        protected override void Dispose (bool disposing)
        {
            if (disposing) {
                if (components != null)
                    components.Dispose();

                _tilePanel.BindController(null);
                _validateController.Dispose();
            }
            base.Dispose(disposing);
        }

        public void BindTileController (ITilePoolListPresenter controller)
        {
            if (_tileController != null) {
                _tileController.SyncTilePoolList -= SyncTilePoolListHandler;
            }

            _tileController = controller;
            _tilePanel.BindController(controller);

            if (_tileController != null) {
                _tileController.SyncTilePoolList += SyncTilePoolListHandler;
                _layerControl.TextureCache.SourcePool = _tileController.TilePoolManager.TexturePool;
            }
            else {
                _layerControl.TextureCache.SourcePool = null;
            }

            InitializeTileSizeList();
        }

        private void SyncTilePoolListHandler (object sender, EventArgs e)
        {
            InitializeTileSizeList();

            if (_layer != null) {
                List<LocatedTile> removeQueue = new List<LocatedTile>();
                foreach (LocatedTile tile in _layer.Tiles) {
                    if (_tileController.TilePoolManager.PoolFromTileId(tile.Tile.Uid) == null)
                        removeQueue.Add(tile);
                }

                foreach (LocatedTile tile in removeQueue) {
                    _layer.RemoveTile(tile.X, tile.Y, tile.Tile);
                    RemoveTileFromBrush(_brush, tile.Tile);
                }
            }
        }

        private void RemoveTileFromBrush (DynamicTileBrush brush, Tile tile)
        {
            if (brush != null) {
                for (int i = 0; i < _brush.BrushClass.SlotCount; i++) {
                    if (_brush.GetTile(i) == tile)
                        _brush.SetTile(i, null);
                }
            }
        }

        private List<string> _reservedNames = new List<string>();

        public List<string> ReservedNames
        {
            get { return _reservedNames; }
            set { _reservedNames = value; }
        }

        public DynamicTileBrush Brush
        {
            get { return _brush; }
        }

        private void InitializeLayers ()
        {
            _layerControl.Zoom = 2f;

            _pointerController = new PointerEventController(_layerControl);
            _layerControl.MouseClick += _pointerController.TargetMouseClick;

            _pointerResponder = new LocalPointerEventResponder(this);
            _pointerController.Responder = _pointerResponder;

            _rootLayer = new GroupLayerPresenter();
            _layerControl.RootLayer = new GroupLayer(_rootLayer);
        }

        private void InitializeBrush (DynamicTileBrush brush)
        {
            _layerControl.ReferenceWidth = brush.BrushClass.TemplateWidth * brush.TileWidth + 1;
            _layerControl.ReferenceHeight = brush.BrushClass.TemplateHeight * brush.TileHeight + 1;

            _layer = new MultiTileGridLayer("Default", brush.TileWidth, brush.TileHeight, brush.BrushClass.TemplateWidth, brush.BrushClass.TemplateHeight);
            for (int i = 0; i < brush.BrushClass.SlotCount; i++) {
                LocatedTile tile = brush.GetLocatedTile(i);
                if (tile.Tile != null)
                    _layer.AddTile(tile.X, tile.Y, tile.Tile);
            }

            _rootLayer.Layers.Clear();
            _rootLayer.Layers.Add(new TileGridLayerPresenter(_layerContext, _layer));
            _rootLayer.Layers.Add(new LocalRenderLayerPresenter(new OverlayRenderCore(this)));
            _rootLayer.Layers.Add(new GridLayerPresenter() {
                GridSpacingX = brush.TileWidth,
                GridSpacingY = brush.TileHeight,
            });

            _nameField.Text = brush.Name;

            _brush = brush;

            SelectCurrentPrototype();
            SelectCurrentTileSize();
        }

        private void InitializePrototypeList ()
        {
            _prototypeList.Items.Clear();
            foreach (var item in Project.DynamicBrushClassRegistry.RegisteredObjects)
                _prototypeList.Items.Add(item.ClassName);

            SelectCurrentPrototype();
        }

        private void SelectCurrentPrototype ()
        {
            if (_brush == null) {
                if (_prototypeList.Items.Count > 0)
                    _prototypeList.SelectedIndex = 0;

                _validateController.Validate();
                return;
            }

            for (int i = 0; i < _prototypeList.Items.Count; i++) {
                string item = _prototypeList.Items[i] as string;
                if (item == _brush.BrushClass.ClassName) {
                    _prototypeList.SelectedIndex = i;
                    break;
                }
            }

            _validateController.Validate();
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
                foreach (TilePoolPresenter pool in _tileController.TilePoolList) {
                    if (_availableSizes.Exists(sz => { return sz.Width == pool.TilePool.TileWidth && sz.Height == pool.TilePool.TileHeight; }))
                        continue;

                    TileSize size = new TileSize(pool.TilePool.TileWidth, pool.TilePool.TileHeight);
                    _availableSizes.Add(size);
                    _tileSizeList.Items.Add(size);
                }
            }

            if (_brush != null) {
                if (!_availableSizes.Exists(sz => { return sz.Width == _brush.TileWidth && sz.Height == _brush.TileHeight; })) {
                    TileSize brushSize = new TileSize(_brush.TileWidth, _brush.TileHeight);
                    _availableSizes.Add(brushSize);
                    _tileSizeList.Items.Add(brushSize);
                }
            }

            SelectCurrentTileSize();
        }

        private void SelectCurrentTileSize ()
        {
            if (_brush == null) {
                if (_tileSizeList.Items.Count > 0)
                    _tileSizeList.SelectedIndex = 0;

                _validateController.Validate();
                return;
            }

            for (int i = 0; i < _tileSizeList.Items.Count; i++) {
                TileSize item = _tileSizeList.Items[i] as TileSize;
                if (item.Width == _brush.TileWidth && item.Height == _brush.TileHeight) {
                    _tileSizeList.SelectedIndex = i;
                    break;
                }
            }

            _validateController.Validate();
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

            DynamicTileBrushClass brushClass = Project.DynamicBrushClassRegistry.Lookup(prototype);
            if (brushClass == null)
                return;

            string name = "";
            if (_brush != null)
                name = _brush.Name;

            InitializeBrush(new DynamicTileBrush(name, size.Width, size.Height, brushClass));
        }

        private void _buttonOk_Click (object sender, EventArgs e)
        {
            if (!_validateController.ValidateForm())
                return;

            _brush.SetName(_nameField.Text);

            for (int i = 0; i < _brush.BrushClass.SlotCount; i++)
                _brush.SetTile(i, null);

            foreach (LocatedTile tile in _layer.Tiles) {
                _brush.SetTile(tile.Location.X, tile.Location.Y, tile.Tile);
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void _buttonCancel_Click (object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void _toggleDraw_Click (object sender, EventArgs e)
        {
            _toggleDraw.Checked = true;
            _toggleErase.Checked = false;
            _pointerResponder.SetDrawTool();
        }

        private void _toggleErase_Click (object sender, EventArgs e)
        {
            _toggleDraw.Checked = false;
            _toggleErase.Checked = true;
            _pointerResponder.SetEraseTool();
        }

        private string ValidateName ()
        {
            string txt = _nameField.Text.Trim();

            if (String.IsNullOrEmpty(txt))
                return "Name field must be non-empty.";
            else if (_reservedNames.Contains(txt))
                return "A resource with this name already exists.";
            else
                return null;
        }

        private string ValidatePrototype ()
        {
            if (_prototypeList.SelectedItem == null)
                return "A brush prototype must be selected.";
            else
                return null;
        }

        private string ValidateTileSize ()
        {
            if (_tileSizeList.SelectedItem == null)
                return "A tile size must be selected.";
            else
                return null;
        }
    }
}
