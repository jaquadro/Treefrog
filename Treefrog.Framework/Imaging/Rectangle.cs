using System;

namespace Treefrog.Framework.Imaging
{
    public struct Rectangle
    {
        public int X;
        public int Y;
        public int Height;
        public int Width;

        public int Left
        {
            get { return X; }
        }

        public int Right
        {
            get { return X + Width; }
        }

        public int Top
        {
            get { return Y; }
        }

        public int Bottom
        {
            get { return Y + Height; }
        }

        public Point Location
        {
            get { return new Point(X, Y); }
        }

        public Size Size
        {
            get { return new Size(Width, Height); }
        }

        public Rectangle (int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectangle (Point location, int width, int height)
            : this(location.X, location.Y, width, height)
        {
        }

        public Rectangle (Point location, Size size)
            : this(location.X, location.Y, size.Width, size.Height)
        {
        }

        public static bool IsAreaNegative (Rectangle rect)
        {
            return IsAreaNegativeOrEmpty(rect) && !IsAreaEmpty(rect);
        }

        public static bool IsAreaEmpty (Rectangle rect)
        {
            return rect.Width == 0 || rect.Height == 0;
        }

        public static bool IsAreaNegativeOrEmpty (Rectangle rect)
        {
            return rect.Width <= 0 || rect.Height <= 0;
        }
    }
}
