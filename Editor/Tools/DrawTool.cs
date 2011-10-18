using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Editor.Model;
using Editor.Model.Controls;
using Editor.Controls;
using Microsoft.Xna.Framework;

namespace Editor
{
    public class DrawTool : TileMouseTool
    {
        private MultiTileControlLayer _control;
        private TileControlLayer _source;
        //private TileSet2D _selectedTileSet;
        private CommandHistory _commandHistory;

        private bool _drawing;
        private Tile _sourceTile;
        private TileReplace2DCommand _drawCommand;

        private RubberBand _rubberBand;

        public DrawTool (MultiTileControlLayer control, TileControlLayer source, CommandHistory commandHistory)
            : base(control)
        {
            _control = control;
            _commandHistory = commandHistory;
            _source = source;
        }

        protected override void AttachHandlers ()
        {
            base.AttachHandlers();
            _source.MouseTileClick += SourceTileSelected;

            if (_rubberBand != null) {
                _rubberBand.Visible = true;
            }
        }

        protected override void DetachHandlers ()
        {
            base.DetachHandlers();
            _source.MouseTileClick -= SourceTileSelected;

            if (_rubberBand != null) {
                _rubberBand.Visible = false;
            }
        }

        protected virtual void SourceTileSelectedHandler (object sender, TileMouseEventArgs e)
        {
            _sourceTile = e.Tile;

            if (_rubberBand != null) {
                _rubberBand.Dispose();
            }

            if (_sourceTile != null) {
                _rubberBand = new RubberBand(_source.Control, _source.Layer.TileWidth, _source.Layer.TileHeight);
                _rubberBand.FillBrush = _control.Control.CreateSolidColorBrush(new Color(.5f, .75f, 1f, .6f));
                _rubberBand.Start(new Point(e.TileLocation.X, e.TileLocation.Y));
                _rubberBand.End(new Point(e.TileLocation.X, e.TileLocation.Y));
            }
        }

        /*protected virtual void SourceTileMultiSelectedHandler (object sender, TileSelectEventArgs e)
        {
            
        }*/

        protected override void MouseTileDownHandler (object sender, TileMouseEventArgs e)
        {
            if (_drawing) {
                throw new InvalidOperationException("DrawTool received MouseDown event while still active.");
            }

            if (_sourceTile == null) {
                return;
            }

            _drawing = true;
            _drawCommand = new TileReplace2DCommand(_control.Layer);
            MouseTileMoveHandler(sender, e);
        }

        protected override void MouseTileUpHandler (object sender, TileMouseEventArgs e)
        {
            if (!_drawing) {
                return;
            }

            _drawing = false;
            _commandHistory.Execute(_drawCommand);
        }

        protected override void MouseTileMoveHandler (object sender, TileMouseEventArgs e)
        {
            if (_drawing) {
                if (e.TileLocation.X < 0 || e.TileLocation.X >= _control.Layer.LayerWidth)
                    return;
                if (e.TileLocation.Y < 0 || e.TileLocation.Y >= _control.Layer.LayerHeight)
                    return;

                if (_control.Layer[e.TileLocation] == null || _control.Layer[e.TileLocation].Top != _sourceTile) {
                    _drawCommand.QueueAdd(e.TileLocation, _sourceTile);
                    _control.Layer.AddTile(e.TileLocation.X, e.TileLocation.Y, _sourceTile);
                }
            }
        }

        #region Private

        private void SourceTileSelected (object sender, TileMouseEventArgs e)
        {
            SourceTileSelectedHandler(sender, e);
        }

        /*private void SourceTileMultiSelected (object sender, TileSelectEventArgs e)
        {
            SourceTileMultiSelectedHandler(sender, e);
        }*/

        #endregion
    }
}
