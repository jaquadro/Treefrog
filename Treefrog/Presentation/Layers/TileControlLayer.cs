using System;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Treefrog.Windows.Controls;
using TFImaging = Treefrog.Framework.Imaging;
using Treefrog.Presentation.Tools;
using Treefrog.Prseentation.Tools;
using Treefrog.Presentation.Commands;
using System.Collections.Generic;

namespace Treefrog.Presentation.Layers
{
    public abstract class TileControlLayer : BaseControlLayer, IPointerToolResponder, ITileSelectionLayer, ICommandSubscriber
    {
        #region Fields

        // Model

        private TileLayer _layer;

        // Graphics and Service

        private Effect _effectTrans;
        private EffectParameter _effectTransColor;

        private Texture2D _tileGridBrush;
        private Texture2D _tileGridBrushRight;
        private Texture2D _tileGridBrushBottom;

        private Color _gridColor = new Color(0, 0, 0, 0.5f);

        private bool _useTransColor = false;
        private Color _transColor = new Color(0, 0, 0);

        #endregion

        #region Events

        public event EventHandler<TileMouseEventArgs> MouseTileClick;
        public event EventHandler<TileMouseEventArgs> MouseTileDown;
        public event EventHandler<TileMouseEventArgs> MouseTileMove;
        public event EventHandler<TileMouseEventArgs> MouseTileUp;

        #endregion

        #region Constructors

        public TileControlLayer (LayerControl control)
            : base(control)
        {
            Control.ZoomChanged += ControlZoomChangedHandler;

            Control.MouseClick += ControlMouseClickHandler;
            Control.MouseUp += ControlMouseUpHandler;
            Control.MouseDown += ControlMouseDownHandler;
            Control.MouseMove += ControlMouseMoveHandler;

            InitializeCommandManager();
        }

        public TileControlLayer (LayerControl control, TileLayer layer)
            : this(control)
        {
            Layer = layer;
            OnVirutalSizeChanged(EventArgs.Empty);
        }

        #endregion

        public void BindLevelController (ILevelPresenter levelController)
        {
            _levelController = levelController;
            _commandManager.Perform(CommandKey.TileToolSelect);
        }

        public void BindObjectController (ITilePoolListPresenter tilePoolController)
        {
            if (_tilePoolController != null) {
                _tilePoolController.TileSelectionChanged -= TileSelectionChangedHandler;
            }

            _tilePoolController = tilePoolController;

            if (_tilePoolController != null) {
                _tilePoolController.TileSelectionChanged += TileSelectionChangedHandler;
            }
        }

        public void BindTileBrushManager (ITileBrushManagerPresenter tileBrushController)
        {
            if (_tileBrushController != null) {
                _tileBrushController.TileBrushSelected -= TileBrushSelectedHandler;
            }

            _tileBrushController = tileBrushController;

            if (_tileBrushController != null) {
                _tileBrushController.TileBrushSelected += TileBrushSelectedHandler;
            }
        }

        public override void Activate ()
        {
            if (_selection != null && _levelController != null) {
                _levelController.Annotations.Remove(_selection.SelectionAnnotation);
                _levelController.Annotations.Add(_selection.SelectionAnnotation);
            }
        }

        public override void Deactivate ()
        {
            if (_selection != null && _levelController != null) {
                _levelController.Annotations.Remove(_selection.SelectionAnnotation);
            }
        }

        private enum DrawToolMode
        {
            Tile,
            Brush,
        }

        private DrawToolMode _drawMode = DrawToolMode.Tile;

        private void TileSelectionChangedHandler (object sender, EventArgs e)
        {
            _drawMode = DrawToolMode.Tile;

            switch (CommandManager.SelectedCommand(CommandToggleGroup.TileTool)) {
                case CommandKey.TileToolDraw:
                    SetTool(TileTool.Draw);
                    break;
                case CommandKey.TileToolErase:
                case CommandKey.TileToolSelect:
                    CommandManager.Perform(CommandKey.TileToolDraw);
                    break;
            }
        }

        private void TileBrushSelectedHandler (object sender, EventArgs e)
        {
            _drawMode = DrawToolMode.Brush;

            switch (CommandManager.SelectedCommand(CommandToggleGroup.TileTool)) {
                case CommandKey.TileToolDraw:
                    SetTool(TileTool.Draw);
                    break;
                case CommandKey.TileToolErase:
                case CommandKey.TileToolSelect:
                    CommandManager.Perform(CommandKey.TileToolDraw);
                    break;
            }
        }

