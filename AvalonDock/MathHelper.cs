using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvalonDock
{
    internal static class MathHelper
    {

        public static double MinMax(double value, double min, double max)
        {
            if (min > max)
                throw new ArgumentException("min>max");

            if (value < min)
                return min;
            if (value > max)
                return max;

            return value;
        }

        public static void AssertIsPositiveOrZero(double value)
        {
            if (value < 0.0)
                throw new ArgumentException("Invalid value, must be a positive number or equal to zero");
        }
    }
}
