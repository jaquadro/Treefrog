using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Aux;

using Drawing = Treefrog.Framework.Imaging.Drawing;
using XnaDrawing = Amphibian.Drawing;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Treefrog.Framework;
using Treefrog.Presentation.Annotations;
using Amphibian.Drawing;

namespace Treefrog.Windows.Annotations
{
    public abstract class AnnotationRenderer : IDisposable
    {
        public void Render (SpriteBatch spriteBatch)
        {
            Render(spriteBatch, 1f);
        }

        public void Render (DrawBatch drawBatch)
        {
            Render(drawBatch, 1f);
        }

        public virtual void Render (SpriteBatch spriteBatch, float zoomFactor)
        { }

        public virtual void Render (DrawBatch drawBatch, float zoomFactor)
        { }

        #region Dispose

        private bool _disposed = false;

        public void Dispose ()
        {
            if (!_disposed) {
                DisposeManaged();
                DisposeUnamanged();

                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        public bool IsDisposed
        {
            get { return _disposed; }
        }

        protected virtual void DisposeManaged ()
        {
        }

        protected virtual void DisposeUnamanged ()
        {
        }

        #endregion
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

        protected override void DisposeManaged ()
        {
            _data = null;

            if (_fillBrush != null) {
                _fillBrush.Dispose();
                _fillBrush = null;
            }

            if (_outlinePen != null) {
                _outlinePen.Dispose();
                _outlinePen = null;
            }

            base.DisposeManaged();
        }

        public override void Render (DrawBatch drawBatch, float zoomFactor)
        {
            if (IsDisposed)
                return;

            if (_fillBrush == null && _data.Fill != null)
                _fillBrush = BrushFactory.Create(drawBatch.GraphicsDevice, _data.Fill);
            if (_outlinePen == null && _data.Outline != null)
                _outlinePen = PenFactory.Create(drawBatch.GraphicsDevice, _data.Outline);

            Rectangle rect = new Rectangle(
                (int)(Math.Min(_data.Start.X, _data.End.X) * zoomFactor),
                (int)(Math.Min(_data.Start.Y, _data.End.Y) * zoomFactor),
                (int)(Math.Abs(_data.End.X - _data.Start.X) * zoomFactor),
                (int)(Math.Abs(_data.End.Y - _data.Start.Y) * zoomFactor)
                );

            if (_fillBrush != null)
                drawBatch.FillRectangle(rect, _fillBrush);
                //XnaDrawing.Draw2D.FillRectangle(spriteBatch, rect, _fillBrush);
            if (_outlinePen != null)
                drawBatch.DrawRectangle(rect, _outlinePen);
                //XnaDrawing.Draw2D.DrawRectangle(spriteBatch, rect, _outlinePen);
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

        protected override void DisposeManaged ()
        {
            if (_data != null) {
                _data.FillInvalidated -= HandleFillInvalidated;
                _data.OutlineInvalidated -= HandleOutlineInvalidated;
                _data = null;
            }

            if (_fillBrush != null) {
                _fillBrush.Dispose();
                _fillBrush = null;
            }

            if (_outlinePen != null) {
                _outlinePen.Dispose();
                _outlinePen = null;
            }

            base.DisposeManaged();
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
            if (IsDisposed)
                return;

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
            if (brush is Drawing.SolidColorBrush) {
                Drawing.SolidColorBrush scBrush = brush as Drawing.SolidColorBrush;
                Color color = new Color(scBrush.Color.R / 255f, scBrush.Color.G / 255f, scBrush.Color.B / 255f, scBrush.Color.A / 255f);
                return new XnaDrawing.SolidColorBrush(device, color);
            }
            else if (brush is Drawing.PatternBrush) {
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
