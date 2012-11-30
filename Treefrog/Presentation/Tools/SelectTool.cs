using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Treefrog.Framework.Model.Support;
using Treefrog.Presentation.Commands;
using Treefrog.Presentation.Layers;
using Amphibian.Drawing;

namespace Treefrog.Presentation.Tools
{
    using XnaKeys = Microsoft.Xna.Framework.Input.Keys;

    public class SelectTool
    {
        private enum ToolState
        {
            None,
            Selecting,
            Dragging,
        }

        private ILevelToolsPresenter _tools;
        private IDocumentToolsPresenter _docTools;
        private LevelPresenter _level;

        private ToolState _state;

        private RubberBand _rubberBand;

        private Brush _selectBrush;
        private TileCoord _mouseCoord;

        private TileSelectionOld _selection;
        private FloatingTileSelectionOld _floating;

        private TileSelectionOld _clipboard;

        public SelectTool (LevelPresenter level)
        {
            _level = level;
            _level.SyncCurrentLayer += SyncLayerHandler;

            _level.LayerControl.MouseEnter += MouseEnterHandler;
            _level.LayerControl.MouseLeave += MouseLeaveHandler;
        }

        public void BindLevelToolsController (ILevelToolsPresenter controller)
        {
            _tools = controller;
        }

        public void BindDocumentToolsController (IDocumentToolsPresenter controller)
        {
            if (_docTools != null) {
                _docTools.CutRaised -= CutToClipboard;
                _docTools.CopyRaised -= CopyToClipboard;
                _docTools.PasteRaised -= PasteFromClipboard;
                _docTools.DeleteRaised -= DeleteSelection;
                _docTools.SelectAllRaised -= SelectAll;
                _docTools.UnselectAllRaised -= UnselectAll;
            }

            _docTools = controller;
            if (_docTools != null) {
                _docTools.CutRaised += CutToClipboard;
                _docTools.CopyRaised += CopyToClipboard;
                _docTools.PasteRaised += PasteFromClipboard;
                _docTools.DeleteRaised += DeleteSelection;
                _docTools.SelectAllRaised += SelectAll;
                _docTools.UnselectAllRaised += UnselectAll;
            }
        }

        private void SyncLayerHandler (object sender, SyncLayerEventArgs e)
        {
            if (e.PreviousControlLayer != null) {
                TileControlLayer prev = e.PreviousControlLayer as TileControlLayer;
                if (prev != null) {
                    prev.MouseTileDown -= MouseTileDownHandler;
                    prev.MouseTileMove -= MouseTileMoveHandler;
                    prev.MouseTileUp -= MouseTileUpHandler;
                    prev.DrawExtraCallback -= DrawBrush;
                    prev.PostDrawContent -= DrawSelectionTiles;
                }
            }

            TileControlLayer control = CurrentControlLayer;
            if (control != null) {
                control.MouseTileDown += MouseTileDownHandler;
                control.MouseTileMove += MouseTileMoveHandler;
                control.MouseTileUp += MouseTileUpHandler;
                control.DrawExtraCallback += DrawBrush;
                control.PostDrawContent += DrawSelectionTiles;
            }
        }

        public TileSelectionOld Selection
        {
            get { return _selection ?? _floating ?? null; }
        }

        public TileSelectionOld Clipboard
        {
            get { return _clipboard; }
        }

        private bool ControllersAttached
        {
            get { return _level != null && _tools != null; }
        }

        private MultiTileGridLayer CurrentLayer
        {
            get
            {
                if (_level != null) {
                    return _level.SelectedLayer as MultiTileGridLayer;
                }

                return null;
            }
        }

        private TileControlLayer CurrentControlLayer
        {
            get
            {
                if (_level != null) {
                    return _level.SelectedControlLayer as TileControlLayer;
                }

                return null;
            }
        }

        private void CancelSelection ()
        {
            if (_selection != null) {
                _selection = null;
            }
            else if (_floating != null) {
                MultiTileGridLayer layer = CurrentLayer;
                if (layer == null) {
                    _floating = null;
                    return;
                }

                TileReplace2DCommand replace = new TileReplace2DCommand(layer);
                foreach (LocatedTileStack lts in _floating) {
                    TileCoord loc = new TileCoord(lts.X + _floating.Origin.X, lts.Y + _floating.Origin.Y);
                    if (lts.Stack != null && loc.X >= 0 && loc.Y >= 0 && loc.X < layer.TilesWide && loc.Y < layer.TilesHigh) {
                        replace.QueueAdd(loc, lts.Stack);
                    }
                }

                _level.History.Execute(replace);

                _floating = null;
            }

            _docTools.RefreshDocumentTools();
        }

