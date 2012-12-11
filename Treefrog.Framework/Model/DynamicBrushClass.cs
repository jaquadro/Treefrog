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

    public abstract class DynamicBrushClass
    {
        protected class TileProxy
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

        protected class DynamicBrushRule
        {
            private TileProxy _tile;
            private bool[] _neighbors = new bool[8];

            public DynamicBrushRule (TileProxy tile, params int[] neighbors)
            {
                _tile = tile;

                foreach (int n in neighbors) {
                    if (n >= 1 && n <= 8)
                        _neighbors[n - 1] = true;
                }
            }

            public TileProxy Tile
            {
                get { return _tile; }
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

        private List<TileProxy> _tiles = new List<TileProxy>();
        private List<DynamicBrushRule> _rules = new List<DynamicBrushRule>();

        private int _tileWidth;
        private int _tileHeight;

        public DynamicBrushClass (int tileWidth, int tileHeight)
        {
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
        }

        public int TileWidth
        {
            get { return _tileWidth; }
        }

        public int TileHeight
        {
            get { return _tileHeight; }
        }

        public abstract int TemplateWidth { get; }
        public abstract int TemplateHeight { get; }

        public int TileCount
        {
            get { return _tiles.Count; }
        }

        protected List<TileProxy> Tiles
        {
            get { return _tiles; }
        }

        protected List<DynamicBrushRule> Rules
        {
            get { return _rules; }
        }

        protected virtual Tile DefaultTile
        {
            get { return null; }
        }

        public int SlotCount
        {
            get { return _tiles.Count; }
        }

        public abstract string ClassName { get; }

        public abstract Tile PrimaryTile { get; }

        public bool ContainsMemberTile (IEnumerable<LocatedTile> tiles)
        {
            foreach (LocatedTile tile in tiles) {
                if (IsMemberTile(tile))
                    return true;
            }

            return false;
        }

        public bool IsMemberTile (LocatedTile tile)
        {
            foreach (TileProxy proxy in _tiles) {
                if (proxy.Tile != null && proxy.Tile.Id == tile.Tile.Id)
                    return true;
            }

            return false;
        }

        public Tile ApplyRules (IList<int> neighbors)
        {
            int bestMatchWeight = 0;
            Tile tile = DefaultTile;

            foreach (DynamicBrushRule rule in _rules) {
                int matchWeight = rule.Matches(neighbors);
                if (matchWeight > bestMatchWeight) {
                    bestMatchWeight = matchWeight;
                    tile = rule.Tile.Tile;
                }
            }

            return tile;
        }

        public void SetTile (int position, Tile tile)
        {
            if (position >= 0 && position < Tiles.Count)
                Tiles[position].Tile = tile;
        }

        public Tile GetTile (int position)
        {
            if (position >= 0 && position < Tiles.Count)
                return Tiles[position].Tile;
            else
                return null;
        }

        public LocatedTile GetLocatedTile (int position)
        {
            if (position >= 0 && position < Tiles.Count)
                return new LocatedTile(Tiles[position].Tile, position % TemplateWidth, position / TemplateWidth);
            else
                return new LocatedTile();
        }

        public virtual TextureResource MakePreview (int maxWidth, int maxHeight)
        {
            return null;
        }
    }

    public class BasicDynamicBrushClass : DynamicBrushClass
    {
        public BasicDynamicBrushClass (int tileWidth, int tileHeight)
            : base(tileWidth, tileHeight)
        {
            // See brush overlay image for intepretation of tile at each coordinate
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                    Tiles.Add(new TileProxy(x, y));

            Rules.AddRange(new DynamicBrushRule[] {
                new DynamicBrushRule(Tiles[0], 4, 5, 6),
                new DynamicBrushRule(Tiles[1], 4, 5, 6, 7, 8),
                new DynamicBrushRule(Tiles[2], 6, 7, 8),
                new DynamicBrushRule(Tiles[3], 2, 3, 4, 5, 6, 7, 8),
                new DynamicBrushRule(Tiles[4], 2, 3, 4, 5, 6),
                new DynamicBrushRule(Tiles[5], 1, 2, 3, 4, 5, 6, 7, 8),
                new DynamicBrushRule(Tiles[6], 1, 2, 6, 7, 8),
                new DynamicBrushRule(Tiles[7], 1, 2, 4, 5, 6, 7, 8),
                new DynamicBrushRule(Tiles[8], 2, 3, 4),
                new DynamicBrushRule(Tiles[9], 1, 2, 3, 4, 8),
                new DynamicBrushRule(Tiles[10], 1, 2, 8),
                new DynamicBrushRule(Tiles[11], 1, 2, 3, 4, 5, 6, 8),
                new DynamicBrushRule(Tiles[13], 1, 2, 4, 5, 6, 8),
                new DynamicBrushRule(Tiles[14], 2, 3, 4, 6, 7, 8),
                new DynamicBrushRule(Tiles[15], 1, 2, 3, 4, 6, 7, 8),
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

        public override Tile PrimaryTile
        {
            get
            {
                if (Tiles[5] != null)
                    return Tiles[5].Tile;
                return null;
            }
        }

        protected override Tile DefaultTile
        {
            get
            {
                if (Tiles[12] != null)
                    return Tiles[12].Tile;
                return null;
            }
        }

        public override TextureResource MakePreview (int maxWidth, int maxHeight)
        {
            TextureResource resource = new TextureResource(maxWidth, maxHeight);

            int tilesWide = Math.Min(3, Math.Max(1, maxWidth / TileWidth));
            int tilesHigh = Math.Min(3, Math.Max(1, maxHeight / TileHeight));
            int previewWidth = Math.Min(maxWidth, tilesWide * TileWidth);
            int previewHeight = Math.Min(maxHeight, tilesHigh * TileHeight);
            int previewX = (maxWidth - previewWidth) / 2;
            int previewY = (maxHeight - previewHeight) / 2;

            Tile[,] previewTiles = new Tile[3, 3] {
                { Tiles[0].Tile, Tiles[1].Tile, Tiles[2].Tile },
                { Tiles[4].Tile, Tiles[5].Tile, Tiles[6].Tile },
                { Tiles[8].Tile, Tiles[9].Tile, Tiles[10].Tile },
            };

            for (int y = 0; y < tilesHigh; y++) {
                for (int x = 0; x < tilesWide; x++) {
                    if (previewTiles[y, x] != null) {
                        TextureResource tex = previewTiles[y, x].Pool.GetTileTexture(previewTiles[y, x].Id);
                        resource.Set(tex, new Point(previewX + x * TileWidth, previewY + y * TileHeight));
                    }
                }
            }

            return resource;
        }
    }

    public class ExtendedDynamicBrushClass : DynamicBrushClass
    {
        public ExtendedDynamicBrushClass (int tileWidth, int tileHeight)
            : base(tileWidth, tileHeight)
        {
            // See brush overlay image for intepretation of tile at each coordinate
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 12; x++)
                    Tiles.Add(new TileProxy(x, y));

            Rules.AddRange(new DynamicBrushRule[] {
                new DynamicBrushRule(Tiles[0], 4, 5, 6),
                new DynamicBrushRule(Tiles[1], 4, 5, 6, 7, 8),
                new DynamicBrushRule(Tiles[2], 6, 7, 8),
                new DynamicBrushRule(Tiles[3], 2, 3, 4, 5, 6, 7, 8),
                new DynamicBrushRule(Tiles[4], 2, 6, 7, 8),
                new DynamicBrushRule(Tiles[5], 4, 5, 6, 8),
                new DynamicBrushRule(Tiles[6], 2, 8),
                new DynamicBrushRule(Tiles[7], 2, 3, 4, 5, 6, 8),
                new DynamicBrushRule(Tiles[8], 2, 6, 8),
                new DynamicBrushRule(Tiles[9], 2, 4, 5, 6, 8),
                new DynamicBrushRule(Tiles[10], 6),
                new DynamicBrushRule(Tiles[11], 2, 6),

                new DynamicBrushRule(Tiles[12], 2, 3, 4, 5, 6),
                new DynamicBrushRule(Tiles[13], 1, 2, 3, 4, 5, 6, 7, 8),
                new DynamicBrushRule(Tiles[14], 1, 2, 6, 7, 8),
                new DynamicBrushRule(Tiles[15], 1, 2, 3, 4, 5, 6, 8),
                new DynamicBrushRule(Tiles[16], 1, 2, 6, 8),
                new DynamicBrushRule(Tiles[17], 2, 3, 4, 8),
                new DynamicBrushRule(Tiles[18], 6, 8),
                new DynamicBrushRule(Tiles[19], 1, 2, 4, 6, 7, 8),
                new DynamicBrushRule(Tiles[20], 2, 4, 6),
                new DynamicBrushRule(Tiles[21], 2, 3, 4, 6, 8),
                new DynamicBrushRule(Tiles[22], 2),
                new DynamicBrushRule(Tiles[23], 4, 8),

                new DynamicBrushRule(Tiles[24], 2, 3, 4),
                new DynamicBrushRule(Tiles[25], 1, 2, 3, 4, 8),
                new DynamicBrushRule(Tiles[26], 1, 2, 8),
                new DynamicBrushRule(Tiles[27], 1, 2, 4, 5, 6, 7, 8),
                new DynamicBrushRule(Tiles[28], 2, 4, 5, 6),
                new DynamicBrushRule(Tiles[29], 4, 6, 7, 8),
                new DynamicBrushRule(Tiles[30], 2, 4),
                new DynamicBrushRule(Tiles[31], 2, 4, 5, 6, 7, 8),
                new DynamicBrushRule(Tiles[32], 2, 4, 8),
                new DynamicBrushRule(Tiles[33], 1, 2, 4, 6, 8),
                new DynamicBrushRule(Tiles[34], 4),
                new DynamicBrushRule(Tiles[35], 2, 4, 6, 8),

                new DynamicBrushRule(Tiles[37], 2, 3, 4, 6, 7, 8),
                new DynamicBrushRule(Tiles[38], 1, 2, 4, 5, 6, 8),
                new DynamicBrushRule(Tiles[39], 1, 2, 3, 4, 6, 7, 8),
                new DynamicBrushRule(Tiles[40], 2, 3, 4, 6),
                new DynamicBrushRule(Tiles[41], 1, 2, 4, 8),
                new DynamicBrushRule(Tiles[42], 4, 6),
                new DynamicBrushRule(Tiles[43], 1, 2, 3, 4, 6, 8),
                new DynamicBrushRule(Tiles[44], 4, 6, 8),
                new DynamicBrushRule(Tiles[45], 2, 4, 6, 7, 8),
                new DynamicBrushRule(Tiles[46], 8),
                new DynamicBrushRule(Tiles[47]),
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

        public override Tile PrimaryTile
        {
            get
            {
                if (Tiles[13] != null)
                    return Tiles[13].Tile;
                return null;
            }
        }

        protected override Tile DefaultTile
        {
            get
            {
                if (Tiles[36] != null)
                    return Tiles[36].Tile;
                return null;
            }
        }

        public override TextureResource MakePreview (int maxWidth, int maxHeight)
        {
            TextureResource resource = new TextureResource(maxWidth, maxHeight);

            int tilesWide = Math.Min(3, Math.Max(1, maxWidth / TileWidth));
            int tilesHigh = Math.Min(3, Math.Max(1, maxHeight / TileHeight));
            int previewWidth = Math.Min(maxWidth, tilesWide * TileWidth);
            int previewHeight = Math.Min(maxHeight, tilesHigh * TileHeight);
            int previewX = (maxWidth - previewWidth) / 2;
            int previewY = (maxHeight - previewHeight) / 2;

            Tile[,] previewTiles = new Tile[3, 3] {
                { Tiles[0].Tile, Tiles[1].Tile, Tiles[2].Tile },
                { Tiles[12].Tile, Tiles[13].Tile, Tiles[14].Tile },
                { Tiles[24].Tile, Tiles[25].Tile, Tiles[26].Tile },
            };

            for (int y = 0; y < tilesHigh; y++) {
                for (int x = 0; x < tilesWide; x++) {
                    if (previewTiles[y, x] != null) {
                        TextureResource tex = previewTiles[y, x].Pool.GetTileTexture(previewTiles[y, x].Id);
                        resource.Set(tex, new Point(previewX + x * TileWidth, previewY + y * TileHeight));
                    }
                }
            }

            return resource;
        }
    }
}