        #region Properties

        public new TileLayer Layer
        {
            get { return _layer; }
            protected set
            {
                _layer = value;
                base.Layer = value;

                if (Control.Initialized) {
                    BuildTileBrush();
                }
            }
        }

        public Color GridColor
        {
            get { return _gridColor; }
            set
            {
                _gridColor = value;

                if (Control.Initialized) {
                    BuildTileBrush();
                }
            }
        }

        #endregion

        #region Event Handlers

        private void ControlZoomChangedHandler (object sender, EventArgs e)
        {
            if (Control.Initialized) {
                BuildTileBrush();
            }
        }

        private void ControlMouseClickHandler (object sender, MouseEventArgs e)
        {
            if (CheckLayerCondition(ShouldRespondToInput) && _layer != null) {
                TileCoord coords = MouseToTileCoords(new Point(e.X, e.Y));
                OnMouseTileClick(new TileMouseEventArgs(e, coords, GetTile(coords)));
            }
        }

        private void ControlMouseUpHandler (object sender, MouseEventArgs e)
        {
            if (CheckLayerCondition(ShouldRespondToInput) && _layer != null) {
                TileCoord coords = MouseToTileCoords(new Point(e.X, e.Y));
                OnMouseTileUp(new TileMouseEventArgs(e, coords, GetTile(coords)));
            }
        }

        private void ControlMouseDownHandler (object sender, MouseEventArgs e)
        {
            if (CheckLayerCondition(ShouldRespondToInput) && _layer != null) {
                TileCoord coords = MouseToTileCoords(new Point(e.X, e.Y));
                OnMouseTileDown(new TileMouseEventArgs(e, coords, GetTile(coords)));
            }
        }

        private void ControlMouseMoveHandler (object sender, MouseEventArgs e)
        {
            if (CheckLayerCondition(ShouldRespondToInput) && _layer != null) {
                TileCoord coords = MouseToTileCoords(new Point(e.X, e.Y));
                OnMouseTileMove(new TileMouseEventArgs(e, coords, GetTile(coords)));
            }
        }

        #endregion

        #region Event Dispatchers

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
        }

        protected virtual void OnMouseTileMove (TileMouseEventArgs e)
        {
            if (MouseTileMove != null) {
                MouseTileMove(this, e);
            }
        }

        protected virtual void OnMouseTileUp (TileMouseEventArgs e)
        {
            if (MouseTileUp != null) {
                MouseTileUp(this, e);
            }
        }

        #endregion

        protected override void Initiailize ()
        {
            base.Initiailize();

            if (_useTransColor) {
                _effectTrans = Control.ContentManager.Load<Effect>("TransColor");
                _effectTransColor = _effectTrans.Parameters["transColor"];
                _effectTransColor.SetValue(_transColor.ToVector4());
            }

            BuildTileBrush();
        }

        protected Vector2 BeginDraw (SpriteBatch spriteBatch)
        {
            Rectangle region = Control.VisibleRegion;

            Vector2 offset = Control.VirtualSurfaceOffset;
            offset.X = (float)Math.Ceiling(offset.X - region.X * Control.Zoom);
            offset.Y = (float)Math.Ceiling(offset.Y - region.Y * Control.Zoom);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, _effectTrans, Matrix.CreateTranslation(offset.X, offset.Y, 0));

