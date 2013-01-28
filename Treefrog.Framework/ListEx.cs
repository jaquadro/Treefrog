using System;
using System.Collections.Generic;
using System.Text;

namespace Treefrog.Framework
{
    public static class ListEx
    {
        public static bool MoveItemBy<T> (this List<T> list, T item, int offset)
        {
            if (offset == 0)
                return false;

            if (!list.Contains(item))
                throw new ArgumentException("Item not found", "item");

            int index = list.IndexOf(item);
            if (index + offset < 0 || index + offset >= list.Count)
                throw new ArgumentOutOfRangeException("offset", "The relative offset results in an invalid index for this item");

            list.Remove(item);
            list.Insert(index + offset, item);

            return true;
        }

        public static bool MoveItem<T> (this List<T> list, T item, int index)
        {
            if (!list.Contains(item))
                throw new ArgumentException("Item not found", "item");

            if (index < 0 || index >= list.Count)
                throw new ArgumentOutOfRangeException("index", "Index is out of range for this list");

            if (index == list.IndexOf(item))
                return false;

            list.Remove(item);
            list.Insert(index, item);

            return true;
        }

        /*public void MoveItemToStart<T> (this List<T> list, T item)
        {
            MoveItem(list, item, 0);
        }

        public void MoveItemToEnd<T> (this List<T> list, T item)
        {
            MoveItem(list, item, list.Count - 1);
        }*/
    }
}
