using System.Collections.ObjectModel;
using Treefrog.Framework;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Imaging.Drawing;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Annotations;
using Treefrog.Presentation.Commands;
using Treefrog.Framework.Model.Support;

namespace Treefrog.Presentation.Tools
{
    public class TileBrushTool : TilePointerTool
    {
        private ObservableCollection<Annotation> _annots;

        private SelectionAnnot _previewMarker;

        public TileBrushTool (CommandHistory history, MultiTileGridLayer layer, ObservableCollection<Annotation> annots)
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
        private TileBrush _activeBrush;

        private void ShowPreviewMarker (TileCoord location)
        {
            if (ActiveBrush == null)
                return;

            if (ActiveBrush != _activeBrush) {
                HidePreviewMarker();
                _previewMarker = null;
                _activeBrush = ActiveBrush;
            }

            if (!_previewMarkerVisible) {
                if (_previewMarker == null) {
                    _previewMarker = new SelectionAnnot();

                    if (ActiveBrush is DynamicTileBrush) {
                        DynamicTileBrush brush = ActiveBrush as DynamicTileBrush;
                        if (brush.PrimaryTile != null)
                            _previewMarker.Fill = new PatternBrush(
                                brush.PrimaryTile.Pool.GetTileTexture(brush.PrimaryTile.Id), 0.5);
                        else
                            _previewMarker.Outline = new Pen(new SolidColorBrush(Colors.Black));
                    }
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
            if (ActiveBrush == null)
                return;

            _drawCommand = new TileReplace2DCommand(Layer);
        }

        private void UpdateDrawPathSequence (PointerEventInfo info)
        {
            if (ActiveBrush == null)
                return;

            TileCoord location = TileLocation(info);
            if (!TileInRange(location))
                return;

            Layer.TileAdding += TileAddingHandler;
            Layer.TileRemoving += TileRemovingHandler;

            ActiveBrush.ApplyBrush(Layer, location.X, location.Y);

            Layer.TileAdding -= TileAddingHandler;
            Layer.TileRemoving -= TileRemovingHandler;
        }

        private void EndDrawPathSequence (PointerEventInfo info)
        {
            if (ActiveBrush == null)
                return;

            History.Execute(_drawCommand);
        }

        private void TileAddingHandler (object sender, LocatedTileEventArgs e)
        {
            _drawCommand.QueueAdd(new TileCoord(e.X, e.Y), e.Tile);
        }

        private void TileRemovingHandler (object sender, LocatedTileEventArgs e)
        {
            _drawCommand.QueueRemove(new TileCoord(e.X, e.Y), e.Tile);
        }

        #endregion
    }
}
