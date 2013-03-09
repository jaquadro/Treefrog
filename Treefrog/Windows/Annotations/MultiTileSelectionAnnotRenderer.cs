using System;
using LilyPath;
using Microsoft.Xna.Framework;
using Treefrog.Framework;
using Treefrog.Presentation.Annotations;

namespace Treefrog.Windows.Annotations
{
    public class MultiTileSelectionAnnotRenderer : DrawAnnotationRenderer
    {
        private MultiTileSelectionAnnot _data;

        public MultiTileSelectionAnnotRenderer (MultiTileSelectionAnnot data)
            : base(data)
        {
            _data = data;
        }

        public override void Render (DrawBatch spriteBatch, float zoomFactor)
        {
            if (IsDisposed)
                return;

            InitializeResources(spriteBatch.GraphicsDevice);

            if (Fill != null) {
                for (int y = _data.TileMinExtant.Y; y <= _data.TileMaxExtant.Y; y++)
                    RenderRow(spriteBatch, zoomFactor, y);
            }

            if (Outline != null) {
                for (int y = _data.TileMinExtant.Y; y <= _data.TileMaxExtant.Y + 1; y++)
                    RenderHorizontalLine(spriteBatch, zoomFactor, y);

                for (int x = _data.TileMinExtant.X; x <= _data.TileMaxExtant.X + 1; x++)
                    RenderVerticalLine(spriteBatch, zoomFactor, x);
            }
        }

        private void RenderRow (DrawBatch spriteBatch, float zoomFactor, int y)
        {
            int xStart = Int32.MinValue;
            for (int x = _data.TileMinExtant.X; x <= _data.TileMaxExtant.X; x++) {
                if (xStart == Int32.MinValue && _data.TileLocations.Contains(new TileCoord(x, y)))
                    xStart = x;

                if (xStart != Int32.MinValue && !_data.TileLocations.Contains(new TileCoord(x + 1, y))) {
                    RenderPartialRow(spriteBatch, zoomFactor, xStart, x, y);
                    xStart = Int32.MinValue;
                }
            }
        }

        private bool SingleSet (TileCoord coord1, TileCoord coord2)
        {
            return _data.TileLocations.Contains(coord1) ^ _data.TileLocations.Contains(coord2);
        }

        private void RenderHorizontalLine (DrawBatch spriteBatch, float zoomFactor, int y)
        {
            int xStart = Int32.MinValue;
            for (int x = _data.TileMinExtant.X; x <= _data.TileMaxExtant.X; x++) {
                if (xStart == Int32.MinValue && SingleSet(new TileCoord(x, y), new TileCoord(x, y - 1)))
                    xStart = x;

                if (xStart != Int32.MinValue && !SingleSet(new TileCoord(x + 1, y), new TileCoord(x + 1, y - 1))) {
                    RenderPartialHorizontalLine(spriteBatch, zoomFactor, xStart, x, y);
                    xStart = Int32.MinValue;
                }
            }
        }

        private void RenderVerticalLine (DrawBatch spriteBatch, float zoomFactor, int x)
        {
            int yStart = Int32.MinValue;
            for (int y = _data.TileMinExtant.Y; y <= _data.TileMaxExtant.Y; y++) {
                if (yStart == Int32.MinValue && SingleSet(new TileCoord(x, y), new TileCoord(x - 1, y)))
                    yStart = y;

                if (yStart != Int32.MinValue && !SingleSet(new TileCoord(x, y + 1), new TileCoord(x - 1, y + 1))) {
                    RenderPartialVerticalLine(spriteBatch, zoomFactor, x, yStart, y);
                    yStart = Int32.MinValue;
                }
            }
        }

        private void RenderPartialHorizontalLine (DrawBatch drawBatch, float zoomFactor, int x1, int x2, int y)
        {
            int top = (int)((y + _data.Offset.Y) * _data.TileHeight * zoomFactor);
            int left = (int)((x1 + _data.Offset.X) * _data.TileWidth * zoomFactor);
            int right = (int)((x2 + _data.Offset.X + 1) * _data.TileWidth * zoomFactor);

            drawBatch.DrawLine(Outline, new Vector2(left, top), new Vector2(right, top));
        }

        private void RenderPartialVerticalLine (DrawBatch drawBatch, float zoomFactor, int x, int y1, int y2)
        {
            int left = (int)((x + _data.Offset.X) * _data.TileWidth * zoomFactor);
            int top = (int)((y1 + _data.Offset.Y) * _data.TileHeight * zoomFactor);
            int bottom = (int)((y2 + _data.Offset.Y + 1) * _data.TileHeight * zoomFactor);

            drawBatch.DrawLine(Outline, new Vector2(left, top), new Vector2(left, bottom));
        }

        private void RenderPartialRow (DrawBatch drawBatch, float zoomFactor, int x1, int x2, int y)
        {
            Rectangle rect = new Rectangle(
                (int)((x1 + _data.Offset.X) * _data.TileWidth * zoomFactor),
                (int)((y + _data.Offset.Y) * _data.TileHeight * zoomFactor),
                (int)((x2 - x1 + 1) * _data.TileWidth * zoomFactor),
                (int)(_data.TileHeight * zoomFactor)
                );

            drawBatch.FillRectangle(Fill, rect);
        }
    }
}
