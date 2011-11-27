using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Editor.Model;
using Editor.Controls;
using Microsoft.Xna.Framework;
using Editor.Model.Controls;
using Editor.A.Presentation;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Amphibian.Drawing;

namespace Editor
{
    public class EraseTool
    {
        private ILevelToolsPresenter _tools;
        private LevelPresenter _level;

        private bool _drawing;
        private bool _selecting;
        private TileReplace2DCommand _drawCommand;

        private RubberBand _rubberBand;

        private bool _drawBrush;
        private Brush _eraseBrush;
        private TileCoord _mouseCoord;

        public EraseTool (LevelPresenter level)
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

        protected void MouseTileDownHandler (object sender, TileMouseEventArgs e)
        {
            if (!ControllersAttached || _tools.ActiveTileTool != TileToolMode.Erase) {
                _drawing = false;
                return;
            }

            if (_drawing) {
                if (e.Button == MouseButtons.Left)
                    throw new InvalidOperationException("EraseTool received MouseDown event while still active.");
                return;
            }

            if (_selecting) {
                if (e.Button == MouseButtons.Right)
                    throw new InvalidOperationException("EraseTool received MouseDown event while still active.");
                return;
            }

            MultiTileGridLayer layer = CurrentLayer;
            if (layer == null) {
                return;
            }

            switch (e.Button) {
                case MouseButtons.Left:
                    _drawing = true;
                    _drawCommand = new TileReplace2DCommand(layer);
                    break;

                case MouseButtons.Right:
                    _selecting = true;
                    _rubberBand = new RubberBand(_level.LayerControl, layer.TileWidth, layer.TileHeight);
                    _rubberBand.Brush = _eraseBrush;
                    _rubberBand.Start(new Point(e.TileLocation.X, e.TileLocation.Y));
                    break;
            }

            MouseTileMoveHandler(sender, e);
        }

        protected void MouseTileUpHandler (object sender, TileMouseEventArgs e)
        {
            if (!ControllersAttached || _tools.ActiveTileTool != TileToolMode.Erase) {
                _drawing = false;
                return;
            }

            if (_drawing && e.Button == MouseButtons.Left) {
                _drawing = false;
                _level.History.Execute(_drawCommand);
            }

            MultiTileGridLayer layer = CurrentLayer;
            if (layer == null) {
                return;
            }

            if (_selecting && e.Button == MouseButtons.Right) {
                _selecting = false;

                TileReplace2DCommand replace = new TileReplace2DCommand(layer);
                for (int x = _rubberBand.Bounds.Left; x < _rubberBand.Bounds.Right; x++) {
                    for (int y = _rubberBand.Bounds.Top; y < _rubberBand.Bounds.Bottom; y++) {
                        if (layer[new TileCoord(x, y)] != null && layer[new TileCoord(x, y)].Count > 0) {
                            replace.QueueReplacement(new TileCoord(x, y), (TileStack)null);
                            layer[new TileCoord(x, y)] = null;
                        }
                    }
                }

                _level.History.Execute(replace);

                _rubberBand.Dispose();
                _rubberBand = null;
            }
        }

        protected void MouseTileMoveHandler (object sender, TileMouseEventArgs e)
        {
            if (!ControllersAttached || _tools.ActiveTileTool != TileToolMode.Erase) {
                _drawing = false;
                return;
            }

            _mouseCoord = e.TileLocation;

            MultiTileGridLayer layer = CurrentLayer;
            if (layer == null) {
                return;
            }

            if (_drawing) {
                if (e.TileLocation.X < 0 || e.TileLocation.X >= layer.LayerWidth)
                    return;
                if (e.TileLocation.Y < 0 || e.TileLocation.Y >= layer.LayerHeight)
                    return;

                if (layer[e.TileLocation] != null && layer[e.TileLocation].Count > 0) {
                    _drawCommand.QueueReplacement(e.TileLocation, (TileStack)null);
                    layer[e.TileLocation] = null;
                }
            }

            if (_selecting) {
                int x = Math.Max(0, Math.Min(layer.LayerWidth - 1, e.TileLocation.X));
                int y = Math.Max(0, Math.Min(layer.LayerHeight - 1, e.TileLocation.Y));

                _rubberBand.End(new Point(x, y));
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
            if (!ControllersAttached || _tools.ActiveTileTool != TileToolMode.Erase) {
                _drawing = false;
                return;
            }

            if (!_drawBrush || _selecting) {
                return;
            }

            if (_eraseBrush == null) {
                _eraseBrush = _level.LayerControl.CreateSolidColorBrush(new Color(.75f, 0f, 0f, .5f));
            }

            MultiTileGridLayer layer = CurrentLayer;
            if (layer == null) {
                return;
            }

            Rectangle rect = new Rectangle(
                (int)(_mouseCoord.X * layer.TileWidth * _level.LayerControl.Zoom),
                (int)(_mouseCoord.Y * layer.TileHeight * _level.LayerControl.Zoom),
                (int)(layer.TileWidth * _level.LayerControl.Zoom),
                (int)(layer.TileHeight * _level.LayerControl.Zoom)
                );

            Draw2D.FillRectangle(e.SpriteBatch, rect, _eraseBrush);
        }
    }
}
