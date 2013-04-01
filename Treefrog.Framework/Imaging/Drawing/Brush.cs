
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

    public class CheckerBrush : Brush
    {
        public CheckerBrush (Color color1, Color color2, int blockSize)
            : this(color1, color2, blockSize, blockSize, 1.0)
        { }

        public CheckerBrush (Color color1, Color color2, int blockSize, double opacity)
            : this(color1, color2, blockSize, blockSize, opacity)
        { }

        public CheckerBrush (Color color1, Color color2, int blockWidth, int blockHeight)
            : this(color1, color2, blockWidth, blockHeight, 1.0)
        { }

        public CheckerBrush (Color color1, Color color2, int blockWidth, int blockHeight, double opacity)
        {
            Color1 = color1;
            Color2 = color2;
            Width = blockWidth;
            Height = blockHeight;
            Opacity = opacity;
        }

        public Color Color1 { get; protected set; }
        public Color Color2 { get; protected set; }

        public int Width { get; protected set; }
        public int Height { get; protected set; }
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
        { }
    }

    public class PrimitivePen : Pen
    {
        public PrimitivePen (SolidColorBrush brush)
            : base(brush, 1.0)
        { }

        public Color Color
        {
            get { return (Brush as SolidColorBrush).Color; }
        }
    }
}
