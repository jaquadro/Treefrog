using System;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using System.Collections.ObjectModel;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Imaging.Drawing;
using Treefrog.Presentation.Annotations;
using Treefrog.Presentation.Commands;

namespace Treefrog.Presentation.Tools
{
    public class TileEraseTool : TilePointerTool
    {
        private ObservableCollection<Annotation> _annots;

        private SelectionAnnot _previewMarker;

        public TileEraseTool (CommandHistory history, MultiTileGridLayer layer, ObservableCollection<Annotation> annots)
            : base(history, layer)
        {
            _annots = annots;

            _previewMarker = new SelectionAnnot(new Point(0, 0))
            {
                Fill = new SolidColorBrush(new Color(192, 0, 0, 128)),
            };

            _annots.Add(_previewMarker);
        }

        protected override void StartPointerSequenceCore (PointerEventInfo info, ILevelGeometry viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    StartErasePathSequence(info);
                    break;
                case PointerEventType.Secondary:
                    StartEraseAreaSequence(info, viewport);
                    break;
            }

            UpdatePointerSequence(info, viewport);
        }

        protected override void UpdatePointerSequenceCore (PointerEventInfo info, ILevelGeometry viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    UpdateErasePathSequence(info);
                    break;
                case PointerEventType.Secondary:
                    UpdateEraseAreaSequence(info, viewport);
                    break;
            }
        }

        protected override void EndPointerSequenceCore (PointerEventInfo info, ILevelGeometry viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    EndErasePathSequence(info);
                    break;
                case PointerEventType.Secondary:
                    EndEraseAreaSequence(info, viewport);
                    break;
            }
        }

        protected override void PointerPositionCore (PointerEventInfo info, ILevelGeometry viewport)
        {
            TileCoord location = TileLocation(info);
            if (!TileInRange(location) || _inAreaSequence) {
                HidePreviewMarker();
                return;
            }

            ShowPreviewMarker(location);
        }

        protected override void PointerLeaveFieldCore ()
        {
            HidePreviewMarker();
        }

        /*protected override void AutoScrollTick (PointerEventInfo info, ILevelGeometry viewport)
        {
            UpdateEraseAreaSequenceCommon(info, viewport);
        }*/

        #region Preview Marker

        private bool _previewMarkerVisible;

        private void ShowPreviewMarker (TileCoord location)
        {
            if (!_previewMarkerVisible) {
                if (_previewMarker == null) {
                    _previewMarker = new SelectionAnnot();
                    _previewMarker.Fill = new SolidColorBrush(new Color(192, 0, 0, 128));
                }

                _annots.Add(_previewMarker);
                _previewMarkerVisible = true;
            }

            int x = (int)(location.X * Layer.TileWidth);
            int y = (int)(location.Y * Layer.TileHeight);

            _previewMarker.Start = new Point(x, y);
            _previewMarker.End = new Point(x + Layer.TileWidth, y + Layer.TileHeight);
        }

        private void HidePreviewMarker ()
        {
            _annots.Remove(_previewMarker);
            _previewMarkerVisible = false;
        }

        #endregion

        #region Erase Path Sequence

        private TileReplace2DCommand _drawCommand;

        private void StartErasePathSequence (PointerEventInfo info)
        {
            _drawCommand = new TileReplace2DCommand(Layer);
        }

        private void UpdateErasePathSequence (PointerEventInfo info)
        {
            TileCoord location = TileLocation(info);
            if (!TileInRange(location))
                return;

            if (Layer[location] != null && Layer[location].Count > 0) {
                _drawCommand.QueueReplacement(location, (TileStack)null);
                Layer[location] = null;
            }
        }

        private void EndErasePathSequence (PointerEventInfo info)
        {
            History.Execute(_drawCommand);
        }

        #endregion

        #region Erase Area Sequence

        private bool _inAreaSequence;
        private RubberBand _band;
        private SelectionAnnot _selection;

        private void StartEraseAreaSequence (PointerEventInfo info, ILevelGeometry viewport)
        {
            HidePreviewMarker();

            TileCoord location = TileLocation(info);

            int x = (int)(location.X * Layer.TileWidth);
            int y = (int)(location.Y * Layer.TileHeight);

            _band = new RubberBand(new Point(location.X, location.Y));
            _selection = new SelectionAnnot(new Point(x, y))
            {
                Fill = new SolidColorBrush(new Color(192, 0, 0, 128)),
                //Outline = new Pen(new SolidColorBrush(new Color(192, 0, 0, 200))),
            };

            _annots.Add(_selection);
            _inAreaSequence = true;

            StartAutoScroll(info, viewport);
        }

        private void UpdateEraseAreaSequence (PointerEventInfo info, ILevelGeometry viewport)
        {
            UpdateEraseAreaSequenceCommon(info, viewport);
            UpdateAutoScroll(info, viewport);
        }

        private void UpdateEraseAreaSequenceCommon (PointerEventInfo info, ILevelGeometry viewport)
        {
            TileCoord location = TileLocation(info);

            int x = Math.Max(0, Math.Min(Layer.TilesWide - 1, location.X)) * Layer.TileWidth;
            int y = Math.Max(0, Math.Min(Layer.TilesHigh - 1, location.Y)) * Layer.TileHeight;

            _band.End = new Point(location.X, location.Y);
            Rectangle selection = _band.Selection;

            _selection.Start = new Point(selection.Left * Layer.TileWidth, selection.Top * Layer.TileHeight);
            _selection.End = new Point(selection.Right * Layer.TileWidth, selection.Bottom * Layer.TileHeight);
        }

        private void EndEraseAreaSequence (PointerEventInfo info, ILevelGeometry viewport)
        {
            Rectangle selection = _band.Selection;
            int xmin = Math.Max(Layer.TileOriginX, selection.Left);
            int ymin = Math.Max(Layer.TileOriginY, selection.Top);
            int xmax = Math.Min(Layer.TileOriginX + Layer.TilesWide, selection.Right);
            int ymax = Math.Min(Layer.TileOriginY + Layer.TilesHigh, selection.Bottom);

            TileReplace2DCommand command = new TileReplace2DCommand(Layer);
            for (int x = xmin; x < xmax; x++) {
                for (int y = ymin; y < ymax; y++) {
                    command.QueueReplacement(new TileCoord(x, y), (TileStack)null);
                    Layer[new TileCoord(x, y)] = null;
                }
            }

            History.Execute(command);

            _annots.Remove(_selection);
            _inAreaSequence = false;

            EndAutoScroll(info, viewport);
        }

        #endregion
    }
}
