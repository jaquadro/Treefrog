using LilyPath.Brushes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Aux;
using Drawing = Treefrog.Framework.Imaging.Drawing;
using XnaDrawing = LilyPath;

namespace Treefrog.Render.Annotations
{
    public class BrushFactory
    {
        public static XnaDrawing.Brush Create (GraphicsDevice device, Drawing.Brush brush)
        {
            if (brush is Drawing.SolidColorBrush) {
                Drawing.SolidColorBrush scBrush = brush as Drawing.SolidColorBrush;
                Color color = new Color(scBrush.Color.R / 255f, scBrush.Color.G / 255f, scBrush.Color.B / 255f, scBrush.Color.A / 255f);
                return new XnaDrawing.SolidColorBrush(color);
            }
            else if (brush is Drawing.PatternBrush) {
                Drawing.PatternBrush pBrush = brush as Drawing.PatternBrush;
                using (Texture2D pattern = pBrush.Pattern.CreateTexture(device)) {
                    return new XnaDrawing.TextureBrush(pattern, (float)pBrush.Opacity);
                }
            }
            else if (brush is Drawing.CheckerBrush) {
                Drawing.CheckerBrush cBrush = brush as Drawing.CheckerBrush;
                Color color1 = cBrush.Color1.ToXnaColor();
                Color color2 = cBrush.Color2.ToXnaColor();
                return new CheckerBrush(device, color1, color2, cBrush.Width, cBrush.Height, (float)cBrush.Opacity);
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
            else if (pen.GetType() == typeof(Drawing.PrimitivePen)) {
                Drawing.SolidColorBrush brush = pen.Brush as Drawing.SolidColorBrush;
                return new XnaDrawing.PrimitivePen(brush.Color.ToXnaColor());
            }

            return null;
        }
    }
}
