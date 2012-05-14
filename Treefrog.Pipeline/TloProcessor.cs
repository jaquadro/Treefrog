using System.ComponentModel;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Treefrog.Pipeline.Content;

namespace Treefrog.Pipeline
{
    [ContentProcessor(DisplayName = "Treefrog TLO Processor")]
    class TloProcessor : ContentProcessor<ObjectRegistryContent, ObjectRegistryContent>
    {
        [DisplayName("Project Key")]
        [Description("The name of a Treefrog project namespace to associate this resource with.")]
        public string ProjectKey { get; set; }

        [DisplayName("Object Pool Key")]
        [Description("The name of a Treefrog project object pool.")]
        public string ObjectPoolKey { get; set; }

        [DisplayName("Object Pool Id")]
        [Description("The id of a Treefrog project object pool.")]
        public int ObjectPoolId { get; set; }

        public override ObjectRegistryContent Process (ObjectRegistryContent input, ContentProcessorContext context)
        {
            if (!Directory.Exists("build")) {
                Directory.CreateDirectory("build");
            }

            input.ObjectPool = input.Project.ObjectPoolManager.Pools[ObjectPoolKey];
            input.Id = ObjectPoolId;

            return input;
        }
    }
}
