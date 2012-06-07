using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.ViewModel.Annotations;
using Treefrog.Aux;

using Drawing = Treefrog.Framework.Imaging.Drawing;
using XnaDrawing = Amphibian.Drawing;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Treefrog.Framework;

namespace Treefrog.Controls.Annotations
{
    public abstract class AnnotationRenderer
    {
        public void Render (SpriteBatch spriteBatch)
        {
            Render(spriteBatch, 1f);
        }

        public abstract void Render (SpriteBatch spriteBatch, float zoomFactor);
    }

    public class SelectionAnnotRenderer : AnnotationRenderer
    {
        private SelectionAnnot _data;

        private XnaDrawing.Brush _fillBrush;
        private XnaDrawing.Pen _outlinePen;

        public SelectionAnnotRenderer (SelectionAnnot data)
        {
            _data = data;
        }

        public override void Render (SpriteBatch spriteBatch, float zoomFactor)
        {
            if (_fillBrush == null && _data.Fill != null)
                _fillBrush = BrushFactory.Create(spriteBatch.GraphicsDevice, _data.Fill);
            if (_outlinePen == null && _data.Outline != null)
                _outlinePen = PenFactory.Create(spriteBatch.GraphicsDevice, _data.Outline);

            Rectangle rect = new Rectangle(
                (int)(Math.Min(_data.Start.X, _data.End.X) * zoomFactor),
                (int)(Math.Min(_data.Start.Y, _data.End.Y) * zoomFactor),
                (int)(Math.Abs(_data.End.X - _data.Start.X) * zoomFactor),
                (int)(Math.Abs(_data.End.Y - _data.Start.Y) * zoomFactor)
                );

            if (_fillBrush != null)
                XnaDrawing.Draw2D.FillRectangle(spriteBatch, rect, _fillBrush);
            if (_outlinePen != null)
                XnaDrawing.Draw2D.DrawRectangle(spriteBatch, rect, _outlinePen);
        }
    }

    public class MultiTileSelectionAnnotRenderer : AnnotationRenderer
    {
        private MultiTileSelectionAnnot _data;

        private XnaDrawing.Brush _fillBrush;
        private XnaDrawing.Pen _outlinePen;

        public MultiTileSelectionAnnotRenderer (MultiTileSelectionAnnot data)
        {
            _data = data;

            _data.FillInvalidated += HandleFillInvalidated;
            _data.OutlineInvalidated += HandleOutlineInvalidated;
        }

        private void HandleFillInvalidated (object sender, EventArgs e)
        {
            _fillBrush = null;
        }

        private void HandleOutlineInvalidated (object sender, EventArgs e)
        {
            _outlinePen = null;
        }

        public override void Render (SpriteBatch spriteBatch, float zoomFactor)
        {
            if (_fillBrush == null && _data.Fill != null)
                _fillBrush = BrushFactory.Create(spriteBatch.GraphicsDevice, _data.Fill);
            if (_outlinePen == null && _data.Outline != null)
                _outlinePen = PenFactory.Create(spriteBatch.GraphicsDevice, _data.Outline);

            if (_fillBrush != null) {
                for (int y = _data.TileMinExtant.Y; y <= _data.TileMaxExtant.Y; y++)
                    RenderRow(spriteBatch, zoomFactor, y);
            }

            if (_outlinePen != null) {
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

            XnaDrawing.Draw2D.DrawLine(spriteBatch, new Point(left, top), new Point(right, top), _outlinePen);
        }

        private void RenderPartialVerticalLine (SpriteBatch spriteBatch, float zoomFactor, int x, int y1, int y2)
        {
            int left = (int)((x + _data.Offset.X) * _data.TileWidth * zoomFactor);
            int top = (int)((y1 + _data.Offset.Y) * _data.TileHeight * zoomFactor);
            int bottom = (int)((y2 + _data.Offset.Y + 1) * _data.TileHeight * zoomFactor);

            XnaDrawing.Draw2D.DrawLine(spriteBatch, new Point(left, top), new Point(left, bottom), _outlinePen);
        }

        private void RenderPartialRow (SpriteBatch spriteBatch, float zoomFactor, int x1, int x2, int y)
        {
            Rectangle rect = new Rectangle(
                (int)((x1 + _data.Offset.X) * _data.TileWidth * zoomFactor),
                (int)((y + _data.Offset.Y) * _data.TileHeight * zoomFactor),
                (int)((x2 - x1 + 1) * _data.TileWidth * zoomFactor),
                (int)(_data.TileHeight * zoomFactor)
                );

            XnaDrawing.Draw2D.FillRectangle(spriteBatch, rect, _fillBrush);
        }
    }

    public class BrushFactory 
    {
        public static XnaDrawing.Brush Create (GraphicsDevice device, Drawing.Brush brush)
        {
            if (brush.GetType() == typeof(Drawing.SolidColorBrush)) {
                Drawing.SolidColorBrush scBrush = brush as Drawing.SolidColorBrush;
                Color color = new Color(scBrush.Color.R / 255f, scBrush.Color.G / 255f, scBrush.Color.B / 255f, scBrush.Color.A / 255f);
                return new XnaDrawing.SolidColorBrush(device, color);
            }
            else if (brush.GetType() == typeof(Drawing.PatternBrush)) {
                Drawing.PatternBrush pBrush = brush as Drawing.PatternBrush;
                using (Texture2D pattern = pBrush.Pattern.CreateTexture(device)) {
                    return new XnaDrawing.PatternBrush(device, pattern, (float)pBrush.Opacity);
                }
            }

            return null;
        }
    }

    public class PenFactory
    {
        public static XnaDrawing.Pen Create (GraphicsDevice device, Drawing.Pen pen)
        {
            if (pen.GetType() == typeof(Drawing.Pen)) {
                XnaDrawing.Brush brush = BrushFactory.Create(device, pen.Brush);
                return new XnaDrawing.Pen(brush, (int)pen.Width);
            }

            return null;
        }
    }
}
