using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel.Design;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Amphibian.Drawing;
using System.ComponentModel;
using Microsoft.Xna.Framework.Input;
using Editor.Model;

using XnaKeys = Microsoft.Xna.Framework.Input.Keys;
using Editor.Controls;

namespace Editor
{
    public class TileMouseEventArgs : MouseEventArgs
    {
        public TileCoord TileLocation { get; private set; }
        public Tile Tile { get; private set; }

        public TileMouseEventArgs (MouseEventArgs e, TileCoord coord)
            : base(e.Button, e.Clicks, e.X, e.Y, e.Delta)
        {
            TileLocation = coord;
        }

        public TileMouseEventArgs (MouseEventArgs e, TileCoord coord, Tile tile)
            : this(e, coord)
        {
            Tile = tile;
        }
    }

    public class TileEventArgs : EventArgs
    {
        private TileCoord _coord;
        private Tile _tile;

        public TileEventArgs (TileCoord coord)
        {
            _coord = coord;
        }

        public TileEventArgs (TileCoord coord, Tile tile)
            : this(coord)
        {
            _tile = tile;
        }

        public TileCoord Location
        {
            get { return _coord; }
        }

        public Tile Tile
        {
            get { return _tile; }
        }
    }

    public class TileSelectEventArgs : EventArgs
    {
        private Dictionary<TileCoord, Tile> _tiles;

        public TileSelectEventArgs (Dictionary<TileCoord, Tile> tiles)
        {
            _tiles = new Dictionary<TileCoord, Tile>();

            // Normalize
            int minx = int.MaxValue;
            int miny = int.MaxValue;

            foreach (TileCoord tc in tiles.Keys) {
                minx = Math.Min(minx, tc.X);
                miny = Math.Min(miny, tc.Y);
            }

            foreach (KeyValuePair<TileCoord, Tile> kv in tiles) {
                _tiles.Add(new TileCoord(kv.Key.X - minx, kv.Key.Y - miny), kv.Value);
            }
        }

        public IEnumerable<KeyValuePair<TileCoord, Tile>> Tiles
        {
            get
            {
                foreach (KeyValuePair<TileCoord, Tile> kv in _tiles) {
                    yield return kv;
                }
            }
        }
    }

    public enum TileControlMode
    {
        Click,
        Select
    }

    public abstract class TileControl : GraphicsDeviceControl, IScrollableControl
    {
        protected SpriteBatch _spriteBatch;
        private IServiceContainer _services;
        private ContentManager _content;

        private Effect _effect;
        private EffectParameter _effectTransColor;

        private float _zoom = 1f;

        private ITileSource _tileSource;

        private TileControlMode _mode;

        private bool _initialized = false;

        #region Events

        public event EventHandler<TileMouseEventArgs> MouseTileClick;
        public event EventHandler<TileMouseEventArgs> MouseTileDown;
        public event EventHandler<TileMouseEventArgs> MouseTileMove;
        public event EventHandler<TileMouseEventArgs> MouseTileUp;

        public event EventHandler<TileEventArgs> TileSelected;
        public event EventHandler<TileSelectEventArgs> TileMultiSelected;

        protected virtual void OnTileSelected (TileEventArgs e)
        {
            if (TileSelected != null) {
                TileSelected(this, e);
            }
        }

        protected virtual void OnTileMultiSelected (TileSelectEventArgs e)
        {
            if (TileMultiSelected != null) {
                TileMultiSelected(this, e);
            }
        }

        #endregion

        public TileControl ()
            : base()
        {
            
        }

        [Browsable(false)]
        public virtual ITileSource TileSource
        {
            get { return _tileSource; }
            set 
            { 
                _tileSource = value;

                if (_initialized) {
                    BuildTileBrush();
                }

                if (_tileSource != null) {
                    OnVirtualSizeChanged();

                    _scrollSmChangeH = _tileSource.TileWidth / 2;
                    _scrollSmChangeV = _tileSource.TileHeight / 2;
                    _scrollLgChangeH = _tileSource.TileWidth * 4;
                    _scrollLgChangeV = _tileSource.TileHeight * 4;
                    OnScrollPropertyChanged();
                }
            }
        }

