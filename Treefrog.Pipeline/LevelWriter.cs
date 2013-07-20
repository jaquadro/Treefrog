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
    public class LevelWriter : ContentTypeWriter<Level>
    {
        protected override void Write (ContentWriter output, Level value)
        {
            
        }

        public override string GetRuntimeReader (TargetPlatform targetPlatform)
        {
            return "Treefrog.Runtime.LevelReader, Treefrog.Runtime";
        }

        public override string GetRuntimeType (TargetPlatform targetPlatform)
        {
            return "Treefrog.Runtime.Level, Treefrog.Runtime";
        }
    }
}
