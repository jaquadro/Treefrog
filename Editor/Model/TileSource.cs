using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Editor.Model
{
    public interface ITileSource : IEnumerable<Tile>
    {
        int Count { get; }

        int TileHeight { get; }
        int TileWidth { get; }
    }

    public interface ITileSource1D : ITileSource
    {
        int Capacity { get; }

        Tile this[int index] { get; set; }
    }

    public interface ITileSource2D : ITileSource
    {
        int TilesHigh { get; }
        int TilesWide { get; }

        int PixelsHigh { get; }
        int PixelsWide { get; }

        Tile this[TileCoord coord] { get; set; }

        IEnumerable<KeyValuePair<TileCoord, Tile>> Region (Rectangle rect);
    }
}
