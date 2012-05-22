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
