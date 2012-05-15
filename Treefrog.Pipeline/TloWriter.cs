using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Treefrog.Framework.Model;
using Treefrog.Pipeline.Content;

namespace Treefrog.Pipeline
{
    [ContentTypeWriter]
    public class TloWriter : ContentTypeWriter<ObjectRegistryContent>
    {
        protected override void Write (ContentWriter output, ObjectRegistryContent value)
        {
            output.Write(value.Version);

            ObjectPool pool = value.ObjectPool;

            output.Write((short)value.Id);

            WritePropertyBlock(output, pool.CustomProperties);

            output.Write((short)pool.Count);
            foreach (ObjectClass objClass in pool.Objects) {
                output.Write((short)objClass.Id);
                output.Write(objClass.Name);

                //WritePropertyBlock(output, objClass.CustomProperties);
            }
        }

        private void WritePropertyBlock (ContentWriter output, IEnumerable<Property> properties)
        {
            short pcount = 0;
            foreach (Property p in properties) {
                pcount++;
            }

            output.Write((short)pcount);
            foreach (Property p in properties) {
                output.Write(p.Name);
                output.Write(p.ToString());
            }
        }

        public override string GetRuntimeReader (TargetPlatform targetPlatform)
        {
            return "Treefrog.Runtime.ObjectPoolReader, Treefrog.Runtime";
        }

        public override string GetRuntimeType (TargetPlatform targetPlatform)
        {
            return "Treefrog.Runtime.ObjectPool, Treefrog.Runtime";
        }
    }
}
