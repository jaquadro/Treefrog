using System;
using System.Globalization;
using System.Xml.Serialization;

namespace Treefrog.Framework.Imaging
{
    [XmlType]
    public struct Point
    {
        public static readonly Point Zero = new Point(0, 0);

        [XmlAttribute]
        public int X;

        [XmlAttribute]
        public int Y;

        public Point (int x, int y)
        {
            X = x;
            Y = y;
        }

        public static bool operator == (Point a, Point b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator != (Point a, Point b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public bool Equals (Point other)
        {
            return this.X == other.X && this.Y == other.Y;
        }

        public override bool Equals (object obj)
        {
            if (obj is Point)
                return Equals((Point)obj);
            return false;
        }

        public override int GetHashCode ()
        {
            return X.GetHashCode() + Y.GetHashCode();
        }

        public override string ToString ()
        {
            CultureInfo culture = CultureInfo.CurrentCulture;
            return string.Format(culture, "{{X:{0} Y:{1}}}", new object[] { 
                X.ToString(culture), Y.ToString(culture)
            });
        }
    }
}
