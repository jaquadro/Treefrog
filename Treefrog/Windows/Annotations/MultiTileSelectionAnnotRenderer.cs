using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Framework;
using Treefrog.Presentation.Annotations;
using Drawing = Treefrog.Framework.Imaging.Drawing;
using XnaDrawing = Amphibian.Drawing;

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

        public override void Render (SpriteBatch spriteBatch, float zoomFactor)
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

        private void RenderRow (SpriteBatch spriteBatch, float zoomFactor, int y)
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

        private void RenderHorizontalLine (SpriteBatch spriteBatch, float zoomFactor, int y)
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

        private void RenderVerticalLine (SpriteBatch spriteBatch, float zoomFactor, int x)
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

        private void RenderPartialHorizontalLine (SpriteBatch spriteBatch, float zoomFactor, int x1, int x2, int y)
        {
            int top = (int)((y + _data.Offset.Y) * _data.TileHeight * zoomFactor);
            int left = (int)((x1 + _data.Offset.X) * _data.TileWidth * zoomFactor);
            int right = (int)((x2 + _data.Offset.X + 1) * _data.TileWidth * zoomFactor);

            XnaDrawing.Draw2D.DrawLine(spriteBatch, new Point(left, top), new Point(right, top), Outline);
        }

        private void RenderPartialVerticalLine (SpriteBatch spriteBatch, float zoomFactor, int x, int y1, int y2)
        {
            int left = (int)((x + _data.Offset.X) * _data.TileWidth * zoomFactor);
            int top = (int)((y1 + _data.Offset.Y) * _data.TileHeight * zoomFactor);
            int bottom = (int)((y2 + _data.Offset.Y + 1) * _data.TileHeight * zoomFactor);

            XnaDrawing.Draw2D.DrawLine(spriteBatch, new Point(left, top), new Point(left, bottom), Outline);
        }

        private void RenderPartialRow (SpriteBatch spriteBatch, float zoomFactor, int x1, int x2, int y)
        {
            Rectangle rect = new Rectangle(
                (int)((x1 + _data.Offset.X) * _data.TileWidth * zoomFactor),
                (int)((y + _data.Offset.Y) * _data.TileHeight * zoomFactor),
                (int)((x2 - x1 + 1) * _data.TileWidth * zoomFactor),
                (int)(_data.TileHeight * zoomFactor)
                );

            XnaDrawing.Draw2D.FillRectangle(spriteBatch, rect, Fill);
        }
    }
}
