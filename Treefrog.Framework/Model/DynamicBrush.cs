using System;
using System.Collections.Generic;
using System.Text;
using Treefrog.Framework.Model.Support;
using System.Xml.Serialization;
using Treefrog.Framework.Imaging;

namespace Treefrog.Framework.Model
{
    public class DynamicBrush : TileBrush
    {
        private DynamicBrushClass _brushClass;

        public DynamicBrush (string name, int tileWidth, int tileHeight, DynamicBrushClass brushClass)
            : base(name, tileWidth, tileHeight)
        {
            _brushClass = brushClass;
        }

        public DynamicBrushClass BrushClass
        {
            get { return _brushClass; }
        }

        public List<LocatedTile> ApplyBrush (TileGridLayer tileLayer, int x, int y)
        {
            List<LocatedTile> updatedTiles = new List<LocatedTile>();

            updatedTiles.Add(new LocatedTile(InnerApply(tileLayer, x, y), x, y));

            // Update valid neighboring tiles
            foreach (TileCoord coord in NeighborCoordSet(x, y)) {
                if (_brushClass.ContainsMemberTile(tileLayer.TilesAt(coord)))
                    updatedTiles.Add(new LocatedTile(InnerApply(tileLayer, coord.X, coord.Y), coord.X, coord.Y));
            }

            return updatedTiles;
        }

        private Tile InnerApply (TileGridLayer tileLayer, int x, int y)
        {
            TileCoord[] coordSet = NeighborCoordSet(x, y);
            List<int> neighbors = new List<int>();

            for (int i = 0; i < coordSet.Length; i++) {
                if (_brushClass.ContainsMemberTile(tileLayer.TilesAt(coordSet[i])))
                    neighbors.Add(i + 1);
            }

            List<LocatedTile> targetStack = new List<LocatedTile>();
            foreach (LocatedTile tile in tileLayer.TilesAt(new TileCoord(x, y)))
                targetStack.Add(tile);

            foreach (LocatedTile tile in targetStack) {
                if (_brushClass.IsMemberTile(tile))
                    tileLayer.RemoveTile(x, y, tile.Tile);
            }

            Tile newTile = _brushClass.ApplyRules(neighbors);
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
                Tile tile = brush.BrushClass.GetTile(i);
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

        public static DynamicBrush FromXmlProxy (DynamicBrushXmlProxy proxy, TilePoolManager manager)
        {
            if (proxy == null)
                return null;

            // TODO: Switch to a factory
            DynamicBrushClass brushClass = null;
            switch (proxy.Type) {
                case "Basic":
                    brushClass = new BasicDynamicBrushClass(proxy.TileWidth, proxy.TileHeight);
                    break;
                default:
                    return null;
            }

            DynamicBrush brush = new DynamicBrush(proxy.Name, proxy.TileWidth, proxy.TileHeight, brushClass);

            foreach (BrushEntryXmlProxy entry in proxy.BrushEntries) {
                TilePool pool = manager.PoolFromTileId(entry.TileId);
                if (pool == null)
                    continue;

                brush.BrushClass.SetTile(entry.Slot, pool.GetTile(entry.TileId));
            }

            return brush;
        }
    }

    [XmlRoot("DynamicBrush")]
    public class DynamicBrushXmlProxy : TileBrushXmlProxy
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Type { get; set; }

        [XmlAttribute]
        public int TileWidth { get; set; }

        [XmlAttribute]
        public int TileHeight { get; set; }

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
