using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Treefrog.Framework.Model;
using Microsoft.Xna.Framework.Content.Pipeline;
using Treefrog.Pipeline.Content;

namespace Treefrog.Pipeline
{
    [ContentTypeWriter]
    public class TlpWriter : ContentTypeWriter<LevelIndexContent>
    {
        protected override void Write (ContentWriter output, LevelIndexContent value)
        {
            output.Write(value.Levels.Count);

            foreach (LevelIndexEntry level in value.Levels) {
                output.Write(level.Id);
                output.Write(level.Name);
                output.Write(level.Asset);

                WritePropertyBlock(output, level.Properties);
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
            return "Treefrog.Runtime.LevelIndexReader, Treefrog.Runtime";
        }

        public override string GetRuntimeType (TargetPlatform targetPlatform)
        {
            return "Treefrog.Runtime.LevelIndex, Treefrog.Runtime";
        }
    }
}
