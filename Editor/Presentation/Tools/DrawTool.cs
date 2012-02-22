using System;
using Microsoft.Xna.Framework;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Commands;
using Treefrog.Presentation.Layers;
using System.Windows.Forms;
using Amphibian.Drawing;

namespace Treefrog.Presentation.Tools
{
    public class DrawTool
    {
        private enum ToolState
        {
            None,
            Drawing,
            Selecting,
        }



        private ITilePoolListPresenter _pool;
        private ILevelToolsPresenter _tools;
        private LevelPresenter _level;

        //private bool _drawing;
        private ToolState _state;
        private TileReplace2DCommand _drawCommand;

        private bool _drawBrush;
        private TileCoord _mouseCoord;

        private RubberBand _rubberBand;
        private Brush _selectBrush;

        public DrawTool (LevelPresenter level)
        {
            _level = level;
            _level.SyncCurrentLayer += SyncLayerHandler;

            _level.LayerControl.MouseEnter += MouseEnterHandler;
            _level.LayerControl.MouseLeave += MouseLeaveHandler;

            if (!_level.LayerControl.Initialized) {
                _level.LayerControl.ControlInitialized += LayerControlInitialized;
            }
            
            Initialize();
        }

        private void LayerControlInitialized (object sender, EventArgs e)
        {
            _level.LayerControl.ControlInitialized -= LayerControlInitialized;

            Initialize();
        }

        private void Initialize ()
        {
            if (_level.LayerControl.Initialized) {
                _selectBrush = _level.LayerControl.CreateSolidColorBrush(new Color(.3f, .7f, 1.0f, .5f));
            }
        }

        public void BindTileSourceController (ITilePoolListPresenter controller)
        {
            _pool = controller;
        }

        public void BindLevelToolsController (ILevelToolsPresenter controller)
        {
            _tools = controller;
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
                }
            }

            TileControlLayer control = CurrentControlLayer;
            if (control != null) {
                control.MouseTileDown += MouseTileDownHandler;
                control.MouseTileMove += MouseTileMoveHandler;
                control.MouseTileUp += MouseTileUpHandler;
                control.DrawExtraCallback += DrawBrush;
            }
        }

        private bool ControllersAttached
        {
            get { return _level != null && _pool != null && _tools != null; }
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

        protected virtual void MouseTileDownHandler (object sender, TileMouseEventArgs e)
        {
            if (!ControllersAttached || _tools.ActiveTileTool != TileToolMode.Draw) {
                _state = ToolState.None;
                return;
            }

            if (_state != ToolState.None) {
                return;
            }

            MultiTileGridLayer layer = CurrentLayer;
            if (layer == null) {
                return;
            }

            switch (e.Button) {
                case MouseButtons.Left:
                    if (_pool.SelectedTile == null)
                        return;

                    _state = ToolState.Drawing;
                    _drawCommand = new TileReplace2DCommand(layer);
                    break;

                case MouseButtons.Right:
                    _state = ToolState.Selecting;
                    _rubberBand = new RubberBand(_level.LayerControl, layer.TileWidth, layer.TileHeight);
                    _rubberBand.Brush = _selectBrush;
                    _rubberBand.Start(new Point(e.TileLocation.X, e.TileLocation.Y));
                    break;
            }

            MouseTileMoveHandler(sender, e);
        }

        protected virtual void MouseTileUpHandler (object sender, TileMouseEventArgs e)
        {
            if (!ControllersAttached || _tools.ActiveTileTool != TileToolMode.Draw) {
                _state = ToolState.None;
                return;
            }

            if (_state == ToolState.None) {
                return;
            }

            switch (e.Button) {
                case MouseButtons.Left:
                    if (_state != ToolState.Drawing)
                        break;

                    _level.History.Execute(_drawCommand);
                    _state = ToolState.None;
                    break;

                case MouseButtons.Right:
                    if (_state != ToolState.Selecting)
                        break;

                    _state = ToolState.None;
                    _rubberBand.Dispose();
                    _rubberBand = null;
                    break;
            }
        }

        protected virtual void MouseTileMoveHandler (object sender, TileMouseEventArgs e)
        {
            if (!ControllersAttached || _tools.ActiveTileTool != TileToolMode.Draw) {
                _state = ToolState.None;
                return;
            }

            _mouseCoord = e.TileLocation;

            MultiTileGridLayer layer = CurrentLayer;
            if (layer == null) {
                return;
            }

            switch (_state) {
                case ToolState.Drawing:
                    if (e.TileLocation.X < 0 || e.TileLocation.X >= layer.TilesWide)
                        return;
                    if (e.TileLocation.Y < 0 || e.TileLocation.Y >= layer.TilesHigh)
                        return;

                    if (layer[e.TileLocation] == null || layer[e.TileLocation].Top != _pool.SelectedTile) {
                        _drawCommand.QueueAdd(e.TileLocation, _pool.SelectedTile);
                        layer.AddTile(e.TileLocation.X, e.TileLocation.Y, _pool.SelectedTile);
                    }
                    break;

                case ToolState.Selecting:
                    int x = Math.Max(0, Math.Min(layer.TilesWide - 1, e.TileLocation.X));
                    int y = Math.Max(0, Math.Min(layer.TilesHigh - 1, e.TileLocation.Y));

                    _rubberBand.End(new Point(x, y));
                    break;
            }
        }

        private void MouseEnterHandler (object sender, EventArgs e)
        {
            _drawBrush = true;
        }

        private void MouseLeaveHandler (object sender, EventArgs e)
        {
            _drawBrush = false;
        }

        private void DrawBrush (object sender, DrawLayerEventArgs e)
        {
            if (!ControllersAttached || _tools.ActiveTileTool != TileToolMode.Draw) {
                _state = ToolState.None;
                return;
            }

            if (!_drawBrush) {
                return;
            }

            if (_state == ToolState.None || _state == ToolState.Drawing) {
                MultiTileGridLayer layer = CurrentLayer;
                if (layer == null) {
                    return;
                }

                if (_pool.SelectedTile == null) {
                    return;
                }

                Rectangle rect = new Rectangle(
                    (int)(_mouseCoord.X * layer.TileWidth * _level.LayerControl.Zoom),
                    (int)(_mouseCoord.Y * layer.TileHeight * _level.LayerControl.Zoom),
                    (int)(layer.TileWidth * _level.LayerControl.Zoom),
                    (int)(layer.TileHeight * _level.LayerControl.Zoom)
                    );

                _pool.SelectedTile.Draw(e.SpriteBatch, rect, new Color(1f, 1f, 1f, 0.5f));
            }
        }
    }
}