        public float Zoom
        {
            get { return _zoom; }
            set 
            { 
                _zoom = value;
                if (_initialized) {
                    BuildTileBrush();
                    OnVirtualSizeChanged();
                    CheckScrollValue();
                }
            }
        }

        public TileControlMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        protected override void Initialize ()
        {
            _services = new System.ComponentModel.Design.ServiceContainer();
            _services.AddService(typeof(IGraphicsDeviceService), GraphicsDeviceService);
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _content = new ContentManager(_services);
            _content.RootDirectory = "EditorContent";

            _effect = _content.Load<Effect>("TransColor");
            _effectTransColor = _effect.Parameters["transColor"];
            _effectTransColor.SetValue(Color.White.ToVector4());

            Application.Idle += delegate { Invalidate(); };

            if (_tileSource != null) {
                BuildTileBrush();
            }

            _selectingBrush = new SolidColorBrush(_spriteBatch, new Color(.2f, .75f, 1f, .3f));
            _selectedBrush = new SolidColorBrush(_spriteBatch, new Color(.2f, .75f, 1f, .6f));

            _initialized = true;
        }

        private TileCoord MouseToTileCoords (Point mouse)
        {
            int vHeight = VirtualSize.Height;
            int vWidth = VirtualSize.Width;

            Vector2 offset = new Vector2(-_scrollH * _zoom, -_scrollV * _zoom);
            if (this.Width > vWidth * _zoom) {
                offset.X = (this.Width - vWidth * _zoom) / 2;
            }
            if (this.Height > vHeight * _zoom) {
                offset.Y = (this.Height - vHeight * _zoom) / 2;
            }

            int tx = (int)Math.Floor(((float)mouse.X - offset.X) / (_tileSource.TileWidth * _zoom));
            int ty = (int)Math.Floor(((float)mouse.Y - offset.Y) / (_tileSource.TileHeight * _zoom));

            return new TileCoord(tx, ty);
        }

        private Vector2 MouseEdgeScroll (Point mouse)
        {
            const int thresh = 16;

            Vector2 scroll = new Vector2();
            if (mouse.X < thresh) {
                scroll.X = -1;
            }
            if (mouse.X > Width - thresh) {
                scroll.X = 1;
            }

            return scroll;
        }

        protected override void OnMouseClick (MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (_tileSource == null) {
                return;
            }

            if (_mode == TileControlMode.Click) {
                OnMouseTileClick(new TileMouseEventArgs(e, MouseToTileCoords(new Point(e.X, e.Y))));
            }
        }

        protected override void OnMouseDown (MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (_tileSource == null) {
                return;
            }

            OnMouseTileDown(new TileMouseEventArgs(e, MouseToTileCoords(new Point(e.X, e.Y))));

            if (CheckAutoScrollCondition()) {
                CheckStartAutoScroll(new Point(e.X, e.Y));
            }
        }

        protected override void OnMouseMove (MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_tileSource == null) {
                return;
            }

            OnMouseTileMove(new TileMouseEventArgs(e, MouseToTileCoords(new Point(e.X, e.Y))));

            if (CheckAutoScrollCondition()) {
                CheckStopAutoScroll(new Point(e.X, e.Y));
                CheckStartAutoScroll(new Point(e.X, e.Y));
            }
        }

        protected override void OnMouseUp (MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_tileSource == null) {
                return;
            }

            OnMouseTileUp(new TileMouseEventArgs(e, MouseToTileCoords(new Point(e.X, e.Y))));

