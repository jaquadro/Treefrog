using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace AvalonDock
{
    internal static class Extentions
    {
        public static bool Contains(this IEnumerable collection, object item)
        {
            foreach (var o in collection)
                if (o == item)
                    return true;

            return false;
        }


        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (T v in collection)
                action(v);
        }
    }
}
