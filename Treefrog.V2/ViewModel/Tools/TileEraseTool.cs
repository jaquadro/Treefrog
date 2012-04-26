﻿using System;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Treefrog.V2.ViewModel.Commands;
using Treefrog.V2.ViewModel.Annotations;
using System.Collections.ObjectModel;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Imaging.Drawing;

namespace Treefrog.V2.ViewModel.Tools
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

        public override void StartPointerSequence (PointerEventInfo info)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    StartErasePathSequence(info);
                    break;
                case PointerEventType.Secondary:
                    StartEraseAreaSequence(info);
                    break;
            }

            UpdatePointerSequence(info);
        }

        public override void UpdatePointerSequence (PointerEventInfo info)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    UpdateErasePathSequence(info);
                    break;
                case PointerEventType.Secondary:
                    UpdateEraseAreaSequence(info);
                    break;
            }
        }

        public override void EndPointerSequence (PointerEventInfo info)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    EndErasePathSequence(info);
                    break;
                case PointerEventType.Secondary:
                    EndEraseAreaSequence(info);
                    break;
            }
        }

        public override void PointerPosition (PointerEventInfo info)
        {
            TileCoord location = TileLocation(info);
            if (!TileInRange(location) || _inAreaSequence) {
                HidePreviewMarker();
                return;
            }

            ShowPreviewMarker(location);
        }

        public override void PointerLeaveField ()
        {
            HidePreviewMarker();
        }

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

        private void StartEraseAreaSequence (PointerEventInfo info)
        {
            HidePreviewMarker();

            TileCoord location = TileLocation(info);

            int x = (int)(location.X * Layer.TileWidth);
            int y = (int)(location.Y * Layer.TileHeight);

            _band = new RubberBand(new Point(location.X, location.Y));
            _selection = new SelectionAnnot(new Point(x, y))
            {
                Fill = new SolidColorBrush(new Color(192, 0, 0, 128)),
                Outline = new Pen(new SolidColorBrush(new Color(192, 0, 0, 200))),
            };

            _annots.Add(_selection);
            _inAreaSequence = true;
        }

        private void UpdateEraseAreaSequence (PointerEventInfo info)
        {
            TileCoord location = TileLocation(info);

            int x = Math.Max(0, Math.Min(Layer.TilesWide - 1, location.X)) * Layer.TileWidth;
            int y = Math.Max(0, Math.Min(Layer.TilesHigh - 1, location.Y)) * Layer.TileHeight;

            _band.End = new Point(location.X, location.Y);
            Rectangle selection = _band.Selection;

            _selection.Start = new Point(selection.Left * Layer.TileWidth, selection.Top * Layer.TileHeight);
            _selection.End = new Point(selection.Right * Layer.TileWidth, selection.Bottom * Layer.TileHeight);
        }

        private void EndEraseAreaSequence (PointerEventInfo info)
        {
            Rectangle selection = _band.Selection;

            TileReplace2DCommand command = new TileReplace2DCommand(Layer);
            for (int x = selection.Left; x < selection.Right; x++) {
                for (int y = selection.Top; y < selection.Bottom; y++) {
                    command.QueueReplacement(new TileCoord(x, y), (TileStack)null);
                    Layer[new TileCoord(x, y)] = null;
                }
            }

            History.Execute(command);

            _annots.Remove(_selection);
            _inAreaSequence = false;
        }

        #endregion
    }
}
