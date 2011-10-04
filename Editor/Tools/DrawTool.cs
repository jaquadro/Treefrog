using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Editor.Model;

namespace Editor
{
    public class DrawTool : MouseTool
    {
        private TileControl1D _source;
        private TileSet2D _selectedTileSet;
        private CommandHistory _commandHistory;

        private bool _drawing;
        private Tile _sourceTile;
        private TileReplace2DCommand _drawCommand;

        public DrawTool (TileControl2D control, TileControl1D source, TileSet2D tileset, CommandHistory commandHistory)
            : base(control)
        {
            _selectedTileSet = tileset;
            _commandHistory = commandHistory;
            _source = source;
        }

        protected override void AttachHandlers ()
        {
            base.AttachHandlers();
            _source.TileSelected += SourceTileSelected;
        }

        protected override void DetachHandlers ()
        {
            base.DetachHandlers();
            _source.TileSelected -= SourceTileSelected;
        }

        protected virtual void SourceTileSelectedHandler (object sender, TileEventArgs e)
        {
            _sourceTile = e.Tile;
        }

        protected virtual void SourceTileMultiSelectedHandler (object sender, TileSelectEventArgs e)
        {
            
        }

        protected override void MouseTileDownHandler (object sender, TileMouseEventArgs e)
        {
            if (_drawing) {
                throw new InvalidOperationException("DrawTool received MouseDown event while still active.");
            }

            _drawing = true;
            _drawCommand = new TileReplace2DCommand(_selectedTileSet);
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
                if (e.TileLocation.X < 0 || e.TileLocation.X >= _selectedTileSet.TilesWide)
                    return;
                if (e.TileLocation.Y < 0 || e.TileLocation.Y >= _selectedTileSet.TilesHigh)
                    return;

                if (_selectedTileSet[e.TileLocation] != _sourceTile) {
                    _drawCommand.QueueReplacement(e.TileLocation, _sourceTile);
                    _selectedTileSet[e.TileLocation] = _sourceTile;
                }
            }
        }

        #region Private

        private void SourceTileSelected (object sender, TileEventArgs e)
        {
            SourceTileSelectedHandler(sender, e);
        }

        private void SourceTileMultiSelected (object sender, TileSelectEventArgs e)
        {
            SourceTileMultiSelectedHandler(sender, e);
        }

        #endregion
    }
}
