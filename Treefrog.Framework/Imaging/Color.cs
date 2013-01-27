using System;
using System.Globalization;

namespace Treefrog.Framework.Imaging
{
    [Serializable]
    public struct Color
    {
        public byte A;
        public byte R;
        public byte G;
        public byte B;

        public Color (byte r, byte g, byte b, byte a)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public Color (byte r, byte g, byte b)
            : this(r, g, b, 255)
        {
        }

        public static bool operator == (Color a, Color b)
        {
            return a.A == b.A
                && a.R == b.R
                && a.G == b.G
                && a.B == b.B;
        }

        public static bool operator != (Color a, Color b)
        {
            return a.A != b.A
                || a.R != b.R
                || a.G != b.G
                || a.B != b.B;
        }

        public bool Equals (Color other)
        {
            return this.A == other.A
                && this.R == other.R
                && this.G == other.G
                && this.B == other.B;
        }

        public override bool Equals (object obj)
        {
            if (obj is Color)
                return Equals((Color)obj);
            return false;
        }

        public override int GetHashCode ()
        {
            return A.GetHashCode() + R.GetHashCode() + G.GetHashCode() + B.GetHashCode();
        }

        public override string ToString ()
        {
            CultureInfo culture = CultureInfo.CurrentCulture;
            return string.Format(culture, "{{A:{0} R:{1} G:{2} B:{3}}}", new object[] { 
                A.ToString(culture), R.ToString(culture), G.ToString(culture), B.ToString(culture)
            });
        }

        public string ToRgbHex ()
        {
            return "#" + ByteToHex2(R) + ByteToHex2(G) + ByteToHex2(B);
        }

        public string ToArgbHex ()
        {
            return "#" + ByteToHex2(A) + ByteToHex2(R) + ByteToHex2(G) + ByteToHex2(B);
        }

        public static Color ParseRgbHex (string hex)
        {
            if ((hex.Length != 6 && hex.Length != 7) ||
                (hex.Length == 7 && hex[0] != '#'))
                throw new ArgumentException("Invalid ARGB hex string.", "hex");

            if (hex[0] == '#')
                hex = hex.Substring(1);

            try {
                byte r = Convert.ToByte(hex.Substring(2, 2), 16);
                byte g = Convert.ToByte(hex.Substring(4, 2), 16);
                byte b = Convert.ToByte(hex.Substring(6, 2), 16);

                return new Color(r, g, b);
            }
            catch (FormatException) {
                throw new ArgumentException("Invalid ARGB hex string.", "hex");
            }
        }

        public static Color ParseArgbHex (string hex)
        {
            if ((hex.Length != 8 && hex.Length != 9) ||
                (hex.Length == 9 && hex[0] != '#'))
                throw new ArgumentException("Invalid ARGB hex string.", "hex");

            if (hex[0] == '#')
                hex = hex.Substring(1);

            try {
                byte a = Convert.ToByte(hex.Substring(0, 2), 16);
                byte r = Convert.ToByte(hex.Substring(2, 2), 16);
                byte g = Convert.ToByte(hex.Substring(4, 2), 16);
                byte b = Convert.ToByte(hex.Substring(6, 2), 16);

                return new Color(r, g, b, a);
            }
            catch (FormatException) {
                throw new ArgumentException("Invalid ARGB hex string.", "hex");
            }
        }

        private string ByteToHex2 (byte value)
        {
            String hex = Convert.ToString(value, 16);
            if (hex.Length >= 2)
                return hex;
            else
                return "0" + hex;
        }
    }
}
