using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace Treefrog.Runtime
{
    public class ObjectPoolReader : ContentTypeReader<ObjectPool>
    {
        protected override ObjectPool Read (ContentReader input, ObjectPool existingInstance)
        {
            return new ObjectPool(input);
        }
    }
}
