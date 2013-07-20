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

namespace Treefrog.Pipeline
{
    [ContentProcessor(DisplayName = "Treefrog Level Processor")]
    public class LevelProcessor : ContentProcessor<Project, Level>
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

        public override Level Process (Project input, ContentProcessorContext context)
        {
            if (!Directory.Exists(BuildPath))
                Directory.CreateDirectory(BuildPath);

            Guid uid = Guid.Parse(LevelUid);

            if (!input.Levels.Contains(uid))
                return null;

            Level level = ProcessLevel(input.Levels[uid]);

            ProcessTileSetTextures(level.Project, context);

            return level;
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

        private void ProcessTileSetTextures (Project input, ContentProcessorContext context)
        {
            foreach (TilePool pool in input.TilePoolManager.Pools) {
                string path = Path.Combine(BuildPath, "tiles_" + pool.Uid + ".png");
                Bitmap image = pool.TileSource.CreateBitmap();
                image.Save(path, ImageFormat.Png);
            }
        }

        private string KeyFromDims (int w, int h)
        {
            return w + "x" + h;
        }
    }
}
