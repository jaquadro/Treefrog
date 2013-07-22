using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using System.Diagnostics;

namespace Treefrog.Pipeline.Content
{
    public class LevelContent
    {
        private int _lastIndex = 0;

        public LevelContent (Level level)
        {
            Level = level;
            UidMap = new Dictionary<Guid, int>();
            AssetMap = new Dictionary<Guid, string>();

            PopulateUidMap();
        }

        public LevelContent (Level level, IEnumerable<KeyValuePair<Guid, string>> assetMap)
            : this(level)
        {
            foreach (var kvp in assetMap)
                AssetMap.Add(kvp.Key, kvp.Value);
        }

        public Level Level { get; private set; }

        public Dictionary<Guid, int> UidMap { get; private set; }

        public Dictionary<Guid, string> AssetMap { get; private set; }

        public int Translate (Guid uid)
        {
            if (!UidMap.ContainsKey(uid))
                Debugger.Launch();

            return UidMap[uid];
        }

        private void PopulateUidMap ()
        {
            foreach (Layer layer in Level.Layers)
                MapUid(layer.Uid);

            foreach (TilePool pool in Level.Project.TilePoolManager.Pools) {
                MapUid(pool.Uid);
                foreach (Tile tile in pool.Tiles)
                    MapUid(tile.Uid);
            }

            foreach (ObjectPool pool in Level.Project.ObjectPoolManager.Pools) {
                MapUid(pool.Uid);
                foreach (ObjectClass obj in pool.Objects)
                    MapUid(obj.Uid);
            }
        }

        private void MapUid (Guid uid)
        {
            if (!UidMap.ContainsKey(uid))
                UidMap[uid] = NextIndex();
        }

        private int NextIndex ()
        {
            return ++_lastIndex;
        }
    }
}
