using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Treefrog.Framework.Model;
using System.ComponentModel;
using System.IO;
using Treefrog.Framework.Model.Support;
using Treefrog.Aux;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Treefrog.Pipeline.Content;

namespace Treefrog.Pipeline
{
    [ContentProcessor(DisplayName = "Treefrog Level Processor")]
    public class LevelProcessor : ContentProcessor<Project, LevelContent>
    {
        [DisplayName("Level Uid")]
        [Description("A level's internal GUID.")]
        public string LevelUid { get; set; }

        [DisplayName("Build Path")]
        [Description("The location to place intermediate build files")]
        [DefaultValue(Common.BuildPath)]
        public string BuildPath { get; set; }

        public LevelProcessor ()
        {
            BuildPath = Common.BuildPath;
        }

        public override LevelContent Process (Project input, ContentProcessorContext context)
        {
            if (!Directory.Exists(BuildPath))
                Directory.CreateDirectory(BuildPath);

            Guid uid = Guid.Parse(LevelUid);

            if (!input.Levels.Contains(uid))
                return null;

            Level level = ProcessLevel(input.Levels[uid]);

            Dictionary<Guid, string> texAssetMap = ProcessTileSetTextures(level.Project, context);

            return new LevelContent(level, texAssetMap);
        }

        private Level ProcessLevel (Level level)
        {
            Project inputProject = level.Project;
            
            Project outputProject = new Project();

            Level outputLevel = new Level(level.Name, level.OriginX, level.OriginY, level.Width, level.Height);
            outputProject.Levels.Add(outputLevel);
            outputLevel.Project = outputProject;

            Dictionary<string, TilePool> pools = new Dictionary<string,TilePool>();
            Dictionary<Guid, Guid> tileUidMap = new Dictionary<Guid, Guid>();

            foreach (TileGridLayer layer in level.GetLayers<TileGridLayer>()) {
                string key = KeyFromDims(layer.TileWidth, layer.TileHeight);
                if (!pools.ContainsKey(key))
                    pools[key] = outputProject.TilePoolManager.CreatePool(key, layer.TileWidth, layer.TileHeight);

                TilePool layerTilePool = pools[key];
                MultiTileGridLayer outLayer = new MultiTileGridLayer(layer.Name, layer.TileWidth, layer.TileHeight, layer.TilesWide, layer.TilesHigh);

                foreach (LocatedTile tile in layer.Tiles) {
                    Guid tileUid;
                    if (!tileUidMap.TryGetValue(tile.Tile.Uid, out tileUid)) {
                        Tile mappedTile = layerTilePool.Tiles.Add(tile.Tile.Pool.Tiles.GetTileTexture(tile.Tile.Uid));
                        tileUidMap[tile.Tile.Uid] = mappedTile.Uid;
                        tileUid = mappedTile.Uid;
                    }

                    outLayer.AddTile(tile.X, tile.Y, layerTilePool.Tiles[tileUid]);
                }

                outputLevel.Layers.Add(outLayer);
            }

            return outputLevel;
        }

        private Dictionary<Guid, string> ProcessTileSetTextures (Project input, ContentProcessorContext context)
        {
            Dictionary<Guid, string> texAssetMap = new Dictionary<Guid, string>();

            string assetPath = Path.GetDirectoryName(context.OutputFilename).Substring(context.OutputDirectory.Length);

            if (!Directory.Exists(Path.Combine(BuildPath, assetPath)))
                Directory.CreateDirectory(Path.Combine(BuildPath, assetPath));

            foreach (TilePool pool in input.TilePoolManager.Pools) {
                //Debugger.Launch();
                string path = Path.Combine(BuildPath, assetPath, "tiles-" + pool.Uid + ".png");
                Bitmap image = pool.TileSource.CreateBitmap();
                image.Save(path, ImageFormat.Png);

                string assetName = Path.GetFileNameWithoutExtension(path).Substring(BuildPath.Length);

                OpaqueDataDictionary data = new OpaqueDataDictionary() {
                    { "GenerateMipmaps", false },
                    { "ResizeToPowerofTwo", false },
                    { "TextureFormat", TextureProcessorOutputFormat.Color },
                };

                context.BuildAsset<TextureContent, TextureContent>(
                    new ExternalReference<TextureContent>(path),
                    "TextureProcessor",
                    data,
                    "TextureImporter",
                    assetName);

                texAssetMap[pool.Uid] = assetName;
            }

            return texAssetMap;
        }

        private string KeyFromDims (int w, int h)
        {
            return w + "x" + h;
        }
    }
}
