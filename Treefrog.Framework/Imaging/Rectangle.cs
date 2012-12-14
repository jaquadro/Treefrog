using System;
using System.Globalization;
using System.Xml.Serialization;

namespace Treefrog.Framework.Imaging
{
    [XmlType]
    public struct Rectangle
    {
        [XmlAttribute]
        public int X;

        [XmlAttribute]
        public int Y;

        [XmlAttribute]
        public int Height;

        [XmlAttribute]
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

        public Point Center
        {
            get { return new Point(X + Width / 2, Y + Height / 2); }
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

        public bool Contains (Point point)
        {
            return point.X >= Left && point.X < Right && point.Y >= Top && point.Y < Bottom;
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

        public static bool operator == (Rectangle a, Rectangle b)
        {
            return a.X == b.X
                && a.Y == b.Y
                && a.Width == b.Width
                && a.Height == b.Height;
        }

        public static bool operator != (Rectangle a, Rectangle b)
        {
            return a.X != b.X
                || a.Y != b.Y
                || a.Width != b.Width
                || a.Height != b.Height;
        }

        public bool Equals (Rectangle other)
        {
            return this.X == other.X
                && this.Y == other.Y
                && this.Width == other.Width
                && this.Height == other.Height;
        }

        public override bool Equals (object obj)
        {
            if (obj is Rectangle)
                return Equals((Rectangle)obj);
            return false;
        }

        public override int GetHashCode ()
        {
            return X.GetHashCode() + Y.GetHashCode() + Width.GetHashCode() + Height.GetHashCode();
        }

        public override string ToString ()
        {
            CultureInfo culture = CultureInfo.CurrentCulture;
            return string.Format(culture, "{{X:{0} Y:{1} W:{2} H:{3}}}", new object[] { 
                X.ToString(culture), Y.ToString(culture), Width.ToString(culture), Height.ToString(culture)
            });
        }

        public static readonly Rectangle Empty = new Rectangle(0, 0, 0, 0);
    }
}
