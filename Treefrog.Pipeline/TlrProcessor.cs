using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Treefrog.Framework.Model;
using System.IO;
using System.Xml;
using Treefrog.Pipeline.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.ComponentModel;

namespace Treefrog.Pipeline
{
    [ContentProcessor(DisplayName = "Treefrog TLR Processor")]
    class TlrProcessor : ContentProcessor<TileRegistryContent, TileRegistryContent>
    {
        [DisplayName("Project Key")]
        [Description("The name of a Treefrog project namespace to associate this resource with.")]
        public string ProjectKey { get; set; }

        public override TileRegistryContent Process (TileRegistryContent input, ContentProcessorContext context)
        {
            if (!Directory.Exists("build")) {
                Directory.CreateDirectory("build");
            }

            int id = 0;
            foreach (TilePool pool in input.Project.TilePools) {
                string path = "build\\" + ProjectKey + "_tileset_" + id++ + ".png";
                pool.Export(path);

                // the asset name is the entire path, minus extension, after the content directory
                string asset = string.Empty;
                if (path.StartsWith(Directory.GetCurrentDirectory()))
                    asset = path.Remove(path.LastIndexOf('.')).Substring(Directory.GetCurrentDirectory().Length + 1);
                else
                    asset = Path.GetFileNameWithoutExtension(path);

                // build the asset as an external reference
                OpaqueDataDictionary data = new OpaqueDataDictionary();
                data.Add("GenerateMipmaps", false);
                data.Add("ResizeToPowerOfTwo", false);
                data.Add("TextureFormat", TextureProcessorOutputFormat.Color);
                context.BuildAsset<TextureContent, TextureContent>(
                    new ExternalReference<TextureContent>(path),
                    "TextureProcessor",
                    data,
                    "TextureImporter",
                    asset);
            }

            return input;
        }
    }
}