            return offset;
        }

        protected void EndDraw (SpriteBatch spriteBatch)
        {
            spriteBatch.End();
        }

        protected override void DrawContentImpl (SpriteBatch spriteBatch)
        {
            if (_layer == null || Visible == false) {
                return;
            }

            Rectangle region = Control.VisibleRegion;

            /*Vector2 offset = Control.VirtualSurfaceOffset;
            offset.X = (float)Math.Ceiling(offset.X - region.X * Control.Zoom);
            offset.Y = (float)Math.Ceiling(offset.Y - region.Y * Control.Zoom);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, _effectTrans, Matrix.CreateTranslation(offset.X, offset.Y, 0));
            spriteBatch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;*/

            int zoomTileWidth = (int)(_layer.TileWidth * Control.Zoom);
            int zoomTileHeight = (int)(_layer.TileHeight * Control.Zoom);

            TFImaging.Rectangle tileRegion = new TFImaging.Rectangle(
                region.X / _layer.TileWidth,
                region.Y / _layer.TileHeight,
                (int)(region.Width + region.X % _layer.TileWidth + _layer.TileWidth - 1) / _layer.TileWidth,
                (int)(region.Height + region.Y % _layer.TileHeight + _layer.TileHeight - 1) / _layer.TileHeight
                );

            DrawTiles(spriteBatch, tileRegion);

            //spriteBatch.End();
        }

        protected override void DrawGridImpl (SpriteBatch spriteBatch)
        {
            if (_layer == null) {
                return;
            }

            Rectangle region = Control.VisibleRegion;

            Vector2 offset = Control.VirtualSurfaceOffset;
            offset.X = (float)Math.Ceiling(offset.X - region.X * Control.Zoom);
            offset.Y = (float)Math.Ceiling(offset.Y - region.Y * Control.Zoom);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));
            spriteBatch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            TFImaging.Rectangle tileRegion = new TFImaging.Rectangle(
                region.X / _layer.TileWidth,
                region.Y / _layer.TileHeight,
                (int)(region.Width + region.X % _layer.TileWidth + _layer.TileWidth - 1) / _layer.TileWidth,
                (int)(region.Height + region.Y % _layer.TileHeight + _layer.TileHeight - 1) / _layer.TileHeight
                );

            Func<int, int, bool> inside = TileInRegionPredicate(tileRegion);

            for (int x = tileRegion.X; x <= tileRegion.X + tileRegion.Width; x++) {
                for (int y = tileRegion.Y; y <= tileRegion.Y + tileRegion.Height; y++) {
                    bool iCenter = inside(x, y);
                    bool iLeft = inside(x - 1, y);
                    bool iUpper = inside(x, y - 1);
                    bool iUpperLeft = inside(x - 1, y - 1);

                    Vector2 pos = new Vector2(x * _layer.TileWidth * Control.Zoom, y * _layer.TileHeight * Control.Zoom);
                    if (iCenter || (iLeft && iUpper)) {
                        spriteBatch.Draw(_tileGridBrush, pos, Color.White);
                    }
                    else if (iLeft) {
                        spriteBatch.Draw(_tileGridBrushRight, pos, Color.White);
                    }
                    else if (iUpper) {
                        spriteBatch.Draw(_tileGridBrushBottom, pos, Color.White);
                    }
                    if (iUpperLeft) {
                        continue;
                    }
                }
            }

            spriteBatch.End();
        }

        protected virtual void DrawTiles (SpriteBatch spriteBatch, TFImaging.Rectangle tileRegion) { }

        protected virtual Tile GetTile (TileCoord coord) 
        { 
            return null; 
        }

        protected abstract Func<int, int, bool> TileInRegionPredicate (TFImaging.Rectangle tileRegion);

        private void BuildTileBrush ()
        {
            if (_layer == null) {
                _tileGridBrush = null;
                _tileGridBrushRight = null;
                _tileGridBrushBottom = null;
                return;
            }

            int x = (int)(_layer.TileWidth * Control.Zoom);
            int y = (int)(_layer.TileHeight * Control.Zoom);

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

            _tileGridBrush = new Texture2D(Control.GraphicsDevice, x, y, false, SurfaceFormat.Color);
            _tileGridBrush.SetData(colors);

            _tileGridBrushRight = new Texture2D(Control.GraphicsDevice, x, y, false, SurfaceFormat.Color);
            _tileGridBrushRight.SetData(right);

            _tileGridBrushBottom = new Texture2D(Control.GraphicsDevice, x, y, false, SurfaceFormat.Color);
            _tileGridBrushBottom.SetData(bottom);
        }

        private TileCoord MouseToTileCoords (Point mouse)
        {
            Rectangle region = Control.VisibleRegion;

            Vector2 offset = Control.VirtualSurfaceOffset;
            offset.X = (float)Math.Ceiling(offset.X - region.X * Control.Zoom);
            offset.Y = (float)Math.Ceiling(offset.Y - region.Y * Control.Zoom);

            int tx = (int)Math.Floor(((float)mouse.X - offset.X) / (_layer.TileWidth * Control.Zoom));
            int ty = (int)Math.Floor(((float)mouse.Y - offset.Y) / (_layer.TileHeight * Control.Zoom));

            return new TileCoord(tx, ty);
        }



        private ILevelPresenter _levelController;
        private ITilePoolListPresenter _tilePoolController;
        private ITileBrushManagerPresenter _tileBrushController;
        private PointerTool _currentTool;
        private TileSelection _selection;

        //private ITileSelectionLayer _selectionController;

        // TODO: MultiTileGridLayer stuff should move into appropriate class
        public void SetTool (TileTool tool)
        {
            if (_levelController == null)
                return;

            if (_selection != null && tool == TileTool.Select)
                _selection.Activate();
            else if (_selection != null)
                _selection.Deactivate();

            switch (tool) {
                case TileTool.Select:
                    _currentTool = new TileSelectTool(_levelController.History, Layer as MultiTileGridLayer, _levelController.Annotations, this);
                    break;
                case TileTool.Draw:
                    if (_drawMode == DrawToolMode.Tile) {
                        TileDrawTool drawTool = new TileDrawTool(_levelController.History, Layer as MultiTileGridLayer, _levelController.Annotations);
                        drawTool.BindTilePoolController(_tilePoolController);
                        drawTool.BindTileBrushManager(_tileBrushController);
                        _currentTool = drawTool;
                    }
                    if (_drawMode == DrawToolMode.Brush) {
                        TileBrushTool drawTool = new TileBrushTool(_levelController.History, Layer as MultiTileGridLayer, _levelController.Annotations);
                        drawTool.BindTilePoolController(_tilePoolController);
                        drawTool.BindTileBrushManager(_tileBrushController);
                        _currentTool = drawTool;
                    }
                    break;
                case TileTool.Erase:
                    _currentTool = new TileEraseTool(_levelController.History, Layer as MultiTileGridLayer, _levelController.Annotations);
                    break;
                case TileTool.Fill:
                    TileFillTool fillTool = new TileFillTool(_levelController.History, Layer as MultiTileGridLayer);
                    fillTool.BindTilePoolController(_tilePoolController);
                    _currentTool = fillTool;
                    break;
                default:
                    _currentTool = null;
                    break;
            }
        }

        #region Pointer Commands

        public void HandleStartPointerSequence (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.StartPointerSequence(info, new LayerControlViewport(Control));
        }

        public void HandleUpdatePointerSequence (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.UpdatePointerSequence(info, new LayerControlViewport(Control));
        }

        public void HandleEndPointerSequence (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.EndPointerSequence(info, new LayerControlViewport(Control));
        }

        public void HandlePointerPosition (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.PointerPosition(info, new LayerControlViewport(Control));
        }

        public void HandlePointerLeaveField ()
        {
            if (_currentTool != null)
                _currentTool.PointerLeaveField();
        }

        #endregion

        #region Tile Selection

        public void CreateTileSelection ()
        {
            if (_selection != null)
                DeleteTileSelection();

            _selection = new TileSelection(Layer.TileWidth, Layer.TileHeight);
            if (!(_currentTool is TileSelectTool))
                _selection.Deactivate();

            _levelController.Annotations.Add(_selection.SelectionAnnotation);

            CommandManager.Invalidate(CommandKey.Cut);
            CommandManager.Invalidate(CommandKey.Copy);
            CommandManager.Invalidate(CommandKey.Delete);
        }

        public void CreateFloatingSelection ()
        {
            CreateTileSelection();

            _selection.Float();
        }

        public void DeleteTileSelection ()
        {
            if (_selection != null) {
                _levelController.Annotations.Remove(_selection.SelectionAnnotation);
                _selection = null;

                CommandManager.Invalidate(CommandKey.Cut);
                CommandManager.Invalidate(CommandKey.Copy);
                CommandManager.Invalidate(CommandKey.Delete);
            }
        }

        public void RestoreTileSelection (TileSelection selection)
        {
            if (_selection != null)
                DeleteTileSelection();

            _selection = new TileSelection(selection);
            _levelController.Annotations.Add(_selection.SelectionAnnotation);
        }

        public void ClearTileSelection ()
        {
            if (_selection != null) {
                _selection.ClearTiles();
            }
        }

        public void SetTileSelection (IEnumerable<TileCoord> tileLocations)
        {
            if (_selection != null) {
                _selection.ClearTiles();
                _selection.AddTiles(Layer as MultiTileGridLayer, tileLocations);
            }
        }

        public void AddTilesToSelection (IEnumerable<TileCoord> tileLocations)
        {
            if (_selection != null) {
                _selection.AddTiles(Layer as MultiTileGridLayer, tileLocations);
            }
        }

        public void RemoveTilesFromSelection (IEnumerable<TileCoord> tileLocations)
        {
            if (_selection != null) {
                _selection.RemoveTiles(tileLocations);
            }
        }

        public void FloatSelection ()
        {
            if (_selection != null && _selection.Floating == false) {
                _selection.Float();
            }
        }

        public TileSelection TileSelection
        {
            get { return _selection; }
        }

        public void DefloatSelection ()
        {
            if (_selection != null && _selection.Floating == true) {
                _selection.Defloat();
            }
        }

        public void SetSelectionOffset (TileCoord offset)
        {
            if (_selection != null)
                _selection.Offset = offset;
        }

        public void MoveSelectionByOffset (TileCoord offset)
        {
            if (_selection != null) {
                _selection.Offset = new TileCoord(_selection.Offset.X + offset.X, _selection.Offset.Y + offset.Y);
            }
        }

        public bool HasSelection
        {
            get { return _selection != null; }
        }

        public TileCoord TileSelectionOffset
        {
            get { return (_selection != null) ? _selection.Offset : new TileCoord(0, 0); }
        }

        public bool TileSelectionCoverageAt (TileCoord coord)
        {
            if (_selection == null)
                return false;

            return _selection.CoverageAt(coord);
        }

        #endregion

        #region Command Handling

        private CommandManager _commandManager;

        private void InitializeCommandManager ()
        {
            _commandManager = new CommandManager();

            _commandManager.Register(CommandKey.Cut, CommandCanCut, CommandCut);
            _commandManager.Register(CommandKey.Copy, CommandCanCopy, CommandCopy);
            _commandManager.Register(CommandKey.Paste, CommandCanPaste, CommandPaste);
            _commandManager.Register(CommandKey.Delete, CommandCanDelete, CommandDelete);

            _commandManager.RegisterToggleGroup(CommandToggleGroup.TileTool, CommandGroupCheckTileTool, CommandGroupPerformTileTool);
            _commandManager.RegisterToggle(CommandToggleGroup.TileTool, CommandKey.TileToolSelect);
            _commandManager.RegisterToggle(CommandToggleGroup.TileTool, CommandKey.TileToolDraw);
            _commandManager.RegisterToggle(CommandToggleGroup.TileTool, CommandKey.TileToolErase);
            _commandManager.RegisterToggle(CommandToggleGroup.TileTool, CommandKey.TileToolFill);
            _commandManager.RegisterToggle(CommandToggleGroup.TileTool, CommandKey.TileToolStamp);
        }

        public CommandManager CommandManager
        {
            get { return _commandManager; }
        }

        private IEnumerable<ICommandSubscriber> CommandForwarder ()
        {
            ICommandSubscriber tool = _currentTool as ICommandSubscriber;
            if (tool != null)
                yield return tool;
        }

        #region Cut

        private bool CommandCanCut ()
        {
            TileSelectTool tool = _currentTool as TileSelectTool;
            if (tool != null)
                return _selection != null;
            else
                return false;
        }

        private void CommandCut ()
        {
            if (CommandCanCut()) {
                TileSelectionClipboard clip = new TileSelectionClipboard(_selection.Tiles);
                clip.CopyToClipboard();

                CompoundCommand command = new CompoundCommand();
                if (!_selection.Floating)
                    command.AddCommand(new FloatTileSelectionCommand(Layer as MultiTileGridLayer, this));
                command.AddCommand(new DeleteTileSelectionCommand(this));

                _levelController.History.Execute(command);

                CommandManager.Invalidate(CommandKey.Paste);
            }
        }

        #endregion

        #region Copy

        private bool CommandCanCopy ()
        {
            return CommandCanCut();
        }

        private void CommandCopy ()
        {
            if (CommandCanCopy()) {
                TileSelectionClipboard clip = new TileSelectionClipboard(_selection.Tiles);
                clip.CopyToClipboard();

                CommandManager.Invalidate(CommandKey.Paste);
            }
        }

        #endregion

        #region Paste

        private bool CommandCanPaste ()
        {
            return TileSelectionClipboard.ContainsData;
        }

        private void CommandPaste ()
        {
            if (CommandCanPaste()) {
                TileSelectionClipboard clip = TileSelectionClipboard.CopyFromClipboard();
                TileSelection selection = clip.GetAsTileSelection(Layer.Level.Project, Layer.TileWidth, Layer.TileHeight);
                if (selection == null)
                    return;

                if (_selection != null) {
                    CompoundCommand command = new CompoundCommand();
                    if (_selection.Floating)
                        command.AddCommand(new DefloatTileSelectionCommand(Layer as MultiTileGridLayer, this));
                    command.AddCommand(new DeleteTileSelectionCommand(this));

                    _levelController.History.Execute(command);
                }

                Command pasteCommand = new PasteFloatingSelectionCommand(this, selection, GetCenterTileOffset());
                _levelController.History.Execute(pasteCommand);

                if (!(_currentTool is TileSelectTool)) {
                    CommandManager.Perform(CommandKey.TileToolSelect);
                    //ITileToolCollection toolCollection = Level.LookupToolCollection<ITileToolCollection>();
                    //if (toolCollection != null)
                    //    toolCollection.SelectedTool = TileTool.Select;
                }

                _selection.Activate();
            }
        }

        private TileCoord GetCenterTileOffset ()
        {
            Rectangle region = _levelController.LayerControl.VisibleRegion;
            int centerViewX = (int)(region.Left + (region.Right - region.Left) / 2);
            int centerViewY = (int)(region.Top + (region.Bottom - region.Top) / 2);

            return new TileCoord(centerViewX / Layer.TileWidth, centerViewY / Layer.TileHeight);
        }

        #endregion

        #region Delete

        private bool CommandCanDelete ()
        {
            return CommandCanCut();
        }

        private void CommandDelete ()
        {
            if (CommandCanDelete()) {
                CompoundCommand command = new CompoundCommand();
                if (!_selection.Floating)
                    command.AddCommand(new FloatTileSelectionCommand(Layer as MultiTileGridLayer, this));
                command.AddCommand(new DeleteTileSelectionCommand(this));

                _levelController.History.Execute(command);

                CommandManager.Invalidate(CommandKey.Paste);
            }
        }

        #endregion

        private bool CommandGroupCheckTileTool ()
        {
            return true;
        }

        private void CommandGroupPerformTileTool ()
        {
            switch (CommandManager.SelectedCommand(CommandToggleGroup.TileTool)) {
                case CommandKey.TileToolSelect:
                    SetTool(TileTool.Select);
                    break;
                case CommandKey.TileToolDraw:
                    SetTool(TileTool.Draw);
                    break;
                case CommandKey.TileToolErase:
                    SetTool(TileTool.Erase);
                    break;
                case CommandKey.TileToolFill:
                    SetTool(TileTool.Fill);
                    break;
                case CommandKey.TileToolStamp:
                    SetTool(TileTool.Stamp);
                    break;
            }
        }

        #endregion

        public override IPointerToolResponder PointerToolResponder
        {
            get { return this; }
        }
    }

    [Serializable]
    public class TileSelectionClipboard
    {
        private Dictionary<TileCoord, int[]> _tiles;

        public TileSelectionClipboard (IDictionary<TileCoord, TileStack> tiles)
        {
            _tiles = new Dictionary<TileCoord, int[]>();
            foreach (KeyValuePair<TileCoord, TileStack> kvp in tiles) {
                int[] stack = TileStack.NullOrEmpty(kvp.Value) ? new int[0] : new int[kvp.Value.Count];
                for (int i = 0; i < stack.Length; i++)
                    stack[i] = kvp.Value[i].Id;
                _tiles.Add(kvp.Key, stack);
            }
        }

        public static bool ContainsData
        {
            get { return Clipboard.ContainsData(typeof(TileSelectionClipboard).FullName); }
        }

        public void CopyToClipboard ()
        {
            Clipboard.SetData(typeof(TileSelectionClipboard).FullName, this);
        }

        public static TileSelectionClipboard CopyFromClipboard ()
        {
            TileSelectionClipboard clip = Clipboard.GetData(typeof(TileSelectionClipboard).FullName) as TileSelectionClipboard;
            if (clip == null)
                return null;

            return clip;
        }

        public TileSelection GetAsTileSelection (Project project, int tileWidth, int tileHeight)
        {
            Dictionary<TileCoord, TileStack> xlat = new Dictionary<TileCoord, TileStack>();
            foreach (KeyValuePair<TileCoord, int[]> item in _tiles) {
                TileStack stack = new TileStack();

                foreach (int tileId in item.Value) {
                    TilePool pool = project.TilePoolManager.PoolFromTileId(tileId);
                    Tile tile = pool.GetTile(tileId);
                    stack.Add(tile);
                }

                xlat.Add(item.Key, stack);
            }

            TileSelection selection = new TileSelection(tileWidth, tileHeight);
            selection.AddTiles(xlat);

            return selection;
        }
    }
}
