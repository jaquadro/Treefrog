using System;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using System.Collections.ObjectModel;
using Treefrog.Framework.Imaging.Drawing;
using Treefrog.Framework.Imaging;
using Treefrog.Presentation.Annotations;
using Treefrog.Presentation.Commands;
using System.Collections.Generic;
using Treefrog.Framework.Model.Support;

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
                case PointerEventType.Secondary:
                    StartDrag(info, viewport);
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
                case PointerEventType.Secondary:
                    UpdateDrag(info, viewport);
                    break;
            }
        }

        protected override void EndPointerSequenceCore (PointerEventInfo info, IViewport viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    EndDrawPathSequence(info);
                    break;
                case PointerEventType.Secondary:
                    EndDrag(info, viewport);
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

        protected override void SourceChangedCore ()
        {
            ClearPreviewMarker();
            _anonBrush = null;
        }

        #region Preview Marker

        private bool _previewMarkerVisible;
        private Tile _activeTile;

        private void ShowPreviewMarker (TileCoord location)
        {
            if (ActiveTile == null && _anonBrush == null)
                return;

            if (_anonBrush == null && ActiveTile != _activeTile) {
                ClearPreviewMarker();
                _activeTile = ActiveTile;
            }

            if (!_previewMarkerVisible) {
                if (_previewMarker == null) {
                    _previewMarker = new SelectionAnnot();
                    if (_anonBrush != null)
                        _previewMarker.Fill = BuildBrushMarker();
                    else
                        _previewMarker.Fill = BuildTileMarker();
                }

                _annots.Add(_previewMarker);
                _previewMarkerVisible = true;
            }

            if (_previewMarker != null) {
                int x = (int)(location.X * Layer.TileWidth);
                int y = (int)(location.Y * Layer.TileHeight);

                _previewMarker.Start = new Point(x, y);
                if (_anonBrush != null)
                    _previewMarker.End = new Point(x + Layer.TileWidth * _anonBrushSize.Width, y + Layer.TileHeight * _anonBrushSize.Height);
                else
                    _previewMarker.End = new Point(x + Layer.TileWidth, y + Layer.TileHeight);
            }
            else {
                HidePreviewMarker();
            }
        }

        private PatternBrush BuildTileMarker ()
        {
            return new PatternBrush(ActiveTile.Pool.GetTileTexture(ActiveTile.Id), 0.5);
        }

        private PatternBrush BuildBrushMarker ()
        {
            Rectangle extants = TileExtants(_anonBrush);
            if (Rectangle.IsAreaNegativeOrEmpty(extants))
                return null;

            TextureResource resource = new TextureResource(extants.Width * Layer.TileWidth, extants.Height * Layer.TileHeight);
            foreach (LocatedTile tile in _anonBrush) {
                int x = (tile.X - extants.X) * Layer.TileWidth;
                int y = (tile.Y - extants.Y) * Layer.TileHeight;
                if (tile.Tile != null)
                    resource.Set(tile.Tile.Pool.GetTileTexture(tile.Tile.Id), new Point(x, y));
            }

            return new PatternBrush(resource, 0.5);
        }

        private void HidePreviewMarker ()
        {
            _annots.Remove(_previewMarker);
            _previewMarkerVisible = false;
        }

        private void ClearPreviewMarker ()
        {
            HidePreviewMarker();
            _previewMarker = null;
        }

        #endregion

        #region Draw Path Sequence

        private TileReplace2DCommand _drawCommand;

        private void StartDrawPathSequence (PointerEventInfo info)
        {
            if (ActiveTile == null && _anonBrush == null)
                return;

            _drawCommand = new TileReplace2DCommand(Layer);
        }

        private void UpdateDrawPathSequence (PointerEventInfo info)
        {
            if (ActiveTile == null && _anonBrush == null)
                return;

            TileCoord location = TileLocation(info);
            if (!TileInRange(location))
                return;

            if (_anonBrush != null) {
                foreach (LocatedTile tile in _anonBrush) {
                    TileCoord tileloc = new TileCoord(location.X + tile.X, location.Y + tile.Y);
                    if (Layer[tileloc] == null || Layer[tileloc].Top != tile.Tile) {
                        _drawCommand.QueueAdd(tileloc, tile.Tile);
                        Layer.AddTile(tileloc.X, tileloc.Y, tile.Tile);
                    }
                }
            }
            else if (Layer[location] == null || Layer[location].Top != ActiveTile) {
                _drawCommand.QueueAdd(location, ActiveTile);
                Layer.AddTile(location.X, location.Y, ActiveTile);
            }
        }

        private void EndDrawPathSequence (PointerEventInfo info)
        {
            if (ActiveTile == null && _anonBrush == null)
                return;

            History.Execute(_drawCommand);
        }

        #endregion

        #region Select Region Sequence

        private List<LocatedTile> _anonBrush;
        private Size _anonBrushSize;

        private RubberBand2 _band;
        private SelectionAnnot _selectionAnnot;

        private void StartDrag (PointerEventInfo info, IViewport viewport)
        {
            ClearPreviewMarker();
            _anonBrush = null;

            TileCoord location = TileLocation(info);

            int x = (int)(location.X * Layer.TileWidth);
            int y = (int)(location.Y * Layer.TileHeight);

            _band = new RubberBand2(new Point(location.X, location.Y));
            _selectionAnnot = new SelectionAnnot(new Point((int)info.X, (int)info.Y)) {
                Fill = new SolidColorBrush(new Color(76, 178, 255, 128)),
            };

            _annots.Add(_selectionAnnot);

            //StartAutoScroll(info, viewport);
        }

        private void UpdateDrag (PointerEventInfo info, IViewport viewport)
        {
            TileCoord location = TileLocation(info);

            _band.End = new Point(location.X, location.Y);
            Rectangle selection = _band.Selection;

            _selectionAnnot.Start = new Point(selection.Left * Layer.TileWidth, selection.Top * Layer.TileHeight);
            _selectionAnnot.End = new Point(selection.Right * Layer.TileWidth, selection.Bottom * Layer.TileHeight);

            //UpdateAutoScroll(info, viewport);
        }

        private void EndDrag (PointerEventInfo info, IViewport viewport)
        {
            Rectangle selection = ClampSelection(_band.Selection);

            _anonBrush = new List<LocatedTile>();
            foreach (LocatedTile tile in Layer.TilesAt(_band.Selection))
                _anonBrush.Add(new LocatedTile(tile.Tile, tile.X - _band.Selection.Left, tile.Y - _band.Selection.Top));

            _anonBrushSize = TileExtants(_anonBrush).Size;
            
            _annots.Remove(_selectionAnnot);

            //EndAutoScroll(info, viewport);
        }

        private IEnumerable<TileCoord> TileCoordsFromRegion (Rectangle region)
        {
            for (int y = region.Top; y < region.Bottom; y++)
                for (int x = region.Left; x < region.Right; x++)
                    yield return new TileCoord(x, y);
        }

        #endregion

        private Rectangle ClampSelection (Rectangle rect)
        {
            int x = Math.Max(rect.X, Layer.TileOriginX);
            int y = Math.Max(rect.Y, Layer.TileOriginY);

            return new Rectangle(x, y,
                Math.Min(rect.Width, Layer.TilesWide - x),
                Math.Min(rect.Height, Layer.TilesHigh - y)
                );
        }

        private Rectangle TileExtants (IEnumerable<LocatedTile> tiles)
        {
            int xmin = int.MaxValue;
            int ymin = int.MaxValue;
            int xmax = int.MinValue;
            int ymax = int.MinValue;

            foreach (LocatedTile tile in _anonBrush) {
                if (tile.X < xmin)
                    xmin = tile.X;
                if (tile.X > xmax)
                    xmax = tile.X;
                if (tile.Y < ymin)
                    ymin = tile.Y;
                if (tile.Y > ymax)
                    ymax = tile.Y;
            }

            int width = Math.Max(0, xmax - xmin + 1);
            int height = Math.Max(0, ymax - ymin + 1);

            if (width <= 0 || height <= 0)
                return Rectangle.Empty;

            return new Rectangle(xmin, ymin, width, height);
        }
    }
}