        private void CreateFloatingSelection ()
        {
            MultiTileGridLayer layer = CurrentLayer;
            if (layer == null) {
                _selectBrush = null;
                return;
            }

            _floating = _selection.CreateFloatingSelection();
            _selection = null;

            CreateFloatingSelectionCommand command = new CreateFloatingSelectionCommand(layer, _floating);
            _level.History.Execute(command);

            _docTools.RefreshDocumentTools();
        }

        protected void MouseTileDownHandler (object sender, TileMouseEventArgs e)
        {
            if (!ControllersAttached || _tools.ActiveTileTool != TileToolMode.Select) {
                return;
            }

            if (_state != ToolState.None) {
                if (e.Button == MouseButtons.Left)
                    throw new InvalidOperationException("SelectTool received MouseDown event while still active.");
                return;
            }

            MultiTileGridLayer layer = CurrentLayer;
            if (layer == null) {
                return;
            }

            if (e.Button == MouseButtons.Right) {
                CancelSelection();
                return;
            }

            if (MouseOverSelection()) {
                _state = ToolState.Dragging;
            }
            else {
                _state = ToolState.Selecting;
                if (!SelectionModifier()) {
                    CancelSelection();
                }
            }

            switch (_state) {
                case ToolState.Selecting:
                    _level.LayerControl.CanAutoScroll = true;

                    if (_selectBrush == null) {
                        _selectBrush = _level.LayerControl.CreateSolidColorBrush(new Color(.3f, .7f, 1f, .5f));
                    }

                    if (!SelectionModifier()) {
                        _selection = null;
                    }

                    _rubberBand = new RubberBand(_level.LayerControl, layer.TileWidth, layer.TileHeight);
                    _rubberBand.Brush = _selectBrush;
                    _rubberBand.Start(new Point(e.TileLocation.X, e.TileLocation.Y));
                    break;

                case ToolState.Dragging:
                    _level.LayerControl.CanAutoScroll = true;
                    if (_floating == null) {
                        CreateFloatingSelection();
                    }
                    _floating.StartMove(e.TileLocation);
                    break;
            }

            MouseTileMoveHandler(sender, e);
        }

        protected void MouseTileUpHandler (object sender, TileMouseEventArgs e)
        {
            if (!ControllersAttached || _tools.ActiveTileTool != TileToolMode.Select) {
                return;
            }

            MultiTileGridLayer layer = CurrentLayer;
            if (layer == null) {
                return;
            }

            if (e.Button != MouseButtons.Left) {
                return;
            }

            switch (_state) {
                case ToolState.Selecting:
                    _level.LayerControl.CanAutoScroll = false;

                    if (!SelectionModifier()) {
                        _selection = new TileSelectionOld(_level, layer.TileWidth, layer.TileHeight);
                    }

                    TileCoord start = new TileCoord(_rubberBand.Bounds.X, _rubberBand.Bounds.Y);
                    TileCoord end = new TileCoord(_rubberBand.Bounds.X + _rubberBand.Bounds.Width - 1, _rubberBand.Bounds.Y + _rubberBand.Bounds.Height - 1);

                    if (SubtractSelectionModifier()) {
                        _selection.RemoveTiles(start, end);
                    }
                    else {
                        _selection.AddTiles(start, end, layer);
                    }

                    _docTools.RefreshDocumentTools();

                    _rubberBand.Dispose();
                    _rubberBand = null;
                    break;

                case ToolState.Dragging:
                    _level.LayerControl.CanAutoScroll = false;
                    _floating.EndMove(e.TileLocation);
                    break;
                    
            }

            _state = ToolState.None;
        }

        protected void MouseTileMoveHandler (object sender, TileMouseEventArgs e)
        {
            if (!ControllersAttached || _tools.ActiveTileTool != TileToolMode.Select) {
                return;
            }

            _mouseCoord = e.TileLocation;

            MultiTileGridLayer layer = CurrentLayer;
            if (layer == null) {
                return;
            }

            switch (_state) {
                case ToolState.Selecting:
                    int x = Math.Max(0, Math.Min(layer.TilesWide - 1, e.TileLocation.X));
                    int y = Math.Max(0, Math.Min(layer.TilesHigh - 1, e.TileLocation.Y));

                    _rubberBand.End(new Point(x, y));
                    break;

                case ToolState.Dragging:
                    _floating.Move(e.TileLocation);
                    break;
            }

            UpdateCursor();
        }

        private void MouseEnterHandler (object sender, EventArgs e)
        {
        }

        private void MouseLeaveHandler (object sender, EventArgs e)
        {
        }

