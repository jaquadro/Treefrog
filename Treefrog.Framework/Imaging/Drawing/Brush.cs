
namespace Treefrog.Framework.Imaging.Drawing
{
    public abstract class Brush
    {
        public double Opacity { get; protected set; }
    }

    public class SolidColorBrush : Brush
    {
        public SolidColorBrush (Color color)
        {
            Color = color;
            Opacity = color.A / 255.0;
        }

        public Color Color { get; protected set; }
    }

    public class PatternBrush : Brush
    {
        public PatternBrush (TextureResource pattern, double opacity)
        {
            Pattern = pattern.Crop(pattern.Bounds);
            Opacity = opacity;
        }

        public PatternBrush (TextureResource pattern)
            : this (pattern, 1.0)
        {
        }

        public TextureResource Pattern { get; protected set; }
    }

    public class StippleBrush : PatternBrush
    {
        public StippleBrush (bool[,] pattern, Color color, double opacity)
            : base(ConvertStipplePattern(pattern, color), opacity)
        {
        }

        public StippleBrush (bool[,] pattern, Color color)
            : base(ConvertStipplePattern(pattern, color))
        {
        }

        private static TextureResource ConvertStipplePattern (bool[,] pattern, Color color)
        {
            int width = pattern.GetLength(0);
            int height = pattern.GetLength(1);

            TextureResource image = new TextureResource(width, height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (pattern[y, x])
                        image[y, x] = color;

            return image;
        }
    }

    public class Pen
    {
        public Brush Brush { get; protected set; }
        public double Width { get; protected set; }

        public Pen (Brush brush, double width)
        {
            Brush = brush;
            Width = width;
        }

        public Pen (Brush brush)
            : this(brush, 1.0)
        {
        }
    }
}
