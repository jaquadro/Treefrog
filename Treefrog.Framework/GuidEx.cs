using System;
using System.Collections.Generic;
using System.Text;

namespace Treefrog.Framework
{
    public static class GuidEx
    {
        public static Guid ValueOrNew (this Guid uid)
        {
            if (uid == Guid.Empty)
                return Guid.NewGuid();
            else
                return uid;
        }
    }
}
