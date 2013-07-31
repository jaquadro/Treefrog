using TF = Treefrog.Framework.Imaging;
using Xna = Microsoft.Xna.Framework;

namespace Treefrog.Render
{
    public static class ToXnaInterop
    {
        public static Xna.Rectangle ToXnaRectangle (this TF.Rectangle rect)
        {
            return new Xna.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static Xna.Point ToXnaPoint (this TF.Point point)
        {
            return new Xna.Point(point.X, point.Y);
        }

        public static Xna.Vector2 ToXnaVector2 (this TF.Point point)
        {
            return new Xna.Vector2(point.X, point.Y);
        }

        public static Xna.Color ToXnaColor (this TF.Color color)
        {
            return new Xna.Color(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }
    }
}
