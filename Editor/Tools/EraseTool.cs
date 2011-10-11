using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Editor.Model;
using Editor.Controls;
using Microsoft.Xna.Framework;
using Editor.Model.Controls;

namespace Editor
{
    public class EraseTool : TileMouseTool
    {
        private TileSet2D _selectedTileSet;
        private CommandHistory _commandHistory;

        private bool _drawing;
        private bool _selecting;
        private TileReplace2DCommand _drawCommand;

        private MultiTileControlLayer _control;
        private RubberBand _rubberBand;

        public EraseTool (MultiTileControlLayer control, TileSet2D tileset, CommandHistory commandHistory)
            : base(control)
        {
            _control = control;
            _selectedTileSet = tileset;
            _commandHistory = commandHistory;
        }

        protected override void MouseTileDownHandler (object sender, TileMouseEventArgs e)
        {
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

            switch (e.Button) {
                case MouseButtons.Left:
                    _drawing = true;
                    _drawCommand = new TileReplace2DCommand(_selectedTileSet);
                    break;

                case MouseButtons.Right:
                    _selecting = true;
                    _rubberBand = new RubberBand(_control.Control, _control.Layer.TileWidth, _control.Layer.TileHeight);
                    _rubberBand.FillBrush = _control.Control.CreateSolidColorBrush(new Color(1f, .25f, .25f, .5f));
                    _rubberBand.Start(new Point(e.TileLocation.X, e.TileLocation.Y));
                    break;
            }

            MouseTileMoveHandler(sender, e);
        }

        protected override void MouseTileUpHandler (object sender, TileMouseEventArgs e)
        {
            if (_drawing && e.Button == MouseButtons.Left) {
                _drawing = false;
                _commandHistory.Execute(_drawCommand);
            }

            if (_selecting && e.Button == MouseButtons.Right) {
                _selecting = false;

                TileReplace2DCommand replace = new TileReplace2DCommand(_selectedTileSet);
                for (int x = _rubberBand.Bounds.Left; x < _rubberBand.Bounds.Right; x++) {
                    for (int y = _rubberBand.Bounds.Top; y < _rubberBand.Bounds.Bottom; y++) {
                        if (_selectedTileSet[new TileCoord(x, y)] != null) {
                            replace.QueueReplacement(new TileCoord(x, y), null);
                            _selectedTileSet[new TileCoord(x, y)] = null;
                        }
                    }
                }

                _commandHistory.Execute(replace);

                _rubberBand.Dispose();
                _rubberBand = null;
            }
        }

        protected override void MouseTileMoveHandler (object sender, TileMouseEventArgs e)
        {
            if (_drawing) {
                if (e.TileLocation.X < 0 || e.TileLocation.X >= _control.Layer.LayerWidth)
                    return;
                if (e.TileLocation.Y < 0 || e.TileLocation.Y >= _control.Layer.LayerHeight)
                    return;

                if (_selectedTileSet[e.TileLocation] != null) {
                    _drawCommand.QueueReplacement(e.TileLocation, null);
                    _selectedTileSet[e.TileLocation] = null;
                }
            }

            if (_selecting) {
                int x = Math.Max(0, Math.Min(_control.Layer.LayerWidth - 1, e.TileLocation.X));
                int y = Math.Max(0, Math.Min(_control.Layer.LayerHeight - 1, e.TileLocation.Y));

                _rubberBand.End(new Point(x, y));
            }
        }
    }
}