            CheckStopAutoScroll(new Point(e.X, e.Y));
        }

        protected override void OnMouseLeave (EventArgs e)
        {
            base.OnMouseLeave(e);
            _selecting = false;
        }

        protected virtual void OnMouseTileClick (TileMouseEventArgs e)
        {
            if (MouseTileClick != null) {
                MouseTileClick(this, e);
            }
        }

        protected virtual void OnMouseTileDown (TileMouseEventArgs e)
        {
            if (MouseTileDown != null) {
                MouseTileDown(this, e);
            }

            CheckBeginSelection(e);
        }

        protected virtual void OnMouseTileMove (TileMouseEventArgs e)
        {
            if (MouseTileMove != null) {
                MouseTileMove(this, e);
            }

            CheckUpdateSelection(e);
        }

        protected virtual void OnMouseTileUp (TileMouseEventArgs e)
        {
            if (MouseTileUp != null) {
                MouseTileUp(this, e);
            }

            CheckEndSelection(e);
        }

        public event EventHandler<DrawEventArgs> DrawExtra;

        protected virtual void OnDrawExtra (DrawEventArgs e)
        {
            if (DrawExtra != null) {
                DrawExtra(this, e);
            }
        }

        protected virtual bool CheckAutoScrollCondition ()
        {
            return IsSelecting;
        }

        // ------------------

        #region Drawing

        #region Fields

        private Color _backColor = Color.Gray;
        private Color _gridColor = new Color(0, 0, 0, 0.5f);

        private Texture2D _tileGridBrush;
        private Texture2D _tileGridBrushRight;
        private Texture2D _tileGridBrushBottom;
        private Brush _tileGridBorderBrush;

        private bool _showGrid = true;

        #endregion

        #region Properties

        [DefaultValue(true)]
        [Description("Indicates whether a tile grid will be drawn on this control")]
        public bool ShowGrid
        {
            get { return _showGrid; }
            set { _showGrid = value; }
        }

        #endregion

        #region Event Handlers

        protected override void OnBackColorChanged (EventArgs e)
        {
            base.OnBackColorChanged(e);

            _backColor = new Color(BackColor.R / 255f, BackColor.G / 255f, BackColor.B / 255f, BackColor.A / 255f);
        }

        protected override void OnForeColorChanged (EventArgs e)
        {
            base.OnForeColorChanged(e);

            _gridColor = new Color(ForeColor.R / 255f, ForeColor.G / 255f, ForeColor.B / 255f, ForeColor.A / 510f);

            if (_initialized) {
                BuildTileBrush();
            }
        }

        protected override void OnPaint (PaintEventArgs e)
        {
            if (_tileSource != null) {
                base.OnPaint(e);
            }
            else {
                PaintUsingSystemDrawing(e.Graphics, Text + "\n\n" + GetType());
            }
        }

        #endregion

        #region Resource Management

        protected virtual void BuildTileBrush ()
        {
            if (_tileSource == null) {
                _tileGridBrush = null;
                _tileGridBrushRight = null;
                _tileGridBrushBottom = null;
                _tileGridBorderBrush = null;
                return;
            }

            int x = (int)(_tileSource.TileWidth * _zoom);
            int y = (int)(_tileSource.TileHeight * _zoom);

            Color[] colors = new Color[x * y];
            Color[] right = new Color[x * y];
            Color[] bottom = new Color[x * y];
            for (int i = 0; i < x; i++) {
                if (i % 4 != 2) {
                    colors[i] = _gridColor;
                    bottom[i] = _gridColor;
                }
            }
            for (int i = 0; i < y; i++) {
                if (i % 4 != 2) {
                    colors[i * x] = _gridColor;
                    right[i * x] = _gridColor;
                }
            }

            _tileGridBrush = new Texture2D(_spriteBatch.GraphicsDevice, x, y, false, SurfaceFormat.Color);
            _tileGridBrush.SetData(colors);

            _tileGridBrushRight = new Texture2D(_spriteBatch.GraphicsDevice, x, y, false, SurfaceFormat.Color);
            _tileGridBrushRight.SetData(right);

            _tileGridBrushBottom = new Texture2D(_spriteBatch.GraphicsDevice, x, y, false, SurfaceFormat.Color);
            _tileGridBrushBottom.SetData(bottom);

            _tileGridBorderBrush = new LineStippleBrush(_spriteBatch, _gridColor, "-- --- --- --- -", 1);
        }

        #endregion

        protected virtual Vector2 DrawOffset ()
        {
            Vector2 offset = new Vector2(0, -_scrollV * _zoom);

            int vHeight = VirtualSize.Height;
            int vWidth = VirtualSize.Width;

            if (_tileSource is ITileSource2D) {
                offset.X = -_scrollH * _zoom;
                if (Width > vWidth * _zoom) {
                    offset.X = (Width - vWidth * _zoom) / 2;
                }
            }

            if (Height > vHeight * _zoom) {
                offset.Y = (Height - vHeight * _zoom) / 2;
            }

            offset.X = (int)offset.X;
            offset.Y = (int)offset.Y;

            return offset;
        }

        protected override void Draw ()
        {
            GraphicsDevice.Clear(_backColor);

            Vector2 offset = DrawOffset();

            _spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, _effect, Matrix.CreateTranslation(offset.X, offset.Y, 0));
            _spriteBatch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            Rectangle region = new Rectangle(
                _scrollH / _tileSource.TileWidth,
                _scrollV / _tileSource.TileHeight,
                (Width + (int)((_scrollH % _tileSource.TileWidth + _tileSource.TileWidth) * _zoom - 1)) / (int)(_tileSource.TileWidth * _zoom),
                (Height + (int)((_scrollV % _tileSource.TileHeight + _tileSource.TileHeight) * _zoom - 1)) / (int)(_tileSource.TileHeight * _zoom)
                );

            DrawTiles(offset, region);

            _spriteBatch.End();

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));
            _spriteBatch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            //DrawSelectBox();
            OnDrawExtra(new DrawEventArgs(_spriteBatch));

            DrawTileSelection();

            if (_showGrid) {
                DrawGrid();
            }

            _spriteBatch.End();
        }

        protected virtual void DrawTiles (Vector2 offset, Rectangle region)
        {

        }

        protected virtual void DrawGrid ()
        {
            int vHeight = VirtualSize.Height;
            int vWidth = VirtualSize.Width;

            int offX = _scrollH % _tileSource.TileWidth;
            int offY = _scrollV % _tileSource.TileHeight;
            int startX = (int)(_scrollH * _zoom) - (int)(offX * _zoom);
            int startY = (int)(_scrollV * _zoom) - (int)(offY * _zoom);
            int tileX = (int)Math.Ceiling((Math.Min(Width, vWidth * _zoom) + (offX * _zoom)) / (float)(_tileSource.TileWidth * _zoom));
            int tileY = (int)Math.Ceiling((Math.Min(Height, vHeight * _zoom) + (offY * _zoom)) / (float)(_tileSource.TileHeight * _zoom));

            Func<int, int, bool> inside = (int x, int y) => { 
                return (x >= 0) && (x < tileX) &&
                    (y >= 0) && (y < tileY) &&
                    (y * tileX + x < _tileSource.Count); 
            };

            for (int x = 0; x <= tileX; x++) {
                for (int y = 0; y <= tileY; y++) {
                    bool iCenter = inside(x, y);
                    bool iLeft = inside(x - 1, y);
                    bool iUpper = inside(x, y - 1);
                    bool iUpperLeft = inside(x - 1, y - 1);

                    Vector2 pos = new Vector2(startX + x * _tileSource.TileWidth * _zoom, startY + y * _tileSource.TileHeight * _zoom);
                    if (iCenter || (iLeft && iUpper)) {
                        _spriteBatch.Draw(_tileGridBrush, pos, Color.White);
                    }
                    else if (iLeft) {
                        _spriteBatch.Draw(_tileGridBrushRight, pos, Color.White);
                    }
                    else if (iUpper) {
                        _spriteBatch.Draw(_tileGridBrushBottom, pos, Color.White);
                    }
                    if (iUpperLeft) {
                        continue;
                    }
                }
            }

            /*if (_tileSource.Count > 0) {
                Primitives2D.DrawLine(_spriteBatch, new Vector2(vWidth * _zoom + 1, startY), (tileY - 1) * _tileSource.TileHeight * _zoom + 1, MathHelper.ToRadians(90), _tileGridBorderBrush, 1);
                Primitives2D.DrawLine(_spriteBatch, new Vector2(startX, vHeight * _zoom), tileX * _tileSource.TileWidth * _zoom + 1, 0, _tileGridBorderBrush, 1);
            }*/

            /*int roomPixelsWide = _map.RoomWidth * _map.TileWidth;
            int roomPixelsHigh = _map.RoomHeight * _map.TileHeight;

            int lineX = ((startX + roomPixelsWide - 1) / roomPixelsWide) * roomPixelsWide;
            int lineY = ((startY + roomPixelsHigh - 1) / roomPixelsHigh) * roomPixelsHigh;

            for (int x = lineX; x < lineX + _canvas.Width; x += roomPixelsWide) {
                Primitives2D.DrawLine(_canvas.SpriteBatch, new Vector2(x + 1, startY), new Vector2(x + 1, startY + tileY * _map.TileHeight), Color.Black);
            }
            for (int y = lineY; y < lineY + _canvas.Height; y += roomPixelsHigh) {
                Primitives2D.DrawLine(_canvas.SpriteBatch, new Vector2(startX, y), new Vector2(startX + tileX * _map.TileWidth, y), Color.Black);
            }*/
        }

        #endregion

        #region Selections

        #region Fields

        private Brush _selectingBrush;
        private Brush _selectedBrush;

        private Rectangle _selectBox;

        private bool _selecting;
        private bool _holdSelection;
        private Vector2 _autoScroll;

        private bool _canSelect = true;
        private bool _canSelectRange = true;
        private bool _canSelectDisjoint = true;

        #endregion

        #region Properties

        [DefaultValue(true)]
        [Description("Indicates whether a selectbox can be used interactively on this control")]
        public bool CanSelectTiles
        {
            get { return _canSelect; }
            set { _canSelect = value; }
        }

        [DefaultValue(true)]
        [Description("Indicates whether a rectangular range of tiles can be selected")]
        public bool CanSelectRange
        {
            get { return _canSelectRange; }
            set { _canSelectRange = value; }
        }

        [DefaultValue(true)]
        [Description("Indicates whether irregular or disjoint sets of tiles can be selected")]
        public bool CanSelectDisjoint
        {
            get { return _canSelectDisjoint; }
            set { _canSelectDisjoint = value; }
        }

        [Browsable(false)]
        public Brush SelectBoxBrush
        {
            get { return _selectingBrush; }
            set { _selectingBrush = value; }
        }

        [Browsable(false)]
        public Brush TileSelectionBrush
        {
            get { return _selectedBrush; }
            set { _selectedBrush = value; }
        }

        [Browsable(false)]
        protected bool IsSelecting
        {
            get { return _selecting; }
        }

        [Browsable(false)]
        protected bool HoldSelection
        {
            get { return _holdSelection; }
            set { _holdSelection = value; }
        }

        #endregion

        #region Abstract Selection Interface

        protected abstract IEnumerable<TileCoord> SelectedTileLocations { get; }
        protected abstract IEnumerable<KeyValuePair<TileCoord, Tile>> SelectedTiles { get; }

        protected abstract void AddSelectedTile (TileCoord coord);
        protected abstract void ClearSelectedTiles ();

        #endregion

        #region Selection Manipulation

        private void CheckBeginSelection (TileMouseEventArgs e)
        {
            if (_canSelect && _mode == TileControlMode.Select) {
                if (!_selecting) {
                    BeginSelection(e);

                    if (!_canSelectRange) {
                        EndSelection(e);

                        if (_canSelectDisjoint) {
                            Dictionary<TileCoord, Tile> sel = new Dictionary<TileCoord, Tile>();
                            foreach (KeyValuePair<TileCoord, Tile> kvp in SelectedTiles) {
                                sel.Add(kvp.Key, kvp.Value);
                            }

                            OnTileMultiSelected(new TileSelectEventArgs(sel));
                        }
                        else {
                            foreach (KeyValuePair<TileCoord, Tile> kvp in SelectedTiles) {
                                OnTileSelected(new TileEventArgs(kvp.Key, kvp.Value));
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void CheckUpdateSelection (TileMouseEventArgs e)
        {
            if (_canSelect && _mode == TileControlMode.Select) {
                if (_selecting) {
                    UpdateSelection(e);
                }
            }
        }

        private void CheckEndSelection (TileMouseEventArgs e)
        {
            if (_canSelect && _mode == TileControlMode.Select) {
                if (_selecting) {
                    EndSelection(e);
                }

                if (_canSelectRange || _canSelectDisjoint) {
                    Dictionary<TileCoord, Tile> sel = new Dictionary<TileCoord, Tile>();
                    foreach (KeyValuePair<TileCoord, Tile> kvp in SelectedTiles) {
                        sel.Add(kvp.Key, kvp.Value);
                    }

                    OnTileMultiSelected(new TileSelectEventArgs(sel));
                }
            }
        }

        protected virtual void BeginSelection (TileMouseEventArgs e)
        {
            if (!_canSelect || _holdSelection) {
                return;
            }

            KeyboardState keyboard = Keyboard.GetState();
            if (!_canSelectDisjoint || (!keyboard.IsKeyDown(XnaKeys.LeftControl) && !keyboard.IsKeyDown(XnaKeys.RightControl))) {
                ClearSelectedTiles();
            }

            _selectBox = new Rectangle(e.TileLocation.X, e.TileLocation.Y, 0, 0);
            _selecting = true;
        }

        protected virtual void UpdateSelection (TileMouseEventArgs e)
        {
            if (!_canSelectRange || !_selecting) {
                return;
            }

            _selectBox.Width = e.TileLocation.X - _selectBox.X;
            _selectBox.Height = e.TileLocation.Y - _selectBox.Y;

            _autoScroll = MouseEdgeScroll(new Point(e.X, e.Y));
        }

        protected virtual void EndSelection (TileMouseEventArgs e)
        {
            if (!_canSelect || !_selecting) {
                return;
            }

            UpdateSelection(e);
            _selecting = false;

            int startx = Math.Min(_selectBox.X, _selectBox.X + _selectBox.Width);
            int starty = Math.Min(_selectBox.Y, _selectBox.Y + _selectBox.Height);
            int endx = Math.Max(_selectBox.X, _selectBox.X + _selectBox.Width);
            int endy = Math.Max(_selectBox.Y, _selectBox.Y + _selectBox.Height);

            for (int x = startx; x <= endx; x++) {
                for (int y = starty; y <= endy; y++) {
                    AddSelectedTile(new TileCoord(x, y));
                }
            }
        }

        #endregion

        #region Drawing

        protected virtual void DrawSelectBox ()
        {
            if (_selecting) {
                int startx = Math.Min(_selectBox.X, _selectBox.X + _selectBox.Width);
                int starty = Math.Min(_selectBox.Y, _selectBox.Y + _selectBox.Height);
                int endx = Math.Max(_selectBox.X, _selectBox.X + _selectBox.Width) + 1;
                int endy = Math.Max(_selectBox.Y, _selectBox.Y + _selectBox.Height) + 1;

                Primitives2D.FillRectangle(_spriteBatch, new Rectangle(
                    (int)(startx * _tileSource.TileWidth * _zoom),
                    (int)(starty * _tileSource.TileHeight * _zoom),
                    (endx - startx) * (int)(_tileSource.TileWidth * _zoom),
                    (endy - starty) * (int)(_tileSource.TileHeight * _zoom)),
                    _selectingBrush);
            }
        }

        protected virtual void DrawTileSelection ()
        {
            foreach (TileCoord coord in SelectedTileLocations) {
                Primitives2D.FillRectangle(_spriteBatch, new Rectangle(
                    (int)(coord.X * _tileSource.TileWidth * _zoom),
                    (int)(coord.Y * _tileSource.TileHeight * _zoom),
                    (int)(_tileSource.TileWidth * _zoom),
                    (int)(_tileSource.TileHeight * _zoom)), 
                    _selectedBrush);
            }
        }

        #endregion

        #endregion

        #region Scrolling

        #region Fields

        private const int _autoScrollThreshold = 24;
        private const int _autoScrollBase = 2;
        private const int _autoScrollMult = 8;

        private bool _canAutoScroll = true;

        private Timer _scrollTimer = new Timer();
        private Vector2 _scrollGradient;

        protected bool _canScrollH = true;
        protected bool _canScrollV = true;

        #endregion

        #region Properties

        [DefaultValue(true)]
        [Description("Indicates whether the control will auto-scroll during qualifying actions")]
        public bool CanAutoScroll
        {
            get { return _canAutoScroll; }
            set
            {
                _canAutoScroll = value;
                if (!_canAutoScroll) {
                    StopAutoScroll();
                }
            }
        }

        #endregion

        #region Scroll Conditions

        private bool ShouldScroll (Point location)
        {
            return location.X < _autoScrollThreshold
                || location.X > Width - _autoScrollThreshold
                || location.Y < _autoScrollThreshold
                || location.Y > Height - _autoScrollThreshold;
        }

        private Vector2 ScrollGradient (Point location)
        {
            Vector2 scroll = new Vector2();
            if (location.X < _autoScrollThreshold) {
                scroll.X = -((_autoScrollThreshold - location.X) / _autoScrollMult) * _autoScrollBase;
            }
            else if (location.X > Width - _autoScrollThreshold) {
                scroll.X = ((location.X - (Width - _autoScrollThreshold)) / _autoScrollMult) * _autoScrollBase;
            }

            if (location.Y < _autoScrollThreshold) {
                scroll.Y = -((_autoScrollThreshold - location.Y) / _autoScrollMult) * _autoScrollBase;
            }
            else if (location.Y > Height - _autoScrollThreshold) {
                scroll.Y = ((location.Y - (Height - _autoScrollThreshold)) / _autoScrollMult) * _autoScrollBase;
            }

            return scroll;
        }

        #endregion

        #region Scroll Control

        private void StartAutoScroll (Point location)
        {
            _scrollGradient = ScrollGradient(location);

            if (_scrollTimer.Interval != 50) {
                _scrollTimer.Interval = 50;
                _scrollTimer.Tick += new System.EventHandler(ScrollTick);
            }
            _scrollTimer.Enabled = true;
        }

        private void StopAutoScroll ()
        {
            _scrollTimer.Enabled = false;
        }

        private void CheckStartAutoScroll (Point location)
        {
            if (!_canAutoScroll) {
                return;
            }

            if (_scrollTimer.Enabled == false) {
                if (_selecting && ShouldScroll(location)) {
                    StartAutoScroll(location);
                }
            }
            else if (_selecting && ShouldScroll(location)) {
                _scrollGradient = ScrollGradient(location);
            }
        }

        private void CheckStopAutoScroll (Point location)
        {
            if (!_canAutoScroll) {
                return;
            }

            if (_scrollTimer.Enabled == true) {
                if (!_selecting || !ShouldScroll(location)) {
                    StopAutoScroll();
                }
            }
        }

        #endregion

        private void ScrollTick (object sender, EventArgs e)
        {
            if (!_canAutoScroll) {
                return;
            }

            int vHeight = VirtualSize.Height;
            int vWidth = VirtualSize.Width;

            int maxH = Math.Max(0, (int)(((vWidth * _zoom) - Width) / _zoom));
            int maxV = Math.Max(0, (int)(((vHeight * _zoom) - Height) / _zoom));

            if (_scrollGradient.X < 0 && _scrollH > 0 && _canScrollH) {
                ScrollTo(ScrollOrientation.HorizontalScroll, Math.Max(0, _scrollH + (int)_scrollGradient.X));
            }
            else if (_scrollGradient.X > 0 && _scrollH < maxH && _canScrollH) {
                ScrollTo(ScrollOrientation.HorizontalScroll, Math.Min(maxH, _scrollH + (int)_scrollGradient.X));
            }

            if (_scrollGradient.Y < 0 && _scrollV > 0 && _canScrollV) {
                ScrollTo(ScrollOrientation.VerticalScroll, Math.Max(0, _scrollV + (int)_scrollGradient.Y));
            }
            else if (_scrollGradient.Y > 0 && _scrollV < maxV && _canScrollV) {
                ScrollTo(ScrollOrientation.VerticalScroll, Math.Min(maxV, _scrollV + (int)_scrollGradient.Y));
            }
        }

        #endregion

        #region IScrollableControl Members

        private int _scrollH;
        private int _scrollV;
        private int _scrollSmChangeH;
        private int _scrollSmChangeV;
        private int _scrollLgChangeH;
        private int _scrollLgChangeV;

        public event ScrollEventHandler Scroll;
        public event EventHandler VirtualSizeChanged;
        public event EventHandler ScrollPropertyChanged;

        private void ScrollTo (ScrollOrientation orientation, int value)
        {
            int oldVal = 0;

            switch (orientation) {
                case ScrollOrientation.HorizontalScroll:
                    if (!_canScrollH) {
                        return;
                    }

                    oldVal = _scrollH;
                    _scrollH = value;
                    break;

                case ScrollOrientation.VerticalScroll:
                    if (!_canScrollV) {
                        return;
                    }

                    oldVal = _scrollV;
                    _scrollV = value;
                    break;
            }

            OnScroll(new ScrollEventArgs(ScrollEventType.ThumbPosition, oldVal, value, orientation));
        }

        protected virtual void OnScroll (ScrollEventArgs e)
        {
            if (Scroll != null) {
                Scroll(this, e);
            }
        }

        protected virtual void OnVirtualSizeChanged ()
        {
            if (VirtualSizeChanged != null) {
                VirtualSizeChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnScrollPropertyChanged ()
        {
            if (ScrollPropertyChanged != null) {
                ScrollPropertyChanged(this, EventArgs.Empty);
            }
        }

        public Control Control
        {
            get { return this; }
        }

        public abstract System.Drawing.Rectangle VirtualSize { get; }

        public int GetScrollValue (ScrollOrientation orientation)
        {
            switch (orientation) {
                case ScrollOrientation.HorizontalScroll:
                    return _scrollH;
                case ScrollOrientation.VerticalScroll:
                    return _scrollV;
            }
            return 0;
        }

        public int GetScrollSmallChange (ScrollOrientation orientation)
        {
            switch (orientation) {
                case ScrollOrientation.HorizontalScroll:
                    return _scrollSmChangeH;
                case ScrollOrientation.VerticalScroll:
                    return _scrollSmChangeV;
            }
            return 0;
        }

        public int GetScrollLargeChange (ScrollOrientation orientation)
        {
            switch (orientation) {
                case ScrollOrientation.HorizontalScroll:
                    return _scrollLgChangeH;
                case ScrollOrientation.VerticalScroll:
                    return _scrollLgChangeV;
            }
            return 0;
        }

        public bool GetScrollEnabled (ScrollOrientation orientation)
        {
            switch (orientation) {
                case ScrollOrientation.HorizontalScroll:
                    return _canScrollH;
                case ScrollOrientation.VerticalScroll:
                    return _canScrollV;
            }
            return false;
        }

        public void SetScrollValue (ScrollOrientation orientation, int value)
        {
            switch (orientation) {
                case ScrollOrientation.HorizontalScroll:
                    _scrollH = value;
                    break;
                case ScrollOrientation.VerticalScroll:
                    _scrollV = value;
                    break;
            }

            Invalidate();
        }

        private void CheckScrollValue ()
        {
            int vHeight = VirtualSize.Height;
            int vWidth = VirtualSize.Width;

            int maxH = Math.Max(0, (int)(((vWidth * _zoom) - Width) / _zoom));
            int maxV = Math.Max(0, (int)(((vHeight * _zoom) - Height) / _zoom));

            _scrollH = Math.Min(_scrollH, maxH);
            _scrollV = Math.Min(_scrollV, maxV);
        }

        #endregion

        public Brush CreateSolidColorBrush (Color color)
        {
            return new SolidColorBrush(_spriteBatch, color);
        }
    }
}