        private void UpdateCursor ()
        {
            if (MouseOverSelection()) {
                Cursor.Current = Cursors.SizeAll;
            }
            else if (Cursor.Current != Cursors.Default) {
                Cursor.Current = Cursors.Default;
            }
        }

        private bool MouseOverSelection ()
        {
            if (_state == ToolState.Selecting)
                return false;

            if (_selection != null) {
                return _selection.Contains(_mouseCoord) && !SelectionModifier();
            }
            else if (_floating != null) {
                return _floating.Contains(_mouseCoord) && !SelectionModifier();
            }
            
            return false;
        }

        private bool AddSelectionModifier ()
        {
            KeyboardState keyboard = Keyboard.GetState();
            return keyboard.IsKeyDown(XnaKeys.RightShift) || keyboard.IsKeyDown(XnaKeys.LeftShift);
        }

        private bool SubtractSelectionModifier ()
        {
            KeyboardState keyboard = Keyboard.GetState();
            return keyboard.IsKeyDown(XnaKeys.RightControl) || keyboard.IsKeyDown(XnaKeys.LeftControl);
        }

        private bool SelectionModifier ()
        {
            return AddSelectionModifier() || SubtractSelectionModifier();
        }

        public void DeleteSelection (object sender, EventArgs e)
        {
            MultiTileGridLayer layer = CurrentLayer;
            if (layer == null) {
                return;
            }

            if (_selection != null) {
                TileReplace2DCommand replace = new TileReplace2DCommand(layer);
                foreach (LocatedTileStack lts in _selection) {
                    TileCoord loc = new TileCoord(lts.X + _selection.Origin.X, lts.Y + _selection.Origin.Y);
                    if (lts.Stack != null && loc.X >= 0 && loc.Y >= 0 && loc.X < layer.TilesWide && loc.Y < layer.TilesHigh) {
                        foreach (Tile t in lts.Stack) {
                            replace.QueueReplacement(loc, (TileStack)null);
                        }
                    }
                }

                _level.History.Execute(replace);
                _selection = null;
            }
            else if (_floating != null) {
                _floating = null;
            }
        }

        public void SelectAll (object sender, EventArgs e)
        {
            MultiTileGridLayer layer = CurrentLayer;
            if (layer == null) {
                return;
            }

            if (_selection != null) {
                _selection.AddTiles(new TileCoord(0, 0), new TileCoord(layer.TilesWide - 1, layer.TilesHigh - 1), layer);
                return;
            }
            
            if (_floating != null) {
                CancelSelection(); 
            }

            _selection = new TileSelectionOld(_level, layer.TileWidth, layer.TileHeight);
            _selection.AddTiles(new TileCoord(0, 0), new TileCoord(layer.TilesWide - 1, layer.TilesHigh - 1), layer);
        }

        public void UnselectAll (object sender, EventArgs e)
        {
            CancelSelection();
        }

        public void CopyToClipboard (object sender, EventArgs e)
        {
            _clipboard = _selection ?? _floating;
        }

        public void CutToClipboard (object sender, EventArgs e)
        {
            _clipboard = _selection ?? _floating;
            DeleteSelection(this, e);
        }

        public void PasteFromClipboard (object sender, EventArgs e)
        {
            MultiTileGridLayer layer = CurrentLayer;
            if (layer == null) {
                return;
            }

            if (_clipboard != null) {
                CancelSelection();

                _floating = _clipboard.CreateFloatingSelection();

                int hscroll = _level.LayerControl.GetScrollValue(ScrollOrientation.HorizontalScroll);
                int vscroll = _level.LayerControl.GetScrollValue(ScrollOrientation.VerticalScroll);

                _floating.StartMove(new TileCoord(0, 0));
                _floating.EndMove(new TileCoord(hscroll / layer.TileWidth, vscroll / layer.TileHeight));
            }
        }

        private void DrawBrush (object sender, DrawLayerEventArgs e)
        {
            if (!ControllersAttached || _tools.ActiveTileTool != TileToolMode.Select) {
                return;
            }

            if (_selection != null) {
                _selection.DrawSelection(e.SpriteBatch, e.Zoom);
            }
            else if (_floating != null) {
                _floating.DrawSelection(e.SpriteBatch, e.Zoom);
            }

        }

        private void DrawSelectionTiles (object sender, DrawLayerEventArgs e)
        {
            if (!ControllersAttached || _tools.ActiveTileTool != TileToolMode.Select) {
                return;
            }

            else if (_floating != null) {
                _floating.DrawTiles(e.SpriteBatch, e.Zoom);
            }
        }
    }
}
