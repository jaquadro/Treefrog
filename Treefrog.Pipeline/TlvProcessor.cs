using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.ComponentModel;
using Treefrog.Pipeline.Content;
using System.IO;

namespace Treefrog.Pipeline
{
    [ContentProcessor(DisplayName = "Treefrog TLV Processor")]
    class TlvProcessor : ContentProcessor<LevelContent, LevelContent>
    {
        [DisplayName("Project Key")]
        [Description("The name of a Treefrog project namespace to associate this resource with.")]
        public string ProjectKey { get; set; }

        [DisplayName("Level Key")]
        [Description("The name of a Treefrog project level.")]
        public string LevelKey { get; set; }

        public override LevelContent Process (LevelContent input, ContentProcessorContext context)
        {
            if (!Directory.Exists("build")) {
                Directory.CreateDirectory("build");
            }

            input.Level = input.Project.Levels[LevelKey];

            return input;
        }
    }
}
