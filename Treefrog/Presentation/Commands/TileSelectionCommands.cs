using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using Treefrog.Framework;
using Treefrog.Framework.Imaging;
using Treefrog.Prseentation.Tools;

namespace Treefrog.Presentation.Commands
{
    public interface ITileSelectionLayer
    {
        bool HasSelection { get; }
        TileCoord TileSelectionOffset { get; }

        TileSelection TileSelection { get; }

        void CreateTileSelection ();
        void CreateFloatingSelection ();
        void DeleteTileSelection ();
        void RestoreTileSelection (TileSelection selection);

        void ClearTileSelection ();
        void SetTileSelection (IEnumerable<TileCoord> tileLocations);
        void AddTilesToSelection (IEnumerable<TileCoord> tileLocations);
        void RemoveTilesFromSelection (IEnumerable<TileCoord> tileLocations);

        void FloatSelection ();
        void DefloatSelection ();

        void SetSelectionOffset (TileCoord offset);
        void MoveSelectionByOffset (TileCoord offset);

        bool TileSelectionCoverageAt (TileCoord coord);
    }

    public class FloatTileSelectionCommand : Command
    {
        private MultiTileGridLayer _tileSource;
        private TileReplace2DCommand _command;

        private ITileSelectionLayer _selectLayer;

        public FloatTileSelectionCommand (MultiTileGridLayer source, ITileSelectionLayer selectLayer)
        {
            _tileSource = source;
            _command = new TileReplace2DCommand(source);

            _selectLayer = selectLayer;
        }

        public override void Execute ()
        {
            TileSelection selection = _selectLayer.TileSelection;

            foreach (KeyValuePair<TileCoord, TileStack> item in selection.Tiles) {
                TileCoord destCoord = new TileCoord(item.Key.X + selection.Offset.X, item.Key.Y + selection.Offset.Y);
                _command.QueueReplacement(destCoord, (TileStack)null);
            }

            _command.Execute();

            _selectLayer.FloatSelection();
        }

        public override void Undo ()
        {
            _command.Undo();

            TileSelection selection = _selectLayer.TileSelection;
            if (selection != null)
                selection.Defloat();
        }

        public override void Redo ()
        {
            _command.Redo();

            TileSelection selection = _selectLayer.TileSelection;
            if (selection != null)
                selection.Float();
        }
    }

    public class DefloatTileSelectionCommand : Command
    {
        private MultiTileGridLayer _tileSource;
        private TileReplace2DCommand _command;

        private ITileSelectionLayer _selectLayer;

        public DefloatTileSelectionCommand (MultiTileGridLayer source, ITileSelectionLayer selectLayer)
        {
            _tileSource = source;
            _command = new TileReplace2DCommand(source);

            _selectLayer = selectLayer;
        }

        public override void Execute ()
        {
            TileSelection selection = _selectLayer.TileSelection;

            foreach (KeyValuePair<TileCoord, TileStack> item in selection.Tiles) {
                TileCoord destCoord = new TileCoord(item.Key.X + selection.Offset.X, item.Key.Y + selection.Offset.Y);
                _command.QueueAdd(destCoord, item.Value);
            }

            _command.Execute();

            _selectLayer.DefloatSelection();
        }

        public override void Undo ()
        {
            _command.Undo();

            TileSelection selection = _selectLayer.TileSelection;
            if (selection != null)
                selection.Float();
        }

        public override void Redo ()
        {
            _command.Redo();

            TileSelection selection = _selectLayer.TileSelection;
            if (selection != null)
                selection.Defloat();
        }
    }

    public class CreateTileSelectionCommand : Command
    {
        private ITileSelectionLayer _selectLayer;

        public CreateTileSelectionCommand (ITileSelectionLayer selectLayer)
        {
            _selectLayer = selectLayer;
        }

        public override void Execute ()
        {
            _selectLayer.CreateTileSelection();
        }

        public override void Undo ()
        {
            _selectLayer.DeleteTileSelection();
        }

        public override void Redo ()
        {
            _selectLayer.CreateTileSelection();
        }
    }

    public class ModifyNewTileSelectionCommand : Command
    {
        private ITileSelectionLayer _selectLayer;
        private List<TileCoord> _diff;

        public ModifyNewTileSelectionCommand (ITileSelectionLayer selectLayer)
        {
            _selectLayer = selectLayer;
            _diff = new List<TileCoord>();
        }

        public void AddLocations (IEnumerable<TileCoord> locations)
        {
            foreach (TileCoord coord in locations)
                _diff.Add(coord);
        }

        public override void Execute ()
        {
            _selectLayer.AddTilesToSelection(_diff);
        }

