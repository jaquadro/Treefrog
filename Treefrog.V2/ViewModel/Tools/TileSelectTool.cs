using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.ViewModel.Commands;
using Treefrog.Framework.Model;
using System.Collections.ObjectModel;
using Treefrog.ViewModel.Annotations;
using System.Windows.Input;
using Treefrog.Framework;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Imaging.Drawing;

namespace Treefrog.ViewModel.Tools
{
    public class TileSelectTool : TilePointerTool
    {
        private ObservableCollection<Annotation> _annots;

        public TileSelectTool (CommandHistory history, MultiTileGridLayer layer, ObservableCollection<Annotation> annots)
            : base(history, layer)
        {
            _annots = annots;
        }

        protected override void StartPointerSequenceCore (PointerEventInfo info)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    StartSelectTilesSequence(info);
                    break;
                case PointerEventType.Secondary:
                    ClearSelected();
                    break;
            }

            UpdatePointerSequence(info);
        }

        protected override void UpdatePointerSequenceCore (PointerEventInfo info)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    UpdateSelectTilesSequence(info);
                    break;
            }
        }

        protected override void EndPointerSequenceCore (PointerEventInfo info)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    EndSelectTilesSequence(info);
                    break;
            }
        }

        private enum UpdateAction
        {
            None,
            Move,
            Box,
        }

        private HashSet<TileCoord> _selectedTiles = new HashSet<TileCoord>();
        private UpdateAction _action;

        private void StartSelectTilesSequence (PointerEventInfo info)
        {
            bool controlKey = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
            bool shiftKey = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

            if (shiftKey) {
                StartDragAdd(info);
                return;
            }
            
            if (controlKey) {
                StartDragRemove(info);
                return;
            }

            TileCoord location = TileLocation(info);
            if (_tileAnnot != null)
                location = new TileCoord(location.X - _tileAnnot.Offset.X, location.Y - _tileAnnot.Offset.Y);

            if (!_selectedTiles.Contains(location)) {
                StartDragNew(info);
            }
            else {
                StartMove(info);
            }
        }

        private void UpdateSelectTilesSequence (PointerEventInfo info)
        {
            switch (_action) {
                case UpdateAction.Move:
                    UpdateMove(info);
                    break;
                case UpdateAction.Box:
                    UpdateDrag(info);
                    break;
            }
        }

        private void EndSelectTilesSequence (PointerEventInfo info)
        {
            switch (_action) {
                case UpdateAction.Move:
                    EndMove(info);
                    break;
                case UpdateAction.Box:
                    EndDrag(info);
                    break;
            }
        }

        private Point _initialLocation;
        private TileCoord _initialOffset;

        #region Move Actions

        private void StartMove (PointerEventInfo info)
        {
            _initialLocation = new Point((int)info.X - Layer.TileWidth / 2, (int)info.Y - Layer.TileHeight / 2);
            _initialOffset = _tileAnnot.Offset;

            _tileAnnot.Fill = new SolidColorBrush(new Color(128, 96, 216, 96));
            _tileAnnot.Outline = new Pen(new SolidColorBrush(new Color(80, 50, 180, 200)), 2);

            _action = UpdateAction.Move;
        }

        private void UpdateMove (PointerEventInfo info)
        {
            int diffx = (int)info.X - _initialLocation.X;
            int diffy = (int)info.Y - _initialLocation.Y;

            if (diffx == 0 && diffy == 0)
                return;

            int tileDiffX = _initialOffset.X + (int)Math.Floor((double)diffx / Layer.TileWidth);
            int tileDiffY = _initialOffset.Y + (int)Math.Floor((double)diffy / Layer.TileHeight);

            _tileAnnot.Offset = new TileCoord(tileDiffX, tileDiffY);
        }

        private void EndMove (PointerEventInfo info)
        {
            _action = UpdateAction.None;
        }

        #endregion

        #region Select Box Actions

        private enum MergeAction
        {
            Add,
            Remove,
        }

        private RubberBand _band;
        private SelectionAnnot _selection;
        private MergeAction _mergeAction;

        private MultiTileSelectionAnnot _tileAnnot;

        

        private void StartDragNew (PointerEventInfo info)
        {
            ClearSelected();
            StartDragAdd(info);
        }

        private void StartDragAdd (PointerEventInfo info)
        {
            StartDrag(info, MergeAction.Add);
        }

        private void StartDragRemove (PointerEventInfo info)
        {
            StartDrag(info, MergeAction.Remove);
        }

        private void StartDrag (PointerEventInfo info, MergeAction action)
        {
            TileCoord location = TileLocation(info);

            int x = (int)(location.X * Layer.TileWidth);
            int y = (int)(location.Y * Layer.TileHeight);

            _band = new RubberBand(new Point(location.X, location.Y));
            _selection = new SelectionAnnot(new Point((int)info.X, (int)info.Y))
            {
                Fill = new SolidColorBrush(new Color(64, 96, 216, 96)),
            };

            _annots.Add(_selection);
            _action = UpdateAction.Box;
            _mergeAction = action;
        }

        private void UpdateDrag (PointerEventInfo info)
        {
            TileCoord location = TileLocation(info);

            _band.End = new Point(location.X, location.Y);
            Rectangle selection = _band.Selection;

            _selection.Start = new Point(selection.Left * Layer.TileWidth, selection.Top * Layer.TileHeight);
            _selection.End = new Point(selection.Right * Layer.TileWidth, selection.Bottom * Layer.TileHeight);
        }

        private void EndDrag (PointerEventInfo info)
        {
            Rectangle selection = ClampSelection(_band.Selection);

            if (_tileAnnot == null) {
                _tileAnnot = new MultiTileSelectionAnnot()
                {
                    Fill = new SolidColorBrush(new Color(64, 96, 216, 96)),
                    Outline = new Pen(new SolidColorBrush(new Color(40, 70, 190, 200)), 2),
                    TileWidth = Layer.TileWidth,
                    TileHeight = Layer.TileHeight,
                };
                _annots.Add(_tileAnnot);
            }

            for (int x = selection.Left; x < selection.Right; x++) {
                for (int y = selection.Top; y < selection.Bottom; y++) {
                    TileCoord coord = new TileCoord(x, y);

                    switch (_mergeAction) {
                        case MergeAction.Add:
                            _selectedTiles.Add(coord);
                            _tileAnnot.AddTileLocation(coord);
                            break;
                        case MergeAction.Remove:
                            _selectedTiles.Remove(coord);
                            _tileAnnot.RemoveTileLocation(coord);
                            break;
                    }
                }
            }

            _annots.Remove(_selection);
            _action = UpdateAction.None;
        }

        #endregion

        private Rectangle ClampSelection (Rectangle rect)
        {
            int x = Math.Max(rect.X, 0);
            int y = Math.Max(rect.Y, 0);

            return new Rectangle(x, y, 
                Math.Min(rect.Width, Layer.TilesWide - x),
                Math.Min(rect.Height, Layer.TilesHigh - y)
                );
        }

        private void ClearSelected ()
        {
            if (_selectedTiles == null || _selectedTiles.Count == 0)
                return;

            _annots.Remove(_selection);
            _annots.Remove(_tileAnnot);

            _selectedTiles.Clear();
            _tileAnnot = null;

            //OnCanDeleteChanged(EventArgs.Empty);
            //OnCanSelectNoneChanged(EventArgs.Empty);
        }

        #region Presentation Control

        public HashSet<TileCoord> FloatingTiles
        {
            get { return _selectedTiles; }
        }

        public TileCoord FloatingOffset
        {
            get { return (_tileAnnot != null) ? _tileAnnot.Offset : new TileCoord(0, 0); }
        }

        #endregion
    }
}
