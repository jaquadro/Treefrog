using System;

namespace Treefrog.Framework.Imaging
{
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
    }
}
