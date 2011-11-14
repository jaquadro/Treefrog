using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Editor.Model.Controls;
using Editor.Controls;
using Microsoft.Xna.Framework;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Editor.Tools;
using Editor.A.Presentation;

namespace Editor
{
    public class DrawTool
    {
        private CommandHistory _commandHistory;

        private ITilePoolListPresenter _pool;
        private ILevelToolsPresenter _tools;
        private LevelPresenter _level;

        private bool _drawing;
        private TileReplace2DCommand _drawCommand;

        public DrawTool (LevelPresenter level)
        {
            _level = level;
            _level.PreSyncLayerSelection += DetachHandlers;
            _level.SyncLayerSelection += AttachHandlers;

            _commandHistory = new CommandHistory();
        }

        public void BindTileSourceController (ITilePoolListPresenter controller)
        {
            _pool = controller;
        }

        public void BindLevelToolsController (ILevelToolsPresenter controller)
        {
            _tools = controller;
        }

        protected void AttachHandlers (object sender, EventArgs e)
        {
            TileControlLayer control = CurrentControlLayer;
            if (control != null) {
                control.MouseTileDown += MouseTileDownHandler;
                control.MouseTileMove += MouseTileMoveHandler;
                control.MouseTileUp += MouseTileUpHandler;
            }
        }

        protected void DetachHandlers (object sender, EventArgs e)
        {
            TileControlLayer control = CurrentControlLayer;
            if (control != null) {
                control.MouseTileDown -= MouseTileDownHandler;
                control.MouseTileMove -= MouseTileMoveHandler;
                control.MouseTileUp -= MouseTileUpHandler;
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
                _drawing = false;
                return;
            }

            if (_drawing) {
                throw new InvalidOperationException("DrawTool received MouseDown event while still active.");
            }

            MultiTileGridLayer layer = CurrentLayer;
            if (layer == null) {
                return;
            }

            if (_pool.SelectedTile == null) {
                return;
            }

            _drawing = true;
            _drawCommand = new TileReplace2DCommand(layer);
            MouseTileMoveHandler(sender, e);
        }

        protected virtual void MouseTileUpHandler (object sender, TileMouseEventArgs e)
        {
            if (!ControllersAttached || _tools.ActiveTileTool != TileToolMode.Draw) {
                _drawing = false;
                return;
            }

            if (!_drawing) {
                return;
            }

            _drawing = false;
            _commandHistory.Execute(_drawCommand);
        }

        protected virtual void MouseTileMoveHandler (object sender, TileMouseEventArgs e)
        {
            if (!ControllersAttached || _tools.ActiveTileTool != TileToolMode.Draw) {
                _drawing = false;
                return;
            }

            if (_drawing) {
                MultiTileGridLayer layer = CurrentLayer;
                if (layer == null) {
                    return;
                }

                if (e.TileLocation.X < 0 || e.TileLocation.X >= layer.LayerWidth)
                    return;
                if (e.TileLocation.Y < 0 || e.TileLocation.Y >= layer.LayerHeight)
                    return;

                if (layer[e.TileLocation] == null || layer[e.TileLocation].Top != _pool.SelectedTile) {
                    _drawCommand.QueueAdd(e.TileLocation, _pool.SelectedTile);
                    layer.AddTile(e.TileLocation.X, e.TileLocation.Y, _pool.SelectedTile);
                }
            }
        }
    }
}
