using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Treefrog.Framework.Model;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace Treefrog.Pipeline
{
    [ContentTypeWriter]
    public class ProjectWriter : ContentTypeWriter<Project>
    {
        protected override void Write (ContentWriter output, Project value)
        {
            output.Write(value.Levels.Count);

            foreach (Level level in value.Levels) {
                output.Write(level.Uid.ToByteArray());
                output.Write(level.Name);

                Common.WritePropertyBlock(output, level.PropertyManager.CustomProperties);
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
