using System;
using System.Collections.Generic;
using System.Text;
using Treefrog.Framework.Model.Support;
using System.Xml.Serialization;
using Treefrog.Framework.Imaging;

namespace Treefrog.Framework.Model
{
    public class StaticTileBrush : TileBrush
    {
        private Dictionary<TileCoord, TileStack> _tiles;

        private int _minX = int.MaxValue;
        private int _minY = int.MaxValue;
        private int _maxX = int.MinValue;
        private int _maxY = int.MinValue;

        public StaticTileBrush (string name, int tileWidth, int tileHeight)
            : base(name, tileWidth, tileHeight)
        {
            _tiles = new Dictionary<TileCoord, TileStack>();
        }

        public IEnumerable<LocatedTile> Tiles
        {
            get
            {
                foreach (var kv in _tiles) {
                    foreach (Tile tile in kv.Value)
                        yield return new LocatedTile(tile, kv.Key);
                }
            }
        }

        public void AddTile (TileCoord coord, Tile tile)
        {
            if (!_tiles.ContainsKey(coord))
                _tiles.Add(coord, new TileStack());

            _tiles[coord].Remove(tile);
            _tiles[coord].Add(tile);

            UpdateExtants(coord);
        }

        public void ClearTile (TileCoord coord)
        {
            _tiles.Remove(coord);
        }

        public void Normalize ()
        {
            if (_minX == 0 || _minY == 0)
                return;

            Dictionary<TileCoord, TileStack> adjustedTiles = new Dictionary<TileCoord, TileStack>();
            foreach (var kv in _tiles)
                adjustedTiles.Add(new TileCoord(kv.Key.X - _minX, kv.Key.Y - _minY), kv.Value);

            _tiles = adjustedTiles;

            ResetExtants();
        }

        public override void ApplyBrush (TileGridLayer tileLayer, int x, int y)
        {
            List<LocatedTile> updatedTiles = new List<LocatedTile>();

            foreach (LocatedTile tile in Tiles)
                tileLayer.AddTile(x + tile.X, y + tile.Y, tile.Tile);
        }

        public override TextureResource MakePreview ()
        {
            if (TilesWide <= 0 || TilesHigh <= 0)
                return null;

            TextureResource resource = new TextureResource(TilesWide * TileWidth, TilesHigh * TileHeight);
            foreach (LocatedTile tile in Tiles) {
                int x = (tile.X - _minX) * TileWidth;
                int y = (tile.Y - _minY) * TileHeight;
                if (tile.Tile != null)
                    resource.Set(tile.Tile.Pool.GetTileTexture(tile.Tile.Id), new Point(x, y));
            }

            return resource;
        }

        public override TextureResource MakePreview (int maxWidth, int maxHeight)
        {
            TextureResource resource = MakePreview();

            if (resource.Width <= maxWidth && resource.Height <= maxHeight)
                return resource;

            double aspectRatio = resource.Width / resource.Height;
            double scale = (aspectRatio > 1)
                ? maxWidth / resource.Width : maxHeight / resource.Height;

            TextureResource preview = new TextureResource((int)(resource.Width * scale), (int)(resource.Height * scale));
            preview.Set(resource, Point.Zero);

            return preview;
        }

        public override int TilesHigh
        {
            get { return (_maxY >= _minY) ? _maxY - _minY + 1 : 0; }
        }

        public override int TilesWide
        {
            get { return (_maxX >= _minX) ? _maxX - _minX + 1 : 0; }
        }

        private void UpdateExtants (TileCoord coord)
        {
            if (coord.X < _minX)
                _minX = coord.X;
            if (coord.X > _maxX)
                _maxX = coord.X;
            if (coord.Y < _minY)
                _minY = coord.Y;
            if (coord.Y > _maxY)
                _maxY = coord.Y;
        }

        private void ResetExtants ()
        {
            _minX = int.MaxValue;
            _minY = int.MaxValue;
            _maxX = int.MinValue;
            _maxY = int.MinValue;

            foreach (TileCoord coord in _tiles.Keys)
                UpdateExtants(coord);
        }
    }

    public class DynamicBrush : TileBrush
    {
        private DynamicBrushClass _brushClass;
        private IList<TileProxy> _tiles;

        public DynamicBrush (string name, int tileWidth, int tileHeight, DynamicBrushClass brushClass)
            : base(name, tileWidth, tileHeight)
        {
            _brushClass = brushClass;
            _tiles = brushClass.CreateTileProxyList();
        }

        public DynamicBrushClass BrushClass
        {
            get { return _brushClass; }
        }

        public Tile PrimaryTile
        {
            get
            {
                int index = _brushClass.PrimaryTileIndex;
                if (index >= 0 && index < _tiles.Count && _tiles[index] != null)
                    return _tiles[index].Tile;
                else
                    return null;
            }
        }

        public override TextureResource MakePreview ()
        {
            Tile tile = PrimaryTile;
            if (tile == null)
                return null;

            return tile.Pool.GetTileTexture(tile.Id);
        }

        public override TextureResource MakePreview (int maxWidth, int maxHeight)
        {
            return _brushClass.MakePreview(_tiles, TileWidth, TileHeight, maxWidth, maxHeight);
        }

        public bool IsMemberTile (LocatedTile tile)
        {
            foreach (TileProxy proxy in _tiles) {
                if (proxy.Tile != null && proxy.Tile.Id == tile.Tile.Id)
                    return true;
            }

            return false;
        }

