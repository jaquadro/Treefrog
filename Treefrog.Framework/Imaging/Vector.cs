using System.Globalization;
using System.Xml.Serialization;
using System;

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

        #region Vector Arithmetic Operators

        public static Vector operator + (Vector v1, Vector v2)
        {
            return new Vector(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vector operator - (Vector v1, Vector v2)
        {
            return new Vector(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector operator * (Vector v1, Vector v2)
        {
            return new Vector(v1.X * v2.X, v1.Y * v2.Y);
        }

        public static Vector operator * (Vector value, float scale)
        {
            return new Vector(value.X * scale, value.Y * scale);
        }

        public static Vector operator * (float scale, Vector value)
        {
            return new Vector(value.X * scale, value.Y * scale);
        }

        public static Vector operator / (Vector v1, Vector v2)
        {
            return new Vector(v1.X / v2.X, v1.Y / v2.Y);
        }

        public static Vector operator / (Vector value, float scale)
        {
            return new Vector(value.X / scale, value.Y / scale);
        }

        #endregion

        #region Vector Products

        public static float Dot (Vector v1, Vector v2)
        {
            return (v1.X * v2.X) + (v1.Y * v2.Y);
        }

        #endregion

        #region Vector Metrics

        public static float DistanceSquared (Vector v1, Vector v2)
        {
            float x = v1.X - v2.X;
            float y = v1.Y - v2.Y;
            return (x * x + y * y);
        }

        public float LengthSquared
        {
            get { return (X * X + Y * Y); }
        }

        public float Length
        {
            get { return (float)Math.Sqrt(LengthSquared); }
        }

        #endregion

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

        public static float Angle (Vector vec1, Vector vec2)
        {
            float dot = Vector.Dot(vec1, vec2);
            dot = dot / (vec1.Length * vec2.Length);
            return (float)Math.Acos(dot);
        }
    }
}
