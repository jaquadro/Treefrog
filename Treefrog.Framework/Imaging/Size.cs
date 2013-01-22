using System;
using System.Xml.Serialization;
using System.Globalization;

namespace Treefrog.Framework.Imaging
{
    [XmlType]
    public struct Size
    {
        public static readonly Size Zero = new Size(0, 0);

        [XmlAttribute]
        public int Height;

        [XmlAttribute]
        public int Width;

        public Size (int width, int height)
        {
            Width = width;
            Height = height;
        }

        

        public static bool operator == (Size a, Size b)
        {
            return a.Width == b.Width && a.Height == b.Height;
        }

        public static bool operator != (Size a, Size b)
        {
            return a.Width != b.Width || a.Height != b.Height;
        }

        public bool Equals (Size other)
        {
            return this.Width == other.Width && this.Height == other.Height;
        }

        public override bool Equals (object obj)
        {
            if (obj is Size)
                return Equals((Size)obj);
            return false;
        }

        public override int GetHashCode ()
        {
            return Width.GetHashCode() + Height.GetHashCode();
        }

        public override string ToString ()
        {
            CultureInfo culture = CultureInfo.CurrentCulture;
            return string.Format(culture, "{{Width:{0} Height:{1}}}", new object[] { 
                Width.ToString(culture), Height.ToString(culture)
            });
        }

        public static Size operator + (Size a, Size b)
        {
            return new Size(a.Width + b.Width, a.Height + b.Height);
        }
    }
}
