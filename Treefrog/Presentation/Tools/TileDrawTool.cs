using System;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using System.Collections.ObjectModel;
using Treefrog.Framework.Imaging.Drawing;
using Treefrog.Framework.Imaging;
using Treefrog.Presentation.Annotations;
using Treefrog.Presentation.Commands;

namespace Treefrog.Presentation.Tools
{
    public class TileDrawTool : TilePointerTool
    {
        private ObservableCollection<Annotation> _annots;

        private SelectionAnnot _previewMarker;

        public TileDrawTool (CommandHistory history, MultiTileGridLayer layer, ObservableCollection<Annotation> annots)
            : base(history, layer)
        {
            _annots = annots;
        }

        protected override void StartPointerSequenceCore (PointerEventInfo info, IViewport viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    StartDrawPathSequence(info);
                    break;
            }

            UpdatePointerSequence(info, viewport);
        }

        protected override void UpdatePointerSequenceCore (PointerEventInfo info, IViewport viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    UpdateDrawPathSequence(info);
                    break;
            }
        }

        protected override void EndPointerSequenceCore (PointerEventInfo info, IViewport viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    EndDrawPathSequence(info);
                    break;
            }
        }

        protected override void PointerPositionCore (PointerEventInfo info, IViewport viewport)
        {
            TileCoord location = TileLocation(info);
            if (!TileInRange(location)) {
                HidePreviewMarker();
                return;
            }

            ShowPreviewMarker(location);
        }

        protected override void PointerLeaveFieldCore ()
        {
            HidePreviewMarker();
        }

        #region Preview Marker

        private bool _previewMarkerVisible;
        private Tile _activeTile;

        private void ShowPreviewMarker (TileCoord location)
        {
            if (ActiveTile == null)
                return;

            if (ActiveTile != _activeTile) {
                HidePreviewMarker();
                _previewMarker = null;
                _activeTile = ActiveTile;
            }

            if (!_previewMarkerVisible) {
                if (_previewMarker == null) {
                    _previewMarker = new SelectionAnnot();
                    _previewMarker.Fill = new PatternBrush(ActiveTile.Pool.GetTileTexture(ActiveTile.Id), 0.5);
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

        #region Draw Path Sequence

        private TileReplace2DCommand _drawCommand;

        private void StartDrawPathSequence (PointerEventInfo info)
        {
            if (ActiveTile == null)
                return;

            _drawCommand = new TileReplace2DCommand(Layer);
        }

        private void UpdateDrawPathSequence (PointerEventInfo info)
        {
            if (ActiveTile == null)
                return;

            TileCoord location = TileLocation(info);
            if (!TileInRange(location))
                return;

            if (Layer[location] == null || Layer[location].Top != ActiveTile) {
                _drawCommand.QueueAdd(location, ActiveTile);
                Layer.AddTile(location.X, location.Y, ActiveTile);
            }
        }

        private void EndDrawPathSequence (PointerEventInfo info)
        {
            if (ActiveTile == null)
                return;

            History.Execute(_drawCommand);
        }

        #endregion
    }
}
