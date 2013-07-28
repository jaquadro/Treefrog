using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treefrog.Pipeline.ImagePacker
{
    public static class MathUtils
    {
        public static int NextPowerOfTwo (int value)
        {
            if (value == 0)
                return 1;

            value--;

            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;

            return value + 1;
        }
    }
}
