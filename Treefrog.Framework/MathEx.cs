using System;
using System.Collections.Generic;
using System.Text;

namespace Treefrog.Framework
{
    public class MathEx
    {
        public static double DegToRad (double angle) {
            return Math.PI * angle / 180.0;
        }

        public static float DegToRad (float angle)
        {
            return (float)(Math.PI * angle / 180.0);
        }

        public static double RadToDeg (double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        public static float RadToDeg (float angle)
        {
            return (float)(angle * (180.0 / Math.PI));
        }
    }
}
