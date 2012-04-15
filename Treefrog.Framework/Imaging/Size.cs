using System;

namespace Treefrog.Framework.Imaging
{
    public struct Size
    {
        public int Height;
        public int Width;

        public Size (int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
