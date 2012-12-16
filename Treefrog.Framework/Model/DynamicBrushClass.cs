using System;
using System.Collections.Generic;
using System.Text;
using Treefrog.Framework.Model.Support;
using Treefrog.Framework.Imaging;

namespace Treefrog.Framework.Model
{
    // +---+---+---+
    // | 1 | 2 | 3 |
    // +---+---+---+
    // | 8 | X | 4 |
    // +---+---+---+
    // | 7 | 6 | 5 |
    // +---+---+---+

    public class DynamicBrushClassRegistry : ObjectRegistry<DynamicBrushClass>
    {
        public DynamicBrushClassRegistry ()
        {
            Register("Basic", new BasicDynamicBrushClass());
            Register("Extended", new ExtendedDynamicBrushClass());
        }
    }

    public class TileProxy
    {
        public Tile Tile { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public TileProxy (int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public abstract class DynamicBrushClass
    {
        protected class DynamicBrushRule
        {
            private int _tileIndex;
            private bool[] _neighbors = new bool[8];

            public DynamicBrushRule (int tileIndex, params int[] neighbors)
            {
                _tileIndex = tileIndex;

                foreach (int n in neighbors) {
                    if (n >= 1 && n <= 8)
                        _neighbors[n - 1] = true;
                }
            }

            public int TileIndex
            {
                get { return _tileIndex; }
            }

            public int Matches (IList<int> neighbors)
            {
                bool[] key = new bool[8];

                foreach (int n in neighbors) {
                    if (n >= 1 && n <= 8)
                        key[n - 1] = true;
                }

                int matchStrength = 0;

                for (int i = 0; i < 8; i++) {
                    if (_neighbors[i] == key[i])
                        matchStrength++;
                    else if (_neighbors[i] && !key[i])
                        return 0;
                }

                return matchStrength;
            }
        }

        private List<DynamicBrushRule> _rules = new List<DynamicBrushRule>();

        public abstract int TemplateWidth { get; }

        public abstract int TemplateHeight { get; }

        protected List<DynamicBrushRule> Rules
        {
            get { return _rules; }
        }

        public virtual int SlotCount
        {
            get { return TemplateWidth * TemplateHeight; }
        }

        public abstract string ClassName { get; }

        public abstract int PrimaryTileIndex { get; }

        protected abstract int DefaultTileIndex { get; }

        public int ApplyRules (IList<int> neighbors)
        {
            int bestMatchWeight = 0;
            int tileIndex = DefaultTileIndex;

            foreach (DynamicBrushRule rule in _rules) {
                int matchWeight = rule.Matches(neighbors);
                if (matchWeight > bestMatchWeight) {
                    bestMatchWeight = matchWeight;
                    tileIndex = rule.TileIndex;
                }
            }

            return tileIndex;
        }

        public virtual TextureResource MakePreview (IList<TileProxy> tiles, int tileWidth, int tileHeight, int maxWidth, int maxHeight)
        {
            return null;
        }

        protected virtual TileProxy TileProxyFromIndex (int index)
        {
            int y = index / TemplateWidth;
            int x = index % TemplateWidth;

            return new TileProxy(x, y);
        }

        public virtual IList<TileProxy> CreateTileProxyList ()
        {
            List<TileProxy> list = new List<TileProxy>();
            for (int i = 0; i < SlotCount; i++)
                list.Add(TileProxyFromIndex(i));

            return list;
        }
    }

    public class BasicDynamicBrushClass : DynamicBrushClass
    {
        public BasicDynamicBrushClass ()
        {
            // See brush overlay image for intepretation of tile at each coordinate
            Rules.AddRange(new DynamicBrushRule[] {
                new DynamicBrushRule(0, new int[] { 4, 5, 6 }),
                new DynamicBrushRule(1, new int[] { 4, 5, 6, 7, 8}),
                new DynamicBrushRule(2, new int[] { 6, 7, 8}),
                new DynamicBrushRule(3, new int[] { 2, 3, 4, 5, 6, 7, 8}),
                new DynamicBrushRule(4, new int[] { 2, 3, 4, 5, 6}),
                new DynamicBrushRule(5, new int[] { 1, 2, 3, 4, 5, 6, 7, 8}),
                new DynamicBrushRule(6, new int[] { 1, 2, 6, 7, 8}),
                new DynamicBrushRule(7, new int[] { 1, 2, 4, 5, 6, 7, 8}),
                new DynamicBrushRule(8, new int[] { 2, 3, 4}),
                new DynamicBrushRule(9, new int[] { 1, 2, 3, 4, 8}),
                new DynamicBrushRule(10, new int[] { 1, 2, 8}),
                new DynamicBrushRule(11, new int[] { 1, 2, 3, 4, 5, 6, 8}),
                new DynamicBrushRule(13, new int[] { 1, 2, 4, 5, 6, 8}),
                new DynamicBrushRule(14, new int[] { 2, 3, 4, 6, 7, 8}),
                new DynamicBrushRule(15, new int[] { 1, 2, 3, 4, 6, 7, 8}),
            });
        }

        public override string ClassName
        {
            get { return "Basic"; }
        }

        public override int TemplateWidth
        {
            get { return 4; }
        }

        public override int TemplateHeight
        {
            get { return 4; }
        }

        public override int PrimaryTileIndex
        {
            get { return 5; }
        }

        protected override int DefaultTileIndex
        {
            get { return 12; }
        }

        public override TextureResource MakePreview (IList<TileProxy> tiles, int tileWidth, int tileHeight, int maxWidth, int maxHeight)
        {
            if (tiles.Count < SlotCount)
                return null;
            for (int i = 0; i < SlotCount; i++)
                if (tiles[i] == null)
                    return null;

            TextureResource resource = new TextureResource(maxWidth, maxHeight);

            int tilesWide = Math.Min(3, Math.Max(1, maxWidth / tileWidth));
            int tilesHigh = Math.Min(3, Math.Max(1, maxHeight / tileHeight));
            int previewWidth = Math.Min(maxWidth, tilesWide * tileWidth);
            int previewHeight = Math.Min(maxHeight, tilesHigh * tileHeight);
            int previewX = (maxWidth - previewWidth) / 2;
            int previewY = (maxHeight - previewHeight) / 2;

            Tile[,] previewTiles = new Tile[3, 3] {
                { tiles[0].Tile, tiles[1].Tile, tiles[2].Tile },
                { tiles[4].Tile, tiles[5].Tile, tiles[6].Tile },
                { tiles[8].Tile, tiles[9].Tile, tiles[10].Tile },
            };

            for (int y = 0; y < tilesHigh; y++) {
                for (int x = 0; x < tilesWide; x++) {
                    if (previewTiles[y, x] != null) {
                        TextureResource tex = previewTiles[y, x].Pool.GetTileTexture(previewTiles[y, x].Id);
                        resource.Set(tex, new Point(previewX + x * tileWidth, previewY + y * tileHeight));
                    }
                }
            }

            return resource;
        }
    }

    public class ExtendedDynamicBrushClass : DynamicBrushClass
    {
        public ExtendedDynamicBrushClass ()
        {
            // See brush overlay image for intepretation of tile at each coordinate
            Rules.AddRange(new DynamicBrushRule[] {
                new DynamicBrushRule(0, new int[] { 4, 5, 6 }),
                new DynamicBrushRule(1, new int[] { 4, 5, 6, 7, 8 }),
                new DynamicBrushRule(2, new int[] { 6, 7, 8 }),
                new DynamicBrushRule(3, new int[] { 2, 3, 4, 5, 6, 7, 8 }),
                new DynamicBrushRule(4, new int[] { 2, 6, 7, 8 }),
                new DynamicBrushRule(5, new int[] { 4, 5, 6, 8 }),
                new DynamicBrushRule(6, new int[] { 2, 8 }),
                new DynamicBrushRule(7, new int[] { 2, 3, 4, 5, 6, 8 }),
                new DynamicBrushRule(8, new int[] { 2, 6, 8 }),
                new DynamicBrushRule(9, new int[] { 2, 4, 5, 6, 8 }),
                new DynamicBrushRule(10, new int[] { 6 }),
                new DynamicBrushRule(11, new int[] { 2, 6 }),

                new DynamicBrushRule(12, new int[] { 2, 3, 4, 5, 6 }),
                new DynamicBrushRule(13, new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }),
                new DynamicBrushRule(14, new int[] { 1, 2, 6, 7, 8 }),
                new DynamicBrushRule(15, new int[] { 1, 2, 3, 4, 5, 6, 8 }),
                new DynamicBrushRule(16, new int[] { 1, 2, 6, 8 }),
                new DynamicBrushRule(17, new int[] { 2, 3, 4, 8 }),
                new DynamicBrushRule(18, new int[] { 6, 8 }),
                new DynamicBrushRule(19, new int[] { 1, 2, 4, 6, 7, 8 }),
                new DynamicBrushRule(20, new int[] { 2, 4, 6 }),
                new DynamicBrushRule(21, new int[] { 2, 3, 4, 6, 8 }),
                new DynamicBrushRule(22, new int[] { 2 }),
                new DynamicBrushRule(23, new int[] { 4, 8 }),

                new DynamicBrushRule(24, new int[] { 2, 3, 4 }),
                new DynamicBrushRule(25, new int[] { 1, 2, 3, 4, 8 }),
                new DynamicBrushRule(26, new int[] { 1, 2, 8 }),
                new DynamicBrushRule(27, new int[] { 1, 2, 4, 5, 6, 7, 8 }),
                new DynamicBrushRule(28, new int[] { 2, 4, 5, 6 }),
                new DynamicBrushRule(29, new int[] { 4, 6, 7, 8 }),
                new DynamicBrushRule(30, new int[] { 2, 4 }),
                new DynamicBrushRule(31, new int[] { 2, 4, 5, 6, 7, 8 }),
                new DynamicBrushRule(32, new int[] { 2, 4, 8 }),
                new DynamicBrushRule(33, new int[] { 1, 2, 4, 6, 8 }),
                new DynamicBrushRule(34, new int[] { 4 }),
                new DynamicBrushRule(35, new int[] { 2, 4, 6, 8 }),

                new DynamicBrushRule(37, new int[] { 2, 3, 4, 6, 7, 8 }),
                new DynamicBrushRule(38, new int[] { 1, 2, 4, 5, 6, 8 }),
                new DynamicBrushRule(39, new int[] { 1, 2, 3, 4, 6, 7, 8 }),
                new DynamicBrushRule(40, new int[] { 2, 3, 4, 6 }),
                new DynamicBrushRule(41, new int[] { 1, 2, 4, 8 }),
                new DynamicBrushRule(42, new int[] { 4, 6 }),
                new DynamicBrushRule(43, new int[] { 1, 2, 3, 4, 6, 8 }),
                new DynamicBrushRule(44, new int[] { 4, 6, 8 }),
                new DynamicBrushRule(45, new int[] { 2, 4, 6, 7, 8 }),
                new DynamicBrushRule(46, new int[] { 8 }),
                new DynamicBrushRule(47),
            });
        }

        public override string ClassName
        {
            get { return "Extended"; }
        }

        public override int TemplateWidth
        {
            get { return 12; }
        }

        public override int TemplateHeight
        {
            get { return 4; }
        }

        public override int PrimaryTileIndex
        {
            get { return 13; }
        }

        protected override int DefaultTileIndex
        {
            get { return 36; }
        }

        public override TextureResource MakePreview (IList<TileProxy> tiles, int tileWidth, int tileHeight, int maxWidth, int maxHeight)
        {
            if (tiles.Count < SlotCount)
                return null;
            for (int i = 0; i < SlotCount; i++)
                if (tiles[i] == null)
                    return null;

            TextureResource resource = new TextureResource(maxWidth, maxHeight);

            int tilesWide = Math.Min(3, Math.Max(1, maxWidth / tileWidth));
            int tilesHigh = Math.Min(3, Math.Max(1, maxHeight / tileHeight));
            int previewWidth = Math.Min(maxWidth, tilesWide * tileWidth);
            int previewHeight = Math.Min(maxHeight, tilesHigh * tileHeight);
            int previewX = (maxWidth - previewWidth) / 2;
            int previewY = (maxHeight - previewHeight) / 2;

            Tile[,] previewTiles = new Tile[3, 3] {
                { tiles[0].Tile, tiles[1].Tile, tiles[2].Tile },
                { tiles[12].Tile, tiles[13].Tile, tiles[14].Tile },
                { tiles[24].Tile, tiles[25].Tile, tiles[26].Tile },
            };

            for (int y = 0; y < tilesHigh; y++) {
                for (int x = 0; x < tilesWide; x++) {
                    if (previewTiles[y, x] != null) {
                        TextureResource tex = previewTiles[y, x].Pool.GetTileTexture(previewTiles[y, x].Id);
                        resource.Set(tex, new Point(previewX + x * tileWidth, previewY + y * tileHeight));
                    }
                }
            }

            return resource;
        }
    }
}
