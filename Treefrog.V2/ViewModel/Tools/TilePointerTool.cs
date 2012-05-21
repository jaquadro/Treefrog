using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.V2.ViewModel.Commands;
using Treefrog.Framework.Model;
using Treefrog.Framework;

namespace Treefrog.V2.ViewModel.Tools
{
    public class TilePointerTool : PointerTool
    {
        private TilePoolManagerService _poolManager;

        private CommandHistory _history;
        private MultiTileGridLayer _layer;

        public TilePointerTool (CommandHistory history, MultiTileGridLayer layer)
        {
            _history = history;
            _layer = layer;
        }

        protected MultiTileGridLayer Layer
        {
            get { return _layer; }
        }

        protected CommandHistory History
        {
            get { return _history; }
        }

        protected Tile ActiveTile
        {
            get
            {
                if (_poolManager == null) {
                    _poolManager = ServiceContainer.Default.GetService<TilePoolManagerService>();
                    if (_poolManager == null)
                        return null;
                }

                TilePoolItemVM activeTile = _poolManager.ActiveTile;
                if (activeTile == null)
                    return null;
                return activeTile.Tile;
            }
        }

        protected TileCoord TileLocation (PointerEventInfo info)
        {
            return new TileCoord((int)info.X / Layer.TileWidth, (int)info.Y / Layer.TileHeight);
        }

        protected bool TileInRange (TileCoord location)
        {
            if (location.X < 0 || location.X >= Layer.TilesWide)
                return false;
            if (location.Y < 0 || location.Y >= Layer.TilesHigh)
                return false;

            return true;
        }
    }
}
