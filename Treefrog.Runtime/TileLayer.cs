using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Treefrog.Runtime
{
    public class TileLayer : Layer
    {
        private TileGrid _tiles;
        private TileRegistry _registry;

        public int TileHeight { get; private set; }

        public int TileWidth { get; private set; }

        public int Height { get; private set; }

        public int Width { get; private set; }

        internal TileLayer (ContentReader reader, TileRegistry registry)
            : base(reader)
        {
            TileWidth = reader.ReadInt16();
            TileHeight = reader.ReadInt16();
            Width = reader.ReadInt16();
            Height = reader.ReadInt16();

            _tiles = new TileGrid(Width, Height);

            int stackCount = reader.ReadInt32();
            for (int i = 0; i < stackCount; i++) {
                int dx = reader.ReadInt16();
                int dy = reader.ReadInt16();

                int tcount = reader.ReadInt16();
                Tile[] st = new Tile[tcount];

                for (int j = 0; j < tcount; j++) {
                    st[j] = _registry[reader.ReadInt16()];
                }

                _tiles[dx, dy] = new TileStack(st);
            }
        }

        public override void Draw (SpriteBatch spriteBatch, Rectangle region)
        {
            if (!Visible) {
                return;
            }

            int startX = region.X / TileWidth;
            int startY = region.Y / TileHeight;
            int endX = startX + (region.Width + TileWidth - 1) / TileWidth;
            int endY = startY + (region.Height + TileHeight - 1) / TileHeight;

            startX = Math.Max(startX, 0);
            startY = Math.Max(startY, 0);
            endX = Math.Min(endX, Width);
            endY = Math.Min(endY, Height);

            for (int y = startY; y < endY; y++) {
                for (int x = startX; x < endX; x++) {
                    if (_tiles[x, y] == null)
                        continue;

                    _tiles[x, y].Draw(spriteBatch, new Rectangle(x * TileWidth, y * TileHeight, TileWidth, TileHeight), Opacity, LayerDepth);
                }
            }
        }
    }
}
