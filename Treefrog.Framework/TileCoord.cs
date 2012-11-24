using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treefrog.Framework
{
    [Serializable]
    public struct TileCoord : IEquatable<TileCoord>
    {
        private readonly int _x;
        private readonly int _y;

        public TileCoord (int x, int y)
        {
            _x = x;
            _y = y;
        }

        public int X
        {
            get { return _x; }
        }

        public int Y
        {
            get { return _y; }
        }

        public bool Equals (TileCoord value)
        {
            return _x == value._x && _y == value._y;
        }

        public override bool Equals (Object o)
        {
            try {
                return this == (TileCoord)o;
            }
            catch {
                return false;
            }
        }

        public override int GetHashCode ()
        {
            int hash = 23;
            hash = hash * 37 + _x;
            hash = hash * 37 + _y;
            return hash;
        }

        public static bool operator == (TileCoord value1, TileCoord value2)
        {
            return value1._x == value2._x && value1._y == value2._y;
        }

        public static bool operator != (TileCoord value1, TileCoord value2)
        {
            return value1._x != value2._x || value1._y != value2._y;
        }

        public override string ToString ()
        {
            return "(" + _x + ", " + _y + ")";
        }
    }
}
