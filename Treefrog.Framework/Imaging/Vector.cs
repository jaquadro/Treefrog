using System.Globalization;
using System.Xml.Serialization;

namespace Treefrog.Framework.Imaging
{
    [XmlType]
    public struct Vector
    {
        public static readonly Vector Zero = new Vector(0, 0);

        [XmlAttribute]
        public float X;

        [XmlAttribute]
        public float Y;

        public Vector (float x, float y)
        {
            X = x;
            Y = y;
        }

        public static bool operator == (Vector a, Vector b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator != (Vector a, Vector b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public bool Equals (Vector other)
        {
            return this.X == other.X && this.Y == other.Y;
        }

        public override bool Equals (object obj)
        {
            if (obj is Vector)
                return Equals((Vector)obj);
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