        public override void Undo ()
        {
            _selectLayer.RemoveTilesFromSelection(_diff);
        }

        public override void Redo ()
        {
            _selectLayer.AddTilesToSelection(_diff);
        }
    }

    public class ModifyAddTileSelectionCommand : Command
    {
        private ITileSelectionLayer _selectLayer;
        private List<TileCoord> _diff;

        public ModifyAddTileSelectionCommand (ITileSelectionLayer selectLayer)
        {
            _selectLayer = selectLayer;
            _diff = new List<TileCoord>();
        }

        public void AddLocations (IEnumerable<TileCoord> locations)
        {
            foreach (TileCoord coord in locations)
                if (!_selectLayer.HasSelection || !_selectLayer.TileSelection.CoverageAt(coord))
                    _diff.Add(coord);
        }

        public override void Execute ()
        {
            _selectLayer.AddTilesToSelection(_diff);
        }

        public override void Undo ()
        {
            _selectLayer.RemoveTilesFromSelection(_diff);
        }

        public override void Redo ()
        {
            _selectLayer.AddTilesToSelection(_diff);
        }
    }

    public class ModifyRemoveTileSelectionCommand : Command
    {
        private ITileSelectionLayer _selectLayer;
        private List<TileCoord> _diff;

        public ModifyRemoveTileSelectionCommand (ITileSelectionLayer selectLayer)
        {
            _selectLayer = selectLayer;
            _diff = new List<TileCoord>();
        }

        public void AddLocations (IEnumerable<TileCoord> locations)
        {
            foreach (TileCoord coord in locations)
                if (_selectLayer.HasSelection && _selectLayer.TileSelection.CoverageAt(coord))
                    _diff.Add(coord);
        }

        public override void Execute ()
        {
            _selectLayer.RemoveTilesFromSelection(_diff);
        }

        public override void Undo ()
        {
            _selectLayer.AddTilesToSelection(_diff);
        }

        public override void Redo ()
        {
            _selectLayer.RemoveTilesFromSelection(_diff);
        }
    }

    public class PasteFloatingSelectionCommand : Command
    {
        private ITileSelectionLayer _selectLayer;
        private TileSelection _selection;
        private TileCoord _centerOffset;

        public PasteFloatingSelectionCommand (ITileSelectionLayer selectLayer, TileSelection selection, TileCoord centerOffset)
        {
            _selectLayer = selectLayer;
            _selection = selection;
            _centerOffset = centerOffset;
        }

        public override void Execute ()
        {
            _selectLayer.RestoreTileSelection(_selection);
            _selectLayer.FloatSelection();

            Rectangle bounds = _selectLayer.TileSelection.Bounds;
            TileCoord offset = new TileCoord(_centerOffset.X - bounds.Center.X, _centerOffset.Y - bounds.Center.Y);
            _selectLayer.SetSelectionOffset(offset);
        }

        public override void Undo ()
        {
            _selectLayer.DeleteTileSelection();
        }

        public override void Redo ()
        {
            _selectLayer.RestoreTileSelection(_selection);
            _selectLayer.FloatSelection();

            Rectangle bounds = _selectLayer.TileSelection.Bounds;
            TileCoord offset = new TileCoord(_centerOffset.X - bounds.Center.X, _centerOffset.Y - bounds.Center.Y);
            _selectLayer.SetSelectionOffset(offset);
        }
    }

    public class DeleteTileSelectionCommand : Command
    {
        private ITileSelectionLayer _selectLayer;
        private TileSelection _selection;

        public DeleteTileSelectionCommand (ITileSelectionLayer selectLayer)
        {
            _selectLayer = selectLayer;
        }

        public override void Execute ()
        {
            _selection = new TileSelection(_selectLayer.TileSelection);

            _selectLayer.DeleteTileSelection();
        }

        public override void Undo ()
        {
            _selectLayer.RestoreTileSelection(_selection);
        }

        public override void Redo ()
        {
            _selectLayer.DeleteTileSelection();
        }
    }

    public class MoveTileSelectionCommand : Command
    {
        private ITileSelectionLayer _selectLayer;
        private TileCoord _prev;
        private TileCoord _new;

        public MoveTileSelectionCommand (ITileSelectionLayer selectLayer, TileCoord previousLocation, TileCoord newLocation)
        {
            _selectLayer = selectLayer;
            _prev = previousLocation;
            _new = newLocation;
        }

        public override void Execute ()
        {
            _selectLayer.TileSelection.Offset = _new;
        }

        public override void Undo ()
        {
            _selectLayer.TileSelection.Offset = _prev;
        }

        public override void Redo ()
        {
            _selectLayer.TileSelection.Offset = _new;
        }
    }
}