        public bool ContainsMemberTile (IEnumerable<LocatedTile> tiles)
        {
            foreach (LocatedTile tile in tiles) {
                if (IsMemberTile(tile))
                    return true;
            }

            return false;
        }

        public void SetTile (int position, Tile tile)
        {
            if (position >= 0 && position < _tiles.Count)
                _tiles[position].Tile = tile;
        }

        public void SetTile (int x, int y, Tile tile)
        {
            int position = y * _brushClass.TemplateWidth + x;
            SetTile(position, tile);
        }

        public Tile GetTile (int position)
        {
            if (position >= 0 && position < _tiles.Count)
                return _tiles[position].Tile;
            else
                return null;
        }

        public LocatedTile GetLocatedTile (int position)
        {
            if (position >= 0 && position < _tiles.Count)
                return new LocatedTile(_tiles[position].Tile, position % _brushClass.TemplateWidth, position / _brushClass.TemplateWidth);
            else
                return new LocatedTile();
        }

        public override void ApplyBrush (TileGridLayer tileLayer, int x, int y)
        {
            List<LocatedTile> updatedTiles = new List<LocatedTile>();

            updatedTiles.Add(new LocatedTile(InnerApply(tileLayer, x, y), x, y));

            // Update valid neighboring tiles
            foreach (TileCoord coord in NeighborCoordSet(x, y)) {
                if (ContainsMemberTile(tileLayer.TilesAt(coord)))
                    updatedTiles.Add(new LocatedTile(InnerApply(tileLayer, coord.X, coord.Y), coord.X, coord.Y));
            }
        }

        private Tile InnerApply (TileGridLayer tileLayer, int x, int y)
        {
            TileCoord[] coordSet = NeighborCoordSet(x, y);
            List<int> neighbors = new List<int>();

            for (int i = 0; i < coordSet.Length; i++) {
                if (ContainsMemberTile(tileLayer.TilesAt(coordSet[i])))
                    neighbors.Add(i + 1);
            }

            List<LocatedTile> targetStack = new List<LocatedTile>();
            foreach (LocatedTile tile in tileLayer.TilesAt(new TileCoord(x, y)))
                targetStack.Add(tile);

            foreach (LocatedTile tile in targetStack) {
                if (IsMemberTile(tile))
                    tileLayer.RemoveTile(x, y, tile.Tile);
            }

            int index = _brushClass.ApplyRules(neighbors);
            Tile newTile = null;

            if (index >= 0 && index < _tiles.Count && _tiles[index].Tile != null)
                newTile = _tiles[index].Tile;

            if (newTile != null)
                tileLayer.AddTile(x, y, newTile);

            return newTile;
        }

        private static TileCoord[] NeighborCoordSet (int x, int y)
        {
            return new TileCoord[] {
                new TileCoord(x - 1, y - 1),
                new TileCoord(x, y - 1),
                new TileCoord(x + 1, y - 1),
                new TileCoord(x + 1, y),
                new TileCoord(x + 1, y + 1),
                new TileCoord(x, y + 1),
                new TileCoord(x - 1, y + 1),
                new TileCoord(x - 1, y),
            };
        }

        public static DynamicBrushXmlProxy ToXmlProxy (DynamicBrush brush)
        {
            if (brush == null)
                return null;

            List<BrushEntryXmlProxy> brushEntries = new List<BrushEntryXmlProxy>();
            for (int i = 0; i < brush.BrushClass.SlotCount; i++) {
                Tile tile = brush.GetTile(i);
                if (tile != null)
                    brushEntries.Add(new BrushEntryXmlProxy() {
                        Slot = i,
                        TileId = tile.Id,
                    });
            }

            return new DynamicBrushXmlProxy() {
                Id = brush.Id,
                Name = brush.Name,
                Type = brush.BrushClass.ClassName,
                TileWidth = brush.TileWidth,
                TileHeight = brush.TileHeight,
                BrushEntries = brushEntries,
            };
        }

        public static DynamicBrush FromXmlProxy (DynamicBrushXmlProxy proxy, TilePoolManager manager, DynamicBrushClassRegistry registry)
        {
            if (proxy == null)
                return null;

            DynamicBrushClass brushClass = registry.Lookup(proxy.Type);
            if (brushClass == null)
                return null;

            DynamicBrush brush = new DynamicBrush(proxy.Name, proxy.TileWidth, proxy.TileHeight, brushClass);

            foreach (BrushEntryXmlProxy entry in proxy.BrushEntries) {
                TilePool pool = manager.PoolFromTileId(entry.TileId);
                if (pool == null)
                    continue;

                brush.SetTile(entry.Slot, pool.GetTile(entry.TileId));
            }

            return brush;
        }
    }

    [XmlRoot("StaticBrush")]
    public class StaticTileBrushXmlProxy : TileBrushXmlProxy
    {
        [XmlArray]
        [XmlArrayItem("Tile")]
        public TileStackXmlProxy Tiles { get; set; }
    }

    [XmlRoot("DynamicBrush")]
    public class DynamicBrushXmlProxy : TileBrushXmlProxy
    {
        [XmlAttribute]
        public string Type { get; set; }

        [XmlArray]
        [XmlArrayItem("Entry")]
        public List<BrushEntryXmlProxy> BrushEntries { get; set; }
    }

    [XmlRoot("BrushEntry")]
    public class BrushEntryXmlProxy
    {
        [XmlAttribute]
        public int Slot { get; set; }

        [XmlAttribute]
        public int TileId { get; set; }
    }
}
