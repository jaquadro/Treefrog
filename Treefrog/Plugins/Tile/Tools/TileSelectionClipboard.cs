using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Treefrog.Framework;
using Treefrog.Framework.Model;

namespace Treefrog.Plugins.Tiles.Tools
{
    [Serializable]
    public class TileSelectionClipboard
    {
        private Dictionary<TileCoord, Guid[]> _tiles;

        public TileSelectionClipboard (IDictionary<TileCoord, TileStack> tiles)
        {
            _tiles = new Dictionary<TileCoord, Guid[]>();
            foreach (KeyValuePair<TileCoord, TileStack> kvp in tiles) {
                Guid[] stack = TileStack.NullOrEmpty(kvp.Value) ? new Guid[0] : new Guid[kvp.Value.Count];
                for (int i = 0; i < stack.Length; i++)
                    stack[i] = kvp.Value[i].Uid;
                _tiles.Add(kvp.Key, stack);
            }
        }

        public static bool ContainsData
        {
            get { return Clipboard.ContainsData(typeof(TileSelectionClipboard).FullName); }
        }

        public void CopyToClipboard ()
        {
            Clipboard.SetData(typeof(TileSelectionClipboard).FullName, this);
        }

        public static TileSelectionClipboard CopyFromClipboard ()
        {
            TileSelectionClipboard clip = Clipboard.GetData(typeof(TileSelectionClipboard).FullName) as TileSelectionClipboard;
            if (clip == null)
                return null;

            return clip;
        }

        public TileSelection GetAsTileSelection (Project project, int tileWidth, int tileHeight)
        {
            Dictionary<TileCoord, TileStack> xlat = new Dictionary<TileCoord, TileStack>();
            foreach (KeyValuePair<TileCoord, Guid[]> item in _tiles) {
                TileStack stack = new TileStack();

                foreach (Guid tileId in item.Value) {
                    TilePool pool = project.TilePoolManager.PoolFromItemKey(tileId);
                    Tile tile = pool.GetTile(tileId);
                    stack.Add(tile);
                }

                xlat.Add(item.Key, stack);
            }

            TileSelection selection = new TileSelection(tileWidth, tileHeight);
            selection.AddTiles(xlat);

            return selection;
        }
    }
}
