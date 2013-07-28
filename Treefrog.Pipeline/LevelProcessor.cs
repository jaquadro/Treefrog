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
using Treefrog.Pipeline.ImagePacker;

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
            ProcessObjectTextures(level.Project, context);

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

            ObjectPool objPool = new ObjectPool("default");

            foreach (Layer layer in level.Layers) {
                TileGridLayer tileLayer = layer as TileGridLayer;
                if (tileLayer != null) {
                    string key = KeyFromDims(tileLayer.TileWidth, tileLayer.TileHeight);
                    if (!pools.ContainsKey(key))
                        pools[key] = outputProject.TilePoolManager.CreatePool(key, tileLayer.TileWidth, tileLayer.TileHeight);

                    outputLevel.Layers.Add(ProcessTileLayer(tileLayer, pools[key], tileUidMap));
                }

                ObjectLayer objLayer = layer as ObjectLayer;
                if (objLayer != null) {
                    outputLevel.Layers.Add(ProcessObjectLayer(objLayer, objPool));
                }
            }

            if (objPool.Count > 0)
                outputProject.ObjectPoolManager.Pools.Add(objPool);

            return outputLevel;
        }

        private TileGridLayer ProcessTileLayer (TileGridLayer layer, TilePool layerTilePool, Dictionary<Guid, Guid> tileUidMap)
        {
            MultiTileGridLayer outLayer = new MultiTileGridLayer(layer.Name, layer.TileWidth, layer.TileHeight, layer.TilesWide, layer.TilesHigh) {
                IsVisible = layer.IsVisible,
                Opacity = layer.Opacity,
                RasterMode = layer.RasterMode,
            };

            foreach (Property prop in layer.PropertyManager.CustomProperties)
                outLayer.PropertyManager.CustomProperties.Add(prop);

            foreach (LocatedTile tile in layer.Tiles) {
                Guid tileUid;
                if (!tileUidMap.TryGetValue(tile.Tile.Uid, out tileUid)) {
                    Tile mappedTile = layerTilePool.Tiles.Add(tile.Tile.Pool.Tiles.GetTileTexture(tile.Tile.Uid));
                    tileUidMap[tile.Tile.Uid] = mappedTile.Uid;
                    tileUid = mappedTile.Uid;

                    foreach (Property prop in tile.Tile.PropertyManager.CustomProperties)
                        mappedTile.PropertyManager.CustomProperties.Add(prop);
                }

                outLayer.AddTile(tile.X, tile.Y, layerTilePool.Tiles[tileUid]);
            }

            return outLayer;
        }

        private ObjectLayer ProcessObjectLayer (ObjectLayer layer, ObjectPool objectLayerPool)
        {
            ObjectLayer outLayer = new ObjectLayer(layer.Name, layer.LayerOriginX, layer.LayerOriginY, layer.LayerWidth, layer.LayerHeight) {
                IsVisible = layer.IsVisible,
                Opacity = layer.Opacity,
                RasterMode = layer.RasterMode,
            };

            foreach (Property prop in layer.PropertyManager.CustomProperties)
                outLayer.PropertyManager.CustomProperties.Add(prop);

            foreach (ObjectInstance obj in layer.Objects) {
                if (!objectLayerPool.Objects.Contains(obj.ObjectClass.Uid))
                    objectLayerPool.Objects.Add(obj.ObjectClass);

                outLayer.AddObject(obj);
            }

            return outLayer;
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

        private Dictionary<Guid, string> ProcessObjectTextures (Project input, ContentProcessorContext context)
        {
            Dictionary<Guid, string> texAssetMap = new Dictionary<Guid, string>();

            string assetPath = Path.GetDirectoryName(context.OutputFilename).Substring(context.OutputDirectory.Length);

            if (!Directory.Exists(Path.Combine(BuildPath, assetPath)))
                Directory.CreateDirectory(Path.Combine(BuildPath, assetPath));

            foreach (ObjectPool pool in input.ObjectPoolManager.Pools) {
                string path = Path.Combine(BuildPath, assetPath, "objects-" + pool.Uid + ".png");
                //Debugger.Launch();
                TexturePacker packer = new TexturePacker(Path.Combine(BuildPath, assetPath), new Settings() {
                    Rotation = false,
                });
                foreach (ObjectClass obj in pool.Objects) {
                    using (Bitmap image = obj.Image.CreateBitmap()) {
                        packer.AddImage(image, obj.Uid.ToString());
                    }
                }

                packer.Pack(Path.Combine(BuildPath, assetPath), "objects-" + pool.Uid);

                string assetName = Path.GetFileNameWithoutExtension(path).Substring(BuildPath.Length);


            }

            return texAssetMap;
        }

        private string KeyFromDims (int w, int h)
        {
            return w + "x" + h;
        }
    }
}
